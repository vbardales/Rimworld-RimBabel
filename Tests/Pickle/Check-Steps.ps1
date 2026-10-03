<#
.SYNOPSIS
  Compile every step pattern of this suite, and check that none of them is ambiguous against any
  other suite in the collection or against Pickle's own vocabulary. No game, a few seconds.

.DESCRIPTION
  Written after a whole run was lost to a pattern that looked right and was not.

  On 2026-09-21 the first WSL pass of this suite played zero scenarios and reported
  `exitReason: infrastructure-error`. The cause was one character class: in a Cucumber Expression,
  parentheses mean OPTIONAL TEXT, so `at ({int}, {int})` is not "a cell" but an optional group
  containing parameters - which is illegal. Pickle refused to build its step table at all, so not
  one scenario of the nineteen ran. The fix is to escape them, `at \({int}, {int}\)`, which in a C#
  literal is written `\\(` and `\\)`. A slash is alternation, so "reveals/hides" is two words to
  choose between rather than one word with a slash in it.

  An invalid pattern costs the whole run, not the scenario using it, and the machine is shared: a
  lost run is also everybody else's forty minutes in the queue. That is worth a few seconds here.

  The ambiguity half came back from PickleTools, whose own Check-Steps grew out of an earlier copy of
  this file. It is the check this one lacked, and the one a text comparison cannot do: two DIFFERENT
  expressions can both match the same line. Pickle loads the steps of every suite installed in a run
  into one namespace and matches on the text alone, so a line two expressions match is an "Ambiguous
  step" that fails a healthy scenario.

  Four things, all against Pickle's own engine rather than against a guess:

    1. Every pattern this suite declares COMPILES, with the PickleParameterTypeRegistry the game uses.
    2. No pattern is declared twice.
    3. No step line anywhere in the collection is matched by one of this suite's expressions AND by
       anything else - another suite, or Pickle's own vocabulary read out of its assemblies.
    4. Every step line of THIS suite's features matches at least one expression.

  A pattern no feature uses is reported as weight, not as an error.

.EXAMPLE
  powershell.exe -ExecutionPolicy Bypass -File Tests/Pickle/Check-Steps.ps1
#>
param(
    [string]$PickleAssemblies = 'C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\3791648678\Assemblies',
    [string]$Cecil = "$env:USERPROFILE\.nuget\packages\mono.cecil\0.11.5\lib\net40\Mono.Cecil.dll"
)

$ErrorActionPreference = 'Stop'
$suite = $PSScriptRoot                                       # ...\BillAutopilot\Tests\Pickle
$mod   = Split-Path (Split-Path $suite -Parent) -Parent      # ...\BillAutopilot
$repo  = Split-Path $mod -Parent                             # ...\rimworld
$me    = Split-Path $mod -Leaf

foreach ($dll in 'CucumberExpressions.dll', 'RimWorks.Pickle.Core.dll') {
    $path = Join-Path $PickleAssemblies $dll
    if (-not (Test-Path $path)) {
        throw "$dll not found under $PickleAssemblies. Pass -PickleAssemblies with the installed Pickle mod's Assemblies folder."
    }
    [Reflection.Assembly]::LoadFrom($path) | Out-Null
}

$core = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'RimWorks.Pickle.Core' }
$registryType = $core.GetType('RimWorks.Pickle.Core.Steps.PickleParameterTypeRegistry')
if (-not $registryType) { throw 'PickleParameterTypeRegistry no longer exists: Pickle renamed it, update this script.' }
$registry = [Activator]::CreateInstance($registryType)

function New-Expr($pattern) { New-Object CucumberExpressions.CucumberExpression($pattern, $registry) }

# The attribute argument is a C# literal: undo its escaping to get the pattern Pickle really sees.
$attr = '\[(?:Given|When|Then)\("((?:[^"\\]|\\.)*)"'
function Read-Patterns($dir, $source) {
    foreach ($f in Get-ChildItem -LiteralPath $dir -Filter *.cs -ErrorAction SilentlyContinue) {
        $text = [IO.File]::ReadAllText($f.FullName)
        foreach ($m in [regex]::Matches($text, $attr)) {
            [pscustomobject]@{
                Source  = $source
                File    = $f.Name
                Pattern = ($m.Groups[1].Value -replace '\\\\', '\' -replace '\\"', '"')
            }
        }
    }
}

$bad = 0

# --- 1 and 2. this suite ---------------------------------------------------------------------------

$mine = @(Read-Patterns (Join-Path $suite 'Source') $me)
if ($mine.Count -eq 0) { throw "no step patterns under $suite\Source: the attribute shape this script looks for has changed." }

foreach ($g in ($mine | Group-Object Pattern | Where-Object { $_.Count -gt 1 })) {
    Write-Host "DUPLICATE  $($g.Name)" -ForegroundColor Red
    Write-Host "           declared $($g.Count) times, in $(($g.Group.File | Sort-Object -Unique) -join ', ')" -ForegroundColor DarkRed
    $bad++
}

$myExprs = @()
foreach ($d in $mine) {
    try {
        $myExprs += [pscustomobject]@{ Pattern = $d.Pattern; Regex = (New-Expr $d.Pattern).Regex; Used = $false }
    } catch {
        $e = $_.Exception; while ($e.InnerException) { $e = $e.InnerException }
        Write-Host "INVALID  $($d.File): $($d.Pattern)" -ForegroundColor Red
        Write-Host "         $(($e.Message -split "`n")[0])" -ForegroundColor DarkRed
        $bad++
    }
}

# --- everything else that shares the one namespace -------------------------------------------------

Add-Type -Path $Cecil
$others = @()

# Pickle's own vocabulary, read from its assemblies rather than from its documentation. Two carry
# steps: Vanilla, and the runner itself (the save steps, "no errors were logged").
foreach ($name in 'RimWorks.Pickle.Vanilla.dll', 'RimWorks.Pickle.dll') {
    $asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $PickleAssemblies $name))
    foreach ($t in $asm.MainModule.GetTypes()) {
        foreach ($m in $t.Methods) {
            foreach ($a in $m.CustomAttributes | Where-Object { $_.AttributeType.Name -in 'GivenAttribute', 'WhenAttribute', 'ThenAttribute' }) {
                $others += [pscustomobject]@{ Source = 'pickle'; Pattern = [string]$a.ConstructorArguments[0].Value }
            }
        }
    }
}
# Pickle's save-and-fixture steps are handled by the runner without an attribute the extraction
# above can see, so they have to be named here or every scenario that loads a save reads as
# unresolved. Each one is listed on evidence rather than on a guess:
#
#   the save {string} is loaded   every scenario of this suite begins with it, and 26 of them ran
#                                 on 2026-09-21; the features Pickle ships use it verbatim.
#   I save and reload             "the autopilot still owns its own bills" passed in that same run,
#                                 and it is that scenario's second step.
#   the save round trips          catalogued in the Pickle repository's Docs/steps.md, in the same
#                                 family as the two above. NOT yet seen to run: the one scenario
#                                 using it is where the run of 2026-09-21 stalled. If it ever comes
#                                 back undefined, this line is the reason it was not caught here.
foreach ($p in 'the save {string} is loaded', 'I save and reload', 'I save and reload as {string}', 'the save round trips') {
    $others += [pscustomobject]@{ Source = 'pickle-engine'; Pattern = $p }
}
$pickleCount = $others.Count

# Every OTHER suite of the collection, and the shared step assemblies of PickleTools. A junction at
# the top level would otherwise count a nested repository's suite twice.
$suiteDirs = @()
foreach ($top in Get-ChildItem -LiteralPath $repo -Directory -ErrorAction SilentlyContinue) {
    if ($top.Attributes -band [IO.FileAttributes]::ReparsePoint) { continue }
    $suiteDirs += $top.FullName
    foreach ($sub in Get-ChildItem -LiteralPath $top.FullName -Directory -ErrorAction SilentlyContinue) {
        if ($sub.Name -in 'Tests', 'Mod', 'Source', '.git', '.build', 'Art') { continue }
        if (Test-Path -LiteralPath (Join-Path $sub.FullName 'Source')) { $suiteDirs += $sub.FullName }
    }
}
$suiteDirs = @($suiteDirs | Sort-Object -Unique)

$sources = 0
foreach ($dir in $suiteDirs) {
    $name = Split-Path $dir -Leaf
    foreach ($src in @((Join-Path $dir 'Tests\Pickle\Source'), (Join-Path $dir 'Source'))) {
        if (-not (Test-Path -LiteralPath $src)) { continue }
        if ($src -like "$mod\*") { continue }        # this suite is "mine", not an "other"
        $found = @(Read-Patterns $src $name)
        if ($found.Count -eq 0) { continue }
        $sources++
        $others += $found
    }
}

$otherExprs = @()
foreach ($o in $others) {
    # A pattern of theirs that does not compile is their own check's business, not this one's.
    try { $otherExprs += [pscustomobject]@{ Source = $o.Source; Pattern = $o.Pattern; Regex = (New-Expr $o.Pattern).Regex } } catch { }
}

# --- 3 and 4. every step line ----------------------------------------------------------------------

$featureFiles = @()
foreach ($dir in $suiteDirs) {
    $fd = Join-Path $dir 'Tests\Pickle\Mod\Pickle\Features'
    if (Test-Path -LiteralPath $fd) { $featureFiles += Get-ChildItem -LiteralPath $fd -Filter *.feature }
}

$lines = 0
$ambiguous = @{}
$unresolved = @()
foreach ($file in $featureFiles) {
    $isMine = $file.FullName -like "$mod\*"
    foreach ($raw in [IO.File]::ReadAllLines($file.FullName)) {
        if ($raw.Trim() -notmatch '^(Given|When|Then|And|But)\s+(.+)$') { continue }
        $step = $Matches[2].Trim()
        if ($isMine) { $lines++ }

        $mineHit  = @($myExprs    | Where-Object { $_.Regex.IsMatch($step) })
        $otherHit = @($otherExprs | Where-Object { $_.Regex.IsMatch($step) })

        foreach ($h in $mineHit) { $h.Used = $true }

        # Ambiguity is only this suite's business when one of ITS expressions is involved.
        if ($mineHit.Count -gt 1 -or ($mineHit.Count -eq 1 -and $otherHit.Count -gt 0)) {
            $names = @($mineHit  | ForEach-Object { "$me `"$($_.Pattern)`"" }) +
                     @($otherHit | ForEach-Object { "$($_.Source) `"$($_.Pattern)`"" })
            $ambiguous[$step] = "$($file.Name): matches " + ($names -join ' AND ')
        }

        if ($isMine -and $mineHit.Count -eq 0 -and $otherHit.Count -eq 0) {
            $unresolved += "$($file.Name): $step"
        }
    }
}

# --- report ----------------------------------------------------------------------------------------

Write-Host ''
Write-Host "$($mine.Count) patterns declared, $($myExprs.Count) compile. Compared against $($otherExprs.Count) others: $pickleCount from Pickle, $($otherExprs.Count - $pickleCount) from $sources step sources. $lines step lines in this suite."

foreach ($k in $ambiguous.Keys) {
    Write-Host "AMBIGUOUS  $k" -ForegroundColor Red
    Write-Host "           $($ambiguous[$k])" -ForegroundColor DarkRed
    $bad++
}

$unused = @($myExprs | Where-Object { -not $_.Used })
if ($unused.Count -gt 0) {
    Write-Host ''
    Write-Host "$($unused.Count) pattern(s) no feature uses - weight, not coverage:" -ForegroundColor Yellow
    foreach ($u in $unused) { Write-Host "  $($u.Pattern)" -ForegroundColor Yellow }
}

if ($unresolved.Count -gt 0) {
    Write-Host ''
    Write-Host "$($unresolved.Count) step line(s) of this suite match no expression at all:" -ForegroundColor Red
    $unresolved | Sort-Object -Unique | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    $bad += $unresolved.Count
}

Write-Host ''
if ($bad -gt 0) {
    Write-Host "$bad PROBLEM(S). Pickle builds its whole step table before it runs anything, so an invalid pattern makes a run play zero scenarios, and an ambiguous one fails healthy scenarios." -ForegroundColor Red
    exit 1
}

Write-Host 'ALL PATTERNS COMPILE, NONE AMBIGUOUS, EVERY STEP LINE RESOLVES' -ForegroundColor Green
exit 0
