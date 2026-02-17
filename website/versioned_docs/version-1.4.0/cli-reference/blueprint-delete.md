---
sidebar_position: 6
title: thresh blueprint delete
description: Delete a generated blueprint
---

# thresh blueprint delete

Delete a user-generated blueprint by name.

## Synopsis

```bash
thresh blueprint delete <name> [options]
```

## Description

The `delete` subcommand removes a previously generated blueprint from the blueprints directory. This only affects user-generated blueprints created via `thresh blueprint generate` or AI chat mode - built-in blueprints cannot be deleted.

:::note New in v1.4.0
Blueprint deletion is a new feature in v1.4.0, allowing you to clean up custom blueprints you no longer need.
:::

## Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `<name>` | Yes | Name of the blueprint to delete (without .json extension) |

## Options

| Option | Description |
|--------|-------------|
| `--help`, `-h` | Show help information |

## Blueprint Storage Locations

Blueprints are stored in platform-specific directories:

| Platform | Location | Built-in | Generated |
|----------|----------|----------|-----------|
| Linux | `~/.local/bin/blueprints/` | ✅ | ✅ |
| macOS | `~/.local/bin/blueprints/` | ✅ | ✅ |
| Windows | `%USERPROFILE%\.local\bin\blueprints\` | ✅ | ✅ |

:::warning Only Generated Blueprints
You can only delete blueprints that you generated. Built-in blueprints (alpine-minimal, ubuntu-dev, etc.) are protected and cannot be deleted.
:::

## Examples

### Delete a Generated Blueprint

```bash
# Delete a blueprint
thresh blueprint delete my-custom-env
```

**Output:**
```
🗑️  Blueprint deleted: my-custom-env.json
```

### Delete After Finding Name

```bash
# List all blueprints
thresh blueprint list

# Find the one you want to delete
thresh blueprint list | grep test

# Delete it
thresh blueprint delete test-nginx
```

### Handle Non-Existent Blueprint

```bash
# Try to delete non-existent blueprint
thresh blueprint delete nonexistent
```

**Output:**
```
❌ Blueprint not found: nonexistent

Available blueprints:
  alpine-minimal
  ubuntu-dev
  python-dev
  my-custom-app
  ...
```

## Common Workflows

### Clean Up Test Blueprints

```bash
# Generate several test blueprints
thresh blueprint generate "nginx test" --output test-nginx-1
thresh blueprint generate "nginx test 2" --output test-nginx-2
thresh blueprint generate "nginx test 3" --output test-nginx-3

# List to verify
thresh blueprint list | grep test-nginx

# Delete them all
thresh blueprint delete test-nginx-1
thresh blueprint delete test-nginx-2
thresh blueprint delete test-nginx-3
```

### Remove Old Experiments

```bash
# List all blueprints
thresh blueprint list

# Delete old experimental blueprints
thresh blueprint delete old-experiment
thresh blueprint delete prototype-v1
thresh blueprint delete temp-test

# Verify they're gone
thresh blueprint list
```

## Error Handling

### Blueprint Not Found

If the blueprint doesn't exist, you'll see an error with available options:

```bash thresh blueprint delete missing-blueprint
```

**Output:**
```
❌ Blueprint not found: missing-blueprint

Available blueprints:
  alpine-minimal
  alpine-python
  ubuntu-dev
  python-dev
  redis-cache
```

### Protected Built-in Blueprints

Built-in blueprints are protected and will not be deleted even if you try:

```bash
# This will fail (built-in blueprint)
thresh blueprint delete alpine-minimal
```

The command will safely skip built-in blueprints.

## Tips

1. **List Before Deleting**: Always run `thresh blueprint list` first to see available blueprints
2. **Use Grep**: Filter blueprints with `thresh blueprint list | grep pattern`
3. **Check Name**: Blueprint names don't include the `.json` extension
4. **Safe Operation**: The delete operation only affects the blueprints directory, not provisioned environments

## Related Commands

- [`thresh blueprint list`](./blueprints.md) - List all blueprints
- [`thresh blueprint generate`](./generate.md) - Generate new blueprints
- [`thresh blueprint`](./blueprint.md) - Parent blueprint command
- [`thresh destroy`](./destroy.md) - Remove provisioned environments (different from deleting blueprints)

## FAQ

**Q: Can I delete built-in blueprints?**  
A: No, built-in blueprints (alpine-minimal, ubuntu-dev, etc.) are protected and cannot be deleted.

**Q: What happens to environments created from a deleted blueprint?**  
A: Deleting a blueprint does not affect already-provisioned environments. Those continue to exist until you use `thresh destroy`.

**Q: Can I recover a deleted blueprint?**  
A: No, deletion is permanent. If you generated it with AI, you can regenerate it with the same prompt.

**Q: Where are blueprints stored?**  
A: On Linux/macOS: `~/.local/bin/blueprints/`. On Windows: `%USERPROFILE%\.local\bin\blueprints\`

**Q: Do I need to include `.json` in the name?**  
A: No, use just the blueprint name without the extension.

---

## Platform Compatibility

| Platform | Status | Notes |
|----------|--------|-------|
| Windows (WSL) | ✅ Supported | Deletes from `%USERPROFILE%\.local\bin\blueprints\` |
| Linux (Docker/nerdctl) | ✅ Supported | Deletes from `~/.local/bin/blueprints/` |
| macOS (containerd) | ✅ Supported | Deletes from `~/.local/bin/blueprints/` |

---

**Introduced:** v1.4.0  
**Category:** Blueprint Management  
**See Also:** [Blueprint Management Guide](/docs/tutorials/custom-blueprints)
