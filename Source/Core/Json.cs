using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RimBabel.Core
{
    /// <summary>
    /// A small JSON reader and writer, so that talking to a translation service needs no library: the game
    /// ships none, and a bundled one is a dependency of the mod. Values are Dictionary&lt;string, object&gt;,
    /// List&lt;object&gt;, string, double, bool and null.
    /// </summary>
    public static class Json
    {
        public static string Write(object value)
        {
            var sb = new StringBuilder();
            WriteValue(sb, value);
            return sb.ToString();
        }

        private static void WriteValue(StringBuilder sb, object v)
        {
            if (v == null) { sb.Append("null"); return; }
            string s = v as string;
            if (s != null) { WriteString(sb, s); return; }
            if (v is bool) { sb.Append((bool)v ? "true" : "false"); return; }
            if (v is int || v is long) { sb.Append(Convert.ToString(v, CultureInfo.InvariantCulture)); return; }
            if (v is double) { sb.Append(((double)v).ToString("R", CultureInfo.InvariantCulture)); return; }
            var dict = v as IDictionary<string, object>;
            if (dict != null)
            {
                sb.Append('{');
                bool first = true;
                foreach (KeyValuePair<string, object> kv in dict)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    WriteString(sb, kv.Key);
                    sb.Append(':');
                    WriteValue(sb, kv.Value);
                }
                sb.Append('}');
                return;
            }
            var list = v as System.Collections.IEnumerable;
            if (list != null)
            {
                sb.Append('[');
                bool first = true;
                foreach (object item in list)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    WriteValue(sb, item);
                }
                sb.Append(']');
                return;
            }
            throw new ArgumentException("Cannot write a " + v.GetType().Name + " as JSON.");
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        public static object Parse(string text)
        {
            int i = 0;
            object v = ReadValue(text, ref i);
            SkipSpace(text, ref i);
            if (i != text.Length) throw new FormatException("Unexpected text after the JSON value, at " + i + ".");
            return v;
        }

        private static void SkipSpace(string t, ref int i)
        {
            while (i < t.Length && (t[i] == ' ' || t[i] == '\t' || t[i] == '\n' || t[i] == '\r')) i++;
        }

        private static object ReadValue(string t, ref int i)
        {
            SkipSpace(t, ref i);
            if (i >= t.Length) throw new FormatException("Unexpected end of JSON.");
            char c = t[i];
            if (c == '{') return ReadObject(t, ref i);
            if (c == '[') return ReadArray(t, ref i);
            if (c == '"') return ReadString(t, ref i);
            if (Literal(t, ref i, "true")) return true;
            if (Literal(t, ref i, "false")) return false;
            if (Literal(t, ref i, "null")) return null;
            int start = i;
            while (i < t.Length && "+-0123456789.eE".IndexOf(t[i]) >= 0) i++;
            double d;
            if (i == start || !double.TryParse(t.Substring(start, i - start), NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                throw new FormatException("Unexpected character '" + c + "' at " + start + ".");
            return d;
        }

        private static bool Literal(string t, ref int i, string word)
        {
            if (string.CompareOrdinal(t, i, word, 0, word.Length) != 0) return false;
            i += word.Length;
            return true;
        }

        private static Dictionary<string, object> ReadObject(string t, ref int i)
        {
            var d = new Dictionary<string, object>();
            i++;
            SkipSpace(t, ref i);
            if (i < t.Length && t[i] == '}') { i++; return d; }
            while (true)
            {
                SkipSpace(t, ref i);
                if (i >= t.Length || t[i] != '"') throw new FormatException("Expected a key at " + i + ".");
                string key = ReadString(t, ref i);
                SkipSpace(t, ref i);
                if (i >= t.Length || t[i] != ':') throw new FormatException("Expected ':' at " + i + ".");
                i++;
                d[key] = ReadValue(t, ref i);
                SkipSpace(t, ref i);
                if (i < t.Length && t[i] == ',') { i++; continue; }
                if (i < t.Length && t[i] == '}') { i++; return d; }
                throw new FormatException("Expected ',' or '}' at " + i + ".");
            }
        }

        private static List<object> ReadArray(string t, ref int i)
        {
            var l = new List<object>();
            i++;
            SkipSpace(t, ref i);
            if (i < t.Length && t[i] == ']') { i++; return l; }
            while (true)
            {
                l.Add(ReadValue(t, ref i));
                SkipSpace(t, ref i);
                if (i < t.Length && t[i] == ',') { i++; continue; }
                if (i < t.Length && t[i] == ']') { i++; return l; }
                throw new FormatException("Expected ',' or ']' at " + i + ".");
            }
        }

        private static string ReadString(string t, ref int i)
        {
            var sb = new StringBuilder();
            i++;
            while (i < t.Length)
            {
                char c = t[i++];
                if (c == '"') return sb.ToString();
                if (c != '\\') { sb.Append(c); continue; }
                if (i >= t.Length) break;
                char e = t[i++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u':
                        if (i + 4 > t.Length) throw new FormatException("Short \\u escape.");
                        sb.Append((char)int.Parse(t.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        i += 4;
                        break;
                    default: throw new FormatException("Unknown escape \\" + e + ".");
                }
            }
            throw new FormatException("Unterminated string.");
        }
    }
}
