---
sidebar_position: 3
title: GitHub Copilot SDK Configuration
description: Set up AI-powered environment management with GitHub Copilot
---

# GitHub Copilot SDK Configuration

Configure thresh to work with GitHub Copilot Chat in VS Code, enabling AI-powered environment management through natural language commands.

## What You'll Learn

- Configure thresh as an MCP server for GitHub Copilot
- Use natural language to manage environments
- Generate blueprints with AI assistance
- Troubleshoot AI chat sessions
- Best practices for AI-driven development

## Prerequisites

- thresh installed ([Quick Start](/docs/tutorials/quick-start))
- VS Code with GitHub Copilot extension
- Active GitHub Copilot subscription
- Basic familiarity with VS Code

## Overview

thresh implements the Model Context Protocol (MCP), allowing GitHub Copilot to:
- List and provision environments
- Generate custom blueprints
- Manage environment lifecycle
- Answer questions about thresh capabilities

```
You (VS Code) → GitHub Copilot → thresh MCP Server → Container Runtime
```

## Step 1: Initialize MCP Configuration

Run the automatic configuration tool:

```powershell
thresh index
```

**Output:**
```
[INFO] Initializing MCP configuration for VS Code

Detected workspace: C:\Users\user\projects\my-app

Creating MCP configuration:
  File: .vscode\mcp.json
  Server: thresh
  Command: thresh serve
  
✓ MCP configuration created successfully!

GitHub Copilot will now have access to thresh commands.
Reload VS Code or GitHub Copilot to activate.
```

This creates `.vscode/mcp.json`:

```json
{
  "mcpServers": {
    "thresh": {
      "command": "thresh",
      "args": ["serve"]
    }
  }
}
```

:::info
`thresh index` automatically detects your VS Code workspace and creates the configuration. You can run it in any project directory.
:::

## Step 2: Reload VS Code

For the MCP configuration to take effect:

1. **Command Palette:** Press `Ctrl+Shift+P` (Windows/Linux) or `Cmd+Shift+P` (macOS)
2. **Reload Window:** Type "Reload Window" and press Enter

Alternatively:
- **Restart GitHub Copilot:** `Ctrl+Shift+P` → "GitHub Copilot: Reload Copilot"

## Step 3: Verify Connection

Open GitHub Copilot Chat:
- **Shortcut:** `Ctrl+Alt+I` (Windows/Linux) or `Cmd+Alt+I` (macOS)
- **Or:** Click the chat icon in the sidebar

Test the connection:

```
Can you list my thresh environments?
```

**Expected response:**
```
I can see 2 thresh environments:

1. python-dev (running)
   - Alpine Linux 3.19
   - Uptime: 2h 15m

2. node-dev (stopped)
   - Alpine Linux 3.19
   - Last active: 1 day ago

Would you like me to start the node-dev environment?
```

## Using AI Commands

### List Environments

```
Show me all my thresh environments
```

```
What environments are running?
```

### Provision Environment

```
Create a Python development environment
```

```
I need a Node.js environment with TypeScript
```

### Generate Custom Blueprint

```
Generate a blueprint for Django development with PostgreSQL and Redis
```

**Copilot will:**
1. Analyze your requirements
2. Choose appropriate distribution
3. Select packages
4. Configure environment variables
5. Create blueprint JSON
6. Save to `.thresh/blueprints/`

### Get Environment Information

```
What's the status of my python-dev environment?
```

```
How much memory is node-dev using?
```

### Troubleshooting

```
Why won't my python-dev environment start?
```

```
How do I install additional packages in my environment?
```

## Real-World Examples

### Example 1: Create Backend API

**You:**
```
I'm building a FastAPI backend. Create an environment with Python 3.11, 
FastAPI, uvicorn, and PostgreSQL client tools.
```

**Copilot:**
```
I'll create a custom environment for FastAPI development.

Creating blueprint: fastapi-backend.json...

{
  "distribution": "alpine:3.19",
  "packages": ["python3", "py3-pip", "postgresql-client"],
  "postInstall": [
    "pip install fastapi uvicorn sqlalchemy psycopg2-binary"
  ]
}

Provisioning environment...
✓ fastapi-backend environment ready!

You can enter it with: wsl -d thresh-fastapi-backend
```

### Example 2: Multi-Environment Setup

**You:**
```
Set up environments for a microservices project:
- Frontend: Node.js 20 with React
- Backend: Python with Flask
- Database: Just the PostgreSQL client tools
```

**Copilot:**
```
I'll create 3 environments for your microservices architecture:

1. Creating react-frontend...
   - Node.js 20, npm, create-react-app
   
2. Creating flask-backend...
   - Python 3.11, Flask, SQLAlchemy
   
3. Creating db-tools...
   - PostgreSQL client
   
All environments provisioned successfully!

Use:
  wsl -d thresh-react-frontend
  wsl -d thresh-flask-backend
  wsl -d thresh-db-tools
```

### Example 3: Debug Issues

**You:**
```
My python-dev environment won't start. What's wrong?
```

**Copilot:**
```
Let me check the environment status...

Issue found: WSL 2 distribution is corrupted

Fix:
1. Export any important files (they'll be in /home/user)
2. Destroy the environment: thresh destroy python-dev
3. Recreate it: thresh up python-dev

Would you like me to help you backup files first?
```

## Advanced Configuration

### Custom Server Path

If thresh isn't in PATH, specify full path:

`.vscode/mcp.json`:
```json
{
  "mcpServers": {
    "thresh": {
      "command": "C:\\Program Files\\thresh\\thresh.exe",
      "args": ["serve"]
    }
  }
}
```

### Environment Variables

Pass configuration to thresh server:

```json
{
  "mcpServers": {
    "thresh": {
      "command": "thresh",
      "args": ["serve"],
      "env": {
        "THRESH_CONFIG": "C:\\custom\\config.json",
        "THRESH_LOG_LEVEL": "debug"
      }
    }
  }
}
```

### Logging

Enable debug logs for troubleshooting:

```json
{
  "mcpServers": {
    "thresh": {
      "command": "thresh",
      "args": ["serve", "--log-file", "C:\\thresh-mcp.log", "--log-level", "debug"]
    }
  }
}
```

View logs:
```powershell
Get-Content C:\thresh-mcp.log -Wait
```

## Multi-Workspace Setup

Different projects can have different MCP configurations:

```
Projects/
├── web-app/
│   └── .vscode/
│       └── mcp.json  ← thresh for this project
├── mobile-app/
│   └── .vscode/
│       └── mcp.json  ← thresh + other tools
└── data-science/
    └── .vscode/
        └── mcp.json  ← thresh with custom blueprints
```

## Workspace vs User Settings

### Workspace (.vscode/mcp.json)

**Best for:** Project-specific configuration, team sharing

```json
{
  "mcpServers": {
    "thresh": {
      "command": "thresh",
      "args": ["serve"]
    }
  }
}
```

Committed to Git for team use.

### User Settings

**Best for:** Personal, global configuration across all projects

**File:** `%APPDATA%\Code\User\settings.json` (Windows)

```json
{
  "github.copilot.advanced": {
    "mcp.thresh.command": "thresh",
    "mcp.thresh.args": ["serve"]
  }
}
```

## AI Chat Best Practices

### Be Specific

**❌ Vague:**
```
Make an environment
```

**✅ Specific:**
```
Create a Python 3.11 environment with Flask, SQLAlchemy, and pytest
```

### Provide Context

**❌ No context:**
```
It's not working
```

**✅ With context:**
```
My python-dev environment won't start. 
Error: "WSL 2 kernel not found"
OS: Windows 11
```

### Use Natural Language

You don't need exact command syntax:

```
✅ "Show my environments"
✅ "List all environments"
✅ "What environments do I have?"
✅ "Are any environments running?"
```

All produce the same result!

### Iterate

```
You: Create a Node.js environment
Copilot: [creates basic Node environment]

You: Add TypeScript and ESLint
Copilot: [updates blueprint with TypeScript]

You: Also add Jest for testing
Copilot: [adds Jest configuration]
```

## Troubleshooting

### Copilot Doesn't See thresh

**Symptom:** Copilot says "I don't have access to thresh commands"

**Fix:**
1. Verify `.vscode/mcp.json` exists in workspace
2. Reload VS Code window
3. Check thresh is in PATH: `thresh --version`
4. Restart GitHub Copilot extension

### MCP Server Won't Start

**Symptom:** "Failed to start MCP server"

**Debug:**
```powershell
# Test server manually
thresh serve

# Should show:
# [INFO] MCP server starting on stdio transport
# [INFO] Server ready, waiting for requests...
```

If this fails, check:
- thresh installation: `thresh --version`
- Container runtime: `wsl --status` (Windows) or `docker ps` (Linux)

### Slow Responses

**Symptom:** Copilot takes >10 seconds to respond

**Causes:**
- First request downloads distribution images
- Large number of environments (>10)
- Container runtime initialization

**Fix:**
- Subsequent requests are faster (caching)
- Destroy unused environments: `thresh list` → `thresh destroy <name>`

### Permission Errors

**Symptom:** "Access denied" or "Permission denied"

**Windows WSL:**
```powershell
# Ensure WSL 2 is running
wsl --status

# Restart WSL if needed
wsl --shutdown
wsl
```

**Linux:**
```bash
# Add user to docker group
sudo usermod -aG docker $USER
# Log out and back in
```

## Examples Library

### Data Science

```
Create a Jupyter notebook environment with pandas, NumPy, 
matplotlib, and scikit-learn
```

### Web Development

```
I need a full-stack environment: Node.js frontend + Python backend
```

### DevOps

```
Set up an environment with Docker, kubectl, Terraform, and Azure CLI
```

### Mobile Development

```
Create a React Native development environment with Android SDK tools
```

## Comparison: CLI vs AI Chat

| Task | CLI Command | AI Chat |
|------|-------------|---------|
| **List environments** | `thresh list` | "Show my environments" |
| **Create environment** | `thresh up python-dev` | "Create a Python environment" |
| **Custom blueprint** | Edit JSON manually | "Generate a Django blueprint" |
| **Troubleshoot** | Read logs, docs | "Why won't my env start?" |
| **Learn** | Read CLI reference | "How do I add packages?" |

**Use CLI when:** Speed, scripting, automation  
**Use AI when:** Learning, complex setups, troubleshooting

## Security Considerations

### What Copilot Can Access

Copilot (via thresh MCP) can:
- ✅ List environments
- ✅ Create/destroy environments
- ✅ Read blueprint files
- ❌ Access files inside environments
- ❌ Modify container runtime directly

### Data Privacy

- Environment names and metadata sent to GitHub Copilot API
- Blueprint templates may be analyzed by AI
- No source code from environments is sent
- MCP communication is via local stdio (no network)

### Disable MCP

Remove `.vscode/mcp.json` to disable thresh integration.

## Next Steps

- **[VS Code MCP Integration](/docs/tutorials/vscode-mcp)** - Deep dive into MCP protocol
- **[Creating Custom Blueprints](/docs/tutorials/custom-blueprints)** - Manual blueprint creation
- **[CLI Reference: serve](/docs/cli-reference/serve)** - MCP server details

## Community Examples

Share your Copilot + thresh workflows on [GitHub Discussions](https://github.com/dealer426/thresh/discussions)!

---

You can now use natural language to manage development environments through GitHub Copilot. Experiment with different prompts to discover what's possible!
