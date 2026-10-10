using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace RimBabel.Core
{
    public sealed class ImportReport
    {
        public int Imported;
        public int Outdated;
        public int Kept;
        public List<string> Orphans = new List<string>();
    }

    /// <summary>
    /// Takes the texts of an existing translation (a Languages/&lt;language&gt; folder, from another pack or
    /// another version of this one) and puts them under the matching entries. What it brings is a person's
    /// work: Human, and never over a text that is already Human, Reviewed or Locked. A text the
    /// file says was written for another source (the "EN:" comment the game's own export puts above
    /// each key) arrives as a Stale draft instead, so it is shown and not trusted. Texts for keys the
    /// source no longer has are reported, not dropped silently.
    /// </summary>
    /// <summary>An installed translation pack: the name shown to the player and the Languages/&lt;language&gt; folders it holds.</summary>
    public sealed class InstalledPack
    {
        public string Label;
        public List<string> Folders = new List<string>();
    }

    public static class Importer
    {
        /// <summary>
        /// Imports several installed packs in turn, the first one winning where two hold the same key (it fills the entry before the
        /// next one is read). A pack that brought at least one text is recorded in <see cref="Manifest.ImportedFrom"/>, which blocks
        /// publication until its licence is checked.
        /// </summary>
        public static ImportReport ImportPacks(Manifest manifest, IEnumerable<InstalledPack> packs)
        {
            var total = new ImportReport();
            foreach (InstalledPack pack in packs)
            {
                int brought = 0;
                foreach (string folder in pack.Folders)
                {
                    ImportReport r = Import(manifest.Entries, folder);
                    brought += r.Imported + r.Outdated;
                    total.Imported += r.Imported;
                    total.Outdated += r.Outdated;
                    total.Kept += r.Kept;
                    total.Orphans.AddRange(r.Orphans);
                }
                if (brought > 0 && !manifest.ImportedFrom.Contains(pack.Label)) manifest.ImportedFrom.Add(pack.Label);
            }
            return total;
        }

        public static ImportReport Import(IEnumerable<Entry> entries, string languageFolder)
        {
            var report = new ImportReport();
            var byId = new Dictionary<string, Entry>();
            foreach (Entry e in entries) byId[e.Id] = e;

            string keyed = Path.Combine(languageFolder, "Keyed");
            if (Directory.Exists(keyed))
                foreach (string file in Files(keyed))
                    ReadFile(file, null, byId, report);

            string injected = Path.Combine(languageFolder, "DefInjected");
            if (Directory.Exists(injected))
                foreach (string dir in Directory.EnumerateDirectories(injected).OrderBy(d => d, StringComparer.Ordinal))
                {
                    string defType = Path.GetFileName(dir);
                    foreach (string file in Files(dir))
                        ReadFile(file, defType, byId, report);
                }
            return report;
        }

        private static IEnumerable<string> Files(string dir)
        {
            return Directory.EnumerateFiles(dir, "*.xml", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal);
        }

        private static void ReadFile(string file, string defType, Dictionary<string, Entry> byId, ImportReport report)
        {
            XDocument doc;
            try { doc = XDocument.Parse(File.ReadAllText(file), LoadOptions.PreserveWhitespace); }
            catch (XmlException) { report.Orphans.Add(file + " (not valid XML)"); return; }
            if (doc.Root == null || doc.Root.Name != "LanguageData") return;

            string pendingSource = null;
            foreach (XNode node in doc.Root.Nodes())
            {
                var comment = node as XComment;
                if (comment != null)
                {
                    string c = comment.Value.Trim();
                    // The game writes "EN: <the source text>" above each key it exports.
                    pendingSource = c.StartsWith("EN:", StringComparison.Ordinal) ? Hashing.Normalize(c.Substring(3).Trim().Replace("\\n", "\n")) : null;
                    continue;
                }
                var el = node as XElement;
                if (el == null || el.HasElements) continue;

                string key = el.Name.LocalName;
                string id = defType == null ? "K:" + key : "D:" + defType + "/" + key;
                string text = Hashing.Normalize(el.Value.Replace("\\n", "\n"));
                string said = pendingSource;
                pendingSource = null;
                if (text.Length == 0 || text == "TODO") continue;   // the game's own placeholder for "not translated"

                Entry entry;
                if (!byId.TryGetValue(id, out entry)) { report.Orphans.Add(id); continue; }
                if (entry.Status == EntryStatus.Human || entry.Status == EntryStatus.Reviewed || entry.Status == EntryStatus.Locked)
                {
                    report.Kept++;
                    continue;
                }

                entry.Target = text;
                entry.Engine = null;
                bool outdated = said != null && Hashing.Of(said) != entry.SourceHash;
                entry.Status = outdated ? EntryStatus.Stale : EntryStatus.Human;
                if (outdated) report.Outdated++; else report.Imported++;
            }
        }
    }
}
