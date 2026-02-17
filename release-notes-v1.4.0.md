# thresh v1.4.0 - Multi-Platform Release 🎉

**Cross-platform development environment manager with AI-powered blueprint generation**

---

## 🌟 Major Highlights

### ✨ Multi-Platform Support
- **Windows** - Full WSL2 integration with nerdctl/containerd
- **Linux** - Docker, nerdctl, and containerd support
- **macOS** - Native containerd support for Apple Silicon (M1/M2/M3)

### 🤖 Enhanced MCP Server
Extended from **6 to 11 MCP tools** for VS Code, Cursor, and Windsurf integration:
- `create_environment` - Parallel environment creation (10x faster)
- `destroy_environment` - Environment cleanup
- `list_environments` - View all environments
- `generate_blueprint` - AI-powered blueprint generation
- `delete_blueprint` - Remove generated blueprints
- `save_blueprint` - Save custom blueprints
- `list_blueprints` - View all blueprints
- `get_blueprint` - Retrieve blueprint details
- `get_version` - Query thresh version
- `get_metrics` - System metrics and monitoring
- `help` - MCP tool documentation

### 📦 Improved Blueprint Management
- Grouped command structure: `thresh blueprint <command>`
- AI blueprints auto-save to bundled directory
- Blueprint metadata tracking with enhanced caching
- Delete command: `thresh blueprint delete <name>`

---

## 🚀 What's New in v1.4.0

### Added Features

#### Platform-Aware AI
- Platform-specific blueprint generation (Docker vs WSL vs containerd)
- Docker Hub image support for Linux environments
- Platform-aware CLI help text
- Platform-specific access instructions

#### Documentation & Guides
- Separate getting started guides for each platform
- Platform-specific installation instructions
- Comprehensive Docusaurus documentation site: https://thresh.sh
- Versioned docs (v1.2.0, v1.3.0, v1.4.0)

#### System Metrics
- IP address display
- Load average monitoring
- Docker/containerd storage information
- JSON export: `thresh metrics --format json`

#### Build & CI/CD
- GitHub Actions multi-platform builds
- Cross-platform CI/CD testing
- Automated release workflows
- SBOM generation for all platforms

### Changed

- **Command Structure**: Migration to grouped blueprint commands
  - `thresh blueprints` → `thresh blueprint list`
  - `thresh generate` → `thresh blueprint generate`
- **UPX Compression**: Updated to v5.1.0 for Linux/Windows
- **macOS Binaries**: Uncompressed (~13MB) to preserve code signing
- **Package Timeout**: Increased from 30s to 300s for reliability
- **License**: Standardized on MIT License

### Fixed

- macOS CI/CD builds (UPX via Homebrew, code signing preserved)
- MCP JSON-RPC protocol compliance (request IDs)
- Blueprint path resolution (cross-platform)
- Container startup sequencing
- Python PEP 668 compliance
- Version consistency across all files

---

## 📥 Installation

### Windows (WSL2 Required)

```powershell
# Download and extract
Invoke-WebRequest -Uri "https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-windows-x64.zip" -OutFile thresh.zip
Expand-Archive thresh.zip
cd thresh
.\thresh.exe --version
```

### Linux

```bash
# Download and extract
curl -LO https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-linux-x64.tar.gz
tar -xzf thresh-linux-x64.tar.gz
chmod +x thresh
sudo mv thresh /usr/local/bin/
thresh --version
```

### macOS (Apple Silicon)

```bash
# Download and extract
curl -LO https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-macos-arm64.tar.gz
tar -xzf thresh-macos-arm64.tar.gz
chmod +x thresh
sudo mv thresh /usr/local/bin/
thresh --version
```

---

## 🎯 Quick Start

```bash
# Create an Alpine environment
thresh up alpine-minimal

# List environments
thresh list

# Generate custom blueprint with AI
thresh blueprint generate "python dev with pandas and jupyter"

# View system metrics
thresh metrics

# Start MCP server for IDE integration
thresh serve --stdio
```

---

## 📦 Release Assets

| Platform | File | Size | Description |
|----------|------|------|-------------|
| **Windows x64** | `thresh-windows-x64.zip` | ~4.2 MB | UPX compressed, Native AOT |
| **Linux x64** | `thresh-linux-x64.tar.gz` | ~4.8 MB | UPX compressed, Native AOT |
| **macOS ARM64** | `thresh-macos-arm64.tar.gz` | ~5.6 MB | Uncompressed, code-signed |

### SBOM (Software Bill of Materials)
- `sbom-win-x64.json` - Windows dependencies
- `sbom-linux-x64.json` - Linux dependencies
- `sbom-osx-arm64.json` - macOS dependencies

---

## 🔧 Requirements

### All Platforms
- **GitHub CLI** (for AI features): `gh auth login`

### Platform-Specific
- **Windows**: WSL2 enabled
- **Linux**: Docker, nerdctl, or containerd
- **macOS**: containerd or Docker Desktop (Apple Silicon only)

---

## 🔗 Links

- **Documentation**: https://thresh.sh
- **Repository**: https://github.com/dealer426/thresh
- **Changelog**: [CHANGELOG.md](https://github.com/dealer426/thresh/blob/main/CHANGELOG.md)
- **Roadmap**: [ROADMAP_2026.md](https://github.com/dealer426/thresh/blob/main/docs/ROADMAP_2026.md)

---

## ⚠️ Breaking Changes

**Command structure has been refactored:**
- Old: `thresh blueprints` → New: `thresh blueprint list`
- Old: `thresh generate <prompt>` → New: `thresh blueprint generate <prompt>`

System.CommandLine provides automatic command suggestions for deprecated commands.

---

## 🙏 Notes

- macOS Intel (x64) builds currently unavailable due to GitHub Actions runner limitations
- Apple Silicon Macs (M1/M2/M3) should use the ARM64 build
- First-time AI usage requires GitHub CLI authentication: `gh auth login`

---

**Full Details**: See [CHANGELOG.md](https://github.com/dealer426/thresh/blob/main/CHANGELOG.md) for complete list of changes.
