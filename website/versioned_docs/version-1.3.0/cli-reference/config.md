---
sidebar_position: 6
title: thresh config
description: Manage thresh configuration settings
---

# thresh config

Manage thresh configuration settings including AI models, telemetry, and custom distributions.

## Synopsis

```powershell
# View settings
thresh config status
thresh config get <key>

# Modify settings
thresh config set <key> <value>
thresh config reset
```

## Description

The `config` command manages thresh's configuration file (`~/.thresh/config.json`). Configuration includes:

- Default AI model
- Default base distribution
- Telemetry preferences
- Custom distribution definitions
- MCP server settings

## Subcommands

### `config status`

Show all configuration settings.

```powershell
thresh config status
```

**Output:**
```
thresh Configuration
═══════════════════════════════════════

GitHub Copilot: ✓ Authenticated
Default Model: gpt-4o
Default Base: ubuntu-22.04
Telemetry: Enabled

Custom Distributions: 0
MCP Server: Enabled
```

### `config get`

Get a specific configuration value.

```powershell
thresh config get <key>
```

**Examples:**
```powershell
# Get default model
thresh config get default-model
# Output: gpt-4o

# Get default base distribution
thresh config get default-base
# Output: ubuntu-22.04

# Get telemetry setting
thresh config get enable-telemetry
# Output: true
```

### `config set`

Set a configuration value.

```powershell
thresh config set <key> <value>
```

**Examples:**
```powershell
# Set default AI model
thresh config set default-model claude-3.5-sonnet

# Set default distribution
thresh config set default-base alpine-3.19

# Disable telemetry
thresh config set enable-telemetry false
```

### `config reset`

Reset all configuration to defaults.

```powershell
thresh config reset
```

**Output:**
```
⚠ This will reset all configuration to defaults.
Continue? (y/N): y

✓ Configuration reset to defaults
```

## Configuration Keys

### AI Settings

| Key | Description | Default | Example Values |
|-----|-------------|---------|----------------|
| `default-model` | Default AI model for generate/chat | `gpt-4o` | `claude-3.5-sonnet`, `gemini-1.5-pro`, `o1-preview` |

### Environment Settings

| Key | Description | Default | Example Values |
|-----|-------------|---------|----------------|
| `default-base` | Default base distribution | `ubuntu-22.04` | `alpine-3.19`, `debian-12` |

### System Settings

| Key | Description | Default | Example Values |
|-----|-------------|---------|----------------|
| `enable-telemetry` | Send anonymous usage data | `true` | `true`, `false` |

### MCP Settings

| Key | Description | Default | Example Values |
|-----|-------------|---------|----------------|
| `mcp.enabled` | Enable MCP server | `true` | `true`, `false` |
| `mcp.port` | MCP server port (if TCP) | `3000` | `3000-65535` |

## Examples

### Set Default AI Model

```powershell
# Use Claude 3.5 Sonnet by default
thresh config set default-model claude-3.5-sonnet

# Verify
thresh config get default-model
# claude-3.5-sonnet

# Now all generate/chat commands use Claude
thresh generate "Python environment"  # Uses Claude
```

### Change Default Distribution

```powershell
# Use Alpine Linux by default (faster, smaller)
thresh config set default-base alpine-3.19

# Future blueprints will default to Alpine
thresh generate "Node.js environment"  # Uses Alpine base
```

### Disable Telemetry

```powershell
thresh config set enable-telemetry false

# Verify
thresh config get enable-telemetry
# false
```

### View All Settings

```powershell
thresh config status
```

## Configuration File

Configuration is stored at `~/.thresh/config.json`:

```json
{
  "defaultModel": "gpt-4o",
  "defaultBase": "ubuntu-22.04",
  "enableTelemetry": true,
  "customDistributions": [],
  "mcp": {
    "enabled": true,
    "port": 3000
  }
}
```

### Manual Editing

You can edit the config file directly:

```powershell
# Edit with VS Code
code ~/.thresh/config.json

# Edit with Notepad
notepad ~/.thresh/config.json
```

:::warning Manual Editing
If you edit manually, ensure valid JSON syntax. Invalid JSON will cause thresh to reset to defaults.
:::

## GitHub Copilot Authentication

thresh uses GitHub CLI for authentication:

```powershell
# Check auth status
gh auth status

# Authenticate if needed
gh auth login

# Verify thresh can access AI
thresh config status
```

**Output:**
```
GitHub Copilot: ✓ Authenticated
```

## Telemetry

When enabled, thresh collects anonymous usage data:

- Commands executed (names only, no arguments)
- Blueprint names used (built-in only)
- Performance metrics (startup time, provision duration)
- Error types (no personal data)

**Not collected:**
- File paths
- Environment names
- Custom blueprint content
- Personal information

To opt out:
```powershell
thresh config set enable-telemetry false
```

## Troubleshooting

### "Configuration file corrupt"

```powershell
# Reset to defaults
thresh config reset
```

### "GitHub CLI not authenticated"

```powershell
# Re-authenticate
gh auth login

# Verify
thresh config status
```

### Configuration Not Persisting

Check file permissions:

```powershell
# View config file
Get-Item ~/.thresh/config.json

# If missing, create directory
New-Item -ItemType Directory -Force -Path ~/.thresh
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `2` | Invalid key or value |
| `5` | Configuration error |

## See Also

- [`thresh generate`](./generate) - Use configured AI model
- [`thresh chat`](./chat) - Use configured AI model
- `thresh serve` - MCP server settings
