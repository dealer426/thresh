# thresh - AI-Powered WSL Environment Manager

**Single-binary CLI for provisioning WSL development environments with AI**

![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Native AOT](https://img.shields.io/badge/Native%20AOT-14MB-green.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

---

## Overview

`thresh` is a **.NET 9 Native AOT** command-line tool that provisions WSL2 environments using AI-generated blueprints. It replaces the legacy Quarkus-based `thresh-cli` with a unified, dependency-free solution.

**Key Features**:
- 🚀 **14 MB native binary** - No .NET runtime required
- 🤖 **AI blueprint generation** - OpenAI GPT-4o-mini integration
- 📦 **12 built-in distros** - Ubuntu, Alpine, Debian, Kali, Oracle, openSUSE
- 🔧 **Custom distro support** - Add any Linux with AI discovery
- 💬 **Interactive AI chat** - Streaming responses for blueprint help
- 🎯 **Hybrid distribution system** - Vendor downloads + MS Store wrapper

---

## Quick Start

### Installation

```powershell
# Build from source
cd Thresh
dotnet publish -c Release -r win-x64 --self-contained

# Binary location
.\bin\Release\net9.0\win-x64\publish\thresh.exe

# Copy to PATH (optional)
copy .\bin\Release\net9.0\win-x64\publish\thresh.exe C:\Windows\System32\
```

### Configuration

```powershell
# Set OpenAI API key
thresh config set openai-api-key sk-...

# Verify
thresh --version
```

---

## Commands

### Environment Management

```powershell
# List available blueprints
thresh blueprints

# Provision environment
thresh up alpine-minimal

# List environments
thresh list [--all]

# Destroy environment
thresh destroy alpine-minimal
```

### AI Features

```powershell
# Generate blueprint from prompt
thresh generate "Python ML environment with Jupyter"

# Interactive AI chat
thresh chat
```

### Distribution Management

```powershell
# List all available distributions
thresh distros

# Add custom distro (AI discovery)
thresh distro add rocky --ai

# Add custom distro (manual)
thresh distro add arch --url https://... --version rolling --package-manager pacman

# List custom distros
thresh distro list

# Remove custom distro
thresh distro remove rocky
```

### Configuration

```powershell
# Set configuration value
thresh config set <key> <value>

# Get configuration value
thresh config get <key>

# Show configuration status
thresh config status

# Reset configuration
thresh config reset
```

---

## Architecture

### Project Structure

```
Thresh/
├── Program.cs                   # CLI entry point (757 lines)
│   └── Command definitions using System.CommandLine
│
├── Services/
│   ├── WslService.cs            # WSL integration (259 lines)
│   │   ├── Distribution listing
│   │   ├── Import/export operations
│   │   └── Command execution
│   │
│   ├── BlueprintService.cs      # Provisioning (476 lines)
│   │   ├── InstallBaseDistributionAsync (Vendor vs MS Store)
│   │   ├── InstallPackagesAsync (apt/apk/dnf/zypper)
│   │   ├── ExecuteScriptsAsync
│   │   └── SetEnvironmentVariablesAsync
│   │
│   ├── RootfsRegistry.cs        # Distribution catalog (257 lines)
│   │   ├── Vendor distributions (10)
│   │   ├── MS Store distributions (5)
│   │   └── Custom distributions (unlimited)
│   │
│   └── ConfigurationService.cs  # Settings (144 lines)
│       ├── OpenAI API key management
│       ├── Custom distro registry
│       └── JSON persistence (~/.thresh/config.json)
│
├── Models/
│   ├── Blueprint.cs             # YAML blueprint model
│   ├── EnvironmentMetadata.cs   # Environment tracking
│   ├── DistributionInfo.cs      # Distro metadata
│   └── UserConfiguration.cs     # User settings
│
├── Utilities/
│   └── ProcessHelper.cs         # Process execution wrapper
│
├── Mcp/
│   └── McpServer.cs             # Model Context Protocol server
│
└── blueprints/                  # Built-in blueprints
    ├── alpine-minimal.yaml
    ├── ubuntu-dev.yaml
    ├── python-dev.yaml
    ├── node-dev.yaml
    └── debian-stable.yaml
```

### Distribution Sources

**Vendor (Direct Downloads)**:
- Ubuntu 20.04, 22.04, 24.04
- Alpine 3.18, 3.19, edge
- Debian 11, 12
- Fedora 41
- Rocky Linux 9

**Microsoft Store (wsl --install wrapper)**:
- Kali Linux
- Oracle Linux 8, 9
- openSUSE Leap, Tumbleweed

**Custom (User-Added)**:
- AI discovery mode
- Manual URL specification
- Stored in ~/.thresh/config.json

### Provisioning Workflow

```
1. Parse Blueprint (YAML → Blueprint object)
   ├─→ Validate base distribution
   ├─→ Validate packages list
   └─→ Validate scripts syntax

2. Install Base Distribution
   ├─→ Vendor Source:
   │   ├─→ Download rootfs tar.gz
   │   ├─→ Cache: ~/.thresh/rootfs-cache/
   │   └─→ wsl --import
   │
   └─→ MS Store Source:
       ├─→ wsl --install <distro> --no-launch
       ├─→ wsl --export <distro> temp.tar
       ├─→ wsl --import <custom-name> <path> temp.tar
       └─→ wsl --unregister <distro>

3. Install Packages
   ├─→ Detect package manager (apt/apk/dnf/zypper)
   ├─→ Update package cache
   └─→ Install packages list

4. Execute Scripts
   ├─→ setup script (pre-install)
   └─→ postInstall script (post-install)

5. Set Environment Variables
   ├─→ Write to /etc/environment
   └─→ Source in shell profile

6. Save Metadata
   └─→ ~/.thresh/metadata/{env-name}.json
```

---

## Blueprint Format

### YAML Schema

```yaml
name: string              # Environment name (required)
description: string       # Description (optional)
base: string             # Base distribution key (required)
                         # e.g., ubuntu-22.04, alpine-3.19, kali

packages:                # List of packages to install
  - package1
  - package2

environment:             # Environment variables (key-value)
  VAR_NAME: value

scripts:
  setup: |               # Pre-install script (optional)
    echo "Setup script"
  
  postInstall: |         # Post-install script (optional)
    echo "Post-install script"
```

### Example: Python ML Environment

```yaml
name: python-ml
description: Python machine learning environment with Jupyter
base: ubuntu-22.04

packages:
  - python3
  - python3-pip
  - python3-venv
  - build-essential
  - git
  - curl

environment:
  PYTHONUNBUFFERED: "1"
  JUPYTER_ENABLE_LAB: "yes"

scripts:
  setup: |
    # Upgrade pip
    pip3 install --upgrade pip
  
  postInstall: |
    # Install ML packages
    pip3 install jupyter pandas numpy scipy scikit-learn matplotlib seaborn
    
    # Create workspace
    mkdir -p ~/notebooks
    
    # Print success message
    echo "✅ Python ML environment ready!"
    echo "Run: jupyter lab --ip=0.0.0.0"
```

---

## Configuration

### User Configuration File

**Location**: `~/.thresh/config.json`

```json
{
  "openAiApiKey": "sk-...",
  "openAiModel": "gpt-4o-mini",
  "customDistributions": {
    "rocky-9": {
      "name": "Rocky Linux",
      "version": "9",
      "packageManager": "dnf",
      "rootfsUrl": "https://dl.rockylinux.org/.../Rocky-9-Container-Base.latest.x86_64.tar.xz",
      "description": "Rocky Linux 9 (RHEL compatible)"
    }
  }
}
```

### Environment Metadata

**Location**: `~/.thresh/metadata/{environment-name}.json`

```json
{
  "environmentName": "alpine-minimal",
  "blueprintName": "alpine-minimal",
  "created": "2026-02-05T18:30:00Z",
  "base": "alpine-3.19",
  "description": "Minimal Alpine Linux environment",
  "distributionSource": "Vendor"
}
```

---

## Build & Development

### Prerequisites

- .NET 9.0 SDK
- Windows 11 with WSL2

### Development Build (JIT)

```powershell
# Fast iteration build
dotnet build

# Run in development
dotnet run -- --version
dotnet run -- up alpine-minimal
```

### Release Build (Native AOT)

```powershell
# Full Native AOT compilation
dotnet publish -c Release -r win-x64 --self-contained

# Output
bin\Release\net9.0\win-x64\publish\thresh.exe  # 14 MB

# Verify no .NET runtime dependency
# Binary runs on systems without .NET installed
```

### Testing

```powershell
# List built-in distributions
thresh distros

# Generate test blueprint
thresh generate "minimal Ubuntu environment"

# Provision test environment
thresh up alpine-minimal

# Verify environment
wsl -l -v
thresh list

# Cleanup
thresh destroy alpine-minimal
```

---

## Native AOT Configuration

### Project Settings

**Thresh.csproj**:
```xml
<PropertyGroup>
  <!-- Native AOT Enabled -->
  <PublishAot>true</PublishAot>
  <SelfContained>true</SelfContained>
  
  <!-- Size Optimization -->
  <IlcOptimizationPreference>Size</IlcOptimizationPreference>
  <IlcGenerateStackTraceData>false</IlcGenerateStackTraceData>
  <InvariantGlobalization>false</InvariantGlobalization>
  <TrimMode>full</TrimMode>
</PropertyGroup>
```

### Dependencies

```xml
<ItemGroup>
  <!-- CLI Framework -->
  <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
  
  <!-- AI Integration -->
  <PackageReference Include="OpenAI" Version="2.1.0" />
  
  <!-- YAML Parsing -->
  <PackageReference Include="YamlDotNet" Version="16.2.1" />
  
  <!-- Configuration -->
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
</ItemGroup>
```

### Build Results

```
Binary Size:     14 MB
Startup Time:    ~50ms
Memory (Idle):   ~30MB
Dependencies:    None (Windows system libraries only)
Warnings:        12 (Azure SDK and StreamJsonRpc trim warnings - acceptable)
```

---

## Performance

| Metric | Value |
|--------|-------|
| Binary Size | 14 MB |
| Startup Time | ~50ms |
| Memory Usage (Idle) | ~30MB |
| Provision Time (Alpine) | ~15s |
| Provision Time (Ubuntu) | ~25s |
| AI Response (First Token) | ~2s |
| Download Cache | ~/.thresh/rootfs-cache/ |

---

## Migration from Quarkus CLI

This replaces the legacy Java/Quarkus `thresh-cli` with a unified .NET solution.

### Why Migrate?

| Aspect | Quarkus CLI | thresh (.NET) |
|--------|-------------|---------------|
| Language | Java 23 | C# 13 |
| Binary Size | 25 MB | 14 MB |
| Startup Time | ~10ms | ~50ms |
| AI Integration | ❌ No (Java only) | ✅ OpenAI SDK |
| Runtime Required | ❌ None | ❌ None |
| Dependencies | GraalVM | None |
| Code Complexity | High | Medium |
| Maintenance | Dual stack | Single stack |

### Migration Complete ✅

- [x] Phase 0: Native AOT setup
- [x] Phase 1: WSL service migration
- [x] Phase 2: Blueprint service migration
- [x] Phase 3: CLI commands implementation
- [x] Phase 4: OpenAI SDK integration
- [x] Phase 5: Configuration & custom distros
- [x] Phase 6: MCP server support
- [x] Phase 7: Testing & validation
- [x] Phase 8: Documentation

---

## Troubleshooting

### Common Issues

**"WSL not found"**
```powershell
# Enable WSL2
wsl --install
```

**"OpenAI API key not configured"**
```powershell
thresh config set openai-api-key sk-...
```

**"Distribution download failed"**
```powershell
# Check internet connection
# Check cache: ~/.thresh/rootfs-cache/
# Try different mirror
```

**"Package installation failed"**
```powershell
# Provision with verbose logging
thresh up blueprint-name --verbose

# Check WSL distribution state
wsl -l -v
```

---

## License

MIT License - see [LICENSE](../LICENSE)

---

## Links

- **Repository**: [github.com/dealer426/thresh](https://github.com/dealer426/thresh)
- **Issues**: [GitHub Issues](https://github.com/dealer426/thresh/issues)
- **Documentation**: [docs/](../docs/)
- **CLI Consolidation Plan**: [CLI_CONSOLIDATION_PLAN.md](../docs/CLI_CONSOLIDATION_PLAN.md)

---

**Built with .NET 9 Native AOT** | **14 MB Binary** | **Zero Dependencies**
