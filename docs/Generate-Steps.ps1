<#
.SYNOPSIS
  Writes docs/steps.md, the catalogue of the Pickle steps of RimBabel, from the attributes in Source/.

.DESCRIPTION
  Nothing in docs/steps.md is written by hand: it reads every [Given(...)], [When(...)] and [Then(...)] attribute of
  Tests/Pickle/Source/*.cs (a pattern written as Prefix + "..." is resolved from the file's `const string Prefix`) and takes the first
  sentences of the XML summary above it as the description. A step with no summary is listed "(no description yet)" and the run says so.
  -Check changes nothing: it exits 1 if docs/steps.md is not what the sources would give.

.EXAMPLE
  powershell.exe -ExecutionPolicy Bypass -File SanctuaryBacklot/docs/Generate-Steps.ps1          # rewrite docs/steps.md
  powershell.exe -ExecutionPolicy Bypass -File SanctuaryBacklot/docs/Generate-Steps.ps1 -Check   # verify it is current
#>
[CmdletBinding()]
param([switch]$Check)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out  = Join-Path $PSScriptRoot 'steps.md'
$utf8 = New-Object Text.UTF8Encoding($false)
$attr = [regex]'\[(Given|When|Then)\(\s*((?:[A-Za-z_]+\s*\+\s*)?)"((?:[^"\\]|\\.)*)"[^\]]*\]'

function Shorten([string]$s) {
    $sent = [regex]::Split($s.Trim(), '(?<=\.)\s+(?=\S)')
    $take = ''
    foreach ($x in $sent) { if (($take + ' ' + $x).Length -gt 340 -and $take) { break }; $take = ($take + ' ' + $x).Trim() }
    if ($take.Length -le 340) { return $take }
    $cut = $take.Substring(0, 337); $cut = $cut.Substring(0, $cut.LastIndexOf(' '))
    if ((($cut.ToCharArray() | Where-Object { $_ -eq '`' }).Count % 2) -eq 1) { $cut = $cut.Substring(0, $cut.LastIndexOf('`')).TrimEnd() }
    return $cut.TrimEnd(',', ';', ':', ' ') + '...'
}
function Get-Summary([string]$text, [int]$index) {
    $lines = $text.Substring(0, $index).Split("`n"); $doc = @()
    for ($i = $lines.Count - 2; $i -ge 0; $i--) {
        $l = $lines[$i].TrimEnd("`r")
        if ($l -match '^\s*///') { $doc = @($l) + $doc; continue }
        if ($l -match '^\s*\[' -or $l -match '^\s*//[^/]') { continue }
        break
    }
    if (-not $doc) { return '' }
    $m = [regex]::Match((($doc | ForEach-Object { $_ -replace '^\s*///\s?', '' }) -join ' '), '<summary>(.*?)</summary>')
    if (-not $m.Success) { return '' }
    $s = $m.Groups[1].Value
    $s = [regex]::Replace($s, '<c>(.*?)</c>', '`$1`')
    $s = [regex]::Replace($s, '<see cref="([^"]*)"\s*/>', '$1')
    $s = [regex]::Replace($s, '<[^>]+>', '')
    $s = ($s -replace '\s+', ' ').Trim() -replace '&lt;', '<' -replace '&gt;', '>' -replace '&amp;', '&'
    return Shorten $s
}

$steps = @(); $empty = @()
foreach ($f in Get-ChildItem (Join-Path $root 'Tests\Pickle\Source') -Filter '*.cs' -File | Sort-Object Name) {
    $text = [IO.File]::ReadAllText($f.FullName); $prefix = ''
    $pm = [regex]::Match($text, 'const string Prefix\s*=\s*"((?:[^"\\]|\\.)*)"')
    if ($pm.Success) { $prefix = $pm.Groups[1].Value }
    foreach ($m in $attr.Matches($text)) {
        $pattern = $m.Groups[3].Value
        if ($m.Groups[2].Value.Trim()) { $pattern = $prefix + $pattern }
        $pattern = $pattern -replace '\\\\', '\' -replace '\\"', '"'
        $desc = Get-Summary $text $m.Index
        if (-not $desc) { $empty += $pattern; $desc = '(no description yet)' }
        $steps += [pscustomobject]@{ Keyword = $m.Groups[1].Value; Pattern = $pattern; Desc = $desc }
    }
}
$lines = @("# Steps of RimBabel", '',
 "The Pickle steps this repository ships. **Pickle's own steps are in its catalogue:**",
 "[Docs/steps.md](https://github.com/RimWorks/Rimworld-Pickle/blob/main/Docs/steps.md). The generic tools (overlay hiding, rectangle framing, waiting, apparel) are in",
 "[Nelim's Pickle Tools](https://github.com/vbardales/Rimworld-Nelim-Pickle-Tools/blob/main/docs/steps.md). Every step here starts with ``RimBabel:`` or names RimBabel (see the note on prefixes in ``Tests/Pickle/README.md``).", '',
 "This file is **generated** from the ``[Given]``, ``[When]`` and ``[Then]`` attributes of ``Tests/Pickle/Source/`` and the summary above them: do not edit it,",
 "run ``docs/Generate-Steps.ps1`` (``-Check`` verifies that it is current). $($steps.Count) steps. Pickle matches on the text alone, so a scenario may use",
 "``Given``, ``When``, ``Then`` or ``And`` as it reads best. The steps of the Sanctuary are in [SanctuaryBacklot/docs/steps.md](https://github.com/vbardales/Rimworld-Nelim-Sanctuary-Backlot/blob/main/docs/steps.md).", '',
 '| Step | Does |', '| --- | --- |')
foreach ($s in $steps) { $lines += "| ``$($s.Pattern -replace '\|','\|')`` ($($s.Keyword)) | $($s.Desc -replace '\|','\|') |" }
$doc = ($lines -join "`n") + "`n"
if ($Check) {
    $cur = if (Test-Path $out) { [IO.File]::ReadAllText($out).Replace("`r`n", "`n") } else { '' }
    if ($cur -eq $doc) { Write-Output "docs/steps.md is current: $($steps.Count) steps."; exit 0 }
    Write-Output 'docs/steps.md is not current: run docs/Generate-Steps.ps1'; exit 1
}
[IO.File]::WriteAllText($out, $doc, $utf8)
Write-Output "wrote $out ($($steps.Count) steps)"
if ($empty.Count) { Write-Output "$($empty.Count) step(s) with no summary:"; $empty | ForEach-Object { Write-Output "  $_" } }
