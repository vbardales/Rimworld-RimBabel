---
localization: unchecked
translation_en: unchecked
translation_fr: unchecked
mod:          RimBabel
packageId:    nelim.rimbabel
repo:         N/A
visibility:   public
detached:     no
stage:        port
workflow_stage: dansMonoRepo
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
  - unverified: dansMonoRepo to horsMonoRepo - the GitHub repository does not exist yet, so no remote and no first pushed commit (the local git repository was initialised on 2026-10-03, nothing committed)
  - unverified: ModIcon, Preview and the Steam description are not made (the owner generates the icon; nothing here generates one)
  - defect: the in-game part does not exist yet - no Mod class, no settings page, no extractor, no engines; only the package core (Source/Core) and its 36 offline checks
  - unverified: the About.xml description is a draft and does not end with the "Source code on GitHub" link yet, because the repository does not exist (PUBLISHING.md, criterion of the step to preOptions); no <url> field either
  - unverified: the Mod/Assemblies folder is empty, so Mod/ cannot ship as it stands
  - unverified: no Pickle suite exists; gate to done needs the suite written or its absence justified in TESTING.md
  - unverified: settings_audit, localization, translation_en and translation_fr are all unchecked: the mod has no player-facing text yet
  - unverified: licence of the generated translation packages - a package is a derivative of its source mod, so the generator refuses to call one publishable while the source licence is unknown (PackageWriter.Blockers)
session:      local_a2fc6f2c-0a40-46e3-b162-497e9923317b
updated:      2026-10-03, first audit against AUDIT.md (version 5a975b5, 2026-10-02): stage port, nothing published, nothing committed
---

# RimBabel - status

## Decision of 2026-10-03 (audit against `AUDIT.md`, protocols read listed in `docs/PROTOCOLS-READ.md`)

**Retained state: `dansMonoRepo`** (`stage: port`, `workflow_stage: dansMonoRepo`). The first transition,
`dansMonoRepo -> horsMonoRepo`, is not met: the criteria are a standalone git repository, an existing GitHub
repository with its remote, and at least one pushed commit. The standalone repository now exists locally (init on
2026-10-03, branch `main`); the GitHub repository and the first commit do not. Nothing later in the chain was
checked, since nothing earlier holds.

What the transition asks, and where each point stands:

| Criterion | State |
|---|---|
| Standalone git repository | **validated locally**, no commit yet |
| GitHub repository and remote, first commit pushed | **unverified**: the repository is the owner's to create |
| `STATUS.md` initialised | **validated** (this file) |
| Visibility and licence status defined and justified | **validated**: public, original work, MIT. Ideas come from other translators, no code does (see `ATTRIBUTION.md`) |
| `upstream_mod_remotes` filled | **validated**: `N/A`, there is no source mod to port. The tools studied are listed in `ATTRIBUTION.md` with their licences, since two of them are GPL and none of their code is used |
| packageId, name, repository and folder coherent | **validated**: `nelim.rimbabel`, `RimBabel`, folder `RimBabel`; the repository name is to be agreed |
| Documentation in English: README, ATTRIBUTION, LICENSE, CHANGELOG | **validated**; `ATTRIBUTION.md` and `LICENSE` are identical copies in `Mod/` |

## Where the work is

- `Source/Core/` is the package core, plain C# with no game type in it: `Entry`, `Manifest` (the XML manifest that is the
  source of truth of a package), `Merge` (update by key and source hash), `PackageWriter` (writes a repository that is
  ready to commit). `Tests/` runs 36 checks on it without the game (`dotnet run`, see `TESTING.md`); all pass as of
  2026-10-03.
- Not started: reading a source mod's texts in game (`DefInjectionUtility`, `Languages/English/Keyed`), the settings page
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
