---
localization: complete
translation_en: unchecked
translation_fr: unchecked
mod:          RimBabel
packageId:    nelim.rimbabel
repo:         Rimworld-RimBabel
visibility:   public
detached:     no
stage:        showcase
workflow_stage: options
licence:      original
licence_at:   original work
upstream_mod_remotes: N/A
licence_name: MIT
licence_file: LICENSE (identical copy in Mod/LICENSE)
dependencies: none (verified 2026-10-07: no modDependencies, loadAfter or incompatibleWith in About.xml; Source references only the game through Krafs.Rimworld.Ref, no Harmony, no other mod; RIMMSQOL is an optional customization mod that may reveal the shortcut, nothing in the code names it; no LoadFolders.xml needed for a single 1.6 version)
showcase:     unchecked
settings_audit: passed
tested_on:    N/A (the Pickle final runs of 2026-10-08 on tree 49fe6c6 are green but predate the four text corrections of adab2c7; replay needed)
workshop:      item 381412975321 (private, created by the owner's prepublication 0.1.0 on 2026-10-05; nothing published)
remaining:
  - unverified: the Steam description is the one in About.xml (rewritten 2026-10-04 and 2026-10-05, last changed in `a463f5a`; the English text was confirmed by the owner in chat on 2026-10-07, recorded by a session on her statement); ModIcon.png (128x128, 25 KB, from the owner's cut-out source) and Preview.png (896x504, 482 KB, rendered by Render-Preview.cjs, copy in Art/Gallery/0-preview.png) are installed; the 32 px legibility check of the icon passes for the head, while the two side objects blur, so the owner decides
  - unverified: the extractor (Source/Game/SourceScanner.cs) ran once in a real game, in the WSL install: Pickle request `20261004-000340-546-454b` (English, tree 8e7ecfb, played 2026-10-04 19:23-19:28) 4 scenarios passed, `exitReason: passed`, `Player.log` shows "5 new, 0 changed, 0 removed" for the fixture mod then "0 new, 5 unchanged" on the second scan, with no error line. Not yet proved: a large real mod (many defs, patches, inheritance), and the narrowed walk with many mods loaded (the suite loads Core and the DLC only)
  - defect: the first scan was far too slow (about 72 s for one small mod: the game's walk read every def of every mod before the mod's own were kept). Fixed 2026-10-04 by passing the mod's metadata to `DefInjectionUtility`, which skips other mods' defs before reading a field; a fifth Pickle scenario asserts a scan under 15 s. Proved in French on 2026-10-04: scans took 18-111 ms and the speed scenario passed (414 ms)
  - defect: the three engines have each talked to their real service once; Google Cloud and Anthropic did on 2026-10-05 (claude-haiku-4-5, three short texts, everything intact, after the key was replaced by one scoped to a workspace); the OpenAI-compatible one on 2026-10-05 on 2026-10-05 (api.openai.com, gpt-4.1-nano, three short texts: placeholders, both markers and the colour tag all came back intact); DeepL did, once, on 2026-10-04 (free host, three short texts), see docs/runs/history.md; the page has a test button to try one, not yet pressed; there is no window to translate a mod from, only the developer-menu action
  - unverified: GitHub social preview image (Mod/About/Preview.png exists now; the setting exists only on the web page, the owner uploads it)
  - unverified: Mod/Assemblies/RimBabel.dll is committed from the sources of 2026-10-04 (revision 7cb8a47 plus the protector fix of 2026-10-04); the shipped assembly must match the sources before any upload
  - verified: the Pickle suite (Tests/Pickle, 8 features, 24 scenarios: 01-package 5, 02-settings 7, 03-rimmsqol-shortcut 3, 04-translation 3, 05-gallery 3, 06-08 restart chain 3, one scenario per launch). Final runs of 2026-10-08 on tree 49fe6c6 (`Mod/` and `Source/` unchanged since f523a85): minimal English `20261007-194429-861-079f` and French `20261007-194432-124-c95c`, 24 discovered, 21 passed, 0 failed, 3 skipped (the RIMMSQOL ones); pass `avec-rimmsqol` English `20261007-194430-577-665f` and French `20261007-194432-844-8395`, 3 of 3 passed; evidence `final-en`, `final-fr`, `final-rimmsqol-en`, `final-rimmsqol-fr`. The restart chain, three launches in one hold of the lock, passed on tree 4181851: English `20261005-224000-666-952f`, French `20261005-224002-867-eeff`, evidence `restart-en` and `restart-fr` (`seq1` to `seq3`). 04-translation proves a translation run in a real game against a fake server on the loopback address; it proves no real engine (see docs/runs/history.md). Passes are declared in Tests/Pickle/README.md (minimal English, minimal French, avec-rimmsqol)
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
holds on 2026-10-05: the settings audit is `passed`. Proved in English and French, by Pickle on tree f523a85 and 4181851: the Mod
options route (the game's own click opens the mod's settings window), the page drawing without a logged error, the file written,
the values read back, RIMMSQOL, the engine chosen in the settings driving a translation run (fake server), and a real restart:
three launches under one hold of the lock (`restart-en` `20261005-224000-666-952f`, `restart-fr` `20261005-224002-867-eeff`), the
second reading what the game loaded at startup from the file the first wrote, the third putting everything back.
`preTest -> done` holds on 2026-10-07, tree of the shipped `Mod/` (`Mod/Assemblies/RimBabel.dll` rebuilt from `Source/` with no difference):
the offline tests ran again (379 checks, 0 failed) and so did the checks against the game's assemblies (47, 0 failed). XML tests: the
files of `Mod/` that are XML (`Languages/*/Keyed`, `DefInjected`, `Defs/MainButtonDefs`) are read by `Tests/SettingsTests.cs`
(same keys and `{n}` parameters in both languages, no empty or garbled text, every key the code asks for exists, the shortcut's
declaration and its French injection); there is no patch and no other Def to test. Pickle scenarios are written (eight features, 24
scenarios, perimeter in `Tests/Pickle/README.md` and `TESTING.md`: only what a running game shows) and, beyond what this step asks,
they were played and are green in both languages. Functional scenarios with preconditions, actions and expected results are the
table of `TESTING.md`. Not applicable: tests of the real services (no automated test may call a paid service; each was tried once by hand,
see `docs/runs/history.md`).
`settings_audit` is `passed`. Retained (2026-10-08, after the review corrections of `adab2c7`): `workflow_stage: options`, `stage: showcase`; before them it was `tested` (`options -> l10n`, `l10n -> preTest` and `preTest -> done` held on 2026-10-07, `done -> tested` on 2026-10-08),
(the `stage` code is `showcase` now.)


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
- **Checked in a running game, 2026-10-05** (Pickle, tree 5d31c07, English `20261005-175434-102-019f` and French `20261005-175437-401-821f`,
  12 of 12 played scenarios passed in each): every text of the page exists in the language of the pass, and the `@review`
  capture of the page was read in both languages: no raw key, no clipped line, the longest French texts fit their rows. The
  texts corrected afterwards (the owner's two review rounds, the encoding fix) have no capture yet and change no layout rule.
- **French review by Virginie, 2026-10-05, revision `b219986`: validated**, with one last correction she dictated in the same
  message (`OllamaPreset`: "Préremplir les champs pour Ollama sur cet ordinateur", no final period), applied in the commit that
  follows. Two earlier rounds asked for seven and four corrections, all applied. Recorded here by a session on her statement in
  chat; the review line of `FRENCH_REVIEW.md` is hers and was not touched. Any later change to a French file sets
  `translation_fr` back to `unchecked`.
- **French re-read, 2026-10-05, revision `3111404`: validated** by the owner in chat ("Français correct, encodage propre, révision 3111404 reproductible"); `translation_fr: complete` holds. Recorded by a session on her statement; the review line of `FRENCH_REVIEW.md` was not touched.
- **Review of 2026-10-08, revision `adab2c7`: four corrections applied, to be read again.** The owner's review of the settings texts asked for changes to `EngineDesc` (both languages: "Each service needs its own key, except a server running on your own computer." / "Chaque service demande sa propre clé, sauf un serveur lancé sur votre propre ordinateur."), `ResetDesc` (French "Rétablit tous les réglages de cette page, sauf vos clés."; the English follows it: "Restores every setting on this page, except your keys."), `LanguageDesc` (both languages, as dictated) and `OutputDesc` (French, infinitive register: "Laisser ce champ vide pour utiliser le dossier par défaut, à côté des sauvegardes."; the English keeps its imperative, natural in English, and is unchanged). Applied in `adab2c7`, `FRENCH_REVIEW.md` regenerated at that revision. Consequence by the rule below: `translation_en` and `translation_fr` are `unchecked` until the owner reads the corrected texts; the stage falls back to `options` (the last transition that holds), `stage: showcase`, and the Pickle runs of 2026-10-07/08 (green on the previous texts) are to be replayed on the new revision. The review also states that no text describes a pawn, so the empty `french-review-flags.json` is coherent.
- **Texts that stay in English, by decision of the owner (2026-10-07).** The three entries of the developer menu ("Write a translation package for a mod...", "Translate a mod with the chosen engine...", "Take back the texts of one engine...") and the `[RimBabel]` lines of the game log are developer tooling: the game's own debug menus and log are English, and the menu only exists in development mode. They are not interface for a player and are not translated. Everything a player sees (the settings page, its tooltips, the shortcut) exists in English and French.
- **Known limits, stated rather than hidden.** (1) The engines' error messages are technical English strings built in the
  core (`anthropic answered 401: ...`); the page shows them after "It failed:". Translating them would mean carrying a key
  through the core for every failure. (2) The developer-menu entry `RimBabel > Write a translation package for a mod...` and
  the log lines are developer tooling in English. (3) Product names (DeepL, Anthropic, OpenAI, Ollama...) are not translated.
- **No gender agreement.** No text refers to a pawn, so the neutral o-series and the player choice for gendered French do not
  apply, and no such setting is offered.
- **French review by the owner: validated 2026-10-05 at b219986 (see the Translation audit section).** On 2026-10-04 the owner read the sheet for revision `88cb3d6` and answered "pas validé" with seven corrections (syntax identifiers `K:key` and `D:DefType/defName.field` kept verbatim in the blacklist text, "mod de traduction" everywhere instead of "paquet de traduction", and the wording of `BatchDesc`, `TestDesc`, `DictionaryBad`, `Author`, `OutputDesc`), and confirmed "enregistrée en clair". They are applied; `translation_fr` stays `partial` until she validates the new sheet, which this section never records for her.

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
| Shortcut integration (RIMMSQOL) | **Passed in English and French (2026-10-05, `20261005-060643-651-594a` and `20261005-060647-164-61e0`, pass `avec-rimmsqol`, tree f30cf14, 3 of 3 each, captures read).** RIMMSQOL's own list offers `RimBabel_Settings`, hidden; RIMMSQOL reveals it, the bar draws it (the RimBabel button appears at the right of the bar) and its file records it; the revealed button opens RimBabel's settings; hiding it empties the bar and forgetting the choice leaves nothing. Not covered: clicking RIMMSQOL's checkbox (the shared steps call what it calls) and RIMMSQOL keeping its choice across a restart, which is its own behaviour. Observation: its edit page shows no icon for the shortcut |
| Runtime robustness | **Unverified**: the scenario that lets the page draw for ten frames and then asks for logged errors has not run. An exception in `OnGUI` is logged, not thrown, so only that scenario can show one |
| No settings | Not applicable |

- **Two traps found on the way**, both now covered by a check: the serializer writes the platform's line break (a Windows save
  reads a multi-line dictionary back with CR LF; the text is normalised on load), and the game has no scrolling text area
  (`Widgets.TextAreaScrollable` does not exist in 1.6; the page lays the text out in its own scroll view).

## Gate to `tested` (prepared 2026-10-07, tree 49fe6c6)

`done -> tested` is not claimed. What is established, what is filed, and what waits, against `AUDIT.md` step 9 and the owner's gate of 2026-10-03:

| Criterion | State |
| --- | --- |
| Pickle suites run and green on the final revision | **read 2026-10-08**: minimal English `20261007-194429-861-079f` and French `20261007-194432-124-c95c` (24 scenarios expected, 3 skipped without their pass), evidence `Tests/Pickle/Evidence/final-en` and `final-fr`: `exitReason` passed, set `sans-facultatifs`, 24 discovered, 21 passed, 0 failed, 3 skipped (the RIMMSQOL ones, played in their own pass). `Mod/` and `Source/` unchanged since f523a85. |
| Every `@requires` has had its pass | **read 2026-10-08**: pass `avec-rimmsqol` English `20261007-194430-577-665f` and French `20261007-194432-844-8395` (03-rimmsqol-shortcut, 3 scenarios), evidence `final-rimmsqol-en` and `final-rimmsqol-fr`: `exitReason` passed, set `avec-rimmsqol`, 3 of 3 passed in each language. |
| Scenarios played against scenarios discovered, `exitReason` read first | done: 8 features, 24 scenarios discovered, 21 played in a minimal pass plus the 3 of the RIMMSQOL pass, `exitReason` read first. |
| `@review` captures opened and looked at | done 2026-10-08 on the final runs: the settings page and the Mod options list were opened in English and French (no clipped line, no raw key, the key field masked or empty, French texts complete); the tooltip of the pointer position covers a slider, a known limit of the harness. |
| No scenario in `@wip` | holds (none in the suite). |
| Logs checked, interface in French and in English | the Pickle step "no errors were logged" passed in every scenario; `Player.log` of the four final runs holds 0 `[ERROR]` in the minimal passes and one in each RIMMSQOL pass, which is Pickle Tools reporting that its own RIMMSQOL sub-mod loads no content (not RimBabel, the 3 scenarios pass); the warnings are the game's own (hidden ritual precept, save version, no Steam in WSL). |
| Options, persistence, shortcut | proved (Mod options route, file, restart chain `restart-en` and `restart-fr`, RIMMSQOL). |
| New game and existing save | not relevant: RimBabel writes no save data and changes no gameplay; `test-colony` is loaded by 02-settings. |
| Regression tests after corrections | the whole suite is replayed on the final revision (above). |
| No manual test left to validate | **not applicable, with reasons**: (1) the developer-menu window (click the entry, pick a mod): a developer tool with no player-facing layout, the code under it is run by 01 and 04 (`DevActions.WriteFor`, `PackageBuilder.Translate`); (2) a translation by a real service inside the game: needs a paid key that no automated run may hold, covered by the fake server in 04 and by the one-off live tries in `docs/runs/history.md`; (3) a large real mod: not what this suite proves. |
| Owner items | English description confirmed 2026-10-07; French validated at `3111404`. |

All four runs read on 2026-10-08. `done -> tested` holds.

## Code review (reviewed commit)

Last review: `a3863248d374d1c2459cc4c95d15d054b3d5d4d8` (2026-10-05, whole history up to that commit, tests left out, low effort); three findings, fixed in the commit that follows. The commit is the reference, not a version number. The next review starts from it.

## Publication file (2026-10-08)

`PUBLICATION.md` rewritten against `PUBLISHING.md`: item `381412975321` (private), the CI route, the gallery order and its rules (`0-` copy of the Preview, candidates, 2 MB and 8 MB), the full Steam description (body, `IF I GO QUIET`, `AI-GENERATED`, `THANKS`, attribution line, source link), the register of thanks (Pickle and RIMMSQOL already `posted`, `RimBabel` added to their `Covers`; four translation workshop pages to draft after a fresh read). Open: the owner to confirm the AI-GENERATED wording (the icon being hers), the four comments, the pictures staged on the Sanctuary.
