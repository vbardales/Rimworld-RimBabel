# Changelog

Format inspired by [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This file serves the repository and the writing of Steam patch notes; RimWorld does not display it in game.

## [Unreleased]

### Fixed

- A translation that comes back identical to a multi-word source (an engine that handed the text back, a service that did not detect the language) is now refused and the text stays pending, instead of being stored as a machine translation. A one-word text is let through, since a name may be the same in both languages.


## [1.0.0] - 2026-10-10

First public version. The Workshop item existed since 0.1.0 (private).

### Added

- Four more engines, Google Cloud, LibreTranslate, MyMemory and Yandex, join DeepL, Anthropic and any OpenAI-compatible server.
- Every text records which engine produced it, so one engine's output can be removed without disturbing the rest.
- The description says how to use the mod in version 1 (the developer menu, step by step), with the thanks, the AI disclosure and the "if I go quiet" notice.
- Gallery images, a Workshop preview with the mod icon, and the repository social preview.
- The Steam publishing workflow of the Rimworld-Release-Admin protocols (`.github/`), with a dry-run before every publish.

### Changed

- English texts polished (dictionary and e-mail descriptions, dictionary error messages); the French settings texts follow the owner's review.
- 379 offline checks, and an in-game Pickle suite of 8 features and 24 scenarios, all green.

## [0.1.0] - 2026-10-05

Creation of a publishId file: the first upload, made by the owner to create the Workshop item (private, as Steam creates
them all). It contained `Mod/` as it stood at commit `5d31c07`. What it
holds is listed below.

### Added

- The package core, outside the game: the entry model with a source hash and a status, the XML manifest
  (`Mod/Babel/manifest.xml`) as the source of truth, the merge that brings a package up to date with its source mod, and
  the writer that produces a repository ready to commit (About, Languages, manifest, README, CHANGELOG, ATTRIBUTION,
  STATUS, LICENCE).
- Reading a loaded mod's texts in game: its Keyed files and every Def string the game itself would let a translation
  inject, through a developer-menu action that writes the package for the active language. Developer tooling for now;
  no window yet.
- Settings page under Mod options: engine (DeepL, Anthropic, OpenAI-compatible) with keys, a test button, packages (author, language, output folder), dictionary and blacklist as text lines, reset. Optional MainButtons shortcut, hidden by default. English and French texts.
- Placeholder and glossary protection around machine translation: placeholders, whole gender switches, rich-text tags and
  markers are hidden from the engine and checked on the way back; a translation that lost one is refused and the text
  stays as the source. A glossary of required translations, a blacklist (glob or regular expression), a pipeline that
  translates only pending and stale texts and never touches human, reviewed or locked ones, and an importer that brings an
  existing translation in as human work. Engines for DeepL, Anthropic and any OpenAI-compatible endpoint, with a small JSON reader and writer so that no library is needed; tested against a fake network only, and not reachable from the game yet.
- A scan of one mod no longer walks the whole game (about 72 s before, 18-111 ms after, measured in the WSL install).
- 304 offline checks of the package core, the Keyed reader, the translation machinery and the engines, and 47 checks of the extractor and the settings round trip against the game's own
  assemblies (hand-built Defs in the real `DefDatabase`, walked by the game's `DefInjectionUtility`).
