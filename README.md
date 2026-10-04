# RimBabel

Translate RimWorld mods into your language, once, the same way in every mod, and ship the result as a mod of its own.

RimBabel reads the texts of a mod, translates what is missing with the engine you choose, and writes a **translation
package**: a normal RimWorld mod, ready to commit and publish, that never touches the mod it translates. Run it again
when the source mod changes and only the texts that moved are translated again.


**Status: v1 in development.** v1 is the developer menu (write the translation mod of an installed mod) plus the settings page (engine, keys, dictionary, blacklist). A window to translate from the interface, regular-expression search and the hover original come later. See [STATUS.md](STATUS.md).
## What it is meant to do

- **One package per translated mod.** A package is a plain mod: `Mod/About`, `Mod/Languages/<language>/Keyed` and
  `DefInjected`, plus `Mod/Babel/manifest.xml`. It works without RimBabel installed.
- **A manifest that is the source of truth.** For each text: what the source says, what the package says, who wrote it
  (a person, an engine, reviewed, locked). `Languages/` is generated from it and can be rebuilt at any time.
- **Updates by difference.** A text that did not move is kept. A text whose source changed becomes a draft to redo,
  unless it was locked. A text gone from the source is dropped. The package version goes up by itself.
- **A dictionary** (this word is that word in every mod), a **blacklist** (mods, keys or kinds of text that are never
  translated, such as colonist names) and **placeholder protection** (`{0}`, `{PAWN_nameDef}`, rich-text tags survive
  translation or the text stays in the source language).
- **Engines you configure**: DeepL, Anthropic, any OpenAI-compatible endpoint (Ollama, LM Studio, OpenRouter...) and a
  free fallback. Keys stay in your own configuration, never in a package or a repository.
- **Import** of an existing translation pack, kept as human work an engine never overwrites.
- **A fast search** across the texts, with regular expressions.

On-the-fly translation of the interface is not part of the first version.

## A package, on disk

```
MyMod-French/
  Mod/                      what ships to the Workshop
    About/About.xml
    Languages/French/Keyed/MyMod.xml
    Languages/French/DefInjected/ThingDef/MyMod.xml
    Babel/manifest.xml      every text, its status and its origin
    LICENSE
  README.md  CHANGELOG.md  ATTRIBUTION.md  STATUS.md  LICENSE  .gitignore
```

A package is a derivative of the mod it translates. RimBabel will not call one publishable while the licence of the
source mod is unknown, and it says why.

## Building and testing

The package core has no game dependency:

```bash
dotnet run --project Tests/RimBabel.Tests.csproj
```

## Licence

MIT, see [LICENSE](LICENSE). What this project learned from other translators, and what it did not take, is in
[ATTRIBUTION.md](ATTRIBUTION.md).

## AI

Code and documentation were written with Claude (Anthropic) under human direction and review.
