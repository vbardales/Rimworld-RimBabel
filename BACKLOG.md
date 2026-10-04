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
- [x] Placeholder protection: tokenise, validate (same tokens, length), retry once alone, else keep the source (`Protector`, `Pipeline`, tested). [ ] Still to do: detect an answer still in the source language, and translate the literal branches of a gender switch.
- [x] Dictionary applied before the engine and handed to it as a glossary; blacklist by key glob or regular expression (`Protector`, `Blacklist`, tested). [ ] Still to do: a dictionary shared across mods (one file, not per package), blacklist by kind of text (colonist names) for the on-the-fly mode, and their screens.
- [x] Settings page under Mod options, hidden MainButtons shortcut, English and French keys.
- [ ] Search across the texts with regular expressions (in-memory index first; a stored index only if it is needed).
- [ ] Generated repositories also carry `.github/` workflows and `publish.config.json` (from `Rimworld-Release-Admin`'s generator) and
  a `FRENCH_REVIEW.md` for French packages.
- [ ] Align the generated `STATUS.md` front matter with the schema of `PUBLISHING.md` / `AUDIT.md`.

## Later

- [ ] On-the-fly translation of the interface (Harmony, a cache by source string, never a blocking call, a blacklist for
  numbers, identifiers and colonist names, the original text on hover).
- [ ] `.po` import and export; a stored index (SQLite) if the in-memory one stops being enough, which means an explicit
  exception to the rule against bundling third-party DLLs.
