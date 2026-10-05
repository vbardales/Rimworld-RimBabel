<#
.SYNOPSIS
  Sets up a free translation engine on this computer: Ollama and one model, ready for RimBabel's "OpenAI-compatible server" engine.
.DESCRIPTION
  1. Installs Ollama with winget when it is not there.
  2. Downloads one model (default qwen2.5:7b, about 4.7 GB, good at many languages; use -Model qwen2.5:3b on a small machine).
  3. Checks that the server answers on http://localhost:11434.
  Then, in the game: Options, Mod options, RimBabel, engine "OpenAI-compatible server", address http://localhost:11434/v1,
  model as printed below, no key. Nothing here is sent anywhere: the texts stay on this computer.
  Run: powershell -ExecutionPolicy Bypass -File Tools\Install-Ollama.ps1
#>
param([string]$Model = 'qwen2.5:7b')
$ErrorActionPreference = 'Stop'

if (-not (Get-Command ollama -ErrorAction SilentlyContinue)) {
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        throw 'winget is not available. Install Ollama by hand from https://ollama.com/download and run this script again.'
    }
    Write-Host 'Installing Ollama...'
    winget install --id Ollama.Ollama -e --accept-package-agreements --accept-source-agreements
    # A fresh install is not on this session's PATH yet.
    $env:Path = [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [Environment]::GetEnvironmentVariable('Path', 'User')
    if (-not (Get-Command ollama -ErrorAction SilentlyContinue)) { throw 'Ollama is installed but not found: open a new terminal and run this script again.' }
}

Write-Host "Downloading the model $Model (the first time only)..."
ollama pull $Model

try {
    Invoke-RestMethod -Uri 'http://localhost:11434/api/tags' -TimeoutSec 10 | Out-Null
} catch {
    Write-Host 'Starting the server...'
    Start-Process ollama -ArgumentList 'serve' -WindowStyle Hidden
    Start-Sleep -Seconds 5
    Invoke-RestMethod -Uri 'http://localhost:11434/api/tags' -TimeoutSec 10 | Out-Null
}

Write-Host ''
Write-Host 'Ready. In RimBabel (Mod options): engine "OpenAI-compatible server"'
Write-Host '  address: http://localhost:11434/v1'
Write-Host "  model:   $Model"
Write-Host '  key:     leave empty'
