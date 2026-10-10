---
localization: complete
translation_en: complete
translation_fr: complete
mod:          RimBabel
packageId:    nelim.rimbabel
repo:         Rimworld-RimBabel
visibility:   public
detached:     no
workflow_stage: dormant
licence:      original
licence_at:   original work
upstream_mod_remotes: N/A
licence_name: MIT
licence_file: LICENSE (identical copy in Mod/LICENSE)
dependencies: none (verified 2026-10-07: no modDependencies, loadAfter or incompatibleWith in About.xml; Source references only the game through Krafs.Rimworld.Ref, no Harmony, no other mod; RIMMSQOL is an optional customization mod that may reveal the shortcut, nothing in the code names it; no LoadFolders.xml needed for a single 1.6 version)
showcase:     unchecked
settings_audit: passed
tested_on:    2026-10-09 (Pickle final runs on tree 9999e7e, English and French, 24 scenarios per language, 21 played outside their own passes; Source unchanged since f523a85)
workshop:      item 3814129753 (public since 2026-10-10; 1.0.0 sent by CI run 38041633893, tag v1.0.0)
echo_review_sha: 161e5fb24cd2d72e40d1517acb7b260c25f8735d
social_preview_sha256: 7c28a6e4c1bbd0f8585f47866ac60b98043a649e958a5b11773d938b0e1c823b
# echo decision, 2026-10-10: keep (redone 2026-10-09) - the echo was drawn at 16 % in the gold ink (near invisible); now the accent colour at full opacity, owner validated the render. Final render after the shared renderer fixes (icon flush with the corner, panel echo at z-index 1): observed, the echo is invisible at z-index -1 and visible at 1 with the text above it (mechanism not established)
remaining:
  - unverified: a large real mod (many defs, patches, inheritance) and the narrowed walk with many mods loaded; the suite loads Core and the DLC only
  - unverified: LibreTranslate and Yandex only offline; DeepL, Anthropic, OpenAI-compatible and Google Cloud answered a real service once each (docs/runs/history.md); the page's test button was never pressed
  - unverified: licence of generated translation packages (a package derives from its source mod; `PackageWriter.Blockers`)
  - open: the published `About.xml` lacks the Workshop links added to THANKS on 2026-10-10 (the Steam page description was pasted by hand and has them); the next CI publish fixes it
  - open: the four thanks comments were posted without a session having read the comment threads
  - note: there is no translation window in v1, only the developer-menu action and the settings page (`BACKLOG.md`)
code_review_sha: a1e58b2a7487d9acf2459ba215659fdaf7750784
publication_changelog_review_sha: 5bec5c69e09b38b48829930baf0f1a817dad921f
session:      local_ac6b8630-0142-49f6-bec9-a501e9012332
updated:      2026-10-10, dormant: 1.0.0 published and public, cleanup done
protocols_read_sha: 9e53ad19092521ae81c488e37d06716b159dd78b
---

# RimBabel - status

Current state only. The history of earlier decisions is in `git log STATUS.md`; run history is in `docs/runs/history.md`.

## Where it stands

`workflow_stage: dormant`. Version 1.0.0 was sent to Steam on 2026-10-10 (CI run 38041633893, SHA f7c9c92f656a5ba0d1a36edd25d355b4a377c998, manifest 394049572684504774); the CI created tag `v1.0.0` and the release. The item `3814129753` is public and the page was validated by the owner on 2026-10-10 (chat). The rollback target is tag `v1.0.0`. v1 scope: the developer menu plus the settings page; a translation window, regular-expression search and hover original are out of v1 (`BACKLOG.md`).

- **Code:** `Source/Core/` is the package core (plain C#, no game type): `Entry`, `Manifest`, `Merge`, `PackageWriter`, `Pipeline`, `Protector`, the seven engines. `Source/Game/` is the in-game layer: `SourceScanner`, `PackageBuilder`, developer-menu actions, settings page, hidden MainButtons shortcut. 379 offline checks (`TESTING.md`); `Mod/Assemblies/RimBabel.dll` rebuilt from `Source/` is byte-identical (also on the CI runner).
- **Tests:** Pickle suite, 8 features, 24 scenarios, green in English and French on tree 9999e7e (`Source/` unchanged since); non-regression replay of the changed texts after the send, green in both languages (`docs/runs/history.md`, evidence `final2-*` and `nonreg-1.0.0-*`). Code review recorded in `code_review_sha`.
- **Texts:** English and French complete, both validated by the owner (French at `adab2c7`, English polish read in chat 2026-10-09). The mod's interface is the settings page and the hidden shortcut; the three developer-menu entries and the `[RimBabel]` log lines stay in English (owner, 2026-10-07). No gender agreement: no text refers to a pawn.
- **Settings audit `passed`:** seven engines, key, model, address, texts per request, test button, author, target language, output folder, dictionary, blacklist, reset; access by Mod options -> RimBabel, or the shortcut `RimBabel_Settings` (hidden by default); global scope, keys in the game's settings file in plain text (the page says so).
- **Dependencies:** none (no modDependencies, loadAfter or incompatibleWith; `Krafs.Rimworld.Ref` only; no Harmony). RIMMSQOL is an optional customization mod that may reveal the shortcut.
- **Gallery and page:** `Art/Gallery/` holds `0-preview.png`, `1-engines.png`, `2-dictionary-and-blacklist.png` (accepted, cropped 5 px each side). Icon source `Art/ModIcon-source.png` (DALL-E, cut out by the owner), preview background DALL-E (stated in the description). Social preview uploaded by the owner and checked byte for byte (`social_preview_sha256`). Adult-content questionnaire answered `No` on the page.
- **Comments:** the four thanks comments are posted (`WORKSHOP_COMMENTS.md`); two optional drafts stay unposted.
- **Reviews:** `publication_changelog_review_sha` recorded for the 1.0 line (owner, chat 2026-10-10).

## Cleanup of 2026-10-10 (AUDIT 14.c)

Dated sections of this file folded into `docs/runs/history.md`; `PROTOCOLS-READ.md` (a manifest of 2026-10-03, replaced by `protocols_read_sha`) deleted; posted drafts and the sent note moved out of `PUBLICATION.md`; superseded Pickle evidence (`gallery-*`, `restart-*`) deleted, kept `final2-*` and `nonreg-1.0.0-*`; no branch other than `main` (14.d, nothing to delete). WSL (14.c): the cache `~/workshop-cache` holds 25 entries (409 MB) and none of the Workshop ids named by this mod's maps (`wsl-deps.avec-rimmsqol.map`, `wsl-deps.sanctuary.map`: 1084452457, 3798082132, 3799726535, 3527486510, 2986402536, 1635901197, 2581693737, 2816938779, 2889716301, 3745223213, 3790129900, 3753978140, 3255379190), which come from the Windows Workshop folder; nothing of this mod's sessions to remove, nothing removed.
