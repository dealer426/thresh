---
sidebar_position: 16
title: thresh auth
description: Authenticate the thresh CLI with Thresh Hub for remote node and cluster management
---

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# thresh auth

:::info New in v1.7.0
The `auth` command group is new in thresh 1.7.0. It provides device-code and token-based authentication for Hub features such as `thresh node` and `thresh cluster`.
:::

Authenticate the thresh CLI with **Thresh Hub** so you can manage remote nodes, deploy blueprints, and organize clusters. Authentication is stored locally and persists across sessions.

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`auth login`](#thresh-auth-login) | Authenticate with a Thresh Hub instance |
| [`auth logout`](#thresh-auth-logout) | Sign out and revoke the current session |
| [`auth status`](#thresh-auth-status) | Show current authentication status |
| [`auth token`](#thresh-auth-token) | Print the current access token (for scripting) |

---

## thresh auth login

Authenticate with a Thresh Hub instance using device-code flow or a pre-existing token.

### Synopsis

```bash
thresh auth login [--hub <url>] [--no-browser] [--token <token>]
```

### Options

| Option | Description |
|--------|-------------|
| `--hub <url>`, `-h <url>` | Hub URL (default: from config or `https://thresh.io`) |
| `--no-browser` | Print the activation URL instead of opening a browser |
| `--token <token>`, `-t <token>` | Login directly with an existing `thresh_cli_*` token |

### Device-Code Flow (Default)

When you run `thresh auth login`, the CLI:

1. Requests a device code from the Hub
2. Opens your browser to the Hub's activation page
3. You enter the code and approve the request
4. The CLI receives a session token and stores it locally

```bash
thresh auth login --hub https://192.168.4.85:7200
```

```
▸ Opening browser to https://192.168.4.85:7200/device
▸ Enter code: ABCD-1234
✓ Authenticated as admin@example.com
```

### Token Login (Scripting / CI)

If you already have a `thresh_cli_*` token (e.g. from the Hub UI), skip the browser flow:

```bash
thresh auth login --token thresh_cli_xxxxxxxxxxxx --hub https://192.168.4.85:7200
```

```
✓ Authenticated with token
```

### Headless Environments

Use `--no-browser` when running on a headless server or inside SSH:

```bash
thresh auth login --hub https://192.168.4.85:7200 --no-browser
```

```
▸ Go to: https://192.168.4.85:7200/device
▸ Enter code: ABCD-1234
▸ Waiting for approval...
✓ Authenticated as admin@example.com
```

---

## thresh auth logout

Sign out and revoke the current CLI session. The stored token is deleted locally and invalidated on the Hub.

### Synopsis

```bash
thresh auth logout
```

### Example

```bash
thresh auth logout
```

```
✓ Signed out
```

---

## thresh auth status

Show the current authentication status, including which Hub you are connected to and your account.

### Synopsis

```bash
thresh auth status
```

### Example Output

**Authenticated:**
```
Auth Status
────────────────────────────────────────
Hub:       https://192.168.4.85:7200
Account:   admin@example.com
Token:     thresh_cli_****
Expires:   2026-05-08 10:30 UTC
```

**Not authenticated:**
```
Not authenticated. Run:
  thresh auth login --hub <url>
```

---

## thresh auth token

Print the raw access token. Useful for scripting or passing credentials to other tools.

### Synopsis

```bash
thresh auth token
```

### Example

```bash
# Use in scripts
TOKEN=$(thresh auth token)
curl -H "Authorization: Bearer $TOKEN" https://hub:7200/api/nodes

# Pipe to clipboard (Windows)
thresh auth token | clip
```

:::caution
The token grants access to your Hub account. Do not log or share it.
:::

---

## Credential Storage

Credentials are stored at `~/.thresh/hub-auth.json`:

```json
{
  "hubUrl": "https://192.168.4.85:7200",
  "token": "thresh_cli_xxxx",
  "email": "admin@example.com",
  "expiresUtc": "2026-05-08T10:30:00Z"
}
```

The file is created on `auth login` and removed on `auth logout`.

---

## Quick Start

```bash
# 1. Login
thresh auth login --hub https://your-hub:7200

# 2. Verify
thresh auth status

# 3. Use Hub features
thresh node list
thresh cluster list
```

## See Also

- [thresh node](/docs/cli-reference/node) — Manage remote nodes
- [thresh cluster](/docs/cli-reference/cluster) — Organize nodes into clusters
- [thresh agent](/docs/cli-reference/agent) — Connect this machine as a fleet node
