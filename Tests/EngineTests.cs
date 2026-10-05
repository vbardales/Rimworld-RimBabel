using System;
using System.Collections.Generic;
using System.Linq;
using RimBabel.Core;

// Json and the engines, with a stand-in for the network.
internal static partial class Program
{
    private sealed class FakeHttp : IHttp
    {
        public string Url, Body;
        public IDictionary<string, string> Headers;
        public HttpReply Get(string url, IDictionary<string, string> headers, int timeoutMs)
        {
            Calls++; Url = url; Headers = headers; Body = null;
            return Respond(url);
        }
        public int Calls;
        public Func<string, HttpReply> Respond;
        public HttpReply PostJson(string url, IDictionary<string, string> headers, string body, int timeoutMs)
        {
            Calls++; Url = url; Headers = headers; Body = body;
            return Respond(body);
        }
    }

    private static HttpReply Ok(string body) { return new HttpReply { Status = 200, Body = body }; }

    private static void JsonTests()
    {
        string tricky = "quote \" slash \\ line\nreturn\r tab\t control\u0001 café ⟦0⟧";
        object back = Json.Parse(Json.Write(new Dictionary<string, object> { { "k", tricky }, { "n", 3 }, { "f", 1.5 }, { "b", true }, { "z", null }, { "l", new List<object> { "a", new List<object>() } } }));
        var d = (Dictionary<string, object>)back;
        Check((string)d["k"] == tricky, "json: a string with every escape round-trips");
        Check((double)d["n"] == 3 && (double)d["f"] == 1.5 && (bool)d["b"] && d["z"] == null, "json: numbers, booleans and null round-trip");
        Check(((List<object>)d["l"]).Count == 2, "json: nested arrays round-trip");
        Check((string)Json.Parse("\"\\u00e9\\n\"") == "é\n", "json: a \\u escape is read");
        foreach (string bad in new[] { "{", "[1,", "{\"a\" 1}", "\"open", "tru", "[1] x" })
        {
            bool threw = false;
            try { Json.Parse(bad); } catch (FormatException) { threw = true; }
            Check(threw, "json: invalid text is refused: " + bad);
        }
    }

    private static void EngineTests()
    {
        string[] texts = { "Hello ⟦0⟧", "Bye" };

        // Anthropic
        var http = new FakeHttp { Respond = b => Ok(Json.Write(new Dictionary<string, object> { { "content", new List<object> { new Dictionary<string, object> { { "type", "text" }, { "text", "Sure:\n```json\n[\"Bonjour ⟦0⟧\",\"Salut\"]\n```" } } } } })) };
        var claude = new LlmEngine(http, LlmEngine.Kind.Anthropic, "sk-secret", "claude-haiku-4-5-20251001", null);
        string[] r = claude.Translate(texts, "English", "French", NoGlossary);
        Check(r.Length == 2 && r[0] == "Bonjour ⟦0⟧" && r[1] == "Salut", "anthropic: the array is read out of a fenced reply");
        Check(http.Url == "https://api.anthropic.com/v1/messages", "anthropic: the Messages endpoint");
        Check(http.Headers["x-api-key"] == "sk-secret" && http.Headers["anthropic-version"] == "2023-06-01", "anthropic: key and version headers");
        var sent = (Dictionary<string, object>)Json.Parse(http.Body);
        Check((string)sent["model"] == "claude-haiku-4-5-20251001" && ((string)sent["system"]).Contains("RimWorld") && ((string)sent["system"]).Contains("French"), "anthropic: model and a system prompt naming the languages");
        var msg = (Dictionary<string, object>)((List<object>)sent["messages"])[0];
        Check((string)msg["role"] == "user" && ((List<object>)Json.Parse((string)msg["content"])).Count == 2, "anthropic: the texts go in as one JSON array");
        Check(!http.Body.Contains("sk-secret"), "anthropic: the key is never in the body");

        // An OpenAI-compatible endpoint, with and without a key
        var local = new FakeHttp { Respond = b => Ok("{\"choices\":[{\"message\":{\"content\":\"[\\\"Bonjour \\u27E60\\u27E7\\\",\\\"Salut\\\"]\"}}]}") };
        var ollama = new LlmEngine(local, LlmEngine.Kind.OpenAiCompatible, "", "qwen2.5", "http://localhost:11434/v1/");
        string[] r2 = ollama.Translate(texts, "English", "French", NoGlossary);
        Check(r2[1] == "Salut" && local.Url == "http://localhost:11434/v1/chat/completions", "openai-compatible: the chat endpoint under the base url");
        Check(!local.Headers.ContainsKey("Authorization"), "openai-compatible: no key, no Authorization header (a local server asks for none)");
        new LlmEngine(local, LlmEngine.Kind.OpenAiCompatible, "k", "m", null).Translate(texts, "English", "French", NoGlossary);
        Check(local.Headers["Authorization"] == "Bearer k" && local.Url == "https://api.openai.com/v1/chat/completions", "openai-compatible: a bearer key and the default host");

        // Failures name the status and never the key
        var refused = new FakeHttp { Respond = b => new HttpReply { Status = 401, Body = "{\"error\":\"invalid key sk-secret\"}" } };
        string message = null;
        try { new LlmEngine(refused, LlmEngine.Kind.Anthropic, "sk-secret", "m", null).Translate(texts, "English", "French", NoGlossary); }
        catch (EngineException ex) { message = ex.Message; }
        Check(message != null && message.Contains("401") && !message.Contains("sk-secret"), "engine: a refusal says the status and hides the key (" + message + ")");

        foreach (string reply in new[] { "no array here", "[\"only one\"]", "[1,2]", "[\"a\",\"b\"" })
        {
            string content = reply;
            var bad = new FakeHttp { Respond = b => Ok("{\"choices\":[{\"message\":{\"content\":" + Json.Write(content) + "}}]}") };
            bool threw = false;
            try { new LlmEngine(bad, LlmEngine.Kind.OpenAiCompatible, "", "m", null).Translate(texts, "English", "French", NoGlossary); }
            catch (EngineException) { threw = true; }
            Check(threw, "engine: an unusable reply is refused: " + reply);
        }
        bool noModel = false;
        try { new LlmEngine(http, LlmEngine.Kind.Anthropic, "k", " ", null); } catch (ArgumentException) { noModel = true; }
        Check(noModel, "engine: a model name is required");

        // DeepL
        var deepl = new FakeHttp
        {
            Respond = b => Ok(Json.Write(new Dictionary<string, object> { { "translations", new List<object>
            {
                new Dictionary<string, object> { { "text", "Bonjour <x i=\"0\"/> &amp; fin" } },
                new Dictionary<string, object> { { "text", "Salut" } },
            } } }))
        };
        var dl = new DeepLEngine(deepl, "abc:fx");
        string[] r3 = dl.Translate(new[] { "Hello ⟦0⟧ & end", "Bye" }, "English", "French (Français)", NoGlossary);
        Check(deepl.Url == "https://api-free.deepl.com/v2/translate", "deepl: a key ending in :fx goes to the free host");
        Check(deepl.Headers["Authorization"] == "DeepL-Auth-Key abc:fx", "deepl: the key header");
        var dsent = (Dictionary<string, object>)Json.Parse(deepl.Body);
        var dtext = ((List<object>)dsent["text"]).Cast<string>().ToList();
        Check(dtext[0] == "Hello <x i=\"0\"/> &amp; end", "deepl: tokens go as tags and the rest is XML-escaped (" + dtext[0] + ")");
        Check((string)dsent["source_lang"] == "EN" && (string)dsent["target_lang"] == "FR" && (string)dsent["tag_handling"] == "xml", "deepl: language codes and tag handling");
        Check(r3[0] == "Bonjour ⟦0⟧ & fin" && r3[1] == "Salut", "deepl: tags become tokens again and entities are decoded");
        new DeepLEngine(deepl, "paid-key").Translate(new[] { "Hello", "Bye" }, "English", "French", NoGlossary);
        Check(deepl.Url == "https://api.deepl.com/v2/translate", "deepl: any other key goes to the paid host");
        bool unknown = false;
        try { dl.Translate(texts, "English", "Klingon", NoGlossary); } catch (EngineException) { unknown = true; }
        Check(unknown && LanguageCodes.ForDeepL("French (Français)") == "FR" && LanguageCodes.ForDeepL("Klingon") == null, "deepl: a language it has no code for is refused");

        // End to end: manifest, pipeline, engine over the fake network
        Entry e = Entry.Keyed("Greeting", "Hello {0}, welcome home");
        var m = ManifestOf(e);
        m.Glossary.Add(new KeyValuePair<string, string>("home", "maison"));
        var echo = new FakeHttp
        {
            Respond = b =>
            {
                var req = (Dictionary<string, object>)Json.Parse(b);
                var inputs = (List<object>)Json.Parse((string)((Dictionary<string, object>)((List<object>)req["messages"])[1])["content"]);
                // A plausible model: translate the words, keep the tokens.
                var outs = inputs.Cast<string>().Select(s => s.Replace("Hello", "Bonjour").Replace("welcome", "bienvenue")).ToList();
                return Ok("{\"choices\":[{\"message\":{\"content\":" + Json.Write(Json.Write(outs.Cast<object>().ToList())) + "}}]}");
            }
        };
        PipelineReport rep = Pipeline.Run(m, new LlmEngine(echo, LlmEngine.Kind.OpenAiCompatible, "", "m", null), "English", "French", 10);
        Check(rep.Translated == 1 && e.Target == "Bonjour {0}, bienvenue maison" && e.Engine == "openai-compatible:m", "end to end: placeholders, the glossary and the engine name survive the whole chain (" + e.Target + ")");
        Check(!echo.Body.Contains("{0}") && !echo.Body.Contains("home"), "end to end: the service never saw the placeholder or the glossary term");
    }
}
