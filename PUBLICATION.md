# Publishing RimBabel

What the Workshop page asks for and the repository holds nowhere else, checked against `PUBLISHING.md`. Kept for
whoever picks this mod up later. State: version 1.0.0 is published and the item is public (2026-10-10).

## Where the item stands

- **Workshop item `3814129753`**, created by the owner's prepublication `0.1.0` on 2026-10-05 from `Mod/` as it stood at
  commit `5d31c07`; public since 2026-10-10, version `1.0.0` sent by the CI (tag `v1.0.0`).
- **`Mod/About/PublishedFileId.txt`** holds that id and is committed. `CHANGELOG.md` opens its `## [0.1.0]` with "creation of a
  publishId file".
- **Publication goes through the CI** (`PUBLISHING.md`, "Publier par la CI"): a green dry-run of the exact commit first, then
  `publish` with the full 40-character SHA. No session approves `steam-production`; the CI creates the tag and the release.
- **Code review**: `code_review_sha` in `STATUS.md` is the last reviewed commit; the next review covers `code_review_sha..HEAD`.

## The one-way parts

- **The description.** The game sends it only when it creates the item; it was sent at the prepublication. From `1.0.0` the CI
  sends it from the Markdown block under `## Steam description`, which also generates the `<description>` of `About.xml`.
  A correction before then is made by hand on the Steam page or by the CI (`update_description`), never from `About.xml`.
- **The packageId**: `nelim.rimbabel`. Changing it after publication disables the mod for everyone.
- **`About/PublishedFileId.txt`**: committed, or the next upload creates a second item.

## Page settings (answered 2026-10-10)

- **Dependencies and DLC**: none to declare. RimBabel needs no other mod and no DLC; Harmony is not used. RIMMSQOL is an optional customization mod that can reveal the hidden settings shortcut: nothing in the code names it, so it is not a `loadAfter` and not a dependency, but it is thanked in the description because a Pickle pass exercises it.
- **Adult-content questionnaire**: answered `No` on the page by the owner (the mod has no such content).
- **Visibility**: public since 2026-10-10 (set by the owner by hand).
- **Gallery**: see "Screenshots, in upload order"; uploaded by hand by the owner.

## Screenshots, in upload order

Steam shows the first image large. `Art/Gallery/` holds only the images to upload, numbered in upload order, under 2 MB each and
under 8 MB in all; `0-` is a byte copy of `Mod/About/Preview.png`. Candidates carry `candidate` in their name until the owner
accepts them (the word is then dropped); refused ones are deleted.

| # | File | What it shows | Why here |
| --- | --- | --- | --- |
| 0 | `0-preview.png` | The Preview | Rule: always first. |
| 1 | `1-engines.png` | The settings page: seven engines, the key masked, the batch size | The most demonstrative page: what the mod offers at a glance. |
| 2 | `2-dictionary-and-blacklist.png` | The dictionary and the texts that must stay as they are | The two things a translator controls. |

**Crop rule (owner, 2026-10-09):** every interface capture taken on `window-backdrop-for-height` is cropped sideways, leaving 5 px of backdrop on each side of the window (owner chose 5 px, 2026-10-09); full height kept. Pictures 1 and 2 are cropped that way (908x1080, from 1220x1080), accepted by the owner the same day.

The earlier candidates were crops of the Pickle
runs of 2026-10-07 (taken in the test colony); they were replaced by the pictures staged on the Sanctuary (feature `05-gallery`,
the place `window-backdrop-for-height`, the save `Nelims-tribe`). The windows are shown as they are, without decorative staging;
the series follows Nelim opening the options, choosing an engine and filling the dictionary.
The developer menu (the way version 1 is used) is not pictured yet: the game's debug window is a tabbed window the scenarios
cannot open on a given category (see `BACKLOG.md`).

## Steam description

The block is Markdown; the CI converts it. Sections in the order `PUBLISHING.md` asks for: body, `IF I GO QUIET`, `AI-GENERATED`,
`THANKS`, the line about `ATTRIBUTION.md` and the licence, then the source link.

```markdown
Translate any RimWorld mod into your language and get a standalone translation mod, ready to publish.

RimBabel reads the texts of a mod, translates what is missing with the engine you choose (DeepL, Anthropic, Google Cloud, LibreTranslate, MyMemory, Yandex, or any OpenAI-compatible server, including one running on your own computer), and writes a normal translation mod. It never touches the mod it translates.

- One dictionary for every mod, so the same word is translated the same way everywhere.
- A blacklist for texts that must remain unchanged.
- Placeholders, tags and markers are kept out of the engine's sight and checked on the way back. A translation that loses one is refused.
- Run it again when the source mod changes: only the texts that moved are translated again. Texts you reviewed by hand are never overwritten.
- Every text records which engine produced it, so you can remove one engine's output without disturbing the rest.
- Import an existing translation as the starting point.
- Settings under Mod options, in English and French. Your keys stay in the game's settings file on your computer.

**How to use it (version 1):** there is no translation window yet. Turn on development mode in the game options, open the debug actions menu, find the RimBabel section, pick "Translate a mod with the chosen engine..." and choose the mod. The result is a new translation mod, in the folder shown under Mod options, RimBabel. Pick "Write a translation package for a mod..." instead to only extract the texts without translating.

**IF I GO QUIET**
If I do not answer within a reasonable time after being contacted, anyone may freely update this or any other of my mods, including publishing a continuation of it. All credit must be preserved.

**AI-GENERATED**
The code, the tests and the texts of this mod were written with Claude (Anthropic), under my direction and review. The icon was generated with DALL-E (OpenAI). The picture behind the Workshop preview was generated with DALL-E too.

**THANKS**
- [Pickle](https://steamcommunity.com/sharedfiles/filedetails/?id=3791648678), for the in-game tests; development only, never a dependency of RimBabel.
- [Nelim's Pickle Tools](https://steamcommunity.com/sharedfiles/filedetails/?id=3806142401), my own test tools, development only.
- [RIMMSQOL](https://steamcommunity.com/sharedfiles/filedetails/?id=1084452457), which can reveal the settings shortcut; optional, a test pass exercises it.
- The authors of the translation tools I read before writing a line: [Auto Translation](https://steamcommunity.com/sharedfiles/filedetails/?id=3278005460), [Auto Translation Framework](https://steamcommunity.com/sharedfiles/filedetails/?id=3759370650), [AI Translation Network](https://steamcommunity.com/sharedfiles/filedetails/?id=3721659501) and [Auto Translator](https://steamcommunity.com/sharedfiles/filedetails/?id=3668680570), and the open-source RimTranslate, RimTrans and their kin. Nothing was copied; what each taught is in ATTRIBUTION.md.

RimBabel is original work under the MIT licence. What I studied and what I took (nothing) is in ATTRIBUTION.md, next to the licence, in the repository.

[Source code on GitHub](https://github.com/vbardales/Rimworld-RimBabel)
```

## Thanks

The register is `WORKSHOP_COMMENTS.md` (key: the recipient's Workshop id). Pickle and RIMMSQOL were thanked earlier (`Covers` lists RimBabel); PickleTools is the same author; Harmony is not used by RimBabel. The owner posted one comment on each of Auto Translation Framework (3759370650), Auto Translation (3278005460), AI Translation Network (3721659501) and Auto Translator (3668680570) on 2026-10-10; the sent texts are in `docs/runs/posted-1.0.0.md`. The register's rule stays: one main comment per Workshop page, not a template copied four times.

## Steam change notes

Each `### <version>` heading is followed by a fenced block whose first line is BBCode carrying that exact version (`[b]x.y.z[/b]`); the CI reads the block and refuses one that does not carry the version of its heading. `0.1.0` was the prepublication (a private item, no note). The note sent with `1.0.0` is in `docs/runs/posted-1.0.0.md`.

Template for the next version:

### x.y.z

```
[b]x.y.z[/b]
What changed, one line each.
```

## Optional thanks drafts, not posted

The owner did not try these two mods; post only if she wants a comment there. Neither is in the register.

Universal Translation Framework (3495185132), **optional, not in the register**: the owner did not try it, so the draft says so and rests on the page only (it replaces hardcoded strings by IL transpilation, the part RimBabel does not cover; source: github.com/Ocean456/UniversalTranslationFramework). Post only if the owner wants a comment there:

```
Haven't tried it myself, but replacing hardcoded strings through IL patching is exactly the gap my own translator leaves open (it only handles XML), so it's good to know it exists :) Thanks for sharing it!
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=3814129753]RimBabel[/url]
```

AutoTranslator (3777360574), **optional, not in the register**: not tried by the owner, so the draft says so and rests on the page only (real-time and batch translation, results kept in the language pack's `Keyed/auto.xml`, a `verified="true"` mark that locks a reviewed entry, Volcano Engine / Alibaba Cloud / Baidu). Post only if the owner wants a comment there:

```
Haven't tried it, but the verified="true" mark that locks a hand-fixed line so the machine never overwrites it is a really good idea, I ended up with the same kind of lock in my own translator :) Thanks for sharing it!
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=3814129753]RimBabel[/url]
```
