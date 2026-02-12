---
id: download
title: Download & Install
sidebar_label: Download
---

# Download & Install thresh

thresh is available through multiple package managers. Choose your preferred platform below.

## Quick Install

### Windows

#### WinGet (Recommended)

```powershell
winget install dealer426.thresh
```

#### Chocolatey

```powershell
choco install thresh
```

#### Manual Download

Download the latest release from [GitHub Releases](https://github.com/dealer426/thresh/releases/latest):

1. Download `thresh-win-x64.zip`
2. Extract to `C:\Program Files\thresh\`
3. Add to PATH:
   ```powershell
   [Environment]::SetEnvironmentVariable("Path", $env:Path + ";C:\Program Files\thresh", "Machine")
   ```

### macOS

#### Homebrew (Recommended)

```bash
brew install dealer426/tap/thresh
```

#### Manual Download

Download the latest release from [GitHub Releases](https://github.com/dealer426/thresh/releases/latest):

1. Download `thresh-macos-x64.tar.gz` (Intel) or `thresh-macos-arm64.tar.gz` (Apple Silicon)
2. Extract and install:
   ```bash
   sudo tar -xzf thresh-macos-*.tar.gz -C /usr/local/bin
   sudo chmod +x /usr/local/bin/thresh
   ```

### Linux

#### APT (Debian/Ubuntu)

```bash
# Add repository
wget -qO- https://dealer426.github.io/thresh/gpg.key | sudo apt-key add -
echo "deb https://dealer426.github.io/thresh/apt stable main" | sudo tee /etc/apt/sources.list.d/thresh.list

# Install
sudo apt update
sudo apt install thresh
```

#### YUM/DNF (RHEL/Fedora/CentOS)

```bash
# Add repository
sudo tee /etc/yum.repos.d/thresh.repo <<EOF
[thresh]
name=thresh Repository
baseurl=https://dealer426.github.io/thresh/rpm
enabled=1
gpgcheck=1
gpgkey=https://dealer426.github.io/thresh/gpg.key
EOF

# Install
sudo dnf install thresh
# or
sudo yum install thresh
```

#### Snap

```bash
sudo snap install thresh --classic
```

#### Manual Download

Download the latest release from [GitHub Releases](https://github.com/dealer426/thresh/releases/latest):

```bash
# Download and install
wget https://github.com/dealer426/thresh/releases/latest/download/thresh-linux-x64.tar.gz
sudo tar -xzf thresh-linux-x64.tar.gz -C /usr/local/bin
sudo chmod +x /usr/local/bin/thresh
```

## Verify Installation

After installation, verify thresh is working:

```bash
thresh version
```

Expected output:
```
thresh version 1.3.0
Runtime: Docker (Linux) / WSL 2 (Windows) / containerd (macOS)
```

## System Requirements

### All Platforms

- **RAM:** 4 GB minimum, 8 GB recommended
- **Disk:** 2 GB for thresh + space for environments
- **Network:** Internet connection for downloading distributions

### Windows

- **OS:** Windows 10 version 2004+ (Build 19041+) or Windows 11
- **WSL:** WSL 2 required
  ```powershell
  # Install WSL 2
  wsl --install
  wsl --set-default-version 2
  ```

### macOS

- **OS:** macOS 11 (Big Sur) or later
- **Runtime:** containerd (installed automatically) or Docker Desktop
- **Architectures:** Intel (x64) and Apple Silicon (ARM64) supported

### Linux

- **OS:** Any modern distribution (Ubuntu 20.04+, Debian 11+, RHEL 8+, etc.)
- **Runtime:** Docker or Podman
  ```bash
  # Install Docker (Ubuntu/Debian)
  curl -fsSL https://get.docker.com | sh
  sudo usermod -aG docker $USER
  # Log out and back in
  
  # Or install Podman (Fedora/RHEL)
  sudo dnf install podman
  ```

## Build from Source

For developers who want the latest features:

```bash
# Clone repository
git clone https://github.com/dealer426/thresh.git
cd thresh/thresh/Thresh

# Build with .NET SDK
dotnet build -c Release

# Publish standalone binary
dotnet publish -c Release -r linux-x64 --self-contained
# Output: bin/Release/net9.0/linux-x64/publish/thresh

# Or use build script
cd ../../
python cleanup_and_build.py
```

### Prerequisites for Building

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git

## Update thresh

### Windows (WinGet)

```powershell
winget upgrade dealer426.thresh
```

### Windows (Chocolatey)

```powershell
choco upgrade thresh
```

### macOS (Homebrew)

```bash
brew upgrade thresh
```

### Linux (APT)

```bash
sudo apt update && sudo apt upgrade thresh
```

### Linux (DNF/YUM)

```bash
sudo dnf upgrade thresh
# or
sudo yum update thresh
```

## Uninstall

### Windows (WinGet)

```powershell
winget uninstall dealer426.thresh
```

### Windows (Chocolatey)

```powershell
choco uninstall thresh
```

### macOS (Homebrew)

```bash
brew uninstall thresh
```

### Linux (APT)

```bash
sudo apt remove thresh
```

### Linux (DNF/YUM)

```bash
sudo dnf remove thresh
# or
sudo yum remove thresh
```

### Manual Cleanup

Remove all thresh data:

```bash
# Remove environments (WARNING: deletes all environments)
thresh destroy $(thresh list --json | jq -r '.[].name')

# Remove configuration
rm -rf ~/.thresh

# Windows: Also remove from Program Files
# Remove-Item "C:\Program Files\thresh\" -Recurse -Force
```

## Package Manager Comparison

| Package Manager | Platform | Auto-Update | Verified | Notes |
|----------------|----------|-------------|----------|-------|
| **WinGet** | Windows | ✅ Yes | ✅ Yes | Recommended for Windows |
| **Chocolatey** | Windows | ✅ Yes | ✅ Yes | Popular alternative |
| **Homebrew** | macOS | ✅ Yes | ✅ Yes | Recommended for macOS |
| **APT** | Debian/Ubuntu | ✅ Yes | ✅ Yes | Official repository |
| **DNF/YUM** | RHEL/Fedora | ✅ Yes | ✅ Yes | Official repository |
| **Snap** | Linux | ✅ Yes | ✅ Yes | Universal Linux package |
| **Manual** | All | ❌ No | ⚠️ Verify GPG | Most control, manual updates |

## Troubleshooting

### Command Not Found

**Problem:** `thresh: command not found`

**Solution:**

```bash
# Check if installed
which thresh

# Add to PATH if needed (Linux/macOS)
export PATH="/usr/local/bin:$PATH"
echo 'export PATH="/usr/local/bin:$PATH"' >> ~/.bashrc

# Windows: Add to PATH in System Environment Variables
# Settings → System → About → Advanced system settings → Environment Variables
```

### Permission Denied

**Problem:** `permission denied` when running thresh

**Solution:**

```bash
# Make executable (Linux/macOS)
sudo chmod +x /usr/local/bin/thresh

# Or add user to docker group (Linux)
sudo usermod -aG docker $USER
# Log out and back in
```

### WSL Not Found (Windows)

**Problem:** `WSL 2 not found or not running`

**Solution:**

```powershell
# Install WSL 2
wsl --install
wsl --set-default-version 2

# Update WSL
wsl --update

# Restart computer
```

### Docker Not Found (Linux)

**Problem:** `Docker runtime not available`

**Solution:**

```bash
# Install Docker
curl -fsSL https://get.docker.com | sh

# Start Docker service
sudo systemctl start docker
sudo systemctl enable docker

# Add user to docker group
sudo usermod -aG docker $USER
# Log out and back in
```

## Next Steps

After installation:

1. **[Quick Start Tutorial](/docs/tutorials/quick-start)** - Get up and running in 5 minutes
2. **[Create Custom Blueprints](/docs/tutorials/custom-blueprints)** - Define your environments
3. **[GitHub Copilot SDK](/docs/tutorials/copilot-sdk)** - Use natural language commands
4. **[MCP Integration](/docs/tutorials/vscode-mcp)** - Integrate with AI assistants

## Support

- **Documentation:** [thresh.sh/docs](/)
- **Issues:** [GitHub Issues](https://github.com/dealer426/thresh/issues)
- **Discussions:** [GitHub Discussions](https://github.com/dealer426/thresh/discussions)
- **Discord:** [thresh Community](https://discord.gg/thresh)

---

**Latest Version:** 1.3.0 ([Release Notes](https://github.com/dealer426/thresh/releases/latest))
