---
sidebar_position: 4
title: Networking & Port Mapping
description: Configure port mapping, exposed ports, and network settings for containerized workloads
---

# Networking & Port Mapping

**Version:** 1.5.0  
**Platform:** Windows (WSL2), Linux, macOS

## Overview

thresh provides comprehensive networking support for WSL environments, enabling you to:

- 🌐 **Port Mapping** - Map host ports to container ports
- 📡 **Port Exposure** - Expose container ports without publishing
- 🔌 **Network Configuration** - Configure container network mode
- 🏷️ **Hostname Control** - Set custom container hostnames

:::tip Why Port Mapping?
Port mapping allows you to access services running inside WSL containers from your Windows host or network. For example, map port 8080 on Windows to port 80 inside the container running nginx.
:::

## Port Mapping Syntax

### Basic Port Mapping

Map a host port to a container port:

```json
{
  "name": "webserver",
  "base": "ubuntu-22.04",
  "ports": [
    "8080:80",      // Host port 8080 → Container port 80
    "8443:443"      // Host port 8443 → Container port 443
  ],
  "packages": ["nginx"]
}
```

### IP Address Binding

Bind to specific network interfaces:

```json
{
  "ports": [
    "127.0.0.1:8080:80",      // Only localhost
    "0.0.0.0:9000:9000",      // All interfaces
    "192.168.1.100:3000:3000" // Specific IP
  ]
}
```

### Protocol Specification

Specify TCP or UDP protocol:

```json
{
  "ports": [
    "8080:80/tcp",        // TCP (default)
    "53:53/udp",          // UDP
    "8080:8080/tcp",      // Explicit TCP
    "8081:8081/udp"       // Explicit UDP
  ]
}
```

### Port Ranges

Map port ranges (experimental):

```json
{
  "ports": [
    "8000-8100:8000-8100"  // Map 101 ports
  ]
}
```

## Exposed Ports

Expose ports without publishing to host:

```json
{
  "name": "api-server",
  "base": "ubuntu-22.04",
  "expose": [
    "9090",    // Exposed but not published
    "9091"     // Accessible from other containers
  ],
  "ports": [
    "8080:80"  // Published to host
  ]
}
```

**Use Cases:**
- Inter-container communication
- Service discovery
- Internal microservices
- Sidecar patterns

## Network Modes

Configure container networking behavior:

### Bridge Network (Default)

Standard Docker bridge networking:

```json
{
  "name": "web-app",
  "network": "bridge",
  "ports": ["8080:80"]
}
```

**Characteristics:**
- Isolated network namespace
- Port mapping required for host access
- Best for most use cases

### Host Network

Share host network namespace:

```json
{
  "name": "monitoring",
  "network": "host"
}
```

**Characteristics:**
- Direct access to host network
- No port mapping needed
- Best for network tools, monitoring
- Less isolation

### None Network

No network access:

```json
{
  "name": "batch-processor",
  "network": "none"
}
```

**Characteristics:**
- Completely isolated
- No network connectivity
- Best for security-sensitive workloads

## Hostname Configuration

Set custom container hostname:

```json
{
  "name": "api-server",
  "hostname": "api.local",
  "base": "ubuntu-22.04"
}
```

**Benefits:**
- Consistent naming across environments
- Service discovery
- Log aggregation
- Application configuration

## Complete Example

### Web Application with Database

**Blueprint:** `web-stack.json`

```json
{
  "name": "web-stack",
  "description": "Full web application stack with port mapping",
  "base": "ubuntu-22.04",
  "ports": [
    "8080:80",      // Web server
    "8443:443",     // HTTPS
    "5432:5432"     // PostgreSQL
  ],
  "expose": [
    "9090",         // Prometheus metrics
    "6379"          // Redis (internal)
  ],
  "network": "bridge",
  "hostname": "webapp.local",
  "packages": [
    "nginx",
    "postgresql",
    "redis-server",
    "curl"
  ],
  "postInstall": "systemctl enable nginx postgresql redis"
}
```

**Provision:**

```powershell
thresh up web-stack --verbose
```

**Access Services:**

```powershell
# Web server
curl http://localhost:8080

# PostgreSQL
psql -h localhost -p 5432 -U postgres

# Check from inside container
wsl -d thresh-web-stack -- curl http://localhost:9090/metrics
```

## WSL2 Port Forwarding

### Windows-Specific Behavior

On Windows with WSL2, thresh automatically:

1. **Detects WSL IP address** - Finds container IP via `hostname -I`
2. **Creates port forwarding rules** - Uses Windows netsh proxy
3. **Persists configuration** - Saves to `~/.thresh/metadata/{env}.json`
4. **Restores on restart** - Reapplies rules when environment starts

### Manual Port Forwarding Management

**Start environment and apply port forwarding:**

```powershell
thresh start webserver
```

**Stop environment and remove port forwarding:**

```powershell
thresh stop webserver
```

**Check Windows port forwarding rules:**

```powershell
netsh interface portproxy show all
```

### Troubleshooting Port Forwarding

**Issue:** Ports not accessible from Windows

```powershell
# 1. Check WSL IP address
wsl -d thresh-webserver -- hostname -I

# 2. Verify service is running
wsl -d thresh-webserver -- ss -tlnp | grep :80

# 3. Test from inside WSL
wsl -d thresh-webserver -- curl http://localhost:80

# 4. Manually add port forwarding
$wslIP = (wsl -d thresh-webserver -- hostname -I).Trim()
netsh interface portproxy add v4tov4 listenport=8080 listenaddress=0.0.0.0 connectport=80 connectaddress=$wslIP

# 5. Restart environment
thresh stop webserver
thresh start webserver
```

**Issue:** Port already in use

```powershell
# Check which process is using the port
netstat -ano | findstr :8080

# Kill the process (replace PID)
taskkill /PID 12345 /F
```

## Linux & macOS Behavior

On Linux and macOS, thresh uses:

- **Docker/nerdctl** native port mapping (`-p` flag)
- **No additional port forwarding** required
- **Direct container networking**

Port mapping works identically across platforms via standard container runtime features.

## Best Practices

### Security

✅ **Bind to localhost when possible:**
```json
"ports": ["127.0.0.1:8080:80"]
```

❌ **Avoid exposing to all interfaces unnecessarily:**
```json
"ports": ["0.0.0.0:22:22"]  // SSH accessible from network
```

### Performance

✅ **Use host network for high-throughput:**
```json
"network": "host"  // Direct host networking
```

✅ **Minimize port mappings:**
- Only map required ports
- Use expose for inter-container communication

### Organization

✅ **Document port mappings:**
```json
{
  "description": "Web server (8080→80), HTTPS (8443→443)",
  "ports": ["8080:80", "8443:443"]
}
```

✅ **Use consistent port conventions:**
- Development: 8000-8999
- Databases: 3000-6999
- Monitoring: 9000-9999

## Integration with WSL Configuration

Combine networking with WSL configuration profiles:

```json
{
  "name": "postgres-mapped",
  "base": "ubuntu-22.04",
  "ports": ["5432:5432"],
  "wslConfig": "database",
  "packages": ["postgresql"]
}
```

The `database` profile disables Windows interop and automount, solving Plan9 filesystem permission issues while maintaining port mapping functionality.

See [WSL Configuration](/docs/wsl-configuration) for more details.

## Reference

### Blueprint Properties

| Property | Type | Description |
|----------|------|-------------|
| `ports` | string[] | Port mappings in "hostPort:containerPort" format |
| `expose` | string[] | Ports to expose without publishing |
| `network` | string | Network mode: "bridge", "host", "none" |
| `hostname` | string | Container hostname |

### CLI Commands

- `thresh up <blueprint>` - Provision with networking config
- `thresh start <env>` - Start and apply port forwarding
- `thresh stop <env>` - Stop and remove port forwarding
- `thresh metrics <env>` - View network statistics

### Metadata Storage

Configuration persisted to: `~/.thresh/metadata/{environmentName}.json`

```json
{
  "Ports": ["8080:80"],
  "Expose": ["9090"],
  "Network": "bridge",
  "Hostname": "web-dev"
}
```

## Next Steps

- [Persistent Volumes](/docs/tutorials/volumes) - Add persistent storage
- [WSL Configuration](/docs/wsl-configuration) - Optimize for databases
- [Custom Blueprints](/docs/tutorials/custom-blueprints) - Build complex setups
