using System;
using System.IO;
using RimBabel.Core;
using RimBabel.Game;
using Verse;

// RimBabelSettings through the game's own Scribe, the way the game saves and loads a mod's settings: the shipped ExposeData and
// the game's serializer, not a substitute. Nothing about the page's layout, which needs a running game.
internal static partial class Program
{
    private static void SettingsRoundTrip(string root)
    {
        UnityEngine.Debug.unityLogger.logHandler = new ThrowingLogHandler();
        DeepProfiler.enabled = false;   // no game preferences or profiler in this process

        // No mod loader runs here: register the serialized type with the game's type lookup.
        var cache = (System.Collections.IDictionary)typeof(GenTypes).GetField("typeCache", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
        var keyType = typeof(GenTypes).GetNestedType("TypeCacheKey", System.Reflection.BindingFlags.NonPublic);
        cache[Activator.CreateInstance(keyType, new object[] { typeof(RimBabelSettings).FullName, null })] = typeof(RimBabelSettings);

        string path = Path.Combine(root, "settings.xml");

        // First use: a file with nothing in it gives the documented defaults.
        File.WriteAllText(path, "<settings />");
        RimBabelSettings d = LoadSettings(path);
        Check(d.engine == EngineKind.DeepL && d.batchSize == 20 && d.anthropicModel == EngineConfig.DefaultAnthropicModel, "settings: a clean configuration loads the documented defaults");
        Check(d.openAiUrl == EngineConfig.DefaultOpenAiUrl && d.deeplKey == "" && d.anthropicKey == "" && d.openAiKey == "" && d.author == "" && d.targetLanguage == "" && d.outputFolder == "", "settings: no key, no author and the default folder and language on a clean configuration");

        // Every field survives a save and a load, including text with line breaks and unusual characters.
        var s = new RimBabelSettings
        {
            engine = EngineKind.OpenAiCompatible, deeplKey = "dk:fx", anthropicKey = "sk-ant", openAiKey = "ok",
            anthropicModel = "claude-x", openAiModel = "qwen2.5", openAiUrl = "http://localhost:1234/v1", batchSize = 7,
            author = "Virginie \u00e9\u00e0", targetLanguage = "French", outputFolder = "D:\\Packages",
            dictionaryText = "colonist = colon\nhome = maison\n# note", blacklistText = "K:Name_*\nre:^D:Pawn",
        };
        SaveSettings(path, s);
        RimBabelSettings back = LoadSettings(path);
        Check(back.engine == EngineKind.OpenAiCompatible && back.batchSize == 7, "settings: the engine and the batch size survive a save and a load");
        Check(back.deeplKey == "dk:fx" && back.anthropicKey == "sk-ant" && back.openAiKey == "ok", "settings: the three keys survive");
        Check(back.anthropicModel == "claude-x" && back.openAiModel == "qwen2.5" && back.openAiUrl == "http://localhost:1234/v1", "settings: models and server address survive");
        Check(back.author == "Virginie \u00e9\u00e0" && back.targetLanguage == "French" && back.outputFolder == "D:\\Packages", "settings: author, language and folder survive, accents included");
        Check(back.dictionaryText == s.dictionaryText && back.blacklistText == s.blacklistText, "settings: the dictionary and the blacklist keep their line breaks (got " + Show(back.dictionaryText) + " | file: " + Show(File.ReadAllText(path)) + ")");
        EngineConfig c = back.ToConfig();
        Check(c.Kind == EngineKind.OpenAiCompatible && c.OpenAiModel == "qwen2.5" && c.BatchSize == 7 && c.DeepLKey == "dk:fx", "settings: ToConfig carries what the page set");

        // The keys are in the settings file, and only there: the manifest and package files are written without them.
        string manifestXml = new Manifest { Language = "French" }.ToXml();
        Check(!manifestXml.Contains("dk:fx") && !manifestXml.Contains("sk-ant"), "settings: a key is never part of a manifest");

        // Out-of-range values written by hand are brought back.
        File.WriteAllText(path, "<settings><batchSize>0</batchSize></settings>");
        Check(LoadSettings(path).batchSize == 1, "settings: a batch size below 1 is brought back to 1");
        File.WriteAllText(path, "<settings><batchSize>5000</batchSize></settings>");
        Check(LoadSettings(path).batchSize == 100, "settings: a batch size above 100 is brought back to 100");

        // Reset keeps the keys and nothing else.
        s.ResetExceptKeys();
        Check(s.engine == EngineKind.DeepL && s.batchSize == 20 && s.author == "" && s.dictionaryText == "" && s.blacklistText == "" && s.outputFolder == "" && s.targetLanguage == "", "settings: reset restores everything on the page");
        Check(s.deeplKey == "dk:fx" && s.anthropicKey == "sk-ant" && s.openAiKey == "ok", "settings: reset keeps the keys");

        // A key is shown masked.
        Check(SettingsPage.Mask("") == "" && SettingsPage.Mask("abcd") == "****", "settings: a short key is masked entirely");
        string shown = SettingsPage.Mask("abcdefghijklmnopqrstuvwxyz0123");
        Check(shown.StartsWith("abc") && shown.EndsWith("123") && !shown.Contains("defgh") && !shown.Contains("xyz0"), "settings: a long key shows its first and last characters only (" + shown + ")");
    }

    private static string Show(string text)
    {
        return text.Replace(((char)13).ToString(), "<CR>").Replace(((char)10).ToString(), "<LF>");
    }

    private static void SaveSettings(string path, RimBabelSettings settings)
    {
        try
        {
            Scribe.saver.InitSaving(path, "settings");
            settings.ExposeData();
            Scribe.saver.FinalizeSaving();
        }
        finally { Scribe.ForceStop(); }
    }

    private static RimBabelSettings LoadSettings(string path)
    {
        try
        {
            Scribe.loader.InitLoading(path);
            var settings = new RimBabelSettings();
            settings.ExposeData();
            Scribe.loader.FinalizeLoading();
            return settings;
        }
        finally { Scribe.ForceStop(); }
    }

    private sealed class ThrowingLogHandler : UnityEngine.ILogHandler
    {
        public void LogFormat(UnityEngine.LogType type, UnityEngine.Object context, string format, params object[] args)
        {
            string message = string.Format(format, args);
            Console.WriteLine(type + ": " + message);
            if (!message.StartsWith("An error occurred while logging an error:") && (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception))
                throw new InvalidOperationException(message);
        }
        public void LogException(Exception exception, UnityEngine.Object context) { throw exception; }
    }
}
