# Reload environment PATH in current PowerShell session
# This is useful after installing tools that modify the PATH

Write-Host "Reloading PATH environment variable..." -ForegroundColor Cyan

$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

Write-Host "✓ PATH reloaded" -ForegroundColor Green
Write-Host ""
Write-Host "Testing thresh:" -ForegroundColor Yellow
thresh --version

Write-Host ""
Write-Host "thresh is now available in this session!" -ForegroundColor Green
