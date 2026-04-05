#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Fire-and-forget notification (no response needed). For progress updates or FYI alerts.
.EXAMPLE
    ./notify-phone.ps1 -Message "Build completed successfully. Moving to tests."
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Message,

    [string]$Topic = "burns-copilot-questions",
    [string]$Title = "Copilot Update",
    [int]$Priority = 3
)

$body = @{
    topic    = $Topic
    title    = $Title
    message  = $Message
    priority = $Priority
    tags     = @("computer")
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://ntfy.sh" -Method Post -Body $body -ContentType "application/json" | Out-Null
Write-Host "Notification sent." -ForegroundColor Cyan
