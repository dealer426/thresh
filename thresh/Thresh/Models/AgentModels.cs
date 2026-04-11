namespace Thresh.Models;

/// <summary>
/// Agent information sent during registration
/// </summary>
public class AgentInfo
{
    /// <summary>
    /// Unique agent ID (GUID)
    /// </summary>
    public string AgentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Machine hostname
    /// </summary>
    public string Hostname { get; set; } = string.Empty;
    
    /// <summary>
    /// Operating system platform (Windows, Linux, macOS)
    /// </summary>
    public string Platform { get; set; } = string.Empty;
    
    /// <summary>
    /// Operating system version
    /// </summary>
    public string OsVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Thresh version
    /// </summary>
    public string ThreshVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Machine IP address
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// CPU architecture (x64, arm64)
    /// </summary>
    public string Architecture { get; set; } = string.Empty;
    
    /// <summary>
    /// Container runtime (WSL, Docker, containerd, nerdctl)
    /// </summary>
    public string ContainerRuntime { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of CPU cores
    /// </summary>
    public int? CpuCores { get; set; }
    
    /// <summary>
    /// Total RAM in GB
    /// </summary>
    public int? RamTotalGb { get; set; }
    
    /// <summary>
    /// Total disk space in GB
    /// </summary>
    public int? DiskTotalGb { get; set; }
    
    /// <summary>
    /// Registration timestamp
    /// </summary>
    public DateTime RegisteredAt { get; set; }
}

/// <summary>
/// Metrics data sent from agent to hub
/// </summary>
public class MetricsData
{
    /// <summary>
    /// Agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Metrics timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuPercent { get; set; }
    
    /// <summary>
    /// Memory used (MB)
    /// </summary>
    public long MemoryUsedMB { get; set; }
    
    /// <summary>
    /// Memory total (MB)
    /// </summary>
    public long MemoryTotalMB { get; set; }
    
    /// <summary>
    /// Disk used (GB)
    /// </summary>
    public long DiskUsedGB { get; set; }
    
    /// <summary>
    /// Disk total (GB)
    /// </summary>
    public long DiskTotalGB { get; set; }
    
    /// <summary>
    /// Number of running environments
    /// </summary>
    public int EnvironmentCount { get; set; }
    
    /// <summary>
    /// Number of GPUs
    /// </summary>
    public int? GpuCount { get; set; }
    
    /// <summary>
    /// GPU model/name
    /// </summary>
    public string? GpuModel { get; set; }
    
    /// <summary>
    /// Total GPU memory in GB
    /// </summary>
    public int? GpuMemoryTotalGb { get; set; }

    /// <summary>
    /// GPU utilization percentage (average across all GPUs)
    /// </summary>
    public double? GpuUtilizationPercent { get; set; }
    
    /// <summary>
    /// Environment details
    /// </summary>
    public List<EnvironmentSummary> Environments { get; set; } = new();
}

/// <summary>
/// Brief environment summary for metrics
/// </summary>
public class EnvironmentSummary
{
    /// <summary>
    /// Environment name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Environment status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Uptime in seconds
    /// </summary>
    public long UptimeSeconds { get; set; }
    
    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuPercent { get; set; }
    
    /// <summary>
    /// Memory used (MB)
    /// </summary>
    public long MemoryMB { get; set; }
    
    /// <summary>
    /// Tags
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Command dispatched from hub to this agent via SignalR.
/// The agent runs the specified tool locally and returns the result via SendCommandResult.
/// </summary>
public class AgentCommand
{
    /// <summary>Unique ID for correlating this command with its result.</summary>
    public string CommandId { get; set; } = string.Empty;

    /// <summary>MCP tool name to execute (e.g. "list_environments", "create_environment").</summary>
    public string Tool { get; set; } = string.Empty;

    /// <summary>JSON arguments for the tool, forwarded as-is from the hub MCP call.</summary>
    public System.Text.Json.JsonElement? Arguments { get; set; }
}

/// <summary>
/// Provision request from hub to agent
/// </summary>
public class ProvisionRequest
{
    /// <summary>
    /// Command ID for tracking
    /// </summary>
    public string CommandId { get; set; } = string.Empty;
    
    /// <summary>
    /// Blueprint name or JSON
    /// </summary>
    public string Blueprint { get; set; } = string.Empty;
    
    /// <summary>
    /// Environment name (optional, can be auto-generated)
    /// </summary>
    public string? EnvironmentName { get; set; }
    
    /// <summary>
    /// Additional parameters
    /// </summary>
    public Dictionary<string, string>? Parameters { get; set; }
}

/// <summary>
/// Destroy request from hub to agent
/// </summary>
public class DestroyRequest
{
    /// <summary>
    /// Command ID for tracking
    /// </summary>
    public string CommandId { get; set; } = string.Empty;
    
    /// <summary>
    /// Environment name to destroy
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to remove volumes
    /// </summary>
    public bool RemoveVolumes { get; set; }
}

/// <summary>
/// Command result sent back to hub.
/// Supports both legacy format (Success+Message) and hub-compatible format (Status+Output).
/// </summary>
public class CommandResult
{
    /// <summary>Agent ID (sent so hub can identify the source for dashboard notifications).</summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>Command ID matching the originating AgentCommand.</summary>
    public string CommandId { get; set; } = string.Empty;

    /// <summary>Status string: "success" or "error" (hub-compatible format).</summary>
    public string? Status { get; set; }

    /// <summary>Whether command succeeded (legacy format — kept for backward compat).</summary>
    public bool Success { get; set; }

    /// <summary>Output text from the command (hub-compatible format).</summary>
    public string? Output { get; set; }

    /// <summary>Result message (legacy format — alias for Output).</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Error message if failed.</summary>
    public string? Error { get; set; }

    /// <summary>Command execution timestamp.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Command execution duration (milliseconds).</summary>
    public long DurationMs { get; set; }
}

/// <summary>Stack service definition received in a deploy_stack command from hub.</summary>
public class StackServiceDefAgent
{
    public string Name { get; set; } = string.Empty;
    /// <summary>OCI image reference — may be prefixed with "docker:" (e.g., "docker:postgres:16-alpine").</summary>
    public string Image { get; set; } = string.Empty;
    public List<string>? Ports { get; set; }
    /// <summary>Volume specs in "name:mountPath" or "hostPath:containerPath" format.</summary>
    public List<string>? Volumes { get; set; }
    /// <summary>Matches "env" property sent by hub (already resolved by hub before dispatch).</summary>
    public Dictionary<string, string>? Env { get; set; }
    public List<string>? DependsOn { get; set; }
    /// <summary>Traefik routing rule — when set, agent writes a dynamic config file after service starts.</summary>
    public string? Route { get; set; }
}

/// <summary>Full payload for the deploy_stack command, sent by hub.</summary>
public class StackDeployPayloadAgent
{
    public string StackId { get; set; } = string.Empty;
    public string StackName { get; set; } = string.Empty;
    /// <summary>When true, the agent writes Traefik dynamic config files for services that have a Route.</summary>
    public bool Traefik { get; set; }
    public List<StackServiceDefAgent> Services { get; set; } = [];
}

/// <summary>Per-service status update sent to hub during a stack deployment.</summary>
public class StackServiceStatusArgs
{
    public string StackId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    /// <summary>pending | deploying | running | stopped | error</summary>
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Agent heartbeat
/// </summary>
public class AgentHeartbeat
{
    /// <summary>
    /// Agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Heartbeat timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Agent status (healthy, degraded, offline)
    /// </summary>
    public string Status { get; set; } = "healthy";
    
    /// <summary>
    /// Number of environments
    /// </summary>
    public int EnvironmentCount { get; set; }
}
