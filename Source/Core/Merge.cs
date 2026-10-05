using System;
using System.Collections.Generic;

namespace RimBabel.Core
{
    public sealed class MergeResult
    {
        public List<Entry> Entries = new List<Entry>();
        public List<Entry> New = new List<Entry>();
        public List<Entry> Changed = new List<Entry>();
        public List<Entry> Removed = new List<Entry>();
        public int Unchanged;

        public bool HasChanges { get { return New.Count + Changed.Count + Removed.Count > 0; } }
    }

    public static class Merge
    {
        /// <summary>
        /// Brings a package up to date with the current source mod, by key and by source hash.
        /// What did not move is kept as it is. A moved source turns a finished text into a Stale draft
        /// (Locked is the exception: the owner said hands off). A key gone from the source is dropped.
        /// </summary>
        public static MergeResult Apply(IEnumerable<Entry> previous, IEnumerable<Entry> current)
        {
            var old = new Dictionary<string, Entry>();
            foreach (Entry e in previous) old[e.Id] = e;

            var result = new MergeResult();
            var seen = new HashSet<string>();
            foreach (Entry cur in current)
            {
                if (!seen.Add(cur.Id)) continue; // a key listed twice keeps its first reading
                Entry prev;
                if (!old.TryGetValue(cur.Id, out prev))
                {
                    Entry n = cur.Clone();
                    n.Target = null; n.Status = EntryStatus.Pending; n.Engine = null;
                    result.Entries.Add(n); result.New.Add(n);
                    continue;
                }

                Entry kept = prev.Clone();
                kept.Source = cur.Source;
                kept.SourceHash = cur.SourceHash;
                if (prev.SourceHash == cur.SourceHash)
                {
                    result.Unchanged++;
                }
                else
                {
                    if (prev.Status != EntryStatus.Locked)
                        kept.Status = prev.Target == null ? EntryStatus.Pending : EntryStatus.Stale;
                    result.Changed.Add(kept);
                }
                result.Entries.Add(kept);
            }
            foreach (KeyValuePair<string, Entry> kv in old)
                if (!seen.Contains(kv.Key)) result.Removed.Add(kv.Value);

            return result;
        }

        /// <summary>No difference: same version. Any difference: next minor.</summary>
        /// <summary>
        /// Takes back what one engine wrote: its machine texts go back to Pending, so the next run sends them again (to another
        /// engine, say, after one proved poor or ran out of credit). The engine is named as the entries record it: "deepl",
        /// or "anthropic" for every model of it, or "anthropic:claude-haiku-4-5-20251001" for one model. Human, Reviewed and
        /// Locked texts are never touched, even when an engine's name is on them. Returns how many were taken back.
        /// </summary>
        public static int Flush(IEnumerable<Entry> entries, string engine)
        {
            if (string.IsNullOrWhiteSpace(engine)) return 0;
            string name = engine.Trim();
            int n = 0;
            foreach (Entry e in entries)
            {
                if (e.Status != EntryStatus.Machine && e.Status != EntryStatus.Stale) continue;
                if (string.IsNullOrEmpty(e.Engine)) continue;
                bool same = string.Equals(e.Engine, name, StringComparison.OrdinalIgnoreCase)
                    || e.Engine.StartsWith(name + ":", StringComparison.OrdinalIgnoreCase);
                if (!same) continue;
                e.Target = null;
                e.Status = EntryStatus.Pending;
                e.Engine = null;
                n++;
            }
            return n;
        }

        public static string NextVersion(string previous, MergeResult diff)
        {
            if (string.IsNullOrEmpty(previous)) return "0.1.0";
            if (!diff.HasChanges) return previous;
            string[] p = previous.Split(new[] { '.' });
            int major, minor;
            if (p.Length != 3 || !int.TryParse(p[0], out major) || !int.TryParse(p[1], out minor)) return previous;
            return major + "." + (minor + 1) + ".0";
        }
    }
}
