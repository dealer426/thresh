namespace Thresh.Models;

/// <summary>
/// Configuration for thresh agent daemon
/// </summary>
public class AgentConfiguration
{
    /// <summary>
    /// Unique agent ID (GUID)
    /// </summary>
    public string AgentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether agent mode is enabled
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Primary midtier URL (on-prem or cloud)
    /// </summary>
    public string? MidtierUrl { get; set; }
    
    /// <summary>
    /// Fallback cloud SaaS URL (disaster recovery)
    /// </summary>
    public string? FallbackUrl { get; set; }
    
    /// <summary>
    /// API key for primary midtier authentication
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// API key for fallback cloud (optional, can be different)
    /// </summary>
    public string? FallbackApiKey { get; set; }
    
    /// <summary>
    /// Transport mode: auto, signalr, rest
    /// </summary>
    public string Transport { get; set; } = "auto";
    
    /// <summary>
    /// Whether to enable automatic failover to cloud
    /// </summary>
    public bool FailoverEnabled { get; set; } = true;
    
    /// <summary>
    /// Timeout before failing over to cloud (seconds)
    /// </summary>
    public int FailoverTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Whether to enable automatic failback to primary
    /// </summary>
    public bool FailbackEnabled { get; set; } = true;
    
    /// <summary>
    /// Delay before attempting failback to primary (seconds)
    /// </summary>
    public int FailbackDelaySeconds { get; set; } = 300; // 5 minutes
    
    /// <summary>
    /// SignalR hub path
    /// </summary>
    public string SignalRHubPath { get; set; } = "/agenthub";
    
    /// <summary>
    /// Connection timeout (seconds)
    /// </summary>
    public int ConnectTimeoutSeconds { get; set; } = 10;
    
    /// <summary>
    /// Command execution timeout (seconds)
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 300;
    
    /// <summary>
    /// Metrics reporting interval (seconds)
    /// </summary>
    public int MetricsIntervalSeconds { get; set; } = 60;
    
    /// <summary>
    /// Verify TLS certificates
    /// </summary>
    public bool TlsVerify { get; set; } = true;
    
    /// <summary>
    /// Custom CA certificate path (optional)
    /// </summary>
    public string? TlsCaCert { get; set; }
    
    /// <summary>
    /// Client certificate path for mTLS (PFX or PEM)
    /// </summary>
    public string? TlsClientCert { get; set; }
    
    /// <summary>
    /// Client certificate private key path (PEM, required if TlsClientCert is PEM)
    /// </summary>
    public string? TlsClientKey { get; set; }
    
    /// <summary>
    /// HTTP proxy URL (optional)
    /// </summary>
    public string? Proxy { get; set; }
    
    /// <summary>
    /// REST polling interval when using REST transport (seconds)
    /// </summary>
    public int RestPollingIntervalSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum REST retry attempts
    /// </summary>
    public int RestMaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Log level: info, debug, warning, error
    /// </summary>
    public string LogLevel { get; set; } = "info";
    
    /// <summary>
    /// Maximum log file size (MB)
    /// </summary>
    public int LogMaxSizeMB { get; set; } = 100;
    
    /// <summary>
    /// Maximum number of log file backups
    /// </summary>
    public int LogMaxBackups { get; set; } = 5;
    
    /// <summary>
    /// Enable offline caching when all connections fail
    /// </summary>
    public bool OfflineCacheEnabled { get; set; } = true;
    
    /// <summary>
    /// Offline cache directory path
    /// </summary>
    public string? OfflineCachePath { get; set; }
    
    /// <summary>
    /// Offline cache duration (seconds)
    /// </summary>
    public int OfflineCacheDurationSeconds { get; set; } = 86400; // 24 hours
    
    /// <summary>
    /// Maximum offline cache size (MB)
    /// </summary>
    public int OfflineCacheMaxSizeMB { get; set; } = 100;
}

/// <summary>
/// Connection tier for multi-tier fallback
/// </summary>
public enum ConnectionTier
{
    /// <summary>
    /// On-prem SignalR (best performance)
    /// </summary>
    PrimarySignalR,
    
    /// <summary>
    /// On-prem REST polling
    /// </summary>
    PrimaryREST,
    
    /// <summary>
    /// Cloud SaaS SignalR (disaster recovery)
    /// </summary>
    CloudSignalR,
    
    /// <summary>
    /// Cloud SaaS REST polling
    /// </summary>
    CloudREST,
    
    /// <summary>
    /// Offline mode (local cache only)
    /// </summary>
    Offline
}

/// <summary>
/// Agent connection status
/// </summary>
public class AgentStatus
{
    /// <summary>
    /// Whether agent is running
    /// </summary>
    public bool IsRunning { get; set; }
    
    /// <summary>
    /// Current connection tier
    /// </summary>
    public ConnectionTier CurrentTier { get; set; }
    
    /// <summary>
    /// Primary midtier URL
    /// </summary>
    public string? PrimaryUrl { get; set; }
    
    /// <summary>
    /// Fallback midtier URL
    /// </summary>
    public string? FallbackUrl { get; set; }
    
    /// <summary>
    /// Whether connected to midtier
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// Last successful connection time
    /// </summary>
    public DateTime? LastConnected { get; set; }
    
    /// <summary>
    /// Last status message
    /// </summary>
    public string? LastMessage { get; set; }
    
    /// <summary>
    /// Last message received time
    /// </summary>
    public DateTime? LastMessageTime { get; set; }
    
    /// <summary>
    /// Number of reconnection attempts
    /// </summary>
    public int ReconnectCount { get; set; }
    
    /// <summary>
    /// Agent uptime
    /// </summary>
    public TimeSpan Uptime { get; set; }
    
    /// <summary>
    /// Agent ID (GUID)
    /// </summary>
    public string? AgentId { get; set; }
    
    /// <summary>
    /// Number of managed environments
    /// </summary>
    public int EnvironmentCount { get; set; }
    
    /// <summary>
    /// Number of running environments
    /// </summary>
    public int RunningEnvironments { get; set; }
    
    /// <summary>
    /// Number of stopped environments
    /// </summary>
    public int StoppedEnvironments { get; set; }
}
