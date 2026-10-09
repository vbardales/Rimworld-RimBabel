# Publishing RimBabel

What the Workshop page asks for and the repository holds nowhere else, checked against `PUBLISHING.md` on 2026-10-09. Kept for
whoever picks this mod up later. State at that date: the item exists and is private; nothing is public.

## Where the item stands

- **Workshop item `381412975321`**, created by the owner's prepublication `0.1.0` on 2026-10-05 from `Mod/` as it stood at
  commit `5d31c07`. Steam creates every item private; the owner switches it to public by hand.
- **`Mod/About/PublishedFileId.txt`** holds that id and is committed. `CHANGELOG.md` opens its `## [0.1.0]` with "creation of a
  publishId file".
- **Publication goes through the CI** (`PUBLISHING.md`, "Publier par la CI"): a green dry-run of the exact commit first, then
  `publish` with the full 40-character SHA. No session approves `steam-production`; the CI creates the tag and the release.
- **Reviewed commit for the code review**: `a3863248d374d1c2459cc4c95d15d054b3d5d4d8` (2026-10-05, the whole history, tests left
  out, low effort; three findings, all fixed). A new review is due from that SHA before the first real publication.

## The one-way parts

- **The description.** The game sends it only when it creates the item; it was sent at the prepublication. From `1.0.0` the CI
  sends it from the Markdown block under `## Steam description`, which also generates the `<description>` of `About.xml`.
  A correction before then is made by hand on the Steam page or by the CI (`update_description`), never from `About.xml`.
- **The packageId**: `nelim.rimbabel`. Changing it after publication disables the mod for everyone.
- **`About/PublishedFileId.txt`**: committed, or the next upload creates a second item.

## Open before the first public upload

- **Dependencies and DLC**: none to declare. RimBabel needs no other mod and no DLC; Harmony is not used. RIMMSQOL is an optional
  customization mod that can reveal the hidden settings shortcut: nothing in the code names it, so it is not a `loadAfter` and
  not a dependency, but it is thanked below because a Pickle pass exercises it.
- **Adult-content questionnaire**: applies; answer `No` (the mod has no such content). To be confirmed on the page.
- **Visibility**: the owner sets it to public by hand once the gallery and the description are in place.
- **Gallery**: see below; the pictures are staged on the Sanctuary and read before they are kept.
- **Description**: the block below carries every required section; the English text must be read once more by the owner before the
  first public version (she confirmed the body on 2026-10-07).

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
- The authors of the translation tools I read before writing a line: Auto Translation, [Auto Translation Framework](https://steamcommunity.com/sharedfiles/filedetails/?id=3759370650), AI Translation Network and Auto Translator (Workshop), and the open-source RimTranslate, RimTrans and their kin. Nothing was copied; what each taught is in ATTRIBUTION.md.

RimBabel is original work under the MIT licence. What I studied and what I took (nothing) is in ATTRIBUTION.md, next to the licence, in the repository.

[Source code on GitHub](https://github.com/vbardales/Rimworld-RimBabel)
```

## Thanks to post

The register is `WORKSHOP_COMMENTS.md` (key: the recipient's Workshop id). Rows and what this project owes them, 2026-10-09:

| Recipient | Register | For this project | State |
| --- | --- | --- | --- |
| Pickle (3791648678) | `posted`, 2026-09-22 | In-game tests; add `RimBabel` to the `Covers` column, repost nothing | recorded in the register (`Covers` lists RimBabel) |
| RIMMSQOL (1084452457) | `posted`, 2026-09-22 | A Pickle pass drives it (hidden shortcut); add `RimBabel` to `Covers`, repost nothing | recorded in the register (`Covers` lists RimBabel) |
| PickleTools (3806142401) | `not_applicable` (same author) | Staged in the passes | nothing to post |
| Harmony (2009463077) | `posted` (for other mods; RimBabel is not in its `Covers`) | **Not used by RimBabel**: not claimed, not thanked here | nothing |
| Auto Translation Framework (3759370650) | not in the register | Studied for its shared library and translation memory (description read, 2026-10-03 and 2026-10-05) | **drafted 2026-10-09** (page read that day), see "Thanks, drafts" |
| Auto Translation (3278005460), AI Translation Network (3721659501), Auto Translator (3668680570) | not in the register | Descriptions read on 2026-10-03; sources not read | **drafted 2026-10-09** (pages read that day), one personalised comment each, under 1000 characters, BBCode with `[url=…]name[/url]`; post only once this item is public |

The four comments are drafted below ("Thanks, drafts"), each from a true detail of its page, none posted. The register's rule stays: one main comment per Workshop page, not a template copied four times.

## Steam change notes

`### 0.1.0` is the prepublication (a private item; there is no note to show players). From the first public version, each `###
<version>` block starts with a BBCode line carrying that exact version.

Draft for the first public version, `1.0.0` (owner, 2026-10-09; the CI refuses a block whose first BBCode line does not carry exactly the version of its `###` heading):

```
### 1.0.0
[b]1.0.0[/b]
First public version. Translate a mod from the developer menu with DeepL, Anthropic, Google Cloud, LibreTranslate, MyMemory, Yandex or any OpenAI-compatible server, and get a standalone translation mod.
Settings under Mod options, RimBabel: engine and key, dictionary, blacklist, target language, output folder.
```

## Thanks, drafts (2026-10-09)

Drafted from the four Workshop pages read on 2026-10-09 (descriptions only: the comment threads load by script and were not read, so
language, mood and whether the author answers are still to check before posting). Owner's voice, one comment per page, none posted;
post only once this item is public. The four drafts below were rewritten from what the owner saw while using each mod (she tested all four); the two optional ones rest on their pages only.

Auto Translation Framework (3759370650), **rewritten from the owner's experience** (same trouble as AI Translation Network: a free engine produced poor results for many translations and no bulk invalidation or retranslation; and the GitHub link on its page is only the presentation site, not the code):

```
Covering the text hidden in C# is a real plus, but a free engine gave poor results for many of my translations and I found no way to invalidate them in bulk or ask for a bulk retranslation. I also looked for the code to suggest a fix, but the GitHub link only leads to the docs site. That gap is why I wrote my own translator. Thanks anyway!
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=381412975321]RimBabel[/url]
```

Auto Translation (3278005460), **rewritten from the owner's own experience** (she used it: the translation editor loaded very slowly and every write was slow too, perhaps an indexing problem):

```
Used your mod for a while, and having Google, DeepL and Claude in one list was really handy :) One thing on my setup: the translation editor took ages to load and every write was slow too, maybe something with indexing? Still, thanks for the work! It helped me with my own translator:
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=381412975321]RimBabel[/url]
```

AI Translation Network (3721659501), **rewritten from the owner's experience** (she enabled the free package, Yandex produced poor results for many translations, and she could neither invalidate them in bulk nor ask for a bulk retranslation):

```
The cloud hub and the multi-provider list are great, but I switched on the free package and Yandex gave poor results for many of my translations, and I could not invalidate them in bulk or ask for a bulk retranslation. That exact problem pushed me to build my own translator (each text remembers which engine wrote it). Thanks!
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=381412975321]RimBabel[/url]
```

Auto Translator (3668680570), **rewritten from the owner's experience** (she could not get it to work, and is not sure the fault was not hers):

```
Tried your mod but I never got it running on my side, so no real feedback, and it may well be my setup :) I like the idea of generating a normal localization mod that people can edit, it is what I went for in my own translator. Thanks for the work!
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=381412975321]RimBabel[/url]
```

Note: the item id in these links is the Workshop item's (private until public).

Universal Translation Framework (3495185132), **optional, not in the register**: the owner did not try it, so the draft says so and rests on the page only (it replaces hardcoded strings by IL transpilation, the part RimBabel does not cover; source: github.com/Ocean456/UniversalTranslationFramework). Post only if the owner wants a comment there:

```
Haven't tried it myself, but replacing hardcoded strings through IL patching is exactly the gap my own translator leaves open (it only handles XML), so it's good to know it exists :) Thanks for sharing it!
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=381412975321]RimBabel[/url]
```

AutoTranslator (3777360574), **optional, not in the register**: not tried by the owner, so the draft says so and rests on the page only (real-time and batch translation, results kept in the language pack's `Keyed/auto.xml`, a `verified="true"` mark that locks a reviewed entry, Volcano Engine / Alibaba Cloud / Baidu). Post only if the owner wants a comment there:

```
Haven't tried it, but the verified="true" mark that locks a hand-fixed line so the machine never overwrites it is a really good idea, I ended up with the same kind of lock in my own translator :) Thanks for sharing it!
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=381412975321]RimBabel[/url]
```
