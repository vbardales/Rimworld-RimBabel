# Backlog - RimBabel

> v1 scope (owner, 2026-10-05): the developer menu plus the settings page. Everything unticked below is after v1.

This mod's own backlog, not the monorepo's. Opened 2026-10-03.

## Before `horsMonoRepo`

- [ ] Create the GitHub repository and agree its name, add the remote, push the first commit (the owner's call).

## Version 1, offline

- [x] Package core: entry, manifest, merge, package writer, offline tests.
- [x] Read a mod's texts in game: `DefInjectionUtility` for Defs, `Languages/English/Keyed` for Keyed (written and
  compiled, **not run in a game yet**).
- [ ] Run the extractor on a real mod and read what it lists; tune `SourceScanner.IsText` (single-word fields other
  than `label` are missed today).
- [ ] Source languages other than English; English DefInjected files that override a Def.
- [x] Import an existing translation pack into a package, as human work an engine never overwrites (`Importer`, tested). [ ] Still to do: check the licence of the pack before it is redistributed, and wire it to a window or action.
- [x] Engines written and tested against a fake network: DeepL (free and paid hosts, tokens sent as XML tags), Anthropic (Messages API) and any OpenAI-compatible endpoint (Ollama, LM Studio, OpenRouter...). [ ] Still to do: try each against its real service (needs keys: the owner's), a free no-key fallback (MyMemory or LibreTranslate), DeepL glossary ids, and the settings that hold the keys (never in a package or a repository).
- [x] Placeholder protection: tokenise, validate (same tokens, length), retry once alone, else keep the source (`Protector`, `Pipeline`, tested). [ ] Done in 1.0.1: an answer equal to a multi-word source is refused. Still to do: translate the literal branches of a gender switch.
- [x] Dictionary applied before the engine and handed to it as a glossary; blacklist by key glob or regular expression (`Protector`, `Blacklist`, tested). [ ] Still to do: a dictionary shared across mods (one file, not per package), blacklist by kind of text (colonist names) for the on-the-fly mode, and their screens.
- [x] Settings page under Mod options, hidden MainButtons shortcut, English and French keys.
- [ ] Search across the texts with regular expressions (in-memory index first; a stored index only if it is needed).
- [ ] Generated repositories also carry `.github/` workflows and `publish.config.json` (from `Rimworld-Release-Admin`'s generator) and
  a `FRENCH_REVIEW.md` for French packages.
- [ ] Align the generated `STATUS.md` front matter with the schema of `PUBLISHING.md` / `AUDIT.md`.
- [ ] Choose how packages are cut: one translation mod per source mod (today), or one big translation mod holding the texts of
  every translated mod, per language. The manifest would then group its entries by source mod; the choice is a setting, and
  a package of the first kind can be merged into the second. Open points: the big mod needs a load-order and
  optional-dependency story (a text of a mod that is not installed must be harmless), and a text is blamed on its source mod
  for licence and attribution, so the big one carries every source licence.
- [ ] A free-pack button: import a translation already installed for the mod being translated (the `Importer` exists, the
  button does not), and a shared library of community translations is a larger question left open.
- [ ] Try LibreTranslate and Yandex against real services (only the request shape is tested offline; Google Cloud was tried 2026-10-05), and the
  new translation entry of the developer menu in a real game.

## Later

- [ ] On-the-fly translation of the interface (Harmony, a cache by source string, never a blocking call, a blacklist for
  numbers, identifiers and colonist names, the original text on hover).
- [ ] Show the original text on hover over a translated text, in the interface and in the translation window, so that a player
  can check what a machine translation replaced. Out of version 1 (owner's decision, 2026-10-05). When it exists: a Pickle
  scenario that hovers a translated text and a capture proving the tooltip shows the source language, not a key or the
  translation itself; Pickle has no step to place the pointer, so that step comes first.
- [ ] `.po` import and export; a stored index (SQLite) if the in-memory one stops being enough, which means an explicit
  exception to the rule against bundling third-party DLLs.

- [ ] Merge the translation mods a player already has installed (several authors, many small mods to load) into one, once they
  are translated: a step after translation, not before. A translation is a derivative of its author's work, so the licence of
  each pack is checked before anything is redistributed (see the free-pack button above and the big-package choice).
- [ ] A translation window in the game, usable on the Steam Deck: in Big Picture mode the player has no file access, so a tool
  that sends the player to an external editor or a folder cannot be used there. The window must run everything in game (pick
  the mod, translate, review, lock), and the output folder must be reachable without a file manager. Owner, 2026-10-09. Today
  the developer menu is the only entry and it needs development mode.
