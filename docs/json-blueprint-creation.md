# How to Create a Blueprint with Storage JSON

## Method 1: Direct File Creation

### Step 1: Navigate to blueprints directory
```bash
cd ~/thresh/thresh/Thresh/blueprints
```

### Step 2: Create your JSON file
```bash
nano my-environment.json
```

### Step 3: Write your JSON
```json
{
  "name": "my-environment",
  "description": "My custom environment with storage",
  "base": "ubuntu-22.04",
  "volumes": [
    {
      "name": "my-data",
      "mount": "/data"
    }
  ],
  "packages": ["nodejs"]
}
```

### Step 4: Rebuild thresh (embeds blueprint)
```bash
cd ~/thresh/thresh/Thresh
dotnet publish -c Release -r linux-x64 -o ../../build-output/linux-x64
```

### Step 5: Use it
```bash
sudo ./build-output/linux-x64/thresh up my-environment
```

---

## Method 2: Using thresh save command (future feature)

```bash
# Create JSON file anywhere
cat > /tmp/my-blueprint.json << 'JSON'
{
  "name": "redis-dev",
  "base": "ubuntu-22.04",
  "ports": ["6379:6379"],
  "volumes": [
    {
      "name": "redis-data",
      "mount": "/data"
    }
  ]
}
JSON

# Save it to thresh
thresh blueprint save /tmp/my-blueprint.json
```

---

## Method 3: Direct JSON in Commands (future feature)

```bash
# Pass JSON directly
thresh up --blueprint '{
  "name": "quick-test",
  "base": "alpine",
  "volumes": [{"name": "test-data", "mount": "/data"}]
}'
```

---

## Real Examples

### Example 1: PostgreSQL Database
```json
{
  "name": "postgres-dev",
  "description": "PostgreSQL with persistent storage",
  "base": "ubuntu-22.04",
  "ports": ["5432:5432"],
  "volumes": [
    {
      "name": "postgres-data",
      "mount": "/var/lib/postgresql/data"
    }
  ],
  "packages": ["postgresql", "postgresql-contrib"],
  "environment": {
    "POSTGRES_PASSWORD": "postgres"
  }
}
```

### Example 2: Web Development
```json
{
  "name": "nodejs-web",
  "description": "Node.js with live code editing",
  "base": "ubuntu-22.04",
  "ports": ["3000:3000"],
  "bind_mounts": [
    {
      "host": "/home/sburns/myapp",
      "container": "/app",
      "readonly": false
    }
  ],
  "volumes": [
    {
      "name": "node-modules",
      "mount": "/app/node_modules"
    }
  ],
  "packages": ["nodejs", "npm"]
}
```

### Example 3: Machine Learning
```json
{
  "name": "ml-training",
  "description": "ML environment with datasets and models",
  "base": "ubuntu-22.04",
  "bind_mounts": [
    {
      "host": "/home/sburns/datasets",
      "container": "/datasets",
      "readonly": true
    },
    {
      "host": "/home/sburns/notebooks",
      "container": "/notebooks",
      "readonly": false
    }
  ],
  "volumes": [
    {
      "name": "ml-models",
      "mount": "/models"
    },
    {
      "name": "training-results",
      "mount": "/results"
    }
  ],
  "tmpfs": ["/tmp"],
  "packages": ["python3", "python3-pip"]
}
```

### Example 4: Multi-Service App
```json
{
  "name": "fullstack-app",
  "description": "Complete web app with database and cache",
  "base": "ubuntu-22.04",
  "ports": [
    "3000:3000",
    "5432:5432",
    "6379:6379"
  ],
  "volumes": [
    {
      "name": "postgres-data",
      "mount": "/var/lib/postgresql/data"
    },
    {
      "name": "redis-data",
      "mount": "/var/lib/redis"
    },
    {
      "name": "app-uploads",
      "mount": "/app/uploads"
    }
  ],
  "bind_mounts": [
    {
      "host": "/home/sburns/myapp",
      "container": "/app",
      "readonly": false
    }
  ],
  "tmpfs": ["/tmp", "/var/cache"],
  "packages": [
    "nodejs",
    "npm",
    "postgresql",
    "postgresql-contrib",
    "redis-server"
  ]
}
```

---

## JSON Schema Reference

```typescript
interface Blueprint {
  name: string;                          // Required: blueprint identifier
  description?: string;                  // Optional: human-readable description
  base: string;                          // Required: base distribution (ubuntu-22.04, alpine, debian)
  
  // Storage options (all optional)
  volumes?: Array<{
    name: string;                        // Volume name in Docker
    mount: string;                       // Container mount path (absolute)
  }>;
  
  bind_mounts?: Array<{
    host: string;                        // Host path (absolute, must exist)
    container: string;                   // Container path (absolute)
    readonly: boolean;                   // true = read-only, false = read-write
  }>;
  
  tmpfs?: string[];                      // Array of paths to mount as tmpfs
  
  // Other options
  ports?: string[];                      // Port mappings: "host:container"
  expose?: number[];                     // Exposed ports (no host mapping)
  packages?: string[];                   // Packages to install
  environment?: { [key: string]: string };  // Environment variables
}
```

---

## Tips for Writing JSON Blueprints

1. **Use absolute paths for bind mounts**
   ✅ `"/home/sburns/code"`
   ❌ `"~/code"` or `"./code"`

2. **Mount paths are container-side**
   - Volumes: Just the mount point (Docker manages storage)
   - Bind mounts: Need both host and container paths

3. **Volume names persist**
   - Once created, volumes keep their name
   - Multiple blueprints can share volumes

4. **Readonly matters for security**
   - Config files: `"readonly": true`
   - Code you're editing: `"readonly": false`

5. **Tmpfs is in-memory**
   - Fast but data lost on stop
   - Good for caches, not for important data

6. **Test with minimal JSON first**
   ```json
   {
     "name": "test",
     "base": "alpine",
     "volumes": [{"name": "test-vol", "mount": "/data"}]
   }
   ```

---

## Common Patterns

### Database with Backup Directory
```json
{
  "volumes": [
    {"name": "db-data", "mount": "/var/lib/postgresql/data"}
  ],
  "bind_mounts": [
    {"host": "/backups/postgres", "container": "/backups", "readonly": false}
  ]
}
```

### Development with Shared Dependencies
```json
{
  "bind_mounts": [
    {"host": "/home/user/code", "container": "/workspace", "readonly": false}
  ],
  "volumes": [
    {"name": "cargo-cache", "mount": "/usr/local/cargo/registry"}
  ]
}
```

### High-Performance Cache
```json
{
  "tmpfs": ["/var/cache/app", "/tmp"]
}
```
