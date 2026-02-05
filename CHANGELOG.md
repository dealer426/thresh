# Changelog

All notable changes to thresh will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2026-02-05

### Added
- Software Bill of Materials (SBOM) in SPDX 2.2 format
- Automated SBOM generation in CI/CD workflows
- Supply chain transparency with 33 documented dependencies

### Changed
- Repository renamed from `eknova` to `thresh`
- All URLs and references updated to reflect new repository name

### Improved
- Package manager manifests (winget, chocolatey, scoop) ready for submission
- GitHub Actions workflows now include SBOM in releases

## [1.0.0] - 2026-02-05

### Added - Initial Release
- **Core Features**
  - Native AOT compilation (12 MB binary, zero dependencies)
  - WSL2 integration for environment provisioning
  - Blueprint-based environment configuration (YAML)
  - 12 built-in Linux distributions
    - Vendor sources: Ubuntu 20.04/22.04/24.04, Alpine 3.18/3.19/edge, Debian 11/12, Fedora 41, Rocky 9
    - Microsoft Store: Kali, Oracle 8/9, openSUSE Leap/Tumbleweed
  
- **AI Features**
  - OpenAI GPT-4o-mini integration
  - Blueprint generation from natural language (`thresh generate`)
  - Interactive AI chat mode (`thresh chat`)
  - Streaming responses for real-time feedback

- **Distribution Management**
  - Hybrid distribution system (Vendor + MS Store wrapper)
  - Custom distribution support
  - AI-powered distribution discovery
  - Manual distribution configuration
  - Distribution metadata tracking

- **Commands**
  - `thresh up <blueprint>` - Provision WSL environment
  - `thresh list [--all]` - List managed environments
  - `thresh destroy <name>` - Remove environment
  - `thresh generate <prompt>` - Generate blueprint with AI
  - `thresh chat` - Interactive AI chat
  - `thresh distros` - List all available distributions
  - `thresh distro add` - Add custom distribution
  - `thresh distro list` - List custom distributions
  - `thresh distro remove` - Remove custom distribution
  - `thresh blueprints` - List available blueprints
  - `thresh config` - Configuration management
  - `thresh --version` - Show version info

- **Configuration**
  - Secure API key storage
  - Custom distribution registry
  - JSON-based configuration (~/.thresh/config.json)
  - Environment metadata tracking

- **Built-in Blueprints**
  - alpine-minimal - Minimal Alpine Linux
  - ubuntu-dev - Ubuntu development environment
  - python-dev - Python development setup
  - node-dev - Node.js development setup
  - debian-stable - Stable Debian environment
  - azure-cli - Azure CLI environment
  - alpine-python - Alpine with Python
  - ubuntu-python - Ubuntu with Python

- **Documentation**
  - Comprehensive README.md
  - Getting Started guide
  - CLI Consolidation Plan (all 8 phases complete)
  - Technical documentation in thresh/README.md

### Technical Details
- **Architecture**: .NET 9 Native AOT
- **Binary Size**: 12 MB (Windows x64)
- **Startup Time**: ~50ms
- **Memory Usage**: ~30MB idle
- **Dependencies**: None (self-contained)
- **Platform**: Windows 11 with WSL2

### Performance
- Binary: 12 MB (vs 25 MB Quarkus, 60% smaller)
- Provision time: 15-25s depending on distribution
- First-class Windows support with Native AOT

---

## Release History

- **v1.0.1** (2026-02-05) - Add SBOM and repository rename
- **v1.0.0** (2026-02-05) - Initial release with full feature set
