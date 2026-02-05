# thresh - AI-Powered WSL Development Environments

> **Provision WSL environments in <30 seconds with AI-generated blueprints**

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Native AOT](https://img.shields.io/badge/Native%20AOT-16.6MB-green.svg)
![WSL](https://img.shields.io/badge/WSL-2.0-blue.svg)

---

## 🚀 What is thresh?

**thresh** is a single-binary CLI tool that uses AI to generate and provision **WSL (Windows Subsystem for Linux) development environments** instantly. Built with .NET 9 Native AOT, it delivers a **16.6MB executable with zero runtime dependencies**.

### Key Features

- 🤖 **Dual AI Providers** - Choose between OpenAI or GitHub Copilot SDK for blueprint generation
- ⚡ **Instant Provisioning** - WSL2 distributions installed and configured in seconds
- 📦 **12 Built-in Distros** - Ubuntu, Alpine, Debian, Kali, Oracle Linux, openSUSE + custom support
- 🎯 **Hybrid Distribution System** - Direct vendor downloads + Microsoft Store wrapper
- 🔧 **Zero Dependencies** - Single native binary, no .NET runtime required
- 🔐 **Custom Distros** - Add any Linux distro with AI discovery or manual configuration
- 💬 **Interactive AI Chat** - Stream responses for blueprint creation and troubleshooting

---

## 🏗️ Architecture

**Single Binary Design** - Unified .NET Native AOT executable

```
thresh.exe (16.6 MB)
├── CLI Layer (System.CommandLine)
│   ├── up <blueprint>           - Provision environment
│   ├── list [--all]             - List environments
│   ├── destroy <name>           - Remove environment
│   ├── generate <prompt>        - AI blueprint generation
│   ├── chat                     - Interactive AI mode
│   ├── config                   - Configuration management
│   ├── distro                   - Custom distro management
│   ├── distros                  - List all available distros
│   └── blueprints               - List available blueprints
│
├── Services Layer
│   ├── WslService               - WSL integration (wsl.exe wrapper)
│   ├── BlueprintService         - Environment provisioning
│   ├── RootfsRegistry           - Distribution catalog
│   ├── ConfigurationService     - Secure settings storage
│   ├── IAIService               - AI provider abstraction
│   ├── OpenAIService            - OpenAI GPT integration
│   ├── GitHubCopilotService     - GitHub Copilot SDK integration
│   └── AIServiceFactory         - Provider selection
│
└── Distribution Sources
    ├── Vendor (10)              - Direct tar.gz downloads
    │   ├── Ubuntu 20.04, 22.04, 24.04
    │   ├── Alpine 3.18, 3.19, edge
    │   ├── Debian 11, 12
    │   ├── Fedora 41
    │   └── Rocky Linux 9
    │
    ├── Microsoft Store (5)      - wsl --install wrapper
    │   ├── Kali Linux
    │   ├── Oracle Linux 8, 9
    │   └── openSUSE Leap, Tumbleweed
    │
    └── Custom (unlimited)       - User-added distros
        ├── AI discovery         - thresh distro add rocky --ai
        └── Manual config        - thresh distro add rocky --url <url>
```

**Tech Stack:**
- Language: C# 13 / .NET 9.0
- CLI Framework: System.CommandLine
- AI Providers: 
  - OpenAI SDK (GPT-4o, GPT-4o-mini, GPT-3.5)
  - GitHub Copilot SDK v0.1.22 (GPT-5, GPT-4, Claude)
- YAML: YamlDotNet
- Compilation: Native AOT (PublishAot=true)
- Binary Size: 16.6 MB
- Dependencies: None (self-contained)

---

## 🛠️ Project Structure

```
thresh/
├── thresh/                      # .NET 9 Native AOT CLI (16.6 MB)
│   ├── Thresh/
│   │   ├── Program.cs           # CLI entry point & commands
│   │   ├── Services/
│   │   │   ├── WslService.cs
│   │   │   ├── BlueprintService.cs
│   │   │   ├── RootfsRegistry.cs
│   │   │   └── ConfigurationService.cs
│   │   ├── Models/
│   │   │   ├── Blueprint.cs
│   │   │   ├── EnvironmentMetadata.cs
│   │   │   └── DistributionInfo.cs
│   │   ├── Utilities/
│   │   │   └── ProcessHelper.cs
│   │   ├── Mcp/
│   │   │   └── McpServer.cs     # MCP protocol support
│   │   └── blueprints/          # Built-in blueprints
│   │       ├── alpine-minimal.yaml
│   │       ├── ubuntu-dev.yaml
│   │       └── python-dev.yaml
│   └── README.md
│
├── thresh-cli/                  # [ARCHIVED] Legacy Quarkus CLI
├── thresh-api/                  # [FUTURE] Aspire API
├── thresh-web/                  # [FUTURE] Next.js Web UI
├── docs/
│   ├── CLI_CONSOLIDATION_PLAN.md
│   └── SESSION_SUMMARY.md
└── README.md                    # This file
```

---

## 🚦 Quick Start

### Prerequisites

- **Windows 11** with WSL2 enabled (`wsl --install`)
- **OpenAI API Key** (for AI features) - Get from [platform.openai.com](https://platform.openai.com)

### Installation

**Option 1: Download Release Binary**
```powershell
# Download from GitHub releases
Invoke-WebRequest -Uri "https://github.com/dealer426/thresh/releases/latest/download/thresh.exe" -OutFile "thresh.exe"

# Move to PATH
Move-Item thresh.exe C:\Windows\System32\
```

**Option 2: Build from Source**
```powershell
# Clone repository
git clone https://github.com/dealer426/thresh.git
cd thresh\thresh\Thresh

# Build Native AOT binary
dotnet publish -c Release -r win-x64 --self-contained

# Binary location
.\bin\Release\net9.0\win-x64\publish\thresh.exe
```

### Configuration

```powershell
# Configure AI Provider (OpenAI or GitHub Copilot SDK)

# Option 1: OpenAI (default)
thresh config set aiprovider openai
thresh config set openai-api-key sk-proj-...

# Option 2: GitHub Copilot SDK (requires GitHub Copilot CLI)
thresh config set aiprovider copilot
thresh config set github-token ghp_...  # Optional, uses logged-in user if not provided

# Set default model (optional)
thresh config set default-model gpt-4o-mini  # For OpenAI
thresh config set default-model gpt-5        # For GitHub Copilot

# View configuration
thresh config list

# Verify installation
thresh --version
```

**AI Provider Comparison:**

| Feature | OpenAI | GitHub Copilot SDK |
|---------|--------|-------------------|
| Models | GPT-4o, GPT-4o-mini, GPT-3.5 | GPT-5, GPT-4, Claude Sonnet |
| Setup | API key only | Copilot CLI + auth |
| Cost | Pay per token | Included with Copilot subscription |
| Streaming | ✅ | ✅ |
| Custom distro discovery | ✅ | ❌ |

See [docs/DUAL_AI_PROVIDERS.md](docs/DUAL_AI_PROVIDERS.md) for detailed configuration.

---

## 📖 Usage Guide

### Basic Commands

```powershell
# List all available distributions (Vendor + MS Store + Custom)
thresh distros

# List available blueprints
thresh blueprints

# Provision environment from blueprint
thresh up alpine-minimal

# List your environments
thresh list

# Destroy environment
thresh destroy alpine-minimal
```

### AI Features

```powershell
# Generate blueprint from natural language
thresh generate "Python data science environment with pandas, numpy, and matplotlib"

# Interactive AI chat for blueprint help
thresh chat
# Chat> "I need a Node.js 20 environment with TypeScript and PostgreSQL"
# Chat> "Add Redis and nginx to my previous blueprint"
```

### Custom Distributions

```powershell
# Add custom distro with AI discovery
thresh distro add rocky --ai

# Add custom distro manually
thresh distro add arch --url https://example.com/arch.tar.gz --version rolling --package-manager pacman

# List custom distros
thresh distro list

# Remove custom distro
thresh distro remove rocky
```

### Advanced Usage

```powershell
# List all environments (including stopped)
thresh list --all

# Provision with verbose logging
thresh up ubuntu-dev --verbose

# View configuration
thresh config status

# Reset configuration
thresh config reset
```

---

## 🎯 Blueprint Examples

### Alpine Minimal
```yaml
name: alpine-minimal
description: Minimal Alpine Linux environment
base: alpine-3.19
packages:
  - curl
  - git
  - vim
environment:
  EDITOR: vim
scripts:
  setup: |
    echo "Minimal Alpine setup complete"
```

### Python Development
```yaml
name: python-dev
description: Python development environment with common tools
base: ubuntu-22.04
packages:
  - python3
  - python3-pip
  - python3-venv
  - build-essential
  - git
environment:
  PYTHONUNBUFFERED: "1"
scripts:
  setup: |
    pip3 install --upgrade pip
    pip3 install virtualenv pytest black flake8
```

### Node.js Development
```yaml
name: node-dev
description: Node.js development environment
base: ubuntu-24.04
packages:
  - curl
  - git
scripts:
  setup: |
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
    apt-get install -y nodejs
    npm install -g typescript @types/node pnpm
```

---

## 📚 Development

### Building from Source

```powershell
# Prerequisites
# - .NET 9.0 SDK
# - Git

# Clone repository
git clone https://github.com/dealer426/thresh.git
cd thresh\thresh\Thresh

# Development build (JIT, fast iteration)
dotnet build
dotnet run -- --version

# Release build (Native AOT)
dotnet publish -c Release -r win-x64 --self-contained

# Output
# bin\Release\net9.0\win-x64\publish\thresh.exe (16.6 MB)
```

### Project Structure

```
Thresh/
├── Program.cs                   # CLI entry point, all commands
├── Services/
│   ├── WslService.cs            # WSL integration (259 lines)
│   ├── BlueprintService.cs      # Provisioning logic (476 lines)
│   ├── RootfsRegistry.cs        # Distribution catalog (257 lines)
│   └── ConfigurationService.cs  # Settings management (144 lines)
├── Models/
│   ├── Blueprint.cs             # Blueprint YAML model
│   ├── EnvironmentMetadata.cs   # Environment tracking
│   └── DistributionInfo.cs      # Distro metadata
├── Utilities/
│   └── ProcessHelper.cs         # Process execution
├── Mcp/
│   └── McpServer.cs             # MCP protocol server
└── blueprints/                  # Built-in blueprints
    ├── alpine-minimal.yaml
    ├── ubuntu-dev.yaml
    └── python-dev.yaml
```

### Configuration Files

**User Configuration**: `~/.thresh/config.json`
```json
{
  "openAiApiKey": "sk-...",
  "openAiModel": "gpt-4o-mini",
  "customDistributions": {
    "rocky-9": {
      "name": "Rocky Linux",
      "version": "9",
      "packageManager": "dnf",
      "rootfsUrl": "https://..."
    }
  }
}
```

**Environment Metadata**: `~/.thresh/metadata/{env-name}.json`
```json
{
  "environmentName": "alpine-minimal",
  "blueprintName": "alpine-minimal",
  "created": "2026-02-05T18:30:00Z",
  "base": "alpine-3.19",
  "distributionSource": "Vendor"
}
```

---

## 🔧 Technical Details

### Native AOT Compilation

**Build Configuration** (`Thresh.csproj`):
```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <SelfContained>true</SelfContained>
  <InvariantGlobalization>false</InvariantGlobalization>
  <IlcOptimizationPreference>Size</IlcOptimizationPreference>
  <IlcGenerateStackTraceData>false</IlcGenerateStackTraceData>
  <TrimMode>full</TrimMode>
</PropertyGroup>
```

**Results**:
- Binary Size: **16.6 MB**
- Startup Time: ~50ms
- Memory Usage: ~30MB idle
- Dependencies: **None** (Windows system libraries only)

### Distribution Sources

**Vendor Distributions** (Direct Downloads):
- Ubuntu 20.04, 22.04, 24.04 → http://cloud-images.ubuntu.com
- Alpine 3.18, 3.19, edge → https://alpinelinux.org/downloads
- Debian 11, 12 → https://github.com/debuerreotype/docker-debian-artifacts
- Fedora 41 → https://mirrors.kernel.org/fedora
- Rocky Linux 9 → https://dl.rockylinux.org

**Microsoft Store Distributions** (wsl --install wrapper):
- Kali Linux → `wsl --install Kali-Linux`
- Oracle Linux 8, 9 → `wsl --install OracleLinux_8_5`, `OracleLinux_9_1`
- openSUSE Leap, Tumbleweed → `wsl --install openSUSE-Leap-15.6`, `openSUSE-Tumbleweed`

**Custom Distributions** (User-Added):
- AI Discovery: Searches for rootfs tar.gz URLs
- Manual: Direct URL specification
- Stored in: `~/.thresh/config.json`

### Hybrid Distribution System

```
thresh up ubuntu-22.04
├─→ Check source: Vendor
├─→ Download: http://cloud-images.ubuntu.com/.../ubuntu-jammy-wsl-amd64-wsl.rootfs.tar.gz
├─→ Cache: ~/.thresh/rootfs-cache/ubuntu-22.04.tar.gz
└─→ Import: wsl --import ubuntu-22.04 C:\WSL\ubuntu-22.04 <tarball>

thresh up kali
├─→ Check source: MicrosoftStore
├─→ Install: wsl --install Kali-Linux --no-launch
├─→ Export: wsl --export Kali-Linux temp.tar
├─→ Import: wsl --import kali C:\WSL\kali temp.tar
└─→ Cleanup: wsl --unregister Kali-Linux
```

---

## 🤖 AI Integration Details

**OpenAI SDK Configuration**:
```csharp
var client = new ChatClient(
    model: "gpt-4o-mini",
    apiKey: apiKey
);

// Streaming blueprint generation
await foreach (var update in client.CompleteChatStreamingAsync(messages))
{
    Console.Write(update.ContentUpdate);
}
```

**System Prompts**:
- **Generate**: "You are a WSL blueprint expert. Generate YAML configurations with: name, description, base, packages, environment, scripts. Output only valid YAML, no markdown."
- **Chat**: "You are an AI assistant helping users create WSL development environment blueprints. Provide helpful, concise responses."

---

## 📊 Performance Benchmarks

| Metric | Value |
|--------|-------|
| Binary Size | 16.6 MB |
| Startup Time | ~50ms |
| Memory (Idle) | ~30MB |
| Provision Time (Alpine) | ~15s |
| Provision Time (Ubuntu) | ~25s |
| AI Response (streaming) | ~2s first token |

---

## 🗺️ Roadmap

### ✅ Completed (v1.0)
- [x] .NET Native AOT migration (16.6 MB binary)
- [x] WSL2 integration
- [x] Blueprint provisioning
- [x] 12 built-in distributions
- [x] Hybrid distribution system (Vendor + MS Store)
- [x] Custom distro support (AI + manual)
- [x] OpenAI integration (generate + chat)
- [x] Configuration management
- [x] MCP server support

### 🚧 In Progress (v1.1)
- [ ] Blueprint marketplace integration
- [ ] Team collaboration features
- [ ] Environment snapshots/exports
- [ ] Multi-blueprint composition

### 🔮 Future (v2.0)
- [ ] Web UI (Next.js)
- [ ] GitHub Actions integration
- [ ] Container hybrid mode
- [ ] Remote environment support
- [ ] Cloud provider templates (Azure, AWS, GCP)

---

## 🤝 Contributing

Contributions welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

**Development Setup**:
```powershell
# Fork and clone
git clone https://github.com/dealer426/thresh.git
cd thresh\thresh\Thresh

# Create feature branch
git checkout -b feature/my-feature

# Make changes, build, test
dotnet build
dotnet test

# Submit PR
git push origin feature/my-feature
```

---

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

## 🙏 Acknowledgments

- **Microsoft** - .NET 9 Native AOT, WSL2
- **OpenAI** - GPT-4o-mini API
- **Community** - Blueprint contributions and testing

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/dealer426/thresh/issues)
- **Discussions**: [GitHub Discussions](https://github.com/dealer426/thresh/discussions)
- **Documentation**: [docs/](docs/)

---

**Built with ❤️ using .NET 9 Native AOT**
cd thresh-api  
dotnet build

# Web UI
cd thresh-web
npm run build
```

### Running in Development

```bash
# CLI (JVM mode for faster iteration)
cd thresh-cli && ./gradlew quarkusDev

# API (Hot reload)
cd thresh-api && dotnet watch --project thresh-api.AppHost

# Web UI (Hot reload)
cd thresh-web && npm run dev
```

### Testing

```bash
# CLI tests
cd thresh-cli && ./gradlew test

# API tests  
cd thresh-api && dotnet test

# Web UI tests
cd thresh-web && npm test
```

---

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Areas We Need Help

- 🐛 **Bug fixes** - WSL integration edge cases
- 📝 **Documentation** - Tutorials and guides  
- 🎨 **Web UI** - Blueprint editor improvements
- 🔧 **Blueprints** - Community templates
- 🌍 **Internationalization** - Multi-language support
- 🧪 **Testing** - Integration and E2E tests

---

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

## 🙏 Acknowledgments

- **Microsoft** - .NET 9 Native AOT, WSL2, GitHub Copilot SDK
- **OpenAI** - GPT-4o and GPT-4o-mini API
- **Community** - All the amazing contributors

---

## 📬 Community

- 💬 **Discord** - [Join our community](https://discord.gg/thresh-dev)
- 🐦 **Twitter** - [@thresh_dev](https://twitter.com/thresh_dev)
- 📧 **Email** - [hello@thresh.dev](mailto:hello@thresh.dev)
- 📖 **Docs** - [docs.thresh.dev](https://docs.thresh.dev)

---

**thresh** - *Your environments, your way, instantly.* ⚡

Built with ❤️ for the Windows + WSL developer community.