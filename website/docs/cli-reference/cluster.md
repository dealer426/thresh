---
sidebar_position: 18
title: thresh cluster
description: Organize fleet nodes into named clusters for grouped management
---

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# thresh cluster

:::info New in v1.7.0
The `cluster` command group is new in thresh 1.7.0. Requires an authenticated session — run `thresh auth login` first.
:::

Organize your fleet nodes into **named clusters** for grouped management. Clusters are logical groupings — they don't change how agents connect, but they make it easier to manage large fleets by region, team, or purpose.

## Prerequisites

- An authenticated CLI session (`thresh auth login`)
- One or more nodes in your account (`thresh node list`)

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`cluster list`](#thresh-cluster-list) | List all clusters in your account |
| [`cluster create`](#thresh-cluster-create) | Create a new cluster |
| [`cluster info`](#thresh-cluster-info) | Show cluster details and live node metrics |
| [`cluster add-node`](#thresh-cluster-add-node) | Add a node to a cluster |
| [`cluster remove-node`](#thresh-cluster-remove-node) | Remove a node from a cluster |
| [`cluster delete`](#thresh-cluster-delete) | Delete a cluster |

---

## thresh cluster list

List all clusters in your Hub account.

### Synopsis

```bash
thresh cluster list [--hub <url>]
```

### Options

| Option | Description |
|--------|-------------|
| `--hub <url>` | Hub URL (overrides stored credentials) |

### Example

```bash
thresh cluster list
```

```
NAME          NODES  DESCRIPTION              CREATED
production    3      Production fleet          2 weeks ago
staging       2      Staging / QA nodes        1 week ago
dev-team      1      Developer workstations    3 days ago
```

---

## thresh cluster create

Create a new empty cluster.

### Synopsis

```bash
thresh cluster create <name> [--description <desc>] [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Cluster name (must be unique within account) |

### Options

| Option | Description |
|--------|-------------|
| `--description <desc>` | Optional description |
| `--hub <url>` | Hub URL (overrides stored credentials) |

### Example

```bash
thresh cluster create production --description "Production fleet nodes"
```

```
✓ Cluster 'production' created
```

---

## thresh cluster info

Show cluster details including all member nodes and their live metrics.

### Synopsis

```bash
thresh cluster info <cluster> [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `cluster` | Cluster name or ID |

### Example

```bash
thresh cluster info production
```

```
Cluster: production
Description: Production fleet nodes
Nodes: 3
──────────────────────────────────────────────────

HOSTNAME          STATUS   CPU    MEM      DISK    ENVS
thresh-node-1     online   12%    4.2 GB   45%     3
thresh-node-2     online    8%    2.1 GB   32%     2
thresh-node-4     online   22%    6.8 GB   58%     5

Totals:  CPU avg 14%  |  MEM 13.1 GB  |  10 environments
```

---

## thresh cluster add-node

Add an existing node to a cluster. A node can belong to one cluster at a time.

### Synopsis

```bash
thresh cluster add-node <cluster> <node> [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `cluster` | Cluster name or ID |
| `node` | Node hostname or agent ID |

### Example

```bash
thresh cluster add-node production thresh-node-1
```

```
✓ Node 'thresh-node-1' added to cluster 'production'
```

---

## thresh cluster remove-node

Remove a node from a cluster. The node remains connected to the Hub — it is only removed from the cluster grouping.

### Synopsis

```bash
thresh cluster remove-node <cluster> <node> [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `cluster` | Cluster name or ID |
| `node` | Node hostname or agent ID |

### Example

```bash
thresh cluster remove-node production thresh-node-3
```

```
✓ Node 'thresh-node-3' removed from cluster 'production'
```

---

## thresh cluster delete

Delete a cluster. The member nodes are **not** removed from the Hub — they simply become unassigned.

### Synopsis

```bash
thresh cluster delete <cluster> [--force] [--hub <url>]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `cluster` | Cluster name or ID |

### Options

| Option | Description |
|--------|-------------|
| `--force` | Skip confirmation prompt |
| `--hub <url>` | Hub URL (overrides stored credentials) |

### Example

```bash
thresh cluster delete staging
```

```
Delete cluster 'staging'? Nodes will not be affected. [y/N] y
✓ Cluster 'staging' deleted
```

---

## Use Cases

### By Region

```bash
thresh cluster create us-east --description "US East Coast nodes"
thresh cluster create us-west --description "US West Coast nodes"
thresh cluster create eu-central --description "EU Frankfurt nodes"

thresh cluster add-node us-east thresh-node-1
thresh cluster add-node us-east thresh-node-2
thresh cluster add-node eu-central thresh-node-5
```

### By Team

```bash
thresh cluster create backend-team --description "Backend engineering"
thresh cluster create ml-team --description "Machine learning training nodes"

thresh cluster add-node ml-team gpu-node-1
thresh cluster add-node ml-team gpu-node-2
```

### By Environment

```bash
thresh cluster create production --description "Prod fleet"
thresh cluster create staging --description "Staging / QA"
thresh cluster create development --description "Dev workstations"
```

---

## Quick Start

```bash
# 1. Authenticate
thresh auth login --hub https://your-hub:7200

# 2. Create a cluster
thresh cluster create production --description "Production nodes"

# 3. Add nodes
thresh cluster add-node production thresh-node-1
thresh cluster add-node production thresh-node-2

# 4. View the cluster
thresh cluster info production
```

## See Also

- [thresh auth](/docs/cli-reference/auth) — Authenticate with Thresh Hub
- [thresh node](/docs/cli-reference/node) — Manage individual remote nodes
- [thresh agent](/docs/cli-reference/agent) — Connect this machine as a fleet node
