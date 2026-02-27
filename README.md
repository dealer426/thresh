# thresh - AI-Powered Container Environment Manager

**Cross-platform CLI for provisioning development environments with AI**

[![Version](https://img.shields.io/badge/version-1.5.0-blue.svg)](https://github.com/dealer426/thresh/releases)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Native AOT](https://img.shields.io/badge/Native%20AOT-Yes-green.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build](https://github.com/dealer426/thresh/actions/workflows/build-multiplatform.yml/badge.svg)](https://github.com/dealer426/thresh/actions)

---

## Overview

`thresh` is a **.NET 10 Native AOT** command-line tool that provisions container-based development environments using AI-generated blueprints. Create development environments in seconds with natural language prompts.

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

**Available AI Models** (via GitHub Copilot SDK):

**GPT Models (OpenAI):**
- **gpt-4o** - GPT-4 Optimized, most capable multimodal model (default)
- **gpt-4o-mini** - Faster, more affordable GPT-4o variant
- **gpt-4-turbo** - GPT-4 Turbo with 128K context window
- **gpt-4** - Standard GPT-4 model
- **gpt-3.5-turbo** - Fast, cost-effective legacy model

**Reasoning Models (OpenAI):**
- **o1-preview** - Advanced reasoning, slower but more thoughtful
- **o1-mini** - Fast reasoning for simpler tasks

**Claude Models (Anthropic):**
- **claude-3.5-sonnet** - Latest Claude 3.5 (recommended for complex tasks)
- **claude-3.5-haiku** - Faster Claude 3.5 variant
- **claude-3-opus** - Most capable Claude 3 model
- **claude-3-sonnet** - Balanced Claude 3
- **claude-3-haiku** - Fast Claude 3

**Gemini Models (Google):**
- **gemini-1.5-pro** - Google's Gemini Pro with 1M token context
- **gemini-1.5-flash** - Faster Gemini variant

**Open Source Models:**
- **llama-3.1-405b** - Meta's largest Llama 3.1 (most capable)
- **llama-3.1-70b** - Meta's Llama 3.1 70B
- **llama-3.1-8b** - Meta's Llama 3.1 8B (fastest)
- **mistral-large** - Mistral AI's large model
- **mistral-nemo** - Smaller Mistral variant

**Usage Examples:**
```powershell
# Use specific model for generation
thresh generate "Python dev env" --model claude-3.5-sonnet

# Use reasoning model for complex blueprints
thresh generate "Multi-tier app with Redis, PostgreSQL, nginx" --model o1-preview

# Interactive chat with Claude
thresh chat --model claude-3.5-sonnet
```

**Note**: All AI features require GitHub CLI authentication (`gh auth login`). No API keys needed!

---

## 🚨 Breaking Changes in v1.4.0

**Command structure has changed from flat to grouped:**

| Old (v1.3.0 and earlier) | New (v1.4.0+) |
|--------------------------|---------------|
| `thresh blueprints` | `thresh blueprint list` |
| `thresh generate <prompt>` | `thresh blueprint generate <prompt>` |
| *(no command)* | `thresh blueprint delete <name>` |

The old commands will show helpful error messages with suggestions.

**Migration Example:**

```bash
# Old way (v1.3.0)
thresh blueprints
thresh generate \"redis cache\"

# New way (v1.4.0+)
thresh blueprint list
thresh blueprint generate \"redis cache\" --output redis-cache
thresh blueprint delete redis-cache
```

### System Metrics & Monitoring

```powershell
# Display system metrics (CPU, memory, storage, containers)
thresh metrics

# Export metrics as JSON for monitoring tools
thresh metrics --json

# Example JSON output
{
  "hostname": "DESKTOP-ABC123",
  "platform": "Windows",
  "runtime": "WSL",
  "runtime_version": "2.1.5.0",
  "cpu_cores": 8,
  "cpu_percent": 23.5,
  "memory_used_gb": 12.4,
  "memory_total_gb": 32.0,
  "memory_percent": 38.75,
  "storage_free_gb": 450.2,
  "storage_total_gb": 950.0,
  "containers": 5
}
```

### MCP Server Integration

```powershell
# Start MCP server for AI editor integration (VS Code, Cursor, Windsurf)
thresh serve

# Available MCP tools:
# - list_environments - List all WSL environments
# - create_environment - Create environments from blueprints
# - destroy_environment - Remove environments
# - list_blueprints - Show available blueprints
# - get_blueprint - Get blueprint details
# - get_version - Show thresh version and runtime info
# - generate_blueprint - AI-powered blueprint generation

# Configure in VS Code settings.json:
{
  "mcp.servers": {
    "thresh": {
      "command": "C:\\path\\to\\thresh.exe",
      "args": ["serve"],
      "serverType": "stdio"
    }
  }
}
```

---

## Blueprint Format

Blueprints are JSON files that define environments:

```json
{
  "name": "python-ml",
  "description": "Python machine learning environment",
  "base": "ubuntu-22.04",
  "packages": [
    "python3",
    "python3-pip",
    "python3-venv",
    "build-essential"
  ],
  "environment": {
    "PYTHONUNBUFFERED": "1"
  },
  "scripts": {
    "setup": "pip3 install --upgrade pip",
    "postInstall": "pip3 install jupyter pandas numpy scikit-learn"
  },
  "ports": ["8888:8888", "5000:5000"],
  "volumes": [
    {"name": "jupyter-data", "mount": "/home/jupyter"}
  ],
  "bind_mounts": [
    {"host": "C:/projects/notebooks", "container": "/notebooks"}
  ],
  "tmpfs": ["/tmp"],
  "network": "bridge",
  "hostname": "ml-workstation",
  "wslConfig": "development"
}
```

**New in v1.5.0:**
- **ports** - Port mappings (host:container or IP:host:container)
- **expose** - Exposed ports for inter-container communication
- **network** - Network mode (bridge, host, none)
- **hostname** - Custom hostname
- **volumes** - Named persistent volumes
- **bind_mounts** - Host directory mounts
- **tmpfs** - In-memory temporary filesystems
- **wslConfig** - WSL configuration profile (Windows only)

**Supported Base Images:**
- Ubuntu: 20.04, 22.04, 24.04
- Alpine: 3.18, 3.19, edge
- Debian: 11, 12
- And more...

---

## Documentation

- 📚 **[Getting Started Guide](GETTING_STARTED.md)** - Detailed setup and usage
- 📖 **[Full Documentation](https://dealer426.github.io/thresh/)** - Docusaurus site with tutorials
- 🔧 **[CLI Reference](website/docs/cli-reference/)** - Complete command documentation
- 🌐 **[Networking Tutorial](website/docs/tutorials/networking.md)** - Port mapping and network configuration (v1.5.0)
- 📦 **[Volumes Tutorial](website/docs/tutorials/volumes.md)** - Persistent storage guide (v1.5.0)
- ⚙️ **[WSL Configuration Guide](website/docs/wsl-configuration.md)** - Database optimization profiles (v1.5.0)
- 🤖 **[MCP Integration Guide](docs/MCP_INTEGRATION.md)** - VS Code, Cursor, Windsurf setup
- 🗺️ **[Roadmap](docs/ROADMAP_2026.md)** - Future plans and features
- 📝 **[Changelog](CHANGELOG.md)** - Version history and changes

---

## Architecture

### Project Structure

```
thresh/
├── Thresh/                      # Main CLI application
│   ├── Program.cs               # CLI entry point
│   ├── Services/                # Core services
│   │   ├── BlueprintService.cs  # Environment provisioning
│   │   ├── GitHubCopilotService.cs  # AI integration
│   │   ├── ContainerServiceFactory.cs  # Multi-platform support
│   │   └── WslService.cs / DockerService.cs / ContainerdService.cs
│   ├── Models/                  # Data models
│   ├── Mcp/                     # MCP server implementation
│   └── blueprints/              # Built-in blueprints
├── website/                     # Docusaurus documentation
├── docs/                        # Additional documentation
└── packages/                    # Package manager configs

```

### Technology Stack

- **.NET 10** with Native AOT compilation
- **System.CommandLine** for CLI framework
- **GitHub.Copilot.SDK** for AI integration
- **Docusaurus** for documentation
- **GitHub Actions** for CI/CD

---

## Performance

| Metric | Value |
|--------|-------|
| Binary Size (Linux/Windows) | ~5 MB (UPX compressed) |
| Binary Size (macOS) | ~13 MB (uncompressed) |
| Startup Time | ~20-30ms |
| Memory Usage (Idle) | ~30MB |
| Provision Time (Alpine) | ~15s |
| Provision Time (Ubuntu) | ~25s |
| AI First Token | ~1-2s |

---

## What's New in v1.5.0

### 🌐 Full Networking Support

**Port Mapping:**
```json
{
  "ports": ["8080:80", "5432:5432", "127.0.0.1:3000:3000"]
}
```

- Flexible port mapping syntax (host:container, IP:host:container)
- **Windows (WSL2)**: Automatic port forwarding to localhost
- **Linux/macOS**: Explicit port mapping with container runtime
- Protocol support (TCP/UDP)

**Advanced Networking:**
```json
{
  "expose": ["9090"],      // Inter-container communication
  "network": "bridge",     // Network modes: bridge, host, none
  "hostname": "api.local"  // Custom hostname
}
```

### 💾 Persistent Storage

**Named Volumes:**
```json
{
  "volumes": [
    {"name": "postgres-data", "mount": "/var/lib/postgresql/data"},
    {"name": "app-logs", "mount": "/var/log/app"}
  ]
}
```

- Data survives environment recreation
- Platform-specific: directory-based (Windows) or containerd-managed (Linux)
- CLI management: `thresh volume list|create|inspect|delete`

**Bind Mounts:**
```json
{
  "bind_mounts": [
    {"host": "C:/projects/myapp", "container": "/app", "readonly": false}
  ]
}
```

- Direct host filesystem access
- Perfect for live code editing

**Tmpfs (In-Memory):**
```json
{
  "tmpfs": ["/tmp", "/cache", "/run"]
}
```

### ⚙️ WSL Configuration Profiles (Windows)

**Fix database permission issues and optimize environments:**

```json
{
  "wslConfig": "database"  // Fixes Plan9 filesystem issues
}
```

**Built-in Profiles:**
- **database** - Fixes PostgreSQL/MySQL/MongoDB permissions
- **docker** - Enables systemd for Docker daemon
- **web-server** - Optimized for nginx/Apache
- **systemd** - Basic systemd support
- **minimal** - Maximum isolation
- **development** - Balanced for general work

**Custom Profiles:**
```bash
thresh wslconf list
thresh wslconf show database
thresh wslconf validate my-profile.wslconf
```

### 🔄 Lifecycle Management

**Start/Stop Commands:**
```bash
# Stop environment without losing data
thresh stop postgres-server

# Start again - all volumes and config preserved
thresh start postgres-server
```

- Graceful shutdown and restart
- Preserves all volumes and configuration
- Metadata tracking for environment restoration

### 📊 Volume Management

**Complete volume lifecycle:**
```bash
thresh volume list                  # List all volumes
thresh volume create app-data       # Pre-create volume
thresh volume inspect postgres-data # View details
thresh volume delete old-data       # Remove unused
```

### 📝 Complete v1.5.0 Example

```json
{
  "name": "fullstack-app",
  "description": "Full-stack application with database",
  "base": "ubuntu-22.04",
  "packages": ["nodejs", "npm", "postgresql-14"],
  "ports": ["3000:3000", "8080:8080", "5432:5432"],
  "volumes": [
    {"name": "postgres-data", "mount": "/var/lib/postgresql/data"},
    {"name": "app-logs", "mount": "/var/log/app"}
  ],
  "bind_mounts": [
    {"host": "C:/projects/myapp", "container": "/app"}
  ],
  "tmpfs": ["/tmp"],
  "network": "bridge",
  "hostname": "app.local",
  "wslConfig": "database",
  "environment": {
    "NODE_ENV": "development",
    "DATABASE_URL": "postgresql://localhost:5432/myapp"
  }
}
```

**Access your application:**
- Frontend: http://localhost:3000
- API: http://localhost:8080
- Database: localhost:5432
- All data persists in named volumes
- Live code editing via bind mount

---

## Contributing

Contributions welcome! Please read our [Contributing Guidelines](.github/CONTRIBUTING.md).

### Development Setup

```bash
# Clone repository
git clone https://github.com/dealer426/thresh.git
cd thresh

# Install .NET 10 SDK
# https://dotnet.microsoft.com/download

# Build project
cd thresh/Thresh
dotnet build

# Run tests
dotnet test

# Run development version
dotnet run -- --version
```

---

## Support

- **Issues**: [GitHub Issues](https://github.com/dealer426/thresh/issues)
- **Discussions**: [GitHub Discussions](https://github.com/dealer426/thresh/discussions)
- **Documentation**: [https://dealer426.github.io/thresh/](https://dealer426.github.io/thresh/)

---

## License

MIT License - see [LICENSE](LICENSE) for details.

---

## Acknowledgments

- **GitHub Copilot SDK** - AI-powered blueprint generation
- **.NET Team** - Native AOT compilation support
- **Docusaurus Team** - Documentation framework
- **Community Contributors** - Testing and feedback

---

**Built with .NET 10 Native AOT** | **Cross-Platform** | **AI-Powered** | **Zero Dependencies**
