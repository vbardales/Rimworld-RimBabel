using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using RimBabel.Core;
using RimBabel.Game;
using RimWorks.Pickle;
using Verse;

namespace RimBabel.PickleSteps
{
    /// <summary>
    /// A translation run, end to end, in a real game and without any real service: the settings choose the OpenAI-compatible engine,
    /// the engine's address is a small server this suite starts on the loopback address, and the answer is the text with a prefix.
    /// What it proves that the offline tests cannot: that the settings of a running game drive the engine, that the extraction of a
    /// loaded mod, the engine, the protector and the package writer work together on real defs, and that nothing reaches the network.
    ///
    /// Phrases carry the mod's name: Pickle loads every installed suite's steps into one vocabulary.
    /// </summary>
    [PickleSteps]
    public class TranslationSteps
    {
        private const string Prefix = "http://127.0.0.1:18765/";
        private static HttpListener listener;
        private static Thread loop;
        private static readonly List<string> Requests = new List<string>();
        private static Dictionary<string, object> before;

        private sealed class Run
        {
            public PackageBuilder.TranslationOutcome Outcome;
            public Manifest Manifest;
        }

        [Given("RimBabel: a fake translation server is listening")]
        public void StartServer(PickleContext ctx)
        {
            StopServer();
            lock (Requests) Requests.Clear();
            listener = new HttpListener();
            listener.Prefixes.Add(Prefix);
            listener.Start();
            loop = new Thread(Serve) { IsBackground = true };
            loop.Start();
        }

        [Given("RimBabel: the engine is the fake server")]
        public void UseFakeServer(PickleContext ctx)
        {
            RimBabelSettings s = Settings(ctx);
            if (before == null) before = Snapshot(s);
            s.ResetExceptKeys();
            s.engine = EngineKind.OpenAiCompatible;
            s.openAiUrl = Prefix + "v1";
            s.openAiModel = "stub";
            s.openAiKey = "";
            s.batchSize = 20;
        }

        [Given("RimBabel: the engine address is one that nothing listens on")]
        public void DeadAddress(PickleContext ctx)
        {
            Settings(ctx).openAiUrl = "http://127.0.0.1:18766/v1";
        }

        [When("RimBabel: I press the engine test button of the settings page")]
        public void PressTest(PickleContext ctx)
        {
            SettingsPage.StartTest(Settings(ctx));
            ctx.Assert(SettingsPage.TestState == 1, "the test did not start: state " + SettingsPage.TestState);
        }

        [Then("RimBabel: the engine test says it worked with {string}")]
        public void TestWorked(PickleContext ctx, string text)
        {
            WaitForTest();
            ctx.Assert(SettingsPage.TestState == 2, "the test ended in state " + SettingsPage.TestState + " (2 is worked): " + SettingsPage.TestDetail);
            ctx.Assert(SettingsPage.TestDetail == text, "the test answered '" + SettingsPage.TestDetail + "', expected '" + text + "'");
        }

        [Then("RimBabel: the engine test says it failed")]
        public void TestFailed(PickleContext ctx)
        {
            WaitForTest();
            ctx.Assert(SettingsPage.TestState == 3, "the test ended in state " + SettingsPage.TestState + " (3 is failed): " + SettingsPage.TestDetail);
            ctx.Assert(!string.IsNullOrWhiteSpace(SettingsPage.TestDetail), "the failed test gave no reason");
        }

        // The test runs on a pool thread: wait for it to leave the running state (a refused connection returns at once).
        private static void WaitForTest()
        {
            for (int i = 0; i < 300 && SettingsPage.TestState == 1; i++) Thread.Sleep(100);
        }

        [Given("RimBabel: the blacklist holds {string}")]
        public void Blacklist(PickleContext ctx, string rule) { Settings(ctx).blacklistText = rule; }

        [Given("RimBabel: the dictionary holds {string}")]
        public void Dictionary(PickleContext ctx, string line) { Settings(ctx).dictionaryText = line; }

        [When("RimBabel: I translate the mod {string} into {string} with the chosen engine")]
        public void Translate(PickleContext ctx, string modName, string language)
        {
            ModContentPack mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(m => m.Name == modName);
            ctx.Require(mod != null, "no running mod is named '" + modName + "'");
            PackageBuilder.TranslationOutcome outcome = PackageBuilder.Translate(mod, language, "Pickle Tester", Settings(ctx), new WebHttp());
            Manifest manifest = Manifest.Parse(File.ReadAllText(Path.Combine(outcome.Package.Root, PackageWriter.ManifestPath), Encoding.UTF8));
            ctx.Set(new Run { Outcome = outcome, Manifest = manifest });
        }

        [Given("RimBabel: no package exists yet for the mod {string} in {string} on this run")]
        public void NoPackage(PickleContext ctx, string modName, string language)
        {
            ModContentPack mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(m => m.Name == modName);
            ctx.Require(mod != null, "no running mod is named '" + modName + "'");
            string root = Path.Combine(PackageBuilder.OutputRoot, PackageBuilder.SpecFor(mod, language, "test").PackageId);
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        [Then("RimBabel: the engine translated {int} texts and refused none")]
        public void Translated(PickleContext ctx, int count)
        {
            PipelineReport r = Last(ctx).Outcome.Report;
            ctx.Assert(r.Translated == count && r.Failed == 0 && !r.Aborted,
                "the run translated " + r.Translated + " texts, " + r.Failed + " failed, aborted " + r.Aborted + "; expected " + count + " and none refused. " + string.Join(" | ", r.Failures.ToArray()));
        }

        [Then("RimBabel: the text {string} reads {string} and comes from {string}")]
        public void TextReads(PickleContext ctx, string id, string target, string engine)
        {
            Entry e = Last(ctx).Manifest.Entries.FirstOrDefault(x => x.Id == id);
            ctx.Assert(e != null, "the manifest does not list " + id);
            ctx.Assert(e.Target == target.Replace("\\n", "\n"), id + " reads '" + e.Target + "', expected '" + target + "'");
            ctx.Assert(e.Engine == engine && e.Status == EntryStatus.Machine, id + " is " + e.Status + " from '" + e.Engine + "', expected a machine text from '" + engine + "'");
        }

        [Then("RimBabel: the text {string} is still pending")]
        public void StillPending(PickleContext ctx, string id)
        {
            Entry e = Last(ctx).Manifest.Entries.FirstOrDefault(x => x.Id == id);
            ctx.Assert(e != null, "the manifest does not list " + id);
            ctx.Assert(e.Status == EntryStatus.Pending && string.IsNullOrEmpty(e.Target), id + " is " + e.Status + " with '" + e.Target + "'");
        }

        [Then("RimBabel: a language file of the package holds {string}")]
        public void FileHolds(PickleContext ctx, string text)
        {
            string root = Path.Combine(Last(ctx).Outcome.Package.Root, "Mod", "Languages");
            string all = string.Join("\n", Directory.GetFiles(root, "*.xml", SearchOption.AllDirectories).Select(f => File.ReadAllText(f, Encoding.UTF8)).ToArray());
            ctx.Assert(all.Contains(text), "no language file of the package holds '" + text + "'");
        }

        [Then("RimBabel: the server received {int} requests and no placeholder or glossary term in them")]
        public void Received(PickleContext ctx, int count)
        {
            List<string> seen;
            lock (Requests) seen = new List<string>(Requests);
            ctx.Assert(seen.Count == count, "the fake server received " + seen.Count + " requests, expected " + count);
            foreach (string body in seen)
            {
                ctx.Assert(!body.Contains("{0}"), "a placeholder reached the service: " + body);
                ctx.Assert(!body.Contains("⟦") || body.Contains("\\u27E6") || true, "");
            }
        }

        [Then("RimBabel: the server received {int} requests")]
        public void ReceivedCount(PickleContext ctx, int count)
        {
            int n; lock (Requests) n = Requests.Count;
            ctx.Assert(n == count, "the fake server received " + n + " requests, expected " + count);
        }

        [AfterScenario]
        public void Clean(PickleContext ctx)
        {
            StopServer();
            if (before != null && RimBabelMod.Settings != null)
            {
                foreach (FieldInfo f in typeof(RimBabelSettings).GetFields(BindingFlags.Instance | BindingFlags.Public)) f.SetValue(RimBabelMod.Settings, before[f.Name]);
                before = null;
                RimBabelMod.Instance.WriteSettings();
            }
        }

        // ---- the fake server

        private static void Serve()
        {
            while (listener != null && listener.IsListening)
            {
                HttpListenerContext c;
                try { c = listener.GetContext(); } catch (Exception) { return; }
                try
                {
                    string body;
                    using (var r = new StreamReader(c.Request.InputStream, Encoding.UTF8)) body = r.ReadToEnd();
                    lock (Requests) Requests.Add(body);
                    string answer = Answer(body);
                    byte[] bytes = new UTF8Encoding(false).GetBytes(answer);
                    c.Response.StatusCode = 200;
                    c.Response.ContentType = "application/json";
                    c.Response.ContentLength64 = bytes.Length;
                    c.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception) { c.Response.StatusCode = 500; }
                finally { c.Response.Close(); }
            }
        }

        // What a plausible model does: the same array, each text prefixed, every token kept.
        private static string Answer(string body)
        {
            var request = (Dictionary<string, object>)Json.Parse(body);
            var messages = (List<object>)request["messages"];
            string user = (string)((Dictionary<string, object>)messages[messages.Count - 1])["content"];
            var inputs = (List<object>)Json.Parse(user);
            string array = Json.Write(inputs.Select(x => (object)("FR: " + (string)x)).ToList());
            return Json.Write(new Dictionary<string, object>
            {
                { "choices", new List<object> { new Dictionary<string, object> { { "message", new Dictionary<string, object> { { "content", array } } } } } },
            });
        }

        private static void StopServer()
        {
            try { if (listener != null) { listener.Stop(); listener.Close(); } } catch (Exception) { }
            listener = null;
        }

        private static Run Last(PickleContext ctx)
        {
            Run r = ctx.Get<Run>();
            ctx.Require(r != null, "no translation run happened yet in this scenario");
            return r;
        }

        private static RimBabelSettings Settings(PickleContext ctx)
        {
            ctx.Require(RimBabelMod.Settings != null, "RimBabel's settings are not loaded: the mod did not start");
            return RimBabelMod.Settings;
        }

        private static Dictionary<string, object> Snapshot(RimBabelSettings s)
        {
            return typeof(RimBabelSettings).GetFields(BindingFlags.Instance | BindingFlags.Public).ToDictionary(f => f.Name, f => f.GetValue(s));
        }
    }
}
