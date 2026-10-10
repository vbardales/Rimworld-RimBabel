using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
                    Restored r = CheckUnchanged(Protector.Restore(answers[i], prot[i], e.Source), prot[i], e.Source, fromLanguage, toLanguage);
                    // After the run stopped, the answers already received are still used; only the extra requests are not made.
                    if (!r.Ok && !report.Aborted)
                    {
                        // One more try, alone: a batch can fail one text for reasons that a single request does not share.
                        try
                        {
                            string[] again = engine.Translate(new[] { prot[i].Text }, fromLanguage, toLanguage, manifest.Glossary);
                            r = again != null && again.Length == 1 ? CheckUnchanged(Protector.Restore(again[0], prot[i], e.Source), prot[i], e.Source, fromLanguage, toLanguage) : r;
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

        /// <summary>
        /// An answer equal to its source, for a text of several words, is the engine handing the source back
        /// (a service that did not detect the language, a model that refused): refused, so the text stays
        /// pending for a later pass instead of being locked in as a "translation". Words are counted outside the protected spans (a source made of placeholders only comes back untouched by design). One word is let through,
        /// since a name or a loanword may be the same in both languages.
        /// </summary>
        private static Restored CheckUnchanged(Restored r, Protected p, string source, string fromLanguage, string toLanguage)
        {
            if (!r.Ok || string.Equals(fromLanguage, toLanguage, StringComparison.OrdinalIgnoreCase)) return r;
            string a = Hashing.Normalize(source).Trim();
            string b = Hashing.Normalize(r.Text).Trim();
            int words = Regex.Replace(p.Text, "⟦\\d+⟧", " ").Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Count(w => w.Any(char.IsLetter));
            if (words < 2 || !string.Equals(a, b, StringComparison.Ordinal)) return r;
            return new Restored { Ok = false, Reason = "the engine returned the text unchanged, still in " + fromLanguage };
        }

        private static void Abort(PipelineReport report, ITranslationEngine engine, List<Entry> batch, Exception ex)
        {
            report.Aborted = true;
            report.Failed += batch.Count;
            report.Failures.Add(engine.Name + " failed and the run stopped: " + ex.Message);
        }
    }
}
