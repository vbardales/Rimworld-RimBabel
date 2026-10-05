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
        /// A translation run split in two: <see cref="Prepare"/> touches the game (the scan, the output folder, the settings) and must
        /// run on the main thread; <see cref="Run"/> only talks to the engine and writes files, and is the part to run off it.
        /// </summary>
        public sealed class TranslationJob
        {
            public PackageResult Package;
            internal PackageSpec Spec;
            internal Manifest Manifest;
            internal ITranslationEngine Engine;
            internal string Language;
            internal int BatchSize;

            /// <summary>Blocks until the engine is done. A refused key or an exhausted quota stops the run, and what was translated before it is kept.</summary>
            public TranslationOutcome Run()
            {
                PipelineReport report = Pipeline.Run(Manifest, Engine, SourceScanner.SourceLanguage, Language, BatchSize);
                PackageWriter.Save(Package.Root, Spec, Manifest);
                return new TranslationOutcome { Package = Package, Report = report, EngineName = Engine.Name };
            }
        }

        /// <summary>
        /// Brings the package of a mod up to date and readies the engine of the settings, with the dictionary and the blacklist of the
        /// settings. Reads the game and the settings now, so the caller can run the returned job on another thread.
        /// </summary>
        public static TranslationJob Prepare(ModContentPack mod, string language, string author, RimBabelSettings settings, IHttp http)
        {
            PackageResult package = Build(mod, language, author);
            Manifest manifest = Manifest.Parse(File.ReadAllText(Path.Combine(package.Root, PackageWriter.ManifestPath), Encoding.UTF8));
            int ignored;
            manifest.Glossary = LineLists.ParseDictionary(settings.dictionaryText, out ignored);
            manifest.Blacklist = LineLists.ParseRules(settings.blacklistText);
            return new TranslationJob
            {
                Package = package, Spec = SpecFor(mod, language, author), Manifest = manifest, Language = language, BatchSize = settings.batchSize,
                Engine = EngineFactory.Create(settings.ToConfig(), http),
            };
        }

        /// <summary>Prepare and Run in one go, for a caller that is already off the main thread and does not mind (a test).</summary>
        public static TranslationOutcome Translate(ModContentPack mod, string language, string author, RimBabelSettings settings, IHttp http)
        {
            return Prepare(mod, language, author, settings, http).Run();
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
