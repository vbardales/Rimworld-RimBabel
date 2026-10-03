# Changelog

Format inspired by [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This file serves the repository and the writing of Steam patch notes; RimWorld does not display it in game.

## [Unreleased]

Nothing has been uploaded. There is no `About/PublishedFileId.txt`, so there is no `0.1.0` entry yet: that entry is
written once the Workshop item exists ("creation of a publishId file") and says what the upload contained.

### Added

- The package core, outside the game: the entry model with a source hash and a status, the XML manifest
  (`Mod/Babel/manifest.xml`) as the source of truth, the merge that brings a package up to date with its source mod, and
  the writer that produces a repository ready to commit (About, Languages, manifest, README, CHANGELOG, ATTRIBUTION,
  STATUS, LICENCE).
- Reading a loaded mod's texts in game: its Keyed files and every Def string the game itself would let a translation
  inject, through a developer-menu action that writes the package for the active language. Developer tooling for now;
  no window yet.
- Placeholder and glossary protection around machine translation: placeholders, whole gender switches, rich-text tags and
  markers are hidden from the engine and checked on the way back; a translation that lost one is refused and the text
  stays as the source. A glossary of required translations, a blacklist (glob or regular expression), a pipeline that
  translates only pending and stale texts and never touches human, reviewed or locked ones, and an importer that brings an
  existing translation in as human work. No engine is connected yet.
- 105 offline checks of the package core, the Keyed reader and the translation machinery, and 32 checks of the extractor against the game's own
  assemblies (hand-built Defs in the real `DefDatabase`, walked by the game's `DefInjectionUtility`).
