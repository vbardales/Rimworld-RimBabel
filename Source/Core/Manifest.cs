using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace RimBabel.Core
{
    /// <summary>
    /// The one source of truth of a Babel package: what each key says in the source mod, what it says
    /// here, who wrote it. The Languages folder is generated from it and can be rebuilt at any time.
    /// Plain XML data, no code: the package works the same with RimBabel absent.
    /// </summary>
    public sealed class Manifest
    {
        public const int SchemaVersion = 1;

        public string PackageVersion = "0.1.0";
        public string Language;
        public string SourcePackageId;
        public string SourceName;
        public string SourceVersion;
        public List<Entry> Entries = new List<Entry>();
        /// <summary>Source term to required translation, applied by every engine.</summary>
        public List<KeyValuePair<string, string>> Glossary = new List<KeyValuePair<string, string>>();
        /// <summary>Entry ids or patterns never sent to an engine.</summary>
        public List<string> Blacklist = new List<string>();

        public string ToXml()
        {
            var root = new XElement("BabelManifest", new XAttribute("schema", SchemaVersion));
            root.Add(new XElement("package", new XAttribute("version", PackageVersion ?? ""), new XAttribute("language", Language ?? "")));
            root.Add(new XElement("source",
                new XAttribute("packageId", SourcePackageId ?? ""),
                new XAttribute("name", SourceName ?? ""),
                new XAttribute("version", SourceVersion ?? "")));
            root.Add(new XElement("glossary", Glossary.Select(g =>
                new XElement("term", new XAttribute("from", g.Key), new XAttribute("to", g.Value)))));
            root.Add(new XElement("blacklist", Blacklist.Select(b => new XElement("li", b))));
            root.Add(new XElement("entries", Entries.Select(ToElement)));

            using (var sw = new Utf8StringWriter())
            {
                new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(sw, SaveOptions.None);
                return sw.ToString();
            }
        }

        private static XElement ToElement(Entry e)
        {
            var el = new XElement("entry",
                new XAttribute("kind", e.Kind.ToString()),
                new XAttribute("key", e.Key),
                new XAttribute("status", e.Status.ToString()),
                new XAttribute("hash", e.SourceHash ?? ""));
            if (e.Kind == EntryKind.DefInjected) el.SetAttributeValue("defType", e.DefType);
            if (!string.IsNullOrEmpty(e.Engine)) el.SetAttributeValue("engine", e.Engine);
            el.Add(new XElement("source", e.Source ?? ""));
            if (e.Target != null) el.Add(new XElement("target", e.Target));
            return el;
        }

        public static Manifest Parse(string xml)
        {
            XDocument doc;
            try { doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace); }
            catch (XmlException ex) { throw new InvalidDataException("Babel manifest is not valid XML: " + ex.Message, ex); }

            XElement root = doc.Root;
            if (root == null || root.Name != "BabelManifest")
                throw new InvalidDataException("Not a Babel manifest.");
            int schema;
            if (!int.TryParse((string)root.Attribute("schema"), out schema))
                throw new InvalidDataException("Babel manifest has no schema number.");
            if (schema > SchemaVersion)
                throw new InvalidDataException("Manifest schema " + schema + " is newer than this RimBabel understands (" + SchemaVersion + "). Update RimBabel.");

            var m = new Manifest();
            XElement pkg = root.Element("package");
            if (pkg != null) { m.PackageVersion = (string)pkg.Attribute("version"); m.Language = (string)pkg.Attribute("language"); }
            XElement src = root.Element("source");
            if (src != null)
            {
                m.SourcePackageId = (string)src.Attribute("packageId");
                m.SourceName = (string)src.Attribute("name");
                m.SourceVersion = (string)src.Attribute("version");
            }
            XElement gl = root.Element("glossary");
            if (gl != null)
                foreach (XElement t in gl.Elements("term"))
                    m.Glossary.Add(new KeyValuePair<string, string>((string)t.Attribute("from"), (string)t.Attribute("to")));
            XElement bl = root.Element("blacklist");
            if (bl != null)
                foreach (XElement li in bl.Elements("li")) m.Blacklist.Add(li.Value);
            XElement entries = root.Element("entries");
            if (entries != null)
                foreach (XElement el in entries.Elements("entry"))
                {
                    var e = new Entry
                    {
                        Kind = (EntryKind)Enum.Parse(typeof(EntryKind), (string)el.Attribute("kind")),
                        DefType = (string)el.Attribute("defType"),
                        Key = (string)el.Attribute("key"),
                        SourceHash = (string)el.Attribute("hash"),
                        Engine = (string)el.Attribute("engine"),
                        Status = (EntryStatus)Enum.Parse(typeof(EntryStatus), (string)el.Attribute("status")),
                        Source = Hashing.Normalize((string)el.Element("source")),
                    };
                    XElement tgt = el.Element("target");
                    e.Target = tgt == null ? null : Hashing.Normalize(tgt.Value);
                    m.Entries.Add(e);
                }
            return m;
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override System.Text.Encoding Encoding { get { return new System.Text.UTF8Encoding(false); } }
        }
    }
}
