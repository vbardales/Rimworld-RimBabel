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
- 36 offline checks of that core.
