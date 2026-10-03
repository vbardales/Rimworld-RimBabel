using System;
using System.Security.Cryptography;
using System.Text;

namespace RimBabel.Core
{
    public enum EntryKind { Keyed, DefInjected }

    /// <summary>
    /// Where a translation stands. Pending has no text yet. Stale has a text written for an older
    /// source: kept as a draft, never shipped as final. Human and Reviewed are never overwritten by
    /// an engine. Locked is never touched at all, even when the source changes.
    /// </summary>
    public enum EntryStatus { Pending, Machine, Stale, Human, Reviewed, Locked }

    public sealed class Entry
    {
        public EntryKind Kind;
        /// <summary>DefInjected only: the def class folder, e.g. ThingDef.</summary>
        public string DefType;
        /// <summary>Keyed: the key. DefInjected: defName.field.path.</summary>
        public string Key;
        public string Source;
        public string Target;
        public string SourceHash;
        public EntryStatus Status = EntryStatus.Pending;
        public string Engine;

        public string Id
        {
            get { return Kind == EntryKind.Keyed ? "K:" + Key : "D:" + DefType + "/" + Key; }
        }

        public Entry Clone()
        {
            return (Entry)MemberwiseClone();
        }

        public static Entry Keyed(string key, string source)
        {
            return new Entry { Kind = EntryKind.Keyed, Key = key, Source = Hashing.Normalize(source), SourceHash = Hashing.Of(source) };
        }

        public static Entry Injected(string defType, string path, string source)
        {
            return new Entry { Kind = EntryKind.DefInjected, DefType = defType, Key = path, Source = Hashing.Normalize(source), SourceHash = Hashing.Of(source) };
        }
    }

    public static class Hashing
    {
        /// <summary>One newline convention, so a file saved on another platform does not look edited.</summary>
        public static string Normalize(string text)
        {
            return text == null ? "" : text.Replace("\r\n", "\n").Replace('\r', '\n');
        }

        public static string Of(string text)
        {
            using (var sha = SHA256.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(Normalize(text)));
                var sb = new StringBuilder(16);
                for (int i = 0; i < 8; i++) sb.Append(h[i].ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
