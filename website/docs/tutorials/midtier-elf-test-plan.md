---
sidebar_position: 10
title: Midtier ELF Migration — Test Plan
description: Full test plan for validating thresh-midtier after conversion to native ELF binary (Native AOT)
---

# Midtier ELF Migration — Test Plan

**Applies to:** thresh-midtier (ELF native binary conversion)  
**Audience:** QA engineers, contributors  
**Difficulty:** Advanced

## Overview

thresh-midtier is being converted from a .NET self-contained bundle to a **native ELF binary** using .NET Native AOT. This strips the CoreCLR runtime from the output, producing a smaller, faster-starting single ELF file — the same compilation model already used by the main `thresh` CLI.

This document covers every test area that must pass before the ELF midtier ships to production.

---

## 1. Build & Binary Validation

Verify that the AOT publish step produces a valid, self-contained ELF binary and that no unintended runtime dependencies are present.

### 1.1 ELF format check

```bash
file ./thresh-midtier
# Expected: ELF 64-bit LSB executable, x86-64, dynamically linked, ...
```

Repeat for `linux-arm64` output when cross-compiled.

### 1.2 No CoreCLR dependency

```bash
ldd ./thresh-midtier | grep -E "libcoreclr|libhostfxr|libhostpolicy"
# Expected: no output (none of these libraries should appear)
```

### 1.3 Binary size regression guard

Record the file size after each successful build. AOT output should be **≤ 25 MB** before UPX and **≤ 8 MB** after UPX compression (adjust thresholds as the feature set grows).

```bash
du -sh ./thresh-midtier
upx --best -o ./thresh-midtier.upx ./thresh-midtier
du -sh ./thresh-midtier.upx
```

### 1.4 Startup smoke test

```bash
./thresh-midtier --version
# Expected: exits 0, prints semantic version string (e.g. "thresh-midtier 1.8.0")
```

```bash
./thresh-midtier --help
# Expected: exits 0, prints usage text
```

### 1.5 Trimming warnings gate

The CI publish step must produce **zero** `ILLink` trim warnings. Any suppressed warning must be justified with a `[DynamicallyAccessedMembers]` or `[RequiresUnreferencedCode]` annotation and a code comment.

---

## 2. Configuration & Startup

### 2.1 Config file loading

| Scenario | Expected behaviour |
|----------|--------------------|
| Valid `appsettings.json` present | Midtier starts, logs resolved Hub URL |
| Missing `appsettings.json` | Process exits 1 with a clear error message |
| Malformed JSON | Process exits 1, prints the offending field |
| `Hub.Url` is empty string | Process exits 1 with validation error |
| `Hub.ApiKey` missing `thresh_mid_` prefix | Process exits 1 with key-format error |

### 2.2 Environment variable overrides

All `appsettings.json` values must be overridable via environment variables (ASP.NET Core convention):

```bash
Hub__Url=https://hub.example.com:7200 \
Hub__ApiKey=thresh_mid_test_abc123 \
  ./thresh-midtier
# Expected: midtier starts using the env-var values, not the file values
```

### 2.3 Health endpoint

```bash
curl -sf http://localhost:5000/health
# Expected: HTTP 200, body: {"status":"Healthy"}
```

Confirm the health endpoint is reachable immediately after startup (within 3 seconds).

---

## 3. Hub Connectivity

### 3.1 Successful registration

Start the midtier pointed at a real or mock Hub. Check:

- Midtier logs `"Connected to Hub"` within 10 seconds of startup
- Hub dashboard (or mock server log) shows the midtier as a registered relay

### 3.2 Mid-tier API key enforcement

| Key presented | Expected Hub response | Midtier behaviour |
|---------------|-----------------------|-------------------|
| `thresh_mid_<valid>` | 200 Registered | Proceeds to accept agents |
| `thresh_live_<valid>` | 403 Forbidden | Logs auth error, retries with back-off |
| Empty string | 401 Unauthorized | Logs auth error, exits 1 |
| Revoked key | 403 Forbidden | Logs auth error, retries with back-off |

### 3.3 TLS verification

| Scenario | Flag | Expected |
|----------|------|----------|
| Valid cert | `TlsVerify: true` (default) | Connection succeeds |
| Self-signed cert, verify on | `TlsVerify: true` | Connection refused, logs cert error |
| Self-signed cert, verify off | `TlsVerify: false` | Connection succeeds |
| Expired cert, verify on | `TlsVerify: true` | Connection refused, logs expiry |

### 3.4 Hub SignalR connection

Confirm the midtier establishes a **SignalR WebSocket** connection to the Hub (not plain HTTP polling) by default:

```bash
# Capture traffic with tcpdump or Wireshark on loopback and confirm:
# - TCP upgrade to WebSocket (HTTP 101 Switching Protocols)
# - Subsequent frames are WebSocket frames, not HTTP request/response pairs
```

---

## 4. Agent Connectivity

### 4.1 Agent handshake

Connect a `thresh agent` node to the midtier endpoint. Verify:

- Agent shows `Status: Connected ✓` in `thresh agent status`
- Midtier logs the agent ID and platform
- Hub dashboard shows the agent as online

### 4.2 Agent API key enforcement

| Key type | Expected |
|----------|----------|
| `thresh_live_<valid>` | Agent accepted, appears in Hub |
| `thresh_mid_<key>` | Connection rejected (403), agent shows error |
| No key / empty | Connection rejected (401) |

### 4.3 Metrics routing

With one agent connected:

1. Wait one metrics interval (default 30 s)
2. Query the Hub API: `GET /api/nodes/{agentId}`
3. Confirm `cpu`, `memoryUsed`, and `containers` fields are non-null and recently updated

### 4.4 Multi-agent load

Connect **50 concurrent agents** (use a test harness or stress script). Verify:

- All 50 agents show `Connected ✓`
- No agent connection is dropped during a 5-minute soak
- Midtier CPU stays below 25 % on a single 2-core VM
- Midtier memory stays below 256 MB RSS

### 4.5 Agent reconnect after midtier restart

1. Start midtier and connect 5 agents
2. `kill -SIGTERM` the midtier process
3. Restart the midtier
4. Within `ReconnectDelay × 3` seconds all 5 agents must report `Connected ✓` again

---

## 5. Command Dispatch

### 5.1 Remote blueprint deploy

From the Hub API (or Hub UI), dispatch a blueprint deploy to an agent through the midtier:

```bash
curl -X POST https://hub:7200/api/nodes/{agentId}/up \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"blueprint":"alpine-minimal","name":"elf-test-env"}'
```

Expected: environment appears in `thresh list` on the target node within 30 s.

### 5.2 Command timeout

Dispatch a command to an offline agent. The midtier must:

- Return a `504 Gateway Timeout` to the Hub within 30 seconds (configurable)
- Log the timeout with the agent ID and command type

### 5.3 Concurrent commands

Dispatch 10 simultaneous commands to 10 different agents. All must complete without cross-contamination (responses delivered to the correct agent).

---

## 6. Resilience & Failover

### 6.1 Graceful shutdown

Send `SIGTERM` to the midtier:

- In-flight metric batches are flushed to the Hub before exit
- No agent receives a `connection refused` error during the drain window (default 10 s)
- Process exits 0

### 6.2 Hub outage and recovery

1. Start midtier + 3 agents
2. Stop the Hub
3. Confirm midtier logs `"Hub unreachable, retrying..."` with exponential back-off (max 60 s)
4. Agents remain connected to the midtier (midtier acts as buffer)
5. Restart the Hub; confirm midtier reconnects and flushes buffered metrics within 10 s

### 6.3 Network partition between midtier and Hub

Use `tc netem` or `iptables` to introduce 100 % packet loss for 60 seconds, then restore:

```bash
# Block traffic to Hub port
sudo iptables -A OUTPUT -p tcp --dport 7200 -j DROP
sleep 60
sudo iptables -D OUTPUT -p tcp --dport 7200 -j DROP
```

Verify midtier reconnects automatically and no agent connection is lost.

---

## 7. Metrics Batching

### 7.1 Batch size enforcement

Configure `MetricsBatchSize: 10`. Connect 15 agents reporting simultaneously. Confirm Hub receives two batches (10 + 5) rather than 15 individual writes.

### 7.2 Stale agent cleanup

Configure `StaleAgentWindowSeconds: 300` (5 min). Disconnect an agent without a clean shutdown. After 5 minutes confirm:

- Midtier no longer proxies commands to that agent
- Hub marks the agent as `Offline`

---

## 8. Observability

### 8.1 Structured logging

Confirm every log line is valid JSON (for log-aggregation pipelines):

```bash
./thresh-midtier 2>&1 | head -20 | python3 -c "
import sys, json
for line in sys.stdin:
    json.loads(line)  # raises if invalid
print('All lines are valid JSON')
"
```

### 8.2 Prometheus metrics endpoint

```bash
curl -sf http://localhost:5000/metrics | grep -E \
  "midtier_agents_connected|midtier_hub_requests_total|midtier_metrics_batches_sent"
# Expected: all three metrics present with numeric values
```

### 8.3 Log levels

| Level | Should appear |
|-------|--------------|
| `Information` | Startup, connection events, batch sends |
| `Warning` | Retries, TLS issues, stale agents |
| `Error` | Auth failures, unexpected exceptions |
| `Debug` | Individual metric payloads (only when `Logging:LogLevel:Default=Debug`) |

---

## 9. Platform Matrix

Run the full test suite (sections 1–8) on each target platform before shipping:

| Platform | Architecture | Status |
|----------|-------------|--------|
| Ubuntu 22.04 | x86-64 | Required |
| Ubuntu 22.04 | arm64 | Required |
| Debian 12 | x86-64 | Required |
| Alpine 3.19 (musl) | x86-64 | Required — test `linux-musl-x64` publish target |
| RHEL 9 / Rocky 9 | x86-64 | Recommended |
| Windows Server 2022 (via WSL2) | x86-64 | Optional — midtier is Linux-first |

:::note Alpine / musl
Native AOT on Alpine requires the `linux-musl-x64` RID and linking against musl libc. Verify that `ldd` output shows `musl` rather than `glibc` on Alpine targets.
:::

---

## 10. Regression Checks vs. Framework Build

Run these checks against both the **previous framework-dependent** build and the **new ELF AOT** build. Results must be identical:

| Check | Framework | ELF AOT |
|-------|-----------|---------|
| Agent handshake round-trip < 200 ms | ✅ | Must match |
| Metrics batch delivered within 2× interval | ✅ | Must match |
| Hub auth error produces correct HTTP 403 | ✅ | Must match |
| `thresh agent status` shows correct midtier version | ✅ | Must match |
| Clean shutdown within 15 s | ✅ | Must match |

---

## 11. CI Integration

Add the following jobs to the midtier GitHub Actions pipeline:

```yaml
- name: Publish AOT binary
  run: |
    dotnet publish src/ThreshMidTier \
      -r linux-x64 \
      -c Release \
      --self-contained true \
      /p:PublishAot=true \
      -o ./dist/linux-x64

- name: Validate ELF binary
  run: |
    file ./dist/linux-x64/thresh-midtier | grep -q "ELF 64-bit"
    ldd ./dist/linux-x64/thresh-midtier | grep -vq "libcoreclr"

- name: Startup smoke test
  run: |
    ./dist/linux-x64/thresh-midtier --version
    ./dist/linux-x64/thresh-midtier --help

- name: Health check
  run: |
    Hub__Url=https://mock-hub:7200 Hub__ApiKey=thresh_mid_test_key \
      ./dist/linux-x64/thresh-midtier &
    sleep 3
    curl -sf http://localhost:5000/health
```

All four jobs must pass on every pull request targeting `main`.

---

## 12. Sign-off Checklist

Before merging the ELF midtier to `main`:

- [ ] `file` command confirms ELF binary on all required platforms
- [ ] `ldd` confirms no CoreCLR/hostfxr dependencies
- [ ] Binary size within targets (pre- and post-UPX)
- [ ] Zero trim warnings in CI publish log
- [ ] All agent connectivity tests pass (sections 4.1–4.5)
- [ ] Hub connectivity and auth tests pass (section 3)
- [ ] Graceful shutdown and Hub-outage resilience pass (section 6)
- [ ] Structured JSON logging confirmed (section 8.1)
- [ ] Prometheus metrics endpoint present (section 8.2)
- [ ] Platform matrix tested (Ubuntu x64, Ubuntu arm64, Alpine musl)
- [ ] Regression comparison table complete (section 10)
- [ ] CI jobs added and passing (section 11)

---

## Related Pages

- [Fleet Management with Thresh Hub](/docs/tutorials/fleet-management) — Architecture overview and setup
- [thresh agent CLI reference](/docs/cli-reference/agent) — Agent configuration keys
- [thresh auth CLI reference](/docs/cli-reference/auth) — Hub authentication
- [thresh node CLI reference](/docs/cli-reference/node) — Remote node management
