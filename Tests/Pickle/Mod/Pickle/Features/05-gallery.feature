# Pictures for the Workshop gallery, for a person to look at and keep: nothing is asserted. Every scenario is @review.
# Invented values, a placeholder key (masked on the page) and the author "Your name": no capture can hold a real key or a real name.
# The pictures are taken in English and in French by the two passes; Art/Gallery/ keeps the ones that are chosen.
# Known limit: a tooltip may sit under the pointer; Pickle has no step to move it, so retake or crop if one covers a control.
Feature: RimBabel's pages, as pictures for the gallery

  Background:
    Given the save "test-colony" is loaded
    And RimBabel: settings are at their defaults
    And RimBabel: the settings hold example values for a picture
    And I close all dialogs

  @review
  Scenario: Options, Mod options: the list holds RimBabel
    When RimBabel: the game's options window is opened on Mod options
    And RimBabel: the settings window draws for 5 frames
    And I take a screenshot "RimBabel in the Mod options list"
    And I close all dialogs

  @review
  Scenario: RimBabel's page from Mod options, top: engines and key
    When RimBabel: the game's options window is opened on Mod options
    And RimBabel: the settings window draws for 5 frames
    And RimBabel: RimBabel is chosen in the Mod options list
    And RimBabel: the settings window draws for 10 frames
    And I take a screenshot "RimBabel page, engines"
    And I close all dialogs

  @review
  Scenario: RimBabel's page, packages, dictionary and blacklist
    When RimBabel's settings shortcut is activated
    And RimBabel: the settings page is scrolled down by 560 pixels
    And RimBabel: the settings window draws for 10 frames
    And I take a screenshot "RimBabel page, packages and dictionary"
    And I close all dialogs
