# The settings across a real restart of the game: launch 2 of 3, read. Nothing ran in this process before: what RimBabel's settings
# hold now is what the game loaded from the file launch 1 left. See 06-restart-write.feature for the command.
Feature: RimBabel's settings are read back by the next launch (2 of 3, read)

  Scenario: the game starts with the values the previous launch wrote
    Given the main menu is open
    Then RimBabel: the settings the game loaded at startup hold the values of the restart test
    And no errors were logged
