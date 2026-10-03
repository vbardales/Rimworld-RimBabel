using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RimBabel.Core
{
    /// <summary>
    /// What an engine is never asked to translate. A rule is matched against the id of an entry
    /// (<c>K:Key</c> or <c>D:DefType/DefName.field</c>): plain text with <c>*</c> and <c>?</c> is a glob over the whole
    /// id, and <c>re:</c> followed by a regular expression matches anywhere in it. A rule that is not a valid
    /// expression matches nothing, and says so through <see cref="Invalid"/>.
    /// </summary>
    public static class Blacklist
    {
        public static bool Matches(IEnumerable<string> rules, Entry entry)
        {
            foreach (string rule in rules)
            {
                if (string.IsNullOrWhiteSpace(rule)) continue;
                try
                {
                    if (rule.StartsWith("re:", System.StringComparison.Ordinal))
                    {
                        if (Regex.IsMatch(entry.Id, rule.Substring(3))) return true;
                    }
                    else if (Regex.IsMatch(entry.Id, "^" + Regex.Escape(rule).Replace("\\*", ".*").Replace("\\?", ".") + "$"))
                    {
                        return true;
                    }
                }
                catch (System.ArgumentException)
                {
                    // An invalid expression matches nothing.
                }
            }
            return false;
        }

        /// <summary>The rules that cannot be read, so a page can say which ones to fix.</summary>
        public static List<string> Invalid(IEnumerable<string> rules)
        {
            var bad = new List<string>();
            foreach (string rule in rules.Where(r => r != null && r.StartsWith("re:", System.StringComparison.Ordinal)))
            {
                try { new Regex(rule.Substring(3)); }
                catch (System.ArgumentException) { bad.Add(rule); }
            }
            return bad;
        }
    }
}
