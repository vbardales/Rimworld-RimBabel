using System;
using System.Collections.Generic;
using System.Linq;
using RimBabel.Core;

// The plain machine-translation engines (Google Cloud, LibreTranslate, MyMemory, Yandex), the engine names, and taking an engine's work back.
internal static partial class Program
{
    private static void MtEngineTests()
    {
        // ---- language codes
        Check(IsoCodes.Of("French (Français)") == "fr" && IsoCodes.Of("ChineseSimplified") == "zh" && IsoCodes.Of("Klingon") == null, "iso: game language folders map to codes");

        // ---- Google Cloud
        var google = new FakeHttp
        {
            Respond = b => Ok("{\"data\":{\"translations\":[{\"translatedText\":\"Bonjour ⟦0⟧ d'un ami\"},{\"translatedText\":\"Salut\"}]}}")
        };
        var g = new GoogleCloudEngine(google, "SECRET-KEY");
        string[] gr = g.Translate(new[] { "Hello ⟦0⟧ of a friend", "Hi" }, "English", "French", NoGlossary);
        Check(google.Url == "https://translation.googleapis.com/language/translate/v2" && !google.Url.Contains("SECRET-KEY"), "google: the key is not in the address");
        Check(google.Headers["X-goog-api-key"] == "SECRET-KEY", "google: the key goes in a header");
        var gsent = (Dictionary<string, object>)Json.Parse(google.Body);
        Check((string)gsent["format"] == "text" && ((List<object>)gsent["q"])[0].ToString() == "Hello ⟦0⟧ of a friend", "google: plain-text mode, the text goes as it is (HTML mode put a space on each side of a wrapped token)");
        Check(gr[0] == "Bonjour ⟦0⟧ d'un ami" && gr[1] == "Salut", "google: the answer is used as it comes (" + gr[0] + ")");
        var gbad = new GoogleCloudEngine(new FakeHttp { Respond = b => new HttpReply { Status = 403, Body = "bad SECRET-KEY" } }, "SECRET-KEY");
        string gmsg = null;
        try { gbad.Translate(new[] { "x" }, "English", "French", NoGlossary); } catch (EngineException ex) { gmsg = ex.Message; }
        Check(gmsg != null && gmsg.Contains("403") && !gmsg.Contains("SECRET-KEY"), "google: a refusal is an error without the key (" + gmsg + ")");

        // ---- LibreTranslate
        var libre = new FakeHttp { Respond = b => Ok("{\"translatedText\":[\"Bonjour\",\"Salut\"]}") };
        var lt = new LibreTranslateEngine(libre, "http://localhost:5000/", "");
        string[] lr = lt.Translate(new[] { "Hello", "Hi" }, "English", "French", NoGlossary);
        var lsent = (Dictionary<string, object>)Json.Parse(libre.Body);
        Check(libre.Url == "http://localhost:5000/translate" && lr[0] == "Bonjour" && lr[1] == "Salut", "libretranslate: the address is the server's and a list comes back as a list");
        Check(!lsent.ContainsKey("api_key") && (string)lsent["source"] == "en" && (string)lsent["target"] == "fr", "libretranslate: no key is sent when none is set");
        new LibreTranslateEngine(libre, "https://libre.example", "K1").Translate(new[] { "Hello", "Hi" }, "English", "Norwegian", NoGlossary);
        var lsent2 = (Dictionary<string, object>)Json.Parse(libre.Body);
        Check((string)lsent2["api_key"] == "K1" && (string)lsent2["target"] == "nb", "libretranslate: the key goes in the body and Norwegian is nb");
        bool old = false;
        try { new LibreTranslateEngine(new FakeHttp { Respond = b => Ok("{\"translatedText\":\"one\"}") }, "http://x", "").Translate(new[] { "a", "b" }, "English", "French", NoGlossary); }
        catch (EngineException) { old = true; }
        Check(old, "libretranslate: a server that cannot translate a list is reported, not mis-assigned");

        // ---- MyMemory
        var seen = new List<string>();
        var memory = new FakeHttp { Respond = u => { seen.Add(u); return Ok("{\"responseData\":{\"translatedText\":\"Bonjour\"},\"responseStatus\":200}"); } };
        var mm = new MyMemoryEngine(memory, "me@example.org");
        string[] mr = mm.Translate(new[] { "Hello world", new string('x', 600) }, "English", "French", NoGlossary);
        Check(seen.Count == 1 && mr[0] == "Bonjour" && mr[1] == null, "mymemory: one request per text, and a text over the limit is answered as nothing (" + seen.Count + ")");
        Check(seen[0].StartsWith("https://api.mymemory.translated.net/get?q=Hello%20world&langpair=en%7Cfr&de=me%40example.org"), "mymemory: the text, the pair and the e-mail are escaped (" + seen[0] + ")");
        var quota = new MyMemoryEngine(new FakeHttp { Respond = u => Ok("{\"responseData\":{\"translatedText\":\"MYMEMORY WARNING: YOU USED ALL AVAILABLE FREE TRANSLATIONS FOR TODAY.\"},\"responseStatus\":200}") }, "");
        bool refused = false;
        try { quota.Translate(new[] { "Hello" }, "English", "French", NoGlossary); } catch (EngineException) { refused = true; }
        Check(refused, "mymemory: the quota warning is an error, not a translation");
        var memory429 = new MyMemoryEngine(new FakeHttp { Respond = u => Ok("{\"responseData\":{\"translatedText\":\"x\"},\"responseStatus\":429}") }, "");
        refused = false;
        try { memory429.Translate(new[] { "Hello" }, "English", "French", NoGlossary); } catch (EngineException) { refused = true; }
        Check(refused, "mymemory: a response status other than 200 is an error");

        // ---- Yandex
        var yandex = new FakeHttp { Respond = b => Ok("{\"translations\":[{\"text\":\"Bonjour\"},{\"text\":\"Salut\"}]}") };
        string[] yr = new YandexEngine(yandex, "YKEY", "folder1").Translate(new[] { "Hello", "Hi" }, "English", "French", NoGlossary);
        var ysent = (Dictionary<string, object>)Json.Parse(yandex.Body);
        Check(yandex.Headers["Authorization"] == "Api-Key YKEY" && (string)ysent["folderId"] == "folder1" && yr[1] == "Salut", "yandex: the key header, the folder and the texts");
        bool noFolder = false;
        try { new YandexEngine(yandex, "k", " "); } catch (ArgumentException) { noFolder = true; }
        Check(noFolder, "yandex: a folder is required");

        // ---- through the protector: a token a service drops is refused, not shipped
        var drop = new FakeHttp { Respond = b => Ok("{\"translatedText\":[\"Bonjour\"]}") };
        Entry entry = Entry.Keyed("K", "Hello {0}");
        var mf = ManifestOf(entry);
        PipelineReport lost = Pipeline.Run(mf, new LibreTranslateEngine(drop, "http://x", ""), "English", "French", 5);
        Check(lost.Translated == 0 && lost.Failed == 1 && entry.Status == EntryStatus.Pending, "a plain service that drops the token is refused and the text stays untranslated");

        // ---- config: the new engines
        var c = new EngineConfig { Kind = EngineKind.GoogleCloud };
        Check(EngineFactory.Problems(c).SequenceEqual(new[] { EngineProblem.NoKey }), "config: Google Cloud needs a key");
        c.GoogleKey = "k";
        Check(EngineFactory.Problems(c).Count == 0 && EngineFactory.Create(c, new FakeHttp()) is GoogleCloudEngine, "config: Google Cloud with a key is complete");
        c = new EngineConfig { Kind = EngineKind.LibreTranslate };
        Check(EngineFactory.Problems(c).Count == 0 && EngineFactory.Create(c, new FakeHttp()) is LibreTranslateEngine, "config: LibreTranslate has a default address and needs no key");
        c.LibreUrl = "libre";
        Check(EngineFactory.Problems(c).SequenceEqual(new[] { EngineProblem.BadUrl }), "config: LibreTranslate refuses an address that is not a web address");
        c = new EngineConfig { Kind = EngineKind.MyMemory };
        Check(EngineFactory.Problems(c).Count == 0 && EngineFactory.Create(c, new FakeHttp()) is MyMemoryEngine, "config: MyMemory needs nothing");
        c = new EngineConfig { Kind = EngineKind.Yandex, YandexKey = "k" };
        Check(EngineFactory.Problems(c).SequenceEqual(new[] { EngineProblem.NoFolder }), "config: Yandex needs a folder as well as a key");

        // ---- engine names say which model wrote a text
        Check(new LlmEngine(new FakeHttp(), LlmEngine.Kind.Anthropic, "k", "claude-x", null).Name == "anthropic:claude-x", "name: an LLM engine names its model");

        // ---- taking an engine's work back
        Entry a = Entry.Keyed("A", "one"); a.Status = EntryStatus.Machine; a.Target = "un"; a.Engine = "deepl";
        Entry b2 = Entry.Keyed("B", "two"); b2.Status = EntryStatus.Machine; b2.Target = "deux"; b2.Engine = "anthropic:claude-haiku";
        Entry c2 = Entry.Keyed("C", "three"); c2.Status = EntryStatus.Machine; c2.Target = "trois"; c2.Engine = "anthropic:claude-opus";
        Entry h = Entry.Keyed("H", "four"); h.Status = EntryStatus.Human; h.Target = "quatre"; h.Engine = "deepl";
        Entry lk = Entry.Keyed("L", "five"); lk.Status = EntryStatus.Locked; lk.Target = "cinq"; lk.Engine = "deepl";
        var all = new[] { a, b2, c2, h, lk };
        Check(Merge.Flush(all, "deepl") == 1 && a.Status == EntryStatus.Pending && a.Target == null && a.Engine == null, "flush: an engine's machine texts go back to pending");
        Check(h.Target == "quatre" && lk.Target == "cinq" && h.Status == EntryStatus.Human && lk.Status == EntryStatus.Locked, "flush: human and locked texts are never touched, whatever engine name they carry");
        Check(Merge.Flush(all, "anthropic:claude-haiku") == 1 && b2.Status == EntryStatus.Pending && c2.Status == EntryStatus.Machine, "flush: one model of an engine can be taken back alone");
        Check(Merge.Flush(all, "anthropic") == 1 && c2.Status == EntryStatus.Pending, "flush: the engine's name alone takes back every model of it");
        Check(Merge.Flush(all, "") == 0 && Merge.Flush(all, "nobody") == 0, "flush: an empty or unknown engine takes nothing back");
    }
}
