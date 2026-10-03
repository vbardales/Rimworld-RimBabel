using System;
using System.Collections.Generic;
using System.Reflection;
using RimBabel.Core;
using Verse;

namespace RimBabel.Game
{
    /// <summary>
    /// Lists the texts of a loaded mod in its source language: the Keyed files of its load folders, and
    /// every string a Def of the mod could have injected. The Def walk is the game's own
    /// (DefInjectionUtility, the one behind the developer "write translation files" action), so the
    /// paths are exactly the ones a DefInjected file must use.
    /// </summary>
    public static class SourceScanner
    {
        public const string SourceLanguage = "English";

        public static List<Entry> Scan(ModContentPack mod)
        {
            var entries = new List<Entry>();
            entries.AddRange(KeyedXml.ReadFolders(mod.foldersToLoadDescendingOrder, SourceLanguage));
            entries.AddRange(ScanDefs(mod));
            return entries;
        }

        public static List<Entry> ScanDefs(ModContentPack mod)
        {
            var entries = new List<Entry>();
            var seen = new HashSet<string>();

            foreach (Type defType in GenDefDatabase.AllDefTypesWithDatabases())
            {
                string folder = defType.Name;
                DefInjectionUtility.ForEachPossibleDefInjection(defType,
                    (suggestedPath, normalizedPath, isCollection, currentValue, currentValueCollection,
                        translationAllowed, fullListTranslationAllowed, field, def) =>
                    {
                        if (!translationAllowed || def.generated || def.modContentPack != mod) return;
                        if (isCollection)
                        {
                            int i = 0;
                            foreach (string item in currentValueCollection)
                                Add(entries, seen, folder, suggestedPath + "." + i++, item, field);
                        }
                        else
                        {
                            Add(entries, seen, folder, suggestedPath, currentValue, field);
                        }
                    });
            }
            return entries;
        }

        private static void Add(List<Entry> entries, HashSet<string> seen, string folder, string path, string text, FieldInfo field)
        {
            if (!IsText(text, field)) return;
            Entry e = Entry.Injected(folder, path, text);
            if (seen.Add(e.Id)) entries.Add(e);
        }

        /// <summary>
        /// Which strings of a Def are texts for a player: the game's own rule for a missing translation
        /// (a field marked MustTranslate, or any other string that holds a space), plus MayTranslate fields.
        /// Def.label and Def.description are MustTranslate, so a one-word label is taken. A one-word string
        /// in an unmarked field is not, which is the rule's known miss: the blacklist and a later pass are
        /// where a mod's own exceptions go.
        /// </summary>
        public static bool IsText(string text, FieldInfo field)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return field.HasAttribute<MustTranslateAttribute>()
                || field.HasAttribute<MayTranslateAttribute>()
                || text.IndexOf(' ') >= 0;
        }
    }
}
