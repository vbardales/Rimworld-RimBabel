# Steps of RimBabel

The Pickle steps this repository ships. **Pickle's own steps are in its catalogue:**
[Docs/steps.md](https://github.com/RimWorks/Rimworld-Pickle/blob/main/Docs/steps.md). The generic tools (overlay hiding, rectangle framing, waiting, apparel) are in
[Nelim's Pickle Tools](https://github.com/vbardales/Rimworld-Nelim-Pickle-Tools/blob/main/docs/steps.md). Every step here starts with `RimBabel:` or names RimBabel (see the note on prefixes in `Tests/Pickle/README.md`).

This file is **generated** from the `[Given]`, `[When]` and `[Then]` attributes of `Tests/Pickle/Source/` and the summary above them: do not edit it,
run `scripts/Generate-Steps.ps1 -ModRoot RimBabel` of the protocols repository (`-Check` verifies that it is current). {count} steps. Pickle matches on the text alone, so a scenario may use
`Given`, `When`, `Then` or `And` as it reads best. The steps of the Sanctuary are in [SanctuaryBacklot/docs/steps.md](https://github.com/vbardales/Rimworld-Nelim-Sanctuary-Backlot/blob/main/docs/steps.md).
