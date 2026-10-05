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
            get
            {
                string chosen = RimBabelMod.Settings == null ? null : RimBabelMod.Settings.outputFolder;
                return string.IsNullOrWhiteSpace(chosen) ? Path.Combine(GenFilePaths.SaveDataFolderPath, "RimBabel") : chosen.Trim();
            }
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

        /// <summary>What a translation run did, for the log.</summary>
        public sealed class TranslationOutcome
        {
            public PackageResult Package;
            public PipelineReport Report;
            public string EngineName;
        }

        /// <summary>
        /// Brings the package of a mod up to date, sends what is still untranslated to the engine of the settings (with the dictionary
        /// and the blacklist of the settings), and writes the package again. Blocks until the engine is done: run it off the main thread.
        /// A refused key or an exhausted quota stops the run, and what was translated before it is kept.
        /// </summary>
        public static TranslationOutcome Translate(ModContentPack mod, string language, string author, RimBabelSettings settings, IHttp http)
        {
            PackageResult package = Build(mod, language, author);
            PackageSpec spec = SpecFor(mod, language, author);
            Manifest manifest = Manifest.Parse(File.ReadAllText(Path.Combine(package.Root, PackageWriter.ManifestPath), Encoding.UTF8));
            int ignored;
            manifest.Glossary = LineLists.ParseDictionary(settings.dictionaryText, out ignored);
            manifest.Blacklist = LineLists.ParseRules(settings.blacklistText);
            ITranslationEngine engine = EngineFactory.Create(settings.ToConfig(), http);
            PipelineReport report = Pipeline.Run(manifest, engine, SourceScanner.SourceLanguage, language, settings.batchSize);
            PackageWriter.Save(package.Root, spec, manifest);
            return new TranslationOutcome { Package = package, Report = report, EngineName = engine.Name };
        }

        /// <summary>Takes back the machine texts one engine wrote in the package of a mod (see Merge.Flush), and writes the package again.</summary>
        public static int Flush(ModContentPack mod, string language, string author, string engine)
        {
            PackageSpec spec = SpecFor(mod, language, author);
            string root = Path.Combine(OutputRoot, spec.PackageId);
            string path = Path.Combine(root, PackageWriter.ManifestPath);
            if (!File.Exists(path)) return 0;
            Manifest manifest = Manifest.Parse(File.ReadAllText(path, Encoding.UTF8));
            int n = Merge.Flush(manifest.Entries, engine);
            if (n > 0) PackageWriter.Save(root, spec, manifest);
            return n;
        }

        /// <summary>The engines whose texts are in the package of a mod, as the entries record them.</summary>
        public static System.Collections.Generic.List<string> EnginesIn(ModContentPack mod, string language, string author)
        {
            string path = Path.Combine(OutputRoot, SpecFor(mod, language, author).PackageId, PackageWriter.ManifestPath);
            var list = new System.Collections.Generic.List<string>();
            if (!File.Exists(path)) return list;
            foreach (Entry e in Manifest.Parse(File.ReadAllText(path, Encoding.UTF8)).Entries)
                if (e.Status == EntryStatus.Machine && !string.IsNullOrEmpty(e.Engine) && !list.Contains(e.Engine)) list.Add(e.Engine);
            return list;
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
