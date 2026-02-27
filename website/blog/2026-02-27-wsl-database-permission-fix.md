---
title: Solving WSL Database Permission Errors with Configuration Profiles
description: How thresh WSL configuration profiles fix Plan9 filesystem issues for PostgreSQL, MySQL, and Redis
slug: wsl-database-permission-fix
authors: [thresh]
tags: [wsl, database, configuration, permissions, postgresql, mysql]
---

# Solving WSL Database Permission Errors with Configuration Profiles

**Problem:** PostgreSQL, MySQL, and Redis fail to start in WSL with cryptic permission errors  
**Solution:** thresh WSL configuration profiles (specifically the `database` profile)

If you've ever tried running a database in WSL and encountered errors like:

```
FATAL: data directory "/var/lib/postgresql/data" has wrong ownership
chmod: changing permissions of '/var/lib/mysql': Operation not permitted
Error: setuid failed for redis user
```

You've hit the **Plan9 filesystem permission problem**. Let's fix it permanently.

<!-- truncate -->

## The Problem: Plan9 Filesystem

WSL uses the Plan9 filesystem protocol to mount Windows drives (like `/mnt/c`). While this enables seamless file sharing between Windows and Linux, it has a critical limitation:

**Plan9 doesn't support `chmod` operations.**

This breaks database servers that require precise file permissions:

- PostgreSQL needs `0700` permissions on data directories
- MySQL requires specific ownership for `/var/lib/mysql`
- Redis needs `0755` permissions and specific user ownership

When these databases try to set permissions, they fail silently or with cryptic errors.

## Traditional "Solutions" (That Don't Work)

### ❌ chmod Commands

```bash
chmod 700 /var/lib/postgresql/data
# Works on standard Linux, fails silently on Plan9
```

### ❌ Moving Data Outside /mnt/c

```bash
# Still uses Plan9 if Windows interop is enabled
# Performance issues persist
```

### ❌ Disabling Metadata

```bash
# /etc/wsl.conf
[automount]
options = "metadata"
# Doesn't solve the core issue
```

## The Real Solution: Database WSL Profile

thresh provides WSL configuration profiles that solve this at the system level.

### What The Database Profile Does

The `database` profile completely disables Windows interop and automount:

```ini
# Automatically configured by thresh
[boot]
systemd=true

[interop]
enabled=false           # No Windows .exe access
appendWindowsPath=false  # No Windows PATH pollution

[automount]
enabled=false           # No /mnt/c, no /mnt/d

[network]
generateResolvConf=true  # Maintain DNS resolution
```

**Result:** Pure Linux filesystem behavior, no Plan9 anywhere. Databases work perfectly.

## Using WSL Configuration Profiles

### Basic Usage

Just add one property to your blueprint:

```json
{
  "name": "postgres-dev",
  "base": "ubuntu-22.04",
  "wslConfig": "database",
  "packages": ["postgresql"]
}
```

That's it! No manual configuration, no complex setup.

### Complete Production Example

```json
{
  "name": "postgres-production",
  "description": "Production PostgreSQL with persistent storage",
  "base": "ubuntu-22.04",
  "ports": ["5432:5432"],
  "volumes": [
    {
      "name": "pgdata",
      "mountPath": "/var/lib/postgresql/data"
    },
    {
      "name": "backups",
      "mountPath": "/backups"
    }
  ],
  "wslConfig": "database",
  "packages": [
    "postgresql",
    "postgresql-contrib"
  ],
  "postInstall": "systemctl enable postgresql"
}
```

```powershell
thresh up postgres-production

# PostgreSQL starts without any permission errors!
wsl -d thresh-postgres-production -- psql -U postgres
```

## How It Works

### 1. Profile Selection

thresh includes 6 built-in profiles:

```powershell
thresh wslconf list
```

```
Available WSL Configuration Profiles:

1. systemd (88 bytes)
   Basic systemd enablement

2. docker (188 bytes)
   Docker daemon auto-start

3. database (355 bytes)
   PostgreSQL, MySQL, MongoDB, Redis optimization

4. web-server (250 bytes)
   Nginx/Apache auto-start

5. minimal (238 bytes)
   Disabled interop for isolation

6. development (350 bytes)
   Full development features
```

### 2. Automatic Configuration

During `thresh up`:

1. Loads the specified profile
2. Validates configuration syntax
3. Writes to `/etc/wsl.conf`
4. Automatically restarts the distro
5. Settings applied on next boot

### 3. Verification

Check what's configured:

```powershell
wsl -d thresh-postgres-production cat /etc/wsl.conf
```

```ini
# Database Profile
# Optimized for database servers (PostgreSQL, MySQL, MongoDB, Redis)
# Disables Windows interop and drive mounting for performance
# Uses native Linux filesystem only

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

## Database-Specific Examples

### PostgreSQL

```json
{
  "name": "postgres-14",
  "base": "ubuntu-22.04",
  "wslConfig": "database",
  "packages": ["postgresql-14"],
  "postInstall": [
    "systemctl enable postgresql",
    "sudo -u postgres psql -c \"ALTER USER postgres PASSWORD 'dev123';\""
  ]
}
```

### MySQL

```json
{
  "name": "mysql-8",
  "base": "ubuntu-22.04",
  "wslConfig": "database",
  "packages": ["mysql-server"],
  "postInstall": "systemctl enable mysql"
}
```

### Redis

```json
{
  "name": "redis-persistent",
  "base": "ubuntu-22.04",
  "wslConfig": "database",
  "volumes": [
    {
      "name": "redis-data",
      "mountPath": "/data"
    }
  ],
  "packages": ["redis-server"],
  "postInstall": [
    "sed -i 's/^dir .*/dir \\/data/' /etc/redis/redis.conf",
    "systemctl enable redis"
  ]
}
```

### MongoDB

```json
{
  "name": "mongodb-replica",
  "base": "ubuntu-22.04",
  "wslConfig": "database",
  "volumes": [
    {
      "name": "mongo-data",
      "mountPath": "/data/db"
    }
  ],
  "packages": ["mongodb"],
  "postInstall": "systemctl enable mongodb"
}
```

## Other WSL Profiles

### Docker Profile

Auto-starts Docker daemon:

```json
{
  "name": "docker-dev",
  "base": "ubuntu-22.04",
  "wslConfig": "docker",
  "packages": ["docker.io", "docker-compose"]
}
```

### Web Server Profile

Auto-starts Nginx:

```json
{
  "name": "nginx-server",
  "base": "ubuntu-22.04",
  "wslConfig": "web-server",
  "ports": ["8080:80"],
  "packages": ["nginx"]
}
```

### Minimal Profile

Maximum isolation (no Windows access):

```json
{
  "name": "isolated-build",
  "base": "alpine-3.19",
  "wslConfig": "minimal",
  "packages": ["build-base", "git"]
}
```

## Custom WSL Configuration

### From File

```json
{
  "name": "custom-env",
  "wslConfigFile": "C:\\config\\custom-wsl.conf",
  "base": "ubuntu-22.04"
}
```

### Inline Configuration

```json
{
  "name": "custom-env",
  "wslConfigCustom": "[boot]\\nsystemd=true\\n[user]\\ndefault=developer",
  "base": "ubuntu-22.04"
}
```

### Priority Order

thresh applies configurations in this order:

1. **Custom inline** (highest priority)
2. **Custom file**
3. **Built-in profile** (lowest priority)

## CLI Commands

### List Profiles

```powershell
thresh wslconf list
```

### Show Profile Content

```powershell
thresh wslconf show database
```

```ini
# Database Profile
# Optimized for database servers (PostgreSQL, MySQL, MongoDB, Redis)
...
```

### Validate Configuration

```powershell
thresh wslconf validate myconfig.conf
```

### Get Configuration Options

```powershell
thresh wslconf options
```

Lists all valid WSL configuration sections and keys.

## Troubleshooting

### Profile Not Applied

**Issue:** Changes not taking effect

```powershell
# 1. Verify wsl.conf exists
wsl -d thresh-postgres-dev cat /etc/wsl.conf

# 2. Manually restart distro
thresh stop postgres-dev
thresh start postgres-dev

# 3. Check systemd status
wsl -d thresh-postgres-dev systemctl status
```

### Database Still Has Permission Errors

**Issue:** Errors persist after applying database profile

```powershell
# 1. Verify interop is disabled
wsl -d thresh-postgres-dev which notepad.exe
# Should return empty (no Windows access)

# 2. Check automount is disabled
wsl -d thresh-postgres-dev ls /mnt
# Should be empty or show error

# 3. Recreate environment
thresh destroy postgres-dev -y
thresh up postgres-dev
```

### Need Windows Access with Database

**Issue:** Need both database optimization and Windows tools

**Solution:** Use separate environments:

```powershell
# Database environment (isolated)
thresh up postgres-production  # wslConfig: database

# Development environment (Windows access)
thresh up ubuntu-dev  # wslConfig: development
```

Access database from dev environment via network:

```bash
# From ubuntu-dev environment
psql -h 172.24.123.45 -p 5432 -U postgres
```

## Performance Benefits

Beyond fixing permissions, the database profile improves performance:

### Without Database Profile

- ✗ Plan9 filesystem overhead
- ✗ Windows PATH scanning
- ✗ Cross-filesystem operations
- ✗ Interop overhead

### With Database Profile

- ✅ Native Linux filesystem (ext4)
- ✅ No PATH pollution
- ✅ Direct filesystem access
- ✅ Minimal overhead

**Result:** 2-3x faster database operations in some workloads.

## Best Practices

### ✅ Always Use Database Profile

For any database server:

```json
"wslConfig": "database"
```

### ✅ Combine with Persistent Volumes

```json
{
  "wslConfig": "database",
  "volumes": [
    {"name": "pgdata", "mountPath": "/var/lib/postgresql/data"}
  ]
}
```

### ✅ Separate Environments by Purpose

Don't mix database servers with development tools:

```
thresh-postgres-prod   → wslConfig: database
thresh-dev-workspace   → wslConfig: development
```

### ❌ Don't Use for Desktop Applications

Database profile disables Windows access:

```json
// ❌ Bad: Need Windows GUI apps
{
  "name": "vscode-server",
  "wslConfig": "database"  // Can't launch Windows apps!
}

// ✅ Good: Use development or systemd
{
  "name": "vscode-server",
  "wslConfig": "development"
}
```

## Learn More

- [WSL Configuration Documentation](/docs/wsl-configuration)
- [Persistent Volumes Guide](/docs/tutorials/volumes)
- [Networking & Port Mapping](/docs/tutorials/networking)
- [Microsoft WSL Configuration Docs](https://learn.microsoft.com/en-us/windows/wsl/wsl-config)

## Summary

WSL database permission errors are caused by the Plan9 filesystem protocol. The solution:

1. Use thresh WSL configuration profiles
2. Apply the `database` profile to any database environment
3. Enjoy native Linux filesystem behavior
4. No more permission errors, ever

```json
{
  "wslConfig": "database"
}
```

That's it. One line. Done. 🚀

---

**Try it now:**

```powershell
# Download thresh
irm https://thresh.sh/install.ps1 | iex

# Create PostgreSQL environment
thresh blueprint generate "PostgreSQL 14 with persistent storage"

# Edit blueprint to add:
"wslConfig": "database"

# Provision
thresh up postgres-14

# No permission errors! 🎉
```
