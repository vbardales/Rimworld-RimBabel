---
localization: unchecked
translation_en: unchecked
translation_fr: unchecked
mod:          RimBabel
packageId:    nelim.rimbabel
repo:         Rimworld-RimBabel
visibility:   public
detached:     no
stage:        showcase
workflow_stage: horsMonoRepo
licence:      original
licence_at:   original work
upstream_mod_remotes: N/A
licence_name: MIT
licence_file: LICENSE (identical copy in Mod/LICENSE)
dependencies: none yet
showcase:     unchecked
settings_audit: unchecked
tested_on:    N/A
workshop:      N/A
remaining:
  - unverified: ModIcon, Preview and the Steam description are not made (the owner generates the icon; nothing here generates one)
  - unverified: the extractor (Source/Game/SourceScanner.cs) ran once in a real game, in the WSL install: Pickle request `20261004-000340-546-454b` (English, tree 8e7ecfb, played 2026-10-04 19:23-19:28) 4 scenarios passed, `exitReason: passed`, `Player.log` shows "5 new, 0 changed, 0 removed" for the fixture mod then "0 new, 5 unchanged" on the second scan, with no error line. Not yet proved: a large real mod (many defs, patches, inheritance), and the narrowed walk with many mods loaded (the suite loads Core and the DLC only)
  - defect: the first scan was far too slow (about 72 s for one small mod: the game's walk read every def of every mod before the mod's own were kept). Fixed 2026-10-04 by passing the mod's metadata to `DefInjectionUtility`, which skips other mods' defs before reading a field; a fifth Pickle scenario asserts a scan under 15 s. Proved in French on 2026-10-04: scans took 18-111 ms and the speed scenario passed (414 ms)
  - defect: no settings page, and the engines (DeepL, Anthropic, OpenAI-compatible) have never talked to their real services (no key was used) and are not reachable from the game; import, glossary, blacklist and the translation pipeline exist in the core (105 offline checks) but nothing reaches them from the game (the Mod class returns an empty settings category, so there is no empty page)
  - unverified: the About.xml description is a draft: it ends with the "Source code on GitHub" link and the <url> field points at the repository, but the text itself is a placeholder to rewrite once the mod does something
  - unverified: GitHub social preview image (needs Mod/About/Preview.png, which the owner generates; the setting exists only on the web page)
  - unverified: Mod/Assemblies/RimBabel.dll is committed from the sources of 2026-10-03; the shipped assembly must match the sources before any upload
  - unverified: the Pickle suite (Tests/Pickle, 1 feature, 5 scenarios): the English pass of 2026-10-04 (4 scenarios, tree 8e7ecfb) passed; the French pass `20261004-000341-508-ad5b` (5 scenarios, including the new speed one) passed on the tree that carries the fix (scans 18-111 ms); the speed scenario also passed in English (`20261004-193459-869-da64`, scan 98 ms); the first four passed in English on the older tree. A full 5-scenario English pass on the final tree is a non-regression pass for the end. Execution and reading of the reports is a criterion of done -> tested; passes are declared in Tests/Pickle/README.md (minimal English, minimal French)
  - unverified: settings_audit, localization, translation_en and translation_fr are all unchecked: the mod has no player-facing text yet
  - unverified: licence of the generated translation packages - a package is a derivative of its source mod, so the generator refuses to call one publishable while the source licence is unknown (PackageWriter.Blockers)
session:      local_a2fc6f2c-0a40-46e3-b162-497e9923317b
updated:      2026-10-04, repository created and first commits pushed (e936541): horsMonoRepo; nothing published
---

# RimBabel - status

## Decision of 2026-10-04: `horsMonoRepo`

The owner asked for the repository to be created. `vbardales/Rimworld-RimBabel` (public, topics `rimworld`,
`rimworld-mod`, `mod`) exists, `origin` is set, and `main` is pushed: the remote head `e936541` equals the local one,
checked with `gh api`. Every criterion of `dansMonoRepo -> horsMonoRepo` now holds (table below, last column of the 10-03
decision). Retained: `workflow_stage: horsMonoRepo`, `stage: showcase` (the `stage` field has six codes and no code of its
own for this state). The next transition, `horsMonoRepo -> ModIcon générée`, asks for finished development, a build and
an icon the owner generates: none of that holds.

## Decision of 2026-10-03, replaced on 2026-10-04 (audit against `AUDIT.md`, protocols read listed in `docs/PROTOCOLS-READ.md`)

**Retained state then: `dansMonoRepo`** (`stage: port`, `workflow_stage: dansMonoRepo`). The first transition,
`dansMonoRepo -> horsMonoRepo`, is not met: the criteria are a standalone git repository, an existing GitHub
repository with its remote, and at least one pushed commit. The standalone repository now exists locally (init on
2026-10-03, branch `main`); the GitHub repository and the first commit do not. Nothing later in the chain was
checked, since nothing earlier holds.

What the transition asks, and where each point stands:

| Criterion | State |
|---|---|
| Standalone git repository | **validated** (own `.git`, branch `main`) |
| GitHub repository and remote, first commit pushed | **validated 2026-10-04**: `vbardales/Rimworld-RimBabel`, remote head equals local head `e936541` (at the 10-03 audit: not met) |
| `STATUS.md` initialised | **validated** (this file) |
| Visibility and licence status defined and justified | **validated**: public, original work, MIT. Ideas come from other translators, no code does (see `ATTRIBUTION.md`) |
| `upstream_mod_remotes` filled | **validated**: `N/A`, there is no source mod to port. The tools studied are listed in `ATTRIBUTION.md` with their licences, since two of them are GPL and none of their code is used |
| packageId, name, repository and folder coherent | **validated**: `nelim.rimbabel`, `RimBabel`, folder `RimBabel`; the repository name is to be agreed |
| Documentation in English: README, ATTRIBUTION, LICENSE, CHANGELOG | **validated**; `ATTRIBUTION.md` and `LICENSE` are identical copies in `Mod/` |

## Where the work is

- `Source/Core/` is the package core, plain C# with no game type in it: `Entry`, `Manifest` (the XML manifest that is the
  source of truth of a package), `Merge` (update by key and source hash), `PackageWriter` (writes a repository that is
  ready to commit). `Tests/` runs 139 checks on it without the game (`dotnet run`, see `TESTING.md`); all pass as of
  2026-10-03.
- `Source/Game/` is the in-game layer: `SourceScanner` lists a loaded mod's texts (the Keyed files of its load folders
  through `Source/Core/KeyedXml.cs`, and every Def string through the game's own `DefInjectionUtility`), `PackageBuilder`
  writes or updates the package for the active language under `SaveData/RimBabel/`, and a developer-menu action
  (`RimBabel > Write a translation package for a mod...`) runs it. It compiles; it has not run in a game.
- Not started: the settings page
  and its hidden MainButtons shortcut, the engines (DeepL, Anthropic, OpenAI-compatible), the glossary and blacklist
  screens, the regex search, the import of an existing translation pack.

## Source mod and git history

RimBabel is original work: there is no source mod and no upstream repository to start from or to send pull requests
to. The translators it learns from are credited in `ATTRIBUTION.md`; for each, the repository, its licence and whether
anything was taken. Their licences were read on 2026-10-03; the GPL-3.0 ones (RimTranslate, TokcDK's translator,
Auto-AI-Translation-Core) rule out copying code into an MIT project, and none was copied.

## Evidence

There is none on disk and none in git yet. `.dds` files are ignored (`*.dds`) and nothing tracked needs removing. What
to keep from a Pickle run, once there are runs, is written in `TESTING.md`.

## Translation audit

Not started. The mod has no player-facing text yet. Its own interface will need English and French Keyed files, with
the French read by the owner (`FRENCH_REVIEW.md` at the root, generated by `scripts/Make-FrenchReview.ps1`). The
translation packages RimBabel generates are a separate matter: each carries its own `STATUS.md`, and a French
package is also read by the owner before it counts as complete.

## Settings audit

Not started (`unchecked`). Expected: useful settings exist (engines and keys, glossary, blacklist, output package), so
primary access is Mod options -> RimBabel and the MainButtons shortcut is hidden by default, opening the same page.
