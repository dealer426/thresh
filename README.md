# thresh - AI-Powered Container Environment Manager

**Cross-platform CLI for provisioning development environments with AI**

[![Version](https://img.shields.io/badge/version-1.6.0-blue.svg)](https://github.com/dealer426/thresh/releases)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Native AOT](https://img.shields.io/badge/Native%20AOT-Yes-green.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build](https://github.com/dealer426/thresh/actions/workflows/build-multiplatform.yml/badge.svg)](https://github.com/dealer426/thresh/actions)

---

## Overview

`thresh` is a **.NET 10 Native AOT** command-line tool that provisions container-based development environments using AI-generated blueprints. Create development environments in seconds with natural language prompts, manage persistent storage and networking, and connect nodes to a centralized Thresh Hub for fleet management.

**✨ Key Features:**
- 🌍 **Multi-Platform** - Windows/WSL2, Linux/Docker/nerdctl, macOS/containerd
- 🤖 **AI-Powered** - GitHub Copilot SDK integration for intelligent blueprint generation
- 🌐 **Full Networking** - Port mapping, network modes, hostnames, automatic WSL2 forwarding
- 💾 **Persistent Storage** - Named volumes, bind mounts, tmpfs for data persistence
- ⚙️ **WSL Configuration** - 6 built-in profiles to fix database permissions and optimize environments
- 🔄 **Lifecycle Management** - Start and stop environments without losing data
- ⚡ **Parallel Creation** - Create multiple environments simultaneously (10x faster)
- 📦 **Built-in Blueprints** - Alpine, Ubuntu, Debian, Python, Node.js, and more
- 🗑️ **Blueprint Management** - List, generate, and delete blueprints
- 💬 **Interactive AI Chat** - Streaming responses for blueprint assistance
- 🚀 **Native Binary** - No .NET runtime required (5-13 MB)
- 📊 **System Metrics** - Monitor CPU, memory, storage, and container usage
- 🔧 **MCP Server** - Model Context Protocol for VS Code, Cursor, Windsurf
- 🖧 **Agent Mode** *(v1.6.0+)* - Connect nodes to Thresh Hub for centralized fleet management

---

## Quick Start

### Installation

**Download Pre-built Binaries:**

```bash
# Linux
wget https://github.com/dealer426/thresh/releases/latest/download/thresh-linux-x64.tar.gz
tar -xzf thresh-linux-x64.tar.gz
sudo mv thresh /usr/local/bin/
chmod +x /usr/local/bin/thresh

# macOS (Apple Silicon)
curl -L https://github.com/dealer426/thresh/releases/latest/download/thresh-macos-arm64.tar.gz -o thresh.tar.gz
tar -xzf thresh.tar.gz
sudo mv thresh /usr/local/bin/
chmod +x /usr/local/bin/thresh

# Windows (PowerShell)
Invoke-WebRequest -Uri \"https://github.com/dealer426/thresh/releases/latest/download/thresh-windows-x64.zip\" -OutFile thresh.zip
Expand-Archive thresh.zip -DestinationPath .
Move-Item thresh.exe C:\\Windows\\System32\\
```

**Build from Source:**

```bash
cd thresh/Thresh
dotnet publish -c Release -r linux-x64 --self-contained   # Linux
dotnet publish -c Release -r osx-arm64 --self-contained   # macOS
dotnet publish -c Release -r win-x64 --self-contained     # Windows
```

### First Steps

**1. Verify thresh installation:**

```bash
thresh version
```

**2. Install GitHub Copilot CLI (required for AI features):**

Learn more: https://github.com/github/copilot-cli

```bash
# Windows (WinGet)
winget install GitHub.Copilot

# macOS (Homebrew)
brew install copilot-cli

# Linux (Homebrew)
brew install copilot-cli

# Alternative: npm (all platforms)
npm install -g @github/copilot

# Alternative: Install script (macOS/Linux)
curl -fsSL https://gh.io/copilot-install | bash
```

**3. Authenticate GitHub Copilot:**

```bash
# Launch Copilot CLI and use /login command
copilot
# Then type: /login
```

**4. Start using thresh:**

```bash
# List available blueprints
thresh blueprint list

# Create your first environment
thresh up alpine-minimal

# Generate custom blueprint with AI
thresh blueprint generate \"Python ML environment with Jupyter\" --output python-ml

# Start interactive chat
thresh chat
```

---

## Platform Support

| Platform | Runtime | Binary Size | Compression | Status |
|----------|---------|-------------|-------------|--------|
| Windows 11 | WSL2 | ~5 MB | UPX | ✅ Supported |
| Linux | Docker, nerdctl, containerd | ~5 MB | UPX | ✅ Supported |
| macOS (M1/M2/M3) | containerd, Docker | ~13 MB | None* | ✅ Supported |

*macOS binaries are uncompressed to preserve Apple code signing and notarization.

### Requirements

**Platform-Specific:**
- **Windows**: Windows 11 with WSL2 enabled
- **Linux**: Docker, nerdctl, or containerd installed
- **macOS**: containerd or Docker Desktop (Apple Silicon only)

**AI Features (Optional but Recommended):**
- **GitHub CLI + GitHub Copilot CLI** - Required for AI blueprint generation and chat
  ```bash
  # 1. Install GitHub CLI
  # Windows: winget install GitHub.cli
  # Linux: sudo apt install gh  # or brew install gh
  # macOS: brew install gh
  
  # 2. Authenticate with GitHub
  gh auth login
  
  # 3. Install GitHub Copilot CLI extension
  gh extension install github/gh-copilot
  
  # 4. Verify thresh can access Copilot
  thresh config status
  ```
  
  **More Info:** https://github.com/features/copilot/cli
  
  Without GitHub CLI and Copilot CLI extension, you can still use built-in blueprints and manual blueprint creation,
  but AI features (`thresh blueprint generate`, `thresh chat`) will not be available.

---

## Commands

### Blueprint Management

```bash
# List all blueprints (built-in + generated)
thresh blueprint list

# Generate blueprint from natural language
thresh blueprint generate \"nginx web server with SSL\" --output nginx-ssl

# Delete generated blueprint
thresh blueprint delete nginx-ssl

# Interactive AI chat
thresh chat
```

### Environment Management

```bash
# Create environment from blueprint
thresh up alpine-minimal

# Create with custom name
thresh up python-dev --name ml-project

# Create with networking and storage (v1.5.0+)
thresh up webserver --name api-server

# List all environments
thresh list

# Start stopped environment (v1.5.0+)
thresh start api-server

# Stop running environment without data loss (v1.5.0+)
thresh stop api-server

# Destroy environment (with confirmation)
thresh destroy alpine-minimal

# Destroy without confirmation
thresh destroy alpine-minimal -y
```

### Networking & Ports (v1.5.0+)

```bash
# Port mapping is configured in blueprints:
{
  "ports": ["8080:80", "5432:5432", "127.0.0.1:3000:3000"],
  "expose": ["9090"],        // Inter-container communication
  "network": "bridge",       // Network mode
  "hostname": "api.local"    // Custom hostname
}

# Windows (WSL2): Automatic port forwarding to localhost
# Linux/macOS: Explicit port mapping with -p flags
```

### Persistent Storage (v1.5.0+)

```bash
# List all volumes
thresh volume list

# Create named volume
thresh volume create app-data

# Inspect volume details
thresh volume inspect app-data

# Delete volume
thresh volume delete app-data

# Configure in blueprints:
{
  "volumes": [
    {"name": "db-data", "mount": "/var/lib/postgresql/data"}
  ],
  "bind_mounts": [
    {"host": "C:/projects/myapp", "container": "/app"}
  ],
  "tmpfs": ["/tmp", "/cache"]
}
```

### WSL Configuration (v1.5.0+, Windows only)

```bash
# List available profiles
thresh wslconf list

# Show profile content
thresh wslconf show database

# Show all configuration options
thresh wslconf options

# Validate custom profile
thresh wslconf validate my-profile.wslconf

# Built-in profiles:
# - database    (fixes PostgreSQL/MySQL/MongoDB permissions)
# - docker      (enables systemd for Docker)
# - web-server  (optimized for nginx/Apache)
# - systemd     (basic systemd support)
# - minimal     (maximum isolation)
# - development (balanced for general dev work)

# Use in blueprints:
{
  "wslConfig": "database"  // Apply built-in profile
}
```

### Agent Mode (v1.6.0+)

```bash
# Configure agent connection to Thresh Hub
thresh agent config set midtier-url https://192.168.1.100:7200
thresh agent config set api-key thresh_live_xxxx
thresh agent config set tls-verify false        # Use for self-signed certs

# Start/stop the agent daemon
thresh agent start
thresh agent stop

# Check connection status and agent ID
thresh agent status

# View all agent configuration
thresh agent config list
thresh agent config get midtier-url
```

### System Metrics

```bash
# Show system metrics (text)
thresh metrics

# Export as JSON
thresh metrics --format json
```

### MCP Server

```bash
# Start MCP server for AI agent integration
thresh serve

# Start in stdio mode (for VS Code/Cursor/Windsurf)
thresh serve --stdio
```

### Configuration

```bash
# Show current configuration
thresh config list

# Set configuration value
thresh config set default-model gpt-4o

# Get specific value
thresh config get default-model
```

## Available AI Models

AI features require GitHub CLI authentication (`gh auth login`). No API keys needed.

| Model | Provider | Best For |
|-------|----------|----------|
| `gpt-4o` | OpenAI | Default; best all-around |
| `gpt-4o-mini` | OpenAI | Fast, cost-effective |
| `o1-preview` | OpenAI | Complex multi-step blueprints |
| `claude-3.5-sonnet` | Anthropic | Complex tasks, long outputs |
| `claude-3.5-haiku` | Anthropic | Fast Claude variant |
| `gemini-1.5-pro` | Google | 1M token context window |
| `llama-3.1-405b` | Meta | Largest open-source model |
| `mistral-large` | Mistral | European open-source option |

```bash
# Use a specific model
thresh blueprint generate "multi-tier app" --model claude-3.5-sonnet
thresh chat --model o1-preview
```

---

## Breaking Changes in v1.4.0

| Old Command | New Command |
|-------------|-------------|
| `thresh blueprints` | `thresh blueprint list` |
| `thresh generate <prompt>` | `thresh blueprint generate <prompt>` |

---

## Blueprint Format

Blueprints are JSON files that define environments:

```json
{
  "name": "fullstack-app",
  "description": "Full-stack app with database and networking",
  "base": "ubuntu-22.04",
  "packages": ["nodejs", "npm", "postgresql-14"],
  "environment": {
    "NODE_ENV": "development",
    "DATABASE_URL": "postgresql://localhost:5432/myapp"
  },
  "scripts": {
    "postInstall": "npm install -g pm2"
  },
  "ports": ["3000:3000", "8080:8080", "5432:5432"],
  "expose": ["9090"],
  "network": "bridge",
  "hostname": "app.local",
  "volumes": [
    {"name": "postgres-data", "mount": "/var/lib/postgresql/data"},
    {"name": "app-logs", "mount": "/var/log/app"}
  ],
  "bind_mounts": [
    {"host": "C:/projects/myapp", "container": "/app"}
  ],
  "tmpfs": ["/tmp", "/cache"],
  "wslConfig": "database"
}
```

**Blueprint Fields:**

| Field | Version | Description |
|-------|---------|-------------|
| `name`, `description` | v1.0+ | Blueprint metadata |
| `base` | v1.0+ | Base container image |
| `packages` | v1.0+ | Packages to install |
| `environment` | v1.0+ | Environment variables |
| `scripts` | v1.0+ | Shell scripts (setup, postInstall) |
| `ports` | v1.5.0+ | Port mappings (`host:container`) |
| `expose` | v1.5.0+ | Inter-container exposed ports |
| `network` | v1.5.0+ | Network mode (bridge, host, none) |
| `hostname` | v1.5.0+ | Custom container hostname |
| `volumes` | v1.5.0+ | Named persistent volume mounts |
| `bind_mounts` | v1.5.0+ | Host directory mounts |
| `tmpfs` | v1.5.0+ | In-memory temporary filesystems |
| `wslConfig` | v1.5.0+ | WSL profile (Windows only) |

---

## Documentation

- 🤖 **[MCP Integration Guide](docs/MCP_INTEGRATION.md)** - VS Code, Cursor, Windsurf setup
- 🗺️ **[Roadmap](docs/ROADMAP_2026.md)** - Future plans and features
- 📝 **[Changelog](CHANGELOG.md)** - Full version history
- 📖 **[Documentation Site](https://dealer426.github.io/thresh/)** - Tutorials and guides

---

## Architecture

### Project Structure

```
thresh/
├── thresh/Thresh/               # Main CLI application
│   ├── Program.cs               # CLI entry point and command definitions
│   ├── Services/
│   │   ├── AgentService.cs      # Agent daemon - SignalR hub connection (v1.6.0)
│   │   ├── ConfigurationService.cs  # Agent/MCP configuration management (v1.6.0)
│   │   ├── BlueprintService.cs  # Environment provisioning
│   │   ├── GitHubCopilotService.cs  # AI integration
│   │   ├── WslService.cs        # WSL2 + port forwarding + volumes
│   │   ├── DockerService.cs     # Docker container management
│   │   ├── ContainerdService.cs # containerd management
│   │   └── WslConfigService.cs  # WSL profile management (v1.5.0)
│   ├── Models/
│   │   ├── AgentConfiguration.cs  # Agent config model (v1.6.0)
│   │   ├── AgentModels.cs         # Agent status/metrics models (v1.6.0)
│   │   └── Blueprint.cs           # Blueprint model
│   ├── Mcp/                     # MCP server implementation
│   └── blueprints/              # Built-in blueprints
├── pulumi/                      # vSphere multi-node deployment
│   └── Program.cs               # Automated cluster deployment
├── website/                     # Docusaurus documentation
├── docs/                        # Additional documentation
└── packages/                    # Package manager configs (Chocolatey, Scoop, Winget)
```

### Technology Stack

- **.NET 10** with Native AOT compilation
- **System.CommandLine** for CLI framework
- **Microsoft.AspNetCore.SignalR.Client** for agent hub connectivity
- **GitHub.Copilot.SDK** for AI integration
- **GitHub Actions** for multi-platform CI/CD

---

## Performance

| Metric | Value |
|--------|-------|
| Binary Size (Linux/Windows) | ~5 MB (UPX compressed) |
| Binary Size (macOS) | ~13 MB (uncompressed) |
| Startup Time | ~20-30ms |
| Memory Usage (Idle) | ~30MB |
| Agent Memory Usage | ~25MB |
| Provision Time (Alpine) | ~15s |
| Provision Time (Ubuntu) | ~25s |
| AI First Token | ~1-2s |

---

## What's New

### v1.6.0 — Agent Mode & Hub Connectivity

The flagship feature of v1.6.0 is **agent mode**, enabling any thresh node to connect to a centralized **Thresh Hub** for fleet management, remote visibility, and aggregated metrics.

**New Commands:**
```bash
thresh agent start                              # Start agent daemon
thresh agent stop                               # Stop agent daemon
thresh agent status                             # Connection status and agent ID
thresh agent config set midtier-url <url>       # Set Hub URL
thresh agent config set api-key <key>           # Set API key
thresh agent config list                        # Show all configuration
```

**Connect to a Thresh Hub:**
```bash
thresh agent config set midtier-url https://192.168.1.100:7200
thresh agent config set api-key thresh_live_xxxx
thresh agent config set tls-verify false        # For self-signed certs
thresh agent start
```

**Transport:** SignalR WebSocket (primary) → REST API (automatic fallback)

**High Availability:** Configure `FallbackUrl` + `AutoFailover` for automatic failover if the primary Hub goes offline.

**Agent config** (`~/.thresh/agent.json`):
```json
{
  "AgentId": "5f6d5891-76d2-466f-a33f-7b87acb17653",
  "Enabled": true,
  "MidtierUrl": "https://hub.example.com:7200",
  "ApiKey": "thresh_live_xxxx",
  "TlsVerify": false,
  "ReconnectDelay": 5,
  "MetricsInterval": 30,
  "AutoFailover": false,
  "FallbackUrl": "https://cloud.thresh.sh"
}
```

---

### v1.5.0 — Networking, Storage & Lifecycle

**Port Mapping:**
```json
{
  "ports": ["8080:80", "5432:5432", "127.0.0.1:3000:3000"]
}
```
- Windows (WSL2): Automatic `netsh` port proxy forwarding to localhost
- Linux/macOS: Explicit port mapping with container runtime
- Protocol support (TCP/UDP)

**Persistent Volumes:**
```json
{
  "volumes": [
    {"name": "postgres-data", "mount": "/var/lib/postgresql/data"},
    {"name": "app-logs", "mount": "/var/log/app"}
  ]
}
```
Data survives environment recreation. CLI: `thresh volume list|create|inspect|delete`

**WSL Configuration (Windows):**
```json
{ "wslConfig": "database" }
```
Fixes Plan9 filesystem permissions for PostgreSQL/MySQL/Redis on Windows.

**Lifecycle:**
```bash
thresh stop my-env    # Graceful stop, data preserved
thresh start my-env   # Resume from stopped state
```

---

## Contributing

```bash
git clone https://github.com/dealer426/thresh.git
cd thresh/thresh/Thresh
dotnet build
dotnet run -- --version
```

Issues and PRs welcome: [GitHub Issues](https://github.com/dealer426/thresh/issues)

---

## License

MIT License — see [LICENSE](LICENSE) for details.

---

## Acknowledgments

- **GitHub Copilot SDK** - AI-powered blueprint generation
- **.NET Team** - Native AOT compilation support
- **Docusaurus Team** - Documentation framework
- **Community Contributors** - Testing and feedback

---

**Built with .NET 10 Native AOT** | **Cross-Platform** | **AI-Powered** | **Zero Dependencies**
