# The Pickle suite for RimBabel

One feature file, written 2026-10-03. Passes of 2026-10-04: English 4 of 4 (older tree), French 5 of 5 (tree with the speed fix), and the speed scenario alone in English. It holds what only a running game can show; everything provable
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
  -EvidenceDir RimBabel/Tests/Pickle/Evidence/package-en
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
