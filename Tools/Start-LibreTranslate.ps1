<#
.SYNOPSIS
  Starts a LibreTranslate server on this computer with Docker, for RimBabel's "LibreTranslate" engine.
.DESCRIPTION
  Free and open source, nothing leaves the computer. Needs Docker Desktop. The first start downloads the language models
  for the languages given in -Languages (default English and French), which takes a few minutes.
  In the game: Options, Mod options, RimBabel, engine "LibreTranslate", address http://localhost:5000, no key.
  Stop it with: docker stop rimbabel-libretranslate
  Run: powershell -ExecutionPolicy Bypass -File Tools\Start-LibreTranslate.ps1 -Languages en,fr,de
#>
param([string[]]$Languages = @('en', 'fr'), [int]$Port = 5000)
$ErrorActionPreference = 'Stop'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker is not available. Install Docker Desktop from https://www.docker.com/products/docker-desktop and run this script again.'
}
$name = 'rimbabel-libretranslate'
if (docker ps -a --filter "name=^$name$" --format '{{.Names}}') { docker rm -f $name | Out-Null }
docker run -d --name $name -p "${Port}:5000" libretranslate/libretranslate --load-only ($Languages -join ',') | Out-Null
Write-Host "Starting... (the first start downloads the models; this can take a few minutes)"
for ($i = 0; $i -lt 90; $i++) {
    try { Invoke-RestMethod -Uri "http://localhost:$Port/languages" -TimeoutSec 5 | Out-Null; Write-Host "Ready on http://localhost:$Port"; return } catch { Start-Sleep -Seconds 5 }
}
throw "The server did not answer in time: see docker logs $name"
