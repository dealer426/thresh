---
sidebar_position: 8
title: thresh volume
description: Manage persistent volumes
---

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# thresh volume

Manage persistent storage volumes for environments.

## Synopsis

```bash
thresh volume <subcommand> [arguments] [options]
```

## Description

The `volume` command provides tools for managing persistent named volumes that can be attached to environments. Volumes store data that persists even when environments are destroyed and recreated, making them ideal for databases, application data, and configuration files.

### Platform Differences

Volume implementation varies by platform:

**Windows (WSL2):**
- Volumes are directories stored in `%USERPROFILE%\.thresh\volumes\`
- Mounted using bind mounts inside WSL distributions
- Accessible from both Windows and WSL

**Linux/macOS (containerd/nerdctl):**
- Volumes are managed by containerd/nerdctl
- Stored in container runtime's volume directory
- Only accessible from inside containers

## Subcommands

### list

List all created volumes.

```bash
thresh volume list
```

**Output (Windows/WSL):**

```
VOLUME NAME                    DRIVER     MOUNTPOINT
--------------------------------------------------------------------------------
postgres-data                  local      C:\Users\demo\.thresh\volumes\postgres-data
redis-cache                    local      C:\Users\demo\.thresh\volumes\redis-cache
app-config                     local      C:\Users\demo\.thresh\volumes\app-config

Total: 3 volume(s)
```

**Output (Linux):**

```
VOLUME NAME                    DRIVER     MOUNTPOINT
--------------------------------------------------------------------------------
postgres-data                  local      /var/lib/nerdctl/volumes/default/postgres-data
redis-cache                    local      /var/lib/nerdctl/volumes/default/redis-cache

Total: 2 volume(s)
```

### create

Create a new named volume.

```bash
thresh volume create <volume-name>
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `<volume-name>` | Yes | Name of the volume to create |

**Examples:**

<Tabs groupId="operating-systems">
  <TabItem value="windows" label="Windows" default>

```powershell
# Create a volume for PostgreSQL data
PS C:\> thresh volume create postgres-data
✅ Volume 'postgres-data' created successfully

# Verify it was created
PS C:\> dir ~\.thresh\volumes\
postgres-data
```

  </TabItem>
  <TabItem value="linux" label="Linux">

```bash
# Create a volume for PostgreSQL data
$ thresh volume create postgres-data
✅ Volume 'postgres-data' created successfully

# Verify with nerdctl
$ nerdctl volume ls
VOLUME NAME          DIRECTORY
postgres-data        /var/lib/nerdctl/volumes/default/postgres-data
```

  </TabItem>
</Tabs>

:::tip Pre-creating Volumes
While volumes are automatically created when referenced in a blueprint, you can pre-create them to:
- Populate with initial data before provisioning
- Share volumes between multiple environments
- Manage volume lifecycle independently
:::

### delete

Delete a volume permanently.

```bash
thresh volume delete <volume-name>
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `<volume-name>` | Yes | Name of the volume to delete |

**Examples:**

```bash
# Delete a volume
thresh volume delete old-data
✅ Volume 'old-data' deleted successfully
```

:::danger Data Loss
Deleting a volume permanently removes all data stored in it. This operation cannot be undone.
:::

:::warning Volume In Use
If a volume is currently mounted to a running environment, deletion will fail:

```
❌ Failed to delete volume 'postgres-data'
   Make sure no containers are using this volume
```

**Solution:** Stop or destroy environments using the volume first:
```bash
thresh stop postgres-server
thresh volume delete postgres-data
```
:::

### inspect

Show detailed information about a volume.

```bash
thresh volume inspect <volume-name>
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `<volume-name>` | Yes | Name of the volume to inspect |

**Examples:**

<Tabs groupId="operating-systems">
  <TabItem value="windows" label="Windows" default>

```powershell
PS C:\> thresh volume inspect postgres-data
Volume: postgres-data
Driver: local
Mountpoint: C:\Users\demo\.thresh\volumes\postgres-data
Scope: local
Created: 2026-02-15 14:30:22
```

  </TabItem>
  <TabItem value="linux" label="Linux">

```bash
$ thresh volume inspect postgres-data
Volume: postgres-data
Driver: local
Mountpoint: /var/lib/nerdctl/volumes/default/postgres-data
Scope: local
Created: 2026-02-15 14:30:22
Labels:
  com.docker.compose.project: thresh
  com.docker.compose.volume: postgres-data
```

  </TabItem>
</Tabs>

**Volume Not Found:**

```
❌ Volume 'unknown-volume' not found
```

## Usage in Blueprints

Volumes are typically defined in blueprints and automatically created during provisioning:

```json
{
  "name": "postgres-server",
  "base": "ubuntu-22.04",
  "packages": ["postgresql-14"],
  "volumes": [
    {
      "name": "postgres-data",
      "mount": "/var/lib/postgresql/data"
    }
  ]
}
```

When you run `thresh up postgres-server`, the volume is automatically:
1. Created (if it doesn't exist)
2. Mounted at the specified path
3. Available for data persistence

## Common Workflows

### Create and Use Volume

```bash
# Method 1: Pre-create volume
thresh volume create app-data

# Use in blueprint
{
  "volumes": [
    {"name": "app-data", "mount": "/app/data"}
  ]
}

# Method 2: Let thresh create it automatically
thresh up my-blueprint  # Volume created during provisioning
```

### Backup Volume Data

<Tabs groupId="operating-systems">
  <TabItem value="windows" label="Windows" default>

```powershell
# Volumes are just directories on Windows
PS C:\> $volumePath = "$env:USERPROFILE\.thresh\volumes\postgres-data"
PS C:\> Compress-Archive -Path $volumePath -DestinationPath postgres-backup.zip

# Restore
PS C:\> Expand-Archive -Path postgres-backup.zip -DestinationPath $volumePath
```

  </TabItem>
  <TabItem value="linux" label="Linux">

```bash
# Create container to backup volume
$ thresh up backup-helper

# Copy data out
$ wsl -d thresh-backup-helper tar czf /tmp/backup.tar.gz /data
$ wsl -d thresh-backup-helper cat /tmp/backup.tar.gz > backup.tar.gz

# Restore
$ cat backup.tar.gz | wsl -d thresh-backup-helper tar xzf - -C /data
```

  </TabItem>
</Tabs>

### Share Volume Between Environments

```json
// First environment
{
  "name": "postgres-server",
  "volumes": [
    {"name": "shared-data", "mount": "/data"}
  ]
}

// Second environment (shares the same volume)
{
  "name": "postgres-backup",
  "volumes": [
    {"name": "shared-data", "mount": "/backup-source"}
  ]
}
```

### Clean Up Unused Volumes

```bash
# List all volumes
thresh volume list

# Delete volumes no longer needed
thresh volume delete old-project-data
thresh volume delete test-volume
thresh volume delete abandoned-db
```

## Troubleshooting

### Volume Not Mounting (Windows/WSL)

**Problem:**
```
⚠️  Failed to create bind mount
```

**Solution:**
1. Check volume exists:
```powershell
dir ~\.thresh\volumes\<volume-name>
```

2. Ensure environment is stopped:
```powershell
thresh stop my-environment
thresh start my-environment
```

3. Verify WSL can access Windows directory:
```powershell
wsl -d thresh-my-environment ls /mnt/c/Users/YourName/.thresh/volumes/
```

### Volume Deletion Failed

**Problem:**
```
❌ Failed to delete volume 'postgres-data'
   Make sure no containers are using this volume
```

**Solution:**
```bash
# Find environments using the volume
thresh list

# Stop all environments
thresh destroy --all -y

# Try deletion again
thresh volume delete postgres-data
```

### Permission Denied (Linux)

**Problem:**
Volume data is created with wrong ownership.

**Solution:**
Use the `database` WSL config profile to fix permission issues:
```json
{
  "wslConfig": "database",
  "volumes": [
    {"name": "db-data", "mount": "/var/lib/postgresql/data"}
  ]
}
```

See [WSL Configuration Guide](/docs/wsl-configuration) for details.

## Related Commands

- [thresh up](./up.md) - Use volumes in blueprints
- [thresh list](./list.md) - Show environments and their volumes
- [thresh destroy](./destroy.md) - Remove environments (volumes persist)

## Reference

- [Volumes Tutorial](/docs/tutorials/volumes) - Complete volume usage guide
- [Blueprint Format](/docs/tutorials/custom-blueprints) - Volume configuration in blueprints

## Examples

### Example 1: Create Volume for Database

```bash
# Create volume
$ thresh volume create postgres-prod-data
✅ Volume 'postgres-prod-data' created successfully

# Use in blueprint
$ cat postgres-prod.json
{
  "name": "postgres-prod",
  "base": "ubuntu-22.04",
  "packages": ["postgresql-14"],
  "volumes": [
    {
      "name": "postgres-prod-data",
      "mount": "/var/lib/postgresql/data"
    }
  ],
  "wslConfig": "database"
}

# Provision
$ thresh up postgres-prod.json
✓ Environment provisioned successfully
✓ Database data persists in 'postgres-prod-data' volume

# Destroy and recreate - data survives
$ thresh destroy postgres-prod -y
$ thresh up postgres-prod.json
✓ Data restored from existing volume
```

### Example 2: Inspect and List Volumes

```bash
# List all volumes
$ thresh volume list
VOLUME NAME                    DRIVER     MOUNTPOINT
--------------------------------------------------------------------------------
postgres-prod-data            local      ~/.thresh/volumes/postgres-prod-data
redis-cache                   local      ~/.thresh/volumes/redis-cache
app-config                    local      ~/.thresh/volumes/app-config

Total: 3 volume(s)

# Inspect specific volume
$ thresh volume inspect postgres-prod-data
Volume: postgres-prod-data
Driver: local
Mountpoint: /home/user/.thresh/volumes/postgres-prod-data
Scope: local
Created: 2026-02-15 14:30:22

# Check volume size (Linux)
$ du -sh ~/.thresh/volumes/postgres-prod-data
2.4G    /home/user/.thresh/volumes/postgres-prod-data
```

### Example 3: Clean Up Old Volumes

```bash
# List volumes to identify unused ones
$ thresh volume list
VOLUME NAME                    DRIVER     MOUNTPOINT
--------------------------------------------------------------------------------
postgres-prod-data            local      ~/.thresh/volumes/postgres-prod-data
old-test-data                 local      ~/.thresh/volumes/old-test-data
abandoned-project             local      ~/.thresh/volumes/abandoned-project

# Delete unused volumes
$ thresh volume delete old-test-data
✅ Volume 'old-test-data' deleted successfully

$ thresh volume delete abandoned-project
✅ Volume 'abandoned-project' deleted successfully

# Verify
$ thresh volume list
VOLUME NAME                    DRIVER     MOUNTPOINT
--------------------------------------------------------------------------------
postgres-prod-data            local      ~/.thresh/volumes/postgres-prod-data

Total: 1 volume(s)
```

### Example 4: Share Volume Between Environments

```bash
# Create shared volume
$ thresh volume create shared-assets
✅ Volume 'shared-assets' created successfully

# First environment writes to volume
$ cat web-frontend.json
{
  "name": "frontend",
  "base": "node-18",
  "volumes": [
    {"name": "shared-assets", "mount": "/app/build"}
  ]
}

# Second environment reads from same volume
$ cat nginx-server.json
{
  "name": "webserver",
  "base": "ubuntu-22.04",
  "packages": ["nginx"],
  "volumes": [
    {"name": "shared-assets", "mount": "/var/www/html"}
  ]
}

# Both environments share the same volume
$ thresh up web-frontend.json
$ thresh up nginx-server.json
# Files written by frontend are served by nginx
```
