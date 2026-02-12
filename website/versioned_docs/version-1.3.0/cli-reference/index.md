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
| [`up`](up) | Provision a new environment from a blueprint |
| [`list`](list) | List all thresh-managed environments |
| [`destroy`](destroy) | Remove an environment |
| [`generate`](generate) | Generate a custom blueprint using AI |
| [`chat`](chat) | Interactive AI chat for environment planning |
| `blueprints` | List available blueprints |
| `distros` | List available distributions |
| `distro` | Manage custom distributions |
| [`config`](config) | Manage configuration settings |
| `serve` | Start MCP server |
| `metrics` | Show performance metrics |
| `version` | Show version information |

## Command Categories

### Environment Management
- **[up](up)** - Create new environments
- **[list](list)** - View all environments
- **[destroy](destroy)** - Remove environments

### AI Features
- **[generate](generate)** - Generate custom blueprints
- **[chat](chat)** - Interactive AI assistant

### Configuration
- **[config](config)** - Manage settings
- **distro** - Custom distributions
- **blueprints** - View blueprints
- **distros** - View distributions

### Integration
- **serve** - MCP server for AI editors

### Information
- **metrics** - Performance monitoring
- **version** - Version info

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
