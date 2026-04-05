# Run this as Administrator to fix sshd_config
$configPath = "C:\ProgramData\ssh\sshd_config"
$lines = Get-Content $configPath

# Rebuild: keep everything up to and including "Match Group administrators",
# then add exactly one AuthorizedKeysFile line
$output = @()
$foundMatch = $false
foreach ($line in $lines) {
    if ($line -match '^\s*Match Group administrators') {
        $foundMatch = $true
        $output += $line
        $output += "       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys"
        continue
    }
    # Skip any existing AuthorizedKeysFile lines after the Match block
    if ($foundMatch -and $line -match '^\s*AuthorizedKeysFile') {
        continue
    }
    $output += $line
}

Set-Content -Path $configPath -Value ($output -join "`r`n") -Force
Restart-Service sshd
Write-Host "sshd_config fixed and sshd restarted." -ForegroundColor Green
Read-Host "Press Enter to close"
