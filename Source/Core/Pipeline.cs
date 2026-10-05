using System;
using System.Collections.Generic;
using System.Linq;

namespace RimBabel.Core
{
    /// <summary>One way to turn texts into another language. It sees only protected text and a glossary for engines that can use one.</summary>
    public interface ITranslationEngine
    {
        string Name { get; }
        string[] Translate(string[] texts, string fromLanguage, string toLanguage, IList<KeyValuePair<string, string>> glossary);
    }

    public sealed class PipelineReport
    {
        public int Translated;
        public int Blacklisted;
        public int Failed;
        public bool Aborted;
        public List<string> Failures = new List<string>();
    }

    /// <summary>
    /// Brings a manifest's untranslated texts to a translation: Pending and Stale ones, minus the
    /// blacklisted. A text goes through the protector, the engine, the protector's check and, when
    /// it fails that check, one more try alone before it is left as it was. Human, Reviewed and
    /// Locked texts are never sent anywhere. An engine that throws stops the run, because the usual
    /// reasons (a refused key, an exhausted quota) would fail every following text too and burn
    /// what is left; what was already translated is kept.
    /// </summary>
    public static class Pipeline
    {
        public static PipelineReport Run(Manifest manifest, ITranslationEngine engine, string fromLanguage, string toLanguage, int batchSize)
        {
            var report = new PipelineReport();
            if (batchSize < 1) batchSize = 1;

            var todo = new List<Entry>();
            foreach (Entry e in manifest.Entries)
            {
                if (e.Status != EntryStatus.Pending && e.Status != EntryStatus.Stale) continue;
                if (string.IsNullOrWhiteSpace(e.Source)) continue;
                if (Blacklist.Matches(manifest.Blacklist, e)) { report.Blacklisted++; continue; }
                todo.Add(e);
            }

            for (int start = 0; start < todo.Count && !report.Aborted; start += batchSize)
            {
                List<Entry> batch = todo.Skip(start).Take(batchSize).ToList();
                var prot = batch.Select(e => Protector.Protect(e.Source, manifest.Glossary)).ToList();
                string[] answers;
                try { answers = engine.Translate(prot.Select(p => p.Text).ToArray(), fromLanguage, toLanguage, manifest.Glossary); }
                catch (Exception ex)
                {
                    Abort(report, engine, batch, ex);
                    break;
                }
                if (answers == null || answers.Length != batch.Count)
                {
                    // A shifted answer would put texts under the wrong keys: nothing of it is used.
                    report.Failed += batch.Count;
                    report.Failures.Add(engine.Name + " returned " + (answers == null ? "no" : answers.Length.ToString()) + " answers for " + batch.Count + " texts; the batch was dropped");
                    continue;
                }

                for (int i = 0; i < batch.Count; i++)
                {
                    Entry e = batch[i];
                    Restored r = Protector.Restore(answers[i], prot[i], e.Source);
                    // After the run stopped, the answers already received are still used; only the extra requests are not made.
                    if (!r.Ok && !report.Aborted)
                    {
                        // One more try, alone: a batch can fail one text for reasons that a single request does not share.
                        try
                        {
                            string[] again = engine.Translate(new[] { prot[i].Text }, fromLanguage, toLanguage, manifest.Glossary);
                            r = again != null && again.Length == 1 ? Protector.Restore(again[0], prot[i], e.Source) : r;
                        }
                        catch (Exception ex)
                        {
                            Abort(report, engine, new List<Entry> { e }, ex);
                            continue;
                        }
                    }
                    if (r.Ok)
                    {
                        e.Target = r.Text;
                        e.Status = EntryStatus.Machine;
                        e.Engine = engine.Name;
                        report.Translated++;
                    }
                    else
                    {
                        report.Failed++;
                        report.Failures.Add(e.Id + ": " + r.Reason);
                    }
                }
            }
            return report;
        }

        private static void Abort(PipelineReport report, ITranslationEngine engine, List<Entry> batch, Exception ex)
        {
            report.Aborted = true;
            report.Failed += batch.Count;
            report.Failures.Add(engine.Name + " failed and the run stopped: " + ex.Message);
        }
    }
}
