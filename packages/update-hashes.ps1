# Update Package Hashes Script
# Run this after v1.0.0 GitHub Release completes

param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "=== thresh Package Hash Updater ===" -ForegroundColor Cyan
Write-Host ""

# Download release zip
$zipUrl = "https://github.com/dealer426/eknova/releases/download/v$Version/thresh-windows-x64.zip"
$zipPath = "$env:TEMP\thresh-windows-x64.zip"

Write-Host "Downloading release zip..." -ForegroundColor Yellow
Write-Host "URL: $zipUrl"
Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath

# Calculate SHA256
Write-Host "`nCalculating SHA256..." -ForegroundColor Yellow
$hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash
Write-Host "SHA256: $hash" -ForegroundColor Green

# Update winget manifest
Write-Host "`nUpdating winget manifest..." -ForegroundColor Yellow
$wingetFile = "packages/winget/dealer426.thresh.installer.yaml"
$content = Get-Content $wingetFile -Raw
$content = $content -replace '<TO_BE_UPDATED_AFTER_RELEASE>', $hash
Set-Content $wingetFile $content
Write-Host "✓ Updated $wingetFile" -ForegroundColor Green

# Update chocolatey script
Write-Host "Updating chocolatey install script..." -ForegroundColor Yellow
$chocoFile = "packages/chocolatey/tools/chocolateyinstall.ps1"
$content = Get-Content $chocoFile -Raw
$content = $content -replace '<TO_BE_UPDATED_AFTER_RELEASE>', $hash
Set-Content $chocoFile $content
Write-Host "✓ Updated $chocoFile" -ForegroundColor Green

# Update scoop manifest
Write-Host "Updating scoop manifest..." -ForegroundColor Yellow
$scoopFile = "packages/scoop/thresh.json"
$content = Get-Content $scoopFile -Raw
$content = $content -replace '<TO_BE_UPDATED_AFTER_RELEASE>', $hash
Set-Content $scoopFile $content
Write-Host "✓ Updated $scoopFile" -ForegroundColor Green

# Clean up
Remove-Item $zipPath

Write-Host "`n=== Update Complete ===" -ForegroundColor Cyan
Write-Host "SHA256: $hash" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Review changes: git diff packages/"
Write-Host "2. Commit: git add packages/ && git commit -m 'chore: Update package hashes for v$Version'"
Write-Host "3. Push: git push origin main"
Write-Host "4. Follow submission guide in packages/README.md"
Write-Host ""
