using RimBabel.Core;
using Verse;

namespace RimBabel.Game
{
    /// <summary>
    /// What the player sets under Mod options. Plain strings and numbers only: the settings are read in the Mod
    /// constructor, before any Def is loaded, so nothing here can name a Def. The keys are stored as plain text in
    /// the game's configuration folder, which the page says; they are never written into a package.
    /// </summary>
    public class RimBabelSettings : ModSettings
    {
        public EngineKind engine = EngineKind.DeepL;
        public string deeplKey = "";
        public string anthropicKey = "";
        public string openAiKey = "";
        public string anthropicModel = EngineConfig.DefaultAnthropicModel;
        public string openAiModel = "";
        public string openAiUrl = EngineConfig.DefaultOpenAiUrl;
        public int batchSize = EngineConfig.DefaultBatchSize;

        public string author = "";
        /// <summary>A language folder name; empty means the language the game runs in.</summary>
        public string targetLanguage = "";
        /// <summary>Empty means next to the saves.</summary>
        public string outputFolder = "";

        public string dictionaryText = "";
        public string blacklistText = "";

        public EngineConfig ToConfig()
        {
            return new EngineConfig
            {
                Kind = engine, DeepLKey = deeplKey ?? "", AnthropicKey = anthropicKey ?? "", OpenAiKey = openAiKey ?? "",
                AnthropicModel = anthropicModel ?? "", OpenAiModel = openAiModel ?? "", OpenAiUrl = openAiUrl ?? "",
                BatchSize = batchSize,
            };
        }

        /// <summary>Everything on the page except the keys: losing a key to a stray click is not worth the tidiness.</summary>
        public void ResetExceptKeys()
        {
            engine = EngineKind.DeepL;
            anthropicModel = EngineConfig.DefaultAnthropicModel;
            openAiModel = "";
            openAiUrl = EngineConfig.DefaultOpenAiUrl;
            batchSize = EngineConfig.DefaultBatchSize;
            author = "";
            targetLanguage = "";
            outputFolder = "";
            dictionaryText = "";
            blacklistText = "";
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref engine, "engine", EngineKind.DeepL);
            Scribe_Values.Look(ref deeplKey, "deeplKey", "");
            Scribe_Values.Look(ref anthropicKey, "anthropicKey", "");
            Scribe_Values.Look(ref openAiKey, "openAiKey", "");
            Scribe_Values.Look(ref anthropicModel, "anthropicModel", EngineConfig.DefaultAnthropicModel);
            Scribe_Values.Look(ref openAiModel, "openAiModel", "");
            Scribe_Values.Look(ref openAiUrl, "openAiUrl", EngineConfig.DefaultOpenAiUrl);
            Scribe_Values.Look(ref batchSize, "batchSize", EngineConfig.DefaultBatchSize);
            Scribe_Values.Look(ref author, "author", "");
            Scribe_Values.Look(ref targetLanguage, "targetLanguage", "");
            Scribe_Values.Look(ref outputFolder, "outputFolder", "");
            Scribe_Values.Look(ref dictionaryText, "dictionaryText", "");
            Scribe_Values.Look(ref blacklistText, "blacklistText", "");
            base.ExposeData();
            // A value read back as null (a hand-edited file) must not reach the page.
            if (deeplKey == null) deeplKey = "";
            if (anthropicKey == null) anthropicKey = "";
            if (openAiKey == null) openAiKey = "";
            if (anthropicModel == null) anthropicModel = "";
            if (openAiModel == null) openAiModel = "";
            if (openAiUrl == null) openAiUrl = "";
            if (author == null) author = "";
            if (targetLanguage == null) targetLanguage = "";
            if (outputFolder == null) outputFolder = "";
            // The serializer writes the platform's line break, so a file saved on Windows reads back with CR LF: one
            // convention in memory, or each save-and-load would change the text the page shows.
            dictionaryText = Hashing.Normalize(dictionaryText);
            blacklistText = Hashing.Normalize(blacklistText);
            if (batchSize < 1) batchSize = 1;
            if (batchSize > 100) batchSize = 100;
        }
    }
}
