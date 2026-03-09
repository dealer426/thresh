---
sidebar_position: 5
title: Persistent Volumes & Storage
description: Configure persistent volumes, bind mounts, and tmpfs for data persistence
---

# Persistent Volumes & Storage

**Version:** 1.5.0  
**Platform:** Windows (WSL2), Linux, macOS

## Overview

thresh provides three types of storage for WSL environments:

- 📦 **Named Volumes** - Persistent, managed storage
- 📁 **Bind Mounts** - Direct host directory access
- 💾 **Tmpfs Mounts** - In-memory temporary storage

:::tip Why Use Volumes?
Volumes persist data across container restarts and environment recreation. Perfect for databases, application state, and configuration files that need to survive environment destruction.
:::

## Named Volumes

### Basic Usage

Create persistent volumes managed by thresh:

```json
{
  "name": "postgres-persistent",
  "base": "ubuntu-22.04",
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
  "packages": ["postgresql"]
}
```

**Characteristics:**
- ✅ Persist across environment destruction
- ✅ Managed by container runtime
- ✅ Automatic permissions
- ✅ Portable across environments
- ✅ Backup-friendly

### Volume Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `name` | string | Yes | Volume name (must be unique) |
| `mountPath` | string | Yes | Container mount path (absolute) |
| `readOnly` | boolean | No | Mount as read-only (default: false) |

### Complete Example

**Blueprint:** `redis-persistent.json`

```json
{
  "name": "redis-persistent",
  "description": "Redis with persistent storage and backups",
  "base": "ubuntu-22.04",
  "ports": ["6379:6379"],
  "volumes": [
    {
      "name": "redis-data",
      "mountPath": "/data"
    },
    {
      "name": "redis-backups",
      "mountPath": "/backups",
      "readOnly": false
    }
  ],
  "wslConfig": "database",
  "packages": ["redis-server"],
  "postInstall": "sed -i 's/^dir .*/dir \\/data/' /etc/redis/redis.conf && systemctl enable redis"
}
```

**Provision:**

```powershell
thresh up redis-persistent
```

**Data persists even after:**

```powershell
# Destroy environment
thresh destroy redis-persistent -y

# Recreate from same blueprint
thresh up redis-persistent --name redis-persistent

# Data still exists in /data volume!
```

## Bind Mounts

### Basic Usage

Mount host directories into containers:

```json
{
  "name": "web-dev",
  "base": "ubuntu-22.04",
  "bindMounts": [
    {
      "source": "C:\\Users\\demo\\projects\\myapp",
      "target": "/app"
    },
    {
      "source": "/home/user/config",
      "target": "/etc/myapp",
      "readOnly": true
    }
  ]
}
```

**Characteristics:**
- ✅ Direct access to host files
- ✅ Real-time file synchronization
- ✅ Use existing host data
- ⚠️ Platform-specific paths
- ⚠️ Requires host directory to exist

### Bind Mount Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `source` | string | Yes | Host path (Windows/Linux/macOS) |
| `target` | string | Yes | Container mount path (absolute) |
| `readOnly` | boolean | No | Mount as read-only (default: false) |

### Platform-Specific Paths

**Windows (WSL2):**
```json
{
  "source": "C:\\Users\\burns\\projects",
  "target": "/workspace"
}
```

**Linux:**
```json
{
  "source": "/home/user/projects",
  "target": "/workspace"
}
```

**macOS:**
```json
{
  "source": "/Users/user/projects",
  "target": "/workspace"
}
```

### Complete Example

**Blueprint:** `nodejs-dev.json`

```json
{
  "name": "nodejs-dev",
  "description": "Node.js development with live code reload",
  "base": "ubuntu-22.04",
  "ports": ["3000:3000"],
  "bindMounts": [
    {
      "source": "C:\\Users\\demo\\projects\\webapp",
      "target": "/app",
      "readOnly": false
    },
    {
      "source": "C:\\Users\\demo\\.npmrc",
      "target": "/root/.npmrc",
      "readOnly": true
    }
  ],
  "volumes": [
    {
      "name": "node_modules",
      "mountPath": "/app/node_modules"
    }
  ],
  "packages": ["nodejs", "npm"]
}
```

**Benefits:**
- Edit code on Windows with VS Code
- Run/test inside Linux container
- Hot reload without rebuilding
- Preserve node_modules in volume (performance)

## Tmpfs Mounts

### Basic Usage

Create in-memory temporary storage:

```json
{
  "name": "fast-cache",
  "base": "ubuntu-22.04",
  "tmpfs": [
    {
      "mountPath": "/cache",
      "size": "512m"
    },
    {
      "mountPath": "/tmp",
      "size": "1g"
    }
  ]
}
```

**Characteristics:**
- ⚡ Extremely fast (RAM-based)
- 🔒 Secure (automatically cleared)
- ⚠️ Data lost on restart
- ⚠️ Consumes system RAM

### Tmpfs Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `mountPath` | string | Yes | Container mount path (absolute) |
| `size` | string | No | Maximum size (e.g., "512m", "1g") |

### Use Cases

**1. Build Caches**

```json
{
  "name": "rust-builder",
  "tmpfs": [
    {
      "mountPath": "/tmp/cargo",
      "size": "2g"
    }
  ]
}
```

**2. Test Databases**

```json
{
  "name": "test-db",
  "tmpfs": [
    {
      "mountPath": "/var/lib/postgresql/data",
      "size": "1g"
    }
  ],
  "packages": ["postgresql"]
}
```

**3. Temporary Processing**

```json
{
  "name": "video-processor",
  "tmpfs": [
    {
      "mountPath": "/workspace",
      "size": "4g"
    }
  ]
}
```

## Combined Storage Example

### Full-Stack Application

**Blueprint:** `fullstack-app.json`

```json
{
  "name": "fullstack-app",
  "description": "Complete app with database, code mount, and caching",
  "base": "ubuntu-22.04",
  "ports": [
    "3000:3000",    // Frontend
    "8080:8080",    // Backend
    "5432:5432"     // Database
  ],
  "volumes": [
    {
      "name": "pgdata",
      "mountPath": "/var/lib/postgresql/data"
    },
    {
      "name": "app-uploads",
      "mountPath": "/app/uploads"
    }
  ],
  "bindMounts": [
    {
      "source": "C:\\Users\\demo\\projects\\myapp",
      "target": "/app"
    }
  ],
  "tmpfs": [
    {
      "mountPath": "/tmp",
      "size": "512m"
    },
    {
      "mountPath": "/app/.cache",
      "size": "256m"
    }
  ],
  "wslConfig": "database",
  "packages": [
    "postgresql",
    "nodejs",
    "npm"
  ]
}
```

## Volume Management

### Lifecycle

**1. Creation (automatic):**
```powershell
thresh up postgres-persistent
# Volumes created automatically
```

**2. Persistence:**
```powershell
thresh destroy postgres-persistent -y
# Volumes remain on system
```

**3. Reuse:**
```powershell
thresh up postgres-persistent --name postgres-persistent
# Same volumes automatically reattached
```

**4. Manual Cleanup:**
```powershell
# Remove all unused volumes
docker volume prune

# Remove specific volume
docker volume rm pgdata
```

### Backup & Restore

**Backup volume data:**

```powershell
# Create backup container
wsl -d thresh-postgres-persistent -- tar czf /tmp/backup.tar.gz -C /var/lib/postgresql/data .

# Copy to host
wsl -d thresh-postgres-persistent -- cat /tmp/backup.tar.gz > backup.tar.gz
```

**Restore volume data:**

```powershell
# Copy backup to container
cat backup.tar.gz | wsl -d thresh-postgres-persistent -- tee /tmp/backup.tar.gz

# Extract to volume
wsl -d thresh-postgres-persistent -- tar xzf /tmp/backup.tar.gz -C /var/lib/postgresql/data
```

### Inspect Volumes

**List all volumes:**

```powershell
docker volume ls
```

**Inspect specific volume:**

```powershell
docker volume inspect pgdata
```

**Check volume usage:**

```powershell
wsl -d thresh-postgres-persistent -- df -h /var/lib/postgresql/data
```

## Best Practices

### Performance

✅ **Use volumes for databases:**
```json
{
  "volumes": [
    {
      "name": "pgdata",
      "mountPath": "/var/lib/postgresql/data"
    }
  ]
}
```

❌ **Don't use bind mounts for databases:**
```json
{
  "bindMounts": [
    {
      "source": "C:\\data\\postgres",  // ❌ Slow on WSL2
      "target": "/var/lib/postgresql/data"
    }
  ]
}
```

✅ **Use tmpfs for build caches:**
```json
{
  "tmpfs": [
    {
      "mountPath": "/tmp/build",
      "size": "2g"
    }
  ]
}
```

### Security

✅ **Use read-only mounts for configuration:**
```json
{
  "bindMounts": [
    {
      "source": "/etc/app/config.yaml",
      "target": "/app/config.yaml",
      "readOnly": true
    }
  ]
}
```

✅ **Isolate sensitive data in volumes:**
```json
{
  "volumes": [
    {
      "name": "secrets",
      "mountPath": "/run/secrets"
    }
  ]
}
```

### Organization

✅ **Name volumes descriptively:**
```json
{
  "volumes": [
    {
      "name": "postgres-prod-data",
      "mountPath": "/var/lib/postgresql/data"
    }
  ]
}
```

✅ **Document mount purposes:**
```json
{
  "description": "Volumes: pgdata (database), backups (nightly dumps)",
  "volumes": [...]
}
```

## WSL2-Specific Considerations

### Performance

**Volume performance (best to worst):**

1. 🥇 **Named volumes** - Stored in WSL2 VHDX (native Linux performance)
2. 🥈 **Tmpfs** - RAM-based (fastest but volatile)
3. 🥉 **Bind mounts (Linux paths)** - If mounting /mnt/wsl/*
4. 🔻 **Bind mounts (Windows paths)** - Cross-filesystem penalty

### Path Translation

Windows paths automatically translated in WSL2:

```json
{
  "source": "C:\\Users\\burns\\projects",
  // Becomes: /mnt/c/Users/burns/projects
  "target": "/app"
}
```

### File Permissions

Combine with WSL configuration for proper permissions:

```json
{
  "name": "mysql-persistent",
  "volumes": [
    {
      "name": "mysql-data",
      "mountPath": "/var/lib/mysql"
    }
  ],
  "wslConfig": "database",  // Disables automount, fixes permissions
  "packages": ["mysql-server"]
}
```

See [WSL Configuration](/docs/wsl-configuration) for details.

## Troubleshooting

### Volume Not Found

**Issue:** Volume doesn't exist after recreation

```powershell
# List all volumes
docker volume ls

# Verify volume name matches blueprint
"name": "pgdata"  // Must match exactly
```

### Permission Denied

**Issue:** Container can't write to volume

```powershell
# Check volume permissions
wsl -d thresh-myenv -- ls -la /var/lib/postgresql/data

# Fix ownership (if needed)
wsl -d thresh-myenv -- chown -R postgres:postgres /var/lib/postgresql/data
```

### Bind Mount Not Working

**Issue:** Host directory not accessible

```powershell
# Verify host path exists
Test-Path "C:\Users\demo\projects"

# Check WSL can access (if Windows)
wsl ls /mnt/c/Users/demo/projects

# Try absolute path
"source": "C:\\Users\\demo\\projects"  // Escaped backslashes
```

### Tmpfs Out of Memory

**Issue:** Tmpfs exceeds size limit

```powershell
# Check tmpfs usage
wsl -d thresh-myenv -- df -h /tmp

# Increase size in blueprint
{
  "size": "2g"  // Was 512m
}
```

## Reference

### Blueprint Properties

| Property | Type | Description |
|----------|------|-------------|
| `volumes` | array | Named volumes (persistent) |
| `bindMounts` | array | Host directory mounts |
| `tmpfs` | array | In-memory temporary mounts |

### Volume Schema

```typescript
{
  name: string;        // Unique volume name
  mountPath: string;   // Container path
  readOnly?: boolean;  // Optional read-only
}
```

### Bind Mount Schema

```typescript
{
  source: string;      // Host path
  target: string;      // Container path
  readOnly?: boolean;  // Optional read-only
}
```

### Tmpfs Schema

```typescript
{
  mountPath: string;   // Container path
  size?: string;       // Max size (e.g., "512m")
}
```

### CLI Commands

- `thresh up <blueprint>` - Create with volumes
- `thresh destroy <env>` - Remove environment (keeps volumes)
- `docker volume ls` - List volumes
- `docker volume prune` - Remove unused volumes
- `docker volume rm <name>` - Remove specific volume

### Metadata Storage

Configuration persisted to: `~/.thresh/metadata/{environmentName}.json`

```json
{
  "Volumes": [
    {"name": "pgdata", "mountPath": "/var/lib/postgresql/data"}
  ],
  "BindMounts": [
    {"source": "C:\\projects", "target": "/app"}
  ],
  "Tmpfs": [
    {"mountPath": "/tmp", "size": "512m"}
  ]
}
```

## Next Steps

- [Networking & Port Mapping](/docs/tutorials/networking) - Add network access
- [WSL Configuration](/docs/wsl-configuration) - Fix filesystem permissions
- [Custom Blueprints](/docs/tutorials/custom-blueprints) - Create complex setups
