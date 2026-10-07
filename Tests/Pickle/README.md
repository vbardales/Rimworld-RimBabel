# The Pickle suite for RimBabel

Eight feature files, 24 scenarios, written from 2026-10-03 to 2026-10-07; the runs and their evidence are in `STATUS.md` and `docs/runs/history.md`. The suite holds what only a running game can show; everything provable
outside one is proven outside one (`Tests/RimBabel.Tests.csproj` for the package core, `Tests/Game` for the extractor against
the game's assemblies). A scenario that restated those would take the machine for nothing.

## What is here, and why a game

| Feature | What a person would do by hand | Why a running game |
| --- | --- | --- |
| 01 package (5 scenarios) | Load a mod, open the developer menu, run `RimBabel > Write a translation package for a mod...`, pick the mod, read the manifest, and notice it took a minute | The Defs are loaded **by the game from XML** (inheritance, `ModContentPack` assignment, the real `DefDatabase`), and the Keyed files are read from the load folders the game computed. `Tests/Game` builds its Defs by hand; this is the only place the whole chain is shown |

The step calls `DevActions.WriteFor`, the method the menu entry calls once a mod is picked, so everything under the menu is the
real code. **The menu window itself is not covered**: it is a developer tool with no player-facing layout. When the settings
window and its hidden MainButtons shortcut exist (BACKLOG.md), they get their own features here, with `@review` captures.

## The source mod is the test companion

`Mod/` is the companion mod (`RimBabel - Pickle tests`, `nelim.rimbabel.pickletests`) **and the mod the scenarios translate**:
it carries two Defs (`Mod/Defs/Fixture.xml`) and a Keyed file (`Mod/Languages/English/Keyed/RimBabelFixture.xml`) whose texts
are known, so what RimBabel lists is checked text by text. No second mod, no `wsl-deps` map: the minimal pass is enough.
Adding a text to the fixture means adding a line to `01-package.feature`.
`02-settings.feature` (5 scenarios, needs the save `test-colony`): hidden shortcut contract, dialog belongs to this mod, page draws without a logged error, settings file written, every text present in the pass language, one `@review` screenshot. Steps in `Source/SettingsSteps.cs`. Run it in English and in French.

`03-rimmsqol-shortcut.feature` (3, pass `avec-rimmsqol`, `wsl-deps.avec-rimmsqol.map`): the shortcut seen from RIMMSQOL's own list.
`04-translation.feature` (3): a translation run in a real game against a fake server on `127.0.0.1:18765` (the steps of `TranslationSteps.cs`).
`05-gallery.feature` (3, `@review`): pictures of Mod options and the page, with example values and a masked placeholder key (`GallerySteps.cs`).
`06-restart-write`, `07-restart-read`, `08-restart-reset` (one scenario each): the settings across a real restart, three launches under one hold of the lock (`RestartSteps.cs`). In a full pass they run in one process and prove only that the steps hold together; only the chain proves the restart:

```powershell
Submit-PickleRun.ps1 -Mod RimBabel -Owner local_<id> -Label "restart, <sha>" -NoBatch -Language English `
  -EvidenceDir RimBabel/Tests/Pickle/Evidence/restart-en -Filter '06-restart-write' -Then '07-restart-read','08-restart-reset'
```

`-EvidenceDir` is always a full path under the repository, never a bare name (a bare name lands at the root of `rimworld`).

## Passes

| Pass | Command | What it establishes |
| --- | --- | --- |
| Minimal, English | `-Mod RimBabel` and no `-DepMap` | The extractor and the package writer on a loaded mod, in a game running in English |
| Minimal, French | the same with `-Language French` | The French package is the same whichever language the game runs in (the package language is a setting, not the game's language) |

No optional integration exists, so there is **no pass with optional mods**; no mod is declared incompatible; no DLC is involved.
The scenarios run at the main menu (no save), about a second each. They are small: one request per pass, filed through the
Ticket Dispatcher (`Submit-PickleRun.ps1`, see `docs/SUBMIT.md` there), with the SHA in `-Label`, and `-EvidenceDir
Tests/Pickle/Evidence/<run>` (gitignored; what to keep is in `TESTING.md`).

```powershell
powershell.exe -ExecutionPolicy Bypass -File C:\Users\nelim\Documents\rimworld\Rimworld-Ticket-Dispatcher\scripts\Submit-PickleRun.ps1 `
  -Mod RimBabel -Owner local_<session id> -Label "01-package, English, <sha>" `
  -EvidenceDir RimBabel/Tests/Pickle/Evidence/suite-en
```

The filter that names the suite is the display name `RimBabel - Pickle tests`, not the folder.

## Before a request

- Build the steps (`dotnet build Tests/Pickle/Source/RimBabel.PickleSteps.csproj -c Release`): Pickle loads step DLLs when the game starts.
- `powershell.exe -ExecutionPolicy Bypass -File Tests/Pickle/Check-Steps.ps1 -PickleAssemblies <the installed Pickle's Assemblies folder>`
  compiles every step expression with Pickle's own engine and checks that none is ambiguous against any other suite: an
  invalid pattern costs a whole run, not one scenario. The folder is on the WSL side (`~/rimworld/Mods/3791648678/1.6/Assemblies`);
  copy it to Windows to run the script.
- Keep the working tree on the revision under test until `RUN_DONE`: a request carries no SHA.

## What stays manual

The developer-menu window (click the entry, pick the mod): a developer tool, not covered. The extractor on a **large real mod**
(many Defs, patches, inheritance across mods) is not what this suite proves: it proves the chain on a small known one.
