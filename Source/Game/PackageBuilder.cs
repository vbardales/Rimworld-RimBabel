using System;
using System.IO;
using System.Text;
using RimBabel.Core;
using Verse;

namespace RimBabel.Game
{
    /// <summary>Scans a loaded mod and writes or updates its translation package on disk.</summary>
    public static class PackageBuilder
    {
        /// <summary>Where packages are written: next to the game's saves, outside every mod folder.</summary>
        public static string OutputRoot
        {
            get { return Path.Combine(GenFilePaths.SaveDataFolderPath, "RimBabel"); }
        }

        public static PackageSpec SpecFor(ModContentPack mod, string language, string author)
        {
            string lang = Slug(language);
            return new PackageSpec
            {
                PackageId = "nelim.rimbabel." + Slug(mod.PackageId) + "." + lang,
                Name = mod.Name + " (" + language + ")",
                Author = author,
                Language = language,
                FileName = Safe(mod.Name),
                SourcePackageId = mod.PackageId,
                SourceName = mod.Name,
            };
        }

        public static PackageResult Build(ModContentPack mod, string language, string author)
        {
            PackageSpec spec = SpecFor(mod, language, author);
            string root = Path.Combine(OutputRoot, spec.PackageId);
            return PackageWriter.Write(root, spec, SourceScanner.Scan(mod), DateTime.UtcNow);
        }

        private static string Slug(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s.ToLowerInvariant())
                if (char.IsLetterOrDigit(c)) sb.Append(c);
                else if (sb.Length > 0 && sb[sb.Length - 1] != '.') sb.Append('.');
            return sb.ToString().Trim(new[] { '.' });
        }

        private static string Safe(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
                if (char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.Length == 0 ? "Mod" : sb.ToString();
        }
    }
}
