---
sidebar_position: 8
title: thresh blueprints
description: List all available blueprints
---

# thresh blueprints

List all available environment blueprints (built-in and custom).

## Synopsis

```powershell
thresh blueprints [options]
```

## Description

The `blueprints` command displays all available blueprints that can be used with `thresh up`. It shows:

- Built-in blueprints (shipped with thresh)
- Custom blueprints (from `~/.thresh/blueprints/`)
- Blueprint metadata (name, base distribution, packages)

Blueprints are JSON files that define:
- Base Linux distribution
- Packages to install
- Configuration files
- Post-installation scripts

## Options

| Option | Description |
|--------|-------------|
| `--json` | Output in JSON format for scripting |
| `--verbose`, `-v` | Show detailed blueprint information |
| `--help`, `-h` | Show help information |

## Examples

### List All Blueprints

```powershell
# Show all available blueprints
thresh blueprints
```

**Output:**
```
Available Blueprints:

Built-in:
  alpine-minimal    - Minimal Alpine Linux (3.19)
  alpine-python     - Alpine with Python 3.11
  python-dev        - Python development (Alpine + pip, venv)
  node-dev          - Node.js 20 development environment
  ubuntu-dev        - Ubuntu 22.04 with dev tools
  debian-stable     - Debian 12 (Bookworm)

Custom:
  my-stack          - Custom full-stack environment
  data-science      - Python with NumPy, Pandas, Jupyter

Use 'thresh up <blueprint-name>' to provision
```

### JSON Output

```powershell
# Get blueprints in JSON format
thresh blueprints --json
```

**Output:**
```json
{
  "built_in": [
    {
      "name": "alpine-minimal",
      "distribution": "alpine",
      "version": "3.19",
      "description": "Minimal Alpine Linux",
      "packages": ["bash", "curl"]
    },
    {
      "name": "python-dev",
      "distribution": "alpine",
      "version": "3.19",
      "description": "Python development environment",
      "packages": ["python3", "py3-pip", "python3-dev"]
    }
  ],
  "custom": [
    {
      "name": "my-stack",
      "distribution": "ubuntu",
      "version": "22.04",
      "description": "Custom full-stack environment",
      "packages": ["nodejs", "postgresql", "redis"]
    }
  ]
}
```

### Verbose Output

```powershell
# Show detailed blueprint information
thresh blueprints --verbose
```

**Output:**
```
alpine-minimal
  Base: Alpine Linux 3.19
  Size: ~15 MB
  Packages: bash, curl, git
  Description: Minimal development environment
  File: C:\Users\user\.thresh\blueprints\alpine-minimal.json

python-dev
  Base: Alpine Linux 3.19
  Size: ~120 MB
  Packages: python3, py3-pip, python3-dev, gcc, musl-dev
  Description: Python development with virtual environments
  File: C:\Users\user\.thresh\blueprints\python-dev.json
```

## Blueprint Locations

Blueprints are searched in this order:

1. **Built-in**: Embedded in thresh binary
2. **User directory**: `~/.thresh/blueprints/`
3. **Project directory**: `./.thresh/blueprints/`

Custom blueprints override built-in ones with the same name.

## Blueprint Structure

Example `custom-stack.json`:

```json
{
  "name": "custom-stack",
  "distribution": {
    "name": "ubuntu",
    "version": "22.04",
    "source": "https://cloud-images.ubuntu.com/...",
    "checksum": "sha256:..."
  },
  "packages": [
    "build-essential",
    "git",
    "nodejs",
    "npm"
  ],
  "postInstall": [
    "npm install -g yarn",
    "git config --global core.autocrlf input"
  ],
  "environment": {
    "NODE_ENV": "development",
    "PATH": "/usr/local/bin:$PATH"
  }
}
```

## Related Information

### Creating Custom Blueprints

```powershell
# Generate a new blueprint with AI
thresh generate "Node.js with PostgreSQL"

# Manually create in user directory
notepad ~/.thresh/blueprints/my-blueprint.json
```

### Using Blueprints

```powershell
# Use built-in blueprint
thresh up python-dev

# Use custom blueprint
thresh up my-stack

# Force re-provision with blueprint
thresh up node-dev --force
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `2` | Invalid blueprint format |

## See Also

- [`thresh up`](/docs/cli-reference/up) - Provision from blueprint
- [`thresh generate`](/docs/cli-reference/generate) - Create custom blueprints
- [`thresh list`](/docs/cli-reference/list) - List provisioned environments
