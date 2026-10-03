# Run history

One text line per run. Raw evidence stays on disk only and is ignored by git. No Pickle run exists yet.

- 2026-10-03: `dotnet run` on `Tests/RimBabel.Tests.csproj` (package core, outside the game): 36 checks, 0 failed.
- 2026-10-03: same, with the Keyed reader added: 43 checks, 0 failed. `dotnet build Source/RimBabel.csproj -c Release`: 0 errors, 0 warnings. Nothing run in game.
- 2026-10-03: `Tests/Game/RimBabel.GameTests.exe` (shipped RimBabel.dll against the real Assembly-CSharp, no game running): 32 checks, 0 failed; the mutation "remove the space rule of `IsText`" makes 1 fail.
- 2026-10-03: `Tests/Pickle/Check-Steps.ps1` against the installed Pickle (WSL copy, 1.6): 11 patterns compile, none ambiguous against 2242 others (234 Pickle, 2008 from 77 sources), all 24 step lines of the suite resolve. No Pickle run: the suite has never been played.
- 2026-10-04: two Pickle requests filed for revision 7d0cc42, minimal pass, no DepMap: `20261004-000340-546-454b` (English, evidence `Tests/Pickle/Evidence/package-en`) and `20261004-000341-508-ad5b` (French, `package-fr`), 4 scenarios each. The worker was paused by the owner (owner holds the launcher and the Windows game) when they were filed. No report yet.
