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

        [DebugAction("RimBabel", "Translate a mod with the chosen engine...", allowedGameStates = AllowedGameStates.Entry | AllowedGameStates.Playing)]
        private static void TranslateMod()
        {
            string language = SettingsPage.TargetLanguage(RimBabelMod.Settings);
            var options = new List<DebugMenuOption>();
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(m => !m.IsCoreMod && !m.IsOfficialMod))
            {
                ModContentPack captured = mod;
                options.Add(new DebugMenuOption(mod.Name, DebugMenuOptionMode.Action, () => StartTranslation(captured, language)));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("RimBabel", "Take back the texts of one engine...", allowedGameStates = AllowedGameStates.Entry | AllowedGameStates.Playing)]
        private static void TakeBack()
        {
            string language = SettingsPage.TargetLanguage(RimBabelMod.Settings);
            string author = AuthorName();
            var options = new List<DebugMenuOption>();
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(m => !m.IsCoreMod && !m.IsOfficialMod))
            {
                ModContentPack captured = mod;
                List<string> engines = PackageBuilder.EnginesIn(captured, language, author);
                if (engines.Count == 0) continue;
                options.Add(new DebugMenuOption(mod.Name, DebugMenuOptionMode.Action, () =>
                {
                    var inner = new List<DebugMenuOption>();
                    foreach (string engine in engines)
                    {
                        string name = engine;
                        inner.Add(new DebugMenuOption(name, DebugMenuOptionMode.Action, () =>
                            Log.Message("[RimBabel] " + captured.Name + ": " + PackageBuilder.Flush(captured, language, author, name) + " texts of " + name + " are untranslated again.")));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(inner));
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        private static volatile bool translating;

        /// <summary>The engine can take minutes: it runs off the main thread, one run at a time, and says so in the log.</summary>
        private static void StartTranslation(ModContentPack mod, string language)
        {
            if (translating) { Log.Warning("[RimBabel] a translation is already running."); return; }
            RimBabelSettings settings = RimBabelMod.Settings;
            var problems = EngineFactory.Problems(settings.ToConfig());
            if (problems.Count > 0) { Log.Error("[RimBabel] the engine is not set up (" + string.Join(", ", problems.ConvertAll(p => p.ToString()).ToArray()) + "): see Mod options, RimBabel."); return; }
            translating = true;
            string author = AuthorName();
            Log.Message("[RimBabel] translating " + mod.Name + " into " + language + "...");
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    PackageBuilder.TranslationOutcome o = PackageBuilder.Translate(mod, language, author, settings, new WebHttp());
                    Log.Message("[RimBabel] " + mod.Name + " -> " + o.Package.Root + ": " + o.Report.Translated + " translated by " + o.EngineName + ", "
                        + o.Report.Failed + " refused or failed, " + o.Report.Blacklisted + " blacklisted" + (o.Report.Aborted ? ", STOPPED: " + string.Join(" ", o.Report.Failures.ToArray()) : ".")
                        + (o.Package.PublishBlockers.Count == 0 ? " Nothing in the way of publication." : " Not publishable yet: " + string.Join(" ", o.Package.PublishBlockers.ToArray())));
                }
                catch (System.Exception ex) { Log.Error("[RimBabel] the translation of " + mod.Name + " failed: " + ex.Message); }
                finally { translating = false; }
            });
        }

        private static string AuthorName()
        {
            return RimBabelMod.Settings != null && !string.IsNullOrWhiteSpace(RimBabelMod.Settings.author) ? RimBabelMod.Settings.author : "RimBabel user";
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
