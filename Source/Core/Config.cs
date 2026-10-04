using System;
using System.Collections.Generic;

namespace RimBabel.Core
{
    public enum EngineKind : byte { DeepL = 0, Anthropic = 1, OpenAiCompatible = 2 }

    /// <summary>What can be wrong with an engine's settings before any request is made.</summary>
    public enum EngineProblem { NoKey, NoModel, BadUrl }

    /// <summary>The engine part of the settings, as plain data: no game type, so it is tested without the game.</summary>
    public sealed class EngineConfig
    {
        public const string DefaultAnthropicModel = "claude-haiku-4-5-20251001";
        public const string DefaultOpenAiUrl = "http://localhost:11434/v1";
        public const int DefaultBatchSize = 20;

        public EngineKind Kind = EngineKind.DeepL;
        public string DeepLKey = "";
        public string AnthropicKey = "";
        public string OpenAiKey = "";
        public string AnthropicModel = DefaultAnthropicModel;
        public string OpenAiModel = "";
        public string OpenAiUrl = DefaultOpenAiUrl;
        public int BatchSize = DefaultBatchSize;
    }

    public static class EngineFactory
    {
        /// <summary>What stops the chosen engine from working. An OpenAI-compatible server may need no key (a local one asks for none).</summary>
        public static List<EngineProblem> Problems(EngineConfig c)
        {
            var list = new List<EngineProblem>();
            switch (c.Kind)
            {
                case EngineKind.DeepL:
                    if (string.IsNullOrWhiteSpace(c.DeepLKey)) list.Add(EngineProblem.NoKey);
                    break;
                case EngineKind.Anthropic:
                    if (string.IsNullOrWhiteSpace(c.AnthropicKey)) list.Add(EngineProblem.NoKey);
                    if (string.IsNullOrWhiteSpace(c.AnthropicModel)) list.Add(EngineProblem.NoModel);
                    break;
                case EngineKind.OpenAiCompatible:
                    if (string.IsNullOrWhiteSpace(c.OpenAiModel)) list.Add(EngineProblem.NoModel);
                    Uri uri;
                    if (!Uri.TryCreate((c.OpenAiUrl ?? "").Trim(), UriKind.Absolute, out uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                        list.Add(EngineProblem.BadUrl);
                    break;
            }
            return list;
        }

        public static ITranslationEngine Create(EngineConfig c, IHttp http)
        {
            if (Problems(c).Count > 0) throw new InvalidOperationException("The engine settings are not complete: " + string.Join(", ", Problems(c).ConvertAll(p => p.ToString()).ToArray()));
            switch (c.Kind)
            {
                case EngineKind.DeepL: return new DeepLEngine(http, c.DeepLKey);
                case EngineKind.Anthropic: return new LlmEngine(http, LlmEngine.Kind.Anthropic, c.AnthropicKey.Trim(), c.AnthropicModel.Trim(), null);
                default: return new LlmEngine(http, LlmEngine.Kind.OpenAiCompatible, (c.OpenAiKey ?? "").Trim(), c.OpenAiModel.Trim(), c.OpenAiUrl.Trim());
            }
        }
    }

    /// <summary>The dictionary and the blacklist are typed as lines of text, so they can be pasted, shared and read as they are.</summary>
    public static class LineLists
    {
        /// <summary>
        /// One <c>word = translation</c> per line. A line starting with # is a comment and is not counted. A line with no equals sign,
        /// or with nothing on one side of it, is ignored and counted in <paramref name="ignored"/>.
        /// </summary>
        public static List<KeyValuePair<string, string>> ParseDictionary(string text, out int ignored)
        {
            ignored = 0;
            var list = new List<KeyValuePair<string, string>>();
            foreach (string raw in (text ?? "").Replace("\r\n", "\n").Split(new[] { '\n' }))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                int eq = line.IndexOf('=');
                string from = eq < 0 ? "" : line.Substring(0, eq).Trim();
                string to = eq < 0 ? "" : line.Substring(eq + 1).Trim();
                if (from.Length == 0 || to.Length == 0) { ignored++; continue; }
                list.Add(new KeyValuePair<string, string>(from, to));
            }
            return list;
        }

        /// <summary>One rule per line, # comments and blank lines left out.</summary>
        public static List<string> ParseRules(string text)
        {
            var list = new List<string>();
            foreach (string raw in (text ?? "").Replace("\r\n", "\n").Split(new[] { '\n' }))
            {
                string line = raw.Trim();
                if (line.Length > 0 && line[0] != '#') list.Add(line);
            }
            return list;
        }
    }
}
