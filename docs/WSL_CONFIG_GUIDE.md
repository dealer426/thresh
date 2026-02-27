# WSL Configuration Profiles

**Feature added**: February 27, 2026  
**Minimum version**: thresh 1.4.0

## Overview

thresh now supports advanced WSL configuration through `wsl.conf` profiles. This feature allows you to optimize WSL distributions for specific workloads (databases, web servers, Docker, etc.) by automatically configuring systemd, Windows interoperability, drive mounting, and other WSL settings.

## Why WSL Configuration Matters

### The Problem

By default, WSL distributions come with generic settings that may not be optimal for your use case:

- **Database servers** suffer from Windows interop overhead and 9P filesystem limitations
- **Docker environments** need systemd enabled and may need auto-start
- **Web servers** benefit from custom hostnames and network configuration
- **Minimal environments** waste resources on unnecessary features

### The Solution

thresh's WSL configuration profiles solve these issues by:

1. **Built-in profiles** for common scenarios (database, docker, web-server, minimal, development)
2. **Custom profiles** you can create and share
3. **Inline configuration** for one-off customizations
4. **Automatic validation** against Microsoft's documented options
5. **Auto-restart** to apply changes (handles the "8-second rule")

## Usage

### Command Line Interface

```bash
# List all available profiles
thresh wslconf list

# Show profile content
thresh wslconf show database

# Display all valid wsl.conf options
thresh wslconf options

# Validate a custom wsl.conf file
thresh wslconf validate ~/.thresh/profiles/custom.wslconf
```

### In Blueprints

Add WSL configuration to any blueprint using one of three methods:

#### Method 1: Built-in Profile (Recommended)

```json
{
  "name": "mysql-optimized",
  "base": "ubuntu-24.04",
  "wslConfig": "database",
  "packages": ["mysql-server"],
  "scripts": {
    "setup": "systemctl enable mysql && systemctl start mysql"
  }
}
```

#### Method 2: External Profile File

```json
{
  "name": "custom-env",
  "base": "ubuntu-24.04",
  "wslConfigFile": "~/.thresh/profiles/my-custom.wslconf"
}
```

#### Method 3: Inline Custom Configuration

```json
{
  "name": "redis-dev",
  "base": "ubuntu-24.04",
  "wslConfigCustom": "[boot]\nsystemd=true\ncommand=service redis-server start\n\n[network]\nhostname=redis-dev"
}
```

## Built-in Profiles

### `systemd`
**Best for**: Modern services that require systemd

```ini
[boot]
systemd=true
```

**Use cases**: PostgreSQL, MySQL, Redis, Docker, systemd services

---

### `database`
**Best for**: Database servers requiring maximum performance

```ini
[boot]
systemd=true

[interop]
enabled=false
appendWindowsPath=false

[automount]
enabled=false

[network]
generateHosts=true
generateResolvConf=true
```

**Why this works**: 
- No Windows interop overhead
- No 9P filesystem issues (databases run on native Linux FS)
- Isolated environment for security
- systemd for service management

**Recommended for**: PostgreSQL, MySQL, MongoDB, Redis, CockroachDB

---

### `docker`
**Best for**: Docker development environments

```ini
[boot]
systemd=true
command=service docker start

[user]
default=developer
```

**Features**:
- Auto-starts Docker daemon on boot
- Sets default user to `developer`
- Full systemd support

---

### `web-server`
**Best for**: Web application servers

```ini
[boot]
systemd=true
command=service nginx start

[network]
hostname=dev-server
generateHosts=true
generateResolvConf=true
```

**Use cases**: Nginx, Apache, Node.js servers, PHP-FPM

---

### `minimal`
**Best for**: Lightweight, fast-starting environments

```ini
[boot]
systemd=false

[interop]
enabled=false
appendWindowsPath=false

[automount]
enabled=false
```

**Characteristics**:
- No systemd (faster startup)
- No Windows integration
- Minimal resource usage
- Fastest possible boot time

---

### `development` (Default)
**Best for**: General development with Windows integration

```ini
[boot]
systemd=true

[interop]
enabled=true
appendWindowsPath=true

[automount]
enabled=true
root=/mnt
options=metadata,uid=1000,gid=1000,umask=022

[network]
generateHosts=true
generateResolvConf=true
```

**Features**:
- Full Windows interoperability
- Access to Windows drives
- Windows tools in $PATH
- Balanced for most scenarios

## Creating Custom Profiles

### 1. Create Profile File

Create a file in `~/.thresh/profiles/`:

```bash
# Windows PowerShell
mkdir -Force $env:USERPROFILE\.thresh\profiles
notepad $env:USERPROFILE\.thresh\profiles\myprofile.wslconf
```

### 2. Define Configuration

Example custom profile for a load testing environment:

```ini
# Load Testing Profile
# Optimized for high-performance load generation

[boot]
systemd=true
command=service prometheus start

[interop]
enabled=false
appendWindowsPath=false

[automount]
enabled=false

[network]
hostname=loadgen
generateHosts=true
generateResolvConf=true

[user]
default=loadtester
```

### 3. Use in Blueprint

```json
{
  "name": "loadtest-env",
  "base": "ubuntu-24.04",
  "wslConfigFile": "~/.thresh/profiles/myprofile.wslconf",
  "packages": ["apache2-utils", "wrk", "prometheus"]
}
```

## Configuration Options Reference

### [boot] Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `systemd` | boolean | false | Enable systemd support (required for most services) |
| `command` | string | null | Command to run at startup (runs as root) |
| `protectBinfmt` | boolean | true | Prevents WSL from generating systemd units |

### [automount] Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `enabled` | boolean | true | Auto-mount Windows drives under /mnt |
| `mountFsTab` | boolean | true | Process /etc/fstab on startup |
| `root` | string | /mnt/ | Mount point directory for Windows drives |
| `options` | string | null | DrvFs mount options (uid, gid, umask, metadata, case) |

### [network] Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `generateHosts` | boolean | true | Generate /etc/hosts file |
| `generateResolvConf` | boolean | true | Generate /etc/resolv.conf for DNS |
| `hostname` | string | null | Custom hostname for the distribution |

### [interop] Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `enabled` | boolean | true | Allow launching Windows processes from WSL |
| `appendWindowsPath` | boolean | true | Add Windows PATH to $PATH environment variable |

### [user] Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `default` | string | root | Default user when starting WSL session |

### [gpu] Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `enabled` | boolean | true | Allow Linux apps to access Windows GPU |

### [time] Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `useWindowsTimezone` | boolean | true | Use and sync to Windows timezone |

## How It Works

### Provisioning Flow

When you run `thresh up` with a blueprint containing WSL configuration:

1. **Base Distribution Installed** - WSL distribution is created
2. **Volumes Setup** - Persistent volumes are mounted (if any)
3. **WSL Configuration Applied** - `wsl.conf` is written to `/etc/wsl.conf`
4. **Distribution Restarted** - Automatic restart to apply changes (8-second rule)
5. **Packages Installed** - Dependencies are installed
6. **Scripts Executed** - Setup and post-install scripts run

### The 8-Second Rule

Microsoft WSL has an "8-second rule": you must wait 8 seconds after stopping all instances of a distribution for configuration changes to take effect.

thresh handles this automatically:
```csharp
await containerService.StopEnvironmentAsync(envName);
await Task.Delay(TimeSpan.FromSeconds(8));
await containerService.StartEnvironmentAsync(envName);
```

### Configuration Priority

If multiple configuration sources are specified, priority is:

1. **`wslConfigCustom`** (highest priority - inline configuration)
2. **`wslConfigFile`** (custom profile file)
3. **`wslConfig`** (built-in profile name)

## Validation

All configurations are validated before being written:

```bash
thresh wslconf validate myconfig.wslconf
```

**Checks performed**:
- Valid section names (`[boot]`, `[automount]`, `[network]`, `[interop]`, `[user]`, `[gpu]`, `[time]`)
- Valid keys within each section
- Proper `key=value` format
- Boolean values are `true` or `false`
- No orphaned keys outside sections

**Example output**:
```
✅ Configuration is valid

Warnings:
  ⚠️  Line 15: Key 'command' expects string value
```

## Examples

### Example 1: High-Performance PostgreSQL

```json
{
  "name": "postgres-prod",
  "base": "ubuntu-24.04",
  "wslConfig": "database",
  "packages": ["postgresql-16"],
  "volumes": [
    {
      "name": "postgres-backups",
      "mount": "/mnt/postgres-backups"
    }
  ],
  "scripts": {
    "setup": "systemctl enable postgresql && systemctl start postgresql"
  }
}
```

**Result**: PostgreSQL runs on native Linux filesystem (no 9P overhead), no Windows interop, systemd managed.

---

### Example 2: Docker Development Environment

```json
{
  "name": "docker-dev",
  "base": "ubuntu-24.04",
  "wslConfig": "docker",
  "packages": ["docker.io", "docker-compose", "git"],
  "environment": {
    "DOCKER_HOST": "unix:///var/run/docker.sock"
  }
}
```

**Result**: Docker auto-starts on boot, full container development environment.

---

### Example 3: Multi-Service with Auto-Start

```json
{
  "name": "fullstack",
  "base": "ubuntu-24.04",
  "wslConfigCustom": "[boot]\nsystemd=true\ncommand=/usr/local/bin/start-services.sh",
  "packages": ["nginx", "postgresql", "redis-server", "nodejs"],
  "scripts": {
    "setup": "cat > /usr/local/bin/start-services.sh << 'EOF'\n#!/bin/bash\nsystemctl start nginx\nsystemctl start postgresql\nsystemctl start redis-server\nEOF\nchmod +x /usr/local/bin/start-services.sh"
  }
}
```

**Result**: All services start automatically when WSL distribution boots.

---

### Example 4: Minimal Python Environment

```json
{
  "name": "python-minimal",
  "base": "alpine-latest",
  "wslConfig": "minimal",
  "packages": ["python3", "py3-pip"],
  "environment": {
    "PYTHONUNBUFFERED": "1"
  }
}
```

**Result**: Lightweight Python environment with no systemd overhead.

## Troubleshooting

### Configuration Not Applied

**Symptom**: Changes to wsl.conf don't take effect

**Solution**: Manually restart the distribution:
```bash
thresh stop <env-name>
# Wait 8 seconds
thresh start <env-name>
```

---

### Validation Errors

**Symptom**: `❌ Configuration has errors: Unknown section [x]`

**Solution**: Check against Microsoft documentation:
```bash
thresh wslconf options
```

Valid sections: `[boot]`, `[automount]`, `[network]`, `[interop]`, `[user]`, `[gpu]`, `[time]`

---

### Profile Not Found

**Symptom**: `❌ Profile 'myprofile' not found`

**Solution**: List available profiles:
```bash
thresh wslconf list
```

Check custom profiles directory:
```powershell
ls $env:USERPROFILE\.thresh\profiles\
```

---

### systemd Not Starting

**Symptom**: Services fail with "Failed to connect to bus"

**Solution**: Verify systemd is enabled:
```bash
thresh wslconf show systemd
```

Ensure your blueprint includes:
```json
"wslConfig": "systemd"
```

## Best Practices

### 1. Choose the Right Profile

| Workload | Recommended Profile | Why |
|----------|-------------------|-----|
| PostgreSQL, MySQL | `database` | No 9P overhead, isolated |
| Redis, MongoDB | `database` | Native filesystem performance |
| Docker containers | `docker` | Auto-start, systemd support |
| Nginx, Apache | `web-server` | Custom hostname, auto-start |
| CLI tools | `minimal` | Fast startup, low overhead |
| General dev | `development` | Windows integration |

### 2. Disable Unused Features

For production-like environments:
```json
"wslConfig": "database",  // Disables interop, automount
```

For development with Windows tools:
```json
"wslConfig": "development",  // Enables interop, automount
```

### 3. Use Custom Profiles for Complex Setups

Instead of inline config, create reusable profiles:

```bash
# Create once
cat > ~/.thresh/profiles/microservices.wslconf << 'EOF'
[boot]
systemd=true
command=/usr/local/bin/start-all.sh

[network]
hostname=micro-dev
generateHosts=true
EOF

# Use many times
"wslConfigFile": "~/.thresh/profiles/microservices.wslconf"
```

### 4. Validate Before Provisioning

```bash
# Validate custom config
thresh wslconf validate ~/.thresh/profiles/myconfig.wslconf

# Provision if valid
thresh up myblueprint
```

### 5. Document Your Custom Profiles

Add comments to explain decisions:

```ini
# API Gateway Profile
# Optimized for high-throughput API services
# Disables Windows interop to reduce latency

[boot]
systemd=true
command=service nginx start

[interop]
enabled=false  # Reduces overhead for API requests
appendWindowsPath=false
```

## SaaS Integration (Future)

The hybrid approach (built-in + custom + inline) enables future SaaS features:

- **Profile Marketplace**: Browse and download community profiles
- **Team Profiles**: Share profiles across your organization
- **Version Control**: Track profile changes over time
- **Analytics**: See which profiles are most popular
- **Auto-Updates**: Get latest profile improvements

```bash
# Future commands
thresh wslconf pull community/high-performance-db
thresh wslconf publish my-custom-profile
thresh wslconf team sync
```

## Related Documentation

- [Volume User Guide](VOLUME_USER_GUIDE.md) - Persistent storage with volumes
- [Plan9 Filesystem Findings](PLAN9_FILESYSTEM_FINDINGS.md) - Technical deep dive into WSL filesystem limitations
- [Microsoft WSL Configuration Docs](https://learn.microsoft.com/en-us/windows/wsl/wsl-config)

## Changelog

### Version 1.4.0 (February 27, 2026)
- ✨ Initial release of WSL configuration profiles
- 🎯 6 built-in profiles (systemd, docker, database, web-server, minimal, development)
- ✅ Automatic validation against Microsoft WSL documentation
- 🔄 Auto-restart handling (8-second rule)
- 📝 Comprehensive CLI commands (`list`, `show`, `options`, `validate`)
- 🎨 Three configuration methods (profile, file, inline)
- 📚 Full blueprint integration
