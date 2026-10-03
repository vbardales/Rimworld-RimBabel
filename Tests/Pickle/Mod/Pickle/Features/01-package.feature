# What a person would otherwise do by hand: load a mod, open the developer menu, choose
# "RimBabel > Write a translation package for a mod...", pick the mod, and read what comes out.
#
# The mod picked is this suite's own companion, "RimBabel - Pickle tests": it carries two defs and a
# Keyed file whose texts are known (Mod/Defs/Fixture.xml, Mod/Languages/English/Keyed/RimBabelFixture.xml),
# so that what RimBabel lists for it can be checked text by text. If a text is added there, a line
# here must name it.
#
# The step calls the same method the menu entry calls (DevActions.WriteFor), so everything below the
# menu is the real code, on defs the game itself loaded from XML. What the menu window looks like is
# not covered: it is a developer tool and has no player-facing layout.
#
# No save is loaded. Mods and defs are settled before a game exists, so these run at the main menu.
# The language of the package is a package setting, not the language of the pass: the French package
# is written whichever language the game runs in.
Feature: RimBabel lists the texts of a loaded mod and writes its translation package

  Scenario: the texts of the source mod are listed in the manifest
    Given RimBabel: no package exists yet for the mod "RimBabel - Pickle tests" in "French"
    When RimBabel: I write the translation package of the mod "RimBabel - Pickle tests" for the language "French"
    Then RimBabel: the manifest lists "K:RbFixture_Hello" as "Hello {0}"
    And RimBabel: the manifest lists "K:RbFixture_Two" as "Line one\nline two"
    And RimBabel: the manifest lists "D:ThingCategoryDef/RbFixtureCategory.label" as "fixture category"
    And RimBabel: the manifest lists "D:ThingCategoryDef/RbFixtureCategory.description" as "Things that exist only to be listed."
    And RimBabel: the manifest lists "D:StatCategoryDef/RbFixtureStats.label" as "fixture stats"
    And no errors were logged

  Scenario: what is not a text is left out, and so are the texts of other mods
    Given RimBabel: no package exists yet for the mod "RimBabel - Pickle tests" in "French"
    When RimBabel: I write the translation package of the mod "RimBabel - Pickle tests" for the language "French"
    Then RimBabel: the manifest does not list "D:ThingCategoryDef/RbFixtureCategory.defName"
    And RimBabel: the manifest lists no text of a def outside the mod
    And RimBabel: the manifest lists no text from the language folders of other mods

  Scenario: nothing is translated yet, and the package says so
    Given RimBabel: no package exists yet for the mod "RimBabel - Pickle tests" in "French"
    When RimBabel: I write the translation package of the mod "RimBabel - Pickle tests" for the language "French"
    Then RimBabel: every text of the manifest is Pending
    And RimBabel: the package id is "nelim.rimbabel.nelim.rimbabel.pickletests.french"
    And RimBabel: the package is not publishable yet because "Licence of the source mod is unknown"

  Scenario: scanning the same mod again changes nothing
    Given RimBabel: no package exists yet for the mod "RimBabel - Pickle tests" in "French"
    When RimBabel: I write the translation package of the mod "RimBabel - Pickle tests" for the language "French"
    And RimBabel: I write the translation package of the mod "RimBabel - Pickle tests" for the language "French"
    Then RimBabel: the last write found 0 new, 0 changed and 0 removed texts
    And RimBabel: the package version is "0.1.0"
    And no errors were logged
