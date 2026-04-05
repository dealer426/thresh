#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sends a push notification to your phone and waits for a response.
    Supports Approve/Reject buttons and free-text replies from ntfy app.

.DESCRIPTION
    Two-way communication bridge between Copilot (desktop) and your phone.
    1. Sends a question/prompt to your phone via ntfy.sh
    2. Adds Approve/Reject action buttons to the notification
    3. Polls a response topic until you reply
    4. Outputs your response so Copilot can continue

.EXAMPLE
    ./notify-and-wait.ps1 -Message "Deploy to production? The tests all passed."
    # Phone gets notification with Approve/Reject buttons
    # You tap Approve or type a custom reply
    # Script outputs: "approved" (or your typed response)

.EXAMPLE
    ./notify-and-wait.ps1 -Message "Which DB provider: sqlite or postgres?" -NoButtons
    # Phone gets notification, you type your answer in ntfy app
    # Script outputs whatever you typed
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Message,

    [string]$QuestionTopic = "burns-copilot-questions",
    [string]$ResponseTopic = "burns-copilot-responses",
    [string]$Title = "Copilot Needs Input",

    [switch]$NoButtons,

    [int]$TimeoutMinutes = 30,
    [int]$PollIntervalSeconds = 5
)

$ErrorActionPreference = "Stop"

# Record the current time (unix epoch) so we only read responses after our question
$sinceTime = [int][double]::Parse((Get-Date -UFormat %s))

# Build the notification payload
$payload = @{
    topic    = $QuestionTopic
    title    = $Title
    message  = $Message
    priority = 4
    tags     = @("question", "computer")
}

# Add Approve/Reject action buttons unless disabled
if (-not $NoButtons) {
    $payload.actions = @(
        @{
            action  = "http"
            label   = "Approve"
            url     = "https://ntfy.sh/$ResponseTopic"
            method  = "POST"
            body    = "approved"
            clear   = $true
        },
        @{
            action  = "http"
            label   = "Reject"
            url     = "https://ntfy.sh/$ResponseTopic"
            method  = "POST"
            body    = "rejected"
            clear   = $true
        }
    )
    $payload.message += "`n`nTap a button OR reply to this notification. You can also publish to topic: $ResponseTopic"
}
else {
    $payload.message += "`n`nReply by publishing to topic: $ResponseTopic"
}

$json = $payload | ConvertTo-Json -Depth 5

# Send the notification
try {
    Invoke-RestMethod -Uri "https://ntfy.sh" -Method Post -Body $json -ContentType "application/json" | Out-Null
    Write-Host ">> Notification sent. Waiting for your response..." -ForegroundColor Cyan
    Write-Host ">> Topic: $ResponseTopic | Timeout: $TimeoutMinutes min" -ForegroundColor DarkGray
}
catch {
    Write-Error "Failed to send notification: $_"
    exit 1
}

# Poll the response topic for a reply
$deadline = (Get-Date).AddMinutes($TimeoutMinutes)
$response = $null

while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds $PollIntervalSeconds

    try {
        $pollUrl = "https://ntfy.sh/$ResponseTopic/json?poll=1&since=$sinceTime"
        $webResp = Invoke-WebRequest -Uri $pollUrl -Method Get -ErrorAction SilentlyContinue -UseBasicParsing

        # Handle Content as byte array or string
        $raw = $webResp.Content
        if ($raw -is [byte[]]) {
            $raw = [System.Text.Encoding]::UTF8.GetString($raw)
        }

        if ($raw -and $raw.Trim() -ne "") {
            $lines = $raw -split "`n" | Where-Object { $_.Trim() -ne "" }
            foreach ($line in $lines) {
                try {
                    $msg = $line | ConvertFrom-Json -ErrorAction Stop
                    if ($msg.event -eq "message" -and $msg.message) {
                        $response = $msg.message.Trim()
                        break
                    }
                } catch {
                    # skip unparseable lines
                }
            }
        }
    }
    catch {
        # Transient network errors — keep polling
    }

    if ($response) { break }
}

if (-not $response) {
    Write-Host ">> Timed out after $TimeoutMinutes minutes with no response." -ForegroundColor Yellow
    Write-Output "TIMEOUT: No response received"
    exit 1
}

Write-Host ">> Response received: $response" -ForegroundColor Green
Write-Output $response
