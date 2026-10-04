using System;
using System.Collections.Generic;
using System.Linq;
using RimBabel.Core;
using UnityEngine;
using Verse;

namespace RimBabel.Game
{
    /// <summary>
    /// The contents of Mod options -> RimBabel, drawn by <see cref="RimBabelMod"/>, and the same page the hidden
    /// MainButtons shortcut opens (it opens the native settings dialog of the mod, not a second window).
    /// </summary>
    public static class SettingsPage
    {
        private static Vector2 scroll;
        private static float viewHeight = 1400f;
        private static Vector2 dictionaryScroll, blacklistScroll;
        private static bool showKeys;

        // The engine test runs off the main thread: a service can take many seconds and the page must not freeze.
        private static volatile int testState;   // 0 none, 1 running, 2 worked, 3 failed
        private static volatile string testDetail = "";

        public static void Draw(Rect inRect, RimBabelSettings s)
        {
            var view = new Rect(0f, 0f, inRect.width - 20f, viewHeight);
            Widgets.BeginScrollView(inRect, ref scroll, view);
            var l = new Listing_Standard();
            l.Begin(view);

            l.Label("RimBabel.Settings.Intro".Translate());
            l.Gap(4f);

            DrawEngine(l, s);
            l.GapLine(10f);
            DrawPackages(l, s);
            l.GapLine(10f);
            DrawDictionary(l, s);
            l.GapLine(10f);
            DrawBlacklist(l, s);
            l.GapLine(10f);

            Rect reset = l.GetRect(30f);
            if (Widgets.ButtonText(reset.LeftPart(0.35f), "RimBabel.Settings.Reset".Translate()))
                s.ResetExceptKeys();
            TooltipHandler.TipRegion(reset.LeftPart(0.35f), "RimBabel.Settings.ResetDesc".Translate());
            l.Label("RimBabel.Settings.Applies".Translate());

            if (Event.current.type == EventType.Layout) viewHeight = l.CurHeight + 12f;
            l.End();
            Widgets.EndScrollView();
        }

        // ---- engine ----

        private static void DrawEngine(Listing_Standard l, RimBabelSettings s)
        {
            Text.Font = GameFont.Medium;
            l.Label("RimBabel.Settings.EngineHeader".Translate());
            Text.Font = GameFont.Small;
            l.Label("RimBabel.Settings.EngineDesc".Translate());

            if (l.RadioButton("RimBabel.Settings.EngineDeepL".Translate(), s.engine == EngineKind.DeepL)) s.engine = EngineKind.DeepL;
            if (l.RadioButton("RimBabel.Settings.EngineAnthropic".Translate(), s.engine == EngineKind.Anthropic)) s.engine = EngineKind.Anthropic;
            if (l.RadioButton("RimBabel.Settings.EngineOpenAi".Translate(), s.engine == EngineKind.OpenAiCompatible)) s.engine = EngineKind.OpenAiCompatible;
            l.Gap(4f);

            switch (s.engine)
            {
                case EngineKind.DeepL:
                    s.deeplKey = KeyRow(l, s.deeplKey);
                    break;
                case EngineKind.Anthropic:
                    s.anthropicKey = KeyRow(l, s.anthropicKey);
                    s.anthropicModel = TextRow(l, "RimBabel.Settings.Model", "RimBabel.Settings.ModelDesc", s.anthropicModel);
                    break;
                default:
                    s.openAiUrl = TextRow(l, "RimBabel.Settings.Url", "RimBabel.Settings.UrlDesc", s.openAiUrl);
                    s.openAiModel = TextRow(l, "RimBabel.Settings.Model", "RimBabel.Settings.ModelDesc", s.openAiModel);
                    s.openAiKey = KeyRow(l, s.openAiKey);
                    break;
            }

            Rect batchRow = l.GetRect(28f);
            Widgets.Label(batchRow.LeftPart(0.5f), "RimBabel.Settings.Batch".Translate(s.batchSize));
            s.batchSize = Mathf.RoundToInt(Widgets.HorizontalSlider(batchRow.RightPart(0.5f), s.batchSize, 1f, 50f, middleAlignment: true));
            TooltipHandler.TipRegion(batchRow, "RimBabel.Settings.BatchDesc".Translate());

            foreach (EngineProblem p in EngineFactory.Problems(s.ToConfig()))
            {
                GUI.color = new Color(1f, 0.6f, 0.4f);
                l.Label(("RimBabel.Settings.Problem." + p).Translate());
                GUI.color = Color.white;
            }

            Rect testRow = l.GetRect(30f);
            bool running = testState == 1;
            if (Widgets.ButtonText(testRow.LeftPart(0.35f), "RimBabel.Settings.Test".Translate()) && !running)
                RunTest(s.ToConfig(), TargetLanguage(s));
            TooltipHandler.TipRegion(testRow.LeftPart(0.35f), "RimBabel.Settings.TestDesc".Translate());
            string status = testState == 1 ? "RimBabel.Settings.TestRunning".Translate().Resolve()
                : testState == 2 ? "RimBabel.Settings.TestOk".Translate(testDetail).Resolve()
                : testState == 3 ? "RimBabel.Settings.TestFailed".Translate(testDetail).Resolve() : "";
            if (status.Length > 0) Widgets.Label(new Rect(testRow.x + testRow.width * 0.37f, testRow.y + 4f, testRow.width * 0.63f, 60f), status);
            l.Gap(testState >= 2 ? 34f : 0f);
        }

        private static string KeyRow(Listing_Standard l, string key)
        {
            Rect row = l.GetRect(30f);
            Widgets.Label(row.LeftPart(0.25f), "RimBabel.Settings.Key".Translate());
            Rect field = new Rect(row.x + row.width * 0.27f, row.y, row.width * 0.5f, row.height);
            TooltipHandler.TipRegion(row, "RimBabel.Settings.KeyDesc".Translate());

            // Shown in clear while empty, so a first key can be pasted; otherwise hidden unless asked for.
            if (showKeys || string.IsNullOrEmpty(key))
                key = Widgets.TextField(field, key ?? "");
            else
                Widgets.Label(new Rect(field.x + 4f, field.y + 3f, field.width, field.height), Mask(key));

            if (Widgets.ButtonText(new Rect(row.x + row.width * 0.79f, row.y, row.width * 0.21f, row.height),
                    (showKeys ? "RimBabel.Settings.KeyHide" : "RimBabel.Settings.KeyShow").Translate()))
                showKeys = !showKeys;
            return key;
        }

        /// <summary>The first and last characters only, never the whole key.</summary>
        public static string Mask(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            if (key.Length <= 8) return new string('*', key.Length);
            return key.Substring(0, 3) + new string('*', Math.Min(key.Length - 6, 20)) + key.Substring(key.Length - 3);
        }

        private static string TextRow(Listing_Standard l, string labelKey, string tipKey, string value)
        {
            Rect row = l.GetRect(30f);
            Widgets.Label(row.LeftPart(0.25f), labelKey.Translate());
            string result = Widgets.TextField(new Rect(row.x + row.width * 0.27f, row.y, row.width * 0.73f, row.height), value ?? "");
            TooltipHandler.TipRegion(row, tipKey.Translate());
            return result;
        }

        private static void RunTest(EngineConfig config, string language)
        {
            testState = 1;
            testDetail = "";
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ITranslationEngine engine = EngineFactory.Create(config, new WebHttp());
                    const string sample = "Hello {0}, welcome to the colony.";
                    Protected p = Protector.Protect(sample, new KeyValuePair<string, string>[0]);
                    string[] answer = engine.Translate(new[] { p.Text }, SourceScanner.SourceLanguage, language, new KeyValuePair<string, string>[0]);
                    Restored r = Protector.Restore(answer[0], p, sample);
                    testDetail = r.Ok ? r.Text : r.Reason;
                    testState = r.Ok ? 2 : 3;
                }
                catch (Exception ex)
                {
                    // The engines never put a key in a message.
                    testDetail = ex.Message;
                    testState = 3;
                }
            });
        }

        // ---- packages ----

        private static void DrawPackages(Listing_Standard l, RimBabelSettings s)
        {
            Text.Font = GameFont.Medium;
            l.Label("RimBabel.Settings.PackagesHeader".Translate());
            Text.Font = GameFont.Small;

            s.author = TextRow(l, "RimBabel.Settings.Author", "RimBabel.Settings.AuthorDesc", s.author);

            Rect langRow = l.GetRect(30f);
            Widgets.Label(langRow.LeftPart(0.25f), "RimBabel.Settings.Language".Translate());
            string shown = string.IsNullOrEmpty(s.targetLanguage)
                ? "RimBabel.Settings.LanguageGame".Translate(LanguageDatabase.activeLanguage.folderName).Resolve()
                : s.targetLanguage;
            Rect langButton = new Rect(langRow.x + langRow.width * 0.27f, langRow.y, langRow.width * 0.73f, langRow.height);
            if (Widgets.ButtonText(langButton, shown))
            {
                var options = new List<FloatMenuOption>
                {
                    new FloatMenuOption("RimBabel.Settings.LanguageGame".Translate(LanguageDatabase.activeLanguage.folderName).Resolve(), () => s.targetLanguage = ""),
                };
                foreach (LoadedLanguage lang in LanguageDatabase.AllLoadedLanguages.Where(x => x.folderName != SourceScanner.SourceLanguage))
                {
                    string folder = lang.folderName;
                    options.Add(new FloatMenuOption(lang.DisplayName + " (" + folder + ")", () => s.targetLanguage = folder));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            TooltipHandler.TipRegion(langRow, "RimBabel.Settings.LanguageDesc".Translate());

            s.outputFolder = TextRow(l, "RimBabel.Settings.Output", "RimBabel.Settings.OutputDesc", s.outputFolder);
            Rect openRow = l.GetRect(44f);
            Widgets.Label(new Rect(openRow.x, openRow.y, openRow.width * 0.7f, openRow.height),
                string.IsNullOrWhiteSpace(s.outputFolder) ? "RimBabel.Settings.OutputDefault".Translate(PackageBuilder.OutputRoot).Resolve() : PackageBuilder.OutputRoot);
            if (Widgets.ButtonText(new Rect(openRow.xMax - openRow.width * 0.28f, openRow.y, openRow.width * 0.28f, 30f), "RimBabel.Settings.OutputOpen".Translate()))
                OpenFolder(PackageBuilder.OutputRoot);
        }

        private static void OpenFolder(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory(path);
                Application.OpenURL("file:///" + path.Replace('\\', '/'));
            }
            catch (Exception ex)
            {
                // A folder that cannot be opened is not a reason to break the page.
                Log.Warning("[RimBabel] Could not open " + path + ": " + ex.Message);
            }
        }

        /// <summary>The language a package is translated into: the player's choice, else the language the game runs in.</summary>
        public static string TargetLanguage(RimBabelSettings s)
        {
            return string.IsNullOrWhiteSpace(s.targetLanguage) ? LanguageDatabase.activeLanguage.folderName : s.targetLanguage;
        }

        // ---- dictionary and blacklist ----

        private static void DrawDictionary(Listing_Standard l, RimBabelSettings s)
        {
            Text.Font = GameFont.Medium;
            l.Label("RimBabel.Settings.DictionaryHeader".Translate());
            Text.Font = GameFont.Small;
            l.Label("RimBabel.Settings.DictionaryDesc".Translate());
            s.dictionaryText = ScrollableTextArea(l.GetRect(140f), s.dictionaryText ?? "", ref dictionaryScroll);
            int ignored;
            LineLists.ParseDictionary(s.dictionaryText, out ignored);
            if (ignored > 0) Warn(l, Counted("RimBabel.Settings.DictionaryBad", ignored));
        }

        private static void DrawBlacklist(Listing_Standard l, RimBabelSettings s)
        {
            Text.Font = GameFont.Medium;
            l.Label("RimBabel.Settings.BlacklistHeader".Translate());
            Text.Font = GameFont.Small;
            l.Label("RimBabel.Settings.BlacklistDesc".Translate());
            s.blacklistText = ScrollableTextArea(l.GetRect(140f), s.blacklistText ?? "", ref blacklistScroll);
            int bad = Blacklist.Invalid(LineLists.ParseRules(s.blacklistText)).Count;
            if (bad > 0) Warn(l, Counted("RimBabel.Settings.BlacklistBad", bad));
        }

        private static void Warn(Listing_Standard l, string text)
        {
            GUI.color = new Color(1f, 0.6f, 0.4f);
            l.Label(text);
            GUI.color = Color.white;
        }

        /// <summary>
        /// A text area that scrolls: the game has none, so the text is laid out in a view as tall as the text needs, inside a scroll view.
        /// Typing at the end keeps the caret in view because the view grows with the text.
        /// </summary>
        private static string ScrollableTextArea(Rect outRect, string text, ref Vector2 scrollPos)
        {
            float width = outRect.width - 20f;
            float height = Mathf.Max(outRect.height, Text.CalcHeight(text + "\n", width) + 8f);
            var view = new Rect(0f, 0f, width, height);
            Widgets.BeginScrollView(outRect, ref scrollPos, view);
            string result = Widgets.TextArea(view, text);
            Widgets.EndScrollView();
            return result;
        }

        /// <summary>A counted phrase from one complete key per form, as TRANSLATIONS.md asks: never a suffix added to a word.</summary>
        public static string Counted(string key, int count)
        {
            return (count == 1 ? key + ".One" : key + ".Many").Translate(count).Resolve();
        }
    }
}
