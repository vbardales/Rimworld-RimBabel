using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using RimBabel.Core;
using RimBabel.Game;
using RimWorld;
using Verse;

// Exercises the shipped RimBabel.dll's extractor against the real RimWorld assemblies, with no game
// running. See the csproj for what this does and does not prove.
internal static class Program
{
    private static int checks, failures;

    private static void Check(bool ok, string what)
    {
        checks++;
        if (!ok) { failures++; Console.WriteLine("FAIL  " + what); }
    }

    private static int Main(string[] args)
    {
        string managed = args.Length > 0 ? args[0] : Metadata("RimWorldManaged");

        // Assembly-CSharp pulls in Unity assemblies that are not beside this executable.
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            string path = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };

        Console.WriteLine("RimWorld:      " + managed);

        // Main must not name a type from those assemblies: the JIT resolves them while it compiles the
        // method, before the handler above is installed. Hence the separate method.
        try { Run(); }
        catch (Exception ex)
        {
            Console.WriteLine("FAIL  the run threw: " + ex);
            return 2;
        }
        Console.WriteLine(checks + " checks, " + failures + " failed");
        return failures == 0 ? 0 : 1;
    }

    private static string Metadata(string key)
    {
        return typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(a => a.Key == key).Value;
    }

    private static string Temp()
    {
        string d = Path.Combine(Path.GetTempPath(), "rimbabel-game-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(d);
        return d;
    }

    /// <summary>A ModContentPack with no game loading behind it: only what the scanner reads is set.</summary>
    private static ModContentPack NewMod(string packageId, string name, string root, params string[] folders)
    {
        var mod = (ModContentPack)FormatterServices.GetUninitializedObject(typeof(ModContentPack));
        Set(mod, "packageIdInt", packageId);
        Set(mod, "nameInt", name);
        Set(mod, "rootDirInt", new DirectoryInfo(root));
        mod.foldersToLoadDescendingOrder = folders.ToList();
        return mod;
    }

    private static T Make<T>(string defName, string label, string description, ModContentPack mod)
    {
        var def = (T)FormatterServices.GetUninitializedObject(typeof(T));
        var d = (Def)(object)def;
        d.defName = defName;
        d.label = label;
        d.description = description;
        d.modContentPack = mod;
        return def;
    }

    private static void Set(object target, string field, object value)
    {
        FieldInfo f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (f == null) throw new MissingFieldException(target.GetType().Name, field);
        f.SetValue(target, value);
    }

    /// <summary>
    /// GenTypes.AllTypes asks every running mod for its assemblies and logs through Unity when one type of the
    /// game assembly cannot load, which outside the engine is a hard failure. Fill its cache with the types of
    /// the game assembly that do load, which is all the Def walk needs.
    /// </summary>
    private static void PrimeTypeCache()
    {
        Assembly game = typeof(Def).Assembly;
        Type[] types;
        try { types = game.GetTypes(); }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t != null).ToArray();
            string[] why = ex.LoaderExceptions.Select(e => e.Message).Distinct().Take(3).ToArray();
            Console.WriteLine("note: " + (ex.Types.Length - types.Length) + " game types do not load here (" + string.Join(" | ", why) + ")");
        }
        FieldInfo cache = typeof(GenTypes).GetField("allTypesCached", BindingFlags.Static | BindingFlags.NonPublic);
        cache.SetValue(null, types.ToList());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run()
    {
        PrimeTypeCache();
        string root = Temp();
        string modRoot = Path.Combine(root, "SourceMod");
        string keyed = Path.Combine(modRoot, "Languages", "English", "Keyed");
        Directory.CreateDirectory(keyed);
        File.WriteAllText(Path.Combine(keyed, "Rb.xml"),
            "<LanguageData><RbHello>Hello {0}</RbHello><RbTwo>Line one\\nline two</RbTwo></LanguageData>");

        ModContentPack mine = NewMod("someone.sourcemod", "Source Mod", modRoot, modRoot);
        ModContentPack other = NewMod("someone.othermod", "Other Mod", Path.Combine(root, "Other"), Path.Combine(root, "Other"));

        // Defs built by hand and put in the game's own database, the way the loader would.
        // Built without their constructors: ThingDef's reaches Unity's shader loading, which needs the engine.
        ThingDef wall = Make<ThingDef>("RbWall", "rb wall", "A sturdy wall of rb stone.", mine);
        wall.graphicData = (GraphicData)FormatterServices.GetUninitializedObject(typeof(GraphicData));
        wall.graphicData.texPath = "Things/Rb Wall";
        DefDatabase<ThingDef>.Add(wall);
        DefDatabase<ThingDef>.Add(Make<ThingDef>("RbTool", "rbtool", "single", mine));
        DefDatabase<ThingDef>.Add(Make<ThingDef>("RbForeign", "foreign thing", "Another mod's text.", other));
        ThingDef madeUp = Make<ThingDef>("RbMadeUp", "made up thing", "Generated at load.", mine);
        madeUp.generated = true;
        DefDatabase<ThingDef>.Add(madeUp);
        ThoughtDef thought = Make<ThoughtDef>("RbThought", null, null, mine);
        thought.stages = new List<ThoughtStage> { new ThoughtStage { label = "rb feeling", description = "It feels rb." } };
        DefDatabase<ThoughtDef>.Add(thought);

        List<Entry> entries = SourceScanner.Scan(mine);
        Func<string, Entry> find = id => entries.FirstOrDefault(e => e.Id == id);

        // Keyed
        Entry hello = find("K:RbHello");
        Check(hello != null && hello.Source == "Hello {0}", "keyed text is listed, placeholder intact");
        Entry two = find("K:RbTwo");
        Check(two != null && two.Source == "Line one\nline two", "keyed line break is a real line break in the source");

        // Defs of the mod
        Entry label = find("D:ThingDef/RbWall.label");
        Check(label != null && label.Source == "rb wall", "def label is listed under its injection path");
        Check(find("D:ThingDef/RbWall.description") != null, "def description is listed");
        Check(find("D:ThingDef/RbTool.label") != null, "a one-word label is taken");
        Check(find("D:ThingDef/RbTool.description") != null, "a one-word description is taken: the game marks the field MustTranslate");

        // What must not be there
        Check(entries.All(e => !e.Key.Contains("texPath")), "a texture path is not a text");
        Check(entries.All(e => !e.Key.EndsWith(".defName")), "a defName is not a text");
        Check(entries.All(e => !e.Key.StartsWith("RbForeign")), "another mod's def is not listed");
        Check(entries.All(e => !e.Key.StartsWith("RbMadeUp")), "a generated def is not listed");

        // Nested lists of objects: the path names the element, and the field after it
        Check(entries.Any(e => e.DefType == "ThoughtDef" && e.Key.StartsWith("RbThought.stages.") && e.Key.EndsWith(".label")),
            "a text inside a list of objects is listed with a path through the list");

        // Every path is a valid XML element name, so the writer can use it
        foreach (Entry e in entries.Where(x => x.Kind == EntryKind.DefInjected))
        {
            bool ok;
            try { System.Xml.XmlConvert.VerifyName(e.Key); ok = true; } catch (System.Xml.XmlException) { ok = false; }
            Check(ok, "injection path is a valid XML name: " + e.Key);
        }
        Check(entries.Select(e => e.Id).Distinct().Count() == entries.Count, "no entry is listed twice");

        // From scan to package
        PackageSpec spec = PackageBuilder.SpecFor(mine, "French", "Tester");
        Check(spec.PackageId == "nelim.rimbabel.someone.sourcemod.french", "package id from source id and language: " + spec.PackageId);
        Check(spec.SourcePackageId == "someone.sourcemod" && spec.FileName == "SourceMod", "spec names its source and a safe file name");

        string pkg = Path.Combine(root, "pkg");
        PackageResult first = PackageWriter.Write(pkg, spec, entries, new DateTime(2026, 10, 3));
        Check(first.Merge.New.Count == entries.Count, "a first write sees every text as new");
        Manifest m = Manifest.Parse(File.ReadAllText(Path.Combine(pkg, "Mod", "Babel", "manifest.xml")));
        Check(m.Entries.Count == entries.Count && m.SourcePackageId == "someone.sourcemod", "the manifest lists every scanned text and names the source");
        Check(first.PublishBlockers.Any(b => b.Contains("Licence of the source mod")), "an unknown source licence blocks publication");

        // Second scan of the same mod: nothing moved, nothing changes
        PackageResult again = PackageWriter.Write(pkg, spec, SourceScanner.Scan(mine), new DateTime(2026, 10, 4));
        Check(!again.Merge.HasChanges && again.Version == first.Version, "scanning an unchanged mod again changes nothing");

        // The source mod changes its text: that one text moves, and a human translation would become stale
        File.WriteAllText(Path.Combine(keyed, "Rb.xml"), "<LanguageData><RbHello>Hello there {0}</RbHello><RbTwo>Line one\\nline two</RbTwo></LanguageData>");
        PackageResult changed = PackageWriter.Write(pkg, spec, SourceScanner.Scan(mine), new DateTime(2026, 10, 5));
        Check(changed.Merge.Changed.Count == 1 && changed.Merge.Changed[0].Key == "RbHello", "an edited source text is the only change");

        // The scanner's own rule, on real fields
        FieldInfo labelField = typeof(Def).GetField("label");
        FieldInfo descField = typeof(Def).GetField("description");
        FieldInfo plainField = typeof(Def).GetField("defName");
        Check(descField.HasAttribute<MustTranslateAttribute>(), "the game marks Def.description MustTranslate");
        Check(labelField.HasAttribute<MustTranslateAttribute>(), "the game marks Def.label MustTranslate");
        Check(SourceScanner.IsText("x", labelField), "IsText: label of one word");
        Check(SourceScanner.IsText("x", descField), "IsText: a MustTranslate field of one word");
        Check(!SourceScanner.IsText("x", plainField), "IsText: a plain field of one word");
        Check(SourceScanner.IsText("two words", plainField), "IsText: a plain field with a space");
        Check(!SourceScanner.IsText("", labelField) && !SourceScanner.IsText(null, labelField), "IsText: empty and null");

        Directory.Delete(root, true);
    }
}
