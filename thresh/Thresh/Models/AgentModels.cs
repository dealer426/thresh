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
/// Command result sent back to hub
/// </summary>
public class CommandResult
{
    /// <summary>
    /// Command ID
    /// </summary>
    public string CommandId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether command succeeded
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Result message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// Command execution timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Command execution duration (milliseconds)
    /// </summary>
    public long DurationMs { get; set; }
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
