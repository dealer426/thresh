---
sidebar_position: 9
title: thresh distros
description: List all available Linux distributions
---

# thresh distros

List all available Linux distributions for environment provisioning.

## Synopsis

```powershell
thresh distros [options]
```

## Description

The `distros` command displays all Linux distributions that thresh can use as base images for environments. It shows:

- Official distributions (Alpine, Ubuntu, Debian, etc.)
- Custom distributions (user-added)
- Distribution metadata (version, architecture, download size)
- Download URLs and checksums

Distributions are the foundation for blueprints - each blueprint specifies which distribution to use as its base.

## Options

| Option | Description |
|--------|-------------|
| `--json` | Output in JSON format for scripting |
| `--verbose`, `-v` | Show detailed distribution information |
| `--cached` | Only show cached/downloaded distributions |
| `--help`, `-h` | Show help information |

## Examples

### List All Distributions

```powershell
# Show all available distributions
thresh distros
```

**Output:**
```
Available Distributions:

Alpine Linux:
  3.19   (latest) - 15 MB
  3.18            - 15 MB

Ubuntu:
  22.04  (jammy)  - 78 MB
  20.04  (focal)  - 73 MB

Debian:
  12     (bookworm) - 116 MB
  11     (bullseye) - 114 MB

Use these in blueprints or with 'thresh up'
```

### JSON Output

```powershell
#Get distributions in JSON format
thresh distros --json
```

**Output:**
```json
{
  "alpine": [
    {
      "version": "3.19",
      "architecture": "x86_64",
      "size_mb": 15,
      "download_url": "https://dl-cdn.alpinelinux.org/...",
      "checksum": "sha256:...",
      "cached": true
    }
  ],
  "ubuntu": [
    {
      "version": "22.04",
      "codename": "jammy",
      "architecture": "amd64",
      "size_mb": 78,
      "download_url": "https://cloud-images.ubuntu.com/...",
      "checksum": "sha256:...",
      "cached": false
    }
  ]
}
```

### Show Only Cached Distributions

```powershell
# List distributions already downloaded
thresh distros --cached
```

**Output:**
```
Cached Distributions:

Alpine 3.19         15 MB   C:\Users\user\.thresh\cache\alpine-3.19.tar.gz
Ubuntu 22.04        78 MB   C:\Users\user\.thresh\cache\ubuntu-22.04.tar.gz

Total cache size: 93 MB
```

### Verbose Output

```powershell
# Show detailed information
thresh distros --verbose
```

**Output:**
```
Alpine Linux 3.19
  Architecture: x86_64
  Size: 15 MB (compressed), ~45 MB (extracted)
  URL: https://dl-cdn.alpinelinux.org/alpine/v3.19/releases/x86_64/alpine-minirootfs-3.19.0-x86_64.tar.gz
  Checksum: sha256:6457d53f..
  Cached: Yes (C:\Users\user\.thresh\cache\alpine-3.19.tar.gz)
  Downloaded: 2026-02-10 14:23:15
  Used by: 3 environments (alpine-minimal, python-dev, node-dev)

Ubuntu 22.04 (Jammy Jellyfish)
  Architecture: amd64
  Size: 78 MB (compressed), ~310 MB (extracted)
  URL: https://cloud-images.ubuntu.com/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64-wsl.rootfs.tar.gz
  Checksum: sha256:a4b2c8f1...
  Cached: No
  Official Ubuntu Cloud Image
```

## Distribution Sources

thresh downloads distributions from official sources:

| Distribution | Source |
|--------------|--------|
| **Alpine** | https://alpinelinux.org/ |
| **Ubuntu** | https://cloud-images.ubuntu.com/ |
| **Debian** | https://cloud.debian.org/ |
| **Custom** | User-specified URLs |

All downloads are verified with SHA256 checksums.

## Cache Location

Downloaded distributions are cached at:

**Windows:** `C:\Users\<user>\.thresh\cache\`  
**Linux/macOS:** `~/.thresh/cache/`

This cache is shared across all environments using the same distribution.

## Adding Custom Distributions

Create a custom distribution entry:

```json
{
  "name": "arch",
  "version": "2024.02.01",
  "architecture": "x86_64",
  "downloadUrl": "https://mirrors.kernel.org/archlinux/...",
  "checksum": "sha256:...",
  "extractPath": "/"
}
```

Save to `~/.thresh/distros/arch-2024.02.01.json`

## Distribution vs Blueprint

**Distribution** = Base Linux image (raw filesystem)  
**Blueprint** = Configuration (which packages, scripts, settings)

```
Distribution (Ubuntu 22.04)
      ↓
Blueprint (python-dev.json)
      ↓
Environment (python-dev-instance)
```

## Managing Cache

```powershell
# Clear distribution cache
Remove-Item -Recurse ~/.thresh/cache/*

# Clear specific distribution
Remove-Item ~/.thresh/cache/alpine-3.19.tar.gz

# Cache will re-download on next use
thresh up python-dev
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `3` | Network error (can't fetch distro list) |

## See Also

- [`thresh distro`](/docs/cli-reference/distro) - Manage custom distributions
- [`thresh blueprint list`](/docs/cli-reference/blueprints) - List blueprints
- [`thresh up`](/docs/cli-reference/up) - Provision environments
