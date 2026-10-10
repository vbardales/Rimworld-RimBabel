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
