---
sidebar_position: 1
---

# Getting Started

**Provision your first development environment in under 5 minutes**

thresh is a lightweight CLI tool for orchestrating development environments across Windows (WSL), Linux, and macOS. Create isolated, reproducible environments with AI-powered blueprint generation.

## Quick Start

### Prerequisites

**Windows (WSL):**
```powershell
wsl --version
```
Expected: WSL 2.x.x or higher

**Linux/macOS:**
- Docker or containerd installed

### Installation

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

<Tabs groupId="os">
  <TabItem value="winget" label="Winget (Windows)" default>

```powershell
winget install dealer426.thresh
```

  </TabItem>
  <TabItem value="choco" label="Chocolatey">

```powershell
choco install thresh
```

  </TabItem>
  <TabItem value="scoop" label="Scoop">

```powershell
scoop install thresh
```

  </TabItem>
  <TabItem value="binary" label="Download Binary">

Download from [GitHub Releases](https://github.com/dealer426/thresh/releases/latest)

```powershell
# Windows
Invoke-WebRequest -Uri "https://github.com/dealer426/thresh/releases/latest/download/thresh.exe" -OutFile "thresh.exe"

# Linux/macOS (coming soon)
curl -LO https://github.com/dealer426/thresh/releases/latest/download/thresh
chmod +x thresh
```

  </TabItem>
</Tabs>

### Verify Installation

```bash
thresh version
```

Expected output:
```
thresh v1.2.0
Runtime: .NET 9.0
Platform: Windows (WSL)
Binary: 14 MB (Native AOT)
```

## Your First Environment

### 1. List Available Blueprints

```bash
thresh blueprints
```

You'll see 8 built-in blueprints:
- `ubuntu-dev` - General purpose Ubuntu development
- `python-dev` - Python 3.11 with pip, venv, common packages
- `node-dev` - Node.js 20 LTS with npm, yarn, pnpm
- `alpine-minimal` - Lightweight Alpine Linux base
- `debian-stable` - Debian stable base
- `azure-cli` - Azure CLI tools pre-configured
- `alpine-python` - Python on Alpine Linux
- `ubuntu-python` - Python on Ubuntu with extras

### 2. Create an Environment

```bash
thresh up python-dev
```

This provisions a Python development environment with:
- ✓ Python 3.11
- ✓ pip, venv, virtualenv
- ✓ Common packages (requests, pytest, black, flake8)
- ✓ Git pre-configured

**Output:**
```
✓ Creating environment: python-dev
✓ Using blueprint: python-dev
✓ Provisioning WSL distribution...
✓ Installing packages...
✓ Configuring environment...
✓ Environment ready!

To access: thresh exec python-dev
```

### 3. Access Your Environment

```bash
thresh exec python-dev
```

You're now in your Python environment! Try:

```bash
python --version  # Python 3.11.x
pip list          # See installed packages
```

### 4. List Environments

```bash
thresh list
```

See all your environments:
```
NAME         STATUS   DISTRO          MEMORY    DISK
python-dev   Running  Ubuntu 22.04    256 MB    2.1 GB
```

### 5. Destroy When Done

```bash
thresh destroy python-dev
```

Completely removes the environment (after confirmation).

## AI-Powered Blueprint Generation

Generate custom blueprints using AI:

```bash
thresh generate "Create a Node.js development environment with Express, TypeScript, and PostgreSQL"
```

The AI will:
1. Analyze your request
2. Generate a complete blueprint JSON
3. Save it to your blueprints directory
4. You can then use it: `thresh up my-custom-env`

## What's New in v1.2.0

🚀 **Performance Optimization**
- Native AOT compilation re-enabled
- Binary size: **75 MB → 14 MB** (81% reduction)
- Faster startup, lower memory usage

✨ **Features from v1.1.0**
- MCP (Model Context Protocol) server integration
- Cross-platform support (Windows/Linux/macOS)
- Host metrics monitoring
- 7 MCP tools for AI editor integration

See the [Changelog](https://github.com/dealer426/thresh/blob/main/CHANGELOG.md) for full details.

## Next Steps

- **CLI Reference** - All 15 commands
- **Blueprints Guide** - Create custom environments
- **AI Providers** - Configure OpenAI, Azure, or GitHub Copilot
- **MCP Integration** - Use with VS Code, Cursor, Windsurf

## Need Help?

- 📖 [Documentation](https://dealer426.github.io/thresh/)
- 🐛 [GitHub Issues](https://github.com/dealer426/thresh/issues)
- 💬 [Discussions](https://github.com/dealer426/thresh/discussions)
- 📋 [Changelog](https://github.com/dealer426/thresh/blob/main/CHANGELOG.md)
