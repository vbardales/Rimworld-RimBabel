# Steps of RimBabel

The Pickle steps this repository ships. **Pickle's own steps are in its catalogue:**
[Docs/steps.md](https://github.com/RimWorks/Rimworld-Pickle/blob/main/Docs/steps.md). The generic tools (overlay hiding, rectangle framing, waiting, apparel) are in
[Nelim's Pickle Tools](https://github.com/vbardales/Rimworld-Nelim-Pickle-Tools/blob/main/docs/steps.md). Every step here starts with `RimBabel:` or names RimBabel (see the note on prefixes in `Tests/Pickle/README.md`).

This file is **generated** from the `[Given]`, `[When]` and `[Then]` attributes of `Tests/Pickle/Source/` and the summary above them: do not edit it,
run `docs/Generate-Steps.ps1` (`-Check` verifies that it is current). 51 steps. Pickle matches on the text alone, so a scenario may use
`Given`, `When`, `Then` or `And` as it reads best. The steps of the Sanctuary are in [SanctuaryBacklot/docs/steps.md](https://github.com/vbardales/Rimworld-Nelim-Sanctuary-Backlot/blob/main/docs/steps.md).

| Step | Does |
| --- | --- |
| `RimBabel: the settings hold example values for a picture` (Given) | (no description yet) |
| `RimBabel: the settings page is scrolled down by {int} pixels` (When) | (no description yet) |
| `RimBabel: no package exists yet for the mod {string} in {string}` (Given) | (no description yet) |
| `RimBabel: I write the translation package of the mod {string} for the language {string}` (When) | (no description yet) |
| `RimBabel: the manifest lists {string} as {string}` (Then) | (no description yet) |
| `RimBabel: the manifest does not list {string}` (Then) | (no description yet) |
| `RimBabel: the manifest lists no text of a def outside the mod` (Then) | (no description yet) |
| `RimBabel: the manifest lists no text from the language folders of other mods` (Then) | (no description yet) |
| `RimBabel: every text of the manifest is Pending` (Then) | (no description yet) |
| `RimBabel: the package id is {string}` (Then) | (no description yet) |
| `RimBabel: the package is not publishable yet because {string}` (Then) | (no description yet) |
| `RimBabel: the last write found {int} new, {int} changed and {int} removed texts` (Then) | (no description yet) |
| `RimBabel: the last write took less than {int} seconds` (Then) | (no description yet) |
| `RimBabel: the package version is {string}` (Then) | (no description yet) |
| `RimBabel: the settings are set to the values of the restart test and written` (When) | (no description yet) |
| `RimBabel: the settings file holds the values of the restart test` (Then) | (no description yet) |
| `RimBabel: the values of the restart test are kept for the next launch` (Then) | (no description yet) |
| `RimBabel: the settings the game loaded at startup hold the values of the restart test` (Then) | (no description yet) |
| `RimBabel: the settings are put back to their defaults and written` (When) | (no description yet) |
| `RimBabel: no value of the restart test remains in the settings file` (Then) | (no description yet) |
| `RimBabel: settings are at their defaults` (Given) | (no description yet) |
| `RimBabel's settings shortcut is hidden on a clean configuration` (Then) | (no description yet) |
| `RimBabel's settings shortcut is revealed, as a customization mod would` (When) | (no description yet) |
| `RimBabel's settings shortcut is hidden again` (When) | (no description yet) |
| `RimBabel's settings shortcut is drawn in the bar` (Then) | (no description yet) |
| `RimBabel's settings shortcut is not drawn in the bar` (Then) | (no description yet) |
| `RimBabel's settings shortcut is activated` (When) | (no description yet) |
| `a settings dialog is open for RimBabel` (Then) | (no description yet) |
| `RimBabel: the settings window draws for {int} frames` (When) | (no description yet) |
| `RimBabel: I set the author name to {string}` (When) | (no description yet) |
| `RimBabel: the settings are written` (When) | (no description yet) |
| `RimBabel: the settings file holds {string}` (Then) | (no description yet) |
| `RimBabel: the settings file holds no key` (Then) | (no description yet) |
| `RimBabel: every settings text exists in the language of this pass` (Then) | (no description yet) |
| `RimBabel: the game's options window is opened on Mod options` (When) | (no description yet) |
| `the Mod options list holds RimBabel` (Then) | (no description yet) |
| `RimBabel: RimBabel is chosen in the Mod options list` (When) | (no description yet) |
| `RimBabel: I set the dictionary to {int} lines` (When) | (no description yet) |
| `RimBabel: the file read back by the game holds author {string} and a dictionary of {int} lines` (Then) | (no description yet) |
| `RimBabel: a fake translation server is listening` (Given) | (no description yet) |
| `RimBabel: the engine is the fake server` (Given) | (no description yet) |
| `RimBabel: the blacklist holds {string}` (Given) | (no description yet) |
| `RimBabel: the dictionary holds {string}` (Given) | (no description yet) |
| `RimBabel: I translate the mod {string} into {string} with the chosen engine` (When) | (no description yet) |
| `RimBabel: no package exists yet for the mod {string} in {string} on this run` (Given) | (no description yet) |
| `RimBabel: the engine translated {int} texts and refused none` (Then) | (no description yet) |
| `RimBabel: the text {string} reads {string} and comes from {string}` (Then) | (no description yet) |
| `RimBabel: the text {string} is still pending` (Then) | (no description yet) |
| `RimBabel: a language file of the package holds {string}` (Then) | (no description yet) |
| `RimBabel: the server received {int} requests and no placeholder or glossary term in them` (Then) | (no description yet) |
| `RimBabel: the server received {int} requests` (Then) | (no description yet) |
