---
localization: partial
translation_en: partial
translation_fr: partial
mod:          RimBabel
packageId:    nelim.rimbabel
repo:         Rimworld-RimBabel
visibility:   public
detached:     no
stage:        showcase
workflow_stage: preOptions
licence:      original
licence_at:   original work
upstream_mod_remotes: N/A
licence_name: MIT
licence_file: LICENSE (identical copy in Mod/LICENSE)
dependencies: none yet
showcase:     unchecked
settings_audit: partial
tested_on:    N/A
workshop:      N/A
remaining:
  - unverified: the Steam description is the one in About.xml (rewritten 2026-10-04, to reread by the owner); ModIcon.png (128x128, 25 KB, from the owner's cut-out source) and Preview.png (896x504, 482 KB, rendered by Render-Preview.cjs, copy in Art/Gallery/0-preview.png) are installed; the 32 px legibility check of the icon passes for the head, while the two side objects blur, so the owner decides
  - unverified: the extractor (Source/Game/SourceScanner.cs) ran once in a real game, in the WSL install: Pickle request `20261004-000340-546-454b` (English, tree 8e7ecfb, played 2026-10-04 19:23-19:28) 4 scenarios passed, `exitReason: passed`, `Player.log` shows "5 new, 0 changed, 0 removed" for the fixture mod then "0 new, 5 unchanged" on the second scan, with no error line. Not yet proved: a large real mod (many defs, patches, inheritance), and the narrowed walk with many mods loaded (the suite loads Core and the DLC only)
  - defect: the first scan was far too slow (about 72 s for one small mod: the game's walk read every def of every mod before the mod's own were kept). Fixed 2026-10-04 by passing the mod's metadata to `DefInjectionUtility`, which skips other mods' defs before reading a field; a fifth Pickle scenario asserts a scan under 15 s. Proved in French on 2026-10-04: scans took 18-111 ms and the speed scenario passed (414 ms)
  - defect: the three engines have each talked to their real service once; Anthropic did on 2026-10-05 (claude-haiku-4-5, three short texts, everything intact, after the key was replaced by one scoped to a workspace); the OpenAI-compatible one on 2026-10-05 on 2026-10-05 (api.openai.com, gpt-4.1-nano, three short texts: placeholders, both markers and the colour tag all came back intact); DeepL did, once, on 2026-10-04 (free host, three short texts), see docs/runs/history.md; the page has a test button to try one, not yet pressed; there is no window to translate a mod from, only the developer-menu action
  - unverified: translation_fr - French review by the owner (FRENCH_REVIEW.md, revision 9632d5d, the owner's seven corrections applied, awaiting her validation); translation_en and localization - the in-game checks in English and French (raw keys, fallback, clipping), scenario 02-settings written and not run
  - unverified: settings_audit - the runtime rows of the table under "Settings audit" (primary route by Mod options, the page drawing without a logged error, the file written by the game, a restart reading it, RIMMSQOL)
  - unverified: the About.xml description is a draft: it ends with the "Source code on GitHub" link and the <url> field points at the repository, but the text itself is a placeholder to rewrite once the mod does something
  - unverified: GitHub social preview image (Mod/About/Preview.png exists now; the setting exists only on the web page, the owner uploads it)
  - unverified: Mod/Assemblies/RimBabel.dll is committed from the sources of 2026-10-04 (revision 7cb8a47 plus the protector fix of 2026-10-04); the shipped assembly must match the sources before any upload
  - unverified: the Pickle suite (Tests/Pickle, 2 features, 12 scenarios: 01-package 5, 02-settings 7. French `20261005-002045-495-1685` 12 of 12 passed on tree acda702, evidence `Tests/Pickle/Evidence/opt-fr`; English `20261005-053541-002-b474` 12 of 12 on the same tree, evidence `opt-en`; the first English attempt `…6660` was cut by a false positive of the launcher after scenario 10 and is no proof): the English pass of 2026-10-04 (4 scenarios, tree 8e7ecfb) passed; the French pass `20261004-000341-508-ad5b` (5 scenarios, including the new speed one) passed on the tree that carries the fix (scans 18-111 ms); the speed scenario also passed in English (`20261004-193459-869-da64`, scan 98 ms); the first four passed in English on the older tree. A full 5-scenario English pass on the final tree is a non-regression pass for the end. Execution and reading of the reports is a criterion of done -> tested; passes are declared in Tests/Pickle/README.md (minimal English, minimal French)
  - unverified: licence of the generated translation packages - a package is a derivative of its source mod, so the generator refuses to call one publishable while the source licence is unknown (PackageWriter.Blockers)
session:      local_a2fc6f2c-0a40-46e3-b162-497e9923317b
updated:      2026-10-05, v1 scope fixed, ModIcon accepted: preOptions; nothing published
---

# RimBabel - status

## Decision of 2026-10-05: `preOptions`

The owner fixed the scope of v1: **the developer menu plus the settings page**. A window to translate a mod from the
interface, the regular-expression search and the hover original are not part of v1 (BACKLOG). She accepted the ModIcon as
delivered (32 px check: the head reads, the two side objects blur; her override). Transitions, in order:
`horsMonoRepo -> ModIcon générée` holds (development finished for that scope, build current, `ModIcon.png` 128x128 in
`Mod/About`, accepted by the owner); `-> Preview générée` holds (`Preview.png` 896x504, 482 KB, read by eye 2026-10-04);
`-> preOptions` holds (English description, plain name, teal accent against the gold secondary ink). `preOptions -> options`
fails: the runtime rows of the settings audit are not all proved (restart reading the values back, RIMMSQOL, the primary
route by Mod options in a real session) and `settings_audit` is `partial`. Retained: `workflow_stage: preOptions`,
`stage: showcase`.


## Decision of 2026-10-04: `horsMonoRepo` (superseded)

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
  ready to commit). `Tests/` runs 304 checks (plus 47 against the game's own assemblies) on it without the game (`dotnet run`, see `TESTING.md`); all pass as of
  2026-10-03.
- `Source/Game/` is the in-game layer: `SourceScanner` lists a loaded mod's texts (the Keyed files of its load folders
  through `Source/Core/KeyedXml.cs`, and every Def string through the game's own `DefInjectionUtility`), `PackageBuilder`
  writes or updates the package for the active language under `SaveData/RimBabel/`, and a developer-menu action
  (`RimBabel > Write a translation package for a mod...`) runs it. It compiles; it has not run in a game.
- Done: the settings page (see the Settings audit below)
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

Audit of 2026-10-04, revision `88cb3d6`. The mod's own interface is the settings page and the hidden shortcut; there is
no other player-facing text (no gizmo, letter, alert or thought).

- **Where the texts live.** English: `Mod/Languages/English/Keyed/RimBabel.xml`, 45 keys. French: `Mod/Languages/French/Keyed/RimBabel.xml`,
  the same 45 keys, and `Mod/Languages/French/DefInjected/MainButtonDef/RimBabel.xml` (2 keys, the shortcut's label and
  description). The English of the shortcut is the Def's own value, the game's native fallback.
- **Checked offline** (`Tests/SettingsTests.cs`, part of the 304 checks): English and French define exactly the same keys;
  no empty text; the same `{n}` parameters in both languages for every key; no `(e)` form in French; every key the code asks
  for exists and every key defined is asked for (a key built from an enum name is covered name by name); counted phrases
  have `.One` and `.Many` forms in both languages; the shortcut is hidden by default and its French injection exists.
  `scripts/Check-DefInjected.ps1` on `Mod`: 2 keys checked, 0 errors (that script checks injection paths, not coverage).
- **Not checked, and why `partial`.** In a running game, in English and in French, for raw keys, fallback text and clipping: the
  Pickle scenarios are written (`02-settings.feature`: every text exists in the language of the pass, and a `@review`
  screenshot) and have not run. French has not been read by the owner.
- **Known limits, stated rather than hidden.** (1) The engines' error messages are technical English strings built in the
  core (`anthropic answered 401: ...`); the page shows them after "It failed:". Translating them would mean carrying a key
  through the core for every failure. (2) The developer-menu entry `RimBabel > Write a translation package for a mod...` and
  the log lines are developer tooling in English. (3) Product names (DeepL, Anthropic, OpenAI, Ollama...) are not translated.
- **No gender agreement.** No text refers to a pawn, so the neutral o-series and the player choice for gendered French do not
  apply, and no such setting is offered.
- **French review by the owner: corrections applied, not yet validated.** On 2026-10-04 the owner read the sheet for revision `88cb3d6` and answered "pas validé" with seven corrections (syntax identifiers `K:key` and `D:DefType/defName.field` kept verbatim in the blacklist text, "mod de traduction" everywhere instead of "paquet de traduction", and the wording of `BatchDesc`, `TestDesc`, `DictionaryBad`, `Author`, `OutputDesc`), and confirmed "enregistrée en clair". They are applied; `translation_fr` stays `partial` until she validates the new sheet, which this section never records for her.

## Settings audit

Audit of 2026-10-04, revision `88cb3d6`. **`settings_audit: partial`**: useful settings exist and the offline checks pass; the
runtime checks have not run.

- **Inventory and why each option exists.** Engine (DeepL, Anthropic or an OpenAI-compatible server: which service translates),
  its key, model and address (what that service needs; a local server needs no key), texts per request (cost against
  gentleness to a small model), a test button (the only way to tell a wrong key from a wrong model without translating a
  mod), author name (written into About.xml of generated packages), target language (default: the game's), output folder
  (default: next to the saves), a dictionary (one word, one translation, in every mod: the owner's requirement), a blacklist
  (what is never sent), and a reset. Not exposed: the length-ratio limits and the retry count, internal constants with no
  decision behind them.
- **Access.** Primary: Mod options -> RimBabel (`SettingsCategory()` returns "RimBabel"). Shortcut: `MainButtonDef` `RimBabel_Settings`,
  `buttonVisible=false`, `validWithoutMap=true`, worker `MainButtonWorker_Settings`, which opens `Dialog_ModSettings` for the same
  mod. No customization mod is required.
- **Application and scope.** Every control applies immediately; the file is written when the window closes. Global, not per save.
  The keys are plain text in the game's settings file, which the page says; they are never written into a manifest or a package
  (checked: a manifest holds no key).
- **Tests against `MOD_SETTINGS.md` section 4.**

| Area | Result |
|---|---|
| First use | **Passed offline** (Tests/Game, 47 checks): a clean file loads the documented defaults, no key, default folder and language |
| Primary access | **Passed in French (2026-10-05, `20261005-002045-495-1685`, tree acda702) and in English (`20261005-053541-002-b474`, tree acda702, 12 of 12).** The scenario opens the game's options window on the Mod options category, finds RimBabel in the list the window itself builds (`cachedModsWithSettings`), chooses it (the click is simulated by setting `selectedMod`) and draws the page ten frames with no logged error |
| Actual effect | **Passed offline** for the data (engine choice makes the matching engine, a missing key or model or a bad address is a problem, the dictionary and blacklist parse); the widgets' effect on screen is **unverified** |
| Persistence | **Passed offline** through the game's own Scribe, and **in game in English and French** (same runs): the game writes the file, and `ReadModSettings` reads back the author and a three-line dictionary without a carriage return. Not a real restart of the game |
| Input validation | **Passed offline**: batch size outside 1-100 is brought back, a bad server address, a missing key or model and an unreadable dictionary line or regular expression are reported |
| Defaults and upgrades | **Passed offline**: reset restores everything on the page except the keys; fields missing from an older file take their defaults |
| Optional dependencies | Not applicable: nothing is optional |
| Shortcut default | **Passed offline** (the Def) and written as a Pickle scenario, **not run** |
| Shortcut integration (RIMMSQOL) | **Not tested.** Needs RIMMSQOL; revealing the button in its own interface is its behaviour |
| Runtime robustness | **Unverified**: the scenario that lets the page draw for ten frames and then asks for logged errors has not run. An exception in `OnGUI` is logged, not thrown, so only that scenario can show one |
| No settings | Not applicable |

- **Two traps found on the way**, both now covered by a check: the serializer writes the platform's line break (a Windows save
  reads a multi-line dictionary back with CR LF; the text is normalised on load), and the game has no scrolling text area
  (`Widgets.TextAreaScrollable` does not exist in 1.6; the page lays the text out in its own scroll view).
