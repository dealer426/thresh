---
sidebar_position: 1
title: Introduction
description: Overview of thresh - AI-powered container environment manager for Windows, Linux, and macOS
---

import Admonition from '@theme/Admonition';

# Introduction to thresh

**AI-powered container environment manager for Windows, Linux, and macOS**

:::tip What's New in v1.7.0 — Fleet Management & Stacks
🛠️ **Full fleet management** is here! Authenticate with Thresh Hub, manage remote nodes, organize clusters, and deploy multi-service stacks — all from the CLI or Hub dashboard.

```bash
# Authenticate with your Hub
thresh auth login --hub https://your-hub:7200

# List your fleet
thresh node list

# Deploy to a remote node
thresh node up thresh-node-1 python-dev

# Organize nodes into clusters
thresh cluster create production
thresh cluster add-node production thresh-node-1
```

➡️ [Read the full v1.7.0 blog post →](/blog/thresh-1.7.0-stacks) &nbsp;|&nbsp; [Node CLI reference →](/docs/cli-reference/node) &nbsp;|&nbsp; [Cluster CLI reference →](/docs/cli-reference/cluster)
:::

thresh is a **.NET 10 Native AOT** command-line tool that provisions container-based development environments using AI-generated blueprints. Create development environments in seconds with natural language prompts, manage remote nodes and clusters through **Thresh Hub**, and deploy multi-service stacks with dependency ordering.

## Architecture Overview

thresh provides isolated development environments using lightweight containers across multiple platforms:

```mermaid
graph TB
    subgraph Platforms["Cross-Platform Support"]
        direction LR
        Win[🪟 Windows]
        Lin[🐧 Linux]
        Mac[🍎 macOS]
    end
    
    subgraph CLI["thresh CLI"]
        Commands[Commands]
        Blueprints[Blueprints]
        Config[Configuration]
        AI[AI Assistant]
        Agent[Agent Mode]
        NodeCmd[Node / Cluster]
    end
    
    subgraph Runtime["Container Runtime"]
        WSL[WSL 2<br/>Windows]
        Docker[Docker<br/>All Platforms]
        Nerdctl[nerdctl<br/>Linux/macOS]
        Containerd[containerd<br/>Linux/macOS]
    end
    
    subgraph Envs["Isolated Environments"]
        E1[python-dev]
        E2[node-dev]
        E3[alpine-minimal]
        E4[ubuntu-dev]
    end

    subgraph HubStack["Thresh Hub"]
        H[Hub API :7200]
        UI[Web Dashboard]
        Stacks[Stacks Engine]
    end

    subgraph MidTier["Mid-Tier (optional)"]
        MT[Aggregator]
    end

    subgraph Fleet["Fleet Nodes"]
        A1[Agent 1]
        A2[Agent 2]
        A3[Agent N]
    end
    
    Platforms --> CLI
    CLI --> Runtime
    Runtime --> Envs
    Blueprints -.->|AI Generate| AI
    NodeCmd -->|REST API| H
    H --> UI
    H --> Stacks
    H -->|SignalR| MT
    MT -->|SignalR| A1
    MT -->|SignalR| A2
    H -->|SignalR direct| A3
    
    style CLI fill:#4CAF50,stroke:#2E7D32,color:#fff
    style Runtime fill:#2196F3,stroke:#1565C0,color:#fff
    style Envs fill:#FF9800,stroke:#E65100,color:#fff
    style Platforms fill:#9C27B0,stroke:#6A1B9A,color:#fff
    style AI fill:#E91E63,stroke:#C2185B,color:#fff
    style HubStack fill:#607D8B,stroke:#37474F,color:#fff
    style MidTier fill:#FF9800,stroke:#E65100,color:#fff
    style Fleet fill:#795548,stroke:#4E342E,color:#fff
```

---

## Key Features

- 🌍 **Multi-Platform** - Windows/WSL2, Linux/Docker/nerdctl, macOS/containerd
- 🤖 **AI-Powered** - GitHub Copilot CLI integration for intelligent blueprint generation
- 🌐 **Port Mapping** - Automatic port forwarding for network services (v1.5.0)
- 📦 **Persistent Volumes** - Data persistence across environment lifecycle (v1.5.0)
- 🗄️ **Database Optimization** - WSL configuration profiles fix Plan9 filesystem issues (v1.5.0)
- 🖧 **Agent Mode** - Connect nodes to Thresh Hub for fleet management (v1.6.0)
- �️ **Hub Authentication** - Device-code and token-based CLI login (v1.7.0)
- 💻 **Remote Node Management** - Deploy, inspect, and monitor fleet nodes from CLI (v1.7.0)
- 🏢 **Cluster Orchestration** - Group nodes by region, team, or purpose (v1.7.0)
- 📦 **Hub-Managed Stacks** - Multi-service deployments via Hub dashboard and API (v1.7.0)
- ⚡ **Parallel Creation** - Create multiple environments simultaneously (10x faster)
- 📦 **Built-in Blueprints** - Alpine, Ubuntu, Debian, Python, Node.js, and more
- 🗑️ **Blueprint Management** - List, generate, and delete blueprints
- 💬 **Interactive AI Chat** - Streaming responses for blueprint assistance
- 🚀 **Native Binary** - No .NET runtime required (5-13 MB)
- 📊 **System Metrics** - Monitor CPU, memory, storage, and container usage
- 🔧 **MCP Server** - Model Context Protocol for VS Code, Cursor, Windsurf

---

## Getting Started by Platform

Choose your platform to get started:

<div class="cards">

### 🪟 [Windows](/docs/getting-started-windows)

Get started with thresh on **Windows 11** using **WSL 2**

**Requirements:**
- Windows 11
- WSL 2 enabled

[Start on Windows →](/docs/getting-started-windows)

---

### 🐧 [Linux](/docs/getting-started-linux)

Get started with thresh on **Linux** using **Docker** or **nerdctl**

**Requirements:**
- Docker or nerdctl/containerd

[Start on Linux →](/docs/getting-started-linux)

---

### 🍎 [macOS](/docs/getting-started-macos)

Get started with thresh on **macOS** (Apple Silicon) using **containerd**

**Requirements:**
- macOS (Apple Silicon M1/M2/M3)
- containerd or Docker Desktop

**Beta Support**

[Start on macOS →](/docs/getting-started-macos)

</div>

---

## Quick Example

```bash
# Install thresh (platform-specific, see guides above)

# Authenticate with GitHub Copilot CLI
copilot
# Then: /login

# List available blueprints
thresh blueprint list

# Create environment with networking and storage (v1.5.0)
thresh blueprint generate "PostgreSQL with persistent volumes and port mapping"

# Provision with automatic configuration
thresh up postgres-dev

# Access from host
psql -h localhost -p 5432 -U postgres

# Generate custom blueprint with AI
thresh blueprint generate "Python ML environment with Jupyter" --output python-ml

# Start interactive chat
thresh chat
```

---

## What's New in v1.7.0

### �️ Hub Authentication & Remote Management

Authenticate the CLI with Thresh Hub and manage your entire fleet without SSH:

```bash
# Authenticate
thresh auth login --hub https://your-hub:7200

# List fleet nodes
thresh node list

# Deploy a blueprint to a remote node
thresh node up thresh-node-1 python-dev --name ml-training

# Check remote node metrics
thresh node metrics thresh-node-1

# Organize nodes into clusters
thresh cluster create production
thresh cluster add-node production thresh-node-1
thresh cluster info production
```

**Features:**
- `thresh auth` — device-code and token-based login with Thresh Hub
- `thresh node` — list, inspect, deploy to, and remove remote nodes
- `thresh cluster` — group nodes by region, team, or purpose
- Hub-managed stacks — multi-service deployments via Hub dashboard and API

[Auth CLI reference →](/docs/cli-reference/auth) &nbsp;|&nbsp; [Node CLI reference →](/docs/cli-reference/node) &nbsp;|&nbsp; [Cluster CLI reference →](/docs/cli-reference/cluster) &nbsp;|&nbsp; [Blog post →](/blog/thresh-1.7.0-stacks)

### 🏗️ Three-Tier Architecture with Mid-Tier

v1.7.0 ships a production-ready **mid-tier aggregator** for large fleets:

```mermaid
graph LR
    subgraph "CLI Users"
        C1["thresh auth login"]
        C2["thresh node list"]
    end

    subgraph "Thresh Hub"
        Hub["Hub API<br/>:7200"]
        DB["PostgreSQL"]
        UI["Web Dashboard"]
    end

    subgraph "Mid-Tier"
        MT["Mid-Tier<br/>Aggregator"]
    end

    subgraph "Fleet"
        A1["Agent<br/>node-1"]
        A2["Agent<br/>node-2"]
        A3["Agent<br/>node-3"]
    end

    C1 & C2 -->|REST| Hub
    Hub --- DB
    Hub --- UI
    Hub -->|SignalR| MT
    MT -->|SignalR| A1
    MT -->|SignalR| A2
    MT -->|SignalR| A3

    style Hub fill:#2196F3,stroke:#1565C0,color:#fff
    style MT fill:#FF9800,stroke:#E65100,color:#fff
    style A1 fill:#607D8B,stroke:#37474F,color:#fff
    style A2 fill:#607D8B,stroke:#37474F,color:#fff
    style A3 fill:#607D8B,stroke:#37474F,color:#fff
```

**Mid-tier benefits:**
- Agents connect locally instead of across the internet
- Batched metrics reduce Hub connection count and bandwidth
- Deploy on-prem for air-gapped or restricted networks
- Scales from 3 nodes (direct) to hundreds (with mid-tier)

**Shipped in v1.7.0:** `thresh_mid_*` key authentication, config-driven TLS, stale-agent cleanup.

**Coming in v2.0:** fleet blueprints, RBAC access control, node group policies, and stack templates.

[Stacks reference →](/docs/cli-reference/stack) &nbsp;|&nbsp; [Fleet management tutorial →](/docs/tutorials/fleet-management)

---

## What's New in v1.6.0

### 🖧 Agent Mode & Hub Connectivity

Connect any thresh node to a centralized **Thresh Hub** instance for real-time fleet-wide visibility and management. Agent Mode is the foundational feature for multi-node workflows.

```bash
# Configure and start the agent
thresh agent config set midtier-url https://your-hub:7200
thresh agent config set api-key thresh_live_xxxxxxxxxxxx
thresh agent start

# Check connection health
thresh agent status

# Update hub URL
thresh agent config set midtier-url https://new-hub:7200
```

**Transport options:**

| Transport | Protocol | Use Case |
|-----------|----------|----------|
| `signalr` | WebSocket (WS/WSS) | Default — low-latency bidirectional |
| `http` | HTTP Long Polling | Fallback for restricted networks |

**What the Hub sees per node:**
- ✅ Live online / offline status
- 📊 Real-time CPU, memory, storage metrics
- 🧱 Running environments list
- 🏷️ Custom node name and region tags
- 🔗 Agent version and platform info

**Shipped in v1.7.0:** remote node/cluster management, Hub authentication, mid-tier key auth, config-driven TLS, Hub-managed stacks. **Coming in v2.0:** fleet blueprints, RBAC access control, node group policies, stack templates.

[Agent CLI reference →](/docs/cli-reference/agent) &nbsp;|&nbsp; [Blog post →](/blog/thresh-1.6.0-agent-hub)

---

## What's New in v1.5.0

### 🌐 Port Mapping & Networking

Map host ports to container services with automatic forwarding:

```json
{
  "ports": ["8080:80", "5432:5432"],
  "network": "bridge",
  "hostname": "webapp.local"
}
```

**Features:**
- Automatic port forwarding on Windows (netsh)
- Multiple port mappings
- IP binding and protocol selection
- Exposed ports for inter-container communication

[Learn more →](/docs/tutorials/networking)

### 📦 Persistent Volumes

Never lose data with three types of storage:

```json
{
  "volumes": [
    {"name": "pgdata", "mountPath": "/var/lib/postgresql/data"}
  ],
  "bindMounts": [
    {"source": "C:\\projects", "target": "/app"}
  ],
  "tmpfs": [
    {"mountPath": "/tmp", "size": "512m"}
  ]
}
```

**Features:**
- Named volumes persist across recreation
- Bind mounts for live code editing
- Tmpfs for fast temporary storage

[Learn more →](/docs/tutorials/volumes)

### 🗄️ WSL Configuration Profiles

Fix database permission errors with built-in profiles:

```json
{
  "wslConfig": "database"  // Fixes Plan9 filesystem issues
}
```

**Built-in profiles:**
- `database` - PostgreSQL, MySQL, MongoDB, Redis
- `docker` - Docker daemon auto-start
- `web-server` - Nginx/Apache auto-start
- `systemd` - Basic systemd enablement
- `minimal` - Maximum isolation
- `development` - Full development features

[Learn more →](/docs/wsl-configuration)

### 🔄 Lifecycle Management

Start and stop environments with networking:

```powershell
# Start with automatic port forwarding
thresh start webserver

# Stop and cleanup
thresh stop webserver
```

---

## Platform Support

| Platform | Runtime | Binary Size | Compression | Status |
|----------|---------|-------------|-------------|--------|
| Windows 11 | WSL2 | ~5 MB | UPX | ✅ Supported |
| Linux | Docker, nerdctl, containerd | ~5 MB | UPX | ✅ Supported |
| macOS (M1/M2/M3) | containerd, Docker | ~13 MB | None* | ✅ Beta |

*macOS binaries are uncompressed to preserve Apple code signing and notarization.

---

## Documentation

- 📚 **[Windows Guide](/docs/getting-started-windows)** - Complete Windows setup
- 🐧 **[Linux Guide](/docs/getting-started-linux)** - Complete Linux setup
- 🍎 **[macOS Guide](/docs/getting-started-macos)** - Complete macOS setup (Beta)
- 🔧 **[CLI Reference](/docs/cli-reference)** - Complete command documentation
- 🤖 **[MCP Integration](/docs/mcp-integration)** - VS Code, Cursor, Windsurf setup
- 📖 **[Tutorials](/docs/tutorials)** - Step-by-step guides

---

## Support

- **Issues**: [GitHub Issues](https://github.com/dealer426/thresh/issues)
- **Discussions**: [GitHub Discussions](https://github.com/dealer426/thresh/discussions)
- **Repository**: [GitHub](https://github.com/dealer426/thresh)

---

## Next Steps

Choose your platform to get started:

- 🪟 **[Windows 11 → Get Started](/docs/getting-started-windows)**
- 🐧 **[Linux → Get Started](/docs/getting-started-linux)**
- 🍎 **[macOS → Get Started](/docs/getting-started-macos)**

**Happy provisioning!** 🚀
