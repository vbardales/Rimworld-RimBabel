# A translation run, end to end, in a real game, with no real service. The settings choose the OpenAI-compatible engine and its
# address is a small server this suite starts on the loopback address (the steps of TranslationSteps.cs): it answers each text
# with a prefix and keeps every token. This is the developer-menu entry "Translate a mod with the chosen engine..." minus the menu.
#
# What it proves that the offline tests cannot: the settings of a running game drive the engine; the extraction of a loaded mod,
# the engine, the protector and the package writer work together on defs the game loaded from XML; the dictionary and the
# blacklist of the settings reach the run; a second run sends nothing again; and nothing leaves the computer.
# What it does not prove: the quality of any real engine (see docs/runs/history.md for the live tries), and the menu window.
#
# The mod translated is this suite's own companion, "RimBabel - Pickle tests": five known texts (see 01-package.feature).
Feature: a mod is translated with the engine of the settings

  Background:
    Given RimBabel: a fake translation server is listening
    And RimBabel: the engine is the fake server
    And RimBabel: no package exists yet for the mod "RimBabel - Pickle tests" in "French" on this run
    And RimBabel: the shared dictionary file of "French" does not exist

  Scenario: every text goes to the engine, comes back, and lands in the package
    When RimBabel: I translate the mod "RimBabel - Pickle tests" into "French" with the chosen engine
    Then RimBabel: the engine translated 5 texts and refused none
    And RimBabel: the text "K:RbFixture_Hello" reads "FR: Hello {0}" and comes from "openai-compatible:stub"
    And RimBabel: the text "K:RbFixture_Two" reads "FR: Line one\nline two" and comes from "openai-compatible:stub"
    And RimBabel: a language file of the package holds "FR: Hello {0}"
    And RimBabel: the server received 1 requests and no placeholder or glossary term in them
    And no errors were logged

  Scenario: a blacklisted text is never sent
    Given RimBabel: the blacklist holds "K:RbFixture_Two"
    When RimBabel: I translate the mod "RimBabel - Pickle tests" into "French" with the chosen engine
    Then RimBabel: the engine translated 4 texts and refused none
    And RimBabel: the text "K:RbFixture_Two" is still pending

  Scenario: a second run sends nothing again
    When RimBabel: I translate the mod "RimBabel - Pickle tests" into "French" with the chosen engine
    And RimBabel: I translate the mod "RimBabel - Pickle tests" into "French" with the chosen engine
    Then RimBabel: the engine translated 0 texts and refused none
    And RimBabel: the server received 1 requests

  Scenario: the test button of the settings page reaches the engine and shows its answer
    When RimBabel: I press the engine test button of the settings page
    Then RimBabel: the engine test says it worked with "FR: Hello {0}, welcome to the colony."
    And RimBabel: the server received 1 requests

  Scenario: the test button says it failed, with a reason, when nothing answers
    Given RimBabel: the engine address is one that nothing listens on
    When RimBabel: I press the engine test button of the settings page
    Then RimBabel: the engine test says it failed

  Scenario: the dictionary file shared by every mod reaches the run
    Given RimBabel: the shared dictionary file of "French" holds "Line one = ligne un"
    When RimBabel: I translate the mod "RimBabel - Pickle tests" into "French" with the chosen engine
    Then RimBabel: the text "K:RbFixture_Two" reads "FR: Ligne un\nline two" and comes from "openai-compatible:stub"

  Scenario: one translation mod for the whole language is generated next to the per-mod package
    Given RimBabel: the settings ask for one translation mod per language
    When RimBabel: I translate the mod "RimBabel - Pickle tests" into "French" with the chosen engine
    Then RimBabel: the one translation mod of "French" loads the folder of "nelim.rimbabel.pickletests" only when that mod is active

  Scenario: the Mistral button fills the address and the free-tier model and keeps the key
    Given RimBabel: the OpenAI-compatible key typed is "typed-by-the-player"
    When RimBabel: the Mistral button of the settings page is applied
    Then RimBabel: the engine address and model are Mistral's free tier and the key is untouched

  Scenario: a mod written in another language is not read as English, and the target cannot be the language of the texts
    Given RimBabel: the language of the texts is "French"
    Then RimBabel: translating the mod "RimBabel - Pickle tests" into "French" is refused because there is nothing to translate
    When RimBabel: I translate the mod "RimBabel - Pickle tests" into "German" with the chosen engine
    Then RimBabel: the manifest lists no Keyed text, since the mod holds none in the language of the texts

  Scenario: the installed translations are found next to a mod, and RimBabel's own packages are left out
    Then RimBabel: the installed translations next to "RimBabel - Pickle tests" in "French" include "nelim.rimbabel" and none of RimBabel's own packages
