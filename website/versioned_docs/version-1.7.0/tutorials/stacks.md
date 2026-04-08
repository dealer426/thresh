---
sidebar_position: 8
title: Deploying Multi-Service Stacks
description: Deploy complete multi-service applications from a single JSON definition file with dependency ordering and rolling updates
---

# Deploying Multi-Service Stacks

**Version:** 1.7.0  
**Time:** 20 minutes  
**Difficulty:** Intermediate

## Overview

thresh stacks let you deploy multi-service applications from a single JSON definition file. Instead of manually provisioning each service, define your entire application — databases, APIs, frontends, reverse proxies — in one file and bring it all up with one command.

:::tip When to Use Stacks
Use stacks when your application has multiple services that need to run together. For single-service environments, `thresh up` is simpler. For complex multi-service apps, stacks handle dependency ordering, environment injection, and lifecycle management automatically.
:::

## Prerequisites

- thresh v1.7.0 or later installed
- Container runtime available (WSL2 on Windows, Docker on Linux, containerd on macOS)
- Optionally, a running Thresh Hub for remote orchestration

## Your First Stack

### Step 1: Create a Stack Definition

Create a file called `webapp.json`:

```json
{
  "name": "webapp",
  "services": [
    {
      "name": "postgres",
      "image": "postgres:16",
      "ports": ["5432:5432"],
      "volumes": ["pgdata:/var/lib/postgresql/data"],
      "env": {
        "POSTGRES_USER": "webapp",
        "POSTGRES_PASSWORD": "devpass123",
        "POSTGRES_DB": "webapp_dev"
      }
    },
    {
      "name": "redis",
      "image": "redis:7-alpine",
      "ports": ["6379:6379"]
    },
    {
      "name": "api",
      "image": "node:20-alpine",
      "ports": ["3000:3000"],
      "depends_on": ["postgres", "redis"],
      "env": {
        "DATABASE_URL": "postgres://webapp:devpass123@postgres:5432/webapp_dev",
        "REDIS_URL": "redis://redis:6379",
        "NODE_ENV": "development"
      }
    },
    {
      "name": "web",
      "image": "nginx:alpine",
      "ports": ["8080:80"],
      "depends_on": ["api"],
      "traefik": true
    }
  ]
}
```

### Step 2: Deploy the Stack

```bash
thresh stack up webapp.json
```

thresh will:
1. Parse the JSON definition
2. Resolve the dependency graph (postgres → redis → api → web)
3. Pull images in parallel
4. Start services in dependency order
5. Inject Traefik reverse-proxy for the `web` service

### Step 3: Verify

```bash
thresh stack list
```

```
Name       Services   Status    Created
─────────────────────────────────────────────
webapp     4          Running   30 seconds ago
```

```bash
thresh stack info webapp
```

```
Stack: webapp
─────────────────────────────────────────────
Service     Image               Status     Ports
postgres    postgres:16         Running    5432:5432
redis       redis:7-alpine      Running    6379:6379
api         node:20-alpine      Running    3000:3000
web         nginx:alpine        Running    8080:80
```

---

## Stack Definition Reference

### Service Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | ✅ | Unique service name within the stack |
| `image` | string | ✅ | Container image to use |
| `ports` | string[] | ❌ | Port mappings (`host:container`) |
| `volumes` | string[] | ❌ | Volume mounts (`name:path`) |
| `env` | object | ❌ | Environment variables |
| `depends_on` | string[] | ❌ | Services that must start first |
| `traefik` | boolean | ❌ | Auto-inject Traefik reverse proxy |

### Dependency Ordering

The `depends_on` field creates a directed acyclic graph (DAG). thresh performs a topological sort to determine startup order:

```mermaid
graph TD
    DB[postgres] --> API[api]
    CACHE[redis] --> API
    API --> WEB[web]
```

- Services with no dependencies start first (in parallel)
- A service won't start until all its dependencies are running
- Circular dependencies are detected and rejected with a clear error

### Environment Variables

Environment variables are injected at container startup:

```json
{
  "env": {
    "DATABASE_URL": "postgres://user:pass@postgres:5432/mydb",
    "LOG_LEVEL": "debug",
    "PORT": "3000"
  }
}
```

Service names resolve to container hostnames within the stack network, so `postgres` in the DATABASE_URL refers to the postgres service container.

### Traefik Auto-Deploy

When `"traefik": true` is set on a service, thresh automatically:
1. Deploys a Traefik reverse-proxy container (if not already running)
2. Configures routing rules for the service
3. Handles SSL termination (if configured)

---

## Rolling Updates

Update a single service's image without redeploying the entire stack:

```bash
# Update the API to a new version
thresh stack update webapp --service api --image node:22-alpine
```

This:
1. Stops the old `api` container
2. Pulls the new image
3. Starts a new container with the same configuration
4. Other services remain untouched

### Zero-Downtime Pattern

For production-like environments, you can combine rolling updates with health checks:

```bash
# Update API image
thresh stack update webapp --service api --image myregistry/api:v2.1

# Verify the update
thresh stack info webapp
```

---

## Lifecycle Management

### Stop a Stack (Preserve Data)

```bash
thresh stack down webapp
```

Stops all containers but keeps volumes intact. Next `thresh stack up webapp.json` will reuse existing data.

### Destroy a Stack (Remove Everything)

```bash
# Interactive confirmation
thresh stack destroy webapp

# Skip confirmation
thresh stack destroy webapp --yes
```

Stops all containers and removes all associated volumes.

---

## Hub Integration

All stack commands support `--hub` for remote orchestration through Thresh Hub:

```bash
# Deploy via Hub
thresh stack up webapp.json --hub https://your-hub:7200

# List stacks across your fleet
thresh stack list --hub https://your-hub:7200

# Rolling update through Hub
thresh stack update webapp --service api --image node:22-alpine --hub https://your-hub:7200

# Remote teardown
thresh stack destroy webapp --yes --hub https://your-hub:7200
```

When using `--hub`, the command is routed through the Hub's mid-tier layer to the target node's agent, which executes the stack operation locally.

---

## Real-World Examples

### Full-Stack Web App

```json
{
  "name": "fullstack",
  "services": [
    {
      "name": "postgres",
      "image": "postgres:16",
      "ports": ["5432:5432"],
      "volumes": ["pgdata:/var/lib/postgresql/data"],
      "env": { "POSTGRES_PASSWORD": "dev" }
    },
    {
      "name": "api",
      "image": "myregistry/api:latest",
      "ports": ["8080:8080"],
      "depends_on": ["postgres"],
      "env": { "DATABASE_URL": "postgres://postgres:dev@postgres:5432/postgres" }
    },
    {
      "name": "frontend",
      "image": "myregistry/web:latest",
      "ports": ["3000:3000"],
      "depends_on": ["api"],
      "traefik": true
    }
  ]
}
```

### Microservices with Message Queue

```json
{
  "name": "microservices",
  "services": [
    {
      "name": "rabbitmq",
      "image": "rabbitmq:3-management",
      "ports": ["5672:5672", "15672:15672"]
    },
    {
      "name": "order-service",
      "image": "myregistry/orders:latest",
      "ports": ["8081:8080"],
      "depends_on": ["rabbitmq"],
      "env": { "AMQP_URL": "amqp://guest:guest@rabbitmq:5672" }
    },
    {
      "name": "inventory-service",
      "image": "myregistry/inventory:latest",
      "ports": ["8082:8080"],
      "depends_on": ["rabbitmq"],
      "env": { "AMQP_URL": "amqp://guest:guest@rabbitmq:5672" }
    },
    {
      "name": "gateway",
      "image": "nginx:alpine",
      "ports": ["80:80"],
      "depends_on": ["order-service", "inventory-service"],
      "traefik": true
    }
  ]
}
```

---

## Troubleshooting

### Service Won't Start

**Problem:** A service fails to start because a dependency isn't ready yet.

**Solution:** thresh waits for dependency containers to be in `Running` state. If a dependency crashes, dependent services won't start. Check the dependency first:

```bash
thresh stack info my-stack
```

### Circular Dependency Detected

**Problem:** `Error: Circular dependency detected: api → auth → api`

**Solution:** Refactor your dependency graph. Extract the shared concern into a separate service, or remove one direction of the dependency.

### Port Conflict

**Problem:** `Error: Port 5432 is already in use`

**Solution:** Stop the conflicting service or use a different host port:

```json
"ports": ["5433:5432"]  // Map to host port 5433 instead
```

---

## Next Steps

- [Stack CLI Reference](/docs/cli-reference/stack) — Complete command documentation
- [Fleet Management Tutorial](/docs/tutorials/fleet-management) — Remote orchestration via Thresh Hub
- [Networking Tutorial](/docs/tutorials/networking) — Deep dive on port mapping
- [Volume Tutorial](/docs/tutorials/volumes) — Persistent storage patterns
