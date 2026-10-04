using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RimBabel.Core;
using RimBabel.Game;
using RimWorks.Pickle;
using RimWorld;
using Verse;

namespace RimBabel.PickleSteps
{
    /// <summary>
    /// The settings page and its optional hidden shortcut, as MOD_SETTINGS.md puts them: Mod options is the primary route, the
    /// MainButtons entry is hidden by default (neither visible nor greyed) and opens the same dialog. What a game answers that
    /// no offline test can: whether the page's drawing code runs without a logged error, which mod the dialog belongs to, that
    /// the game writes the settings file, and that every text exists in the language of the pass.
    ///
    /// Phrases carry the mod's name: Pickle loads every installed suite's steps into one vocabulary.
    /// </summary>
    [PickleSteps]
    public class SettingsSteps
    {
        private const string ShortcutDefName = "RimBabel_Settings";

        // What the scenario found, put back after it: settings are shared with the next scenario and the next run.
        private static Dictionary<string, object> before;

        [Given("RimBabel: settings are at their defaults")]
        public void Defaults(PickleContext ctx)
        {
            RimBabelSettings s = Settings(ctx);
            if (before == null) before = Snapshot(s);
            s.ResetExceptKeys();
            s.deeplKey = s.anthropicKey = s.openAiKey = "";
            Shortcut(ctx).buttonVisible = false;
        }

        [Then("RimBabel's settings shortcut is hidden on a clean configuration")]
        public void HiddenByDefault(PickleContext ctx)
        {
            MainButtonDef def = Shortcut(ctx);
            ctx.Assert(!def.buttonVisible, "RimBabel_Settings ships with buttonVisible true: it would stand in everyone's main bar without anyone asking for it");
            ctx.Assert(!def.Worker.Visible, "the shortcut reports Visible true with buttonVisible " + def.buttonVisible + ": it shows without anything having revealed it");
        }

        [When("RimBabel's settings shortcut is revealed, as a customization mod would")]
        public void Reveal(PickleContext ctx) { Shortcut(ctx).buttonVisible = true; }

        [When("RimBabel's settings shortcut is hidden again")]
        public void Hide(PickleContext ctx) { Shortcut(ctx).buttonVisible = false; }

        // Both halves matter: MOD_SETTINGS.md forbids a greyed shortcut as firmly as a visible one, and a def can be drawn and dead.
        [Then("RimBabel's settings shortcut is drawn in the bar")]
        public void Drawn(PickleContext ctx)
        {
            MainButtonDef def = Shortcut(ctx);
            ctx.Assert(def.Worker.Visible, "the shortcut has been revealed and its worker still reports Visible false, so a customization mod cannot put it in the bar");
            ctx.Assert(!def.Worker.Disabled, "the shortcut is drawn but greyed out, which MOD_SETTINGS.md forbids");
        }

        [Then("RimBabel's settings shortcut is not drawn in the bar")]
        public void NotDrawn(PickleContext ctx)
        {
            ctx.Assert(!Shortcut(ctx).Worker.Visible, "the shortcut was hidden again and its worker still reports Visible true");
        }

        [When("RimBabel's settings shortcut is activated")]
        public void Activate(PickleContext ctx) { Shortcut(ctx).Worker.Activate(); }

        // The claim is not "a settings window opened" but "the SAME settings opened": a dialog built for another mod looks
        // identical in a screenshot, so the window is asked which mod it is for.
        [Then("a settings dialog is open for RimBabel")]
        public void DialogOpen(PickleContext ctx)
        {
            ctx.Require(Find.WindowStack != null, "there is no window stack: no game and no main menu is running");
            var dialogs = Find.WindowStack.Windows.OfType<Dialog_ModSettings>().ToList();
            ctx.Assert(dialogs.Count > 0, "no Dialog_ModSettings is open: activating the shortcut opened nothing at all");
            bool found = dialogs.Any(d => ModOf(ctx, d) == RimBabelMod.Instance);
            ctx.Assert(found, "a settings dialog is open, but not this mod's: it was built for "
                + string.Join(", ", dialogs.Select(d => ModOf(ctx, d) == null || ModOf(ctx, d).Content == null ? "an unknown mod" : ModOf(ctx, d).Content.Name).ToArray()));
        }

        // The page draws in OnGUI, where an exception is logged as an error rather than thrown: letting it draw for a few
        // frames, then asking the log, is what shows that its code ran. Frames, not ticks: a settings window pauses the game.
        [When("RimBabel: the settings window draws for {int} frames")]
        public async Task Draws(PickleContext ctx, int frames)
        {
            await ctx.WaitFrames(frames);
        }

        [When("RimBabel: I set the author name to {string}")]
        public void SetAuthor(PickleContext ctx, string name) { Settings(ctx).author = name; }

        [When("RimBabel: the settings are written")]
        public void Write(PickleContext ctx) { RimBabelMod.Instance.WriteSettings(); }

        [Then("RimBabel: the settings file holds {string}")]
        public void FileHolds(PickleContext ctx, string text)
        {
            string path = SettingsFile();
            ctx.Assert(File.Exists(path), "the game wrote no settings file at " + path);
            string content = File.ReadAllText(path);
            ctx.Assert(content.Contains(text), "the settings file " + path + " does not hold '" + text + "'");
        }

        [Then("RimBabel: the settings file holds no key")]
        public void FileHoldsNoKey(PickleContext ctx)
        {
            string path = SettingsFile();
            ctx.Assert(File.Exists(path), "the game wrote no settings file at " + path);
            string content = File.ReadAllText(path);
            ctx.Assert(content.Contains("<deeplKey></deeplKey>") || content.Contains("<deeplKey />") || !content.Contains("<deeplKey>"),
                "a key is in the settings file although the scenario set none");
        }

        // Compared with what the active language really has loaded, not with the other language's folder: both can omit a key.
        // In developer mode (every Pickle run) a key missing from the active language is not shown in English but as accented
        // gibberish, which a capture shows and a person has to notice; this asks the language itself.
        [Then("RimBabel: every settings text exists in the language of this pass")]
        public void EveryTextExists(PickleContext ctx)
        {
            ModContentPack mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(m => m.PackageId == "nelim.rimbabel");
            ctx.Require(mod != null, "RimBabel is not among the running mods");
            string file = Path.Combine(mod.RootDir, "Languages", "English", "Keyed", "RimBabel.xml");
            ctx.Require(File.Exists(file), "the English Keyed file is not where expected: " + file);
            var keys = KeyedXml.Parse(File.ReadAllText(file)).Select(kv => kv.Key).ToList();
            ctx.Assert(keys.Count > 20, "only " + keys.Count + " keys were read from " + file);

            LoadedLanguage language = LanguageDatabase.activeLanguage;
            List<string> missing = keys.Where(k => !language.HaveTextForKey(k)).ToList();
            ctx.Assert(missing.Count == 0, missing.Count + " of " + keys.Count + " settings texts are missing in " + language.folderName + ": "
                + string.Join(", ", missing.Take(8).ToArray()));
        }

        [AfterScenario]
        public void PutSettingsBack(PickleContext ctx)
        {
            if (RimBabelMod.Settings == null) return;
            MainButtonDef def = DefDatabase<MainButtonDef>.GetNamedSilentFail(ShortcutDefName);
            if (def != null) def.buttonVisible = false;
            if (before != null)
            {
                Restore(RimBabelMod.Settings, before);
                before = null;
                RimBabelMod.Instance.WriteSettings();
            }
        }

        // The game builds the file name itself and keeps the method private; asking it avoids guessing how the name is made.
        private static string SettingsFile()
        {
            MethodInfo m = typeof(LoadedModManager).GetMethod("GetSettingsFilename", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return (string)m.Invoke(null, new object[] { RimBabelMod.Instance.Content.FolderName, RimBabelMod.Instance.GetType().Name });
        }

        private static RimBabelSettings Settings(PickleContext ctx)
        {
            ctx.Require(RimBabelMod.Settings != null, "RimBabel's settings are not loaded: the mod did not start");
            return RimBabelMod.Settings;
        }

        private static MainButtonDef Shortcut(PickleContext ctx)
        {
            MainButtonDef def = DefDatabase<MainButtonDef>.GetNamedSilentFail(ShortcutDefName);
            ctx.Require(def != null, "no MainButtonDef named '" + ShortcutDefName + "': the shortcut RIMMSQOL is meant to be able to reveal is not shipped at all");
            return def;
        }

        private static Dictionary<string, object> Snapshot(RimBabelSettings s)
        {
            return typeof(RimBabelSettings).GetFields(BindingFlags.Instance | BindingFlags.Public).ToDictionary(f => f.Name, f => f.GetValue(s));
        }

        private static void Restore(RimBabelSettings s, Dictionary<string, object> values)
        {
            foreach (FieldInfo f in typeof(RimBabelSettings).GetFields(BindingFlags.Instance | BindingFlags.Public))
                f.SetValue(s, values[f.Name]);
        }

        // Dialog_ModSettings keeps the mod it was built for in a private field whose name has moved between versions.
        private static Mod ModOf(PickleContext ctx, Dialog_ModSettings dialog)
        {
            System.Type type = typeof(Dialog_ModSettings);
            FieldInfo field = type.GetField("mod", BindingFlags.Instance | BindingFlags.NonPublic) ?? type.GetField("selMod", BindingFlags.Instance | BindingFlags.NonPublic);
            ctx.Require(field != null, "Dialog_ModSettings has neither a 'mod' nor a 'selMod' field in this version; it has: "
                + string.Join(", ", type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Select(f => f.Name).ToArray()));
            return field.GetValue(dialog) as Mod;
        }
    }
}
