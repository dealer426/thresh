---
sidebar_position: 1
title: thresh up
description: Provision a new WSL environment from a blueprint
---

# thresh up

Provision a new WSL development environment from a blueprint.

## Synopsis

```powershell
thresh up <blueprint-name> [options]
```

## Description

The `up` command creates a new WSL environment based on a blueprint specification. It handles:

- Downloading and caching the base distribution
- Importing the distribution into WSL
- Installing packages specified in the blueprint
- Configuring the environment
- Running post-installation scripts

## Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `<blueprint-name>` | Yes | Name of the blueprint to use (without .json extension) |

## Options

| Option | Description |
|--------|-------------|
| `--verbose`, `-v` | Show detailed provisioning progress |
| `--force`, `-f` | Force re-provision if environment exists |
| `--help`, `-h` | Show help information |

## Examples

### Basic Usage

```powershell
# Provision using built-in blueprint
thresh up alpine-minimal
```

### With Verbose Output

```powershell
# See detailed progress
thresh up python-dev --verbose
```

**Output:**
```
✓ Loading blueprint: python-dev
✓ Using distribution: Ubuntu 22.04
✓ Checking cache...
✓ Downloading rootfs (125 MB)...
  [████████████████████] 125/125 MB
✓ Importing to WSL...
✓ Installing packages...
  - python3
  - python3-pip
  - python3-venv
✓ Running post-install scripts...
✓ Environment ready!

Access with: wsl -d python-dev
```

### Force Re-provision

```powershell
# Replace existing environment
thresh up ubuntu-dev --force
```

### Using Custom Blueprint

```powershell
# Create custom blueprint first
thresh generate "Rust development with cargo and rustfmt" > rust-dev.json

# Provision from custom blueprint
thresh up rust-dev
```

## Built-in Blueprints

See `thresh blueprints` for the full list of available blueprints:

- `alpine-minimal` - Lightweight Alpine Linux base (fastest)
- `ubuntu-dev` - General purpose Ubuntu development
- `python-dev` - Python 3.x with pip and common packages
- `node-dev` - Node.js 20 LTS with npm, yarn, pnpm
- `debian-stable` - Debian stable base
- `azure-cli` - Azure CLI tools pre-configured
- `alpine-python` - Python on Alpine Linux
- `ubuntu-python` - Python on Ubuntu with extras

## Performance

Provisioning time varies by blueprint and network speed:

| Blueprint | Download Size | Time (Typical) |
|-----------|---------------|----------------|
| `alpine-minimal` | ~3 MB | 15-20 seconds |
| `alpine-python` | ~8 MB | 25-30 seconds |
| `ubuntu-dev` | ~30 MB | 45-60 seconds |
| `python-dev` | ~50 MB | 60-90 seconds |

:::tip Caching
Distributions are cached after first download in `~/.thresh/cache/`. Subsequent provisions of the same distribution are much faster.
:::

## Troubleshooting

### "Blueprint not found"

```powershell
# List available blueprints
thresh blueprints

# Check custom blueprints directory
ls ~/.thresh/blueprints
```

### "WSL distribution already exists"

```powershell
# Use --force to replace
thresh up alpine-minimal --force

# Or destroy first
thresh destroy alpine-minimal
thresh up alpine-minimal
```

### "Download failed"

```powershell
# Check internet connection
Test-NetConnection google.com

# Try with verbose to see details
thresh up alpine-minimal --verbose

# Clear cache and retry
Remove-Item -Recurse -Force ~/.thresh/cache
thresh up alpine-minimal
```

### "Package installation failed"

Check the logs with verbose output:

```powershell
thresh up python-dev --verbose
```

Common causes:
- Network connectivity issues
- Repository mirrors temporarily unavailable
- Invalid package names in blueprint

## Blueprint Structure

Blueprints are JSON files with this structure:

```json
{
  "name": "python-dev",
  "description": "Python development environment",
  "base": "ubuntu-22.04",
  "packages": [
    "python3",
    "python3-pip",
    "python3-venv",
    "git"
  ],
  "post_install": [
    "pip3 install --upgrade pip",
    "pip3 install requests pytest black flake8"
  ]
}
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Environment provisioned successfully |
| `1` | General provisioning error |
| `2` | Invalid blueprint or arguments |
| `3` | WSL not available |
| `4` | Download failed |
| `5` | Package installation failed |

## See Also

- [`thresh list`](./list) - List provisioned environments
- [`thresh destroy`](./destroy) - Remove environments
- [`thresh generate`](./generate) - Generate custom blueprints
- [`thresh blueprints`](./blueprints) - List available blueprints
