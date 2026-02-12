---
sidebar_position: 13
title: thresh version
description: Display version and build information
---

# thresh version

Display version, build information, and system compatibility details.

## Synopsis

```powershell
# Show version
thresh version

# Also available as:
thresh --version
thresh -v
```

## Description

The `version` command displays:

- thresh version number (semantic versioning)
- Build date and commit hash
- Target architecture and OS
- Container runtime compatibility
- UPX compression status (if applicable)

Use this to verify installation, troubleshoot compatibility issues, or report bugs.

## Options

| Option | Description |
|--------|-------------|
| `--json` | Output in JSON format |
| `--check-update` | Check for newer versions online |
| `--verbose` | Show detailed build information |
| `--help`, `-h` | Show help information |

## Examples

### Basic Version

```powershell
# Show current version
thresh version
```

**Output:**
```
thresh 1.3.0
```

### Detailed Information

```powershell
# Show all build details
thresh version --verbose
```

**Output:**
```
thresh 1.3.0

Build Information:
  Version:      1.3.0
  Build Date:   2026-02-10 14:32:15 UTC
  Commit:       a1b2c3d
  Branch:       main
  
Platform:
  OS:           Windows
  Architecture: x64
  Runtime:      .NET 9.0.2
  Compression:  UPX 4.2.4 (compressed to 35% of original size)
  
Container Runtime:
  Type:         WSL 2
  Version:      2.3.17.0
  Kernel:       5.15.153.1-microsoft-standard-WSL2
  Status:       Available ✓
  
Installation:
  Path:         C:\Program Files\thresh\thresh.exe
  Config:       C:\Users\user\.thresh\
  
Release Channel: Stable
```

### JSON Output

```powershell
# Get version as JSON
thresh version --json
```

**Output:**
```json
{
  "version": "1.3.0",
  "build": {
    "date": "2026-02-10T14:32:15Z",
    "commit": "a1b2c3d4e5f6",
    "branch": "main"
  },
  "platform": {
    "os": "windows",
    "architecture": "x64",
    "runtime": "net9.0.2",
    "upx_compressed": true,
    "upx_version": "4.2.4",
    "compression_ratio": 0.35
  },
  "container_runtime": {
    "type": "wsl2",
    "version": "2.3.17.0",
    "kernel": "5.15.153.1-microsoft-standard-WSL2",
    "available": true
  },
  "installation": {
    "executable": "C:\\Program Files\\thresh\\thresh.exe",
    "config_dir": "C:\\Users\\user\\.thresh"
  },
  "release_channel": "stable"
}
```

### Check for Updates

```powershell
# Check if newer version available
thresh version --check-update
```

**Output (up to date):**
```
thresh 1.3.0 (latest)

You are running the latest version.
```

**Output (update available):**
```
thresh 1.3.0

A newer version is available: 1.3.1
Released: 2 days ago

Release notes:
  - Fixed: Environment destroy on Windows
  - Added: New alpine-minimal blueprint
  
Update with:
  winget upgrade dealer426.thresh
```

### Short Flags

```powershell
# Equivalent short forms
thresh -v
thresh --version

# All output "thresh 1.3.0"
```

## Semantic Versioning

thresh follows [Semantic Versioning](https://semver.org/) 2.0.0:

```
MAJOR.MINOR.PATCH
  1  .  3  .  0
  │    │    │
  │    │    └─── Bug fixes (backward compatible)
  │    └──────── New features (backward compatible)
  └───────────── Breaking changes
```

### Version Examples

| Version | Meaning |
|---------|---------|
| `1.3.0` | Current stable release |
| `1.3.1` | Patch release (bug fixes) |
| `1.4.0` | Minor release (new features) |
| `2.0.0` | Major release (breaking changes) |
| `1.3.0-rc.1` | Release candidate |
| `1.3.0-beta.2` | Beta release |

## Build Metadata

### Commit Hash

The commit hash identifies the exact source code:

```powershell
thresh version --verbose | Select-String "Commit"
```

**Output:**
```
Commit: a1b2c3d4e5f6
```

View source at: `https://github.com/dealer426/thresh/commit/a1b2c3d4e5f6`

### Build Date

UTC timestamp of when the binary was compiled:

```powershell
thresh version --json | jq -r '.build.date'
```

**Output:**
```
2026-02-10T14:32:15Z
```

### UPX Compression

thresh binaries are compressed with [UPX](https://upx.github.io/):

| Platform | Original Size | Compressed Size | Ratio |
|----------|---------------|-----------------|-------|
| Windows x64 | ~85 MB | ~30 MB | 35% |
| Linux x64 | ~75 MB | ~26 MB | 35% |

Compression reduces download size and disk usage with no runtime performance impact.

## Container Runtime Detection

thresh automatically detects the container runtime:

### Windows

```
Container Runtime:
  Type:    WSL 2
  Version: 2.3.17.0
  Kernel:  5.15.153.1-microsoft-standard-WSL2
  Status:  Available ✓
```

### Linux

```
Container Runtime:
  Type:    Docker
  Version: 26.1.3
  API:     1.45
  Status:  Available ✓
```

### macOS

```
Container Runtime:
  Type:    containerd
  Version: 1.7.13
  Runtime: io.containerd.runc.v2
  Status:  Available ✓
```

### Unavailable

```
Container Runtime:
  Status:  Not Found ✗
  
ERROR: No container runtime detected
  
Install WSL 2 (Windows) or Docker (Linux/macOS) to use thresh.
See: https://thresh.sh/docs/installation
```

## Installation Paths

### Binary Location

```powershell
# Find thresh executable
thresh version --json | jq -r '.installation.executable'
```

**Windows:** `C:\Program Files\thresh\thresh.exe`  
**Linux:** `/usr/local/bin/thresh`  
**macOS:** `/opt/homebrew/bin/thresh`

### Configuration Directory

```powershell
# Find config directory
thresh version --json | jq -r '.installation.config_dir'
```

**Windows:** `C:\Users\<user>\.thresh\`  
**Linux/macOS:** `~/.thresh/`

Contains:
- `config.json` - User settings
- `blueprints/` - Custom blueprints
- `distros/` - Custom distributions
- `cache/` - Downloaded distribution images

## Release Channels

| Channel | Description | Audience |
|---------|-------------|----------|
| **Stable** | Production-ready releases | All users |
| **Beta** | Pre-release testing | Early adopters |
| **Nightly** | Daily builds from main branch | Contributors |

### Switching Channels

**Winget (Windows):**
```powershell
# Stable (default)
winget install dealer426.thresh

# Beta
winget install dealer426.thresh-beta
```

**Homebrew (macOS):**
```bash
# Stable
brew install thresh

# Beta
brew install thresh-beta
```

## Version Compatibility

### API Stability

| Component | Stability Promise |
|-----------|-------------------|
| **CLI commands** | Stable (no breaking changes in minor versions) |
| **Blueprint format** | Stable (backward compatible) |
| **Config file** | Stable (auto-migrated on version upgrade) |
| **MCP protocol** | Follows MCP spec versioning |

### Blueprint Compatibility

Blueprints are **forward compatible**:
- Blueprints from thresh 1.0 work in thresh 1.3
- New blueprint features require minimum version

**Example (minimum version):**
```json
{
  "minThreshVersion": "1.3.0",
  "distribution": "alpine:3.19",
  "packages": ["python3", "py3-pip"]
}
```

## Troubleshooting

### Version Mismatch

**Issue:** "Blueprint requires thresh 1.3.0 or later"

**Fix:**
```powershell
# Check current version
thresh version

# If outdated, update:
winget upgrade dealer426.thresh  # Windows
brew upgrade thresh              # macOS
```

### Build Information Missing

**Issue:** `thresh version --verbose` doesn't show build metadata

**Cause:** Custom build or development version

**Check:**
```powershell
# Verify official release
thresh version --json | jq -r '.build.branch'
# Should output "main" or "release/1.3"
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `3` | Network error (update check failed) |

## See Also

- [Release Notes](https://github.com/dealer426/thresh/releases) - Changelog for all versions
- [Installation Guide](/docs/installation) - Setup instructions
- [Contributing Guide](https://github.com/dealer426/thresh/blob/main/CONTRIBUTING.md) - Build from source
