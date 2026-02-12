---
sidebar_position: 12
title: thresh metrics
description: Display environment resource usage and statistics
---

# thresh metrics

Show resource usage, performance statistics, and operational metrics for environments.

## Synopsis

```powershell
# Show all environments
thresh metrics

# Show specific environment
thresh metrics <environment-name>

# Watch metrics in real-time
thresh metrics <environment-name> --watch
```

## Description

The `metrics` command displays resource consumption and performance data for thresh environments:

- CPU usage (percentage and time)
- Memory usage (RSS, cache, swap)
- Disk I/O (read/write operations and bytes)
- Network I/O (transmitted/received bytes)
- Process counts and states
- Uptime and container lifecycle

Metrics are collected from the container runtime and updated in real-time with `--watch` mode.

## Options

| Option | Description |
|--------|-------------|
| `--watch`, `-w` | Continuously update metrics (1-second interval) |
| `--json` | Output in JSON format |
| `--interval <seconds>` | Update interval for watch mode (default: 1) |
| `--no-stream` | Disable streaming, show current snapshot only |
| `--help`, `-h` | Show help information |

## Examples

### Show All Environments

```powershell
# Display metrics for all environments
thresh metrics
```

**Output:**
```
Environment Metrics:

python-dev (running)
  CPU:     12.5% (2.3s total)
  Memory:  256 MB / 8 GB (3.2%)
  Disk:    15 MB read, 8 MB written
  Network: 1.2 MB sent, 850 KB received
  Uptime:  2h 15m

node-dev (stopped)
  Status:  Not running
  Last:    Stopped 45m ago
```

### Monitor Specific Environment

```powershell
# Show metrics for one environment
thresh metrics python-dev
```

**Output:**
```
python-dev

CPU Usage:
  Current:  12.5%
  Average:  8.3%
  Total:    2.3s

Memory:
  RSS:      256 MB
  Cache:    128 MB
  Swap:     0 MB
  Limit:    8 GB
  Usage:    3.2%

Disk I/O:
  Read:     15.2 MB (125 ops)
  Write:    8.4 MB (89 ops)

Network I/O:
  Sent:     1.2 MB
  Received: 850 KB

Processes:
  Total:    12
  Running:  3
  Sleeping: 9

Container:
  Status:   Running
  Uptime:   2h 15m 34s
  Restarts: 0
```

### Watch Mode (Real-Time)

```powershell
# Live-updating metrics (Ctrl+C to exit)
thresh metrics python-dev --watch
```

**Output (updates every second):**
```
python-dev (live updates - press Ctrl+C to exit)

CPU:     14.2% ████████░░░░░░░░░░░░  Memory: 268 MB  Disk: 15.3 MB  Net: ↑ 1.3 MB ↓ 912 KB

Updated: 2026-02-12 14:35:22
```

### JSON Output

```powershell
# Get structured metrics
thresh metrics python-dev --json
```

**Output:**
```json
{
  "environment": "python-dev",
  "status": "running",
  "timestamp": "2026-02-12T14:35:22Z",
  "uptime_seconds": 8134,
  "cpu": {
    "usage_percent": 12.5,
    "total_seconds": 2.3,
    "system_seconds": 0.8,
    "user_seconds": 1.5
  },
  "memory": {
    "rss_bytes": 268435456,
    "cache_bytes": 134217728,
    "swap_bytes": 0,
    "limit_bytes": 8589934592,
    "usage_percent": 3.2
  },
  "disk_io": {
    "read_bytes": 15925248,
    "write_bytes": 8808038,
    "read_ops": 125,
    "write_ops": 89
  },
  "network_io": {
    "sent_bytes": 1258291,
    "received_bytes": 870400,
    "sent_packets": 1024,
    "received_packets": 896
  },
  "processes": {
    "total": 12,
    "running": 3,
    "sleeping": 9,
    "stopped": 0,
    "zombie": 0
  }
}
```

### Custom Watch Interval

```powershell
# Update every 5 seconds
thresh metrics python-dev --watch --interval 5
```

### Compare Environments

```powershell
# Export metrics for all environments
thresh metrics --json | jq '.[] | {name: .environment, cpu: .cpu.usage_percent, mem: .memory.usage_percent}'
```

**Output:**
```json
{
  "name": "python-dev",
  "cpu": 12.5,
  "mem": 3.2
}
{
  "name": "node-dev",
  "cpu": 0,
  "mem": 0
}
```

## Metric Definitions

### CPU Metrics

| Metric | Description |
|--------|-------------|
| **Current Usage** | CPU percentage over last sampling period |
| **Average Usage** | Mean CPU usage since container start |
| **Total Time** | Cumulative CPU seconds consumed |
| **User Time** | CPU time in user mode |
| **System Time** | CPU time in kernel mode |

### Memory Metrics

| Metric | Description |
|--------|-------------|
| **RSS** | Resident Set Size - physical RAM used |
| **Cache** | Page cache memory (can be reclaimed) |
| **Swap** | Swap space used |
| **Limit** | Maximum memory container can use |
| **Usage %** | RSS / Limit percentage |

### Disk I/O Metrics

| Metric | Description |
|--------|-------------|
| **Read Bytes** | Total bytes read from disk |
| **Write Bytes** | Total bytes written to disk |
| **Read Ops** | Number of read operations |
| **Write Ops** | Number of write operations |

### Network I/O Metrics

| Metric | Description |
|--------|-------------|
| **Sent Bytes** | Total bytes transmitted |
| **Received Bytes** | Total bytes received |
| **Sent Packets** | Total packets transmitted |
| **Received Packets** | Total packets received |

## Use Cases

### Resource Optimization

Identify memory-hungry environments:

```powershell
# Find environments using >50% memory
thresh metrics --json | jq '.[] | select(.memory.usage_percent > 50)'
```

### Performance Monitoring

Track CPU usage during development:

```powershell
# Watch CPU during intensive task
thresh metrics node-dev --watch
# In another terminal:
# Run build process in environment
```

### Capacity Planning

Determine resource requirements:

```powershell
# Run typical workload, collect metrics
thresh metrics python-dev --json > metrics-baseline.json

# Analyze peak usage
jq '.memory.rss_bytes' metrics-baseline.json | numfmt --to=iec
```

### Health Checks

Verify environment is within bounds:

```powershell
# Check if CPU or memory is excessive
$metrics = thresh metrics app-env --json | ConvertFrom-Json
if ($metrics.cpu.usage_percent -gt 80) {
  Write-Warning "High CPU usage detected"
}
```

## Integration with Monitoring

### Prometheus Exporter

Export metrics in Prometheus format:

```powershell
# Create custom exporter script
thresh metrics --json | jq -r '
  .[] | 
  "thresh_cpu_percent{env=\"\(.environment)\"} \(.cpu.usage_percent)\n" +
  "thresh_memory_bytes{env=\"\(.environment)\"} \(.memory.rss_bytes)"
'
```

**Output:**
```
thresh_cpu_percent{env="python-dev"} 12.5
thresh_memory_bytes{env="python-dev"} 268435456
```

### Grafana Dashboard

Collect metrics periodically:

```bash
#!/bin/bash
# metrics-collector.sh

while true; do
  thresh metrics --json >> /var/log/thresh-metrics.jsonl
  sleep 60
done
```

Import into Grafana using JSON API data source.

### Alerting

Monitor for anomalies:

```powershell
# Simple alerting script
$metrics = thresh metrics production --json | ConvertFrom-Json

if ($metrics.memory.usage_percent -gt 90) {
  # Send alert (email, Slack, PagerDuty, etc.)
  Send-Alert "High memory usage: $($metrics.memory.usage_percent)%"
}
```

## Container Runtime Sources

Metrics are sourced from:

**Windows (WSL):** `/sys/fs/cgroup/` interfaces via WSL 2  
**Linux (Docker):** `docker stats` API  
**macOS (containerd):** containerd metrics API

## Performance Impact

Metrics collection is **lightweight**:
- <1% CPU overhead
- <5 MB memory overhead
- Non-invasive polling (no ptrace)

Watch mode impact:
- 1-second interval: negligible
- 0.1-second interval: ~2% CPU overhead

## Limitations

### Stopped Environments

Metrics are not available for stopped environments - only status and last activity.

### Historical Data

thresh does not persist metrics history. For historical analysis, export to external monitoring tools.

### Nested Containers

Metrics reflect the container, not nested processes if running Docker-in-Docker or similar.

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `2` | Environment not found |
| `3` | Container runtime unavailable |

## See Also

- [`thresh list`](/docs/cli-reference/list) - List environment statuses
- [`thresh up`](/docs/cli-reference/up) - Provision environments
- [`thresh destroy`](/docs/cli-reference/destroy) - Clean up unused environments
