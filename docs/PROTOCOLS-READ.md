# Protocols read, and in which version

What the RimBabel session read on **2026-10-03** (audit against `AUDIT.md`, at the owner's request). A version is the
last commit that touched the file in the repository that holds it; `M` means modified and not committed. Protocol
documents live in `vbardales/Rimworld-protocols` (git dir `../rimworld-protocols.git`, work tree = the monorepo root),
so a plain `git log` from the monorepo answers for the commit that removed them: read them with `--git-dir`, as
`WELCOME.md` says. When a commit below has moved, the document changed: read it again before relying on it.
"Whole" means every line was read; "not read" means not opened this time.

## Protocol documents

| Document | Read | Version | Useful here? |
|---|---|---|---|
| `AGENTS.md` | whole (the copy loaded as project instructions) | 7fd7475, 2026-09-29, 21 lines | Yes: mod workflow gates, evidence rule, publishing by CI |
| `AUDIT.md` | whole | 5a975b5, 2026-10-02, 278 lines | Yes: it is the task. Stage chain, the `done -> showcase/preTest` step, Pickle rules, title of the session |
| `MOD_SETTINGS.md` | whole | b83933b, 2026-09-23, 107 lines | Yes: `settings_audit` values, access routes, hidden shortcut. Applies once the settings page exists |
| `TRANSLATIONS.md` | whole | af8427f, 2026-10-02, 216 lines | Yes: Keyed keys, plurals, French review by the owner, `FRENCH_REVIEW.md`. Also bears on what generated packages carry |
| `PUBLISHING.md` | whole | 4a1de56, 2026-10-02, 791 lines | Yes: single Markdown description source, one-way parts, original-repository rule, thanks register, pathspec commits |
| `STYLE_RIMWORLD.md` | **not read** | 4e44398, 2026-10-02, 716 lines | **No**: the graphic style of Preview and ModIcon. A session never generates either; only the file limits matter (Preview under 1 MB, ModIcon checked at 32 px by the owner) |
| `WORKSHOP_COMMENTS.md` | **not read** | 4e44398, 2026-10-02 (M), 163 lines | **Not yet**: needed when thank-you comments are drafted, section "Writing a comment" first. Nothing is published |
| `scripts/SEARCHING.md` | **not read** | 50de695, 2026-09-28, 222 lines | **No**: searching the mod corpus, which this work did not need |

## Tools

| Document | Read | Version | Useful here? |
|---|---|---|---|
| `PickleTools/README.md` | whole | bb732f7, 2026-10-02, 91 lines | Yes: the table of tools and how a pass map names one. No Pickle suite exists yet |
| `PickleTools/Headless/README.md` | **not read** | ed4e73a, 2026-09-26, 509 lines | **Not yet**: read before the first Pickle request (filters, `-DepMap`, traps) |
| `PickleTools/docs/steps.md` | **not read** | bb732f7, 2026-10-02, 344 lines | **Not yet**: the step catalogue, to consult before writing a step |
| `Rimworld-Release-Admin/docs/OPERATIONS.md` | whole | 3c03f51, 2026-09-26, 112 lines | Yes: dry-run of the exact SHA, 40-character SHA, only the owner approves `steam-production`, CI creates tag and release |
| `Rimworld-Ticket-Dispatcher/docs/WELCOME.md` | whole | 77ca9d7, 2026-09-27, 150 lines | Yes: which test for which task, a request carries no SHA, the list of documents to note |
| `Rimworld-Ticket-Dispatcher/docs/SUBMIT.md` | whole | 294c43d, 2026-10-03 (M), 159 lines | Yes for the first request: options, `-EvidenceDir`, priority is the Ticket Manager's, `-NoBatch`, exit codes |

## Documents of this repository (no commit yet)

| Document | Read | Version | Useful here? |
|---|---|---|---|
| `STATUS.md`, `README.md`, `CHANGELOG.md`, `ATTRIBUTION.md`, `LICENSE`, `PUBLICATION.md`, `TESTING.md`, `BACKLOG.md` | written 2026-10-03 | uncommitted | Yes. `ATTRIBUTION.md` and `LICENSE` have identical copies in `Mod/` (checked with `cmp`); `BACKLOG.md` is the mod's own, not the monorepo's |
| `docs/runs/history.md` | written 2026-10-03 | uncommitted | Yes |
| `Mod/About/About.xml` | written 2026-10-03 | uncommitted | Yes; the description is a draft |
| `Tests/Pickle/` | does not exist | | |
| `NOTES.md`, `BUGS.md` | do not exist | | Nothing to note yet |

## Useless this time, not to reread when they change

`STYLE_RIMWORLD.md` and `scripts/SEARCHING.md`: not needed for a mod that generates neither an icon nor a Preview and
searches no corpus. Reread `WORKSHOP_COMMENTS.md`, `Headless/README.md` and `steps.md` at the step that needs them
(thank-you drafts, first Pickle request, writing a step), not before.

## What reading them changed

- Session title is `rimbabel / dansMonoRepo`: packageId without `nelim.`, a slash, then `workflow_stage` as written in `STATUS.md`.
- The gate to `tested` carries the three explicit conditions (no `@wip`, every conditional scenario run, no manual test left) in `TESTING.md`.
- Evidence rules are written in `TESTING.md` and the ignore rules are in `.gitignore` (`Tests/Pickle/Evidence/`, `docs/runs/evidence/`, `pickle-reports-archive/`, `*.dds`, `*.ico`, `desktop.ini`).
- `CHANGELOG.md` has no `0.1.0` entry: there is no `PublishedFileId.txt`, so no item was created.
- Commits will use a pathspec (`git commit -- <paths>`), and `git status` is reread after each.
