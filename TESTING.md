# Testing RimBabel

Nothing has been played in game: the in-game part does not exist yet. `STATUS.md` is the source of truth for what has
run and when; this file says what each layer proves and what to keep.

## What runs without the game

The package core (`Source/Core`) is plain C# and its tests compile it in directly, so no RimWorld install is needed:

```bash
dotnet run --project Tests/RimBabel.Tests.csproj
```

43 checks, exit code 1 when one fails; all passed on 2026-10-03. They cover: the source hash ignores the newline
convention; the manifest round-trips special characters, newlines, status, engine, glossary and blacklist, and refuses a
newer schema; the merge keeps unchanged and human text, makes a stale draft of a changed source, never touches a locked
text, reports new and removed keys, and bumps the version only on a change; the writer lays out the package, escapes
line breaks as the game reads them, leaves pending texts out, keeps `PublishedFileId.txt` and human texts across an
update, lists the changelog newest first, adds nothing when nothing moved, and refuses a hand-made language folder, a
path-like language name and a key that is not an XML name.

## What needs the game

The extractor (`Source/Game`) reads a mod's texts through `DefInjectionUtility`. It compiles, but a real list of Defs
only exists in a running game, so what it returns for a real mod is unverified. Its file reading is plain C# and is
tested above (`KeyedXml`: comments and nested elements skipped, backslash-n as a line break, the first load folder
wins a key, empty values dropped, another language not read). Not written yet: the settings page, the shortcut, the
engines' calls. The Pickle
suite under `Tests/Pickle/` does not exist. Gate `preTest -> done` needs it written, with its scope justified, or its
absence justified in this file: the layer that is plain C# is proved above, and what only a running game can show
is the page layout and the real reading of a mod's texts. Engines are exercised with a fake endpoint, not the real
services: no test calls DeepL or Anthropic.

## Passes

Declared when the suite exists. Expected: without optional mods, in English and in French (one pass per language,
`-Language`); with optional mods only if an integration is added. Non-regression passes are filed together at the end,
on the final revision.

## What is not tested here, and why

The behaviour of a vanilla or other mod's Def, left as it is, is not tested: RimBabel does not change it. The game's
own language switching is not tested either: a pass runs in one language.

## Gate to `tested` (owner, 2026-10-03)

Beyond the criteria of `AUDIT.md`, step `done -> tested`, three conditions are checked explicitly and written in
`STATUS.md` before the move:

1. **No scenario left in `@wip`.** Each is repaired and replayed, or deleted with its justification.
2. **Every conditional scenario has run.** Each `@requires:<packageId>` had its pass, on a map that loads that mod, and
   its report was read (suite and scenario names checked, since the report folder is shared by the machine).
3. **No manual test left to validate.** What remained to tick by hand is automated and green, or listed as not
   applicable with the reason. `@review` captures are still opened and read.

## Evidence: what to keep

Evidence stays on disk, never in git (`.gitignore`: `Tests/Pickle/Evidence/`, `docs/runs/evidence/`,
`pickle-reports-archive/`). Per scenario, keep **the latest report for the revision now in the repository**, plus an
older one only if it is the sole proof of a check the latest did not repeat. Delete the rest once a newer report
replaces it. A report built for a superseded revision proves nothing about the current one. Keep `summary.json` and
`junit.xml`; `report.html` and `messages.ndjson` of a stale build are not worth their weight. Captures are kept
minified (reduced JPEG, with the `@review` ones at full size). One line per run goes into `docs/runs/history.md`; no
field of `STATUS.md` may point to a report that has been deleted, so repoint it first.
