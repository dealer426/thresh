---
sidebar_position: 2
title: Creating Custom Blueprints
description: Build your own environment templates tailored to your workflow
---

# Creating Custom Blueprints

Learn how to create custom blueprints that match your exact development workflow. Blueprints are JSON templates that define Linux distributions, packages, scripts, and environment settings.

## What You'll Learn

- Blueprint file structure and syntax
- Where to store custom blueprints
- Package management for different distributions
- Post-install scripts and environment variables
- Testing and sharing blueprints

## Prerequisites

- thresh installed ([Quick Start](/docs/tutorials/quick-start))
- Text editor (VS Code, vim, nano, etc.)
- Basic JSON knowledge

## Blueprint Anatomy

A blueprint is a JSON file with this structure:

```json
{
  "distribution": "alpine:3.19",
  "packages": [
    "python3",
    "py3-pip"
  ],
  "postInstall": [
    "pip install --upgrade pip"
  ],
  "environment": {
    "EDITOR": "vim"
  }
}
```

Let's break down each section:

| Field | Required | Description |
|-------|----------|-------------|
| `distribution` | Yes | Base Linux distribution (format: `name:version`) |
| `packages` | No | System packages to install |
| `postInstall` | No | Shell commands to run after package installation |
| `environment` | No | Environment variables to set |
| `mounts` | No | Host directories to mount |
| `ports` | No | Ports to expose |

## Your First Custom Blueprint

Let's create a Go development environment.

### Step 1: Create Blueprint Directory

```powershell
# Windows
mkdir $env:USERPROFILE\.thresh\blueprints

# Linux/macOS
mkdir -p ~/.thresh/blueprints
```

### Step 2: Create Blueprint File

Create `go-dev.json`:

```powershell
# Windows
notepad $env:USERPROFILE\.thresh\blueprints\go-dev.json

# Linux/macOS
nano ~/.thresh/blueprints/go-dev.json
```

### Step 3: Define Blueprint

```json
{
  "name": "go-dev",
  "description": "Go development environment",
  "distribution": "alpine:3.19",
  "packages": [
    "go",
    "git",
    "make",
    "gcc",
    "musl-dev"
  ],
  "postInstall": [
    "go version",
    "echo 'Go environment ready!'"
  ],
  "environment": {
    "GOPATH": "/home/user/go",
    "GOBIN": "/home/user/go/bin"
  }
}
```

### Step 4: Use Your Blueprint

```powershell
# Provision environment from custom blueprint
thresh up go-dev

# Verify
wsl -d thresh-go-dev
go version
# Output: go version go1.21.6 linux/amd64
```

## Distribution Options

Choose the right base distribution:

```mermaid
graph LR
    subgraph Choices[\"Distribution Choices\"]
        Alpine[Alpine Linux<br/>15 MB<br/>apk]
        Ubuntu[Ubuntu<br/>78 MB<br/>apt]
        Debian[Debian<br/>116 MB<br/>apt]
    end
    
    subgraph Uses[\"Best For\"]
        Light[Lightweight<br/>CI/CD]
        General[General Dev<br/>Learning]
        Stable[Production<br/>Stability]
    end
    
    Alpine --> Light
    Ubuntu --> General
    Debian --> Stable
    
    style Alpine fill:#0D597F,stroke:#0A4461,color:#fff
    style Ubuntu fill:#E95420,stroke:#C9401F,color:#fff
    style Debian fill:#D70A53,stroke:#B20843,color:#fff
```

### Alpine Linux (Minimal)

**Best for:** Small footprint, fast downloads, production containers

```json
{
  "distribution": "alpine:3.19",
  "packages": ["nodejs", "npm"]
}
```

**Package manager:** `apk`  
**Size:** ~15 MB  
**Use case:** Lightweight development, CI/CD

### Ubuntu (Popular)

**Best for:** Familiarity, broad package availability, tutorials

```json
{
  "distribution": "ubuntu:22.04",
  "packages": ["nodejs", "npm"]
}
```

**Package manager:** `apt`  
**Size:** ~78 MB  
**Use case:** General development, learning

### Debian (Stable)

**Best for:** Long-term support, stability, servers

```json
{
  "distribution": "debian:12",
  "packages": ["nodejs", "npm"]
}
```

**Package manager:** `apt`  
**Size:** ~116 MB  
**Use case:** Production-like environments

## Package Management

Different distributions use different package managers:

### Alpine (apk)

```json
{
  "distribution": "alpine:3.19",
  "packages": [
    "python3",      // Python interpreter
    "py3-pip",      // pip package manager
    "py3-requests", // Python library (use py3- prefix)
    "git",
    "vim"
  ]
}
```

:::tip
Alpine uses `py3-` prefix for Python packages. Check available packages: https://pkgs.alpinelinux.org/
:::

### Ubuntu/Debian (apt)

```json
{
  "distribution": "ubuntu:22.04",
  "packages": [
    "python3",
    "python3-pip",
    "python3-requests",
    "git",
    "vim"
  ],
  "postInstall": [
    "apt-get update && apt-get upgrade -y"
  ]
}
```

## Post-Install Scripts

Run commands after package installation:

### Simple Commands

```json
{
  "postInstall": [
    "pip install flask fastapi",
    "npm install -g typescript",
    "curl --version"
  ]
}
```

### Multi-line Scripts

```json
{
  "postInstall": [
    "pip install flask",
    "mkdir -p /app",
    "cd /app && git clone https://github.com/example/repo.git"
  ]
}
```

### Complex Setup

```json
{
  "postInstall": [
    "# Configure Node.js",
    "npm config set prefix ~/.npm-global",
    "echo 'export PATH=~/.npm-global/bin:$PATH' >> ~/.bashrc",
    "",
    "# Install global tools",
    "npm install -g typescript ts-node nodemon",
    "",
    "# Create project structure",
    "mkdir -p ~/projects/{src,tests,docs}"
  ]
}
```

:::warning
Post-install scripts run as the default user. Use `sudo` for system-level changes (not recommended for security).
:::

## Environment Variables

Set environment variables for all sessions:

```json
{
  "environment": {
    "EDITOR": "vim",
    "NODE_ENV": "development",
    "API_URL": "http://localhost:3000",
    "DATABASE_URL": "postgresql://localhost:5432/dev"
  }
}
```

These are added to `~/.bashrc` and persist across sessions.

## Advanced: Directory Mounts

Mount host directories into the environment:

```json
{
  "mounts": [
    {
      "host": "C:\\Users\\user\\projects",
      "container": "/home/user/projects",
      "readOnly": false
    },
    {
      "host": "C:\\Users\\user\\.ssh",
      "container": "/home/user/.ssh",
      "readOnly": true
    }
  ]
}
```

:::caution
Mounts are configured when environment is created. Changing mounts requires destroying and recreating the environment.
:::

## Advanced: Port Forwarding

Expose container ports to host:

```json
{
  "ports": [
    {
      "container": 3000,
      "host": 3000,
      "protocol": "tcp"
    },
    {
      "container": 5432,
      "host": 5432,
      "protocol": "tcp"
    }
  ]
}
```

Access from Windows:
```powershell
curl http://localhost:3000
```

## Real-World Examples

### Full-Stack JavaScript

```json
{
  "name": "fullstack-js",
  "description": "Node.js + PostgreSQL + Redis",
  "distribution": "alpine:3.19",
  "packages": [
    "nodejs",
    "npm",
    "postgresql",
    "redis",
    "git"
  ],
  "postInstall": [
    "npm install -g typescript tsx nodemon",
    "rc-update add postgresql",
    "rc-update add redis",
    "/etc/init.d/postgresql setup",
    "/etc/init.d/postgresql start",
    "/etc/init.d/redis start",
    "psql -U postgres -c \"CREATE DATABASE devdb;\""
  ],
  "environment": {
    "NODE_ENV": "development",
    "DATABASE_URL": "postgresql://postgres@localhost:5432/devdb",
    "REDIS_URL": "redis://localhost:6379"
  },
  "ports": [
    {"container": 3000, "host": 3000},
    {"container": 5432, "host": 5432},
    {"container": 6379, "host": 6379}
  ]
}
```

### Python Data Science

```json
{
  "name": "datascience-python",
  "description": "Python with Jupyter, pandas, NumPy",
  "distribution": "ubuntu:22.04",
  "packages": [
    "python3",
    "python3-pip",
    "python3-dev",
    "build-essential",
    "git"
  ],
  "postInstall": [
    "pip install --upgrade pip",
    "pip install jupyter pandas numpy scipy matplotlib seaborn scikit-learn",
    "mkdir -p ~/notebooks"
  ],
  "environment": {
    "JUPYTER_ENABLE_LAB": "yes"
  },
  "ports": [
    {"container": 8888, "host": 8888}
  ]
}
```

### Rust Development

```json
{
  "name": "rust-dev",
  "description": "Rust with cargo and common tools",
  "distribution": "alpine:3.19",
  "packages": [
    "rust",
    "cargo",
    "git",
    "gcc",
    "musl-dev"
  ],
  "postInstall": [
    "rustup default stable",
    "rustup component add rustfmt clippy",
    "cargo install cargo-watch cargo-edit"
  ],
  "environment": {
    "RUST_BACKTRACE": "1"
  }
}
```

### PHP Laravel

```json
{
  "name": "laravel",
  "description": "PHP 8.2 + Composer + Laravel",
  "distribution": "alpine:3.19",
  "packages": [
    "php82",
    "php82-cli",
    "php82-pdo",
    "php82-pdo_mysql",
    "php82-openssl",
    "php82-mbstring",
    "php82-xml",
    "php82-curl",
    "composer",
    "git"
  ],
  "postInstall": [
    "composer global require laravel/installer",
    "echo 'export PATH=\"$HOME/.composer/vendor/bin:$PATH\"' >> ~/.bashrc"
  ]
}
```

## Blueprint Discovery

thresh searches for blueprints in this order:

1. **Current directory:** `./.thresh/blueprints/`
2. **User directory:** `~/.thresh/blueprints/`
3. **Built-in:** Embedded in thresh binary

Create project-specific blueprints in `./.thresh/blueprints/` for team sharing.

## Testing Your Blueprint

### Provision and Test

```powershell
# Create environment
thresh up my-blueprint

# Enter environment
wsl -d thresh-my-blueprint

# Test installations
which python3
python3 --version
pip list
env | grep MY_VAR

# Exit
exit
```

### Verify with JSON Output

```powershell
thresh blueprints --json | jq '.[] | select(.name == "my-blueprint")'
```

### Iterate Quickly

```powershell
# Destroy old version
thresh destroy my-blueprint

# Edit blueprint
notepad ~/.thresh/blueprints/my-blueprint.json

# Recreate
thresh up my-blueprint
```

## Blueprint Validation

Avoid common mistakes:

### ✅ Valid Blueprint

```json
{
  "distribution": "alpine:3.19",
  "packages": ["python3", "py3-pip"],
  "postInstall": ["pip install requests"]
}
```

### ❌ Invalid: Missing Distribution

```json
{
  "packages": ["python3"]
}
```

**Error:** `Blueprint must specify a distribution`

### ❌ Invalid: Wrong Package Name

```json
{
  "distribution": "alpine:3.19",
  "packages": ["python-pip"]  // Should be "py3-pip"
}
```

**Error:** Package installation fails

### ❌ Invalid: Bad JSON Syntax

```json
{
  "distribution": "alpine:3.19",
  "packages": ["python3"]  // Missing comma
  "postInstall": []
}
```

**Error:** JSON parsing error

## Sharing Blueprints

### Team Blueprint Repository

Create a shared repository:

```bash
my-team-blueprints/
├── python-api.json
├── node-frontend.json
├── golang-service.json
└── README.md
```

Team members clone and symlink:

```powershell
# Clone team repo
git clone https://github.com/myteam/blueprints.git

# Symlink to thresh directory
New-Item -ItemType SymbolicLink -Path "$env:USERPROFILE\.thresh\blueprints\team" -Target ".\blueprints"

# Use team blueprints
thresh up python-api
```

### GitHub Gist

Share single blueprints:

```powershell
# Download blueprint from gist
curl https://gist.githubusercontent.com/user/id/raw/blueprint.json -o ~/.thresh/blueprints/custom.json

# Use it
thresh up custom
```

## AI-Generated Blueprints

Use thresh's built-in AI to generate blueprints:

```powershell
# Interactive chat
thresh chat

# Enter prompt:
# "Create a blueprint for Django development with PostgreSQL"
```

thresh will generate a custom blueprint based on your requirements.

See [GitHub Copilot SDK Configuration](/docs/tutorials/copilot-sdk) for AI-powered blueprint generation.

## Best Practices

### Keep Blueprints Minimal

```json
// ✅ Good: Focused, single purpose
{
  "distribution": "alpine:3.19",
  "packages": ["python3", "py3-pip"]
}

// ❌ Bad: Kitchen sink
{
  "distribution": "alpine:3.19",
  "packages": ["python3", "nodejs", "go", "rust", "php", ...]
}
```

### Use Specific Versions

```json
// ✅ Good: Reproducible
{
  "distribution": "alpine:3.19"
}

// ⚠️ Risky: May change
{
  "distribution": "alpine:latest"
}
```

### Document Your Blueprints

```json
{
  "name": "python-api",
  "description": "Python 3.11 FastAPI development with PostgreSQL client",
  "distribution": "alpine:3.19",
  "packages": ["python3", "py3-pip", "postgresql-client"],
  "postInstall": [
    "pip install fastapi uvicorn sqlalchemy psycopg2-binary"
  ]
}
```

### Test Before Sharing

Always provision at least once before sharing with team.

## Troubleshooting

### Package Not Found

**Error:** `Package 'xyz' not found`

**Fix:** Check package names for your distribution:
- Alpine: https://pkgs.alpinelinux.org/
- Ubuntu: https://packages.ubuntu.com/
- Debian: https://packages.debian.org/

### Post-Install Script Fails

**Error:** Script exits with non-zero code

**Debug:**
```json
{
  "postInstall": [
    "set -x",  // Enable debug output
    "your-command-here"
  ]
}
```

### Environment Variable Not Set

Variables are added to `~/.bashrc`. Ensure you're using an interactive shell:

```bash
# Check if variable exists
echo $MY_VAR

# Source bashrc manually if needed
source ~/.bashrc
```

## Next Steps

- **[VS Code MCP Integration](/docs/tutorials/vscode-mcp)** - AI-powered blueprint generation
- **[Cross-Platform Development](/docs/tutorials/cross-platform)** - Tips for Windows, Linux, macOS
- **[CLI Reference: blueprints](/docs/cli-reference/blueprints)** - Blueprint command details

---

You now have the skills to create custom blueprints tailored to any development workflow. Share your blueprints with the community by creating a [GitHub Discussion](https://github.com/dealer426/thresh/discussions)!
