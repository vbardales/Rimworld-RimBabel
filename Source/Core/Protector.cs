using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RimBabel.Core
{
    /// <summary>One thing hidden from the engine: a placeholder to give back as it was, or a glossary term to give back as its required translation.</summary>
    public sealed class Slot
    {
        public string Token;
        public string Original;
        public string Restore;
        public bool IsTerm;
    }

    public sealed class Protected
    {
        public string Text;
        public List<Slot> Slots = new List<Slot>();
    }

    public sealed class Restored
    {
        public bool Ok;
        public string Text;
        public string Reason;
    }

    /// <summary>
    /// Keeps what a machine must not touch out of its sight, and checks it came back. Placeholders
    /// ({0}, {PAWN_nameDef}, a whole {PAWN_gender ? a : b} switch), rich-text tags (&lt;color=#fff&gt;) and
    /// markers ((*Colonist)) are swapped for opaque tokens; glossary terms are swapped for tokens too, so
    /// the required translation is used whatever the engine would have chosen. A translation that lost a
    /// token, invented one, came back empty or has an implausible length is refused: the caller keeps the
    /// source text rather than ship a broken string.
    ///
    /// A gender switch is protected whole, so its literal branches are not translated; that is a known
    /// limit of the first version, and a refused translation is better than a half-translated one.
    /// </summary>
    public static class Protector
    {
        private const string Open = "⟦";   // mathematical left white square bracket
        private const string Close = "⟧";

        private static readonly Regex Tags = new Regex(@"</?[A-Za-z][A-Za-z0-9_]*(?:=[^>]*)?>|\(\*[A-Za-z_]+(?:\s[^)]*)?\)|\(/[A-Za-z_]+\)", RegexOptions.Compiled);
        private static readonly Regex TokenPattern = new Regex(Open + @"(\d+)" + Close, RegexOptions.Compiled);

        public static Protected Protect(string source, IList<KeyValuePair<string, string>> glossary)
        {
            source = Hashing.Normalize(source);
            var result = new Protected();

            // 1. Spans to hide: balanced braces first (they win), then tags and markers outside them.
            var spans = new List<KeyValuePair<int, int>>();
            int i = 0;
            while (i < source.Length)
            {
                if (source[i] != '{') { i++; continue; }
                int depth = 0, j = i;
                for (; j < source.Length; j++)
                {
                    if (source[j] == '{') depth++;
                    else if (source[j] == '}' && --depth == 0) break;
                }
                if (depth != 0) { i++; continue; }   // unbalanced: left alone rather than guessed at
                spans.Add(new KeyValuePair<int, int>(i, j + 1 - i));
                i = j + 1;
            }
            foreach (Match m in Tags.Matches(source))
                if (!spans.Any(s => m.Index < s.Key + s.Value && m.Index + m.Length > s.Key))
                    spans.Add(new KeyValuePair<int, int>(m.Index, m.Length));
            spans.Sort((a, b) => a.Key.CompareTo(b.Key));

            var sb = new StringBuilder();
            int at = 0;
            foreach (KeyValuePair<int, int> span in spans)
            {
                sb.Append(source, at, span.Key - at);
                var slot = new Slot { Token = Open + result.Slots.Count + Close, Original = source.Substring(span.Key, span.Value) };
                slot.Restore = slot.Original;
                result.Slots.Add(slot);
                sb.Append(slot.Token);
                at = span.Key + span.Value;
            }
            sb.Append(source, at, source.Length - at);
            string text = sb.ToString();

            // 2. Glossary terms in what is left, longest first so "colony ship" wins over "colony".
            if (glossary != null)
                foreach (KeyValuePair<string, string> term in glossary.Where(g => !string.IsNullOrEmpty(g.Key)).OrderByDescending(g => g.Key.Length))
                {
                    var rx = new Regex(@"(?<!\w)" + Regex.Escape(term.Key) + @"(?!\w)", RegexOptions.IgnoreCase);
                    string to = term.Value;
                    text = rx.Replace(text, match =>
                    {
                        string restore = to;
                        // A term that opens a sentence keeps its capital.
                        if (match.Value.Length > 0 && char.IsUpper(match.Value[0]) && restore.Length > 0 && char.IsLower(restore[0]))
                            restore = char.ToUpperInvariant(restore[0]) + restore.Substring(1);
                        var slot = new Slot { Token = Open + result.Slots.Count + Close, Original = match.Value, Restore = restore, IsTerm = true };
                        result.Slots.Add(slot);
                        return slot.Token;
                    });
                }

            result.Text = text;
            return result;
        }

        public static Restored Restore(string translated, Protected p, string source)
        {
            if (translated == null) return Fail("the engine returned nothing");
            string text = translated;
            string src = Hashing.Normalize(source);

            if (src.Trim().Length > 0 && text.Trim().Length == 0) return Fail("the translation is empty");

            // Every token exactly once, and no token that was never given.
            var seen = new Dictionary<int, int>();
            foreach (Match m in TokenPattern.Matches(text))
            {
                int n = int.Parse(m.Groups[1].Value);
                if (n >= p.Slots.Count) return Fail("the translation holds a token that was never sent: " + m.Value);
                int c;
                seen.TryGetValue(n, out c);
                seen[n] = c + 1;
            }
            for (int n = 0; n < p.Slots.Count; n++)
            {
                int c;
                seen.TryGetValue(n, out c);
                if (c != 1) return Fail("the token for '" + p.Slots[n].Original + "' appears " + c + " times instead of once");
            }

            // Tokens may swap places (grammar differs), but a closing tag in front of its own opening tag is broken markup.
            for (int c = 0; c < p.Slots.Count; c++)
            {
                Match close = Regex.Match(p.Slots[c].Original, @"^(?:</([A-Za-z][A-Za-z0-9_]*)>|\(/([A-Za-z_]+)\))$");
                if (!close.Success) continue;
                string name = close.Groups[1].Success ? close.Groups[1].Value : close.Groups[2].Value;
                // The nth closer of a name belongs to the nth opener of that name, as in the source: two pairs of one tag may swap places.
                int nth = 0;
                for (int k = 0; k < c; k++)
                {
                    Match earlier = Regex.Match(p.Slots[k].Original, @"^(?:</([A-Za-z][A-Za-z0-9_]*)>|\(/([A-Za-z_]+)\))$");
                    if (earlier.Success && (earlier.Groups[1].Success ? earlier.Groups[1].Value : earlier.Groups[2].Value) == name) nth++;
                }
                for (int o = 0; o < p.Slots.Count; o++)
                {
                    if (!Regex.IsMatch(p.Slots[o].Original, @"^(?:<" + Regex.Escape(name) + @"(?:=[^>]*)?>|\(\*" + Regex.Escape(name) + @"(?:\s[^)]*)?\))$")) continue;
                    if (nth-- > 0) continue;
                    if (text.IndexOf(p.Slots[c].Token, StringComparison.Ordinal) < text.IndexOf(p.Slots[o].Token, StringComparison.Ordinal))
                        return Fail("the closing '" + p.Slots[c].Original + "' comes before its opening '" + p.Slots[o].Original + "'");
                    break;
                }
            }

            // A length far from the source's is a sign of a refusal, a summary or a run-on.
            string probe = p.Text.Trim();
            if (probe.Length >= 12)
            {
                double ratio = (double)text.Trim().Length / probe.Length;
                if (ratio < 0.25 || ratio > 4.0) return Fail("the translation is " + ratio.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " times the length of the source");
            }

            foreach (Slot slot in p.Slots) text = text.Replace(slot.Token, slot.Restore);

            // Engines like to trim: give back the source's own leading and trailing whitespace.
            string lead = src.Substring(0, src.Length - src.TrimStart().Length);
            string trail = src.Substring(src.TrimEnd().Length);
            text = lead + text.Trim() + trail;
            return new Restored { Ok = true, Text = text };
        }

        private static Restored Fail(string reason)
        {
            return new Restored { Ok = false, Reason = reason };
        }
    }
}
