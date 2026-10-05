# The part of MOD_SETTINGS.md that needs RIMMSQOL. 02-settings.feature tests this mod's side of the contract by moving the def's
# buttonVisible by hand. This drives RIMMSQOL itself, through the shared steps of PickleTools/RimmsqolSteps:
#
#   - RIMMSQOL's own list of main buttons offers RimBabel_Settings, and the entry a player would click reads hidden;
#   - RIMMSQOL reveals it (its own settings instance, its own write), the main bar then draws it, and the file RIMMSQOL wrote says so;
#   - the revealed button opens THIS mod's settings, the same dialog as Mod options;
#   - hiding it again empties the bar, and forgetting the choice leaves nothing in RIMMSQOL's file.
#
# What it does not do: click RIMMSQOL's checkbox (the steps call what the checkbox calls), and check that RIMMSQOL keeps its
# choice across a restart, which is RIMMSQOL's own behaviour. Played only by the pass avec-rimmsqol (wsl-deps.avec-rimmsqol.map).
@review @rimmsqol @requires:MalteSchulze.RIMMSqol @requires:nelim.pickletools.rimmsqol
Feature: RIMMSQOL reveals and hides the RimBabel shortcut

  Background:
    Given the save "test-colony" is loaded
    And RimBabel: settings are at their defaults
    And I close all dialogs
    Then mod "MalteSchulze.RIMMSqol" is loaded
    And RIMMSQOL is ready to be driven

  Scenario: RIMMSQOL's own list offers the shortcut, hidden, and the bar does not draw it
    Then RIMMSQOL's own list of main buttons offers "RimBabel_Settings"
    And RIMMSQOL shows the main button "RimBabel_Settings" as hidden
    And RIMMSQOL holds no choice for the main button "RimBabel_Settings"
    And the main bar does not draw the button "RimBabel_Settings"
    When RIMMSQOL's own window is opened on its list of main buttons
    Then RIMMSQOL's own window is open
    When I take a screenshot "rimmsqol, its list of main buttons, with the rimbabel shortcut"
    And I close all dialogs

  Scenario: revealed in RIMMSQOL the shortcut is drawn, and it opens the same settings as Mod options
    When RIMMSQOL reveals the main button "RimBabel_Settings"
    Then RIMMSQOL shows the main button "RimBabel_Settings" as visible
    And RIMMSQOL's settings file records the main button "RimBabel_Settings" as visible
    And the main bar draws the button "RimBabel_Settings"
    When RIMMSQOL's own window is opened on the main button "RimBabel_Settings"
    Then RIMMSQOL's own window is open
    When I take a screenshot "rimmsqol, edit page of the rimbabel shortcut, revealed"
    And I close all dialogs
    And the main bar's button "RimBabel_Settings" is activated
    Then a settings dialog is open for RimBabel
    And no errors were logged
    When I take a screenshot "rimbabel settings, opened by the shortcut RIMMSQOL revealed"
    And I close all dialogs

  Scenario: hidden again in RIMMSQOL the shortcut leaves the bar, and forgetting the choice leaves nothing behind
    Given RIMMSQOL reveals the main button "RimBabel_Settings"
    And the main bar draws the button "RimBabel_Settings"
    When RIMMSQOL hides the main button "RimBabel_Settings"
    Then RIMMSQOL shows the main button "RimBabel_Settings" as hidden
    And the main bar does not draw the button "RimBabel_Settings"
    And RIMMSQOL's settings file records the main button "RimBabel_Settings" as hidden
    When RIMMSQOL forgets its choice for the main button "RimBabel_Settings"
    Then RIMMSQOL holds no choice for the main button "RimBabel_Settings"
    And RIMMSQOL's settings file records no choice for the main button "RimBabel_Settings"
