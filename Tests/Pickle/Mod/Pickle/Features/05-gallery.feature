# Pictures for the Workshop gallery, for a person to look at and keep: nothing is asserted. Every scenario is @review.
# Staged on the Sanctuary of Nelim (SanctuaryBacklot, save Nelims-tribe), in the pass `sanctuary` (wsl-deps.sanctuary.map).
#
# The story: Nelim, the only colonist of the Sanctuary, has a mod she cannot read in her language. She opens the game options, finds
# RimBabel among the mods that have settings, chooses the service that will translate (a key she has typed stays masked), says who
# signs the translation, and keeps a dictionary and a list of texts that must stay as they are. The pictures are the windows of
# that day; windows are screen captures of what they are and are not dressed (PUBLISHING.md), but a window that covers the map
# is taken on `window-backdrop-for-height`, the bamboo backdrop of the Sanctuary, and cropped sideways afterwards.
#
# The place was chosen on the empty photographs of every site (docs/SANCTUAIRE-LIEUX.md, PickleTools/docs/GALERIE.md): the settings
# page is a tall window (about 900 x 700 px), so `window-backdrop-for-height` and not `-for-width`. Invented values, a placeholder
# key (masked on the page) and the author "Your name": no capture can hold a real key or a real name.
@requires:nelim.pickletools.screenshotstudio
@requires:nelim.pickletools.clearscreen
Feature: RimBabel's windows, as pictures for the gallery

  Background:
    Given the save "Nelims-tribe" is loaded
    And RimBabel: settings are at their defaults
    And RimBabel: the settings hold example values for a picture
    And I close all dialogs
    And Nelim's Sanctuary: I am at the sanctuary "window-backdrop-for-height"
    And Nelim's Pickle Tools: the tooltips are hidden
    And Nelim's Pickle Tools: the colonist bar is hidden
    And Nelim's Pickle Tools: the learning helper is hidden
    And Nelim's Pickle Tools: the resource readout is hidden
    And Nelim's Pickle Tools: the alerts are hidden

  @review
  Scenario: Options, Mod options: RimBabel is among the mods that have settings
    When RimBabel: the game's options window is opened on Mod options
    And RimBabel: the settings window draws for 5 frames
    And Nelim's Pickle Tools: I move the mouse to (5, 5)
    And I take a screenshot "RimBabel in the Mod options list"
    And I close all dialogs

  @review
  Scenario: RimBabel's settings, the engines and the key
    When RimBabel: the game's options window is opened on Mod options
    And RimBabel: the settings window draws for 5 frames
    And RimBabel: RimBabel is chosen in the Mod options list
    And RimBabel: the settings window draws for 10 frames
    And Nelim's Pickle Tools: I move the mouse to (5, 5)
    And I take a screenshot "RimBabel settings, engines"
    And I close all dialogs

  @review
  Scenario: RimBabel's settings, the dictionary and the texts that stay as they are
    When RimBabel: the game's options window is opened on Mod options
    And RimBabel: the settings window draws for 5 frames
    And RimBabel: RimBabel is chosen in the Mod options list
    And RimBabel: the settings page is scrolled down by 560 pixels
    And RimBabel: the settings window draws for 10 frames
    And Nelim's Pickle Tools: I move the mouse to (5, 5)
    And I take a screenshot "RimBabel settings, dictionary and blacklist"
    And I close all dialogs
