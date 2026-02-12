---
sidebar_position: 11
title: thresh serve
description: Start the MCP server for AI integration
---

# thresh serve

Start the Model Context Protocol (MCP) server for subprocess communication.

## Synopsis

```powershell
thresh serve [options]
```

## Description

The `serve` command starts thresh as an MCP server, enabling communication with AI tools like GitHub Copilot and Claude Desktop. The server:

- Exposes thresh commands as MCP tools
- Runs in stdio mode for subprocess communication
- Provides structured JSON-RPC 2.0 responses
- Enables AI-driven environment management

This is typically called by AI client applications and not run directly by users.

## Options

| Option | Description |
|--------|-------------|
| `--log-file <path>` | Write logs to file (default: stderr) |
| `--log-level <level>` | Log verbosity: debug, info, warn, error |
| `--help`, `-h` | Show help information |

## Communication Protocol

thresh MCP server uses **stdio** transport:

- **Input:** JSON-RPC 2.0 requests via stdin
- **Output:** JSON-RPC 2.0 responses via stdout
- **Errors:** Diagnostic logs via stderr or log file

### Example Request

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "thresh_list",
    "arguments": {}
  }
}
```

### Example Response

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Available environments:\n\npython-dev (running)\nnode-dev (stopped)"
      }
    ]
  }
}
```

## Available MCP Tools

When running as an MCP server, thresh exposes these tools:

| Tool Name | Description |
|-----------|-------------|
| `thresh_list` | List all environments |
| `thresh_up` | Provision new environment |
| `thresh_destroy` | Destroy environment |
| `thresh_generate` | Generate blueprint from environment |
| `thresh_blueprints` | List available blueprints |
| `thresh_distros` | List available distributions |

## Client Configuration

### GitHub Copilot (VS Code)

**File:** `.vscode/mcp.json`

```json
{
  "mcpServers": {
    "thresh": {
      "command": "thresh",
      "args": ["serve"],
      "env": {}
    }
  }
}
```

### Claude Desktop

**Windows:** `%APPDATA%\Claude\claude_desktop_config.json`  
**macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`  
**Linux:** `~/.config/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "thresh": {
      "command": "C:\\Program Files\\thresh\\thresh.exe",
      "args": ["serve"]
    }
  }
}
```

### Cline (VS Code Extension)

**File:** `.vscode/settings.json`

```json
{
  "cline.mcpServers": {
    "thresh": {
      "command": "thresh",
      "args": ["serve"]
    }
  }
}
```

## Examples

### Direct Invocation (Manual)

```powershell
# Start MCP server (blocks until terminated)
thresh serve

# With debug logging to file
thresh serve --log-file thresh-mcp.log --log-level debug
```

**Output (stderr):**
```
[INFO] MCP server starting on stdio transport
[INFO] Registered 6 tools
[INFO] Server ready, waiting for requests...
```

### Test with Manual Request

```powershell
# Send JSON-RPC request via stdin
echo '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | thresh serve
```

**Output (stdout):**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "tools": [
      {
        "name": "thresh_list",
        "description": "List all thresh environments",
        "inputSchema": {
          "type": "object",
          "properties": {}
        }
      }
    ]
  }
}
```

### Integration with AI Clients

Typically invoked automatically by AI clients:

```
User → GitHub Copilot → thresh serve (subprocess) → Container Runtime
```

The AI client:
1. Spawns `thresh serve` as subprocess
2. Sends JSON-RPC requests via stdin
3. Receives responses via stdout
4. Parses responses and presents to user

## Logging

### Log Levels

| Level | Description |
|-------|-------------|
| `debug` | Verbose diagnostic information |
| `info` | Normal operational messages |
| `warn` | Warning conditions |
| `error` | Error conditions |

### Log Destinations

```powershell
# Logs to stderr (default)
thresh serve

# Logs to specific file
thresh serve --log-file ~/.thresh/mcp-server.log

# Silent operation (only errors)
thresh serve --log-level error
```

### Log Format

```
[2026-02-12T14:30:45Z] [INFO] MCP server starting
[2026-02-12T14:30:45Z] [INFO] Registered tool: thresh_list
[2026-02-12T14:30:46Z] [DEBUG] Received request: tools/list
[2026-02-12T14:30:46Z] [DEBUG] Sending response: 234 bytes
```

## Troubleshooting

### Server Won't Start

```powershell
# Check thresh is in PATH
thresh --version

# Verify container runtime
wsl --status  # Windows
docker ps     # Linux/macOS
```

### Client Can't Connect

**Issue:** "MCP server not responding"

**Fix:** Verify client configuration has correct path:

```json
{
  "command": "thresh",  // Must be in PATH
  "args": ["serve"]     // Exactly "serve"
}
```

### No Tool Responses

**Issue:** Server starts but tools don't work

**Debug:**
```powershell
# Enable debug logging
thresh serve --log-file debug.log --log-level debug

# Check log for errors
cat debug.log
```

### Permission Errors

**Windows (WSL):**
```powershell
# Ensure WSL 2 is running
wsl --status

# Restart WSL if needed
wsl --shutdown
wsl
```

**Linux:**
```bash
# Check Docker permissions
docker ps

# Add user to docker group if needed
sudo usermod -aG docker $USER
```

## Process Management

### Background Service (Linux/macOS)

Create systemd service:

**File:** `~/.config/systemd/user/thresh-mcp.service`

```ini
[Unit]
Description=thresh MCP Server
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/thresh serve --log-file %h/.thresh/mcp.log
Restart=on-failure

[Install]
WantedBy=default.target
```

**Enable:**
```bash
systemctl --user enable thresh-mcp
systemctl --user start thresh-mcp
```

### Windows Service

Use NSSM (Non-Sucking Service Manager):

```powershell
# Install NSSM
winget install nssm

# Create service
nssm install thresh-mcp "C:\Program Files\thresh\thresh.exe" serve
nssm set thresh-mcp AppDirectory "C:\Users\user\.thresh"
nssm start thresh-mcp
```

## Security Considerations

### Localhost Only

MCP server only accepts stdio communication - no network exposure.

### Process Isolation

Each AI client spawns its own thresh serve instance:
- Separate process space
- Independent authentication
- No shared state

### Container Privileges

thresh containers run **rootless** by default - no host system access beyond mapped directories.

## Performance

### Resource Usage

- **Memory:** ~10 MB idle
- **CPU:** <1% idle, spikes during tool calls
- **Startup:** <100ms

### Concurrent Clients

Multiple AI clients can run separate `thresh serve` instances:
- VS Code: 1 instance
- Claude Desktop: 1 instance
- Cline: 1 instance

Each operates independently with no conflicts.

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Clean shutdown (SIGTERM/SIGINT) |
| `1` | Initialization error |
| `2` | Protocol error (invalid JSON-RPC) |
| `3` | Container runtime unavailable |

## See Also

- [MCP Integration Guide](/docs/mcp-integration) - Full setup walkthrough
- [`thresh index`](/docs/cli-reference/index) - Initialize MCP configuration
- [Model Context Protocol Specification](https://modelcontextprotocol.io/) - Protocol details
