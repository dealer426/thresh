---
sidebar_position: 3
title: thresh destroy
description: Remove a WSL environment
---

# thresh destroy

Remove a thresh-managed WSL environment completely.

## Synopsis

```powershell
thresh destroy <environment-name> [options]
```

## Description

The `destroy` command permanently removes a WSL environment, including:

- The WSL distribution
- All files and data inside the environment
- Configuration and metadata
- Cached data (optional)

:::warning Data Loss
This operation is **irreversible**. All data in the environment will be permanently deleted.
:::

## Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `<environment-name>` | Yes | Name of the environment to remove |

## Options

| Option | Description |
|--------|-------------|
| `--force`, `-f` | Skip confirmation prompt |
| `--keep-cache` | Keep cached rootfs (for faster re-provisioning) |
| `--help`, `-h` | Show help information |

## Examples

### Basic Usage (with confirmation)

```powershell
thresh destroy python-dev
```

**Output:**
```
⚠ This will permanently delete the environment 'python-dev' and all its data.
Continue? (y/N): y

✓ Stopping environment...
✓ Unregistering WSL distribution...
✓ Removing metadata...
✓ Environment destroyed.
```

### Force Destroy (no confirmation)

```powershell
thresh destroy alpine-minimal --force
```

**Output:**
```
✓ Stopping environment...
✓ Unregistering WSL distribution...
✓ Removing metadata...
✓ Environment destroyed.
```

### Keep Cache for Re-provisioning

```powershell
# Destroy but keep cached rootfs
thresh destroy ubuntu-dev --keep-cache

# Later, provisioning will be faster
thresh up ubuntu-dev  # Uses cached rootfs
```

### Destroy Multiple Environments

```powershell
# PowerShell script
@("env1", "env2", "env3") | ForEach-Object {
    thresh destroy $_ --force
}
```

## What Gets Deleted

### Always Deleted
- ✓ WSL distribution
- ✓ All files in the environment
- ✓ Environment metadata
- ✓ Configuration specific to this environment

### Optionally Kept
- Cache files (with `--keep-cache`)
- Global thresh configuration

## Troubleshooting

### "Environment not found"

```powershell
# List existing environments
thresh list

# Check all WSL distributions
wsl -l -v
```

### "Environment is running"

The environment will be automatically stopped before destruction:

```powershell
thresh destroy python-dev --force
```

### Manual WSL Cleanup

If thresh fails to destroy an environment, use WSL directly:

```powershell
# Unregister WSL distribution
wsl --unregister python-dev

# Remove thresh metadata
Remove-Item -Recurse ~/.thresh/environments/python-dev
```

## Safety Features

### Confirmation Prompt

By default, thresh asks for confirmation:

```powershell
thresh destroy my-env
```
```
⚠ This will permanently delete the environment 'my-env' and all its data.
Continue? (y/N):
```

Type `y` or `yes` to proceed, anything else to cancel.

### Name Verification

Thresh verifies the environment is managed by thresh before destruction:

```powershell
# Only works on thresh-managed environments
thresh destroy python-dev  ✓ Success

# Not managed by thresh
thresh destroy Ubuntu-22.04  ✗ Error: Not a thresh environment
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Environment destroyed successfully |
| `1` | General error |
| `2` | Invalid arguments |
| `3` | WSL not available |
| `4` | Environment not found |
| `5` | User cancelled operation |

## See Also

- [`thresh list`](./list) - List environments
- [`thresh up`](./up) - Create new environments
