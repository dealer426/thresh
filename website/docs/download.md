---
id: download
title: Download & Install
sidebar_label: Download
---

# Download & Install thresh

thresh is available through multiple package managers for Windows.

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

## Verify Installation

After installation, verify thresh is working:

```bash
thresh version
```

Expected output:
```
thresh version 1.3.0
Runtime: WSL 2
```

## System Requirements

- **RAM:** 4 GB minimum, 8 GB recommended
- **Disk:** 2 GB for thresh + space for environments
- **Network:** Internet connection for downloading distributions
- **OS:** Windows 10 version 2004+ (Build 19041+) or Windows 11
- **WSL:** WSL 2 required
  ```powershell
  # Install WSL 2
  wsl --install
  wsl --set-default-version 2
  ```
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
dotnet publish -c Release -r win-x64 --self-contained
# Output: bin/Release/net9.0/win-x64/publish/thresh.exe

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

## Uninstall

### Windows (WinGet)

```powershell
winget uninstall dealer426.thresh
```

### Windows (Chocolatey)

```powershell
choco uninstall thresh
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
| **Manual** | Windows | ❌ No | ⚠️ Verify GPG | Most control, manual updates |

## Troubleshooting

### Command Not Found

**Problem:** `thresh: command not found`

**Solution:**

```powershell
# Check if installed
where thresh

# Windows: Add to PATH in System Environment Variables
# Settings → System → About → Advanced system settings → Environment Variables
```

### Permission Denied

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
