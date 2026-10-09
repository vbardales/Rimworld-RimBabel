---
localization: complete
translation_en: complete
translation_fr: complete
mod:          RimBabel
packageId:    nelim.rimbabel
repo:         Rimworld-RimBabel
visibility:   public
detached:     no
stage:        tested
workflow_stage: tested
licence:      original
licence_at:   original work
upstream_mod_remotes: N/A
licence_name: MIT
licence_file: LICENSE (identical copy in Mod/LICENSE)
dependencies: none (verified 2026-10-07: no modDependencies, loadAfter or incompatibleWith in About.xml; Source references only the game through Krafs.Rimworld.Ref, no Harmony, no other mod; RIMMSQOL is an optional customization mod that may reveal the shortcut, nothing in the code names it; no LoadFolders.xml needed for a single 1.6 version)
showcase:     unchecked
settings_audit: passed
tested_on:    2026-10-09 (Pickle final runs on tree 9999e7e, English and French, 24 scenarios per language, 21 played outside their own passes; Source unchanged since f523a85)
workshop:      item 381412975321 (private, created by the owner's prepublication 0.1.0 on 2026-10-05; nothing published)
remaining:
  - verified 2026-10-09: the Steam description (the Markdown block of `PUBLICATION.md`, source of `About.xml`) was read by the owner for this delivery: English confirmed in chat 2026-10-07, her one read of `PUBLICATION.md`, and the later wording corrections approved in chat (recorded by a session on her statement). Earlier note: it was the one in About.xml (rewritten 2026-10-04 and 2026-10-05, last changed in `a463f5a`; the English text was confirmed by the owner in chat on 2026-10-07, recorded by a session on her statement); ModIcon.png (128x128, transparent padding, regenerated 2026-10-08 from the new `Art/ModIcon-source.png`, a DALL-E image cut out by the owner) and Preview.png (896x504, 482 KB, rendered by Render-Preview.cjs, copy in Art/Gallery/0-preview.png) are installed; the 32 px legibility check of the icon passes for the head, while the two side objects blur, so the owner decides
  - unverified: the extractor (Source/Game/SourceScanner.cs) ran once in a real game, in the WSL install: Pickle request `20261004-000340-546-454b` (English, tree 8e7ecfb, played 2026-10-04 19:23-19:28) 4 scenarios passed, `exitReason: passed`, `Player.log` shows "5 new, 0 changed, 0 removed" for the fixture mod then "0 new, 5 unchanged" on the second scan, with no error line. Not yet proved: a large real mod (many defs, patches, inheritance), and the narrowed walk with many mods loaded (the suite loads Core and the DLC only)
  - defect: the first scan was far too slow (about 72 s for one small mod: the game's walk read every def of every mod before the mod's own were kept). Fixed 2026-10-04 by passing the mod's metadata to `DefInjectionUtility`, which skips other mods' defs before reading a field; a fifth Pickle scenario asserts a scan under 15 s. Proved in French on 2026-10-04: scans took 18-111 ms and the speed scenario passed (414 ms)
  - defect: the three engines have each talked to their real service once; Google Cloud and Anthropic did on 2026-10-05 (claude-haiku-4-5, three short texts, everything intact, after the key was replaced by one scoped to a workspace); the OpenAI-compatible one on 2026-10-05 on 2026-10-05 (api.openai.com, gpt-4.1-nano, three short texts: placeholders, both markers and the colour tag all came back intact); DeepL did, once, on 2026-10-04 (free host, three short texts), see docs/runs/history.md; the page has a test button to try one, not yet pressed; there is no window to translate a mod from, only the developer-menu action
  - unverified: GitHub social preview image (Mod/About/Preview.png exists now; the setting exists only on the web page, the owner uploads it)
  - verified 2026-10-09: Mod/Assemblies/RimBabel.dll rebuilt from Source/ (Release, no incremental) is byte-identical to the committed one
  - verified: the Pickle suite (Tests/Pickle, 8 features, 24 scenarios: 01-package 5, 02-settings 7, 03-rimmsqol-shortcut 3, 04-translation 3, 05-gallery 3, 06-08 restart chain 3, one scenario per launch). Final runs of 2026-10-09 on tree 9999e7e (the owner-validated texts of adab2c7, the new icon; the `Source/` unchanged since f523a85): minimal English `20261008-132558-965-7143` and French `20261008-132600-696-2bf9`, 24 discovered, 18 passed, 0 failed, 6 skipped (3 RIMMSQOL and 3 gallery, each played in its own pass); pass `avec-rimmsqol` English `20261008-132559-605-9c54` and French `20261008-132601-597-59cc`, 3 of 3 passed; gallery pass `sanctuary` (save Nelims-tribe, place window-backdrop-for-height) English `20261008-142709-921-dd6d` and French `20261008-131408-231-64e7`, 3 of 3 passed; evidence `final2-en`, `final2-fr`, `final2-rimmsqol-en`, `final2-rimmsqol-fr`, `gallery-en-b`, `gallery-fr`. The first English gallery attempt `…-432a` was ended by the launcher before any scenario (no report, no proof). The restart chain, three launches in one hold of the lock, passed on tree 4181851 (`Source/` unchanged since): English `20261005-224000-666-952f`, French `20261005-224002-867-eeff`, evidence `restart-en` and `restart-fr` (`seq1` to `seq3`). 04-translation proves a translation run in a real game against a fake server on the loopback address; it proves no real engine (see docs/runs/history.md). Passes are declared in Tests/Pickle/README.md (minimal English, minimal French, avec-rimmsqol, sanctuary)
  - unverified: licence of the generated translation packages - a package is a derivative of its source mod, so the generator refuses to call one publishable while the source licence is unknown (PackageWriter.Blockers)
code_review_sha: b297ec08a7bc60428e1b25add17a0a9fd76f09fa
publication_changelog_review_sha: 8392baf14f5dbe7a61ee36fec852eb306046cf61
session:      local_ac6b8630-0142-49f6-bec9-a501e9012332
updated:      2026-10-09, STATUS trimmed to the current state; gallery and publication drafts done; nothing published
protocols_read_sha: e3c3e3d3c78df91838c4fdec8e4ce5f8270d078c
---

# RimBabel - status

Current state only. The history of earlier decisions is in `git log STATUS.md`; run history is in `docs/runs/history.md`.

## Where it stands (2026-10-09)

`workflow_stage: tested`. Nothing is published; Workshop item `381412975321` is private. v1 scope (owner, 2026-10-05): the developer menu plus
the settings page; a translation window, regular-expression search and hover original are out of v1 (`BACKLOG.md`). Next transition:
`tested -> prepublished` (`AUDIT.md` step 10). Open for it: the icon at 32 px (her call),
the GitHub social preview (web upload), `echo_review`; mine: the `## [1.0.0]` section of `CHANGELOG.md` and the dry-run on the exact SHA, on the
day of the deploy. First public version: `1.0.0`. Draft note and thanks comments: `PUBLICATION.md`.

## Where the work is

- `Source/Core/` is the package core, plain C# with no game type: `Entry`, `Manifest` (XML manifest, source of truth), `Merge`, `PackageWriter`,
  `Pipeline`, `Protector`, the engines. `Tests/` runs 379 offline checks (`dotnet run`, see `TESTING.md`).
- `Source/Game/` is the in-game layer: `SourceScanner`, `PackageBuilder` (`Prepare` on the main thread, `Run` off it), the developer-menu
  actions, the settings page and its hidden MainButtons shortcut.

## Translation audit

The mod's own interface is the settings page and the hidden shortcut; no gizmo, letter, alert or thought.

- **Texts.** English `Mod/Languages/English/Keyed/RimBabel.xml`; French `Mod/Languages/French/Keyed/RimBabel.xml` (same keys) and
  `DefInjected/MainButtonDef/RimBabel.xml` (shortcut label and description). Offline checks (`Tests/SettingsTests.cs`): same keys and `{n}`
  parameters in both languages, no empty text, no `(e)` form, every asked key exists, `.One`/`.Many` forms, shortcut hidden by default.
- **French:** validated by the owner at `adab2c7` (four corrections applied, then her validation in chat; recorded by a session on her
  statement, the review line of `FRENCH_REVIEW.md` is hers and untouched). French files unchanged since.
- **English polish, 2026-10-09 (`9c7f86d`, `a6a5637`):** `DictionaryDesc` and `DictionaryBad.One/Many`, then `EmailDesc` and
  `DictionaryBad`, reworded on a review suggestion the owner approved in chat. `translation_en` is `complete` again: the owner read the changed English texts and validated them in chat on 2026-10-09, revision 25546ae (recorded by a session on her statement; the review line of `FRENCH_REVIEW.md` was not touched), by the rule of protocols
  5286cad (a text edit after validation sends nothing back and needs no full validation pass, but her review of the changed texts stays a gate BEFORE the fail-fast deploy); the Pickle non-regression replay (English
  and French, scenarios that show the changed texts) is owed AFTER the fail-fast deploy; record its verdict here and in `docs/runs/`.
  `FRENCH_REVIEW.md` regenerated with the shared `scripts/Make-FrenchReview.ps1`; no flags file kept (an empty one is redundant).
- **Texts that stay in English (owner, 2026-10-07):** the three developer-menu entries and the `[RimBabel]` log lines are developer
  tooling. Everything a player sees exists in English and French.
- **Known limits:** engine error messages are technical English strings built in the core; product names are not translated.
- **No gender agreement:** no text refers to a pawn, so no `GENDER` branch.

## Settings audit

**`settings_audit: passed`** (audit of 2026-10-04, passed 2026-10-05, replayed 2026-10-09).

- **Options.** Engine (seven: DeepL, Anthropic, OpenAI-compatible, Google Cloud, LibreTranslate, MyMemory, Yandex), key, model, address, texts per
  request, test button, author, target language, output folder, dictionary, blacklist, reset. Each exists for a decision the player makes.
- **Access.** Primary: Mod options -> RimBabel. Shortcut: `MainButtonDef` `RimBabel_Settings`, `buttonVisible=false`, worker opening
  `Dialog_ModSettings` for the same mod. No customization mod required.
- **Scope.** Global, applied immediately, written when the window closes; keys are plain text in the game's settings file (the page says so)
  and never in a manifest or package.
- **Proved** (offline and by Pickle in English and French): defaults, Mod options route, page drawing without a logged error, file written and read
  back, engine choice driving a translation run (fake server), restart chain (three launches under one lock), RIMMSQOL shortcut. Input validation,
  reset and older files covered offline. Not covered: RIMMSQOL keeping its own choice across a restart (its behaviour).

## Gate to `tested` (held 2026-10-09, tree 9999e7e)

Pickle suite: 8 features, 24 scenarios (`Tests/Pickle/README.md`, `TESTING.md`). Minimal English `20261008-132558-965-7143` and French
`20261008-132600-696-2bf9`: 18 passed, 0 failed, 6 skipped (3 RIMMSQOL, 3 gallery, each played in its own pass). Pass `avec-rimmsqol`:
`20261008-132559-605-9c54`, `20261008-132601-597-59cc` (3 of 3). Gallery pass `sanctuary` (save Nelims-tribe, place
`window-backdrop-for-height`): `20261008-142709-921-dd6d` (English), `20261008-131408-231-64e7` (French), 3 of 3. Restart chain on 4181851:
`20261005-224000-666-952f`, `20261005-224002-867-eeff`. `exitReason` read first; `@review` captures read; evidence `final2-*`, `gallery-*`,
`restart-*`. 04-translation proves a run against a fake server on loopback, no real engine (see `docs/runs/history.md`). Not applicable, with
reasons: the developer-menu window (developer tool), a real paid service in game (one-off live tries recorded in history), a large real mod.

## Remaining

- Verified 2026-10-09: `Mod/Assemblies/RimBabel.dll` rebuilt from `Source/` is byte-identical to the committed one. Code review of
  `a386324..HEAD` (low effort, Pipeline, Protector, PackageBuilder, RimBabelMod): no finding; `code_review_sha` = reviewed HEAD.
- `publication_changelog_review_sha`: recorded on the owner's statement that she read both files once and will not re-read them for this
  delivery; the sha is HEAD when she confirmed (2026-10-09, chat) that the one read of this major delivery stands for the later corrections too (drafts, 1.0.0 note, external review fixes); not a commit she named. `PUBLICATION.md` changed since (drafts, note, gallery), not re-read.
- Unverified: a large real mod and the narrowed walk with many mods loaded (the suite loads Core and the DLC only); a first scan in a real
  game was fast (18-111 ms; a fifth scenario asserts under 15 s); the three engines each answered a real service once (Google Cloud and
  Anthropic 2026-10-05, OpenAI-compatible 2026-10-05, DeepL 2026-10-04); LibreTranslate and Yandex only offline; the page's test button not yet pressed.
- Unverified: licence of generated translation packages (a package derives from its source mod; `PackageWriter.Blockers`).

## Gallery and publication

`Art/Gallery/`: `0-preview.png` (copy of `Mod/About/Preview.png`), `1-engines.png`, `2-dictionary-and-blacklist.png` (cropped to 5 px each
side, owner-accepted 2026-10-09, indexes contiguous). Icon: DALL-E source `Art/ModIcon-source.png`, cut out by the owner; 32 px check: the head
reads, the two side objects blur (her override). Preview background is DALL-E, stated in the description. `PUBLICATION.md` follows
`PUBLISHING.md`: CI route, gallery order, full Steam description, adult-content questionnaire answered `No`, four thanks comments (plus two optional)
drafted from the owner's experience, none posted before the item is public.
