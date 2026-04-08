---
title: "Stack Patterns: Real-World Multi-Service Deployments with thresh"
description: Practical stack definition patterns for common application architectures — web apps, microservices, data pipelines, and development tooling.
slug: stack-patterns-multi-service
authors: [thresh]
tags: [guide, stacks, patterns, architecture, docker, orchestration]
---

# Stack Patterns: Real-World Multi-Service Deployments

thresh stacks turn multi-service applications into single-command deployments. But what does that look like for real applications? In this post, we cover battle-tested patterns for the most common architectures — from three-tier web apps to microservice meshes to data engineering pipelines.

Every example below is a complete, working stack definition you can save as a `.json` file and deploy with `thresh stack up`.

<!-- truncate -->

## Pattern 1: Three-Tier Web Application

The classic: database, API, frontend. This is the most common pattern and the best place to start.

```json
{
  "name": "three-tier",
  "services": [
    {
      "name": "postgres",
      "image": "postgres:16",
      "ports": ["5432:5432"],
      "volumes": ["pgdata:/var/lib/postgresql/data"],
      "env": {
        "POSTGRES_USER": "app",
        "POSTGRES_PASSWORD": "devpass",
        "POSTGRES_DB": "appdb"
      }
    },
    {
      "name": "api",
      "image": "node:20-alpine",
      "ports": ["3000:3000"],
      "depends_on": ["postgres"],
      "env": {
        "DATABASE_URL": "postgres://app:devpass@postgres:5432/appdb",
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

**Why it works:**
- `depends_on` ensures Postgres is ready before the API starts
- Environment variables wire the services together using container hostnames
- Traefik on the frontend handles reverse-proxy routing automatically
- Named volume `pgdata` persists data across stack restarts

**Deploy:**
```bash
thresh stack up three-tier.json
```

---

## Pattern 2: API + Cache + Worker

For applications that need background processing with a cache layer:

```json
{
  "name": "api-workers",
  "services": [
    {
      "name": "redis",
      "image": "redis:7-alpine",
      "ports": ["6379:6379"]
    },
    {
      "name": "postgres",
      "image": "postgres:16",
      "ports": ["5432:5432"],
      "volumes": ["pgdata:/var/lib/postgresql/data"],
      "env": { "POSTGRES_PASSWORD": "dev" }
    },
    {
      "name": "api",
      "image": "python:3.12-slim",
      "ports": ["8000:8000"],
      "depends_on": ["postgres", "redis"],
      "env": {
        "DATABASE_URL": "postgres://postgres:dev@postgres:5432/postgres",
        "REDIS_URL": "redis://redis:6379/0",
        "CELERY_BROKER": "redis://redis:6379/1"
      }
    },
    {
      "name": "worker",
      "image": "python:3.12-slim",
      "depends_on": ["redis", "postgres"],
      "env": {
        "DATABASE_URL": "postgres://postgres:dev@postgres:5432/postgres",
        "CELERY_BROKER": "redis://redis:6379/1"
      }
    }
  ]
}
```

**Key detail:** The worker service has no `ports` — it doesn't accept inbound traffic. It only consumes from the Redis queue. Both `api` and `worker` depend on `redis` and `postgres`, so all infrastructure starts before either application service.

---

## Pattern 3: Microservices with API Gateway

Multiple services behind a unified gateway:

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
      "name": "user-service",
      "image": "myregistry/users:latest",
      "ports": ["8081:8080"],
      "depends_on": ["rabbitmq"],
      "env": { "AMQP_URL": "amqp://guest:guest@rabbitmq:5672" }
    },
    {
      "name": "order-service",
      "image": "myregistry/orders:latest",
      "ports": ["8082:8080"],
      "depends_on": ["rabbitmq"],
      "env": { "AMQP_URL": "amqp://guest:guest@rabbitmq:5672" }
    },
    {
      "name": "notification-service",
      "image": "myregistry/notifications:latest",
      "depends_on": ["rabbitmq"],
      "env": { "AMQP_URL": "amqp://guest:guest@rabbitmq:5672" }
    },
    {
      "name": "gateway",
      "image": "nginx:alpine",
      "ports": ["80:80"],
      "depends_on": ["user-service", "order-service"],
      "traefik": true
    }
  ]
}
```

**Architecture:**
- RabbitMQ is the shared message bus
- Each service registers independently
- `notification-service` is a pure consumer (no ports)
- The gateway routes external traffic to `user-service` and `order-service`

**Updating a single service:**
```bash
thresh stack update microservices --service order-service --image myregistry/orders:v2.0
```

---

## Pattern 4: Data Engineering Pipeline

ETL pipeline with source database, processing, and analytics:

```json
{
  "name": "data-pipeline",
  "services": [
    {
      "name": "source-db",
      "image": "postgres:16",
      "ports": ["5432:5432"],
      "volumes": ["source-data:/var/lib/postgresql/data"],
      "env": {
        "POSTGRES_DB": "source",
        "POSTGRES_PASSWORD": "dev"
      }
    },
    {
      "name": "redis",
      "image": "redis:7-alpine",
      "ports": ["6379:6379"]
    },
    {
      "name": "etl-worker",
      "image": "python:3.12-slim",
      "depends_on": ["source-db", "redis"],
      "env": {
        "SOURCE_DB": "postgres://postgres:dev@source-db:5432/source",
        "CACHE_URL": "redis://redis:6379/0"
      }
    },
    {
      "name": "analytics-db",
      "image": "postgres:16",
      "ports": ["5433:5432"],
      "volumes": ["analytics-data:/var/lib/postgresql/data"],
      "depends_on": ["etl-worker"],
      "env": {
        "POSTGRES_DB": "analytics",
        "POSTGRES_PASSWORD": "dev"
      }
    },
    {
      "name": "dashboard",
      "image": "metabase/metabase:latest",
      "ports": ["3000:3000"],
      "depends_on": ["analytics-db"],
      "env": {
        "MB_DB_TYPE": "postgres",
        "MB_DB_HOST": "analytics-db",
        "MB_DB_PORT": "5432",
        "MB_DB_DBNAME": "analytics",
        "MB_DB_PASS": "dev"
      }
    }
  ]
}
```

**Flow:** Source DB → ETL Worker → Analytics DB → Metabase Dashboard

Notice `analytics-db` uses host port **5433** to avoid conflicting with `source-db` on 5432.

---

## Pattern 5: Development Tooling Stack

A stack of developer tools — useful for onboarding or standardizing team environments:

```json
{
  "name": "dev-tools",
  "services": [
    {
      "name": "gitea",
      "image": "gitea/gitea:latest",
      "ports": ["3000:3000", "2222:22"],
      "volumes": ["gitea-data:/data"]
    },
    {
      "name": "registry",
      "image": "registry:2",
      "ports": ["5000:5000"],
      "volumes": ["registry-data:/var/lib/registry"]
    },
    {
      "name": "minio",
      "image": "minio/minio:latest",
      "ports": ["9000:9000", "9001:9001"],
      "volumes": ["minio-data:/data"],
      "env": {
        "MINIO_ROOT_USER": "minioadmin",
        "MINIO_ROOT_PASSWORD": "minioadmin"
      }
    }
  ]
}
```

No `depends_on` needed — these services are independent. They'll all start in parallel for fastest deployment.

---

## Tips & Best Practices

### 1. Keep Stack Definitions in Your Repo

Store `.json` stack files alongside your application code. They become part of your versioned infrastructure:

```
my-app/
├── src/
├── stack.json
└── README.md
```

### 2. Use Rolling Updates for Zero-Downtime

When you build a new container image, update just that service:

```bash
thresh stack update my-app --service api --image myregistry/api:$(git rev-parse --short HEAD)
```

### 3. Separate Data Services from App Services

Data services (Postgres, Redis, RabbitMQ) rarely change images. Application services change frequently. Structure your dependencies so data services are at the bottom of the graph — they start first and get updated last.

### 4. Use Hub for Team Stacks

Deploy shared development stacks through the Hub so everyone on the team can see what's running:

```bash
thresh stack up team-env.json --hub https://hub.internal:7200
```

### 5. Name Your Volumes

Always use named volumes instead of anonymous ones. This makes it clear what data belongs to what service and lets you clean up selectively.

---

## What's Next

These patterns cover most real-world use cases, but v2.0 will bring stack templates — parameterized definitions that let you define a pattern once and instantiate it with different configurations. Stay tuned.

**Happy stacking!** 📦

---

- [Stack CLI Reference](/docs/cli-reference/stack)
- [Stacks Tutorial](/docs/tutorials/stacks)
- [Fleet Management Tutorial](/docs/tutorials/fleet-management)
