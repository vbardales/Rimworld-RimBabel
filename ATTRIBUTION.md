# Attribution

RimBabel is original work, released under the MIT licence (see `LICENSE`). It ports no mod and is derived from none.

## Studied, nothing taken

These translation tools were read for how they work, on 2026-10-03, from their public descriptions and READMEs. **No
code and no text was copied from any of them.** Where a licence is GPL-3.0, copying would not be possible in an MIT
project, so the ideas below are the only thing that crossed.

| Project | Licence (as stated) | What it taught |
|---|---|---|
| [RimTranslate](https://github.com/winterheart/RimTranslate) (winterheart) | GPL-3.0 | Reusing an older translation as a memory with a "fuzzy" mark; a human review step |
| [Rimworld-Mod-Translator](https://github.com/kelvinauta/Rimworld-Mod-Translator) (kelvinauta) | not stated | Listing the XML tags that carry translatable text |
| [RimTrans](https://github.com/RimWorld-zh/RimTrans) (RimWorld-zh) | MIT | Finding translatable fields by reflection on the game types |
| [RimLanguageHotReload](https://github.com/lordfanger/RimLanguageHotReload) (lordfanger) | not stated | That reloading language data at run time is a development tool, not something to ship |
| [RimWorld-Auto-AI-Translation-Core](https://github.com/as5611198/RimWorld-Auto-AI-Translation-Core) (as5611198) | GPL-3.0 | A separate generated package instead of editing the source mod; a translation memory built from every installed translation; leaving a text in the source language when the machine output is invalid |
| [RimWorld-ModTranslator](https://github.com/R17CTL/RimWorld-ModTranslator) (R17CTL) | MIT | That an unofficial web endpoint is a poor default |
| [RimworldModTranslator](https://github.com/TokcDK/RimworldModTranslator) (TokcDK) | GPL-3.0 | Filling a target from existing translations; keeping a reviewable file |

Workshop items read for their description only: Auto Translation (3278005460), Auto Translation Framework
(3759370650), AI Translation Network (3721659501) and Auto Translator (3668680570). Their sources are not
published or were not read.

## Third-party components

- **Harmony** (Andreas Pardeike, MIT), when the in-game part needs it. Not used yet.
- **Krafs.Rimworld.Ref** (Krafs, MIT) for the game reference assemblies at build time.

## Tools

Built with Claude (Anthropic). The mod icon (`Art/ModIcon-source.png`, and the Preview's badge) was generated with DALL-E (OpenAI) and cut out by the owner. Tests, once they exist in game, run with Pickle and RimLogging, which are development
tools and never a dependency of the mod.
