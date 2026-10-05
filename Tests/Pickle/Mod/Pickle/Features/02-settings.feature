# The settings page and its hidden shortcut: the part of MOD_SETTINGS.md that needs a running game.
#
# What the offline tests already prove and is NOT repeated here: the engine configuration and its problems, the two line lists,
# every key the page asks for existing in English and French, the shortcut's declaration (hidden, usable without a map, its
# worker), the settings round trip through the game's own Scribe, and the masking of a key.
#
# What only a game answers: that the page's drawing code runs without a logged error (an exception in OnGUI is logged, not thrown),
# which mod the dialog belongs to, what the main bar's own worker does with the shortcut, that the game writes the settings
# file, and that the language of the pass really holds every text. What stays manual: revealing the shortcut inside RIMMSQOL's own
# interface (that is RIMMSQOL's behaviour, not ours), and whether its visibility choice survives a restart.
#
# The save is needed because the main bar's worker answers differently without a map. Each scenario starts from default settings
# and puts the player's own back afterwards.
Feature: RimBabel's settings page and its hidden shortcut

  Background:
    Given the save "test-colony" is loaded
    And RimBabel: settings are at their defaults
    And I close all dialogs

  Scenario: the shortcut is hidden on a clean configuration, drawn and live once revealed, gone again when hidden
    Then RimBabel's settings shortcut is hidden on a clean configuration
    When RimBabel's settings shortcut is revealed, as a customization mod would
    Then RimBabel's settings shortcut is drawn in the bar
    When RimBabel's settings shortcut is hidden again
    Then RimBabel's settings shortcut is not drawn in the bar

  Scenario: activating the shortcut opens this mod's settings, and the page draws without an error
    When RimBabel's settings shortcut is activated
    Then a settings dialog is open for RimBabel
    When RimBabel: the settings window draws for 10 frames
    Then no errors were logged
    When I close all dialogs

  Scenario: a changed setting reaches the file the game writes, and no key is there when none was set
    When RimBabel: I set the author name to "Pickle Tester"
    And RimBabel: the settings are written
    Then RimBabel: the settings file holds "Pickle Tester"
    And RimBabel: the settings file holds no key

  Scenario: every text of the page exists in the language of this pass
    Then RimBabel: every settings text exists in the language of this pass

  # @review: the page as a player sees it, for a person to read. Nothing about its contents is asserted: raw keys, clipping, a
  # control hidden behind another and the layout at this resolution are things only an eye catches, and in the French pass this is
  # where a text too long for its row shows up.
  @review
  Scenario: the settings page, for a person to look at
    When RimBabel's settings shortcut is activated
    And RimBabel: the settings window draws for 10 frames
    And I take a screenshot "RimBabel settings page"
    And I close all dialogs

  # The primary route of MOD_SETTINGS.md, through the game's own options window. A click on the mod's name opens the mod's own
  # settings window over it (Dialog_ModSettings); a real pointer click is not something Pickle sends to an immediate-mode window.
  Scenario: Options, Mod options lists RimBabel and its page draws from there
    When RimBabel: the game's options window is opened on Mod options
    And RimBabel: the settings window draws for 5 frames
    Then the Mod options list holds RimBabel
    When RimBabel: RimBabel is chosen in the Mod options list
    Then a settings dialog is open for RimBabel
    And RimBabel: the settings window draws for 10 frames
    Then no errors were logged
    When I close all dialogs

  # What the next start would read: the game's loader on the file just written, not the object in memory. Two lines or more of
  # text, since a Windows save reads line breaks back as CR LF.
  Scenario: values survive a write and a read of the file, including line breaks
    When RimBabel: I set the author name to "Pickle Reader"
    And RimBabel: I set the dictionary to 3 lines
    And RimBabel: the settings are written
    Then RimBabel: the file read back by the game holds author "Pickle Reader" and a dictionary of 3 lines
