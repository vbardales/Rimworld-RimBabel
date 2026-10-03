using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace RimBabel.Core
{
    /// <summary>
    /// Reads the Keyed files of a source mod the way the game does, without the game: every child of
    /// LanguageData is a key, and a backslash followed by n in the value is a line break.
    /// </summary>
    public static class KeyedXml
    {
        public static List<KeyValuePair<string, string>> Parse(string xml)
        {
            var list = new List<KeyValuePair<string, string>>();
            XDocument doc;
            try { doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace); }
            catch (XmlException ex) { throw new InvalidDataException("Keyed file is not valid XML: " + ex.Message, ex); }
            if (doc.Root == null || doc.Root.Name != "LanguageData") return list;
            foreach (XElement el in doc.Root.Elements())
            {
                // The game reads the element's text; a nested element is not a text and is skipped.
                if (el.HasElements) continue;
                list.Add(new KeyValuePair<string, string>(el.Name.LocalName, el.Value.Replace("\\n", "\n")));
            }
            return list;
        }

        /// <summary>
        /// The Keyed entries of a language across the load folders of a mod, given from the highest
        /// priority down. The first folder to define a key wins, as in the game.
        /// </summary>
        public static List<Entry> ReadFolders(IEnumerable<string> loadFolders, string language)
        {
            var seen = new HashSet<string>();
            var result = new List<Entry>();
            foreach (string folder in loadFolders)
            {
                string dir = Path.Combine(folder, "Languages", language, "Keyed");
                if (!Directory.Exists(dir)) continue;
                // Sorted, so the order of the manifest does not depend on the file system.
                foreach (string file in Directory.EnumerateFiles(dir, "*.xml", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
                    foreach (KeyValuePair<string, string> kv in Parse(File.ReadAllText(file)))
                        if (seen.Add(kv.Key) && kv.Value.Length > 0)
                            result.Add(Entry.Keyed(kv.Key, kv.Value));
            }
            return result;
        }
    }
}
