---
sidebar_position: 5
title: Cross-Platform Development
description: Best practices for developing across Windows, Linux, and macOS with thresh
---

# Cross-Platform Development

Master cross-platform development with thresh. Learn platform-specific configurations, file system quirks, networking differences, and how to create blueprints that work everywhere.

## What You'll Learn

- Platform-specific thresh setup
- Cross-platform blueprint strategies
- File system compatibility
- Networking and port forwarding
- Path handling and line endings
- Performance optimization per platform
- Common pitfalls and solutions

## Platform Overview

| Platform | Container Runtime | Native Performance | Setup Complexity |
|----------|-------------------|-------------------|------------------|
| **Windows** | WSL 2 | Excellent | Medium |
| **Linux** | Docker/Podman | Native | Easy |
| **macOS** | containerd | Good | Medium |

## Windows-Specific Configuration

### WSL 2 Backend

thresh on Windows uses WSL 2 (Windows Subsystem for Linux):

```powershell
# Verify WSL 2 is installed
wsl --status

# List WSL distros
wsl --list --verbose

# thresh environments appear as WSL distributions
# Example: thresh-python-dev
```

### File System Access

#### Accessing Windows Files from Environment

Windows drives are mounted at `/mnt/`:

```bash
# Inside thresh environment
cd /mnt/c/Users/burns/projects
ls -la

# Read Windows files
cat /mnt/c/Users/burns/example.txt
```

#### Accessing Environment Files from Windows

WSL filesystems are accessible via `\\wsl$\`:

```powershell
# From Windows Explorer or PowerShell
cd \\wsl$\thresh-python-dev\home\user

# Copy file from environment to Windows
copy \\wsl$\thresh-python-dev\home\user\app.py C:\backup\
```

### Performance Considerations

**Fast:** Files within WSL filesystem

```bash
# Inside environment - FAST
cd ~/projects
# Files at: /home/user/projects
```

**Slow:** Files on Windows filesystem accessed from WSL

```bash
# Inside environment - SLOW
cd /mnt/c/projects
# Accessing Windows NTFS from Linux
```

:::tip
Keep source code in WSL filesystem (`~/ `) for best performance. Use `/mnt/c/` only for sharing files.
:::

### Windows-Specific Blueprint

```json
{
  "name": "windows-optimized",
  "distribution": "alpine:3.19",
  "packages": ["git", "vim"],
  "mounts": [
    {
      "host": "C:\\Users\\burns\\.ssh",
      "container": "/home/user/.ssh",
      "readOnly": true
    }
  ],
  "postInstall": [
    "# Configure Git for Windows line endings",
    "git config --global core.autocrlf input"
  ]
}
```

### WSL Configuration

Create `.wslconfig` for performance tuning:

**File:** `C:\Users\burns\.wslconfig`

```ini
[wsl2]
memory=4GB
processors=4
swap=2GB
localhostForwarding=true
```

Restart WSL:
```powershell
wsl --shutdown
wsl
```

## Linux-Specific Configuration

### Container Runtime

thresh supports Docker and Podman:

```bash
# Check Docker
docker --version
sudo systemctl status docker

# Or Podman
podman --version
```

### Rootless Containers

**Recommended:** Add user to docker group

```bash
# Add current user
sudo usermod -aG docker $USER

# Log out and back in, then verify:
docker ps  # Should work without sudo
```

### File Paths

All paths are native:

```json
{
  "mounts": [
    {
      "host": "/home/user/projects",
      "container": "/workspace"
    }
  ]
}
```

### systemd Integration

Run thresh as a service:

**File:** `~/.config/systemd/user/thresh-mcp.service`

```ini
[Unit]
Description=thresh MCP Server
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/thresh serve
Restart=on-failure

[Install]
WantedBy=default.target
```

Enable:
```bash
systemctl --user enable thresh-mcp
systemctl --user start thresh-mcp
```

### Linux-Specific Blueprint

```json
{
  "name": "linux-native",
  "distribution": "ubuntu:22.04",
  "packages": [
    "build-essential",
    "git",
    "curl"
  ],
  "postInstall": [
    "# Native Linux setup",
    "echo 'export PATH=$HOME/.local/bin:$PATH' >> ~/.bashrc"
  ]
}
```

## macOS-Specific Configuration

### containerd Backend

thresh uses containerd on macOS:

```bash
# Verify containerd
brew services list | grep containerd

# Start if needed
brew services start containerd
```

### File System

macOS uses APFS with case-insensitive default:

```json
{
  "mounts": [
    {
      "host": "/Users/burns/projects",
      "container": "/workspace"
    }
  ]
}
```

### Apple Silicon (M1/M2/M3)

Use ARM64 distributions:

```json
{
  "distribution": "alpine:3.19",
  "architecture": "arm64",
  "packages": ["python3", "py3-pip"]
}
```

thresh auto-detects architecture, but you can override if needed.

### macOS-Specific Blueprint

```json
{
  "name": "macos-m1",
  "distribution": "alpine:3.19",
  "packages": ["git", "vim"],
  "postInstall": [
    "# Configure for macOS host",
    "git config --global core.trustctime false"
  ]
}
```

## Cross-Platform Blueprints

### Strategy 1: Minimal, Universal Packages

Stick to packages available on all distributions:

```json
{
  "name": "universal-dev",
  "distribution": "alpine:3.19",
  "packages": [
    "git",
    "curl",
    "vim"
  ],
  "postInstall": [
    "# Universal setup via language package managers",
    "pip install --user black pytest",  // Python packages
    "npm install -g typescript"         // Node packages
  ]
}
```

### Strategy 2: Platform Detection

Use scripts to detect host platform:

```json
{
  "postInstall": [
    "# Detect Windows WSL",
    "if grep -qi microsoft /proc/version; then",
    "  echo 'Running on Windows WSL'",
    "  git config --global core.autocrlf input",
    "fi",
    "",
    "# Detect macOS",
    "if [ \"$(uname)\" = \"Darwin\" ]; then",
    "  echo 'Running on macOS'",
    "fi"
  ]
}
```

### Strategy 3: Distribution Abstraction

Use package managers that work everywhere:

```json
{
  "distribution": "alpine:3.19",
  "packages": ["python3", "py3-pip"],
  "postInstall": [
    "# Use pip (works on all platforms)",
    "pip install --user flask fastapi uvicorn",
    "# Use npm (works on all platforms)",
    "npm install -g typescript eslint"
  ]
}
```

## File System Compatibility

### Line Endings

**Problem:** Windows uses CRLF (`\r\n`), Linux/macOS use LF (`\n`)

**Solution:** Configure Git

```bash
# Inside environment (all platforms)
git config --global core.autocrlf input
git config --global core.eol lf
```

Or in blueprint:

```json
{
  "postInstall": [
    "git config --global core.autocrlf input",
    "git config --global core.eol lf"
  ]
}
```

### Case Sensitivity

| Platform | Default |
|----------|---------|
| **Linux** | Case-sensitive |
| **macOS** | Case-insensitive (APFS) |
| **Windows** | Case-insensitive (NTFS) |

**Best practice:** Always use consistent casing:

```bash
# ✅ Good
src/utils/helpers.js
src/Utils/Helpers.js  # Different file

# ❌ Risky (breaks on case-insensitive filesystems)
src/utils/helpers.js
src/utils/Helpers.js  # Same file on macOS/Windows
```

### Symbolic Links

**Windows:** Requires developer mode or admin privileges  
**Linux/macOS:** No restrictions

**Blueprint workaround:**

```json
{
  "postInstall": [
    "# Use copy instead of symlink for cross-platform compatibility",
    "cp -r /etc/template ~/config || ln -s /etc/template ~/config"
  ]
}
```

## Path Handling

### Universal Paths

Use forward slashes in scripts:

```bash
# ✅ Works everywhere
./scripts/build.sh
cd ~/projects/app

# ❌ Windows only
.\\scripts\\build.sh
cd %APPDATA%\\app
```

### Environment Variables

Set in blueprint for consistency:

```json
{
  "environment": {
    "PROJECT_ROOT": "/workspace",
    "CONFIG_DIR": "/workspace/config",
    "LOG_FILE": "/var/log/app.log"
  }
}
```

Use in scripts:

```bash
#!/bin/bash
cd "$PROJECT_ROOT"
cat "$CONFIG_DIR/settings.json"
```

## Networking and Ports

### Localhost Forwarding

All platforms forward `localhost` automatically:

```bash
# Inside environment
python -m http.server 8000

# Access from host browser
# http://localhost:8000
```

Works on Windows, Linux, macOS with no configuration.

### Port Conflicts

**Problem:** Port already in use on host

**Solution:** Use different ports per environment

```json
{
  "name": "frontend",
  "ports": [{"container": 3000, "host": 3000}]
}

{
  "name": "backend",
  "ports": [{"container": 3000, "host": 3001}]  # Different host port
}
```

### Firewall Configuration

**Windows:**
```powershell
# Allow WSL traffic
New-NetFirewallRule -DisplayName "WSL" -Direction Inbound -Action Allow
```

**Linux:**
```bash
# Allow Docker traffic
sudo ufw allow from 172.17.0.0/16
```

**macOS:**
```bash
# containerd usually doesn't need firewall config
```

## Architecture Differences

### Intel vs ARM

**Intel (x86_64):**
- Windows (most PCs)
- Linux (servers, older Macs)
- macOS (Intel Macs)

**ARM64:**
- macOS (M1/M2/M3)
- Linux (Raspberry Pi, AWS Graviton)

### Cross-Architecture Blueprints

```json
{
  "distribution": "alpine:3.19",
  "// Automatically selects correct architecture": "",
  "packages": ["python3", "py3-pip"]
}
```

thresh auto-detects and uses:
- `alpine:3.19-x86_64` on Intel
- `alpine:3.19-aarch64` on ARM

### Architecture-Specific Packages

```json
{
  "postInstall": [
    "# Check architecture",
    "ARCH=$(uname -m)",
    "if [ \"$ARCH\" = \"x86_64\" ]; then",
    "  # Intel-specific",
    "  echo 'Running on x86_64'",
    "elif [ \"$ARCH\" = \"aarch64\" ]; then",
    "  # ARM-specific",
    "  echo 'Running on ARM64'",
    "fi"
  ]
}
```

## Performance Optimization

### Windows (WSL 2)

**Faster:**
- Keep files in WSL filesystem (`/home/user/`)
- Use Alpine (smaller, faster startup)
- Configure `.wslconfig` (memory, CPU limits)

**Slower:**
- Accessing `/mnt/c/` (Windows <-> Linux filesystem bridge)
- Large file operations across boundaries

### Linux (Docker)

**Faster:**
- Native performance (no VM overhead)
- Use Docker volume mounts (better than bind mounts)

**Slower:**
- Bind mounts to slow disks (HDD vs SSD)

### macOS (containerd)

**Faster:**
- Use ARM64 distributions on Apple Silicon
- Allocate more resources in containerd config

**Slower:**
- Intel containers on Apple Silicon (emulation)

## Common Pitfalls

### Issue 1: Path Separators

**Problem:**
```json
{
  "mounts": [
    {"host": "C:\\Users\\burns\\projects", ...}  // Works on Windows only
  ]
}
```

**Solution:** Use environment variables

```json
{
  "postInstall": [
    "# Detect platform and set paths",
    "if grep -qi microsoft /proc/version; then",
    "  export PROJECTS=/mnt/c/Users/burns/projects",
    "else",
    "  export PROJECTS=$HOME/projects",
    "fi"
  ]
}
```

### Issue 2: Permissions

**Problem:** Files created in environment have wrong permissions

**Windows:**
```bash
# Files created in /mnt/c/ get Windows permissions
touch /mnt/c/test.txt  # May not be executable
```

**Solution:** Keep executables in WSL filesystem

```bash
# ✅ Good
~/scripts/build.sh  # Can be chmod +x

# ❌ Risky
/mnt/c/scripts/build.sh  # Permissions issues
```

### Issue 3: Line Endings

**Problem:** Scripts fail with `command not found`

```bash
./script.sh
# bash: ./script.sh: /bin/bash^M: bad interpreter
```

**Cause:** CRLF line endings from Windows

**Solution:**

```bash
# Convert to LF
dos2unix script.sh

# Or with sed
sed -i 's/\r$//' script.sh

# Or configure Git globally
git config --global core.autocrlf input
```

### Issue 4: Clock Drift (Windows/macOS)

**Problem:** Timer/date issues in containers

**Solution:**

**Windows:**
```powershell
# Restart WSL
wsl --shutdown
wsl
```

**macOS:**
```bash
# Restart containerd
brew services restart containerd
```

## Testing Cross-Platform Blueprints

### GitHub Actions

Test on all platforms automatically:

**.github/workflows/test-blueprints.yml:**

```yaml
name: Test Blueprints

on: [push]

jobs:
  test:
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
   
 runs-on: ${{ matrix.os }}
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Install thresh
        run: |
          # Platform-specific installation
          
      - name: Test blueprint
        run: |
          thresh up python-dev
          thresh list
          thresh destroy python-dev
```

### Manual Testing

```powershell
# Windows
thresh up my-blueprint
wsl -d thresh-my-blueprint
# Test...
thresh destroy my-blueprint

# Linux
thresh up my-blueprint
docker exec -it thresh-my-blueprint bash
# Test...
thresh destroy my-blueprint

# macOS
thresh up my-blueprint
# Test...
thresh destroy my-blueprint
```

## Best Practices Summary

### ✅ Do This

- Use Alpine for maximum compatibility
- Configure Git line endings in blueprint
- Keep code in container filesystem (not `/mnt/c/`)
- Use language package managers (pip, npm) over system packages
- Test on multiple platforms before sharing
- Use forward slashes in scripts

### ❌ Avoid This

- Hardcoded Windows paths (`C:\`) in blueprints
- assuming case-sensitive filesystem
- Creating symlinks in blueprints (Windows compatibility)
- Using `/mnt/` paths for performance-critical operations
- Platform-specific system packages

## Real-World Example

### Universal Python + Node Blueprint

```json
{
  "name": "fullstack-universal",
  "description": "Works on Windows, Linux, macOS",
  "distribution": "alpine:3.19",
  "packages": [
    "python3",
    "py3-pip",
    "nodejs",
    "npm",
    "git"
  ],
  "postInstall": [
    "# Configure for cross-platform",
    "git config --global core.autocrlf input",
    "git config --global core.eol lf",
    "",
    "# Install via language package managers (universal)",
    "pip install --user black pytest flask",
    "npm install -g typescript eslint prettier",
    "",
    "# Create workspace (platform-agnostic paths)",
    "mkdir -p ~/projects/{frontend,backend}",
    "",
    "# Platform detection example",
    "if grep -qi microsoft /proc/version; then",
    "  echo 'export PLATFORM=windows' >> ~/.bashrc",
    "elif [ \"$(uname)\" = \"Darwin\" ]; then",
    "  echo 'export PLATFORM=macos' >> ~/.bashrc",
    "else",
    "  echo 'export PLATFORM=linux' >> ~/.bashrc",
    "fi"
  ],
  "environment": {
    "WORKSPACE": "/home/user/projects",
    "EDITOR": "vim"
  }
}
```

## Next Steps

- **[Quick Start](/docs/tutorials/quick-start)** - Platform-specific installation
- **[Creating Custom Blueprints](/docs/tutorials/custom-blueprints)** - Blueprint development
- **[Installation Guide](/docs/installation)** - Platform setup details

---

You're now equipped to create thresh environments that work seamlessly across Windows, Linux, and macOS. Share your universal blueprints with the community!
