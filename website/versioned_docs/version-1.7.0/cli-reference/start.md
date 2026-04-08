---
sidebar_position: 12
title: thresh start
description: Start a stopped environment
---

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# thresh start

Start a stopped environment.

## Synopsis

<Tabs groupId="operating-systems">
  <TabItem value="windows" label="Windows" default>

```powershell
thresh start <environment-name>
```

  </TabItem>
  <TabItem value="linux" label="Linux">

```bash
thresh start <environment-name>
```

  </TabItem>
</Tabs>

## Description

The `start` command starts a stopped environment:

**Windows (WSL2):**
- Starts the WSL distribution
- Services automatically accessible via localhost (WSL2 automatic port forwarding)

**Linux/macOS (containerd):**
- Starts the container
- Port mappings defined in blueprint are restored

This is useful after:
- Manual `thresh stop` command
- System shutdown or restart
- `wsl --shutdown` (Windows only)

## Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `<environment-name>` | Yes | Name of the environment to start |

## Examples

### Basic Usage

<Tabs groupId="operating-systems">
  <TabItem value="windows" label="Windows" default>

```powershell
# Start a stopped environment
PS C:\> thresh start webserver
Starting environment 'webserver'...
✅ Environment 'webserver' started successfully

# Access services via localhost (automatic WSL2 forwarding)
PS C:\> curl http://localhost:8080
```

  </TabItem>
  <TabItem value="linux" label="Linux">

```bash
# Start a stopped container
$ thresh start webserver
Starting environment 'webserver'...
✅ Environment 'webserver' started successfully

# Access via published ports
$ curl http://localhost:8080
```

  </TabItem>
</Tabs>

## Port Access

### Windows (WSL2)

WSL2 automatically forwards all listening ports to Windows localhost:

```json
{
  "EnvironmentName": "webserver",
  "Ports": ["8080:80", "8443:443"],
  "Network": "bridge"
}
```

**Generated Rules:**
```powershell
netsh interface portproxy add v4tov4 listenport=8080 listenaddress=0.0.0.0 connectport=80 connectaddress=172.24.123.45
netsh interface portproxy add v4tov4 listenport=8443 listenaddress=0.0.0.0 connectport=443 connectaddress=172.24.123.45
```

### Verify Port Forwarding

```powershell
# Check all Windows port forwarding rules
netsh interface portproxy show all

# Test connectivity
curl http://localhost:8080
```

## Linux & macOS Behavior

On Linux and macOS, `thresh start` simply starts the container/environment:

```bash
# No additional port forwarding needed
thresh start webserver

# Ports work via native Docker networking
curl http://localhost:8080
```

## Common Workflows

### After Windows Restart

```powershell
# Windows restarted, WSL port forwarding is lost
thresh start webserver
# ✓ Port forwarding automatically restored
```

### After `wsl --shutdown`

```powershell
# Manual WSL shutdown
wsl --shutdown

# Restart environment with networking
thresh start postgres-dev
# ✓ Environment started with port forwarding
```

### Multiple Environments

```powershell
# Start multiple environments
thresh start web-frontend
thresh start api-backend
thresh start postgres-db

# All port mappings restored for each environment
```

## Troubleshooting

### Port Already in Use

**Issue:** Port is already bound on Windows

```powershell
# Find process using the port
netstat -ano | findstr :8080

# Kill the process
taskkill /PID 12345 /F

# Retry start
thresh start webserver
```

### Port Forwarding Not Working

**Issue:** Services not accessible from Windows

```powershell
# 1. Check WSL IP
wsl -d thresh-webserver -- hostname -I

# 2. Verify service is running inside WSL
wsl -d thresh-webserver -- ss -tlnp | grep :80

# 3. Test from inside WSL
wsl -d thresh-webserver -- curl http://localhost:80

# 4. Check Windows firewall
# Ensure port 8080 is allowed

# 5. Manually verify port forwarding rules
netsh interface portproxy show all
```

### Environment Already Running

**Issue:** Environment is already started

```powershell
thresh start webserver
# Output: Environment 'webserver' is already running

# Check status
wsl -l -v | findstr thresh-webserver
```

### Metadata Not Found

**Issue:** Environment metadata missing

```powershell
# Check if metadata exists
ls ~/.thresh/metadata/webserver.json

# If missing, port forwarding won't be configured
# Recreate environment from blueprint:
thresh destroy webserver -y
thresh up webserver
```

## Best Practices

### Automatic Startup

Create a Windows Task Scheduler task to auto-start environments on login:

**PowerShell Script:** `start-dev-envs.ps1`
```powershell
# Auto-start development environments
thresh start web-frontend
thresh start api-backend
thresh start postgres-db
```

**Task Scheduler:**
- Trigger: At log on
- Action: PowerShell -File "C:\scripts\start-dev-envs.ps1"

### Health Checks

Verify services are accessible after starting:

```powershell
# Start environment
thresh start webserver

# Wait for service to be ready
Start-Sleep -Seconds 3

# Health check
$response = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing
if ($response.StatusCode -eq 200) {
    Write-Host "✓ Web server is healthy"
}
```

### Startup Scripts

Run initialization commands after starting:

```powershell
# Start environment
thresh start postgres-dev

# Run migrations
wsl -d thresh-postgres-dev -- psql -U postgres -f /app/migrations/001_init.sql
```

## Related Commands

- [`thresh stop`](/docs/cli-reference/stop) - Stop environment and remove port forwarding
- [`thresh up`](/docs/cli-reference/up) - Create new environment
- [`thresh list`](/docs/cli-reference/list) - List all environments
- [`thresh metrics`](/docs/cli-reference/metrics) - View environment metrics

## Reference

### Metadata File Location

- **Windows:** `C:\Users\<username>\.thresh\metadata\{environmentName}.json`
- **Linux:** `/home/<username>/.thresh/metadata/{environmentName}.json`
- **macOS:** `/Users/<username>/.thresh/metadata/{environmentName}.json`

### Metadata Schema

```json
{
  "EnvironmentName": "string",
  "Ports": ["hostPort:containerPort"],
  "Expose": ["port"],
  "Network": "bridge|host|none",
  "Hostname": "string",
  "Volumes": [...],
  "BindMounts": [...],
  "Tmpfs": [...]
}
```

### Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `4` | Environment not found |
| `5` | Failed to apply port forwarding |

## Next Steps

- [thresh stop](/docs/cli-reference/stop) - Stop environments
- [Networking & Port Mapping](/docs/tutorials/networking) - Configure networking
- [MCP Integration](/docs/mcp-integration) - Use with AI editors
