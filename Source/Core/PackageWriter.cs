using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RimBabel.Core
{
    public sealed class PackageSpec
    {
        /// <summary>This package's own id, e.g. nelim.rimbabel.somemod.fr.</summary>
        public string PackageId;
        public string Name;
        public string Author;
        /// <summary>Language folder name as the game knows it, e.g. French.</summary>
        public string Language;
        /// <summary>Base name of the generated files, e.g. SomeMod.</summary>
        public string FileName;
        public string SourcePackageId;
        public string SourceName;
        public string SourceVersion;
        public string SourceUrl;
        /// <summary>Licence of the source mod, as the author states it, or null when unknown.</summary>
        public string SourceLicenceName;
        /// <summary>Full text of the licence this package ships under, or null when none applies yet.</summary>
        public string LicenceText;
        public string GameVersion = "1.6";
        public string Repo;
    }

    public sealed class PackageResult
    {
        public string Root;
        public string Version;
        public MergeResult Merge;
        /// <summary>Why the package cannot be published as it stands. Empty means nothing is in the way.</summary>
        public List<string> PublishBlockers = new List<string>();
    }

    /// <summary>
    /// Writes a Babel package as a repository ready to commit: Mod/ is what ships, everything around it
    /// is what a mod repository here carries. Running it again on the same folder is an update, not a
    /// rewrite: the manifest decides what moved, and PublishedFileId.txt, icons and anything else in
    /// Mod/ stay where they are.
    /// </summary>
    public static class PackageWriter
    {
        public const string ManifestPath = "Mod/Babel/manifest.xml";

        public static PackageResult Write(string root, PackageSpec spec, IEnumerable<Entry> current, DateTime today)
        {
            RequireName(spec.Language, "Language");
            RequireName(spec.FileName, "FileName");
            string manifestFile = Path.Combine(root, "Mod", "Babel", "manifest.xml");
            string langDir = Path.Combine(root, "Mod", "Languages", spec.Language);

            Manifest manifest;
            if (File.Exists(manifestFile))
            {
                manifest = Manifest.Parse(File.ReadAllText(manifestFile, Encoding.UTF8));
                if (!string.Equals(manifest.Language, spec.Language, StringComparison.Ordinal))
                    throw new InvalidOperationException("This folder holds a " + manifest.Language + " package, not " + spec.Language + ".");
            }
            else
            {
                // Without a manifest the folder is not ours: its language files may be hand work.
                if (Directory.Exists(langDir) && Directory.EnumerateFileSystemEntries(langDir).Any())
                    throw new InvalidOperationException("Refusing to overwrite " + langDir + ": it has files but no Babel manifest. Import it instead.");
                manifest = new Manifest { Language = spec.Language };
            }

            string previousVersion = File.Exists(manifestFile) ? manifest.PackageVersion : null;
            MergeResult merge = Merge.Apply(manifest.Entries, current);
            manifest.Entries = merge.Entries;
            manifest.PackageVersion = previousVersion == null ? "0.1.0" : RimBabel.Core.Merge.NextVersion(previousVersion, merge);
            manifest.SourcePackageId = spec.SourcePackageId;
            manifest.SourceName = spec.SourceName;
            manifest.SourceVersion = spec.SourceVersion;

            Save(root, spec, manifest);
            WriteChangelog(root, manifest, merge, previousVersion == null, today);

            var result = new PackageResult { Root = root, Version = manifest.PackageVersion, Merge = merge };
            result.PublishBlockers.AddRange(Blockers(root, spec, manifest));
            WriteIfMissing(Path.Combine(root, "STATUS.md"), Status(spec, manifest, result.PublishBlockers, today));
            return result;
        }

        /// <summary>
        /// Rebuilds everything generated from an edited manifest, without touching the source mod.
        /// Used after a review pass or an engine run changed entries.
        /// </summary>
        public static void Save(string root, PackageSpec spec, Manifest manifest)
        {
            RequireName(spec.Language, "Language");
            RequireName(spec.FileName, "FileName");
            string langDir = Path.Combine(root, "Mod", "Languages", spec.Language);

            // The Keyed and DefInjected folders are generated: removing them drops files of keys that are gone.
            foreach (string sub in new[] { "Keyed", "DefInjected" })
            {
                string d = Path.Combine(langDir, sub);
                if (Directory.Exists(d)) Directory.Delete(d, true);
            }

            // Pending entries have no text: left out, so the game falls back to the source.
            IEnumerable<Entry> shipped = manifest.Entries.Where(e => !string.IsNullOrEmpty(e.Target));

            var keyed = shipped.Where(e => e.Kind == EntryKind.Keyed).ToList();
            if (keyed.Count > 0)
                WriteLanguageFile(Path.Combine(langDir, "Keyed", spec.FileName + ".xml"), keyed);
            foreach (IGrouping<string, Entry> g in shipped.Where(e => e.Kind == EntryKind.DefInjected).GroupBy(e => e.DefType))
            {
                RequireName(g.Key, "DefType");
                WriteLanguageFile(Path.Combine(langDir, "DefInjected", g.Key, spec.FileName + ".xml"), g.ToList());
            }

            WriteText(Path.Combine(root, "Mod", "Babel", "manifest.xml"), manifest.ToXml());
            WriteText(Path.Combine(root, "Mod", "About", "About.xml"), About(spec, manifest));
            WriteIfMissing(Path.Combine(root, "README.md"), Readme(spec));
            WriteIfMissing(Path.Combine(root, ".gitignore"), GitIgnore);
            if (!string.IsNullOrEmpty(spec.LicenceText))
            {
                WriteIfMissing(Path.Combine(root, "LICENSE"), spec.LicenceText);
                WriteIfMissing(Path.Combine(root, "Mod", "LICENSE"), spec.LicenceText);
            }
            WriteText(Path.Combine(root, "ATTRIBUTION.md"), Attribution(spec, manifest));
        }

        public static List<string> Blockers(string root, PackageSpec spec, Manifest manifest)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(spec.SourceLicenceName))
                list.Add("Licence of the source mod is unknown: a translation is a derivative, so it stays local until it is known.");
            if (string.IsNullOrEmpty(spec.LicenceText))
                list.Add("No licence text for this package.");
            if (!File.Exists(Path.Combine(root, "Mod", "About", "Preview.png")))
                list.Add("Mod/About/Preview.png is missing.");
            if (!File.Exists(Path.Combine(root, "Mod", "About", "ModIcon.png")))
                list.Add("Mod/About/ModIcon.png is missing.");
            if (manifest.Entries.Count == 0)
                list.Add("The package has no entries.");
            return list;
        }

        // ---- language files ----

        internal static void WriteLanguageFile(string path, List<Entry> entries)
        {
            var data = new XElement("LanguageData");
            foreach (Entry e in entries)
            {
                string name = e.Key;
                try { XmlConvert.VerifyName(name); }
                catch (XmlException) { throw new InvalidDataException("Key '" + name + "' cannot be an XML element name; the game could not read it either."); }
                // The game reads a backslash and an n as a line break; a raw line break would be kept as such.
                data.Add(new XElement(name, e.Target.Replace("\r\n", "\n").Replace("\n", "\\n")));
            }
            WriteText(path, new XDocument(new XDeclaration("1.0", "utf-8", null), data).ToStringWithDeclaration());
        }

        private static string ToStringWithDeclaration(this XDocument doc)
        {
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
                doc.Root.WriteTo(w);
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + sb.ToString().Replace("\r\n", "\n") + "\n";
        }

        // ---- repository files ----

        private static string About(PackageSpec s, Manifest m)
        {
            int total = m.Entries.Count;
            int done = m.Entries.Count(e => !string.IsNullOrEmpty(e.Target) && e.Status != EntryStatus.Stale);
            var about = new XElement("ModMetaData",
                new XElement("name", s.Name),
                new XElement("author", s.Author),
                new XElement("packageId", s.PackageId),
                new XElement("supportedVersions", new XElement("li", s.GameVersion)),
                new XElement("loadAfter", new XElement("li", s.SourcePackageId)),
                new XElement("description",
                    s.Language + " translation of " + s.SourceName + ". Needs " + s.SourceName + ".\n\n" +
                    done + " of " + total + " texts translated. Texts without a translation show in English.\n\n" +
                    "Original mod: " + (string.IsNullOrEmpty(s.SourceUrl) ? s.SourceName : s.SourceUrl) + "\n" +
                    "Made with RimBabel."));
            if (!string.IsNullOrEmpty(s.SourceUrl)) about.Add(new XElement("url", s.SourceUrl));
            return new XDocument(new XDeclaration("1.0", "utf-8", null), about).ToStringWithDeclaration();
        }

        private static string Readme(PackageSpec s)
        {
            return "# " + s.Name + "\n\n" +
                s.Language + " translation of " + s.SourceName + ", generated with RimBabel.\n\n" +
                "- `Mod/` is what ships to the Workshop.\n" +
                "- `Mod/Babel/manifest.xml` lists every text, its status and where it came from. " +
                "It is the source of truth: `Mod/Languages/` is generated from it.\n";
        }

        private static string Attribution(PackageSpec s, Manifest m)
        {
            return "# Attribution\n\n" +
                "Translation of **" + s.SourceName + "** (`" + s.SourcePackageId + "`" +
                (string.IsNullOrEmpty(s.SourceVersion) ? "" : ", version " + s.SourceVersion) + ").\n" +
                (string.IsNullOrEmpty(s.SourceUrl) ? "" : "Source: " + s.SourceUrl + "\n") +
                "Licence of the source mod: " + (string.IsNullOrEmpty(s.SourceLicenceName) ? "unknown" : s.SourceLicenceName) + ".\n" +
                "Texts in this package translate the source mod's texts; the source texts remain its author's work.\n";
        }

        private static string Status(PackageSpec s, Manifest m, List<string> blockers, DateTime today)
        {
            var sb = new StringBuilder();
            sb.Append("---\n");
            sb.Append("mod:          ").Append(s.Name).Append('\n');
            sb.Append("packageId:    ").Append(s.PackageId).Append('\n');
            sb.Append("repo:         ").Append(string.IsNullOrEmpty(s.Repo) ? "N/A" : s.Repo).Append('\n');
            sb.Append("visibility:   ").Append(blockers.Count == 0 ? "public" : "private").Append('\n');
            sb.Append("stage:        draft\n");
            sb.Append("source_mod:   ").Append(s.SourcePackageId).Append('\n');
            sb.Append("licence_source: ").Append(string.IsNullOrEmpty(s.SourceLicenceName) ? "unknown" : s.SourceLicenceName).Append('\n');
            sb.Append("remaining:\n");
            if (blockers.Count == 0) sb.Append("  - unverified: first in-game check of the generated texts\n");
            foreach (string b in blockers) sb.Append("  - defect: ").Append(b).Append('\n');
            sb.Append("updated:      ").Append(today.ToString("yyyy-MM-dd")).Append(", generated by RimBabel\n");
            sb.Append("---\n\n# ").Append(s.Name).Append(" - status\n");
            return sb.ToString();
        }

        private static void WriteChangelog(string root, Manifest m, MergeResult merge, bool first, DateTime today)
        {
            string path = Path.Combine(root, "CHANGELOG.md");
            if (!first && !merge.HasChanges) return;
            string existing = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : "# Changelog\n";
            int split = existing.IndexOf('\n');
            string head = split < 0 ? existing : existing.Substring(0, split);
            string rest = split < 0 ? "" : existing.Substring(split + 1);
            string line = first
                ? "- First generation: " + merge.New.Count + " texts."
                : "- " + merge.New.Count + " new, " + merge.Changed.Count + " changed in the source, " + merge.Removed.Count + " removed.";
            string older = rest.Trim().Length == 0 ? "" : "\n" + rest.TrimStart('\n');
            WriteText(path, head + "\n\n## " + m.PackageVersion + " - " + today.ToString("yyyy-MM-dd") + "\n\n" + line + "\n" + older);
        }

        private const string GitIgnore =
            "# Build intermediates and tooling\n.build/\n.claude/\n\n# Run evidence stays on disk only\ndocs/runs/evidence/\n\n# System\nThumbs.db\ndesktop.ini\n.DS_Store\n*.dds\n";

        // ---- helpers ----

        private static void RequireName(string value, string what)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Contains("..") || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException(what + " must be a plain name, got '" + value + "'.");
        }

        private static void WriteText(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        private static void WriteIfMissing(string path, string text)
        {
            if (!File.Exists(path)) WriteText(path, text);
        }
    }
}
