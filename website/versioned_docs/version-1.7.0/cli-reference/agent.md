---
sidebar_position: 14
title: thresh agent
description: Connect a thresh node to Thresh Hub for centralized fleet management
---

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# thresh agent

:::info New in v1.6.0
The `agent` command group is new in thresh 1.6.0.
:::

Connect a thresh node to **Thresh Hub** for centralized fleet management. When running as an agent, thresh maintains a persistent WebSocket connection to the Hub and streams real-time metrics.

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`agent start`](#thresh-agent-start) | Start the agent daemon |
| [`agent stop`](#thresh-agent-stop) | Stop the agent daemon |
| [`agent status`](#thresh-agent-status) | Show connection status |
| [`agent config set`](#thresh-agent-config-set) | Set a configuration value |
| [`agent config get`](#thresh-agent-config-get) | Get a configuration value |
| [`agent config list`](#thresh-agent-config-list) | List all configuration |

---

## thresh agent start

Start the thresh agent daemon and connect to Thresh Hub.

### Synopsis

<Tabs groupId="operating-systems">
  <TabItem value="linux" label="Linux" default>

```bash
thresh agent start
```

  </TabItem>
  <TabItem value="windows" label="Windows">

```powershell
thresh agent start
```

  </TabItem>
  <TabItem value="macos" label="macOS">

```bash
thresh agent start
```

  </TabItem>
</Tabs>

### Description

Starts the agent daemon in the background. The agent:

1. Loads configuration from `~/.thresh/agent.json`
2. Authenticates with Thresh Hub using the configured API key
3. Opens a SignalR WebSocket connection to the Hub
4. Streams system metrics at the configured interval (default: 30s)
5. Automatically reconnects on connection loss

### Prerequisites

Before starting the agent, configure the Hub URL and API key:

```bash
thresh agent config set midtier-url https://your-hub:7200
thresh agent config set api-key thresh_live_xxxxxxxxxxxx
```

### Example

```bash
thresh agent start
# Agent started. Connected to https://hub.example.com:7200
# Agent ID: 5f6d5891-76d2-466f-a33f-7b87acb17653
```

---

## thresh agent stop

Stop the running agent daemon.

### Synopsis

<Tabs groupId="operating-systems">
  <TabItem value="linux" label="Linux" default>

```bash
thresh agent stop
```

  </TabItem>
  <TabItem value="windows" label="Windows">

```powershell
thresh agent stop
```

  </TabItem>
  <TabItem value="macos" label="macOS">

```bash
thresh agent stop
```

  </TabItem>
</Tabs>

### Example

```bash
thresh agent stop
# Agent stopped.
```

---

## thresh agent status

Show the current connection status of the agent.

### Synopsis

<Tabs groupId="operating-systems">
  <TabItem value="linux" label="Linux" default>

```bash
thresh agent status
```

  </TabItem>
  <TabItem value="windows" label="Windows">

```powershell
thresh agent status
```

  </TabItem>
  <TabItem value="macos" label="macOS">

```bash
thresh agent status
```

  </TabItem>
</Tabs>

### Example Output

**Connected:**
```
Agent Status
────────────────────────────────────────
Agent ID:    5f6d5891-76d2-466f-a33f-7b87acb17653
Status:      Connected ✓
Hub URL:     https://192.168.4.85:7200
Transport:   SignalR
Uptime:      2h 14m
Last Report: 28 seconds ago
```

**Disconnected:**
```
Agent Status
────────────────────────────────────────
Agent ID:    5f6d5891-76d2-466f-a33f-7b87acb17653
Status:      Disconnected ✗
Hub URL:     https://192.168.4.85:7200
Last Error:  Connection refused
Retry In:    4 seconds
```

**Not configured:**
```
Agent not configured. Run:
  thresh agent config set midtier-url <url>
  thresh agent config set api-key <key>
```

---

## thresh agent config set

Set an agent configuration value.

### Synopsis

```bash
thresh agent config set <key> <value>
```

### Keys

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `midtier-url` | URL | — | Primary Thresh Hub URL (required) |
| `api-key` | string | — | API key from Thresh Hub UI (required) |
| `tls-verify` | bool | `true` | Verify Hub TLS certificate |
| `reconnect-delay` | int | `5` | Seconds between reconnect attempts |
| `metrics-interval` | int | `30` | Seconds between metric reports |
| `fallback-url` | URL | `""` | Backup Hub URL for HA setups |
| `fallback-api-key` | string | `""` | API key for fallback Hub |
| `auto-failover` | bool | `false` | Auto-switch to fallback on outage |
| `failover-timeout` | int | `30` | Seconds before triggering failover |
| `failback-enabled` | bool | `true` | Return to primary when it recovers |
| `transport` | string | `"auto"` | Transport mode: `auto`, `signalr`, `rest` |

### Examples

```bash
# Required configuration
thresh agent config set midtier-url https://192.168.4.85:7200
thresh agent config set api-key thresh_live_xxxxxxxxxxxx

# Disable TLS verification (self-signed certs)
thresh agent config set tls-verify false

# Set metrics reporting interval to 60 seconds
thresh agent config set metrics-interval 60

# Configure high-availability failover
thresh agent config set fallback-url https://backup-hub:7200
thresh agent config set auto-failover true
thresh agent config set failover-timeout 30

# Force REST-only transport
thresh agent config set transport rest
```

:::tip
`midtier-url` and `api-key` are the only required values. Everything else has sensible defaults.
:::

---

## thresh agent config get

Get a single agent configuration value.

### Synopsis

```bash
thresh agent config get <key>
```

### Example

```bash
thresh agent config get midtier-url
# https://192.168.4.85:7200

thresh agent config get tls-verify
# false
```

---

## thresh agent config list

List all agent configuration values.

### Synopsis

```bash
thresh agent config list
```

### Example Output

```
Agent Configuration (~/.thresh/agent.json)
──────────────────────────────────────────────────
AgentId:              5f6d5891-76d2-466f-a33f-7b87acb17653
Enabled:              true
MidtierUrl:           https://192.168.4.85:7200
ApiKey:               thresh_live_****
TlsVerify:            false
ReconnectDelay:       5s
MetricsInterval:      30s
Transport:            auto
AutoFailover:         false
FallbackUrl:          (not set)
FailoverTimeout:      30s
FailbackEnabled:      true
```

---

## Configuration File

Agent configuration is stored at `~/.thresh/agent.json`:

```json
{
  "AgentId": "5f6d5891-76d2-466f-a33f-7b87acb17653",
  "Enabled": true,
  "MidtierUrl": "https://hub.example.com:7200",
  "ApiKey": "thresh_live_xxxx",
  "FallbackUrl": "",
  "FallbackApiKey": "",
  "Transport": "auto",
  "TlsVerify": true,
  "ReconnectDelay": 5,
  "MetricsInterval": 30,
  "AutoFailover": false,
  "FailoverTimeoutSeconds": 30,
  "FailbackEnabled": true
}
```

:::note
The `AgentId` is auto-generated on first start and should not be changed. It uniquely identifies this node in the Hub.
:::

---

## Transport Modes

| Mode | Description |
|------|-------------|
| `auto` | Try SignalR first; fall back to REST if unavailable (recommended) |
| `signalr` | Use SignalR WebSocket only |
| `rest` | Use REST API only (polling) |

SignalR provides a persistent, low-latency connection ideal for real-time metrics. REST polling is available for environments where WebSocket connections are restricted.

---

## Quick Start

```bash
# 1. Configure
thresh agent config set midtier-url https://your-hub:7200
thresh agent config set api-key thresh_live_xxxx
thresh agent config set tls-verify false   # if using self-signed cert

# 2. Start
thresh agent start

# 3. Verify
thresh agent status
```

Your node will now appear in the Thresh Hub UI.
