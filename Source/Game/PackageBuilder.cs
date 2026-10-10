using System;
using System.IO;
using System.Linq;
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

        /// <summary>The language the texts of the mods are written in, from the settings (English by default).</summary>
        public static string SourceLanguage
        {
            get
            {
                string chosen = RimBabelMod.Settings == null ? null : RimBabelMod.Settings.sourceLanguage;
                return string.IsNullOrWhiteSpace(chosen) ? SourceScanner.SourceLanguage : chosen.Trim();
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
            return PackageWriter.Write(root, spec, SourceScanner.Scan(mod, true, SourceLanguage), DateTime.UtcNow);
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
                PipelineReport report = Pipeline.Run(Manifest, Engine, SourceLanguage, Language, BatchSize);
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
            if (string.Equals(language, SourceLanguage, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The language of the texts and the target language are both " + language + ": there is nothing to translate.");
            PackageResult package = Build(mod, language, author);
            Manifest manifest = Manifest.Parse(File.ReadAllText(Path.Combine(package.Root, PackageWriter.ManifestPath), Encoding.UTF8));
            manifest.Glossary = GlossaryFor(settings, language);
            manifest.Blacklist = LineLists.ParseRules(settings.blacklistText);
            if (settings.reuseInstalledPacks) Importer.ImportPacks(manifest, InstalledPacks(mod, language));
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

        /// <summary>
        /// The translation packs installed next to a mod: every other running mod that holds a Languages/&lt;language&gt; folder, in its root or
        /// in one of its load folders. The packages RimBabel wrote itself are left out (their texts are machine work and would come back as
        /// human work), and so is the mod being translated.
        /// </summary>
        public static System.Collections.Generic.List<InstalledPack> InstalledPacks(ModContentPack source, string language)
        {
            var packs = new System.Collections.Generic.List<InstalledPack>();
            foreach (ModContentPack other in LoadedModManager.RunningModsListForReading)
            {
                if (other == source || other.IsCoreMod || other.IsOfficialMod) continue;
                if (other.PackageId != null && other.PackageId.StartsWith("nelim.rimbabel.", StringComparison.OrdinalIgnoreCase)) continue;
                var pack = new InstalledPack { Label = other.Name + " (" + other.PackageId + ")" };
                var roots = new System.Collections.Generic.List<string> { other.RootDir };
                roots.AddRange(other.foldersToLoadDescendingOrder);
                foreach (string root in roots.Distinct())
                {
                    string folder = Path.Combine(Path.Combine(root, "Languages"), language);
                    if (Directory.Exists(folder)) pack.Folders.Add(folder);
                }
                if (pack.Folders.Count > 0) packs.Add(pack);
            }
            return packs;
        }

        /// <summary>
        /// Brings the texts of the installed translation packs into the package of a mod, as human work, and writes the package again.
        /// Nothing is sent to an engine. Returns what came in.
        /// </summary>
        public static ImportReport ImportInstalled(ModContentPack mod, string language, string author)
        {
            PackageResult package = Build(mod, language, author);
            PackageSpec spec = SpecFor(mod, language, author);
            Manifest manifest = Manifest.Parse(File.ReadAllText(Path.Combine(package.Root, PackageWriter.ManifestPath), Encoding.UTF8));
            ImportReport report = Importer.ImportPacks(manifest, InstalledPacks(mod, language));
            PackageWriter.Save(package.Root, spec, manifest);
            return report;
        }

        /// <summary>The dictionary shared by every mod translated into one language: one file in the output folder, not one per package.</summary>
        public static string SharedDictionaryPath(string language)
        {
            return Path.Combine(OutputRoot, "dictionary." + Safe(language) + ".txt");
        }

        /// <summary>The page's own lines first, then the shared file's: the page overrides the file for a word both hold.</summary>
        private static System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>> GlossaryFor(RimBabelSettings settings, string language)
        {
            int ignored;
            var own = LineLists.ParseDictionary(settings.dictionaryText, out ignored);
            string path = SharedDictionaryPath(language);
            if (!File.Exists(path)) return own;
            return LineLists.MergeDictionaries(own, LineLists.ParseDictionary(File.ReadAllText(path, Encoding.UTF8), out ignored));
        }

        /// <summary>Creates the shared dictionary file of a language with a short header, when it does not exist yet, and returns its path.</summary>
        public static string EnsureSharedDictionary(string language)
        {
            string path = SharedDictionaryPath(language);
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "# RimBabel dictionary for " + language + ", shared by every mod you translate into it.\n# One line per word: word = translation. The dictionary on the settings page wins where both have the same word.\n", new UTF8Encoding(false));
            }
            return path;
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
