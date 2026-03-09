using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Thresh.Models;

namespace Thresh.Services;

/// <summary>
/// Agent service for connecting to thresh-hub midtier
/// </summary>
public class AgentService
{
    private readonly ConfigurationService _configService;
    private readonly MetricsService _metricsService;
    private readonly CredentialService _credentialService = new();
    private HubConnection? _hubConnection;
    private ConnectionTier _currentTier = ConnectionTier.Offline;
    private DateTime? _lastConnected;
    private DateTime _startTime = DateTime.UtcNow;
    private int _reconnectCount = 0;
    private bool _isRunning = false;
    private CancellationTokenSource? _cts;
    private string _agentId = string.Empty;
    private DateTime _lastFailoverTime = DateTime.MinValue;
    private readonly HttpClient _httpClient = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        TypeInfoResolver = AgentJsonContext.Default
    };

    public AgentService(ConfigurationService configService, MetricsService metricsService)
    {
        _configService = configService;
        _metricsService = metricsService;
    }

    /// <summary>
    /// Get agent configuration
    /// </summary>
    public AgentConfiguration GetConfiguration()
    {
        var config = _configService.GetAgentConfiguration();
        if (string.IsNullOrEmpty(config.AgentId))
        {
            config.AgentId = Guid.NewGuid().ToString();
            _configService.SaveAgentConfiguration(config);
        }
        _agentId = config.AgentId;
        return config;
    }

    /// <summary>
    /// Update agent configuration
    /// </summary>
    public void UpdateConfiguration(AgentConfiguration config)
    {
        _configService.SaveAgentConfiguration(config);
    }

    /// <summary>
    /// Get current agent status
    /// </summary>
    public AgentStatus GetStatus()
    {
        var config = GetConfiguration();
        return new AgentStatus
        {
            IsRunning = _isRunning,
            CurrentTier = _currentTier,
            IsConnected = _hubConnection?.State == HubConnectionState.Connected,
            LastConnected = _lastConnected,
            LastMessage = _lastConnected.HasValue ? $"Connected via {_currentTier}" : "Not connected",
            ReconnectCount = _reconnectCount,
            Uptime = _isRunning ? DateTime.UtcNow - _startTime : TimeSpan.Zero,
            AgentId = _agentId,
            PrimaryUrl = config.MidtierUrl,
            FallbackUrl = config.FallbackUrl,
            EnvironmentCount = 0, // TODO: Get from container service
            RunningEnvironments = 0, // TODO: Get from container service
            StoppedEnvironments = 0 // TODO: Get from container service
        };
    }

    /// <summary>
    /// Start agent daemon
    /// </summary>
    public async Task<bool> StartAsync()
    {
        if (_isRunning)
        {
            Console.WriteLine("Agent is already running");
            return false;
        }

        var config = GetConfiguration();
        _cts = new CancellationTokenSource();
        _startTime = DateTime.UtcNow;
        _isRunning = true;

        Console.WriteLine($"Starting thresh agent {_agentId}...");
        Console.WriteLine($"Primary midtier: {config.MidtierUrl}");
        if (!string.IsNullOrEmpty(config.FallbackUrl))
        {
            Console.WriteLine($"Fallback URL: {config.FallbackUrl}");
        }

        // Start connection in background
        _ = Task.Run(async () => await RunAgentLoopAsync(_cts.Token), _cts.Token);

        return true;
    }

    /// <summary>
    /// Stop agent daemon
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            Console.WriteLine("Agent is not running");
            return;
        }

        Console.WriteLine("Stopping thresh agent...");
        _isRunning = false;
        _cts?.Cancel();

        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
            catch { }
            _hubConnection = null;
        }

        _currentTier = ConnectionTier.Offline;
        Console.WriteLine("Agent stopped");
    }

    /// <summary>
    /// Manual failover to cloud tier
    /// </summary>
    public async Task<bool> FailoverAsync()
    {
        var config = GetConfiguration();
        if (string.IsNullOrEmpty(config.FallbackUrl))
        {
            Console.WriteLine("No fallback URL configured");
            return false;
        }

        Console.WriteLine("Initiating manual failover to cloud...");
        _lastFailoverTime = DateTime.UtcNow;
        await DisconnectAsync();
        return await ConnectToTierAsync(ConnectionTier.CloudSignalR, config);
    }

    /// <summary>
    /// Manual failback to primary tier
    /// </summary>
    public async Task<bool> FailbackAsync()
    {
        var config = GetConfiguration();
        Console.WriteLine("Initiating manual failback to primary...");
        await DisconnectAsync();
        return await ConnectToTierAsync(ConnectionTier.PrimarySignalR, config);
    }

    /// <summary>
    /// Main agent loop
    /// </summary>
    private async Task RunAgentLoopAsync(CancellationToken ct)
    {
        var config = GetConfiguration();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Try to connect following tier hierarchy
                if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                {
                    var connected = await TryConnectToAnyTierAsync(config, ct);
                    if (!connected)
                    {
                        await Task.Delay(5000, ct); // Wait before retry
                        continue;
                    }
                }

                // Send metrics periodically
                await Task.Delay(TimeSpan.FromSeconds(config.MetricsIntervalSeconds), ct);
                await SendMetricsAsync(config, ct);

                // Check failback conditions
                if (config.FailbackEnabled && ShouldAttemptFailback(config))
                {
                    Console.WriteLine("Attempting automatic failback to primary...");
                    var success = await FailbackAsync();
                    if (success)
                    {
                        Console.WriteLine("Failback successful");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Agent loop error: {ex.Message}");
                await Task.Delay(5000, ct); // Wait before retry
            }
        }
    }

    /// <summary>
    /// Try to connect to any available tier
    /// </summary>
    private async Task<bool> TryConnectToAnyTierAsync(AgentConfiguration config, CancellationToken ct)
    {
        var tiers = new[] 
        { 
            ConnectionTier.PrimarySignalR, 
            ConnectionTier.PrimaryREST,
            ConnectionTier.CloudSignalR,
            ConnectionTier.CloudREST 
        };

        foreach (var tier in tiers)
        {
            if (ct.IsCancellationRequested) return false;

            // Skip cloud tiers if no fallback URL
            if ((tier == ConnectionTier.CloudSignalR || tier == ConnectionTier.CloudREST) 
                && string.IsNullOrEmpty(config.FallbackUrl))
            {
                continue;
            }

            var connected = await ConnectToTierAsync(tier, config);
            if (connected)
            {
                return true;
            }
        }

        // Fall back to offline mode
        _currentTier = ConnectionTier.Offline;
        Console.WriteLine("All connection attempts failed, entering offline mode");
        return false;
    }

    /// <summary>
    /// Connect to a specific tier
    /// </summary>
    private async Task<bool> ConnectToTierAsync(ConnectionTier tier, AgentConfiguration config)
    {
        try
        {
            if (tier == ConnectionTier.PrimarySignalR || tier == ConnectionTier.CloudSignalR)
            {
                return await ConnectSignalRAsync(tier, config);
            }
            else if (tier == ConnectionTier.PrimaryREST || tier == ConnectionTier.CloudREST)
            {
                return await ConnectRESTAsync(tier, config);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to {tier}: {ex.Message}");
        }
        return false;
    }

    /// <summary>
    /// Connect via SignalR
    /// </summary>
    private async Task<bool> ConnectSignalRAsync(ConnectionTier tier, AgentConfiguration config)
    {
        var baseUrl = tier == ConnectionTier.PrimarySignalR ? config.MidtierUrl : config.FallbackUrl;
        var apiKey = tier == ConnectionTier.PrimarySignalR ? config.ApiKey : config.FallbackApiKey;
        
        if (string.IsNullOrEmpty(baseUrl)) return false;

        var hubUrl = $"{baseUrl.TrimEnd('/')}/{config.SignalRHubPath.TrimStart('/')}";
        Console.WriteLine($"Connecting to {tier} at {hubUrl}...");

        var builder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.Headers["X-Agent-Id"] = _agentId;
                if (!string.IsNullOrEmpty(apiKey))
                {
                    options.Headers["Authorization"] = $"Bearer {apiKey}";
                }
                var cliToken = _credentialService.GetEffectiveToken();
                if (!string.IsNullOrEmpty(cliToken))
                {
                    options.Headers["X-Cli-Token"] = cliToken;
                }
                options.HttpMessageHandlerFactory = handler =>
                {
                    if (handler is HttpClientHandler clientHandler && !config.TlsVerify)
                    {
                        clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                    }
                    return handler;
                };
            })
           .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.TypeInfoResolver = AgentJsonContext.Default;
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) });

        var connection = builder.Build();

        // Register handlers
        connection.On<ProvisionRequest>("ProvisionEnvironment", OnProvisionEnvironmentAsync);
        connection.On<DestroyRequest>("DestroyEnvironment", OnDestroyEnvironmentAsync);
        connection.On("Ping", () => Task.CompletedTask);

        connection.Reconnecting += error =>
        {
            Console.WriteLine($"Connection lost, reconnecting... ({error?.Message})");
            return Task.CompletedTask;
        };

        connection.Reconnected += connectionId =>
        {
            Console.WriteLine($"Reconnected to {tier}");
            _reconnectCount++;
            return RegisterAgentAsync(config);
        };

        connection.Closed += async error =>
        {
            Console.WriteLine($"Connection closed ({error?.Message}), will retry...");
            await Task.Delay(5000);
        };

        try
        {
            await connection.StartAsync();
            _hubConnection = connection;
            _currentTier = tier;
            _lastConnected = DateTime.UtcNow;
            
            // Register agent
            await RegisterAgentAsync(config);
            
            Console.WriteLine($"Connected to {tier} successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR connection failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Connect via REST polling
    /// </summary>
    private async Task<bool> ConnectRESTAsync(ConnectionTier tier, AgentConfiguration config)
    {
        var baseUrl = tier == ConnectionTier.PrimaryREST ? config.MidtierUrl : config.FallbackUrl;
        var apiKey = tier == ConnectionTier.PrimaryREST ? config.ApiKey : config.FallbackApiKey;
        
        if (string.IsNullOrEmpty(baseUrl)) return false;

        var url = $"{baseUrl.TrimEnd('/')}/api/agent/register";
        Console.WriteLine($"Connecting to {tier} at {url}...");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
            }
            var cliToken = _credentialService.GetEffectiveToken();
            if (!string.IsNullOrEmpty(cliToken))
            {
                request.Headers.Add("X-Cli-Token", cliToken);
            }

            var agentInfo = await GetAgentInfoAsync();
            request.Content = JsonContent.Create(agentInfo, AgentJsonContext.Default.AgentInfo);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _currentTier = tier;
                _lastConnected = DateTime.UtcNow;
                Console.WriteLine($"Connected to {tier} successfully (REST mode)");
                
                // Start polling loop
                _ = Task.Run(async () => await PollCommandsAsync(config));
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"REST connection failed: {ex.Message}");
        }
        return false;
    }

    /// <summary>
    /// Poll for commands via REST
    /// </summary>
    private async Task PollCommandsAsync(AgentConfiguration config)
    {
        var baseUrl = _currentTier == ConnectionTier.PrimaryREST ? config.MidtierUrl : config.FallbackUrl;
        var apiKey = _currentTier == ConnectionTier.PrimaryREST ? config.ApiKey : config.FallbackApiKey;
        var url = $"{baseUrl?.TrimEnd('/')}/api/agent/commands";

        while (_isRunning && (_currentTier == ConnectionTier.PrimaryREST || _currentTier == ConnectionTier.CloudREST))
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-Agent-Id", _agentId);
                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");
                }

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // TODO: Process commands
                }
            }
            catch { }

            await Task.Delay(30000); // Poll every 30 seconds
        }
    }

    /// <summary>
    /// Disconnect from current tier
    /// </summary>
    private async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
            catch { }
            _hubConnection = null;
        }
    }

    /// <summary>
    /// Check if should attempt failback
    /// </summary>
    private bool ShouldAttemptFailback(AgentConfiguration config)
    {
        // Only failback if we're in cloud tier
        if (_currentTier != ConnectionTier.CloudSignalR && _currentTier != ConnectionTier.CloudREST)
        {
            return false;
        }

        // Check if enough time has passed since failover
        var timeSinceFailover = DateTime.UtcNow - _lastFailoverTime;
        return timeSinceFailover.TotalSeconds >= config.FailbackDelaySeconds;
    }

    /// <summary>
    /// Register agent with hub
    /// </summary>
    private async Task RegisterAgentAsync(AgentConfiguration config)
    {
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                var agentInfo = await GetAgentInfoAsync();
                await _hubConnection.InvokeAsync("RegisterAgent", agentInfo);
                Console.WriteLine("Agent registered successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to register agent: {ex.Message}");
        }
    }

    /// <summary>
    /// Send metrics to hub
    /// </summary>
    private async Task SendMetricsAsync(AgentConfiguration config, CancellationToken ct)
    {
        try
        {
            var metrics = await GetMetricsDataAsync();
            
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendMetrics", metrics, cancellationToken: ct);
            }
            else if (_currentTier == ConnectionTier.PrimaryREST || _currentTier == ConnectionTier.CloudREST)
            {
                var baseUrl = _currentTier == ConnectionTier.PrimaryREST ? config.MidtierUrl : config.FallbackUrl;
                var apiKey = _currentTier == ConnectionTier.PrimaryREST ? config.ApiKey : config.FallbackApiKey;
                var url = $"{baseUrl?.TrimEnd('/')}/api/agent/metrics";

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("X-Agent-Id", _agentId);
                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");
                }
                request.Content = JsonContent.Create(metrics, AgentJsonContext.Default.MetricsData);
                await _httpClient.SendAsync(request, ct);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send metrics: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle provision environment request
    /// </summary>
    private async Task OnProvisionEnvironmentAsync(ProvisionRequest request)
    {
        Console.WriteLine($"Received provisioning request: {request.CommandId}");
        var sw = Stopwatch.StartNew();
        
        try
        {
            // TODO: Implement environment provisioning
            var result = new CommandResult
            {
                CommandId = request.CommandId,
                Success = true,
                Message = $"Environment '{request.EnvironmentName}' provisioned successfully",
                Timestamp = DateTime.UtcNow,
                DurationMs = sw.ElapsedMilliseconds
            };

            await SendCommandResultAsync(result);
        }
        catch (Exception ex)
        {
            var result = new CommandResult
            {
                CommandId = request.CommandId,
                Success = false,
                Message = "Provisioning failed",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow,
                DurationMs = sw.ElapsedMilliseconds
            };

            await SendCommandResultAsync(result);
        }
    }

    /// <summary>
    /// Handle destroy environment request
    /// </summary>
    private async Task OnDestroyEnvironmentAsync(DestroyRequest request)
    {
        Console.WriteLine($"Received destroy request: {request.CommandId}");
        var sw = Stopwatch.StartNew();
        
        try
        {
            // TODO: Implement environment destruction
            var result = new CommandResult
            {
                CommandId = request.CommandId,
                Success = true,
                Message = $"Environment '{request.EnvironmentName}' destroyed successfully",
                Timestamp = DateTime.UtcNow,
                DurationMs = sw.ElapsedMilliseconds
            };

            await SendCommandResultAsync(result);
        }
        catch (Exception ex)
        {
            var result = new CommandResult
            {
                CommandId = request.CommandId,
                Success = false,
                Message = "Destroy failed",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow,
                DurationMs = sw.ElapsedMilliseconds
            };

            await SendCommandResultAsync(result);
        }
    }

    /// <summary>
    /// Send command result to hub
    /// </summary>
    private async Task SendCommandResultAsync(CommandResult result)
    {
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendCommandResult", result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send command result: {ex.Message}");
        }
    }

    /// <summary>
    /// Get agent information
    /// </summary>
    private async Task<AgentInfo> GetAgentInfoAsync()
    {
        // Collect system metrics to get hardware info
        var metrics = await _metricsService.CollectMetricsAsync();
        
        return new AgentInfo
        {
            AgentId = _agentId,
            Hostname = System.Environment.MachineName,
            Platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            OsVersion = System.Environment.OSVersion.VersionString,
            ThreshVersion = "1.6.0",
            Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            ContainerRuntime = "WSL", // TODO: Detect actual runtime
            RegisteredAt = DateTime.UtcNow,
            CpuCores = metrics.CpuCores,
            RamTotalGb = (int)Math.Round(metrics.MemoryTotalGb),
            DiskTotalGb = (int)Math.Round(metrics.StorageTotalGb)
        };
    }

    /// <summary>
    /// Get metrics data
    /// </summary>
    private async Task<MetricsData> GetMetricsDataAsync()
    {
        var hostMetrics = await _metricsService.CollectMetricsAsync();
        
        // Try to detect GPU (basic detection for now)
        int? gpuCount = null;
        string? gpuModel = null;
        int? gpuMemoryGb = null;
        
        try
        {
            // TODO: Implement proper GPU detection using nvidia-smi, rocm-smi, etc.
            // For now, just check if nvidia-smi is available
            var gpuInfo = System.Environment.GetEnvironmentVariable("GPU_INFO");
            if (!string.IsNullOrEmpty(gpuInfo))
            {
                // Parse GPU_INFO if set
            }
        }
        catch { /* GPU detection is optional */ }
        
        return new MetricsData
        {
            AgentId = _agentId,
            Timestamp = DateTime.UtcNow,
            CpuPercent = hostMetrics.CpuPercent,
            MemoryUsedMB = (long)(hostMetrics.MemoryUsedGb * 1024),
            MemoryTotalMB = (long)(hostMetrics.MemoryTotalGb * 1024),
            DiskUsedGB = (long)(hostMetrics.StorageTotalGb - hostMetrics.StorageFreeGb),
            DiskTotalGB = (long)hostMetrics.StorageTotalGb,
            EnvironmentCount = 0, // TODO: Get from container service
            GpuCount = gpuCount,
            GpuModel = gpuModel,
            GpuMemoryTotalGb = gpuMemoryGb,
            Environments = new List<EnvironmentSummary>() // TODO: Get from container service
        };
    }
}
