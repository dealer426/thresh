[Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

$body = '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"get_version","arguments":{"node_id":"df312049-ca8e-4fb4-ae2c-cba4fbcf0b65"}}}'

$headers = @{
    Authorization = "Bearer thresh_cli_1ac0a25e-3ed1-4375-b8e6-3bef1ef39cf5_DIK6B09reEJDkVCOik7ojkgXYIke56C7dhY00Qy6WDc"
}

$result = Invoke-RestMethod -Uri "https://192.168.4.85:7200/mcp" -Method Post -ContentType "application/json" -Headers $headers -Body $body
$result | ConvertTo-Json -Depth 10
