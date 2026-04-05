# Run elevated - appends the missing AuthorizedKeysFile line and restarts sshd
Add-Content -Path "C:\ProgramData\ssh\sshd_config" -Value "       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys"
Restart-Service sshd
Write-Host "Done - line added and sshd restarted" -ForegroundColor Green
Read-Host "Press Enter to close"
