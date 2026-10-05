# The settings across a real restart of the game: launch 3 of 3, reset. Puts RimBabel's settings back to their defaults and shows
# that nothing of the test remains in the file. Run it alone after a chain that was cut. See 06-restart-write.feature.
Feature: RimBabel's settings are put back after the restart test (3 of 3, reset)

  Scenario: the values of the restart test are gone
    Given the main menu is open
    When RimBabel: the settings are put back to their defaults and written
    Then RimBabel: no value of the restart test remains in the settings file
    And no errors were logged
