using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RimBabel.Core;

// The settings page, outside the game: what its data does (engine config, the two line lists), and that every text it shows
// exists in English and in French. The layout itself needs a game and is in the Pickle suite.
internal static partial class Program
{
    private static string RepoRoot()
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "Mod", "About", "About.xml"))) dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new InvalidOperationException("Cannot find the repository root from " + AppDomain.CurrentDomain.BaseDirectory);
        return dir;
    }

    private static void ConfigTests()
    {
        var c = new EngineConfig();
        Check(EngineFactory.Problems(c).SequenceEqual(new[] { EngineProblem.NoKey }), "config: DeepL without a key says it needs one");
        c.DeepLKey = " abc:fx ";
        Check(EngineFactory.Problems(c).Count == 0 && EngineFactory.Create(c, new FakeHttp()) is DeepLEngine, "config: DeepL with a key makes a DeepL engine");

        c.Kind = EngineKind.Anthropic;
        Check(EngineFactory.Problems(c).SequenceEqual(new[] { EngineProblem.NoKey }), "config: Anthropic needs a key, and the default model is already set");
        c.AnthropicKey = "sk";
        Check(EngineFactory.Problems(c).Count == 0 && EngineFactory.Create(c, new FakeHttp()).Name.StartsWith("anthropic:"), "config: Anthropic with a key makes an Anthropic engine");
        c.AnthropicModel = "  ";
        Check(EngineFactory.Problems(c).SequenceEqual(new[] { EngineProblem.NoModel }), "config: a blank model is a problem");

        c.Kind = EngineKind.OpenAiCompatible;
        Check(EngineFactory.Problems(c).SequenceEqual(new[] { EngineProblem.NoModel }), "config: an OpenAI-compatible server needs a model but no key");
        c.OpenAiModel = "qwen2.5";
        Check(EngineFactory.Problems(c).Count == 0 && EngineFactory.Create(c, new FakeHttp()).Name.StartsWith("openai-compatible:"), "config: a local server with no key is complete");
        foreach (string url in new[] { "", "localhost:11434", "ftp://x/y", "not a url" })
        {
            c.OpenAiUrl = url;
            Check(EngineFactory.Problems(c).Contains(EngineProblem.BadUrl), "config: '" + url + "' is not a usable server address");
        }
        c.OpenAiUrl = "https://openrouter.ai/api/v1";
        Check(EngineFactory.Problems(c).Count == 0, "config: an https address is accepted");
        bool refused = false;
        try { EngineFactory.Create(new EngineConfig(), new FakeHttp()); } catch (InvalidOperationException) { refused = true; }
        Check(refused, "config: an incomplete configuration is refused instead of making a broken engine");

        int ignored;
        var d = LineLists.ParseDictionary("# comment\ncolonist = colon\n\n  home  =  maison \nbroken\n= nothing\nnothing =\nsum = a = b\r\nlast = fin", out ignored);
        Check(d.Count == 4 && ignored == 3, "lists: dictionary lines are read, comments skipped, three bad lines counted (" + d.Count + "/" + ignored + ")");
        Check(d[0].Key == "colonist" && d[0].Value == "colon" && d[1].Key == "home" && d[1].Value == "maison", "lists: both sides are trimmed");
        Check(d[2].Key == "sum" && d[2].Value == "a = b", "lists: only the first equals sign splits");
        Check(LineLists.ParseDictionary(null, out ignored).Count == 0 && ignored == 0, "lists: no text, no entries");
        Check(LineLists.ParseRules("# c\nK:Name_*\n\n re:^D:Pawn \r\n").SequenceEqual(new[] { "K:Name_*", "re:^D:Pawn" }), "lists: rules are one per line, trimmed, comments and blanks left out");
    }

    private static void TranslationCoverageTests()
    {
        string root = RepoRoot();
        Func<string, Dictionary<string, string>> keyed = lang =>
        {
            var map = new Dictionary<string, string>();
            string file = Path.Combine(root, "Mod", "Languages", lang, "Keyed", "RimBabel.xml");
            Check(File.Exists(file), "l10n: " + lang + " Keyed file exists");
            if (File.Exists(file)) foreach (var kv in KeyedXml.Parse(File.ReadAllText(file))) map[kv.Key] = kv.Value;
            return map;
        };
        Dictionary<string, string> en = keyed("English"), fr = keyed("French");

        // A UTF-8 text read through another code page shows up as these pairs ("clé" as "clÃ©", the apostrophe as "â€™"): seen once in the French file.
        foreach (string file in Directory.GetFiles(Path.Combine(root, "Mod", "Languages"), "*.xml", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(file);
            Check(!text.Contains("Ã©") && !text.Contains("Ã¨") && !text.Contains("â€") && !text.Contains("Ãƒ") && !text.Contains("�"),
                "l10n: " + Path.GetFileName(Path.GetDirectoryName(file)) + "/" + Path.GetFileName(file) + " holds no garbled characters");
        }

        Check(en.Keys.OrderBy(k => k, StringComparer.Ordinal).SequenceEqual(fr.Keys.OrderBy(k => k, StringComparer.Ordinal)), "l10n: English and French define exactly the same keys");
        Check(en.Values.All(v => v.Trim().Length > 0) && fr.Values.All(v => v.Trim().Length > 0), "l10n: no empty text in either language");
        foreach (string k in en.Keys.Where(fr.ContainsKey))
        {
            Func<string, string> holes = s => string.Join(",", Regex.Matches(s, @"\{\d+\}").Cast<Match>().Select(m => m.Value).Distinct().OrderBy(x => x).ToArray());
            Check(holes(en[k]) == holes(fr[k]), "l10n: " + k + " uses the same {n} parameters in both languages");
            Check(!fr[k].Contains("(e)") && !fr[k].Contains("(s)"), "l10n: " + k + " has no (e) form in French");
        }

        // Every key the code asks for exists, and every key defined is asked for.
        var literals = new HashSet<string>();
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "Source"), "*.cs", SearchOption.AllDirectories))
            foreach (Match m in Regex.Matches(File.ReadAllText(file), "\"(RimBabel\\.[A-Za-z0-9_.]+)\"")) literals.Add(m.Groups[1].Value);

        var used = new HashSet<string>();
        foreach (string lit in literals)
        {
            if (en.ContainsKey(lit)) used.Add(lit);
            else if (en.ContainsKey(lit + ".One") && en.ContainsKey(lit + ".Many")) { used.Add(lit + ".One"); used.Add(lit + ".Many"); }
            else if (lit.EndsWith("."))
            {
                // A key built from a prefix and an enum name: every name of the enum must have its key.
                Type e = typeof(EngineProblem);
                foreach (string name in Enum.GetNames(e))
                {
                    Check(en.ContainsKey(lit + name), "l10n: the key built as " + lit + name + " exists");
                    used.Add(lit + name);
                }
            }
            else Check(false, "l10n: the code asks for " + lit + ", which no language defines");
        }
        foreach (string k in en.Keys) Check(used.Contains(k), "l10n: " + k + " is defined and nothing asks for it");

        // The shortcut: declared, hidden by default, and its texts injected in French.
        XDocument defs = XDocument.Load(Path.Combine(root, "Mod", "Defs", "MainButtonDefs", "RimBabel.xml"));
        XElement button = defs.Root.Elements("MainButtonDef").Single();
        Check((string)button.Element("buttonVisible") == "false", "shortcut: hidden by default, not merely greyed");
        Check((string)button.Element("validWithoutMap") == "true", "shortcut: usable without a map, so it works from the main menu");
        Check((string)button.Element("workerClass") == "RimBabel.Game.MainButtonWorker_Settings", "shortcut: its worker is the one that opens the mod's settings");
        string defName = (string)button.Element("defName");
        XDocument inj = XDocument.Load(Path.Combine(root, "Mod", "Languages", "French", "DefInjected", "MainButtonDef", "RimBabel.xml"));
        var injKeys = inj.Root.Elements().Select(x => x.Name.LocalName).ToList();
        Check(injKeys.Contains(defName + ".label") && injKeys.Contains(defName + ".description"), "shortcut: French injects its label and description");
        Check(!string.IsNullOrEmpty((string)button.Element("label")) && !string.IsNullOrEmpty((string)button.Element("description")), "shortcut: English comes from the Def itself, the game's own fallback");
    }
}
