[Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

$headers = @{
    Authorization = "Bearer thresh_cli_1ac0a25e-3ed1-4375-b8e6-3bef1ef39cf5_DIK6B09reEJDkVCOik7ojkgXYIke56C7dhY00Qy6WDc"
}

$result = Invoke-RestMethod -Uri "https://192.168.4.85:7200/api/agents" -Headers $headers
$result | ConvertTo-Json -Depth 5
