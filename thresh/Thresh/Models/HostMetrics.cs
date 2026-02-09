using System.Text.Json.Serialization;

namespace Thresh.Models;

/// <summary>
/// Host system metrics for capacity planning and monitoring
/// </summary>
public class HostMetrics
{
    /// <summary>
    /// Hostname of the machine
    /// </summary>
    [JsonPropertyName("hostname")]
    public string Hostname { get; set; } = string.Empty;

    /// <summary>
    /// Platform name (Windows, Linux, macOS)
    /// </summary>
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// Container runtime name (WSL, docker, nerdctl, containerd)
    /// </summary>
    [JsonPropertyName("runtime")]
    public string Runtime { get; set; } = string.Empty;

    /// <summary>
    /// Container runtime version
    /// </summary>
    [JsonPropertyName("runtime_version")]
    public string? RuntimeVersion { get; set; }

    /// <summary>
    /// CPU usage percentage (0-100)
    /// </summary>
    [JsonPropertyName("cpu_percent")]
    public double CpuPercent { get; set; }

    /// <summary>
    /// Number of CPU cores
    /// </summary>
    [JsonPropertyName("cpu_cores")]
    public int CpuCores { get; set; }

    /// <summary>
    /// Memory used in GB
    /// </summary>
    [JsonPropertyName("memory_used_gb")]
    public double MemoryUsedGb { get; set; }

    /// <summary>
    /// Total memory in GB
    /// </summary>
    [JsonPropertyName("memory_total_gb")]
    public double MemoryTotalGb { get; set; }

    /// <summary>
    /// Memory usage percentage (0-100)
    /// </summary>
    [JsonPropertyName("memory_percent")]
    public double MemoryPercent => MemoryTotalGb > 0 ? (MemoryUsedGb / MemoryTotalGb) * 100 : 0;

    /// <summary>
    /// Storage free space in GB
    /// </summary>
    [JsonPropertyName("storage_free_gb")]
    public double StorageFreeGb { get; set; }

    /// <summary>
    /// Total storage in GB
    /// </summary>
    [JsonPropertyName("storage_total_gb")]
    public double StorageTotalGb { get; set; }

    /// <summary>
    /// Storage usage percentage (0-100)
    /// </summary>
    [JsonPropertyName("storage_percent")]
    public double StoragePercent => StorageTotalGb > 0 ? ((StorageTotalGb - StorageFreeGb) / StorageTotalGb) * 100 : 0;

    /// <summary>
    /// Number of containers/environments
    /// </summary>
    [JsonPropertyName("containers")]
    public int Containers { get; set; }

    /// <summary>
    /// Timestamp when metrics were collected
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime_seconds")]
    public long? UptimeSeconds { get; set; }

    /// <summary>
    /// Additional metadata (OS version, architecture, etc.)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}
