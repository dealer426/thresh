---
sidebar_position: 2
title: CLI Reference
description: Complete reference for all thresh commands
---

# CLI Reference

thresh provides a comprehensive set of commands for managing WSL development environments, AI-powered blueprint generation, and MCP server integration.

## Quick Overview

| Command | Description |
|---------|-------------|
| [`up`](/docs/cli-reference/up) | Provision a new environment from a blueprint |
| [`list`](/docs/cli-reference/list) | List all thresh-managed environments |
| [`destroy`](/docs/cli-reference/destroy) | Remove an environment |
| [`generate`](/docs/cli-reference/generate) | Generate a custom blueprint using AI |
| [`chat`](/docs/cli-reference/chat) | Interactive AI chat for environment planning |
| [`blueprints`](/docs/cli-reference/blueprints) | List available blueprints |
| [`distros`](/docs/cli-reference/distros) | List available distributions |
| [`distro`](/docs/cli-reference/distro) | Manage custom distributions |
| [`config`](/docs/cli-reference/config) | Manage configuration settings |
| [`index`](/docs/cli-reference/index) | Initialize MCP configuration |
| [`serve`](/docs/cli-reference/serve) | Start MCP server |
| [`metrics`](/docs/cli-reference/metrics) | Show performance metrics |
| [`version`](/docs/cli-reference/version) | Show version information |

## Command Categories

### Environment Management
- **[up](/docs/cli-reference/up)** - Create new environments
- **[list](/docs/cli-reference/list)** - View all environments
- **[destroy](/docs/cli-reference/destroy)** - Remove environments

### AI Features
- **[generate](/docs/cli-reference/generate)** - Generate custom blueprints
- **[chat](/docs/cli-reference/chat)** - Interactive AI assistant

### Configuration
- **[config](/docs/cli-reference/config)** - Manage settings
- **[distro](/docs/cli-reference/distro)** - Custom distributions
- **[blueprints](/docs/cli-reference/blueprints)** - View blueprints
- **[distros](/docs/cli-reference/distros)** - View distributions

### Integration
- **[index](/docs/cli-reference/index)** - Initialize MCP configuration
- **[serve](/docs/cli-reference/serve)** - MCP server for AI editors

### Information
- **[metrics](/docs/cli-reference/metrics)** - Performance monitoring
- **[version](/docs/cli-reference/version)** - Version info

## Global Options

All commands support these global options:

| Option | Description |
|--------|-------------|
| `--help`, `-h` | Show help information |
| `--verbose`, `-v` | Enable verbose output |
| `--quiet`, `-q` | Suppress non-error output |
| `--version` | Show version information |

## Getting Help

For any command, use `--help` to see detailed usage:

```powershell
thresh --help               # Show all commands
thresh up --help           # Show help for 'up' command
thresh config --help       # Show help for 'config' command
```

## Exit Codes

thresh uses standard exit codes:

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `2` | Invalid arguments |
| `3` | WSL not available |
| `4` | Environment not found |
| `5` | Configuration error |

## Examples

See individual command pages for detailed examples and use cases.
