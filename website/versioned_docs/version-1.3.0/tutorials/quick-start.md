---
sidebar_position: 1
title: Quick Start (5 Minutes)
description: Get your first thresh environment running in 5 minutes
---

# Quick Start: Your First Environment in 5 Minutes

Get up and running with thresh in just a few minutes. This guide will take you from installation to your first working development environment.

## Prerequisites

**Windows:**
- Windows 10/11 with WSL 2 enabled
- 4 GB free disk space

**Linux:**
- Docker or Podman installed
- 4 GB free disk space

**macOS:**
- containerd installed
- 4 GB free disk space

:::tip
If you don't have WSL 2 (Windows) or Docker (Linux/macOS) installed, see our [Installation Guide](/docs/installation) for detailed setup instructions.
:::

## Step 1: Install thresh (2 minutes)

Choose your platform:

<tabs groupId="operating-systems">
<tabItem value="windows" label="Windows">

```powershell
# Using Winget (recommended)
winget install dealer426.thresh

# Verify installation
thresh --version
```

</tabItem>
<tabItem value="macos" label="macOS">

```bash
# Using Homebrew
brew install thresh

# Verify installation
thresh --version
```

</tabItem>
<tabItem value="linux" label="Linux">

```bash
# Download and install
curl -fsSL https://thresh.sh/install.sh | bash

# Verify installation
thresh --version
```

</tabItem>
</tabs>

**Expected output:**
```
thresh 1.3.0
```

## Step 2: Choose a Blueprint (30 seconds)

List available pre-configured environments:

```powershell
thresh blueprints
```

**Output:**
```
Available Blueprints:

General Development:
  ubuntu-dev      - Ubuntu 22.04 with common dev tools
  debian-stable   - Debian 12 minimal setup
  alpine-minimal  - Lightweight Alpine Linux

Language-Specific:
  python-dev      - Python 3.11 + pip + venv
  node-dev        - Node.js 20 + npm + yarn
  alpine-python   - Alpine with Python 3

Cloud/DevOps:
  azure-cli       - Azure CLI tools
```

:::info What's a Blueprint?
A blueprint is a pre-configured environment template. It specifies the Linux distribution, packages, and setup scripts. Think of it as a recipe for creating consistent development environments.
:::

## Step 3: Provision Your Environment (2 minutes)

Let's create a Python development environment:

```powershell
thresh up python-dev
```

**What happens:**
1. Downloads Alpine Linux image (~15 MB)
2. Creates WSL/container instance
3. Installs Python 3.11, pip, and development tools
4. Configures environment

**Output:**
```
[INFO] Provisioning environment: python-dev
[INFO] Using blueprint: python-dev
[INFO] Distribution: alpine:3.19

Downloading alpine-3.19.tar.gz...
████████████████████████████████ 15 MB / 15 MB [100%]

Setting up environment...
Installing packages: python3 py3-pip py3-virtualenv git
Running post-install scripts...

✓ Environment 'python-dev' is ready!

Enter with: thresh shell python-dev
```

## Step 4: Enter Your Environment (10 seconds)

Access your new environment:

```powershell
# Enter the environment shell
wsl -d thresh-python-dev

# Or use thresh shell command
thresh shell python-dev
```

You're now inside an isolated Alpine Linux environment!

## Step 5: Verify Everything Works (30 seconds)

Test the environment:

```bash
# Check Python version
python3 --version
# Output: Python 3.11.8

# Check pip
pip --version
# Output: pip 24.0 from /usr/lib/python3.11/site-packages/pip (python 3.11)

# Create a simple test
echo 'print("Hello from thresh!")' > test.py
python3 test.py
# Output: Hello from thresh!

# Check git
git --version
# Output: git version 2.43.0
```

## Step 6: Exit and Manage (30 seconds)

```bash
# Exit the environment
exit
```

Back in your host terminal:

```powershell
# List all environments
thresh list
# Output:
# Available environments:
# 
# python-dev (running)
#   Distribution: alpine:3.19
#   Status: Running
#   Uptime: 2m 15s
```

## What You Just Did

In 5 minutes, you:
- ✅ Installed thresh
- ✅ Listed available blueprints
- ✅ Provisioned a Python environment
- ✅ Verified Python, pip, and git work
- ✅ Learned basic environment management

### Your Workflow Visualized

```mermaid
flowchart LR
    A[Install thresh] --> B[Choose Blueprint]
    B --> C[thresh up python-dev]
    C --> D[Download Alpine]
    D --> E[Install Packages]
    E --> F[Configure Environment]
    F --> G[Environment Ready]
    G --> H[wsl -d thresh-python-dev]
    
    style A fill:#4CAF50,stroke:#2E7D32,color:#fff
    style C fill:#2196F3,stroke:#1565C0,color:#fff
    style G fill:#FF9800,stroke:#E65100,color:#fff
    style H fill:#9C27B0,stroke:#6A1B9A,color:#fff
```

## Next Steps

### Try Other Environments

```powershell
# Node.js development
thresh up node-dev

# Azure CLI tools
thresh up azure-cli

# Minimal Ubuntu
thresh up ubuntu-dev
```

### Multiple Environments

You can run multiple environments simultaneously:

```powershell
thresh up python-dev
thresh up node-dev
thresh list
```

**Output:**
```
Available environments:

python-dev (running)
  Distribution: alpine:3.19
  
node-dev (running)
  Distribution: alpine:3.19
```

### Use with VS Code

Open your environment in VS Code:

```powershell
# Enter environment
wsl -d thresh-python-dev

# Inside environment
code .
```

VS Code will automatically connect to the WSL environment.

### Install Additional Packages

Inside the environment:

```bash
# Alpine (apk)
sudo apk add vim curl htop

# Ubuntu/Debian (apt)
sudo apt install vim curl htop
```

## Common Tasks

### Stop an Environment

```powershell
# Stop (preserves state)
wsl --terminate thresh-python-dev
```

### Restart an Environment

```powershell
# Just enter it again
wsl -d thresh-python-dev
```

### Remove an Environment

```powershell
# Destroy completely
thresh destroy python-dev
```

:::warning
`thresh destroy` permanently deletes the environment. Any files inside will be lost unless they're in a mounted directory.
:::

## Troubleshooting

### "Command not found: thresh"

**Windows:**
```powershell
# Restart terminal after installation
# Or add to PATH manually
```

**Linux/macOS:**
```bash
# Reload shell
source ~/.bashrc  # or ~/.zshrc
```

### "WSL 2 not found" (Windows)

Install WSL 2:
```powershell
wsl --install
# Restart computer
```

### Environment won't start

```powershell
# Check container runtime
wsl --status  # Windows
docker ps     # Linux/macOS

# View logs
thresh list --verbose
```

### Slow download speeds

The first time you use a distribution, thresh downloads the base image. Subsequent environments using the same distribution are instant (cached).

## Resource Usage

A minimal Alpine-based environment uses:
- **Disk:** ~150 MB
- **Memory:** ~50 MB idle
- **Startup:** <5 seconds

## Real-World Example

Let's build a simple Flask app:

```powershell
# Create environment
thresh up python-dev

# Enter environment
wsl -d thresh-python-dev
```

Inside the environment:

```bash
# Install Flask
pip install flask

# Create app
cat > app.py << 'EOF'
from flask import Flask
app = Flask(__name__)

@app.route('/')
def hello():
    return 'Hello from thresh!'

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
EOF

# Run app
python app.py
```

**Output:**
```
 * Running on http://0.0.0.0:5000
```

Access from Windows:
```powershell
# In Windows browser or terminal
curl http://localhost:5000
# Output: Hello from thresh!
```

## What Makes thresh Different?

| Feature | thresh | Traditional VMs | Docker Desktop |
|---------|--------|-----------------|----------------|
| **Startup** | <5 seconds | 30-60 seconds | 10-20 seconds |
| **Memory** | ~50 MB idle | 2-4 GB | 1-2 GB |
| **Disk** | 150 MB | 10-20 GB | 2-5 GB |
| **AI Integration** | Built-in (MCP) | Manual | Manual |
| **Blueprints** | Pre-configured | DIY | Dockerfiles |

## Learn More

**Tutorials:**
- [Creating Custom Blueprints](/docs/tutorials/custom-blueprints) - Build your own templates
- [VS Code MCP Integration](/docs/tutorials/vscode-mcp) - AI-powered environment management
- [Cross-Platform Development](/docs/tutorials/cross-platform) - Windows, Linux, macOS tips

**CLI Reference:**
- [`thresh up`](/docs/cli-reference/up) - Provision environments
- [`thresh list`](/docs/cli-reference/list) - Manage environments
- [`thresh blueprints`](/docs/cli-reference/blueprints) - Explore blueprints

**Getting Help:**
- [GitHub Issues](https://github.com/dealer426/thresh/issues) - Bug reports and feature requests
- [Discussions](https://github.com/dealer426/thresh/discussions) - Community Q&A

---

**Congratulations!** 🎉 You've completed the thresh quick start. You now have the skills to provision, manage, and use isolated development environments.

Ready to customize? Check out [Creating Custom Blueprints](/docs/tutorials/custom-blueprints) next.
