using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RimBabel.Core
{
    /// <summary>One source mod's translation, as the per-mod package holds it: what a combined package is made from.</summary>
    public sealed class CombinedSource
    {
        public PackageSpec Spec;
        public Manifest Manifest;
    }

    /// <summary>
    /// One translation mod for a whole language, instead of one per source mod. Each source mod's texts go in a folder of their own,
    /// <c>Mod/&lt;source packageId&gt;/Languages/&lt;language&gt;</c>, and <c>LoadFolders.xml</c> loads that folder only when the source
    /// mod is active (<c>IfModActive</c>), so the texts of a mod the player does not have are never read and cannot log an error.
    /// The per-mod packages stay the source of truth; this is generated from them and can be rebuilt at any time.
    /// A text is blamed on its source mod for licence and attribution, so the combined package carries every source licence and
    /// every blocker of the packages it is made from.
    /// </summary>
    public static class CombinedPackage
    {
        public const string ListPath = "Mod/Babel/combined.txt";

        public static PackageResult Write(string root, PackageSpec spec, IList<CombinedSource> sources)
        {
            PackageWriter.RequireName(spec.Language, "Language");
            PackageWriter.RequireName(spec.PackageId, "PackageId");
            string mod = Path.Combine(root, "Mod");
            string listFile = Path.Combine(root, ListPath);
            if (!File.Exists(listFile) && Directory.Exists(mod) && Directory.EnumerateFileSystemEntries(mod).Any())
                throw new InvalidOperationException("Refusing to overwrite " + mod + ": it has files but no list of combined sources.");

            // The folders of the previous generation go: a source that is no longer translated must not leave its texts behind.
            if (File.Exists(listFile))
                foreach (string line in File.ReadAllLines(listFile, Encoding.UTF8))
                {
                    string folder = line.Split('\t')[0].Trim();
                    if (folder.Length == 0) continue;
                    PackageWriter.RequireName(folder, "Folder");
                    string old = Path.Combine(mod, folder);
                    if (Directory.Exists(old)) Directory.Delete(old, true);
                }

            var ordered = sources.OrderBy(s => s.Manifest.SourcePackageId, StringComparer.OrdinalIgnoreCase).ToList();
            var list = new StringBuilder();
            var loadFolders = new XElement("v1.6", new XElement("li", "/"));
            foreach (CombinedSource s in ordered)
            {
                string id = s.Manifest.SourcePackageId;
                PackageWriter.RequireName(id, "SourcePackageId");
                string fileName = string.IsNullOrEmpty(s.Spec.FileName) ? "Texts" : s.Spec.FileName;
                PackageWriter.WriteLanguageFolder(Path.Combine(mod, id, "Languages", spec.Language), fileName, s.Manifest.Entries);
                loadFolders.Add(new XElement("li", new XAttribute("IfModActive", id), id));
                list.Append(id).Append('\t').Append(s.Manifest.SourceName).Append('\n');
            }

            PackageWriter.WriteText(Path.Combine(mod, "LoadFolders.xml"), Document(new XElement("loadFolders", loadFolders)));
            PackageWriter.WriteText(listFile, list.ToString());
            PackageWriter.WriteText(Path.Combine(mod, "About", "About.xml"), Document(About(spec, ordered)));
            PackageWriter.WriteText(Path.Combine(root, "ATTRIBUTION.md"), Attribution(spec, ordered));
            if (!string.IsNullOrEmpty(spec.LicenceText))
            {
                PackageWriter.WriteText(Path.Combine(root, "LICENSE"), spec.LicenceText);
                PackageWriter.WriteText(Path.Combine(mod, "LICENSE"), spec.LicenceText);
            }

            var result = new PackageResult { Root = root, Version = "1.0.0" };
            result.PublishBlockers.AddRange(Blockers(root, spec, ordered));
            return result;
        }

        public static List<string> Blockers(string root, PackageSpec spec, IList<CombinedSource> sources)
        {
            var list = new List<string>();
            if (sources.Count == 0) list.Add("The combined package has no source mod.");
            foreach (CombinedSource s in sources)
            {
                string name = s.Manifest.SourceName ?? s.Manifest.SourcePackageId;
                if (string.IsNullOrEmpty(s.Spec.SourceLicenceName))
                    list.Add("Licence of the source mod " + name + " is unknown: a translation is a derivative, so it stays local until it is known.");
                foreach (string pack in s.Manifest.ImportedFrom)
                    list.Add("Texts of " + name + " were imported from the installed translation " + pack + ": its licence must allow redistribution before this package is published.");
            }
            if (string.IsNullOrEmpty(spec.LicenceText))
                list.Add("No licence text for this package.");
            if (!File.Exists(Path.Combine(root, "Mod", "About", "Preview.png")))
                list.Add("Mod/About/Preview.png is missing.");
            if (!File.Exists(Path.Combine(root, "Mod", "About", "ModIcon.png")))
                list.Add("Mod/About/ModIcon.png is missing.");
            return list;
        }

        private static XElement About(PackageSpec s, IList<CombinedSource> sources)
        {
            int total = sources.Sum(x => x.Manifest.Entries.Count);
            int done = sources.Sum(x => x.Manifest.Entries.Count(e => !string.IsNullOrEmpty(e.Target) && e.Status != EntryStatus.Stale));
            return new XElement("ModMetaData",
                new XElement("name", s.Name),
                new XElement("author", s.Author),
                new XElement("packageId", s.PackageId),
                new XElement("supportedVersions", new XElement("li", s.GameVersion)),
                new XElement("loadAfter", sources.Select(x => new XElement("li", x.Manifest.SourcePackageId))),
                new XElement("description",
                    s.Language + " translation of " + sources.Count + " mods: " + string.Join(", ", sources.Select(x => x.Manifest.SourceName).ToArray()) + ".\n\n" +
                    "The texts of each mod load only when that mod is active.\n\n" +
                    done + " of " + total + " texts translated. Texts without a translation show in English.\n\n" +
                    "Made with RimBabel."));
        }

        private static string Attribution(PackageSpec s, IList<CombinedSource> sources)
        {
            var sb = new StringBuilder("# Attribution\n\n" + s.Language + " translation of these mods, one folder each under `Mod/`:\n\n");
            foreach (CombinedSource x in sources)
            {
                sb.Append("- **").Append(x.Manifest.SourceName).Append("** (`").Append(x.Manifest.SourcePackageId).Append("`");
                if (!string.IsNullOrEmpty(x.Manifest.SourceVersion)) sb.Append(", version ").Append(x.Manifest.SourceVersion);
                sb.Append("), licence of the source mod: ").Append(string.IsNullOrEmpty(x.Spec.SourceLicenceName) ? "unknown" : x.Spec.SourceLicenceName).Append(".");
                if (x.Manifest.ImportedFrom.Count > 0) sb.Append(" Some texts imported from installed translations: ").Append(string.Join("; ", x.Manifest.ImportedFrom.ToArray())).Append(".");
                sb.Append('\n');
            }
            sb.Append("\nTexts in this package translate the source mods' texts; the source texts remain their authors' work.\n");
            return sb.ToString();
        }

        private static string Document(XElement root)
        {
            var sb = new StringBuilder();
            using (var w = System.Xml.XmlWriter.Create(sb, new System.Xml.XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
                root.WriteTo(w);
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + sb.ToString().Replace("\r\n", "\n") + "\n";
        }
    }
}
