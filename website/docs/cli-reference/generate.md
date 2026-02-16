---
sidebar_position: 5
title: thresh blueprint generate
description: Generate custom blueprints using AI
---

# thresh blueprint generate

Generate custom environment blueprints using AI from natural language descriptions.

:::info New in v1.4.0
This command replaces `thresh generate` as part of the grouped blueprint command structure.

**Migration:**
- Old: `thresh generate "..."`
- New: `thresh blueprint generate "..."`
:::

## Synopsis

```bash
thresh blueprint generate <prompt> [options]
```

## Description

The `blueprint generate` subcommand uses AI (via GitHub Copilot SDK) to create custom environment blueprints from natural language descriptions. The AI analyzes your requirements and generates a complete blueprint specification.

:::info AI Model Support
Supports 20+ AI models including GPT-4o, Claude 3.5, Gemini 1.5, and more. Set your preferred model with `thresh config set default-model gpt-4o`.
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

```bash
thresh blueprint generate "Python data science environment with Jupyter and pandas"
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

```bash
# Generate and display
thresh blueprint generate "Node.js 20 with TypeScript and PostgreSQL"

# Use the generated blueprint (automatically saved)
thresh up node-ts
```

### Use Specific Model

```bash
# Use Claude 3.5 Sonnet
thresh blueprint generate "Rust development environment" --model claude-3.5-sonnet

# Use GPT-4o
thresh blueprint generate "Go web server with Redis" --model gpt-4o
```

### Complex Environments

```bash
thresh blueprint generate "Full-stack development environment with:
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

```bash
# Generate and save with specific name
thresh blueprint generate "PHP 8.2 with Laravel" --output laravel

# List to verify
thresh blueprint list | grep laravel

# Provision
thresh up laravel
```

## Blueprint Customization

### Edit Generated Blueprints

```bash
# Generate
thresh blueprint generate "Python environment"

# Find the generated blueprint
thresh blueprint list | grep python

# Edit the JSON file directly
vim ~/.local/bin/blueprints/python-environment.json

# Use modified blueprint
thresh up python-environment
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

```bash
# GPT Models (fastest)
thresh blueprint generate "..." --model gpt-4o
thresh blueprint generate "..." --model gpt-4o-mini

# Claude Models (best for complex specs)
thresh blueprint generate "..." --model claude-3.5-sonnet
thresh blueprint generate "..." --model claude-3-opus

# Reasoning Models (for complex requirements)
thresh blueprint generate "..." --model o1-preview

# Gemini Models
thresh blueprint generate "..." --model gemini-1.5-pro

# Open Source Models
thresh blueprint generate "..." --model llama-3.1-405b
thresh blueprint generate "..." --model mistral-large
```

### Set Default Model

```bash
# Set for all future commands
thresh config set default-model claude-3.5-sonnet

# Verify
thresh config get default-model
```

## Prompt Engineering Tips

### Be Specific

```bash
# ❌ Vague
thresh blueprint generate "development environment"

# ✅ Specific
thresh blueprint generate "Node.js 20 development with Express, TypeScript, and MongoDB"
```

### Include Version Numbers

```bash
# ✅ Better
thresh blueprint generate "Python 3.11 with Django 5.0 and PostgreSQL 15"
```

### Specify Tools and Configuration

```bash
# ✅ Comprehensive
thresh blueprint generate "
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

```bash
# Check authentication
gh auth status

# Re-authenticate
gh auth login

# Verify
thresh config status
```

### "Model not available"

```bash
# List supported models (see documentation)
# Use a different model
thresh blueprint generate "..." --model gpt-4o
```

### Invalid Blueprint Generated

The AI occasionally generates invalid JSON. You can verify and edit:

```bash
# Generate and check
thresh blueprint generate "..." --verbose

# If issues, find and edit the generated file
thresh blueprint list
vim ~/.local/bin/blueprints/generated-blueprint.json
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Blueprint generated successfully |
| `1` | AI error or invalid response |
| `2` | Invalid arguments |
| `5` | Configuration error (API key missing) |

## See Also

- [`thresh blueprint`](/docs/cli-reference/blueprint) - Parent blueprint command
- [`thresh blueprint list`](/docs/cli-reference/blueprints) - List available blueprints
- [`thresh blueprint delete`](/docs/cli-reference/blueprint-delete) - Delete generated blueprints
- [`thresh chat`](./chat) - Interactive AI assistant
- [`thresh up`](./up) - Provision from blueprint
- [`thresh config`](./config) - Configure AI settings
