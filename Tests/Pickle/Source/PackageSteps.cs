using System.IO;
using System.Linq;
using RimBabel.Core;
using RimBabel.Game;
using RimWorks.Pickle;
using Verse;

namespace RimBabel.PickleSteps
{
    /// <summary>
    /// The developer-menu action of RimBabel, driven without the menu: pick a loaded mod, write its
    /// translation package, read the manifest that comes out. The state a scenario carries between
    /// steps is the last result of a write.
    ///
    /// Every phrase is prefixed with the mod's name: Pickle loads the steps of every installed suite
    /// into one vocabulary, and two suites declaring the same text make every step ambiguous.
    /// </summary>
    [PickleSteps]
    public class PackageSteps
    {
        private sealed class Written
        {
            public PackageResult Result;
            public ModContentPack Mod;
            public Manifest Manifest;
        }

        [Given("RimBabel: no package exists yet for the mod {string} in {string}")]
        public void NoPackageYet(PickleContext ctx, string modName, string language)
        {
            ModContentPack mod = FindMod(ctx, modName);
            string root = Path.Combine(PackageBuilder.OutputRoot, PackageBuilder.SpecFor(mod, language, "test").PackageId);
            // Removed on purpose, and from a folder this suite owns by name: a leftover package from an
            // earlier run would turn "first write" into "update" and every count below would be wrong.
            if (Directory.Exists(root)) Directory.Delete(root, true);
            ctx.Assert(!Directory.Exists(root), "the package folder is still there after removing it: " + root);
        }

        [When("RimBabel: I write the translation package of the mod {string} for the language {string}")]
        public void Write(PickleContext ctx, string modName, string language)
        {
            ModContentPack mod = FindMod(ctx, modName);
            PackageResult result = DevActions.WriteFor(mod, language);
            Manifest manifest = Manifest.Parse(File.ReadAllText(Path.Combine(result.Root, "Mod", "Babel", "manifest.xml")));
            ctx.Set(new Written { Result = result, Mod = mod, Manifest = manifest });
        }

        [Then("RimBabel: the manifest lists {string} as {string}")]
        public void ManifestLists(PickleContext ctx, string id, string expected)
        {
            Written w = Last(ctx);
            Entry e = w.Manifest.Entries.FirstOrDefault(x => x.Id == id);
            ctx.Assert(e != null, "the manifest does not list " + id + ". It lists " + w.Manifest.Entries.Count + " texts: "
                + string.Join(", ", w.Manifest.Entries.Select(x => x.Id).Take(12).ToArray()));
            // A backslash and an n in the feature stand for a line break, as they do in the game's own files.
            string want = expected.Replace("\\n", "\n");
            ctx.Assert(e.Source == want, id + " is listed with the text '" + e.Source + "', expected '" + want + "'");
        }

        [Then("RimBabel: the manifest does not list {string}")]
        public void ManifestDoesNotList(PickleContext ctx, string id)
        {
            Written w = Last(ctx);
            ctx.Assert(w.Manifest.Entries.All(x => x.Id != id), id + " is listed in the manifest, and it is not a text");
        }

        [Then("RimBabel: the manifest lists no text of a def outside the mod")]
        public void NoForeignDef(PickleContext ctx)
        {
            Written w = Last(ctx);
            var own = w.Mod.AllDefs.Select(d => d.defName).ToList();
            ctx.Assert(own.Count > 0, "the mod has no def loaded, so this check would pass for nothing: " + w.Mod.Name);
            foreach (Entry e in w.Manifest.Entries.Where(x => x.Kind == EntryKind.DefInjected))
            {
                string defName = e.Key.Substring(0, e.Key.IndexOf('.'));
                ctx.Assert(own.Contains(defName), e.Id + " belongs to a def named " + defName + ", which this mod does not own");
            }
        }

        [Then("RimBabel: the manifest lists no text from the language folders of other mods")]
        public void NoForeignKeyed(PickleContext ctx)
        {
            Written w = Last(ctx);
            var keyed = w.Manifest.Entries.Where(x => x.Kind == EntryKind.Keyed).Select(x => x.Key).ToList();
            ctx.Assert(keyed.Count > 0, "no Keyed text is listed at all");
            foreach (string key in keyed)
                ctx.Assert(key.StartsWith("RbFixture_"), "the Keyed text " + key + " is listed, and this mod's Keyed file only holds RbFixture_ keys");
        }

        [Then("RimBabel: every text of the manifest is Pending")]
        public void AllPending(PickleContext ctx)
        {
            Written w = Last(ctx);
            Entry other = w.Manifest.Entries.FirstOrDefault(x => x.Status != EntryStatus.Pending);
            ctx.Assert(other == null, other == null ? "" : other.Id + " is " + other.Status + " on a first write, before any engine has run");
            ctx.Assert(w.Manifest.Entries.Count > 0, "the manifest is empty");
        }

        [Then("RimBabel: the package id is {string}")]
        public void PackageId(PickleContext ctx, string expected)
        {
            Written w = Last(ctx);
            string actual = Path.GetFileName(w.Result.Root);
            ctx.Assert(actual == expected, "the package folder is named " + actual + ", expected " + expected);
            string about = File.ReadAllText(Path.Combine(w.Result.Root, "Mod", "About", "About.xml"));
            ctx.Assert(about.Contains("<packageId>" + expected + "</packageId>"), "About.xml does not carry the package id " + expected);
        }

        [Then("RimBabel: the package is not publishable yet because {string}")]
        public void Blocked(PickleContext ctx, string reason)
        {
            Written w = Last(ctx);
            ctx.Assert(w.Result.PublishBlockers.Any(b => b.Contains(reason)),
                "no publication blocker mentions '" + reason + "'. Blockers: " + string.Join(" | ", w.Result.PublishBlockers.ToArray()));
        }

        [Then("RimBabel: the last write found {int} new, {int} changed and {int} removed texts")]
        public void Counts(PickleContext ctx, int added, int changed, int removed)
        {
            MergeResult m = Last(ctx).Result.Merge;
            ctx.Assert(m.New.Count == added && m.Changed.Count == changed && m.Removed.Count == removed,
                "the last write found " + m.New.Count + " new, " + m.Changed.Count + " changed and " + m.Removed.Count
                + " removed texts, expected " + added + ", " + changed + " and " + removed);
        }

        [Then("RimBabel: the package version is {string}")]
        public void Version(PickleContext ctx, string expected)
        {
            Written w = Last(ctx);
            ctx.Assert(w.Result.Version == expected, "the package version is " + w.Result.Version + ", expected " + expected);
        }

        private static Written Last(PickleContext ctx)
        {
            // Get throws when nothing was set: say which step is missing rather than leak that exception.
            try { return ctx.Get<Written>(); }
            catch (System.InvalidOperationException)
            {
                ctx.Require(false, "no package was written in this scenario: add the 'I write the translation package' step first");
                return null;
            }
        }

        private static ModContentPack FindMod(PickleContext ctx, string name)
        {
            ModContentPack mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(m => m.Name == name);
            ctx.Require(mod != null, "no running mod is named '" + name + "'. Running: "
                + string.Join(", ", LoadedModManager.RunningModsListForReading.Select(m => m.Name).Take(15).ToArray()));
            return mod;
        }
    }
}
