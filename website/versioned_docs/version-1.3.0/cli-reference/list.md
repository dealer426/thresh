---
sidebar_position: 2
title: thresh list
description: List all thresh-managed environments
---

# thresh list

List all thresh-managed WSL environments with their status and resource usage.

## Synopsis

```powershell
thresh list [options]
```

## Description

The `list` command displays all WSL environments managed by thresh, showing their current status, distribution, memory usage, and disk space.

## Options

| Option | Description |
|--------|-------------|
| `--all`, `-a` | Include stopped environments |
| `--verbose`, `-v` | Show detailed information |
| `--json` | Output in JSON format |
| `--help`, `-h` | Show help information |

## Examples

### Basic Usage

```powershell
thresh list
```

**Output:**
```
NAME              STATUS    DISTRO          MEMORY     DISK
python-dev        Running   Ubuntu 22.04    256 MB     2.1 GB
alpine-minimal    Running   Alpine 3.19     128 MB     512 MB
node-dev          Running   Ubuntu 22.04    384 MB     3.2 GB
```

### Include Stopped Environments

```powershell
thresh list --all
```

**Output:**
```
NAME              STATUS    DISTRO          MEMORY     DISK
python-dev        Running   Ubuntu 22.04    256 MB     2.1 GB
alpine-minimal    Stopped   Alpine 3.19     -          512 MB
node-dev          Running   Ubuntu 22.04    384 MB     3.2 GB
test-env          Stopped   Debian 12       -          1.8 GB
```

### Verbose Output

```powershell
thresh list --verbose
```

**Output:**
```
NAME: python-dev
STATUS: Running
DISTRO: Ubuntu 22.04
MEMORY: 256 MB
DISK: 2.1 GB
CREATED: 2024-01-15 10:30:00
BLUEPRINT: python-dev
PACKAGES: 42
WSL VERSION: 2

NAME: alpine-minimal
STATUS: Running
DISTRO: Alpine 3.19
MEMORY: 128 MB
DISK: 512 MB
CREATED: 2024-01-15 09:15:00
BLUEPRINT: alpine-minimal
PACKAGES: 12
WSL VERSION: 2
```

### JSON Output

```powershell
thresh list --json
```

**Output:**
```json
[
  {
    "name": "python-dev",
    "status": "Running",
    "distribution": "Ubuntu 22.04",
    "memory_mb": 256,
    "disk_gb": 2.1,
    "created": "2024-01-15T10:30:00Z",
    "blueprint": "python-dev",
    "packages": 42,
    "wsl_version": 2
  },
  {
    "name": "alpine-minimal",
    "status": "Running",
    "distribution": "Alpine 3.19",
    "memory_mb": 128,
    "disk_gb": 0.5,
    "created": "2024-01-15T09:15:00Z",
    "blueprint": "alpine-minimal",
    "packages": 12,
    "wsl_version": 2
  }
]
```

## Output Columns

| Column | Description |
|--------|-------------|
| **NAME** | Environment name (same as WSL distribution name) |
| **STATUS** | Running, Stopped, or Installing |
| **DISTRO** | Base distribution and version |
| **MEMORY** | Current memory usage (running only) |
| **DISK** | Total disk space used |

## Comparing with WSL

```powershell
# thresh-managed environments only
thresh list

# All WSL distributions (including non-thresh)
wsl -l -v
```

## Filtering Environments

### Only Running

```powershell
# Default behavior - shows only running
thresh list
```

### Only Stopped

```powershell
# PowerShell filter
thresh list --all | Where-Object { $_ -match "Stopped" }
```

### By Name Pattern

```powershell
# PowerShell filter
thresh list | Where-Object { $_ -match "python" }
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | Error listing environments |
| `3` | WSL not available |

## See Also

- [`thresh up`](./up) - Create new environments
- [`thresh destroy`](./destroy) - Remove environments
- [`thresh metrics`](./metrics) - Show performance metrics
