---
sidebar_position: 4
title: thresh generate
description: Generate custom blueprints using AI
---

# thresh generate

Generate custom environment blueprints using AI from natural language descriptions.

## Synopsis

```powershell
thresh generate <prompt> [options]
```

## Description

The `generate` command uses AI (via GitHub Copilot SDK) to create custom environment blueprints from natural language descriptions. The AI analyzes your requirements and generates a complete blueprint specification.

:::info AI Model Support
Supports 20+ AI models including GPT-4o, Claude 3.5, Gemini 1.5, and more. Set your preferred model with `thresh config set default-model`.
:::

## Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `<prompt>` | Yes | Natural language description of the environment |

## Options

| Option | Description |
|--------|-------------|
| `--model <name>`, `-m` | Use specific AI model (overrides default) |
| `--output <file>`, `-o` | Save blueprint to file |
| `--verbose`, `-v` | Show AI reasoning process |
| `--help`, `-h` | Show help information |

## Examples

### Basic Usage

```powershell
thresh generate "Python data science environment with Jupyter and pandas"
```

**Output:**
```json
{
  "name": "python-datascience",
  "description": "Python data science environment with Jupyter and pandas",
  "base": "ubuntu-22.04",
  "packages": [
    "python3",
    "python3-pip",
    "python3-venv",
    "build-essential",
    "git"
  ],
  "post_install": [
    "pip3 install jupyter pandas numpy matplotlib seaborn scikit-learn",
    "jupyter notebook --generate-config"
  ]
}
```

### Save to File

```powershell
# Save and use
thresh generate "Node.js 20 with TypeScript and PostgreSQL" > node-ts.json
thresh up node-ts
```

### Use Specific Model

```powershell
# Use Claude 3.5 Sonnet
thresh generate "Rust development environment" --model claude-3.5-sonnet

# Use GPT-4o
thresh generate "Go web server with Redis" --model gpt-4o
```

### Complex Environments

```powershell
thresh generate "Full-stack development environment with:
- Node.js 20 and TypeScript
- PostgreSQL 15
- Redis 7
- nginx as reverse proxy
- Docker for containerization
- Git and common dev tools"
```

**Output includes:**
- Base distribution selection
- All required packages
- Service configuration scripts
- Environment variables
- Post-install setup

### Save with Custom Name

```powershell
# Generate and save
thresh generate "PHP 8.2 with Laravel" --output ~/.thresh/blueprints/laravel.json

# List to verify
thresh blueprints

# Provision
thresh up laravel
```

## Blueprint Customization

### Edit Generated Blueprints

```powershell
# Generate
thresh generate "Python environment" > custom.json

# Edit with your preferred editor
code custom.json  # VS Code
notepad custom.json  # Notepad

# Use modified blueprint
thresh up custom
```

### Common Modifications

**Add packages:**
```json
{
  "packages": [
    "python3",
    "git",
    "neovim"  // Added manually
  ]
}
```

**Add environment variables:**
```json
{
  "env": {
    "EDITOR": "vim",
    "PATH": "/usr/local/bin:$PATH"
  }
}
```

**Add custom scripts:**
```json
{
  "post_install": [
    "pip install -r requirements.txt",
    "git config --global user.name 'Your Name'",
    "echo 'source ~/.bashrc' >> ~/.profile"
  ]
}
```

## AI Models

### Available Models

```powershell
# GPT Models (fastest)
thresh generate "..." --model gpt-4o
thresh generate "..." --model gpt-4o-mini

# Claude Models (best for complex specs)
thresh generate "..." --model claude-3.5-sonnet
thresh generate "..." --model claude-3-opus

# Reasoning Models (for complex requirements)
thresh generate "..." --model o1-preview

# Gemini Models
thresh generate "..." --model gemini-1.5-pro

# Open Source Models
thresh generate "..." --model llama-3.1-405b
thresh generate "..." --model mistral-large
```

### Set Default Model

```powershell
# Set for all future commands
thresh config set default-model claude-3.5-sonnet

# Verify
thresh config get default-model
```

## Prompt Engineering Tips

### Be Specific

```powershell
# ❌ Vague
thresh generate "development environment"

# ✅ Specific
thresh generate "Node.js 20 development with Express, TypeScript, and MongoDB"
```

### Include Version Numbers

```powershell
# ✅ Better
thresh generate "Python 3.11 with Django 5.0 and PostgreSQL 15"
```

### Specify Tools and Configuration

```powershell
# ✅ Comprehensive
thresh generate "
Go 1.21 development environment with:
- golangci-lint for code quality
- delve for debugging
- PostgreSQL 15 client libraries
- Docker CLI
- git with pre-commit hooks
- vim with go plugin
"
```

## Troubleshooting

### "GitHub CLI not authenticated"

```powershell
# Check authentication
gh auth status

# Re-authenticate
gh auth login

# Verify
thresh config status
```

### "Model not available"

```powershell
# List supported models (see documentation)
# Use a different model
thresh generate "..." --model gpt-4o
```

### Invalid Blueprint Generated

The AI occasionally generates invalid JSON. Edit the output:

```powershell
# Generate
thresh generate "..." > blueprint.json

# Validate JSON syntax
cat blueprint.json | ConvertFrom-Json

# Fix any issues manually
notepad blueprint.json
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Blueprint generated successfully |
| `1` | AI error or invalid response |
| `2` | Invalid arguments |
| `5` | Configuration error (API key missing) |

## See Also

- [`thresh chat`](./chat) - Interactive AI assistant
- [`thresh up`](./up) - Provision from blueprint
- [`thresh blueprints`](./blueprints) - List available blueprints
- [`thresh config`](./config) - Configure AI settings
