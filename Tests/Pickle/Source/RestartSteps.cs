using System.IO;
using System.Linq;
using System.Reflection;
using RimBabel.Core;
using RimBabel.Game;
using RimWorks.Pickle;
using Verse;

namespace RimBabel.PickleSteps
{
    /// <summary>
    /// The settings across a real restart of the game, in three launches under one hold of the lock (Submit-PickleRun -Filter
    /// 06-restart-write -Then 07-restart-read, 08-restart-reset). A value written in one process and read back from the same
    /// process proves nothing: it is still in memory. The second launch reads what RimBabel's settings hold when the game has just
    /// started, which is what the game loaded from the file the first launch left.
    ///
    /// The first launch leaves its values on purpose (its last step says so, and comes last): a scenario that fails before it puts
    /// everything back. Phrases carry the mod's name: Pickle loads every installed suite's steps into one vocabulary.
    /// </summary>
    [PickleSteps]
    public class RestartSteps
    {
        private const string Author = "Pickle Restart";
        private const int Batch = 37;
        private const string Dictionary = "colonist = colon\nraider = pillard";
        private const string Blacklist = "K:Restart_*";

        private static bool written, kept;

        [When("RimBabel: the settings are set to the values of the restart test and written")]
        public void WriteValues(PickleContext ctx)
        {
            RimBabelSettings s = Settings(ctx);
            written = true;
            s.author = Author;
            s.batchSize = Batch;
            s.engine = EngineKind.OpenAiCompatible;
            s.openAiModel = "restart-model";
            s.dictionaryText = Dictionary;
            s.blacklistText = Blacklist;
            RimBabelMod.Instance.WriteSettings();
        }

        [Then("RimBabel: the settings file holds the values of the restart test")]
        public void FileHoldsValues(PickleContext ctx)
        {
            string path = SettingsFile();
            ctx.Assert(File.Exists(path), "the game wrote no settings file at " + path);
            string content = File.ReadAllText(path);
            ctx.Assert(content.Contains(Author) && content.Contains("restart-model") && content.Contains("K:Restart_*"),
                "the settings file " + path + " does not hold the values of the restart test");
        }

        [Then("RimBabel: the values of the restart test are kept for the next launch")]
        public void Keep(PickleContext ctx) { kept = true; }

        // Nothing ran in this process before this scenario: what the settings hold is what the game loaded at startup.
        [Then("RimBabel: the settings the game loaded at startup hold the values of the restart test")]
        public void LoadedAtStartup(PickleContext ctx)
        {
            RimBabelSettings s = Settings(ctx);
            ctx.Assert(s.author == Author, "the author loaded at startup is '" + s.author + "', not '" + Author + "'");
            ctx.Assert(s.batchSize == Batch, "the batch size loaded at startup is " + s.batchSize + ", not " + Batch);
            ctx.Assert(s.engine == EngineKind.OpenAiCompatible && s.openAiModel == "restart-model", "the engine loaded at startup is " + s.engine + " / '" + s.openAiModel + "'");
            ctx.Assert(Normal(s.dictionaryText) == Dictionary, "the dictionary loaded at startup is '" + s.dictionaryText + "', not the two lines written; a line break may have changed");
            ctx.Assert(Normal(s.blacklistText) == Blacklist, "the blacklist loaded at startup is '" + s.blacklistText + "'");
        }

        [When("RimBabel: the settings are put back to their defaults and written")]
        public void Reset(PickleContext ctx)
        {
            RimBabelSettings s = Settings(ctx);
            s.ResetExceptKeys();
            s.batchSize = EngineConfig.DefaultBatchSize;
            RimBabelMod.Instance.WriteSettings();
            kept = false;
        }

        [Then("RimBabel: no value of the restart test remains in the settings file")]
        public void NothingRemains(PickleContext ctx)
        {
            string path = SettingsFile();
            string content = File.Exists(path) ? File.ReadAllText(path) : "";
            ctx.Assert(!content.Contains(Author) && !content.Contains("restart-model") && !content.Contains("K:Restart_*"),
                "a value of the restart test is still in " + path);
        }

        // A first launch that failed before its last step leaves nothing behind.
        [AfterScenario]
        public void ForgetIfNotKept(PickleContext ctx)
        {
            if (!written || kept || RimBabelMod.Settings == null) return;
            RimBabelMod.Settings.ResetExceptKeys();
            RimBabelMod.Settings.batchSize = EngineConfig.DefaultBatchSize;
            RimBabelMod.Instance.WriteSettings();
            written = false;
        }

        private static string Normal(string text) { return (text ?? "").Replace("\r", ""); }

        private static RimBabelSettings Settings(PickleContext ctx)
        {
            ctx.Require(RimBabelMod.Settings != null, "RimBabel's settings are not loaded: the mod did not start");
            return RimBabelMod.Settings;
        }

        private static string SettingsFile()
        {
            MethodInfo m = typeof(LoadedModManager).GetMethod("GetSettingsFilename", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return (string)m.Invoke(null, new object[] { RimBabelMod.Instance.Content.FolderName, RimBabelMod.Instance.GetType().Name });
        }
    }
}
