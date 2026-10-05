using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace RimBabel.Core
{
    public sealed class HttpReply
    {
        public int Status;
        public string Body;
    }

    /// <summary>The only thing the engines need from the network, so tests can stand in for it.</summary>
    public interface IHttp
    {
        HttpReply PostJson(string url, IDictionary<string, string> headers, string body, int timeoutMs);
        HttpReply Get(string url, IDictionary<string, string> headers, int timeoutMs);
    }

    /// <summary>A service refused or failed. The message never holds the key.</summary>
    public sealed class EngineException : Exception
    {
        public EngineException(string message) : base(message) { }
    }

    public sealed class WebHttp : IHttp
    {
        public HttpReply PostJson(string url, IDictionary<string, string> headers, string body, int timeoutMs)
        {
            return Send("POST", url, headers, body, timeoutMs);
        }

        public HttpReply Get(string url, IDictionary<string, string> headers, int timeoutMs)
        {
            return Send("GET", url, headers, null, timeoutMs);
        }

        private static HttpReply Send(string method, string url, IDictionary<string, string> headers, string body, int timeoutMs)
        {
            // TLS 1.2 is not on by default in every Mono the game may run on.
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = method;
            req.Timeout = timeoutMs;
            req.ReadWriteTimeout = timeoutMs;
            foreach (KeyValuePair<string, string> h in headers) req.Headers[h.Key] = h.Value;
            try
            {
                if (body != null)
                {
                    req.ContentType = "application/json";
                    byte[] data = new UTF8Encoding(false).GetBytes(body);
                    req.ContentLength = data.Length;
                    using (Stream s = req.GetRequestStream()) s.Write(data, 0, data.Length);
                }
                using (var resp = (HttpWebResponse)req.GetResponse())
                    return new HttpReply { Status = (int)resp.StatusCode, Body = Read(resp) };
            }
            catch (WebException ex)
            {
                var resp = ex.Response as HttpWebResponse;
                if (resp == null) throw new EngineException("the service could not be reached: " + ex.Message);
                using (resp) return new HttpReply { Status = (int)resp.StatusCode, Body = Read(resp) };
            }
        }

        private static string Read(WebResponse resp)
        {
            using (var r = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) return r.ReadToEnd();
        }
    }
    /// <summary>The names the game gives a language folder, as the codes the services want.</summary>
    public static class LanguageCodes
    {
        private static readonly Dictionary<string, string> Deepl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "English", "EN" }, { "French", "FR" }, { "German", "DE" }, { "Spanish", "ES" }, { "Italian", "IT" },
            { "Portuguese", "PT-BR" }, { "Russian", "RU" }, { "Japanese", "JA" }, { "Korean", "KO" },
            { "ChineseSimplified", "ZH" }, { "Polish", "PL" }, { "Dutch", "NL" }, { "Turkish", "TR" },
            { "Czech", "CS" }, { "Danish", "DA" }, { "Norwegian", "NB" }, { "Swedish", "SV" }, { "Finnish", "FI" },
            { "Hungarian", "HU" }, { "Ukrainian", "UK" },
        };

        /// <summary>The service's code for a game language folder (e.g. "French (Français)" gives FR), or null when unknown.</summary>
        public static string ForDeepL(string folder)
        {
            string name = (folder ?? "").Split(new[] { ' ' })[0];
            string code;
            return Deepl.TryGetValue(name, out code) ? code : null;
        }
    }

    /// <summary>
    /// Asks a language model for the translations, in one request per batch, as a JSON array it must give back
    /// in the same length and order. Works with Anthropic's Messages API and with anything that speaks the OpenAI
    /// chat format (Ollama, LM Studio, OpenRouter, Gemini's compatible endpoint).
    /// </summary>
    public sealed class LlmEngine : ITranslationEngine
    {
        public enum Kind { Anthropic, OpenAiCompatible }

        private readonly IHttp http;
        private readonly Kind kind;
        private readonly string key, model, baseUrl;

        public string Name { get { return (kind == Kind.Anthropic ? "anthropic" : "openai-compatible") + ":" + model; } }

        public LlmEngine(IHttp http, Kind kind, string key, string model, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("A model name is required.");
            this.http = http; this.kind = kind; this.key = key ?? ""; this.model = model;
            this.baseUrl = string.IsNullOrWhiteSpace(baseUrl)
                ? (kind == Kind.Anthropic ? "https://api.anthropic.com" : "https://api.openai.com/v1")
                : baseUrl.TrimEnd(new[] { '/' });
        }

        public string[] Translate(string[] texts, string fromLanguage, string toLanguage, IList<KeyValuePair<string, string>> glossary)
        {
            string system = SystemPrompt(fromLanguage, toLanguage);
            string user = Json.Write(texts.Cast<object>().ToList());
            string url; var headers = new Dictionary<string, string>(); string body;

            if (kind == Kind.Anthropic)
            {
                url = baseUrl + "/v1/messages";
                headers["x-api-key"] = key;
                headers["anthropic-version"] = "2023-06-01";
                body = Json.Write(new Dictionary<string, object>
                {
                    { "model", model }, { "max_tokens", 4096 }, { "temperature", 0.0 }, { "system", system },
                    { "messages", new List<object> { new Dictionary<string, object> { { "role", "user" }, { "content", user } } } },
                });
            }
            else
            {
                url = baseUrl + "/chat/completions";
                if (key.Length > 0) headers["Authorization"] = "Bearer " + key;
                body = Json.Write(new Dictionary<string, object>
                {
                    { "model", model }, { "temperature", 0.0 },
                    { "messages", new List<object>
                        {
                            new Dictionary<string, object> { { "role", "system" }, { "content", system } },
                            new Dictionary<string, object> { { "role", "user" }, { "content", user } },
                        } },
                });
            }

            HttpReply reply = http.PostJson(url, headers, body, 120000);
            if (reply.Status < 200 || reply.Status >= 300)
                throw new EngineException(Name + " answered " + reply.Status + ": " + Scrub(reply.Body, key));

            string content = Content(reply.Body);
            return ParseArray(content, texts.Length);
        }

        internal static string SystemPrompt(string from, string to)
        {
            return "You translate texts of a video game (RimWorld, a colony simulation) from " + from + " to " + to + ". "
                + "The input is a JSON array of strings. Answer with ONLY a JSON array of strings: the same number of items, in the same order, nothing else. "
                + "Items may hold tokens that look like \u27E60\u27E7, \u27E61\u27E7. Keep every token exactly as written, exactly once, and never translate, delete or invent one. "
                + "Keep line breaks. Keep the register short and plain, as in a game interface. Do not explain anything.";
        }

        private string Content(string body)
        {
            try
            {
                var root = (Dictionary<string, object>)Json.Parse(body);
                if (kind == Kind.Anthropic)
                {
                    var parts = (List<object>)root["content"];
                    var sb = new StringBuilder();
                    foreach (object p in parts)
                    {
                        var d = p as Dictionary<string, object>;
                        object t;
                        if (d != null && d.TryGetValue("text", out t)) sb.Append((string)t);
                    }
                    return sb.ToString();
                }
                var choices = (List<object>)root["choices"];
                var message = (Dictionary<string, object>)((Dictionary<string, object>)choices[0])["message"];
                return (string)message["content"];
            }
            catch (Exception ex)
            {
                if (ex is EngineException) throw;
                throw new EngineException(Name + " answered something that is not the expected format (" + ex.GetType().Name + ")");
            }
        }

        /// <summary>The model's reply as the array of translations, tolerating a code fence or a sentence around it.</summary>
        internal static string[] ParseArray(string content, int expected)
        {
            string c = content ?? "";
            int a = c.IndexOf('['), b = c.LastIndexOf(']');
            if (a < 0 || b <= a) throw new EngineException("the reply holds no JSON array");
            object v;
            try { v = Json.Parse(c.Substring(a, b - a + 1)); }
            catch (FormatException) { throw new EngineException("the reply holds a JSON array that cannot be read"); }
            var list = v as List<object>;
            if (list == null || list.Any(x => !(x is string))) throw new EngineException("the reply is not an array of strings");
            if (list.Count != expected) throw new EngineException("the reply holds " + list.Count + " items for " + expected + " texts");
            return list.Cast<string>().ToArray();
        }

        internal static string Scrub(string text, string key)
        {
            string t = (text ?? "").Trim();
            if (key != null && key.Length > 0) t = t.Replace(key, "[key]");
            return t.Length > 300 ? t.Substring(0, 300) + "..." : t;
        }
    }

    /// <summary>
    /// DeepL. Placeholder tokens are sent as self-closing XML tags with tag handling on, which is how DeepL is told to
    /// leave something alone, and turned back into tokens on the way in. A free key (ending in :fx) goes to the free host.
    /// </summary>
    public sealed class DeepLEngine : ITranslationEngine
    {
        private static readonly Regex Token = new Regex("\u27E6(\\d+)\u27E7", RegexOptions.Compiled);
        private static readonly Regex Tag = new Regex("<x i=\"(\\d+)\"\\s*/>", RegexOptions.Compiled);

        private readonly IHttp http;
        private readonly string key;

        public string Name { get { return "deepl"; } }

        public DeepLEngine(IHttp http, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A DeepL key is required.");
            this.http = http; this.key = key.Trim();
        }

        public string[] Translate(string[] texts, string fromLanguage, string toLanguage, IList<KeyValuePair<string, string>> glossary)
        {
            string from = LanguageCodes.ForDeepL(fromLanguage), to = LanguageCodes.ForDeepL(toLanguage);
            if (from == null) throw new EngineException("DeepL has no code for the language '" + fromLanguage + "'");
            if (to == null) throw new EngineException("DeepL has no code for the language '" + toLanguage + "'");

            // The text is XML for DeepL: escape what XML would read, then put the tokens in as tags.
            var sent = texts.Select(t => Token.Replace(Escape(t), m => "<x i=\"" + m.Groups[1].Value + "\"/>")).Cast<object>().ToList();
            string host = key.EndsWith(":fx", StringComparison.Ordinal) ? "https://api-free.deepl.com" : "https://api.deepl.com";
            string body = Json.Write(new Dictionary<string, object>
            {
                { "text", sent }, { "source_lang", from }, { "target_lang", to },
                { "tag_handling", "xml" }, { "preserve_formatting", true },
            });
            var headers = new Dictionary<string, string> { { "Authorization", "DeepL-Auth-Key " + key } };

            HttpReply reply = http.PostJson(host + "/v2/translate", headers, body, 60000);
            if (reply.Status < 200 || reply.Status >= 300)
                throw new EngineException("deepl answered " + reply.Status + ": " + LlmEngine.Scrub(reply.Body, key));

            try
            {
                var root = (Dictionary<string, object>)Json.Parse(reply.Body);
                var list = (List<object>)root["translations"];
                if (list.Count != texts.Length) throw new EngineException("deepl returned " + list.Count + " texts for " + texts.Length);
                return list.Select(x => Unescape(Tag.Replace((string)((Dictionary<string, object>)x)["text"], m => "\u27E6" + m.Groups[1].Value + "\u27E7"))).ToArray();
            }
            catch (Exception ex)
            {
                if (ex is EngineException) throw;
                throw new EngineException("deepl answered something that is not the expected format (" + ex.GetType().Name + ")");
            }
        }

        private static string Escape(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string Unescape(string s)
        {
            return s.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }
    }
}
