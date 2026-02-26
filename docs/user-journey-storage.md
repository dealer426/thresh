# User Journey: Adding Storage to Thresh Blueprints

## Journey 1: Manual Blueprint Creation 👨‍💻

### Step 1: Create Your Blueprint File
You want a MongoDB environment with persistent data storage.

```bash
# Navigate to blueprints directory
cd ~/thresh/thresh/Thresh/blueprints

# Create your blueprint
nano mongodb-dev.json
```

### Step 2: Write the Blueprint with Storage

```json
{
  "name": "mongodb-dev",
  "description": "MongoDB with persistent data volume",
  "base": "ubuntu-22.04",
  "ports": ["27017:27017"],
  "volumes": [
    {
      "name": "mongodb-data",
      "mount": "/data/db"
    }
  ],
  "packages": [
    "mongodb-server",
    "mongodb-clients"
  ]
}
```

**What you're specifying:**
- `"name": "mongodb-data"` - The volume name (Docker will create it)
- `"mount": "/data/db"` - Where MongoDB expects its data

### Step 3: Rebuild Thresh (to include new blueprint)

```bash
cd ~/thresh/thresh/Thresh
dotnet publish -c Release -r linux-x64 -o ../../build-output/linux-x64
```

### Step 4: Use Your Blueprint

```bash
# Provision the environment
sudo ./build-output/linux-x64/thresh up mongodb-dev

# Output:
# Loading bundled blueprint: mongodb-dev
# 
# Blueprint: mongodb-dev
# Description: MongoDB with persistent data volume
# Base: ubuntu-22.04
# 
# Creating environment 'mongodb-dev' from blueprint 'mongodb-dev'
# [1/5] Installing base distribution: ubuntu-22.04
#   ✅ Base distribution installed
# [2/5] Installing packages (2 packages)...
#   ✅ Packages installed
# ...
# ✅ Environment 'mongodb-dev' provisioned successfully!
```

### Step 5: Verify the Storage is Working

```bash
# Check that volume was created
sudo ./build-output/linux-x64/thresh volume list

# Output:
# VOLUME NAME                    DRIVER     MOUNTPOINT
# ------------------------------------------------------------------------
# mongodb-data                   local      /var/lib/docker/volumes/mongodb-data/_data

# Check container has volume mounted
sudo docker inspect thresh-mongodb-dev --format '{{json .Mounts}}' | python3 -m json.tool

# Output:
# [
#     {
#         "Type": "volume",
#         "Name": "mongodb-data",
#         "Source": "/var/lib/docker/volumes/mongodb-data/_data",
#         "Destination": "/data/db",
#         "Driver": "local",
#         "RW": true
#     }
# ]
```

### Step 6: Test Data Persistence

```bash
# Write some data
sudo docker exec thresh-mongodb-dev bash -c "echo 'test data' > /data/db/test.txt"

# Verify it's there
sudo docker exec thresh-mongodb-dev cat /data/db/test.txt
# Output: test data

# Destroy the container
sudo ./build-output/linux-x64/thresh destroy mongodb-dev

# Check volume is still there
sudo ./build-output/linux-x64/thresh volume list
# Output: mongodb-data (still exists!)

# Recreate with same blueprint
sudo ./build-output/linux-x64/thresh up mongodb-dev

# Data survived!
sudo docker exec thresh-mongodb-dev cat /data/db/test.txt
# Output: test data
```

---

## Journey 2: AI-Generated Blueprint with Storage 🤖

### Step 1: Ask AI to Generate Blueprint

```bash
sudo ./build-output/linux-x64/thresh blueprint generate "PostgreSQL database with persistent data storage on port 5433"
```

### Step 2: Review AI-Generated Blueprint

```
Generating blueprint with GitHub Copilot...

Generated Blueprint:
{
  "name": "postgresql-persistent",
  "description": "PostgreSQL database with persistent data storage",
  "base": "ubuntu-22.04",
  "ports": ["5433:5432"],
  "volumes": [
    {
      "name": "postgresql-data",
      "mount": "/var/lib/postgresql/data"
    }
  ],
  "packages": [
    "postgresql",
    "postgresql-contrib"
  ],
  "environment": {
    "POSTGRES_PASSWORD": "postgres"
  }
}

💡 AI automatically included:
   ✅ volume for /var/lib/postgresql/data (where Postgres stores data)
   ✅ Port 5433 mapped to container port 5432
   ✅ Environment variable for database password

Save this blueprint? [y/n]
```

### Step 3: Save and Use

```bash
# Save when prompted
y

# Output:
# 💾 Blueprint saved: postgresql-persistent.json
#    Available in: thresh blueprint list
#    To provision: thresh up postgresql-persistent

# Use it immediately
sudo ./build-output/linux-x64/thresh up postgresql-persistent
```

---

## Journey 3: Interactive Chat Mode for Complex Storage 💬

### Step 1: Start Chat Mode

```bash
sudo ./build-output/linux-x64/thresh chat
```

### Step 2: Describe Your Needs

```
🤖 AI: How can I help you build your environment?

👤 You: I need a web application environment with:
       - Node.js for the app
       - PostgreSQL for the database
       - Redis for caching
       - My source code mounted from /home/sburns/myapp
       - Persistent storage for database and uploads
       - Fast cache that doesn't need to persist

🤖 AI: I'll create a blueprint with:
       - Named volumes for PostgreSQL data and uploads
       - Bind mount for your source code (live editing)
       - Tmpfs for Redis cache (in-memory, fast)
       - All necessary ports mapped

Here's the blueprint:

{
  "name": "fullstack-dev",
  "description": "Full-stack web app with DB and cache",
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
  "tmpfs": [
    "/var/lib/redis"
  ],
  "packages": [
    "nodejs",
    "npm",
    "postgresql",
    "redis-server"
  ]
}

👤 You: Perfect! Save it as fullstack-dev

🤖 AI: ✅ Saved as fullstack-dev.json

       Usage:
       thresh up fullstack-dev

       This will:
       - Create 2 persistent volumes (postgres-data, app-uploads)
       - Mount your code from /home/sburns/myapp → /app (live editing!)
       - Use in-memory storage for Redis (fast cache)
       - Map ports 3000, 5432, 6379 to localhost
```

---

## Journey 4: Modifying Existing Blueprint 📝

### You want to add storage to an existing blueprint

```bash
# List available blueprints
sudo ./build-output/linux-x64/thresh blueprint list

# Output:
# Available blueprints:
#   alpine-minimal       - Alpine Linux minimal environment
#   node-dev             - Node.js development environment
#   python-dev           - Python development environment
#   ubuntu-dev           - Ubuntu development environment
```

### Add storage to python-dev

```bash
# Edit the blueprint
nano ~/thresh/thresh/Thresh/blueprints/python-dev.json
```

**Before:**
```json
{
  "name": "python-dev",
  "description": "Python development environment",
  "base": "ubuntu-22.04",
  "packages": ["python3", "python3-pip", "python3-venv"]
}
```

**After (add volumes for your ML models):**
```json
{
  "name": "python-dev",
  "description": "Python development environment",
  "base": "ubuntu-22.04",
  "volumes": [
    {
      "name": "python-models",
      "mount": "/models"
    },
    {
      "name": "python-data",
      "mount": "/data"
    }
  ],
  "bind_mounts": [
    {
      "host": "/home/sburns/notebooks",
      "container": "/workspace",
      "readonly": false
    }
  ],
  "packages": ["python3", "python3-pip", "python3-venv"]
}
```

**Rebuild and test:**
```bash
# Rebuild
cd ~/thresh/thresh/Thresh
dotnet publish -c Release -r linux-x64 -o ../../build-output/linux-x64

# Use updated blueprint
sudo ./build-output/linux-x64/thresh up python-dev

# Your notebooks are now at /workspace inside the container!
# Models and data persist in Docker volumes!
```

---

## Journey 5: Pre-creating Volumes 🎯

### Sometimes you want to create volumes before provisioning

```bash
# Create volumes manually first
sudo ./build-output/linux-x64/thresh volume create ml-models
sudo ./build-output/linux-x64/thresh volume create training-data
sudo ./build-output/linux-x64/thresh volume create experiment-results

# Check they exist
sudo ./build-output/linux-x64/thresh volume list

# Output:
# VOLUME NAME                    DRIVER     MOUNTPOINT
# ------------------------------------------------------------------------
# ml-models                      local      /var/lib/docker/volumes/ml-models/_data
# training-data                  local      /var/lib/docker/volumes/training-data/_data
# experiment-results             local      /var/lib/docker/volumes/experiment-results/_data

# Now create blueprint that uses them
nano ~/thresh/thresh/Thresh/blueprints/ml-training.json
```

```json
{
  "name": "ml-training",
  "description": "ML training environment with pre-created volumes",
  "base": "ubuntu-22.04",
  "volumes": [
    {
      "name": "ml-models",
      "mount": "/models"
    },
    {
      "name": "training-data",
      "mount": "/data"
    },
    {
      "name": "experiment-results",
      "mount": "/results"
    }
  ],
  "packages": ["python3", "python3-pip", "cuda-toolkit"]
}
```

**When you provision, it uses your pre-created volumes!**

---

## Common User Scenarios 🎬

### 1. Database Development
```json
"volumes": [
  {
    "name": "db-data",
    "mount": "/var/lib/postgresql/data"
  }
]
```

### 2. Live Code Editing
```json
"bind_mounts": [
  {
    "host": "/home/sburns/myproject",
    "container": "/workspace",
    "readonly": false
  }
]
```

### 3. Shared Config Files (Read-Only)
```json
"bind_mounts": [
  {
    "host": "/home/sburns/.aws",
    "container": "/root/.aws",
    "readonly": true
  }
]
```

### 4. Fast Temporary Storage
```json
"tmpfs": ["/tmp", "/var/cache/app"]
```

### 5. Multiple Types Together
```json
{
  "volumes": [
    {"name": "db-data", "mount": "/var/lib/postgresql/data"}
  ],
  "bind_mounts": [
    {"host": "/home/sburns/code", "container": "/app", "readonly": false}
  ],
  "tmpfs": ["/tmp"]
}
```

---

## Quick Reference Card 📋

```bash
# Volume management
thresh volume list                 # See all volumes
thresh volume create my-vol        # Create volume
thresh volume inspect my-vol       # View details
thresh volume delete my-vol        # Remove volume

# Blueprint management
thresh blueprint list              # See available blueprints
thresh blueprint generate "..."    # AI generates blueprint
thresh chat                        # Interactive AI mode

# Using blueprints with storage
thresh up my-blueprint             # Volumes auto-created
thresh destroy my-blueprint        # Container removed, volumes kept
thresh up my-blueprint             # Reattaches same volumes (data persists!)
```

---

## Tips 💡

1. **Volume names persist across containers** - Use descriptive names
2. **Bind mounts need absolute paths** - `/home/user/code`, not `~/code`
3. **Tmpfs is fast but temporary** - Great for caches, not for data
4. **AI knows common patterns** - Just describe what you need
5. **Volumes survive destroy** - Your data is safe!
6. **Rebuild after editing blueprints** - Required for bundled blueprints

