# Thresh Persistent Volumes - User Guide

**Platform**: Windows 11 + WSL2  
**Version**: thresh v1.5.0  
**Date**: February 27, 2026

## Overview

Thresh provides **persistent volumes** that survive environment destruction, allowing you to preserve data across environment lifecycles. Volumes are stored as **Windows directories** in `~/.thresh/volumes/` and automatically mounted into WSL distros.

### Key Benefits

✅ **No Admin Required** - Uses standard Windows directories  
✅ **Data Persistence** - Survives `thresh destroy`  
✅ **Windows Integration** - Files accessible from Explorer, VS Code, etc.  
✅ **Auto-Mounting** - Volumes remount automatically on login  
✅ **Cross-Environment Sharing** - Multiple environments can use the same volume  
✅ **Bidirectional Access** - Changes visible in both Windows and WSL  

---

## Quick Start

### 1. Create a Volume

```bash
thresh volume create my-data
```

Creates: `C:\Users\<username>\.thresh\volumes\my-data\`

### 2. Use Volume in Blueprint

```json
{
  "name": "my-app",
  "base": "ubuntu-22.04",
  "volumes": [
    {
      "name": "my-data",
      "mount": "/data"
    }
  ]
}
```

### 3. Provision Environment

```bash
thresh up my-app --name app-env
```

Volume automatically:
- Created (if doesn't exist)
- Mounted to `/data` in the distro
- Persistent mount script created in `/etc/profile.d/thresh-volumes.sh`

---

## Volume Commands

### List All Volumes

```bash
thresh volume list
```

**Output**:
```
VOLUME NAME        DRIVER     MOUNTPOINT
my-data           local      C:\Users\burns\.thresh\volumes\my-data
postgres-data     local      C:\Users\burns\.thresh\volumes\postgres-data
```

### Inspect Volume

```bash
thresh volume inspect my-data
```

**Output**:
```
Volume: my-data
Driver: local
Mountpoint: C:\Users\burns\.thresh\volumes\my-data
Scope: local
```

### Delete Volume

```bash
thresh volume delete my-data
```

⚠️ **Warning**: This permanently deletes the directory and all data!

---

## Using Volumes in Blueprints

### Single Volume

```json
{
  "name": "postgres-dev",
  "base": "ubuntu-22.04",
  "packages": ["postgresql"],
  "volumes": [
    {
      "name": "postgres-data",
      "mount": "/var/lib/postgresql/data"
    }
  ]
}
```

### Multiple Volumes

```json
{
  "name": "wordpress",
  "base": "ubuntu-22.04",
  "volumes": [
    {
      "name": "mysql-data",
      "mount": "/var/lib/mysql"
    },
    {
      "name": "wordpress-files",
      "mount": "/var/www/html"
    }
  ]
}
```

---

## Example: PostgreSQL with Persistent Data

### Create Environment

```bash
thresh up postgres-persistent --name mydb
```

### Write Data

```bash
wsl -d thresh-mydb
# Inside the distro:
psql -h localhost -U dev -d devdb
CREATE TABLE users (id serial, name text);
INSERT INTO users (name) VALUES ('Alice'), ('Bob');
\q
exit
```

### Destroy Environment

```bash
thresh destroy mydb
```

**Data survives!** Volume remains in `C:\Users\<username>\.thresh\volumes\postgres-data\`

### Recreate with Same Volume

```bash
thresh up postgres-persistent --name mydb-restored
```

Your data is restored! The new environment mounts the existing volume.

---

## How It Works

### Volume Storage

Volumes are plain Windows directories:

```
C:\Users\<username>\.thresh\volumes\
├── postgres-data\
│   ├── PG_VERSION
│   ├── base\
│   └── ...
├── redis-data\
│   ├── dump.rdb
│   └── appendonly.aof
└── my-app-data\
    └── data.json
```

### Automatic Mounting

When thresh provisions an environment with volumes:

1. **Creates volume directory** (if doesn't exist)
   ```
   C:\Users\burns\.thresh\volumes\postgres-data\
   ```

2. **Converts to WSL path**
   ```
   /mnt/c/Users/burns/.thresh/volumes/postgres-data
   ```

3. **Bind mounts to target location**
   ```bash
   mount --bind /mnt/c/Users/burns/.thresh/volumes/postgres-data /var/lib/postgresql/data
   ```

4. **Creates persistent mount script**
   ```bash
   # /etc/profile.d/thresh-volumes.sh
   #!/bin/sh
   [ ! -d '/var/lib/postgresql/data' ] && mkdir -p '/var/lib/postgresql/data'
   ! mountpoint -q '/var/lib/postgresql/data' && \
     mount --bind '/mnt/c/.../postgres-data' '/var/lib/postgresql/data' || true
   ```

### Mount Persistence

The `/etc/profile.d/thresh-volumes.sh` script ensures volumes remount automatically:

- **Interactive login**: `wsl -d thresh-myenv` → Mounts active ✅
- **Login shell**: `wsl -d thresh-myenv sh -l -c "..."` → Mounts active ✅
- **Non-login shell**: `wsl -d thresh-myenv sh -c "..."` → May need manual remount

**Workaround for non-login shells**:
```bash
wsl -d thresh-myenv sh -c "source /etc/profile.d/thresh-volumes.sh && your-command"
```

---

## Built-in Blueprints with Volumes

### MySQL

```bash
thresh up mysql-persistent --name mysql-dev
```

- Volume: `mysql-data` → `/var/lib/mysql`
- User: `dev` / Password: `devpassword`
- Port: 3306

### PostgreSQL

```bash
thresh up postgres-persistent --name postgres-dev
```

- Volume: `postgres-data` → `/var/lib/postgresql/14/main`
- Database: `devdb`
- User: `dev` / Password: `devpassword`
- Port: 5432

### Redis

```bash
thresh up redis-persistent --name redis-dev
```

- Volume: `redis-data` → `/var/lib/redis`
- Persistence: RDB + AOF enabled
- Port: 6379

### MongoDB

```bash
thresh up mongodb-persistent --name mongo-dev
```

- Volume: `mongodb-data` → `/data/db`
- Port: 27017

### WordPress Stack

```bash
thresh up wordpress-stack --name wordpress
```

- Volumes:
  - `mysql-wordpress-data` → `/var/lib/mysql`
  - `wordpress-files` → `/var/www/html`
- Access: http://localhost:8080
- MySQL User: `wpuser` / Password: `wppassword`

---

## Accessing Volume Data from Windows

### Direct File Access

```bash
# Open in Windows Explorer
explorer C:\Users\<username>\.thresh\volumes\my-data

# Edit with VS Code
code C:\Users\<username>\.thresh\volumes\my-data\config.json

# Copy files
cp C:\Users\<username>\.thresh\volumes\my-data\backup.sql .\
```

### Bidirectional Changes

Changes made in Windows appear immediately in WSL:

```bash
# From Windows PowerShell
echo "Hello from Windows" > C:\Users\burns\.thresh\volumes\my-data\test.txt

# From WSL
wsl -d thresh-myenv cat /data/test.txt
# Output: Hello from Windows
```

---

## Volume Backup and Restore

### Backup Volume

```bash
# Compress volume directory
tar -czf postgres-backup-$(date +%Y%m%d).tar.gz \
  -C C:/Users/<username>/.thresh/volumes/ postgres-data
```

### Restore Volume

```bash
# Extract to volumes directory
tar -xzf postgres-backup-20260227.tar.gz \
  -C C:/Users/<username>/.thresh/volumes/
```

### Copy Volume to Another Machine

```bash
# On source machine
robocopy C:\Users\<username>\.thresh\volumes\postgres-data \
         \\network\backup\postgres-data /E

# On destination machine
robocopy \\network\backup\postgres-data \
         C:\Users\<username>\.thresh\volumes\postgres-data /E
```

---

## Advanced: Sharing Volumes Between Environments

Multiple environments can mount the same volume:

```bash
# Environment 1: Writer
thresh up postgres-persistent --name db-primary

# Environment 2: Reader (same volume)
thresh up postgres-persistent --name db-readonly
```

Both environments access the same `postgres-data` volume. Use this for:
- Read replicas
- Development + Testing environments
- Shared data directories

---

## Troubleshooting

### Volume Not Mounted After WSL Restart

**Problem**: Files appear empty after `wsl --terminate`

**Solution**: Use login shell to trigger mount script:
```bash
wsl -d thresh-myenv sh -l -c "ls /data/"
```

Or manually remount:
```bash
wsl -d thresh-myenv sh -c "source /etc/profile.d/thresh-volumes.sh"
```

### Permission Denied

**Problem**: Cannot write to mounted volume

**Solution**: Check directory ownership in distro:
```bash
wsl -d thresh-myenv
sudo chown -R $(whoami):$(whoami) /data
```

### Volume Appears Empty

**Problem**: Volume shows no files in WSL

**Solution**: Verify mount is active:
```bash
wsl -d thresh-myenv mountpoint /data
```

If not mounted, check the mount script:
```bash
wsl -d thresh-myenv cat /etc/profile.d/thresh-volumes.sh
wsl -d thresh-myenv sh /etc/profile.d/thresh-volumes.sh
```

### Windows Path Spaces

**Problem**: Volume path contains spaces

**Solution**: Thresh handles this automatically, but if calling `wsl` manually, quote paths:
```bash
wsl -d thresh-myenv mount --bind "/mnt/c/Users/John Doe/.thresh/volumes/data" /data
```

---

## Best Practices

### 1. Name Volumes by Purpose

✅ **Good**: `postgres-data`, `redis-cache`, `app-logs`  
❌ **Bad**: `data1`, `vol`, `temp`

### 2. One Volume Per Data Type

Separate volumes for different data concerns:
```json
{
  "volumes": [
    {"name": "app-database", "mount": "/var/lib/mysql"},
    {"name": "app-uploads", "mount": "/var/www/uploads"},
    {"name": "app-logs", "mount": "/var/log/app"}
  ]
}
```

### 3. Regular Backups

```bash
# Daily backup script
#!/bin/bash
DATE=$(date +%Y%m%d)
tar -czf ~/backups/postgres-$DATE.tar.gz \
  -C ~/.thresh/volumes/ postgres-data
```

### 4. Clean Up Unused Volumes

```bash
# List volumes
thresh volume list

# Delete unused volumes
thresh volume delete old-data
```

### 5. Document Volume Contents

Create a README in each volume:
```bash
echo "# Postgres Data Volume
Created: 2026-02-27
Environment: production-db
Backup Schedule: Daily 2 AM" > ~/.thresh/volumes/postgres-data/README.md
```

---

## Comparison: VHD vs Directory Volumes

| Feature | VHD Volumes (Old) | Directory Volumes (Current) |
|---------|-------------------|----------------------------|
| Admin Required | ❌ Yes (PowerShell + Hyper-V) | ✅ No |
| Performance | ⚡ Native ext4 | 🚀 9P (~10-20% slower) |
| Windows Access | ⚠️ Must unmount first | ✅ Always accessible |
| Size | 📦 Fixed/Dynamic (8GB default) | 📊 Grows with data |
| Backup | Single .vhdx file | Standard file copy |
| Tooling | Requires mount/unmount | Any Windows tool works |
| Use Case | Enterprise, high I/O | General development ✅ |

**Recommendation**: Directory volumes for 99% of use cases. Only consider VHDs for extreme I/O requirements.

---

## FAQ

**Q: Do volumes require administrator privileges?**  
A: No! Directory-based volumes work without admin rights.

**Q: Where are volumes stored?**  
A: `C:\Users\<username>\.thresh\volumes\<volume-name>\`

**Q: Can I access volume files from Windows?**  
A: Yes! They're regular Windows directories.

**Q: Do volumes survive environment deletion?**  
A: Yes! `thresh destroy` only removes the distro, not volumes.

**Q: Can I use the same volume in multiple environments?**  
A: Yes, but be cautious with concurrent write access.

**Q: How do I backup a volume?**  
A: Copy the directory: `robocopy ~/.thresh/volumes/data D:\backup\data /E`

**Q: What happens if I delete a volume?**  
A: The directory and all data are permanently deleted.

**Q: Do mounts persist across WSL restarts?**  
A: With login shells, yes (via `/etc/profile.d/` script). Non-login shells may need manual remounting.

---

## Next Steps

- **Create your first volume**: `thresh volume create my-project-data`
- **Try a database blueprint**: `thresh up postgres-persistent --name testdb`
- **Check existing volumes**: `thresh volume list`
- **Explore blueprints**: See examples in `build-output/win-x64/blueprints/`

For more examples, see: [Example Blueprints](#built-in-blueprints-with-volumes)
