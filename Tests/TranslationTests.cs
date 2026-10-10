using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimBabel.Core;

// Protector, glossary, blacklist, pipeline and importer: the machinery between "a text" and "its translation", with
// fake engines. No network and no game.
internal static partial class Program
{
    private static readonly KeyValuePair<string, string>[] NoGlossary = new KeyValuePair<string, string>[0];

    private static string Round(string source, string engineOutput)
    {
        Protected p = Protector.Protect(source, NoGlossary);
        Restored r = Protector.Restore(engineOutput, p, source);
        return r.Ok ? r.Text : "FAIL:" + r.Reason;
    }

    private static void ProtectorTests()
    {
        string src = "Hello {0}, <color=#ff0000>danger</color> (*Colonist) {PAWN_gender ? he : she} runs";
        Protected p = Protector.Protect(src, NoGlossary);
        Check(p.Slots.Count == 5, "protector: braces, both tags, the marker and the switch are hidden (" + p.Slots.Count + ")");
        Check(!p.Text.Contains("{") && !p.Text.Contains("<") && !p.Text.Contains("(*"), "protector: nothing the engine could touch is left in the text");
        Check(p.Text.Contains("Hello ") && p.Text.Contains("danger") && p.Text.Contains(" runs"), "protector: the words stay for the engine");

        string good = p.Text.Replace("Hello", "Bonjour").Replace("danger", "danger!").Replace("runs", "court");
        Check(Round(src, good) == "Bonjour {0}, <color=#ff0000>danger!</color> (*Colonist) {PAWN_gender ? he : she} court", "protector: a faithful translation gets every original back");

        // The closing marker of a (*Name)...(/Name) pair is hidden too: DeepL, run live, translated "(/Good)" to "(/Bon)".
        Protected pair = Protector.Protect("(*Good)Warm hearth(/Good) here", NoGlossary);
        Check(pair.Slots.Count == 2 && !pair.Text.Contains("(/"), "protector: the closing (/Name) marker is hidden as well as the opening one");

        // And the live answer to that sentence put the closing token before the opening one: refused, not shipped.
        string tOpen = pair.Slots[0].Token, tClose = pair.Slots[1].Token;
        Check(!Protector.Restore("Un foyer" + tClose + " " + tOpen + "chaleureux ici", pair, "(*Good)Warm hearth(/Good) here").Ok, "protector: a closing marker before its opening one is refused");
        Check(Protector.Restore(tOpen + "Un foyer chaleureux" + tClose + " ici", pair, "(*Good)Warm hearth(/Good) here").Ok, "protector: the same pair in order is accepted");

        // Reordered tokens are legal: grammar differs between languages.
        Protected two = Protector.Protect("{0} gave {1} a gift", NoGlossary);
        Restored swapped = Protector.Restore("⟦1⟧ a reçu un cadeau de ⟦0⟧", two, "{0} gave {1} a gift");
        Check(swapped.Ok && swapped.Text == "{1} a reçu un cadeau de {0}", "protector: tokens may change places");

        Check(Round("Hello {0} and welcome there", "Bonjour et bienvenue ici").StartsWith("FAIL:"), "protector: a lost placeholder is refused");
        Check(Round("Hello {0} and welcome there", "Bonjour ⟦0⟧ ⟦0⟧ et bienvenue").StartsWith("FAIL:"), "protector: a duplicated placeholder is refused");
        Check(Round("Hello {0} and welcome there", "Bonjour ⟦0⟧ ⟦5⟧ et bienvenue").StartsWith("FAIL:"), "protector: an invented token is refused");
        Check(Round("Hello there my friend", "").StartsWith("FAIL:"), "protector: an empty answer is refused");
        Check(Round("Hello there my friend", null).StartsWith("FAIL:"), "protector: no answer is refused");
        Check(Round("A fairly long sentence to translate here", "Oui").StartsWith("FAIL:"), "protector: an answer a tenth the length is refused");
        Check(Round("A fairly long sentence to translate here", new string('x', 400)).StartsWith("FAIL:"), "protector: an answer ten times the length is refused");
        Check(Round("Hi", "Salut, mon ami, comment vas-tu aujourd'hui ?") != null && !Round("Hi", "Salut, mon ami, comment vas-tu aujourd'hui ?").StartsWith("FAIL:"), "protector: a short source is not held to the length rule");

        Check(Round("  Hello \n", "Bonjour") == "  Bonjour \n", "protector: the source's own leading and trailing whitespace comes back");
        Check(Round("Line one\nline two", "Ligne un\nligne deux") == "Ligne un\nligne deux", "protector: line breaks pass through");
        Protected open = Protector.Protect("An unbalanced {0 brace", NoGlossary);
        Check(open.Slots.Count == 0, "protector: an unbalanced brace is left alone rather than guessed at");
        Protected nested = Protector.Protect("{0_label ? {1} : none} left", NoGlossary);
        Check(nested.Slots.Count == 1 && nested.Slots[0].Original == "{0_label ? {1} : none}", "protector: a nested switch is one slot");
    }

    private static void GlossaryTests()
    {
        var g = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("colonist", "colon"),
            new KeyValuePair<string, string>("colony", "colonie"),
            new KeyValuePair<string, string>("colony ship", "vaisseau colonial"),
        };

        Protected p = Protector.Protect("The colonist left the colony ship.", g);
        Check(!p.Text.ToLowerInvariant().Contains("colonist") && !p.Text.ToLowerInvariant().Contains("colony"), "glossary: the terms are hidden from the engine");
        Check(p.Slots.Count == 2 && p.Slots.All(s => s.IsTerm), "glossary: the longest term wins (colony ship, not colony)");
        Restored r = Protector.Restore(p.Text.Replace("The ", "Le ").Replace(" left the ", " a quitté le "), p, "The colonist left the colony ship.");
        Check(r.Ok && r.Text == "Le colon a quitté le vaisseau colonial.", "glossary: the required translation is what comes back");

        Protected cap = Protector.Protect("Colonist down", g);
        Restored rc = Protector.Restore("⟦0⟧ a terre", cap, "Colonist down");
        Check(rc.Ok && rc.Text == "Colon a terre", "glossary: a term that opens a sentence keeps its capital");

        Protected word = Protector.Protect("Colonists gather", g);
        Check(word.Slots.Count == 0, "glossary: a term inside a longer word is not replaced");

        Protected mix = Protector.Protect("{0} is a colonist", g);
        Check(mix.Slots.Count == 2 && !mix.Slots[0].IsTerm && mix.Slots[1].IsTerm, "glossary: placeholders and terms are told apart");
        Check(Protector.Restore("⟦0⟧ est un", mix, "{0} is a colonist").Ok == false, "glossary: an engine that drops a term is refused");
    }

    private static void BlacklistTests()
    {
        Entry name = Entry.Injected("PawnKindDef", "Colonist.label", "colonist");
        Entry text = Entry.Keyed("Greeting", "Hello");
        Check(Blacklist.Matches(new[] { "K:Greeting" }, text), "blacklist: an exact id");
        Check(Blacklist.Matches(new[] { "K:Greet*" }, text), "blacklist: a glob");
        Check(Blacklist.Matches(new[] { "D:PawnKindDef/*" }, name), "blacklist: a def type by glob");
        Check(!Blacklist.Matches(new[] { "D:PawnKindDef/*" }, text), "blacklist: a glob does not leak to other ids");
        Check(Blacklist.Matches(new[] { "re:^D:PawnKindDef/.*\\.label$" }, name), "blacklist: a regular expression");
        Check(!Blacklist.Matches(new[] { "re:^D:ThingDef/" }, name), "blacklist: a regular expression that does not match");
        Check(!Blacklist.Matches(new[] { "re:([" }, name), "blacklist: an invalid expression matches nothing");
        Check(Blacklist.Invalid(new[] { "re:([", "re:ok", "K:x" }).SequenceEqual(new[] { "re:([" }), "blacklist: the invalid rules are named");
        Check(!Blacklist.Matches(new[] { "", "  " }, text), "blacklist: empty rules match nothing");
        Check(!Blacklist.Matches(new[] { "K:Greeting.x" }, text), "blacklist: a glob is anchored (no partial match)");
    }

    private sealed class FakeEngine : ITranslationEngine
    {
        public string Name { get { return "fake"; } }
        public Func<string[], string[]> Behaviour;
        public int Calls;
        public List<string[]> Seen = new List<string[]>();
        public IList<KeyValuePair<string, string>> LastGlossary;
        public string[] Translate(string[] texts, string from, string to, IList<KeyValuePair<string, string>> glossary)
        {
            Calls++;
            Seen.Add(texts);
            LastGlossary = glossary;
            return Behaviour(texts);
        }
    }

    private static Manifest ManifestOf(params Entry[] entries)
    {
        var m = new Manifest { Language = "French" };
        m.Entries.AddRange(entries);
        return m;
    }

    private static void PipelineTests()
    {
        Func<string[], string[]> upper = t => t.Select(x => x.ToUpperInvariant().Replace("⟦", "⟦")).ToArray();

        Entry hello = Entry.Keyed("Hello", "Hello {0}, welcome home");
        Entry human = Entry.Keyed("Human", "Human text here"); human.Target = "Texte"; human.Status = EntryStatus.Human;
        Entry locked = Entry.Keyed("Locked", "Locked text here"); locked.Target = "Verrouillé"; locked.Status = EntryStatus.Locked;
        Entry name = Entry.Keyed("Name_Bob", "Bob the builder");
        Entry stale = Entry.Keyed("Stale", "Stale text here"); stale.Target = "Vieux"; stale.Status = EntryStatus.Stale;
        Manifest m = ManifestOf(hello, human, locked, name, stale);
        m.Blacklist.Add("K:Name_*");
        m.Glossary.Add(new KeyValuePair<string, string>("home", "maison"));

        var engine = new FakeEngine { Behaviour = upper };
        PipelineReport rep = Pipeline.Run(m, engine, "English", "French", 10);
        Check(rep.Translated == 2 && rep.Blacklisted == 1 && rep.Failed == 0 && !rep.Aborted, "pipeline: pending and stale translated, the blacklisted one counted");
        Check(hello.Status == EntryStatus.Machine && hello.Engine == "fake", "pipeline: a machine text carries its engine");
        Check(hello.Target == "HELLO {0}, WELCOME maison", "pipeline: placeholders and glossary terms come back as they should (" + hello.Target + ")");
        Check(stale.Target == "STALE TEXT HERE" && stale.Status == EntryStatus.Machine, "pipeline: a stale text is translated again");
        Check(human.Target == "Texte" && human.Status == EntryStatus.Human, "pipeline: a human text is never touched");
        Check(locked.Target == "Verrouillé" && locked.Status == EntryStatus.Locked, "pipeline: a locked text is never touched");
        Check(name.Status == EntryStatus.Pending && name.Target == null, "pipeline: a blacklisted text is never sent");
        Check(engine.Seen.SelectMany(x => x).All(s => !s.Contains("{0}") && !s.ToLowerInvariant().Contains("home")), "pipeline: the engine never sees a placeholder or a glossary term");
        Check(engine.LastGlossary != null && engine.LastGlossary.Count == 1, "pipeline: the glossary is handed to the engine for those that can use it");

        // An engine that drops a placeholder: the text stays as it was.
        Entry a = Entry.Keyed("A", "Hello {0}, welcome home");
        var dropper = new FakeEngine { Behaviour = t => t.Select(x => "Bonjour et bienvenue").ToArray() };
        PipelineReport bad = Pipeline.Run(ManifestOf(a), dropper, "English", "French", 10);
        Check(bad.Failed == 1 && bad.Translated == 0 && a.Status == EntryStatus.Pending && a.Target == null, "pipeline: a broken translation is left as the source");
        Check(bad.Failures.Count == 1 && bad.Failures[0].StartsWith("K:A:"), "pipeline: the failure names the text");
        Check(dropper.Calls == 2, "pipeline: a failing text gets one more try alone, then stops");

        // The retry rescues a text that only failed inside a batch.
        Entry b1 = Entry.Keyed("B1", "First {0} text here"), b2 = Entry.Keyed("B2", "Second {0} text here");
        var flaky = new FakeEngine { Behaviour = t => t.Length > 1 ? t.Select(x => "oups").ToArray() : upper(t) };
        PipelineReport rescued = Pipeline.Run(ManifestOf(b1, b2), flaky, "English", "French", 10);
        Check(rescued.Translated == 2 && rescued.Failed == 0 && b1.Status == EntryStatus.Machine, "pipeline: a retry alone rescues what failed in the batch");

        // An engine that hands the source back is refused for a text of several words, let through for one word.
        Entry u1 = Entry.Keyed("U1", "Still in English here"), u2 = Entry.Keyed("U2", "Colonist");
        var same = new FakeEngine { Behaviour = t => t };
        PipelineReport un = Pipeline.Run(ManifestOf(u1, u2), same, "English", "French", 10);
        Check(un.Failed == 1 && un.Translated == 1 && u1.Status == EntryStatus.Pending && u1.Target == null && u2.Status == EntryStatus.Machine, "pipeline: an answer equal to a multi-word source is refused, a one-word one is kept");
        Check(un.Failures.Count == 1 && un.Failures[0].Contains("unchanged"), "pipeline: the refusal says the text came back unchanged");

        // A source made only of protected spans comes back untouched by design: it is not an unchanged answer.
        Entry p1 = Entry.Keyed("P1", "{PAWN_nameDef} {PAWN_pronoun}");
        PipelineReport ph = Pipeline.Run(ManifestOf(p1), new FakeEngine { Behaviour = t => t }, "English", "French", 10);
        Check(ph.Translated == 1 && ph.Failed == 0 && p1.Status == EntryStatus.Machine, "pipeline: a source of placeholders only is not refused as unchanged");

        // A shifted answer is dropped whole.
        Entry c1 = Entry.Keyed("C1", "First text here"), c2 = Entry.Keyed("C2", "Second text here");
        var shifted = new FakeEngine { Behaviour = t => new[] { "un" } };
        PipelineReport sh = Pipeline.Run(ManifestOf(c1, c2), shifted, "English", "French", 10);
        Check(sh.Translated == 0 && sh.Failed == 2 && c1.Target == null && c2.Target == null, "pipeline: an answer of the wrong length is dropped whole");

        // An engine that throws stops the run and keeps what was done.
        Entry d1 = Entry.Keyed("D1", "First text here"), d2 = Entry.Keyed("D2", "Second text here"), d3 = Entry.Keyed("D3", "Third text here");
        int n = 0;
        var thrower = new FakeEngine { Behaviour = t => { if (++n == 2) throw new InvalidOperationException("quota exhausted"); return upper(t); } };
        PipelineReport th = Pipeline.Run(ManifestOf(d1, d2, d3), thrower, "English", "French", 1);
        Check(th.Aborted && th.Translated == 1 && d1.Status == EntryStatus.Machine, "pipeline: what was translated before a failure is kept");
        Check(d2.Status == EntryStatus.Pending && d3.Status == EntryStatus.Pending, "pipeline: nothing after the failure is attempted");
        Check(th.Failures.Any(f => f.Contains("quota exhausted")), "pipeline: the engine's reason is reported");
        Check(thrower.Calls == 2, "pipeline: the run stops at the first throw (" + thrower.Calls + " calls)");

        // Batches
        var many = Enumerable.Range(0, 25).Select(i => Entry.Keyed("M" + i, "Text number " + i + " is here")).ToArray();
        var counter = new FakeEngine { Behaviour = upper };
        Pipeline.Run(ManifestOf(many), counter, "English", "French", 10);
        Check(counter.Calls == 3 && counter.Seen[0].Length == 10 && counter.Seen[2].Length == 5, "pipeline: texts go in batches of the asked size");
    }

    private static void ImporterTests()
    {
        string root = Temp();
        string lang = Path.Combine(root, "French");
        Directory.CreateDirectory(Path.Combine(lang, "Keyed"));
        Directory.CreateDirectory(Path.Combine(lang, "DefInjected", "ThingDef"));
        File.WriteAllText(Path.Combine(lang, "Keyed", "X.xml"),
            "<LanguageData><!-- EN: Hello {0} --><A>Bonjour {0}</A><B>deux\\nlignes</B><C>TODO</C><Gone>x</Gone><Taken>pris</Taken></LanguageData>");
        File.WriteAllText(Path.Combine(lang, "DefInjected", "ThingDef", "X.xml"),
            "<LanguageData><!-- EN: old wall --><RbWall.label>mur</RbWall.label><RbWall.description>décrit</RbWall.description><Orphan.label>x</Orphan.label></LanguageData>");

        Entry a = Entry.Keyed("A", "Hello {0}");
        Entry b = Entry.Keyed("B", "two\nlines");
        Entry c = Entry.Keyed("C", "to do");
        Entry taken = Entry.Keyed("Taken", "taken"); taken.Target = "mine"; taken.Status = EntryStatus.Human;
        Entry label = Entry.Injected("ThingDef", "RbWall.label", "new wall");
        Entry desc = Entry.Injected("ThingDef", "RbWall.description", "a wall");

        ImportReport rep = Importer.Import(new[] { a, b, c, taken, label, desc }, lang);
        Check(a.Target == "Bonjour {0}" && a.Status == EntryStatus.Human, "import: a text whose EN comment matches the source is Human");
        Check(b.Target == "deux\nlignes" && b.Status == EntryStatus.Human, "import: a backslash-n is a line break");
        Check(c.Target == null && c.Status == EntryStatus.Pending, "import: the game's TODO placeholder is not a translation");
        Check(taken.Target == "mine" && rep.Kept == 1, "import: an existing human text is kept");
        Check(label.Target == "mur" && label.Status == EntryStatus.Stale && rep.Outdated == 1, "import: a text written for another source arrives as a stale draft");
        Check(desc.Target == "décrit" && desc.Status == EntryStatus.Human, "import: a text with no EN comment is trusted as human");
        Check(rep.Imported == 3, "import: three texts imported (" + rep.Imported + ")");
        Check(rep.Orphans.Contains("K:Gone") && rep.Orphans.Contains("D:ThingDef/Orphan.label"), "import: texts for keys the source no longer has are reported");
        Check(Importer.Import(new[] { a }, Path.Combine(root, "Nothing")).Imported == 0, "import: a folder that does not exist imports nothing");
        Directory.Delete(root, true);
    }
}
