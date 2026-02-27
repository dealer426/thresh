---
sidebar_position: 16
title: thresh wslconf
description: Manage WSL configuration profiles
---

# thresh wslconf

Manage WSL configuration profiles for optimized environments.

:::info Windows Only
The `wslconf` command is only available on Windows systems with WSL 2.
:::

## Synopsis

```powershell
thresh wslconf <subcommand> [arguments] [options]
```

## Description

The `wslconf` command provides tools for managing WSL configuration profiles that optimize WSL environments for specific use cases. These profiles control WSL behavior through the `/etc/wsl.conf` file, which is automatically applied during environment provisioning.

Configuration profiles help solve common WSL issues:

- **Database Performance**: Fix filesystem permission issues that prevent databases from starting
- **Docker Compatibility**: Enable systemd support required by modern Docker installations
- **Web Development**: Optimize mount options for faster file I/O
- **Minimal Overhead**: Disable unnecessary Windows integration for performance

## Built-in Profiles

Thresh includes 6 built-in configuration profiles:

| Profile | Description | Use Case |
|---------|-------------|----------|
| `systemd` | Enables systemd support | Required for Docker, Kubernetes, and modern system services |
| `docker` | Optimized for Docker containers | Systemd enabled, Windows interop disabled for performance |
| `database` | Fixes database file permissions | Solves PostgreSQL, MySQL, MongoDB, Redis permission errors |
| `web-server` | Web development optimizations | Proper mount options for fast development workflows |
| `minimal` | Minimal Windows integration | Maximum performance with disabled Windows features |
| `development` | Balanced development setup | Systemd enabled with sensible defaults |

## Subcommands

### list

List all available WSL configuration profiles (built-in and user-defined).

```powershell
thresh wslconf list
```

**Output:**

```
Available WSL Configuration Profiles:
=====================================

Built-in Profiles:
------------------
  systemd         Enables systemd support for modern services
  docker          Optimized for Docker containers with systemd and fast mounts
  database        Optimized for databases with working file permissions
  web-server      Optimized for web development with fast file I/O
  minimal         Minimal configuration for maximum performance
  development     Balanced development setup with systemd and sensible defaults

User Profiles:
--------------
  team-standard   Company-wide WSL configuration
  custom-db       Custom database configuration with specific mount options

Total: 8 profile(s)

Usage in blueprints:
  "wslConfig": "database"
  "wslConfigFile": "~/.thresh/profiles/custom.wslconf"
  "wslConfigCustom": "[boot]\nsystemd=true"
```

### show

Display the full content of a configuration profile.

```powershell
thresh wslconf show <profile-name>
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `<profile-name>` | Yes | Name of the profile to display |

**Examples:**

```powershell
# Show the database profile
thresh wslconf show database
```

**Output:**

```
Profile: database
==================================================
# Database Profile
# Optimized for PostgreSQL, MySQL, MongoDB, Redis
# Fixes file permission issues with Plan9 filesystem

[boot]
systemd=true

[automount]
enabled=true
options=metadata,uid=1000,gid=1000,umask=077,fmask=177,dmask=077

[interop]
enabled=false
appendWindowsPath=false
==================================================
```

:::tip
Use `show` to understand what each profile does before applying it to your environment.
:::

### options

Display all available WSL configuration options from Microsoft documentation.

```powershell
thresh wslconf options
```

This command shows:

- All valid `wsl.conf` sections (`[boot]`, `[automount]`, `[network]`, etc.)
- All valid keys within each section
- Expected value types (boolean, string, etc.)
- Descriptions of what each option does
- Complete example configuration

**Output (excerpt):**

```
WSL Configuration Options (wsl.conf)
====================================

Based on Microsoft WSL documentation:
https://learn.microsoft.com/en-us/windows/wsl/wsl-config

[boot]
  Boot settings (Windows 11+ and Server 2022)

  systemd
    boolean (true/false) - Enable systemd support

  command
    string - Command to run when WSL instance starts (runs as root)

  protectBinfmt
    boolean (true/false) - Prevents WSL from generating systemd units


[automount]
  Automount settings for Windows drives

  enabled
    boolean (true/false) - Auto-mount fixed drives under /mnt

  mountFsTab
    boolean (true/false) - Process /etc/fstab on WSL start

  root
    string (path) - Directory where drives will be mounted (default: /mnt/)

  options
    comma-separated - DrvFs mount options (uid, gid, umask, fmask, dmask, metadata, case)

...
```

:::tip
Use `options` when creating custom profiles to ensure you're using valid configuration keys.
:::

### validate

Validate a custom WSL configuration file for syntax errors and unknown options.

```powershell
thresh wslconf validate <file-path>
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `<file-path>` | Yes | Path to the `.wslconf` file to validate |

**Examples:**

```powershell
# Validate a custom profile before using it
thresh wslconf validate my-custom.wslconf

# Validate a team-shared profile
thresh wslconf validate C:\Team\Configs\wsl-standard.wslconf
```

**Success Output:**

```
✅ Configuration is valid

Warnings:
  ⚠️  Line 12: Key 'command' will run as root user
```

**Error Output:**

```
❌ Configuration has errors:

  ❌ Line 5: Unknown section [runtime]
  ❌ Line 8: Unknown key 'dockerEnabled' in section [boot]
  ❌ Line 15: Key 'enabled' expects boolean value (true/false), got 'yes'

Warnings:
  ⚠️  Line 10: Key 'options' has complex syntax - verify mount options are correct
```

:::warning Validation Scope
The `validate` command checks for:
- Valid section names (`[boot]`, `[automount]`, etc.)
- Valid keys within each section
- Proper syntax (`key=value` format)
- Boolean value correctness

It does **not** validate:
- Mount option syntax (e.g., `uid=1000,gid=1000`)
- Command strings in `[boot]` section
- Path existence for directories
:::

## Usage in Blueprints

Configuration profiles are applied during environment provisioning using blueprint properties:

### Using Built-in Profiles

```json
{
  "name": "postgres-dev",
  "distro": "Ubuntu-22.04",
  "wslConfig": "database",
  "packages": ["postgresql-14"]
}
```

### Using Custom Profile Files

```json
{
  "name": "team-environment",
  "distro": "Ubuntu-22.04",
  "wslConfigFile": "~/.thresh/profiles/team-standard.wslconf"
}
```

### Using Inline Custom Configuration

```json
{
  "name": "web-server",
  "distro": "Ubuntu-22.04",
  "wslConfigCustom": "[boot]\\nsystemd=true\\ncommand=service nginx start\\n\\n[automount]\\nenabled=true\\noptions=metadata,uid=1000,gid=1000"
}
```

:::tip Priority Order
If multiple configuration properties are specified:
1. `wslConfigCustom` takes highest priority
2. `wslConfigFile` takes second priority
3. `wslConfig` (built-in) is used if neither custom option is specified
:::

## Creating Custom Profiles

Custom profiles should be placed in `~/.thresh/profiles/` directory:

### 1. Create Profile File

```powershell
# Create the profiles directory
mkdir ~/.thresh/profiles -Force

# Create a custom profile
notepad ~/.thresh/profiles/my-custom.wslconf
```

### 2. Write Configuration

```ini
# My Custom Profile
# Description: Specialized setup for ML workloads

[boot]
systemd=true
command=nvidia-smi

[automount]
enabled=true
options=metadata,uid=1000,gid=1000,umask=022

[network]
generateHosts=true
hostname=ml-workstation

[gpu]
enabled=true
```

### 3. Validate Configuration

```powershell
thresh wslconf validate ~/.thresh/profiles/my-custom.wslconf
```

### 4. Test in Blueprint

```json
{
  "name": "ml-environment",
  "distro": "Ubuntu-22.04",
  "wslConfigFile": "~/.thresh/profiles/my-custom.wslconf",
  "packages": ["python3", "python3-pip"]
}
```

## Common Workflows

### Discover Available Options

```powershell
# See what configuration options are available
thresh wslconf options

# Browse built-in profiles
thresh wslconf list

# Examine a specific profile
thresh wslconf show database
```

### Fix Database Permission Issues

```powershell
# Use the database profile in your blueprint
{
  "name": "postgres-server",
  "distro": "Ubuntu-22.04",
  "wslConfig": "database",
  "packages": ["postgresql-14"]
}
```

Or apply to an existing environment:

```powershell
# Show what the database profile contains
thresh wslconf show database

# Manually apply to existing distro
wsl -d MyDistro
sudo mkdir -p /etc
sudo nano /etc/wsl.conf  # Copy content from 'show' output
exit
wsl --terminate MyDistro
```

### Enable Docker Support

```powershell
# Use the docker profile (enables systemd)
{
  "name": "docker-dev",
  "distro": "Ubuntu-22.04",
  "wslConfig": "docker",
  "packages": ["docker.io"]
}
```

### Share Team Configuration

```powershell
# Create team-shared profile
# 1. Organization admin creates standard profile
notepad C:\Team\Configs\wsl-company-standard.wslconf

# 2. Validate it works
thresh wslconf validate C:\Team\Configs\wsl-company-standard.wslconf

# 3. Team members reference it in blueprints
{
  "name": "team-environment",
  "distro": "Ubuntu-22.04",
  "wslConfigFile": "C:/Team/Configs/wsl-company-standard.wslconf"
}
```

## Troubleshooting

### Profile Not Found

```
❌ Profile 'mycustom' not found
```

**Solution:**

```powershell
# List available profiles
thresh wslconf list

# Verify profile file exists
dir ~/.thresh/profiles/*.wslconf
```

### Configuration Not Applied

If changes don't take effect after provisioning:

```powershell
# Verify wsl.conf was created in the environment
wsl -d YourEnvironment cat /etc/wsl.conf

# If empty or wrong, WSL needs termination to reload
wsl --terminate YourEnvironment
wsl -d YourEnvironment
```

### Windows 10 systemd Not Available

The `systemd` option requires:
- Windows 11 22H2 or later
- Windows Server 2022 or later
- WSL version 0.67.6 or later

**Check WSL version:**

```powershell
wsl --version
```

**Upgrade if needed:**

```powershell
wsl --update
```

### Validation Passes But Configuration Fails

The validator checks syntax but cannot verify:

- Mount option compatibility
- Command existence/syntax
- Path availability

**Test manually:**

```powershell
# Create test environment
wsl -d TestDistro

# Try configuration manually
sudo nano /etc/wsl.conf
# Paste your configuration
# Exit and restart

wsl --terminate TestDistro
wsl -d TestDistro

# Check for errors
dmesg | grep -i wsl
```

## Related Commands

- [thresh up](./up.md) - Apply configuration during provisioning
- [WSL Configuration Guide](../wsl-configuration.md) - Complete configuration documentation

## Reference

- [Microsoft WSL Configuration Documentation](https://learn.microsoft.com/en-us/windows/wsl/wsl-config)
- [WSL Configuration File Location](https://learn.microsoft.com/en-us/windows/wsl/wsl-config#wslconf)
- [systemd Support in WSL](https://devblogs.microsoft.com/commandline/systemd-support-is-now-available-in-wsl/)

## Examples

### Example 1: Browse Available Profiles

```powershell
# List all profiles
PS C:\> thresh wslconf list

Available WSL Configuration Profiles:
=====================================

Built-in Profiles:
------------------
  systemd         Enables systemd support for modern services
  docker          Optimized for Docker containers
  database        Optimized for databases with working file permissions
  web-server      Optimized for web development
  minimal         Minimal configuration for maximum performance
  development     Balanced development setup

Total: 6 profile(s)

# Show specific profile content
PS C:\> thresh wslconf show systemd

Profile: systemd
==================================================
# Systemd Profile
# Enables systemd for modern service management

[boot]
systemd=true
==================================================
```

### Example 2: Create and Validate Custom Profile

```powershell
# Create custom profile for Java development
PS C:\> notepad ~/.thresh/profiles/java-dev.wslconf

# File content:
# [boot]
# systemd=true
#
# [automount]
# enabled=true
# options=metadata,uid=1000,gid=1000
#
# [network]
# hostname=java-workstation

# Validate before use
PS C:\> thresh wslconf validate ~/.thresh/profiles/java-dev.wslconf
✅ Configuration is valid

# Use in blueprint
PS C:\> cat blueprint.json
{
  "name": "java-environment",
  "distro": "Ubuntu-22.04",
  "wslConfigFile": "~/.thresh/profiles/java-dev.wslconf",
  "packages": ["openjdk-17-jdk", "maven", "gradle"]
}

PS C:\> thresh up blueprint.json
```

### Example 3: Check Available Configuration Options

```powershell
# See all valid wsl.conf options
PS C:\> thresh wslconf options

WSL Configuration Options (wsl.conf)
====================================

Based on Microsoft WSL documentation:
https://learn.microsoft.com/en-us/windows/wsl/wsl-config

[boot]
  Boot settings (Windows 11+ and Server 2022)

  systemd
    boolean (true/false) - Enable systemd support

  command
    string - Command to run when WSL instance starts (runs as root)
...

# Use this information to create custom profiles
```

### Example 4: Fix Database Permission Error

```powershell
# Problem: PostgreSQL won't start
PS C:\> wsl -d MyDatabase
$ sudo systemctl start postgresql
Job for postgresql.service failed...
$ sudo tail /var/log/postgresql/postgresql-14-main.log
FATAL: data directory "/var/lib/postgresql/14/main" has wrong ownership
DETAIL: The server must be started by the user that owns the data directory.

# Solution: Use database profile
PS C:\> thresh wslconf show database
Profile: database
==================================================
# Database Profile
# Fixes file permission issues

[boot]
systemd=true

[automount]
enabled=true
options=metadata,uid=1000,gid=1000,umask=077,fmask=177,dmask=077

[interop]
enabled=false
appendWindowsPath=false
==================================================

# Create new environment with database profile
PS C:\> cat db-blueprint.json
{
  "name": "postgres-fixed",
  "distro": "Ubuntu-22.04",
  "wslConfig": "database",
  "packages": ["postgresql-14"]
}

PS C:\> thresh up db-blueprint.json
# PostgreSQL now works correctly!
```

### Example 5: Inline Custom Configuration

```powershell
# Quick environment with inline configuration
PS C:\> cat quick-env.json
{
  "name": "quick-dev",
  "distro": "Ubuntu-22.04",
  "wslConfigCustom": "[boot]\\nsystemd=true\\n\\n[network]\\nhostname=devbox",
  "packages": ["nodejs", "npm"]
}

PS C:\> thresh up quick-env.json

# Verify configuration was applied
PS C:\> wsl -d quick-dev cat /etc/wsl.conf
[boot]
systemd=true

[network]
hostname=devbox
```
