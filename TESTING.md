# Testing RimBabel

The three layers below all ran on the tree of the shipped `Mod/`. `STATUS.md` is the source of truth for what has
run and when; this file says what each layer proves and what to keep.

## What runs without the game

The package core (`Source/Core`) is plain C# and its tests compile it in directly, so no RimWorld install is needed:

```bash
dotnet run --project Tests/RimBabel.Tests.csproj
```

379 checks (the translation machinery and the engines included), exit code 1 when one fails; 0 failed on 2026-10-07 on the tree of the current `Mod/`. They cover: the source hash ignores the newline
convention; the manifest round-trips special characters, newlines, status, engine, glossary and blacklist, and refuses a
newer schema; the merge keeps unchanged and human text, makes a stale draft of a changed source, never touches a locked
text, reports new and removed keys, and bumps the version only on a change; the writer lays out the package, escapes
line breaks as the game reads them, leaves pending texts out, keeps `PublishedFileId.txt` and human texts across an
update, lists the changelog newest first, adds nothing when nothing moved, and refuses a hand-made language folder, a
path-like language name and a key that is not an XML name.

### The translation machinery (2026-10-04)

Offline, with fake engines and no network: **placeholder protection** (braces, a whole gender switch, nested switches,
rich-text tags and `(*Colonist)` markers hidden behind tokens; a faithful answer gets every original back, tokens may
change places, and a lost, duplicated, invented or empty answer, or one ten times too long or too short, is refused);
the **glossary** (terms hidden from the engine and given back as the required translation, longest term first, a capital
kept at the start of a sentence, no match inside a longer word, an engine that drops a term refused); the **blacklist**
(glob, regular expression, invalid rules named); the **pipeline** (pending and stale texts translated, human and locked
never touched, blacklisted never sent, a broken translation left as the source after one more try alone, a shifted answer
dropped whole, an engine that throws stops the run and keeps what was done, batches of the asked size); the **importer**
(an existing translation brought in as human work, the game's `EN:` comment used to tell a text written for another source
apart, the `TODO` placeholder ignored, orphans reported, existing human texts kept). Three mutations were run (the status
filter of the pipeline, the once-only token rule, and the empty-answer check) and each makes tests fail. **Not covered**:
the real services. The engines (DeepL, Anthropic, any OpenAI-compatible endpoint) are tested against a stand-in for the network: request URL, headers and body, reading of the reply (including a fenced one), a refusal that names the status and never the key, an unusable reply refused, and a whole chain from manifest to translated text. **No test calls a real service**, so nothing proves a real key, a real model name or a real reply format works; that needs the owner's keys.

## What runs against the game's assemblies, still outside the game

```bash
dotnet build Source/RimBabel.csproj -c Release && dotnet build Tests/Game/RimBabel.GameTests.csproj -c Release && Tests/Game/bin/Release/RimBabel.GameTests.exe
```

Needs a RimWorld install (the `Managed` folder; override `RimWorldManaged` or pass it as the first argument). It runs
the **shipped** `Mod/Assemblies/RimBabel.dll` against the real `Assembly-CSharp`, builds a few Defs by hand, registers them
in the game's own `DefDatabase` and lets `SourceScanner` walk them with the game's own `DefInjectionUtility`. 47 checks, 0 failed on 2026-10-07,
Keyed texts listed with their placeholders and real line breaks; a def's label and description
listed under the exact injection path; a one-word label and description taken because the game marks both
`MustTranslate`; a texture path (even with a space in it), a defName, another mod's def and a generated def left out; a
text inside a list of objects listed through the list; every path a valid XML element name; no entry twice; then the
package built from the scan (id, source named, manifest complete, unknown licence blocks publication), a second scan
changing nothing, and an edited source text being the only change. The scanner's rule is also checked on real fields.

**A mutation check was run**: removing the "holds a space" rule makes one check fail, so those checks can fail.
Removing the former `label` clause changed nothing, which showed it was dead code (`Def.label` is `MustTranslate`),
and it was deleted.

What this cannot show: a real mod loaded by the game (XML parsing, patches, inheritance, translations already
loaded, mods that add their own assemblies), and the developer-menu action itself, which needs Unity. Two traps of this
setup, so the next person does not lose time: Defs are built **without their constructors** (`ThingDef`'s reaches
Unity's shader loading), and `GenTypes`' type cache is filled by hand, because it logs through Unity when 27 game
types fail to load on this runtime. The test host is .NET Framework and the game runs Unity's Mono, so a BCL overload
that only exists in the newer profile can fail here without being a defect in the game (three were met and avoided:
`Trim(char)`, `TrimStart(char)`, `Split(char, ...)`; the code now passes arrays).

## What needs the game

A real list of Defs, a settings window, a restart and the main bar only exist in a running game, so they are Pickle scenarios
(`Tests/Pickle/`, eight features, 24 scenarios, played in English and in French through the Ticket Dispatcher; the step
assemblies are in `Tests/Pickle/Source/`, `Check-Steps.ps1` checks the step expressions). **Not tested anywhere: the real
services.** No automated test calls DeepL, Anthropic, OpenAI, Google Cloud or the others: they run against a fake endpoint
(offline in `Tests/`, and a small server on the loopback address in `04-translation`). Each real service was tried once by
hand with a throwaway program outside the repository, and the outcome is in `docs/runs/history.md`.

### Functional scenarios (preconditions, actions, expected results)

| Feature | Precondition | Action | Expected |
| --- | --- | --- | --- |
| 01 package (5) | main menu, the companion mod loaded | the code the developer-menu entry calls scans the companion mod | its five known texts are in the manifest, nothing else; a rescan changes nothing; the scan takes well under a second |
| 02 settings (7) | the save `test-colony`, settings at their defaults | reveal and hide the shortcut; activate it; choose RimBabel in Mod options; set values and write | hidden by default, drawn once revealed; the dialog is this mod's; the page draws for ten frames with no logged error; the file holds the values and no key; every text exists in the language of the pass; values come back after a read of the file, line breaks included |
| 03 rimmsqol-shortcut (3, pass `avec-rimmsqol`) | RIMMSQOL loaded | reveal, open and hide the shortcut from RIMMSQOL's own list | offered hidden; drawn and opens the same settings; gone again, nothing left behind |
| 04 translation (3) | a fake server on the loopback address, the engine set to it | translate the companion mod with the chosen engine; with a blacklisted text; twice | five texts translated and recorded with their engine, the blacklisted one still pending, the second run sends nothing, no placeholder or glossary term reaches the server |
| 05 gallery (3, `@review`) | example values, a masked placeholder key | open Mod options, the page, scrolled down | pictures for a person; nothing asserted |
| 06-08 restart (3, one chain of three launches) | main menu | write values; in a new game process read what the game loaded at startup; put back | the second launch holds the values the first wrote; the third leaves nothing |

The menu window of the developer entries is not covered (a developer tool, not player-facing; see `STATUS.md`).

## Passes

Declared in `Tests/Pickle/README.md`: **minimal in English, then minimal in French** (the package language is a setting,
not the game's language, so the French pass shows it does not depend on it). No optional integration exists, so no pass
with optional mods; no declared incompatibility; no DLC. Each pass is one request filed through the Ticket Dispatcher.
Non-regression passes run after the deploy, in small tickets (`AUDIT.md` 14.a).

## What is not tested here, and why

The behaviour of a vanilla or other mod's Def, left as it is, is not tested: RimBabel does not change it. The game's
own language switching is not tested either: a pass runs in one language.

## Evidence: what to keep

Evidence stays on disk, never in git (`.gitignore`: `Tests/Pickle/Evidence/`, `docs/runs/evidence/`,
`pickle-reports-archive/`). Per scenario, keep **the latest report for the revision now in the repository**, plus an
older one only if it is the sole proof of a check the latest did not repeat. Delete the rest once a newer report
replaces it. A report built for a superseded revision proves nothing about the current one. Keep `summary.json` and
`junit.xml`; `report.html` and `messages.ndjson` of a stale build are not worth their weight. Captures are kept
minified (reduced JPEG, with the `@review` ones at full size). One line per run goes into `docs/runs/history.md`; no
field of `STATUS.md` may point to a report that has been deleted, so repoint it first.
