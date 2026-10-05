using System;
using System.Collections.Generic;
using System.Linq;

namespace RimBabel.Core
{
    /// <summary>
    /// The codes the plain machine-translation services want, from the name the game gives a language folder.
    /// Chinese and Norwegian are the two languages the services spell differently; each engine adjusts them.
    /// </summary>
    public static class IsoCodes
    {
        private static readonly Dictionary<string, string> Base = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "English", "en" }, { "French", "fr" }, { "German", "de" }, { "Spanish", "es" }, { "Italian", "it" },
            { "Portuguese", "pt" }, { "PortugueseBrazilian", "pt" }, { "Russian", "ru" }, { "Japanese", "ja" }, { "Korean", "ko" },
            { "ChineseSimplified", "zh" }, { "Polish", "pl" }, { "Dutch", "nl" }, { "Turkish", "tr" },
            { "Czech", "cs" }, { "Danish", "da" }, { "Norwegian", "no" }, { "Swedish", "sv" }, { "Finnish", "fi" },
            { "Hungarian", "hu" }, { "Ukrainian", "uk" }, { "Catalan", "ca" }, { "Greek", "el" }, { "Romanian", "ro" },
        };

        /// <summary>The ISO code of a language folder (e.g. "French (Français)" gives fr), or null when the game language is not known here.</summary>
        public static string Of(string folder)
        {
            string name = (folder ?? "").Split(new[] { ' ' })[0];
            string code;
            return Base.TryGetValue(name, out code) ? code : null;
        }
    }

    internal static class MtHelpers
    {
        public static string Code(string service, string language)
        {
            string code = IsoCodes.Of(language);
            if (code == null) throw new EngineException(service + " has no code for the language '" + language + "'");
            return code;
        }

        public static string Fail(string name, HttpReply reply, string secret)
        {
            return name + " answered " + reply.Status + ": " + LlmEngine.Scrub(reply.Body, secret);
        }

        public static T Parse<T>(string name, Func<T> read)
        {
            try { return read(); }
            catch (Exception ex)
            {
                if (ex is EngineException) throw;
                throw new EngineException(name + " answered something that is not the expected format (" + ex.GetType().Name + ")");
            }
        }
    }

    /// <summary>
    /// Google Cloud Translation, basic edition (v2), with the user's own API key. The key goes in a header, never in the address,
    /// so it cannot end up in a log line. Plain-text mode: HTML mode, with the tokens wrapped in a notranslate span, put a space on both sides of every span.
    /// </summary>
    public sealed class GoogleCloudEngine : ITranslationEngine
    {
        private readonly IHttp http;
        private readonly string key;

        public string Name { get { return "google-cloud"; } }

        public GoogleCloudEngine(IHttp http, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A Google Cloud API key is required.");
            this.http = http; this.key = key.Trim();
        }

        public string[] Translate(string[] texts, string fromLanguage, string toLanguage, IList<KeyValuePair<string, string>> glossary)
        {
            string from = MtHelpers.Code(Name, fromLanguage), to = MtHelpers.Code(Name, toLanguage);
            if (from == "zh") from = "zh-CN";
            if (to == "zh") to = "zh-CN";
            var q = texts.Cast<object>().ToList();
            string body = Json.Write(new Dictionary<string, object> { { "q", q }, { "source", from }, { "target", to }, { "format", "text" } });
            var headers = new Dictionary<string, string> { { "X-goog-api-key", key } };
            HttpReply reply = http.PostJson("https://translation.googleapis.com/language/translate/v2", headers, body, 60000);
            if (reply.Status < 200 || reply.Status >= 300) throw new EngineException(MtHelpers.Fail(Name, reply, key));
            return MtHelpers.Parse(Name, () =>
            {
                var data = (Dictionary<string, object>)((Dictionary<string, object>)Json.Parse(reply.Body))["data"];
                var list = (List<object>)data["translations"];
                if (list.Count != texts.Length) throw new EngineException(Name + " returned " + list.Count + " texts for " + texts.Length);
                return list.Select(x => (string)((Dictionary<string, object>)x)["translatedText"]).ToArray();
            });
        }

    }

    /// <summary>
    /// LibreTranslate, the open-source server: one the user runs (docker, pip) or a hosted one. The address is the server's;
    /// a key is only needed by a server that asks for one.
    /// </summary>
    public sealed class LibreTranslateEngine : ITranslationEngine
    {
        private readonly IHttp http;
        private readonly string url, key;

        public string Name { get { return "libretranslate"; } }

        public LibreTranslateEngine(IHttp http, string url, string key)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("A LibreTranslate address is required.");
            this.http = http; this.url = url.Trim().TrimEnd(new[] { '/' }); this.key = (key ?? "").Trim();
        }

        public string[] Translate(string[] texts, string fromLanguage, string toLanguage, IList<KeyValuePair<string, string>> glossary)
        {
            string from = MtHelpers.Code(Name, fromLanguage), to = MtHelpers.Code(Name, toLanguage);
            if (from == "no") from = "nb";
            if (to == "no") to = "nb";
            var body = new Dictionary<string, object> { { "q", texts.Cast<object>().ToList() }, { "source", from }, { "target", to }, { "format", "text" } };
            if (key.Length > 0) body["api_key"] = key;
            HttpReply reply = http.PostJson(url + "/translate", new Dictionary<string, string>(), Json.Write(body), 120000);
            if (reply.Status < 200 || reply.Status >= 300) throw new EngineException(MtHelpers.Fail(Name, reply, key));
            return MtHelpers.Parse(Name, () =>
            {
                var root = (Dictionary<string, object>)Json.Parse(reply.Body);
                object t = root["translatedText"];
                var list = t as List<object>;
                if (list == null)
                {
                    if (texts.Length == 1 && t is string) return new[] { (string)t };
                    throw new EngineException(Name + " answered one text for " + texts.Length + ": the server is too old to translate a list");
                }
                if (list.Count != texts.Length) throw new EngineException(Name + " returned " + list.Count + " texts for " + texts.Length);
                return list.Cast<string>().ToArray();
            });
        }
    }

    /// <summary>
    /// MyMemory: free without a key, a few thousand characters a day (more with an e-mail address). It translates one text per
    /// request and refuses a text over 500 bytes, which is answered as nothing so the protector reports it as not translated.
    /// </summary>
    public sealed class MyMemoryEngine : ITranslationEngine
    {
        private readonly IHttp http;
        private readonly string email;

        public string Name { get { return "mymemory"; } }

        public MyMemoryEngine(IHttp http, string email)
        {
            this.http = http; this.email = (email ?? "").Trim();
        }

        public string[] Translate(string[] texts, string fromLanguage, string toLanguage, IList<KeyValuePair<string, string>> glossary)
        {
            string from = MtHelpers.Code(Name, fromLanguage), to = MtHelpers.Code(Name, toLanguage);
            if (from == "zh") from = "zh-CN";
            if (to == "zh") to = "zh-CN";
            var result = new string[texts.Length];
            for (int i = 0; i < texts.Length; i++)
            {
                if (System.Text.Encoding.UTF8.GetByteCount(texts[i]) > 500) continue;
                string url = "https://api.mymemory.translated.net/get?q=" + Uri.EscapeDataString(texts[i])
                    + "&langpair=" + Uri.EscapeDataString(from + "|" + to) + (email.Length > 0 ? "&de=" + Uri.EscapeDataString(email) : "");
                HttpReply reply = http.Get(url, new Dictionary<string, string>(), 30000);
                if (reply.Status < 200 || reply.Status >= 300) throw new EngineException(MtHelpers.Fail(Name, reply, email));
                result[i] = MtHelpers.Parse(Name, () =>
                {
                    var root = (Dictionary<string, object>)Json.Parse(reply.Body);
                    object status;
                    if (!root.TryGetValue("responseStatus", out status)) status = 200.0;
                    string text = (string)((Dictionary<string, object>)root["responseData"])["translatedText"];
                    if (Convert.ToInt32(status) != 200 || text.StartsWith("MYMEMORY WARNING", StringComparison.OrdinalIgnoreCase))
                        throw new EngineException(Name + " refused: " + LlmEngine.Scrub(text, email));
                    return text;
                });
            }
            return result;
        }
    }

    /// <summary>Yandex Cloud Translate (v2), with an API key and the folder it belongs to.</summary>
    public sealed class YandexEngine : ITranslationEngine
    {
        private readonly IHttp http;
        private readonly string key, folder;

        public string Name { get { return "yandex"; } }

        public YandexEngine(IHttp http, string key, string folder)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A Yandex API key is required.");
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("A Yandex folder id is required.");
            this.http = http; this.key = key.Trim(); this.folder = folder.Trim();
        }

        public string[] Translate(string[] texts, string fromLanguage, string toLanguage, IList<KeyValuePair<string, string>> glossary)
        {
            string from = MtHelpers.Code(Name, fromLanguage), to = MtHelpers.Code(Name, toLanguage);
            string body = Json.Write(new Dictionary<string, object>
            {
                { "folderId", folder }, { "texts", texts.Cast<object>().ToList() },
                { "sourceLanguageCode", from }, { "targetLanguageCode", to }, { "format", "PLAIN_TEXT" },
            });
            var headers = new Dictionary<string, string> { { "Authorization", "Api-Key " + key } };
            HttpReply reply = http.PostJson("https://translate.api.cloud.yandex.net/translate/v2/translate", headers, body, 60000);
            if (reply.Status < 200 || reply.Status >= 300) throw new EngineException(MtHelpers.Fail(Name, reply, key));
            return MtHelpers.Parse(Name, () =>
            {
                var list = (List<object>)((Dictionary<string, object>)Json.Parse(reply.Body))["translations"];
                if (list.Count != texts.Length) throw new EngineException(Name + " returned " + list.Count + " texts for " + texts.Length);
                return list.Select(x => (string)((Dictionary<string, object>)x)["text"]).ToArray();
            });
        }
    }
}
