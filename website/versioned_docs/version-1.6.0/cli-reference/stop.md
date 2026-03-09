---
sidebar_position: 13
title: thresh stop
description: Stop a running environment
---

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# thresh stop

Stop a running environment.

## Synopsis

<Tabs groupId="operating-systems">
  <TabItem value="windows" label="Windows" default>

```powershell
thresh stop <environment-name>
```

  </TabItem>
  <TabItem value="linux" label="Linux">

```bash
thresh stop <environment-name>
```

  </TabItem>
</Tabs>

## Description

The `stop` command stops a running environment:

**Windows (WSL2):**
- Terminates the WSL distribution
- All volumes and data preserved

**Linux/macOS (containerd):**
- Stops the container
- Volumes persist (can restart with `thresh start`)

Use when:
- Freeing system resources
- Temporarily disabling services
- System is low on memory

:::info Data Persistence
Stopping an environment does **not** delete any data. Volumes, configuration, and state are preserved. Use `thresh start` to resume.
:::

## Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `<environment-name>` | Yes | Name of the environment to stop |

## Examples

### Basic Usage

<Tabs groupId="operating-systems">
  <TabItem value="windows" label="Windows" default>

```powershell
# Stop a running environment
PS C:\> thresh stop webserver
Stopping environment 'webserver'...
✅ Environment 'webserver' stopped successfully
```

  </TabItem>
  <TabItem value="linux" label="Linux">

```bash
# Stop a running container
$ thresh stop webserver
Stopping environment 'webserver'...
✅ Environment 'webserver' stopped successfully
```

  </TabItem>
</Tabs>

### Stop Multiple Environments

```bash
# Stop all development environments
thresh stop web-frontend
thresh stop api-backend
thresh stop postgres-db
```

## Data Persistence

### How It Works

1. **Reads metadata** - Loads port configuration from `~/.thresh/metadata/{env}.json`
2. **Removes rules** - Deletes netsh portproxy rules for each mapped port
3. **Validates** - Ensures rules are fully removed
4. **Stops distribution** - Terminates the WSL distribution

### Example Cleanup

**Metadata:** `~/.thresh/metadata/webserver.json`
```json
{
  "EnvironmentName": "webserver",
  "Ports": ["8080:80", "8443:443"]
}
```

**Removed Rules:**
```powershell
netsh interface portproxy delete v4tov4 listenport=8080 listenaddress=0.0.0.0
netsh interface portproxy delete v4tov4 listenport=8443 listenaddress=0.0.0.0
```

### Verify Cleanup

```powershell
# Check remaining Windows port forwarding rules
netsh interface portproxy show all

# Should not show rules for stopped environment
```

## Linux & macOS Behavior

On Linux and macOS, `thresh stop` simply stops the container/environment:

```bash
# No additional cleanup needed
thresh stop webserver

# Port mappings automatically released by Docker
```

## Common Workflows

### Temporary Shutdown

```powershell
# Stop for maintenance
thresh stop webserver

# Make changes, update configuration...

# Resume
thresh start webserver
```

### Resource Management

```powershell
# Free up memory by stopping unused environments
thresh list
thresh stop old-project-1
thresh stop old-project-2

# Check resource usage
thresh metrics
```

### Before Windows Shutdown

```powershell
# Gracefully stop all environments before shutting down Windows
thresh stop web-app
thresh stop api-server
thresh stop database

# Prevents potential issues with WSL state
```

## Troubleshooting

### Environment Not Running

**Issue:** Environment is already stopped

```powershell
thresh stop webserver
# Output: Environment 'webserver' is not running

# Check status
wsl -l -v | findstr thresh-webserver
```

### Port Forwarding Cleanup Failed

**Issue:** netsh rules not removed

```powershell
# Manually remove port forwarding rules
netsh interface portproxy delete v4tov4 listenport=8080 listenaddress=0.0.0.0

# Verify
netsh interface portproxy show all
```

### Permission Denied

**Issue:** Cannot stop WSL distribution

```powershell
# Run PowerShell as Administrator
Start-Process powershell -Verb RunAs

# Retry stop command
thresh stop webserver
```

### Metadata Not Found

**Issue:** Warning about missing metadata

```powershell
# Stop still works, but port forwarding cleanup is skipped
thresh stop webserver
# Warning: Metadata not found, skipping port forwarding cleanup

# Distribution still stops successfully
```

## Data Persistence

### What Gets Preserved

✅ **Preserved when stopped:**
- Named volumes
- Environment metadata
- WSL configuration
- User data
- Application state

❌ **Lost when stopped:**
- Running processes
- In-memory data (tmpfs)
- Network connections
- Port forwarding rules (Windows)

### Resume Example

```powershell
# Stop with data in database
thresh stop postgres-dev

# Later... restart
thresh start postgres-dev

# All database data is intact
wsl -d thresh-postgres-dev -- psql -U postgres -c "SELECT * FROM users;"
```

## Best Practices

### Graceful Shutdown

Stop services before stopping environment:

```powershell
# Gracefully stop services first
wsl -d thresh-postgres-dev -- systemctl stop postgresql

# Then stop environment
thresh stop postgres-dev
```

### Shutdown Scripts

Create scripts for multiple environments:

**PowerShell:** `stop-all-dev.ps1`
```powershell
# Stop all development environments gracefully
$environments = @("web-frontend", "api-backend", "postgres-db")

foreach ($env in $environments) {
    Write-Host "Stopping $env..."
    thresh stop $env
}

Write-Host "All environments stopped"
```

### Check Before Stopping

Verify environment is stopped:

```powershell
# Check status
wsl -l -v | findstr thresh-webserver

# If running, stop it
if ((wsl -l -v | findstr "thresh-webserver.*Running")) {
    thresh stop webserver
}
```

### Automated Cleanup

Schedule regular cleanup of unused environments:

```powershell
# Stop environments not used in 7 days
Get-ChildItem ~/.thresh/metadata/*.json | 
    Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-7)} |
    ForEach-Object {
        $envName = $_.BaseName
        thresh stop $envName
    }
```

## Performance Impact

### Memory Release

Stopping environments frees:
- **WSL2 Memory:** ~500 MB - 4 GB per environment
- **CPU:** Background processes stopped
- **Disk I/O:** No active disk operations

### Quick Comparison

```powershell
# Check memory before
wsl --shutdown
wsl -d thresh-postgres-dev --exec free -h

# Stop environment
thresh stop postgres-dev

# Memory is released to Windows
```

## Related Commands

- [`thresh start`](/docs/cli-reference/start) - Restart stopped environment
- [`thresh destroy`](/docs/cli-reference/destroy) - Permanently remove environment
- [`thresh list`](/docs/cli-reference/list) - View all environments
- [`thresh metrics`](/docs/cli-reference/metrics) - Check resource usage

## Reference

### WSL Distribution States

| State | Description | Can Stop? |
|-------|-------------|-----------|
| Running | Environment is active | ✅ Yes |
| Stopped | Environment is inactive | ❌ Already stopped |
| Not Found | Environment doesn't exist | ❌ Not found |

### Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `4` | Environment not found |
| `5` | Failed to remove port forwarding |

### Metadata File

Port forwarding configuration read from:
- **Windows:** `C:\Users\<username>\.thresh\metadata\{environmentName}.json`
- **Linux:** `/home/<username>/.thresh/metadata/{environmentName}.json`
- **macOS:** `/Users/<username>/.thresh/metadata/{environmentName}.json`

## Next Steps

- [thresh start](/docs/cli-reference/start) - Resume environments
- [thresh destroy](/docs/cli-reference/destroy) - Permanently remove
- [Networking & Port Mapping](/docs/tutorials/networking) - Configure networking
- [Persistent Volumes](/docs/tutorials/volumes) - Manage data persistence
