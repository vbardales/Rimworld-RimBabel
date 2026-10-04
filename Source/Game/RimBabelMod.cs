using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimBabel.Core;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimBabel.Game
{
    /// <summary>
    /// Entry point of the mod. Its settings are Mod options -> RimBabel; the optional MainButtons shortcut
    /// (hidden by default) opens the same dialog.
    /// </summary>
    public class RimBabelMod : Mod
    {
        public static RimBabelMod Instance { get; private set; }
        public static RimBabelSettings Settings { get; private set; }

        public RimBabelMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<RimBabelSettings>();
        }

        // The mod's name, a proper name that is the same in every language.
        public override string SettingsCategory() { return "RimBabel"; }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            SettingsPage.Draw(inRect, Settings);
        }
    }

    /// <summary>Optional shortcut to the same native dialog Mod options opens. Hidden by default: a customization mod such as RIMMSQOL may reveal it.</summary>
    public class MainButtonWorker_Settings : MainButtonWorker
    {
        public override void Activate()
        {
            Find.WindowStack.Add(new Dialog_ModSettings(RimBabelMod.Instance));
        }
    }

    /// <summary>
    /// Developer-menu entry that runs the extractor without a window: pick a loaded mod, get its translation
    /// package for the target language. Developer tooling, not player-facing text.
    /// </summary>
    public static class DevActions
    {
        [DebugAction("RimBabel", "Write a translation package for a mod...", allowedGameStates = AllowedGameStates.Entry | AllowedGameStates.Playing)]
        private static void WritePackage()
        {
            string language = SettingsPage.TargetLanguage(RimBabelMod.Settings);
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
            string author = RimBabelMod.Settings != null && !string.IsNullOrWhiteSpace(RimBabelMod.Settings.author)
                ? RimBabelMod.Settings.author : "RimBabel user";
            var clock = System.Diagnostics.Stopwatch.StartNew();
            PackageResult r = PackageBuilder.Build(mod, language, author);
            clock.Stop();
            Log.Message("[RimBabel] " + mod.Name + " -> " + r.Root + " (version " + r.Version + "): "
                + r.Merge.New.Count + " new, " + r.Merge.Changed.Count + " changed, " + r.Merge.Removed.Count
                + " removed, " + r.Merge.Unchanged + " unchanged, in " + clock.ElapsedMilliseconds + " ms. " + (r.PublishBlockers.Count == 0
                    ? "Nothing in the way of publication."
                    : "Not publishable yet: " + string.Join(" ", r.PublishBlockers.ToArray())));
            return r;
        }
    }
}
