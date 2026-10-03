using System;
using System.IO;
using System.Linq;
using RimBabel.Core;

internal static partial class Program
{
    private static int checks, failures;

    private static void Check(bool ok, string what)
    {
        checks++;
        if (!ok) { failures++; Console.WriteLine("FAIL  " + what); }
    }

    private static int Main()
    {
        Hash();
        ManifestRoundTrip();
        MergeRules();
        WriterLayout();
        WriterUpdate();
        WriterRefusals();
        KeyedReading();
        ProtectorTests();
        GlossaryTests();
        BlacklistTests();
        PipelineTests();
        ImporterTests();
        Console.WriteLine(checks + " checks, " + failures + " failed");
        return failures == 0 ? 0 : 1;
    }

    private static PackageSpec Spec()
    {
        return new PackageSpec
        {
            PackageId = "nelim.rimbabel.demo.fr", Name = "Demo (French)", Author = "Nelim",
            Language = "French", FileName = "Demo", SourcePackageId = "someone.demo",
            SourceName = "Demo", SourceVersion = "1.0", SourceLicenceName = "MIT", LicenceText = "MIT License\n",
        };
    }

    private static string Temp()
    {
        string d = Path.Combine(Path.GetTempPath(), "rimbabel-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(d);
        return d;
    }

    private static void Hash()
    {
        Check(Hashing.Of("a\r\nb") == Hashing.Of("a\nb"), "hash ignores newline convention");
        Check(Hashing.Of("a") != Hashing.Of("b"), "hash differs on text");
    }

    private static void ManifestRoundTrip()
    {
        var m = new Manifest { Language = "French", SourcePackageId = "x.y", SourceName = "X & <Y>", SourceVersion = "2" };
        m.Glossary.Add(new System.Collections.Generic.KeyValuePair<string, string>("colonist", "colon"));
        m.Blacklist.Add("K:Name.*");
        Entry e = Entry.Keyed("Greeting", "Hello \"world\" & <b>you</b>\nline two {0}");
        e.Target = "Bonjour & <b>toi</b>\nligne deux {0}"; e.Status = EntryStatus.Human; e.Engine = "deepl";
        Entry d = Entry.Injected("ThingDef", "Wall.label", "stone wall");
        m.Entries.Add(e); m.Entries.Add(d);

        Manifest back = Manifest.Parse(m.ToXml());
        Check(back.SourceName == "X & <Y>", "manifest keeps special chars in attributes");
        Check(back.Entries.Count == 2, "manifest keeps entries");
        Check(back.Entries[0].Source == e.Source && back.Entries[0].Target == e.Target, "manifest keeps text and newlines");
        Check(back.Entries[0].Status == EntryStatus.Human && back.Entries[0].Engine == "deepl", "manifest keeps status and engine");
        Check(back.Entries[1].Target == null && back.Entries[1].DefType == "ThingDef", "pending entry has no target");
        Check(back.Glossary.Count == 1 && back.Glossary[0].Value == "colon", "manifest keeps glossary");
        Check(back.Blacklist.SequenceEqual(new[] { "K:Name.*" }), "manifest keeps blacklist");

        bool refused = false;
        try { Manifest.Parse(m.ToXml().Replace("schema=\"1\"", "schema=\"99\"")); } catch (InvalidDataException) { refused = true; }
        Check(refused, "newer schema is refused");
    }

    private static void MergeRules()
    {
        var human = Entry.Keyed("A", "one"); human.Target = "un"; human.Status = EntryStatus.Human;
        var machine = Entry.Keyed("B", "two"); machine.Target = "deux"; machine.Status = EntryStatus.Machine;
        var locked = Entry.Keyed("C", "three"); locked.Target = "trois"; locked.Status = EntryStatus.Locked;
        var gone = Entry.Keyed("D", "four"); gone.Target = "quatre"; gone.Status = EntryStatus.Human;
        var previous = new[] { human, machine, locked, gone };

        var current = new[] { Entry.Keyed("A", "one"), Entry.Keyed("B", "two!"), Entry.Keyed("C", "three!"), Entry.Keyed("E", "five") };
        MergeResult r = Merge.Apply(previous, current);

        Entry a = r.Entries.Single(x => x.Key == "A");
        Check(a.Status == EntryStatus.Human && a.Target == "un", "unchanged human text is kept");
        Entry b = r.Entries.Single(x => x.Key == "B");
        Check(b.Status == EntryStatus.Stale && b.Target == "deux" && b.Source == "two!", "changed source makes a stale draft");
        Entry c = r.Entries.Single(x => x.Key == "C");
        Check(c.Status == EntryStatus.Locked && c.Target == "trois" && c.Source == "three!", "locked text survives a source change");
        Check(r.New.Count == 1 && r.New[0].Status == EntryStatus.Pending, "new key is pending");
        Check(r.Removed.Count == 1 && r.Removed[0].Key == "D", "gone key is reported");
        Check(r.Entries.All(x => x.Key != "D"), "gone key is dropped");
        Check(Merge.NextVersion("0.3.0", r) == "0.4.0", "any change bumps minor");
        Check(Merge.NextVersion("0.3.0", Merge.Apply(previous.Take(1), current.Take(1))) == "0.3.0", "no change keeps the version");
    }

    private static void WriterLayout()
    {
        string root = Temp();
        var k = Entry.Keyed("Greeting", "Hello\nworld"); k.Target = "Bonjour\nmonde & <b>"; k.Status = EntryStatus.Machine;
        var d = Entry.Injected("ThingDef", "Wall.label", "stone wall"); d.Target = "mur de pierre"; d.Status = EntryStatus.Machine;
        var p = Entry.Injected("ThingDef", "Wall.description", "a wall");

        // Entries come from a source scan: no target yet. Give targets through the manifest path.
        PackageResult res = PackageWriter.Write(root, Spec(), new[] { k, d, p }, new DateTime(2026, 10, 3));
        Check(res.Version == "0.1.0", "first version is 0.1.0");
        Check(File.Exists(Path.Combine(root, "Mod", "Babel", "manifest.xml")), "manifest written");
        Check(File.Exists(Path.Combine(root, "Mod", "About", "About.xml")), "About.xml written");
        Check(File.Exists(Path.Combine(root, "CHANGELOG.md")) && File.Exists(Path.Combine(root, "STATUS.md")) && File.Exists(Path.Combine(root, "LICENSE")), "repo files written");

        // Scan entries are pending, so nothing is shipped yet: Languages holds no text.
        Check(!Directory.Exists(Path.Combine(root, "Mod", "Languages", "French", "Keyed")), "pending entries ship no file");

        // Fill targets via the manifest, then save.
        Manifest m = Manifest.Parse(File.ReadAllText(Path.Combine(root, "Mod", "Babel", "manifest.xml")));
        m.Entries.Single(e => e.Key == "Greeting").Target = k.Target;
        m.Entries.Single(e => e.Key == "Wall.label").Target = d.Target;
        PackageWriter.Save(root, Spec(), m);
        string keyed = File.ReadAllText(Path.Combine(root, "Mod", "Languages", "French", "Keyed", "Demo.xml"));
        Check(keyed.Contains("<Greeting>Bonjour\\nmonde &amp; &lt;b&gt;</Greeting>"), "keyed file escapes and uses backslash-n");
        Check(File.Exists(Path.Combine(root, "Mod", "Languages", "French", "DefInjected", "ThingDef", "Demo.xml")), "def injected file per def type");
        Check(!File.ReadAllText(Path.Combine(root, "Mod", "Languages", "French", "DefInjected", "ThingDef", "Demo.xml")).Contains("description"), "pending key is not written");
        Check(res.PublishBlockers.Any(b => b.Contains("Preview.png")), "missing preview blocks publication");
        Directory.Delete(root, true);
    }

    private static void WriterUpdate()
    {
        string root = Temp();
        PackageWriter.Write(root, Spec(), new[] { Entry.Keyed("A", "one"), Entry.Keyed("B", "two") }, new DateTime(2026, 10, 3));
        Manifest m = Manifest.Parse(File.ReadAllText(Path.Combine(root, "Mod", "Babel", "manifest.xml")));
        m.Entries[0].Target = "un"; m.Entries[0].Status = EntryStatus.Human;
        PackageWriter.Save(root, Spec(), m);
        File.WriteAllText(Path.Combine(root, "Mod", "About", "PublishedFileId.txt"), "12345");

        PackageResult r = PackageWriter.Write(root, Spec(), new[] { Entry.Keyed("A", "one"), Entry.Keyed("C", "three") }, new DateTime(2026, 10, 4));
        Check(r.Version == "0.2.0", "update bumps minor");
        Check(r.Merge.New.Count == 1 && r.Merge.Removed.Count == 1, "update reports new and removed");
        Check(File.ReadAllText(Path.Combine(root, "Mod", "About", "PublishedFileId.txt")) == "12345", "PublishedFileId kept");
        Check(File.ReadAllText(Path.Combine(root, "Mod", "Languages", "French", "Keyed", "Demo.xml")).Contains("<A>un</A>"), "human text kept across update");
        string log = File.ReadAllText(Path.Combine(root, "CHANGELOG.md"));
        Check(log.IndexOf("0.2.0") < log.IndexOf("0.1.0"), "changelog lists newest first");

        PackageResult same = PackageWriter.Write(root, Spec(), new[] { Entry.Keyed("A", "one"), Entry.Keyed("C", "three") }, new DateTime(2026, 10, 5));
        Check(same.Version == "0.2.0" && !File.ReadAllText(Path.Combine(root, "CHANGELOG.md")).Contains("2026-10-05"), "no change, no new version, no changelog line");
        Directory.Delete(root, true);
    }

    private static void KeyedReading()
    {
        var kv = KeyedXml.Parse("<LanguageData><!-- c --><A>one\\ntwo</A><B>x &amp; y</B><C><d>nested</d></C></LanguageData>");
        Check(kv.Count == 2, "keyed: comment and nested element are skipped");
        Check(kv[0].Value == "one\ntwo", "keyed: backslash-n becomes a line break");
        Check(kv[1].Value == "x & y", "keyed: entities are decoded");
        Check(KeyedXml.Parse("<Other><A>x</A></Other>").Count == 0, "keyed: a file that is not LanguageData is empty");

        string hi = Temp(), lo = Temp();
        foreach (string[] f in new[] { new[] { hi, "<LanguageData><A>high</A><B>b</B></LanguageData>" }, new[] { lo, "<LanguageData><A>low</A><C>c</C><E></E></LanguageData>" } })
        {
            string d = Path.Combine(f[0], "Languages", "English", "Keyed");
            Directory.CreateDirectory(d);
            File.WriteAllText(Path.Combine(d, "X.xml"), f[1]);
        }
        var entries = KeyedXml.ReadFolders(new[] { hi, lo, Path.Combine(hi, "missing") }, "English");
        Check(entries.Count == 3 && entries.Single(e => e.Key == "A").Source == "high", "keyed: the first load folder wins a key");
        Check(entries.All(e => e.Key != "E"), "keyed: an empty value is not a text");
        Check(KeyedXml.ReadFolders(new[] { hi }, "French").Count == 0, "keyed: another language is not read");
        Directory.Delete(hi, true); Directory.Delete(lo, true);
    }

    private static void WriterRefusals()
    {
        string root = Temp();
        string lang = Path.Combine(root, "Mod", "Languages", "French", "Keyed");
        Directory.CreateDirectory(lang);
        File.WriteAllText(Path.Combine(lang, "Hand.xml"), "<LanguageData/>");
        bool refused = false;
        try { PackageWriter.Write(root, Spec(), new[] { Entry.Keyed("A", "one") }, DateTime.Today); } catch (InvalidOperationException) { refused = true; }
        Check(refused && File.Exists(Path.Combine(lang, "Hand.xml")), "hand-made language folder is never overwritten");

        var bad = Spec(); bad.Language = "..\\x";
        refused = false;
        try { PackageWriter.Write(Temp(), bad, new[] { Entry.Keyed("A", "one") }, DateTime.Today); } catch (ArgumentException) { refused = true; }
        Check(refused, "path traversal in language is refused");

        var bk = Entry.Keyed("1bad key", "x"); bk.Target = "y";
        string r2 = Temp();
        refused = false;
        try
        {
            PackageWriter.Write(r2, Spec(), new[] { bk }, DateTime.Today);
            Manifest m = Manifest.Parse(File.ReadAllText(Path.Combine(r2, "Mod", "Babel", "manifest.xml")));
            m.Entries[0].Target = "y";
            PackageWriter.Save(r2, Spec(), m);
        }
        catch (InvalidDataException) { refused = true; }
        Check(refused, "a key that is not an XML name is refused");
        Directory.Delete(root, true);
    }
}
