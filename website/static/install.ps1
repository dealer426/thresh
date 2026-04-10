#Requires -Version 5.1
<#
.SYNOPSIS
    Installs thresh on Windows.
.DESCRIPTION
    Downloads and installs the latest thresh release from GitHub.
    thresh is an AI-powered cross-platform environment manager.
.EXAMPLE
    irm https://thresh.sh/install.ps1 | iex
#>

$ErrorActionPreference = 'Stop'

$Repo        = 'dealer426/thresh'
$Asset       = 'thresh-windows-x64.zip'
$InstallDir  = "$env:ProgramFiles\thresh"
$BinaryName  = 'thresh.exe'

Write-Host "Fetching latest thresh release..." -ForegroundColor Cyan

$releaseUrl = "https://api.github.com/repos/$Repo/releases/latest"
try {
    $release    = Invoke-RestMethod -Uri $releaseUrl -UseBasicParsing
    $latestTag  = $release.tag_name
} catch {
    Write-Error "Failed to fetch release information from GitHub: $_"
    exit 1
}

if (-not $latestTag) {
    Write-Error "Could not determine the latest release version."
    exit 1
}

$downloadUrl = "https://github.com/$Repo/releases/download/$latestTag/$Asset"

Write-Host "Downloading thresh $latestTag for Windows x64..." -ForegroundColor Cyan

$tmpDir   = Join-Path $env:TEMP "thresh-install-$(Get-Random)"
$zipPath  = Join-Path $tmpDir $Asset

try {
    New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null

    Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath -UseBasicParsing

    Write-Host "Extracting..." -ForegroundColor Cyan
    Expand-Archive -Path $zipPath -DestinationPath $tmpDir -Force

    $binarySource = Join-Path $tmpDir $BinaryName
    if (-not (Test-Path $binarySource)) {
        Write-Error "Unexpected archive structure: '$BinaryName' not found after extraction."
        exit 1
    }

    Write-Host "Installing thresh to $InstallDir..." -ForegroundColor Cyan
    if (-not (Test-Path $InstallDir)) {
        New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
    }
    Copy-Item -Path $binarySource -Destination (Join-Path $InstallDir $BinaryName) -Force

    # Add InstallDir to the system PATH if not already present
    $machinePath = [Environment]::GetEnvironmentVariable('Path', 'Machine')
    if ($machinePath -notlike "*$InstallDir*") {
        Write-Host "Adding thresh to system PATH..." -ForegroundColor Cyan
        [Environment]::SetEnvironmentVariable(
            'Path',
            "$machinePath;$InstallDir",
            'Machine'
        )
        # Also update the current session PATH so thresh is immediately usable
        $env:Path += ";$InstallDir"
    }

    Write-Host ""
    Write-Host "thresh $latestTag installed successfully!" -ForegroundColor Green
    Write-Host ""
    try { & "$InstallDir\$BinaryName" version } catch {}
    Write-Host ""
    Write-Host "Get started: thresh --help"
    Write-Host "Documentation: https://thresh.sh/docs"
} finally {
    Remove-Item -Path $tmpDir -Recurse -Force -ErrorAction SilentlyContinue
}
