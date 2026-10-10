using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimBabel.Core;

// One translation mod for a whole language: a folder per source mod, loaded only when that mod is active.
internal static partial class Program
{
    private static CombinedSource SourceOf(string packageId, string name, string licence, params Entry[] entries)
    {
        var m = new Manifest { Language = "French", SourcePackageId = packageId, SourceName = name };
        m.Entries.AddRange(entries);
        return new CombinedSource
        {
            Manifest = m,
            Spec = new PackageSpec { PackageId = "x." + packageId, Name = name, Language = "French", FileName = name.Replace(" ", ""), SourcePackageId = packageId, SourceName = name, SourceLicenceName = licence },
        };
    }

    private static Entry Done(Entry e, string target) { e.Target = target; e.Status = EntryStatus.Human; return e; }

    private static void CombinedTests()
    {
        string root = Temp();
        var spec = new PackageSpec { PackageId = "nelim.rimbabel.all.french", Name = "RimBabel French", Author = "me", Language = "French", LicenceText = "MIT" };
        CombinedSource a = SourceOf("a.mod", "Mod A", "MIT", Done(Entry.Keyed("A_Hello", "Hello"), "Bonjour"), Done(Entry.Injected("ThingDef", "WallA.label", "wall"), "mur"));
        CombinedSource b = SourceOf("b.mod", "Mod B", null, Done(Entry.Keyed("B_Bye", "Bye"), "Au revoir"), Entry.Keyed("B_Pending", "Later"));
        b.Manifest.ImportedFrom.Add("Pack (p.k)");

        PackageResult r = CombinedPackage.Write(root, spec, new[] { b, a });
        string mod = Path.Combine(root, "Mod");
        Check(File.Exists(Path.Combine(mod, "a.mod", "Languages", "French", "Keyed", "ModA.xml")) && File.Exists(Path.Combine(mod, "b.mod", "Languages", "French", "Keyed", "ModB.xml")), "combined: each source mod has its own Languages folder");
        Check(File.Exists(Path.Combine(mod, "a.mod", "Languages", "French", "DefInjected", "ThingDef", "ModA.xml")), "combined: the def texts of a source go under its own folder");
        Check(!File.ReadAllText(Path.Combine(mod, "b.mod", "Languages", "French", "Keyed", "ModB.xml")).Contains("B_Pending"), "combined: a pending text is left out");

        string lf = File.ReadAllText(Path.Combine(mod, "LoadFolders.xml"));
        Check(lf.Contains("IfModActive=\"a.mod\"") && lf.Contains("IfModActive=\"b.mod\"") && lf.Contains("<li>/</li>"), "combined: each folder loads only when its source mod is active");
        Check(lf.IndexOf("a.mod", StringComparison.Ordinal) < lf.IndexOf("b.mod", StringComparison.Ordinal), "combined: sources are listed in a fixed order whatever the order given");
        string about = File.ReadAllText(Path.Combine(mod, "About", "About.xml"));
        Check(about.Contains("<li>a.mod</li>") && about.Contains("<li>b.mod</li>") && about.Contains("loadAfter") && about.Contains("nelim.rimbabel.all.french"), "combined: About names every source in loadAfter");
        string attr = File.ReadAllText(Path.Combine(root, "ATTRIBUTION.md"));
        Check(attr.Contains("Mod A") && attr.Contains("licence of the source mod: MIT") && attr.Contains("licence of the source mod: unknown") && attr.Contains("Pack (p.k)"), "combined: attribution carries every source licence and imported pack");
        Check(r.PublishBlockers.Any(x => x.Contains("Mod B") && x.Contains("unknown")) && r.PublishBlockers.Any(x => x.Contains("installed translation Pack (p.k)")), "combined: the blockers of every source are in the combined package");

        // A source that is no longer there leaves nothing behind; running again on the same folder is an update.
        PackageResult again = CombinedPackage.Write(root, spec, new[] { a });
        Check(!Directory.Exists(Path.Combine(mod, "b.mod")) && Directory.Exists(Path.Combine(mod, "a.mod")), "combined: a source that dropped out loses its folder");
        Check(!File.ReadAllText(Path.Combine(mod, "LoadFolders.xml")).Contains("b.mod") && !again.PublishBlockers.Any(x => x.Contains("Mod B")), "combined: and its entry and its blockers");

        // A folder that is not ours is refused.
        string foreign = Temp();
        Directory.CreateDirectory(Path.Combine(foreign, "Mod", "Languages"));
        bool refused = false;
        try { CombinedPackage.Write(foreign, spec, new[] { a }); } catch (InvalidOperationException) { refused = true; }
        Check(refused, "combined: a folder with files and no list of sources is not overwritten");
        Directory.Delete(root, true);
        Directory.Delete(foreign, true);
    }
}
