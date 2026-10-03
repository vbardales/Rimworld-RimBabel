using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimBabel.Core;
using Verse;

namespace RimBabel.Game
{
    /// <summary>
    /// Entry point of the mod. It has no settings page yet, and a mod that returns an empty category gets
    /// no entry in the mod options: no empty page, as MOD_SETTINGS.md asks.
    /// </summary>
    public class RimBabelMod : Mod
    {
        public RimBabelMod(ModContentPack content) : base(content) { }

        public override string SettingsCategory() { return ""; }
    }

    /// <summary>
    /// Developer-menu entry for the first version, before there is a window: pick a loaded mod, get its
    /// translation package for the active language. Developer tooling, not player-facing text.
    /// </summary>
    public static class DevActions
    {
        [DebugAction("RimBabel", "Write a translation package for a mod...", allowedGameStates = AllowedGameStates.Entry | AllowedGameStates.Playing)]
        private static void WritePackage()
        {
            string language = LanguageDatabase.activeLanguage.folderName;
            var options = new List<DebugMenuOption>();
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(m => !m.IsCoreMod && !m.IsOfficialMod))
            {
                ModContentPack captured = mod;
                options.Add(new DebugMenuOption(mod.Name, DebugMenuOptionMode.Action, () => WriteFor(captured, language)));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        /// <summary>What the menu entry does once a mod is picked. Public so a test can run it without the menu.</summary>
        public static PackageResult WriteFor(ModContentPack mod, string language)
        {
            PackageResult r = PackageBuilder.Build(mod, language, "RimBabel user");
            Log.Message("[RimBabel] " + mod.Name + " -> " + r.Root + " (version " + r.Version + "): "
                + r.Merge.New.Count + " new, " + r.Merge.Changed.Count + " changed, " + r.Merge.Removed.Count
                + " removed, " + r.Merge.Unchanged + " unchanged. " + (r.PublishBlockers.Count == 0
                    ? "Nothing in the way of publication."
                    : "Not publishable yet: " + string.Join(" ", r.PublishBlockers.ToArray())));
            return r;
        }
    }
}
