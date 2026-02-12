---
sidebar_position: 10
title: thresh distro
description: Manage custom Linux distributions
---

# thresh distro

Add, remove, and inspect custom Linux distributions.

## Synopsis

```powershell
# Add a custom distribution
thresh distro add <name> <version> <url> [options]

# Remove a custom distribution
thresh distro remove <name> <version>

# Show details about a specific distribution
thresh distro show <name> <version>

# Verify distribution checksum
thresh distro verify <name> <version>
```

## Description

The `distro` command manages custom Linux distributions. Use this to:

- Add distributions not included by default (Arch, Fedora, etc.)
- Manage custom-built base images
- Remove outdated distributions
- Inspect distribution metadata
- Verify download integrity

Custom distributions are stored in `~/.thresh/distros/` and appear in `thresh distros` output.

## Subcommands

### distro add

Add a custom Linux distribution.

**Usage:**
```powershell
thresh distro add <name> <version> <url> [options]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<name>` | Distribution name (e.g., "arch", "fedora") |
| `<version>` | Version identifier (e.g., "2024.02.01") |
| `<url>` | Download URL for root filesystem tarball |

**Options:**

| Option | Description |
|--------|-------------|
| `--checksum <sha256>` | Expected SHA256 checksum |
| `--architecture <arch>` | Architecture (default: x86_64) |
| `--description <text>` | Human-readable description |
| `--extract-path <path>` | Path to extract from tarball (default: /) |

**Example:**
```powershell
thresh distro add arch 2024.02.01 `
  "https://mirrors.kernel.org/archlinux/iso/2024.02.01/archlinux-bootstrap-2024.02.01-x86_64.tar.gz" `
  --checksum "sha256:a1b2c3..." `
  --description "Arch Linux minimal bootstrap"
```

### distro remove

Remove a custom distribution from thresh.

**Usage:**
```powershell
thresh distro remove <name> <version>
```

**Example:**
```powershell
# Remove custom Arch Linux distribution
thresh distro remove arch 2024.02.01

# Removes metadata and cached files
```

**Note:** Only custom distributions can be removed. Built-in distributions (Alpine, Ubuntu, Debian) cannot be removed.

### distro show

Display detailed information about a distribution.

**Usage:**
```powershell
thresh distro show <name> <version> [options]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--json` | Output in JSON format |

**Example:**
```powershell
thresh distro show arch 2024.02.01
```

**Output:**
```
Arch Linux 2024.02.01

  Name: arch
  Version: 2024.02.01
  Architecture: x86_64
  Type: Custom
  
  Download:
    URL: https://mirrors.kernel.org/archlinux/...
    Size: 145 MB (compressed)
    Checksum: sha256:a1b2c3...
    
  Cache:
    Status: Downloaded
    Location: C:\Users\user\.thresh\cache\arch-2024.02.01.tar.gz
    Downloaded: 2026-02-10 15:30:22
    
  Usage:
    Used by: 0 environments
    
  Metadata:
    Added: 2026-02-10 15:25:10
    Source: Custom
    Description: Arch Linux minimal bootstrap
```

### distro verify

Verify the checksum of a downloaded distribution.

**Usage:**
```powershell
thresh distro verify <name> <version>
```

**Example:**
```powershell
thresh distro verify arch 2024.02.01
```

**Output:**
```
Verifying arch 2024.02.01...

File: C:\Users\user\.thresh\cache\arch-2024.02.01.tar.gz
Expected:  sha256:a1b2c3d4e5f6...
Calculated: sha256:a1b2c3d4e5f6...

✓ Checksum verified successfully
```

## Examples

### Add Fedora Distribution

```powershell
thresh distro add fedora 39 `
  "https://download.fedoraproject.org/pub/fedora/linux/releases/39/Container/x86_64/images/Fedora-Container-Base-39-1.5.x86_64.tar.xz" `
  --checksum "sha256:f3d8a..." `
  --description "Fedora 39 Container Base"
```

### Add Custom Corporate Image

```powershell
# Add internal base image
thresh distro add corporate-ubuntu 22.04-hardened `
  "https://artifacts.company.com/images/ubuntu-22.04-hardened.tar.gz" `
  --checksum "sha256:abc123..." `
  --description "Corporate hardened Ubuntu 22.04"
```

### List Available Distributions

```powershell
# Show all (including custom)
thresh distros

# Custom distributions appear with [Custom] tag
```

### Remove Old Custom Distribution

```powershell
# Remove outdated custom distro
thresh distro remove arch 2023.12.01

# Updated version can be added
thresh distro add arch 2024.02.01 <url>
```

### Verify Before Use

```powershell
# Verify integrity before creating environments
thresh distro verify fedora 39

# If checksum fails, re-download:
Remove-Item ~/.thresh/cache/fedora-39.tar.gz
thresh up fedora-environment  # Will re-download
```

## Custom Distribution File Format

Custom distributions are stored as JSON:

**Location:** `~/.thresh/distros/<name>-<version>.json`

```json
{
  "name": "arch",
  "version": "2024.02.01",
  "architecture": "x86_64",
  "downloadUrl": "https://mirrors.kernel.org/archlinux/iso/2024.02.01/archlinux-bootstrap-2024.02.01-x86_64.tar.gz",
  "checksum": "sha256:a1b2c3d4e5f6...",
  "extractPath": "/",
  "sizeBytes": 152428800,
  "description": "Arch Linux minimal bootstrap",
  "metadata": {
    "addedDate": "2026-02-10T15:25:10Z",
    "source": "custom",
    "maintainer": "user@example.com"
  }
}
```

## Using Custom Distributions in Blueprints

Reference custom distributions in blueprint files:

```json
{
  "distribution": "arch:2024.02.01",
  "packages": [
    "base-devel",
    "git",
    "vim"
  ],
  "postInstall": [
    "pacman -Syu --noconfirm"
  ]
}
```

Or use directly with `thresh up`:

```powershell
thresh up my-arch-env --distro arch:2024.02.01
```

## Security Considerations

### Checksum Verification

Always provide `--checksum` when adding distributions:

```powershell
# GOOD: Provides checksum
thresh distro add fedora 39 <url> --checksum "sha256:..."

# WARNING: No checksum verification
thresh distro add fedora 39 <url>
```

### Trusted Sources

Only add distributions from trusted sources:
- Official distribution mirrors
- Corporate artifact repositories
- Verified third-party providers

### HTTPS Required

Downloads must use HTTPS URLs for security.

## Distribution Discovery

Find official root filesystem images:

| Distribution | Download Location |
|--------------|-------------------|
| **Arch Linux** | https://archlinux.org/download/ |
| **Fedora** | https://getfedora.org/en/server/download/ |
| **openSUSE** | https://get.opensuse.org/tumbleweed/ |
| **Rocky Linux** | https://rockylinux.org/download |
| **AlmaLinux** | https://almalinux.org/get-almalinux/ |

Look for "container base image" or "root filesystem" downloads (`.tar.gz`, `.tar.xz`).

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `2` | Invalid arguments |
| `3` | Network error (download failed) |
| `4` | Checksum verification failed |
| `5` | Distribution already exists |
| `6` | Distribution not found |

## See Also

- [`thresh distros`](/docs/cli-reference/distros) - List all distributions
- [`thresh blueprints`](/docs/cli-reference/blueprints) - List blueprints
- [`thresh up`](/docs/cli-reference/up) - Create environments with custom distros
