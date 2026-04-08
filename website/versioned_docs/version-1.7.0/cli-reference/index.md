---
sidebar_position: 2
title: CLI Reference
description: Complete reference for all thresh commands
---

# CLI Reference

thresh provides a comprehensive set of commands for managing WSL development environments, AI-powered blueprint generation, MCP server integration, and fleet management via Thresh Hub.

:::info New in v1.7.0
Thresh v1.7.0 introduces **Hub authentication, remote node management, and cluster orchestration**:
- `thresh auth login/logout/status/token` — authenticate the CLI with Thresh Hub
- `thresh node list/info/metrics/up/blueprints/remove` — manage fleet nodes remotely
- `thresh cluster list/create/info/add-node/remove-node/delete` — organize nodes into clusters
- **Hub-managed stacks** — deploy multi-service stacks through the Hub dashboard and API
:::

:::info New in v1.6.0
Thresh v1.6.0 introduces **agent mode** for connecting nodes to Thresh Hub:
- `thresh agent start/stop/status` — manage the agent daemon
- `thresh agent config` — configure Hub URL, API key, transport, and HA failover
- SignalR WebSocket connection with automatic REST fallback
- Real-time metrics streaming to Thresh Hub
:::

:::info New in v1.5.0
Thresh v1.5.0 introduces **networking and storage features**:
- Port mapping and forwarding for WSL environments
- Persistent volumes and bind mounts
- New commands: `thresh start` and `thresh stop` for lifecycle management
- WSL configuration profiles for database optimization
:::

:::info New in v1.4.0
Thresh v1.4.0 introduces **grouped blueprint commands** for better organization:
- `thresh blueprints` → `thresh blueprint list`
- `thresh generate` → `thresh blueprint generate`
- New: `thresh blueprint delete` to remove generated blueprints
:::

## Quick Overview

| Command | Description |
|---------|-------------|
| [`up`](/docs/cli-reference/up) | Provision a new environment from a blueprint |
| [`start`](/docs/cli-reference/start) | Start a stopped environment and apply port forwarding |
| [`stop`](/docs/cli-reference/stop) | Stop a running environment and remove port forwarding |
| [`list`](/docs/cli-reference/list) | List all thresh-managed environments |
| [`destroy`](/docs/cli-reference/destroy) | Remove an environment |
| [`blueprint`](/docs/cli-reference/blueprint) | Manage blueprints (list, generate, delete) |
| [`blueprint list`](/docs/cli-reference/blueprints) | List available blueprints |
| [`blueprint generate`](/docs/cli-reference/generate) | Generate custom blueprint using AI |
| [`blueprint delete`](/docs/cli-reference/blueprint-delete) | Delete a generated blueprint |
| [`chat`](/docs/cli-reference/chat) | Interactive AI chat for environment planning |
| [`auth`](/docs/cli-reference/auth) | Authenticate CLI with Thresh Hub (v1.7.0) |
| [`agent`](/docs/cli-reference/agent) | Connect node to Thresh Hub (v1.6.0) |
| [`node`](/docs/cli-reference/node) | Manage remote fleet nodes (v1.7.0) |
| [`cluster`](/docs/cli-reference/cluster) | Organize nodes into clusters (v1.7.0) |
| [`distros`](/docs/cli-reference/distros) | List available distributions |
| [`distro`](/docs/cli-reference/distro) | Manage custom distributions |
| [`config`](/docs/cli-reference/config) | Manage configuration settings |
| [`wslconf`](/docs/cli-reference/wslconf) | Manage WSL configuration profiles |
| [`volume`](/docs/cli-reference/volume) | Manage persistent volumes |
| [`serve`](/docs/cli-reference/serve) | Start MCP server |
| [`metrics`](/docs/cli-reference/metrics) | Show performance metrics |
| [`version`](/docs/cli-reference/version) | Show version information |

## Command Categories

### Environment Management
- **[up](/docs/cli-reference/up)** - Create new environments
- **[start](/docs/cli-reference/start)** - Start stopped environments (v1.5.0)
- **[stop](/docs/cli-reference/stop)** - Stop running environments (v1.5.0)
- **[list](/docs/cli-reference/list)** - View all environments
- **[destroy](/docs/cli-reference/destroy)** - Remove environments

### Blueprint Management
- **[blueprint](/docs/cli-reference/blueprint)** - Manage blueprints (parent command)
- **[blueprint list](/docs/cli-reference/blueprints)** - List available blueprints
- **[blueprint generate](/docs/cli-reference/generate)** - Generate custom blueprints with AI
- **[blueprint delete](/docs/cli-reference/blueprint-delete)** - Delete generated blueprints

### AI Features
- **[chat](/docs/cli-reference/chat)** - Interactive AI assistant

### Configuration
- **[config](/docs/cli-reference/config)** - Manage settings
- **[distro](/docs/cli-reference/distro)** - Custom distributions
- **[distros](/docs/cli-reference/distros)** - View distributions
- **[wslconf](/docs/cli-reference/wslconf)** - WSL configuration profiles (v1.5.0)

### Storage
- **[volume](/docs/cli-reference/volume)** - Manage persistent volumes (v1.5.0)

### Fleet Management
- **[auth](/docs/cli-reference/auth)** - Authenticate with Thresh Hub (v1.7.0)
- **[agent](/docs/cli-reference/agent)** - Connect to Thresh Hub (v1.6.0)
- **[node](/docs/cli-reference/node)** - Manage remote fleet nodes (v1.7.0)
- **[cluster](/docs/cli-reference/cluster)** - Organize nodes into clusters (v1.7.0)

### Stacks (Hub-Managed)
- **[Stacks](/docs/cli-reference/stack)** - Deploy multi-service stacks via Hub dashboard and API (v1.7.0)

### Integration
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
