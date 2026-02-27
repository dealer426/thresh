---
sidebar_position: 3
title: thresh blueprint
description: Manage environment blueprints
---

# thresh blueprint

Manage environment blueprints - list, generate, and delete blueprint configurations.

## Synopsis

```bash
thresh blueprint <command> [options]
```

## Description

The `blueprint` command is the parent command for all blueprint-related operations. It provides subcommands for listing available blueprints, generating new ones with AI, and deleting custom blueprints.



## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`list`](#thresh-blueprint-list) | List all available blueprints |
| [`generate`](#thresh-blueprint-generate) | Generate blueprint from natural language using AI |
| [`delete`](#thresh-blueprint-delete) | Delete a generated blueprint |

## Options

| Option | Description |
|--------|-------------|
| `--help`, `-h` | Show help information |

## Subcommand Details

### thresh blueprint list

List all available environment blueprints (built-in and custom).

**Synopsis:**
```bash
thresh blueprint list [options]
```

**Options:**
- `--json` - Output in JSON format
- `--verbose`, `-v` - Show detailed information

**Example:**
```bash
# List all blueprints
thresh blueprint list

# JSON output for scripting
thresh blueprint list --json
```

**Output:**
```
Available blueprints:

  alpine-minimal       - Alpine Linux minimal environment
  alpine-python        - Alpine Linux with Python
  ubuntu-dev           - Ubuntu development environment
  python-dev           - Python development with pip and venv
  node-dev             - Node.js development environment
  ...
```

**See:** [Complete list documentation](./blueprints.md)

---

### thresh blueprint generate

Generate custom blueprint from natural language description using AI.

**Synopsis:**
```bash
thresh blueprint generate <prompt> [options]
```

**Arguments:**
- `<prompt>` - Natural language description of desired environment

**Options:**
- `--output <name>`, `-o` - Save blueprint with specified name
- `--model <name>`, `-m` - Use specific AI model
- `--verbose`, `-v` - Show detailed AI reasoning

**Examples:**
```bash
# Generate and display blueprint
thresh blueprint generate "Python ML environment with Jupyter"

# Generate and save with custom name
thresh blueprint generate "nginx web server" --output nginx

# Use specific model
thresh blueprint generate "Redis cache" --model gpt-4o
```

**Generated blueprints are automatically saved** to the blueprints directory and become immediately available via `thresh blueprint list`.

**See:** [Complete generate documentation](./generate.md)

---

### thresh blueprint delete

Delete a generated blueprint by name.

**Synopsis:**
```bash
thresh blueprint delete <name> [options]
```

**Arguments:**
- `<name>` - Blueprint name to delete

**Options:**
- `--help`, `-h` - Show help information

**Examples:**
```bash
# Delete a generated blueprint
thresh blueprint delete nginx-test

# List blueprints to find name
thresh blueprint list
thresh blueprint delete my-custom-env
```

**Notes:**
- Only deletes user-generated blueprints (not built-in)
- Blueprints are deleted from `~/.local/bin/blueprints/` (Linux/macOS) or `%USERPROFILE%\.local\bin\blueprints\` (Windows)
- Shows confirmation message with available blueprints if name not found

**See:** [Complete delete documentation](./blueprint-delete.md)

---

## Migration from v1.3.0

If you're upgrading from v1.3.0 or earlier, update your commands:

| Old Command (v1.3.0) | New Command (v1.4.0+) |
|----------------------|----------------------|
| `thresh blueprints` | `thresh blueprint list` |
| `thresh generate "..."` | `thresh blueprint generate "..."` |
| *(no command)* | `thresh blueprint delete <name>` |

The old commands will show error messages with suggestions for the new commands.

---

## Common Workflows

### List and Use Blueprint

```bash
# Find available blueprints
thresh blueprint list

# Use a blueprint
thresh up alpine-minimal
```

### Generate Custom Blueprint

```bash
# Generate with AI
thresh blueprint generate "Django web app with PostgreSQL" --output django-app

# Verify it was saved
thresh blueprint list | grep django-app

# Use the generated blueprint
thresh up django-app
```

### Manage Generated Blueprints

```bash
# List all blueprints
thresh blueprint list

# Delete old test blueprints
thresh blueprint delete test-env-1
thresh blueprint delete test-env-2

# Verify deletion
thresh blueprint list
```

---

## See Also

- [thresh blueprint list](./blueprints.md) - Detailed list documentation
- [thresh blueprint generate](./generate.md) - Detailed generate documentation
- [thresh blueprint delete](./blueprint-delete.md) - Detailed delete documentation
- [thresh up](./up.md) - Provision environments from blueprints
- [Custom Blueprints Tutorial](/docs/tutorials/custom-blueprints) - Creating custom blueprints

---

## Platform Compatibility

| Platform | Status | Notes |
|----------|--------|-------|
| Windows (WSL) | ✅ Supported | Full functionality |
| Linux (Docker/nerdctl) | ✅ Supported | Full functionality |
| macOS (containerd) | ✅ Supported | Full functionality |

---

**Introduced:** v1.4.0  
**Category:** Blueprint Management
