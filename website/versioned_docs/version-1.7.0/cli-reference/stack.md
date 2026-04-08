---
sidebar_position: 15
title: thresh stack
description: Deploy and manage multi-service stacks on your fleet nodes via Thresh Hub
---

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# thresh stack

:::info New in v1.7.0
The `stack` command group is new in thresh 1.7.0.
:::

Deploy and manage **multi-service stacks** on your fleet nodes via Thresh Hub. Define your services in a JSON file, and thresh orchestrates the deployment — pulling images, resolving dependencies, injecting environment variables, and optionally deploying Traefik as a reverse proxy.

## Prerequisites

- A running **Thresh Hub** instance
- At least one agent connected to the hub (`thresh agent start`)
- Authenticated CLI session (`thresh auth login`)

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`stack up`](#thresh-stack-up) | Deploy a stack from a JSON definition |
| [`stack down`](#thresh-stack-down) | Stop a running stack (keeps volumes) |
| [`stack destroy`](#thresh-stack-destroy) | Stop a stack and remove all volumes |
| [`stack list`](#thresh-stack-list) | List all stacks in your account |
| [`stack info`](#thresh-stack-info) | Show per-service status for a stack |
| [`stack update`](#thresh-stack-update) | Rolling update — change a service image |

---

## thresh stack up

Deploy a stack from a JSON definition file. The hub resolves `depends_on` ordering (topological sort), injects `${service.host}` / `${service.port}` environment variables, and dispatches the deployment to an online agent.

### Synopsis

```bash
thresh stack up <file.json> [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `file` | Path to the stack definition JSON file |

### Options

| Option | Description |
|--------|-------------|
| `--hub <url>` | Hub URL (overrides stored credentials) |

### Stack Definition Format

```json
{
  "name": "my-stack",
  "services": {
    "db": {
      "image": "docker:postgres:16-alpine",
      "ports": ["5432:5432"],
      "env": {
        "POSTGRES_DB": "myapp",
        "POSTGRES_USER": "admin",
        "POSTGRES_PASSWORD": "secret"
      }
    },
    "app": {
      "image": "docker:my-org/my-app:latest",
      "ports": ["8080:8080"],
      "depends_on": ["db"],
      "env": {
        "DATABASE_HOST": "${db.host}",
        "DATABASE_PORT": "${db.port}"
      }
    }
  },
  "traefik": true
}
```

### Key Features

- **`depends_on` ordering** — services deploy in topological order (db before app)
- **`${service.host}` / `${service.port}` injection** — resolved at deploy time by the hub
- **OCI/Docker images** — prefix with `docker:` to pull from registries
- **Traefik auto-deploy** — set `"traefik": true` to deploy Traefik v3.3 with dynamic routing

### Example

```bash
thresh stack up ./mystack.json
```

```
🚀 Stack 'my-stack' deploying to node thresh-node-1
   db ........... deploying → running
   app .......... deploying → running
✅ Stack 'my-stack' deployed (2 services)
```

---

## thresh stack down

Stop all services in a running stack. Volumes are preserved so data is retained.

### Synopsis

```bash
thresh stack down <name> [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Name of the stack to stop |

### Example

```bash
thresh stack down my-stack
```

```
⏹  Stack 'my-stack' stopped
```

---

## thresh stack destroy

Stop all services and remove all associated volumes. **This is destructive** — all data in stack volumes will be lost.

### Synopsis

```bash
thresh stack destroy <name> [--hub <url>] [--yes|-y]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Name of the stack to destroy |

### Options

| Option | Description |
|--------|-------------|
| `--yes`, `-y` | Skip confirmation prompt |
| `--hub <url>` | Hub URL (overrides stored credentials) |

### Example

```bash
thresh stack destroy my-stack
```

```
Destroy stack 'my-stack' and remove all volumes? [y/N] y
💥 Stack 'my-stack' destroyed
```

---

## thresh stack list

List all stacks deployed to your account.

### Synopsis

```bash
thresh stack list [--hub <url>]
```

### Example

```bash
thresh stack list
```

```
NAME         NODE           SERVICES  STATUS    CREATED
my-stack     thresh-node-1  3         running   2 hours ago
dev-env      thresh-node-3  2         stopped   3 days ago
```

---

## thresh stack info

Show detailed per-service status for a stack, including image, ports, and individual service state.

### Synopsis

```bash
thresh stack info <name> [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Name of the stack to inspect |

### Example

```bash
thresh stack info my-stack
```

```
Stack: my-stack
Node:  thresh-node-1
Status: running

SERVICE  IMAGE                        STATUS    PORTS
db       docker:postgres:16-alpine    running   5432:5432
app      docker:my-org/my-app:latest  running   8080:8080
traefik  docker:traefik:v3.3          running   80:80, 443:443
```

---

## thresh stack update

Perform a rolling update by changing the image for a single service. The hub dispatches the update to the target node, which pulls the new image and restarts the service.

### Synopsis

```bash
thresh stack update <name> --service <svc> --image <img> [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Name of the stack |

### Options

| Option | Description |
|--------|-------------|
| `--service <svc>` | **(Required)** Service name to update |
| `--image <img>` | **(Required)** New image (e.g. `docker:my-app:v2`) |
| `--hub <url>` | Hub URL (overrides stored credentials) |

### Example

```bash
thresh stack update my-stack --service app --image docker:my-org/my-app:v2.1
```

```
🔄 Rolling update started: 'app' → docker:my-org/my-app:v2.1
   Run 'thresh stack info my-stack' to track progress.
```

---

## Authentication

All stack commands require an authenticated session. Use `thresh auth login` to authenticate via device-code flow, or provide an API key via `thresh agent config set api-key <key>`.

Stack operations are scoped to your account — you can only see and manage stacks belonging to your account.

## See Also

- [thresh agent](/docs/cli-reference/agent) — Connect nodes to Thresh Hub
- [Getting Started on Windows](/docs/getting-started-windows) — Initial setup guide
