# Run history

One text line per run. Raw evidence stays on disk only and is ignored by git. No Pickle run exists yet.

- 2026-10-03: `dotnet run` on `Tests/RimBabel.Tests.csproj` (package core, outside the game): 36 checks, 0 failed.
- 2026-10-03: same, with the Keyed reader added: 43 checks, 0 failed. `dotnet build Source/RimBabel.csproj -c Release`: 0 errors, 0 warnings. Nothing run in game.
- 2026-10-03: `Tests/Game/RimBabel.GameTests.exe` (shipped RimBabel.dll against the real Assembly-CSharp, no game running): 32 checks, 0 failed; the mutation "remove the space rule of `IsText`" makes 1 fail.
