---
sidebar_position: 4
title: Installation
description: Complete installation guide for Windows, Linux, and macOS
---

# Installation Guide

This guide covers all installation methods for thresh across Windows, Linux, and macOS.

## Prerequisites

### Windows

**WSL 2 Required**

```powershell
# Check if WSL is installed
wsl --version
```

Expected output:
```
WSL version: 2.x.x.x
Kernel version: 5.x.x.x
```

:::info Install WSL
If WSL is not installed:
```powershell
wsl --install
```
Then restart your computer.
:::

### Linux

**Container Runtime Required**

One of the following:
- Docker Engine
- containerd
- Podman

```bash
# Check Docker
docker --version

# Or check containerd
containerd --version
```

### macOS

**Container Runtime Required**

One of the following:
- Docker Desktop for Mac
- Podman
- Colima with containerd

```bash
# Check Docker
docker --version
```

---

## Installation Methods

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

<Tabs groupId="install-method">
  <TabItem value="direct" label="Direct Download (Recommended)" default>

### Download Pre-built Binary

**Windows:**

```powershell
# Create installation directory
New-Item -ItemType Directory -Force -Path C:\thresh

# Download latest release (v1.3.0)
Invoke-WebRequest -Uri "https://github.com/dealer426/thresh/releases/latest/download/thresh.exe" -OutFile "C:\thresh\thresh.exe"

# Add to PATH for current session
$env:Path += ";C:\thresh"

# Add to PATH permanently
[Environment]::SetEnvironmentVariable("Path", $env:Path + ";C:\thresh", [EnvironmentVariableTarget]::User)

# Verify installation
thresh --version
```

**Linux:**

```bash
# Download (Linux x64)
curl -LO https://github.com/dealer426/thresh/releases/latest/download/thresh-linux-x64

# Make executable
chmod +x thresh-linux-x64

# Move to PATH
sudo mv thresh-linux-x64 /usr/local/bin/thresh

# Verify
thresh --version
```

**macOS:**

```bash
# Download (macOS ARM64)
curl -LO https://github.com/dealer426/thresh/releases/latest/download/thresh-osx-arm64

# Make executable
chmod +x thresh-osx-arm64

# Move to PATH
sudo mv thresh-osx-arm64 /usr/local/bin/thresh

# Verify
thresh --version
```

:::tip Binary Size
The thresh binary is only **3.8 MB** thanks to Native AOT + UPX compression!
:::

  </TabItem>
  <TabItem value="package-managers" label="Package Managers (Coming Soon)">

### Windows Package Managers

**Winget:**
```powershell
winget install dealer426.thresh
```

**Chocolatey:**
```powershell
choco install thresh
```

**Scoop:**
```powershell
scoop bucket add dealer426 https://github.com/dealer426/scoop-bucket
scoop install thresh
```

:::info Availability
Package manager releases are in progress. For now, use direct download method.
:::

### Linux Package Managers

**Homebrew (Linux):**
```bash
brew install thresh
```

**APT (Debian/Ubuntu):**
```bash
# Add repository
curl -fsSL https://packages.dealer426.dev/gpg | sudo gpg --dearmor -o /usr/share/keyrings/dealer426.gpg
echo "deb [signed-by=/usr/share/keyrings/dealer426.gpg] https://packages.dealer426.dev/apt stable main" | sudo tee /etc/apt/sources.list.d/dealer426.list

# Install
sudo apt update
sudo apt install thresh
```

### macOS Package Managers

**Homebrew:**
```bash
brew tap dealer426/tap
brew install thresh
```

  </TabItem>
  <TabItem value="source" label="Build from Source">

### Build from Source

**Prerequisites:**
- .NET 10 SDK
- Git

**Clone and Build:**

```bash
# Clone repository
git clone https://github.com/dealer426/thresh.git
cd thresh/thresh/Thresh

# Build for your platform
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained

# macOS ARM64
dotnet publish -c Release -r osx-arm64 --self-contained

# Binary will be in: bin/Release/net10.0/<runtime>/publish/
```

**With UPX Compression (Optional):**

```bash
# Install UPX
# Windows:
Invoke-WebRequest -Uri "https://github.com/upx/upx/releases/download/v4.2.4/upx-4.2.4-win64.zip" -OutFile upx.zip
Expand-Archive upx.zip
Move-Item upx-4.2.4-win64/upx.exe .

# Linux/macOS:
wget https://github.com/upx/upx/releases/download/v4.2.4/upx-4.2.4-amd64_linux.tar.xz
tar -xf upx-4.2.4-amd64_linux.tar.xz
sudo mv upx-4.2.4-amd64_linux/upx /usr/local/bin/

# Compress binary
upx --best --lzma bin/Release/net10.0/<runtime>/publish/thresh.exe

# Size: 13.5 MB → 3.8 MB (73% reduction)
```

**Install:**

```bash
# Windows
Copy-Item bin\Release\net10.0\win-x64\publish\thresh.exe C:\thresh\
$env:Path += ";C:\thresh"

# Linux/macOS
sudo cp bin/Release/net10.0/<runtime>/publish/thresh /usr/local/bin/
```

  </TabItem>
</Tabs>

---

## GitHub Copilot Setup

thresh uses GitHub Copilot SDK for AI features (no API keys needed!).

### 1. Install GitHub CLI

<Tabs groupId="gh-install">
  <TabItem value="windows" label="Windows" default>

```powershell
winget install GitHub.cli
```

  </TabItem>
  <TabItem value="linux" label="Linux">

```bash
# Debian/Ubuntu
sudo apt install gh

# Fedora/RHEL
sudo dnf install gh

# Arch
sudo pacman -S github-cli
```

  </TabItem>
  <TabItem value="macos" label="macOS">

```bash
brew install gh
```

  </TabItem>
</Tabs>

### 2. Authenticate

```bash
gh auth login
```

Follow the prompts to authenticate with your GitHub account.

### 3. Verify

```bash
# Check GitHub CLI
gh auth status

# Check thresh can access AI
thresh config status
```

**Expected output:**
```
thresh Configuration
═══════════════════════════════════════

GitHub Copilot: ✓ Authenticated
Default Model: gpt-4o
...
```

---

## Verification

### Test Installation

```bash
# Show version
thresh --version
```

**Expected output:**
```
thresh v1.3.0
Runtime: .NET 10.0.0
Platform: Windows/WSL (or Linux, macOS)
Binary Size: 3.8 MB
```

### List Available Commands

```bash
thresh --help
```

### List Built-in Blueprints

```bash
thresh blueprints
```

**Expected output:**
```
Available blueprints:

alpine-minimal    - Minimal Alpine Linux environment
ubuntu-dev        - Ubuntu development environment with common tools
python-dev        - Python development environment
node-dev          - Node.js development environment
...
```

### Quick Test Environment

```bash
# Create a minimal environment (fastest)
thresh up alpine-minimal

# List environments
thresh list

# Access environment
wsl -d alpine-minimal  # Windows
# or
docker exec -it alpine-minimal sh  # Linux/macOS

# Clean up
thresh destroy alpine-minimal
```

---

## Upgrading

### Upgrade to Latest Version

<Tabs groupId="upgrade-method">
  <TabItem value="direct-upgrade" label="Direct Download" default>

```powershell
# Windows - Download and replace
Invoke-WebRequest -Uri "https://github.com/dealer426/thresh/releases/latest/download/thresh.exe" -OutFile "C:\thresh\thresh.exe"

# Verify new version
thresh --version
```

```bash
# Linux/macOS - Download and replace
curl -LO https://github.com/dealer426/thresh/releases/latest/download/thresh-linux-x64
chmod +x thresh-linux-x64
sudo mv thresh-linux-x64 /usr/local/bin/thresh

# Verify
thresh --version
```

  </TabItem>
  <TabItem value="package-upgrade" label="Package Managers">

```powershell
# Windows
winget upgrade dealer426.thresh
# or
choco upgrade thresh
# or
scoop update thresh
```

```bash
# Linux
sudo apt update && sudo apt upgrade thresh
# or
brew upgrade thresh
```

  </TabItem>
</Tabs>

---

## Uninstallation

### Remove thresh

<Tabs groupId="uninstall">
  <TabItem value="windows" label="Windows" default>

```powershell
# Remove binary
Remove-Item C:\thresh\thresh.exe

# Remove from PATH (if added permanently)
# Open System Properties > Environment Variables
# Edit PATH and remove C:\thresh

# Remove configuration (optional)
Remove-Item -Recurse ~/.thresh
```

  </TabItem>
  <TabItem value="linux-mac" label="Linux/macOS">

```bash
# Remove binary
sudo rm /usr/local/bin/thresh

# Remove configuration (optional)
rm -rf ~/.thresh
```

  </TabItem>
</Tabs>

### Clean Up Environments

Before uninstalling, clean up all environments:

```bash
# List all environments
thresh list

# Destroy each environment
thresh destroy <environment-name>

# Or destroy all (PowerShell)
thresh list | ForEach-Object { 
  $name = ($_ -split '\s+')[0]
  thresh destroy $name --force
}
```

---

## Troubleshooting

### "Command not found" after installation

**Windows:**
```powershell
# Check if thresh.exe exists
Test-Path C:\thresh\thresh.exe

# Add to PATH for current session
$env:Path += ";C:\thresh"

# Verify
thresh --version
```

**Linux/macOS:**
```bash
# Check if binary exists
ls -l /usr/local/bin/thresh

# Check PATH
echo $PATH

# Make sure /usr/local/bin is in PATH
export PATH="/usr/local/bin:$PATH"
```

### "WSL not found" (Windows)

```powershell
# Install WSL
wsl --install

# Restart computer
shutdown /r /t 0
```

### "Permission denied" (Linux/macOS)

```bash
# Make binary executable
chmod +x /usr/local/bin/thresh

# Or if in current directory
chmod +x thresh
```

### Binary won't run on Linux

```bash
# Check dependencies (none should be required for self-contained build)
ldd /usr/local/bin/thresh

# If issues, rebuild from source without Native AOT
dotnet publish -c Release -r linux-x64
```

---

## Next Steps

- ✅ Installation complete
- 🎯 [Get Started](../intro) - Create your first environment
- 🎯 [CLI Reference](../cli-reference/) - Learn all commands
- 🎯 [MCP Integration](../mcp-integration) - Use with AI editors

**Happy provisioning!** 🚀
