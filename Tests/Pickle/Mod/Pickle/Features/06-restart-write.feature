# The settings across a real restart of the game: launch 1 of 3, write. A value read back in the process that wrote it is still in
# memory and proves nothing; launch 2 (07) reads what the game loaded at startup. The chain is three launches under ONE hold of the
# lock, with a real array for -Then and -NoBatch:
#   Submit-PickleRun.ps1 -Mod RimBabel -Owner <id> -Label "..." -NoBatch -EvidenceDir RimBabel/Tests/Pickle/Evidence/restart-en `
#       -Filter '06-restart-write' -Then '07-restart-read','08-restart-reset'
# THIS LAUNCH LEAVES ITS VALUES BEHIND ON PURPOSE: the last step says so, and it comes last, so a scenario that fails before it puts
# everything back. If the chain is cut after that step, the values stay in RimBabel's settings file (Config/Mod_RimBabel_RimBabelMod.xml
# of the WSL profile) until a run of 08-restart-reset.
# In a full pass of the suite the three files run in one process, in order: that proves only that the steps hold together, not the
# restart. Only the chained run proves it.
Feature: RimBabel's settings are written for the next launch (1 of 3, write)

  Scenario: the settings are written, and the file holds them
    Given the main menu is open
    When RimBabel: the settings are set to the values of the restart test and written
    Then RimBabel: the settings file holds the values of the restart test
    And no errors were logged
    And RimBabel: the values of the restart test are kept for the next launch
