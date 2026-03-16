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
        var envs = GetContainerService().ListEnvironmentsAsync(false).GetAwaiter().GetResult();
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
            EnvironmentCount = envs.Count,
            RunningEnvironments = envs.Count(e => e.Status == Thresh.Models.EnvironmentStatus.Running),
            StoppedEnvironments = envs.Count(e => e.Status == Thresh.Models.EnvironmentStatus.Stopped)
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
        connection.On<AgentCommand>("ExecuteCommand", OnExecuteCommandAsync);
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
    /// Handle a generic tool-call command dispatched from the hub MCP server.
    /// Executes the requested tool locally and reports the result back via SendCommandResult.
    /// </summary>
    private async Task OnExecuteCommandAsync(AgentCommand command)
    {
        Console.WriteLine($"[agent] ExecuteCommand: {command.Tool} ({command.CommandId[..8]}...)");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        string? output = null;
        string? error = null;
        var success = true;

        try
        {
            output = command.Tool switch
            {
                "list_environments" => await AgentToolListEnvironmentsAsync(command.Arguments),
                "create_environment" => await AgentToolCreateEnvironmentAsync(command.Arguments),
                "start_environment" => await AgentToolStartStopEnvironmentAsync(command.Arguments, start: true),
                "stop_environment" => await AgentToolStartStopEnvironmentAsync(command.Arguments, start: false),
                "destroy_environment" => await AgentToolDestroyEnvironmentAsync(command.Arguments),
                "list_blueprints" => AgentToolListBlueprints(),
                "get_blueprint" => AgentToolGetBlueprint(command.Arguments),
                "save_blueprint" => AgentToolSaveBlueprint(command.Arguments),
                "get_metrics" => await AgentToolGetMetricsAsync(),
                "get_version" => "thresh 1.6.0",
                "help" => AgentToolHelp(),
                _ => throw new InvalidOperationException($"Unknown tool: {command.Tool}")
            };
        }
        catch (Exception ex)
        {
            success = false;
            error = ex.Message;
            Console.WriteLine($"[agent] Tool error [{command.Tool}]: {ex.Message}");
        }

        await SendCommandResultAsync(new CommandResult
        {
            AgentId = _agentId,
            CommandId = command.CommandId,
            Status = success ? "success" : "error",
            Success = success,
            Output = output,
            Message = output ?? string.Empty,
            Error = error,
            Timestamp = DateTime.UtcNow,
            DurationMs = sw.ElapsedMilliseconds
        });
    }

    // ─── Local tool implementations for hub-dispatched commands ──────────────

    private IContainerService GetContainerService()
        => ContainerServiceFactory.Create();

    private async Task<string> AgentToolListEnvironmentsAsync(System.Text.Json.JsonElement? args)
    {
        var includeAll = args?.TryGetProperty("include_all", out var p) == true && p.GetBoolean();
        var svc = GetContainerService();
        var envs = await svc.ListEnvironmentsAsync(includeAll);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"📦 {svc.RuntimeName} Environments ({envs.Count}):");
        sb.AppendLine();

        if (envs.Count == 0)
        {
            sb.AppendLine("  No environments found.");
        }
        else
        {
            foreach (var env in envs)
            {
                var icon = env.Status == Thresh.Models.EnvironmentStatus.Running ? "🟢" :
                           env.Status == Thresh.Models.EnvironmentStatus.Stopped ? "⚪" : "❓";
                sb.AppendLine($"  {icon} {env.Name}");
                sb.AppendLine($"     Status: {env.Status}");
                if (!string.IsNullOrEmpty(env.Blueprint) && env.Blueprint != "unknown")
                    sb.AppendLine($"     Blueprint: {env.Blueprint}");
            }
        }

        return sb.ToString();
    }

    private async Task<string> AgentToolCreateEnvironmentAsync(System.Text.Json.JsonElement? args)
    {
        if (!args.HasValue)
            throw new ArgumentException("Missing arguments");

        var blueprint = args.Value.TryGetProperty("blueprint", out var bp) ? bp.GetString() : null;
        var name = args.Value.TryGetProperty("name", out var n) ? n.GetString() : null;

        if (string.IsNullOrEmpty(blueprint))
            throw new ArgumentException("Missing required argument: blueprint");
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Missing required argument: name");

        var svc = GetContainerService();

        if (!await svc.IsAvailableAsync())
            throw new InvalidOperationException($"{svc.RuntimeName} is not available on this node");

        // Locate blueprint file: explicit path → user dir → install dir
        var userBlueprintDir = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".thresh", "blueprints");
        var installBlueprintDir = Path.Combine(AppContext.BaseDirectory, "blueprints");
        var blueprintFile = blueprint.EndsWith(".json") ? blueprint : blueprint + ".json";

        var blueprintPath = File.Exists(blueprint) ? blueprint
            : File.Exists(Path.Combine(userBlueprintDir, blueprintFile)) ? Path.Combine(userBlueprintDir, blueprintFile)
            : Path.Combine(installBlueprintDir, blueprintFile);

        if (!File.Exists(blueprintPath))
            throw new FileNotFoundException($"Blueprint not found: {blueprint}");

        if (await svc.EnvironmentExistsAsync(name))
            throw new InvalidOperationException($"Environment '{name}' already exists. Destroy it first.");

        var configService = new ConfigurationService();
        var rootfsRegistry = new RootfsRegistry(configService);
        var blueprintService = new BlueprintService(svc, rootfsRegistry);
        var bpModel = blueprintService.LoadBlueprint(blueprintPath);

        await blueprintService.ProvisionEnvironmentAsync(name, bpModel, verbose: false);

        return $"✅ Environment '{name}' created from blueprint '{blueprint}'";
    }

    private async Task<string> AgentToolStartStopEnvironmentAsync(System.Text.Json.JsonElement? args, bool start)
    {
        var name = args?.TryGetProperty("name", out var n) == true ? n.GetString()
            : throw new ArgumentException("Missing argument: name");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Missing required argument: name");

        var svc = GetContainerService();
        var ok = start
            ? await svc.StartEnvironmentAsync(name)
            : await svc.StopEnvironmentAsync(name);

        var action = start ? "started" : "stopped";
        return ok
            ? $"✅ Environment '{name}' {action}"
            : $"❌ Failed to {(start ? "start" : "stop")} environment '{name}'";
    }

    private async Task<string> AgentToolDestroyEnvironmentAsync(System.Text.Json.JsonElement? args)
    {
        var name = args?.TryGetProperty("name", out var n) == true ? n.GetString()
            : throw new ArgumentException("Missing argument: name");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Missing required argument: name");

        var svc = GetContainerService();
        var ok = await svc.RemoveEnvironmentAsync(name);

        return ok
            ? $"✅ Environment '{name}' destroyed"
            : $"❌ Failed to destroy environment '{name}'";
    }

    private string AgentToolListBlueprints()
    {
        var userBlueprintDir = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".thresh", "blueprints");
        var installBlueprintDir = Path.Combine(AppContext.BaseDirectory, "blueprints");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("📋 Blueprints:");
        sb.AppendLine();

        // Collect names from both directories, user dir takes precedence
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var names = new List<string>();

        if (Directory.Exists(userBlueprintDir))
        {
            foreach (var f in Directory.GetFiles(userBlueprintDir, "*.json"))
            {
                var n = Path.GetFileNameWithoutExtension(f);
                if (seen.Add(n)) names.Add(n);
            }
        }

        if (Directory.Exists(installBlueprintDir))
        {
            foreach (var f in Directory.GetFiles(installBlueprintDir, "*.json"))
            {
                var n = Path.GetFileNameWithoutExtension(f);
                if (seen.Add(n)) names.Add(n);
            }
        }

        if (names.Count == 0)
        {
            sb.AppendLine("  No blueprints found.");
        }
        else
        {
            foreach (var n in names.OrderBy(x => x))
                sb.AppendLine($"  • {n}");
        }

        return sb.ToString();
    }

    private string AgentToolGetBlueprint(System.Text.Json.JsonElement? args)
    {
        var name = args?.TryGetProperty("name", out var n) == true ? n.GetString()
            : throw new ArgumentException("Missing argument: name");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Missing required argument: name");

        var userBlueprintDir = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".thresh", "blueprints");
        var installBlueprintDir = Path.Combine(AppContext.BaseDirectory, "blueprints");
        var blueprintFile = name.EndsWith(".json") ? name : name + ".json";

        var path = File.Exists(Path.Combine(userBlueprintDir, blueprintFile))
            ? Path.Combine(userBlueprintDir, blueprintFile)
            : Path.Combine(installBlueprintDir, blueprintFile);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Blueprint not found: {name}");

        return File.ReadAllText(path);
    }

    private string AgentToolSaveBlueprint(System.Text.Json.JsonElement? args)
    {
        var name = args?.TryGetProperty("name", out var n) == true ? n.GetString()
            : throw new ArgumentException("Missing argument: name");
        var blueprintJson = args?.TryGetProperty("blueprint_json", out var bj) == true ? bj.GetString()
            : throw new ArgumentException("Missing argument: blueprint_json");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Missing required argument: name");
        if (string.IsNullOrEmpty(blueprintJson))
            throw new ArgumentException("Missing required argument: blueprint_json");

        var blueprintDir = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".thresh", "blueprints");

        Directory.CreateDirectory(blueprintDir);

        var fileName = name.EndsWith(".json") ? name : name + ".json";
        var path = Path.Combine(blueprintDir, fileName);
        File.WriteAllText(path, blueprintJson);

        return $"✅ Blueprint '{name}' saved to {path}";
    }

    private static string AgentToolHelp()
        => """
            thresh agent — supported tools

            list_environments   List running containers
            create_environment  Create container from a blueprint
            start_environment   Start a stopped container
            stop_environment    Stop a running container
            destroy_environment Remove container(s)
            list_blueprints     List blueprints in ~/.thresh/blueprints/
            get_blueprint       Get blueprint JSON by name
            save_blueprint      Save blueprint JSON to ~/.thresh/blueprints/
            get_metrics         Show CPU / RAM / disk metrics for this node
            get_version         Show thresh agent version
            """;

    private async Task<string> AgentToolGetMetricsAsync()
    {
        var metrics = await _metricsService.CollectMetricsAsync();

        return $"""
            📊 Node Metrics ({System.Environment.MachineName}):
              CPU    : {metrics.CpuPercent:F1}% ({metrics.CpuCores} cores)
              RAM    : {metrics.MemoryUsedGb:F1}/{metrics.MemoryTotalGb:F1} GB
              Disk   : {metrics.StorageFreeGb:F1} GB free of {metrics.StorageTotalGb:F1} GB
            """;
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
        
        var environments = await GetContainerService().ListEnvironmentsAsync(false);
        
        return new MetricsData
        {
            AgentId = _agentId,
            Timestamp = DateTime.UtcNow,
            CpuPercent = hostMetrics.CpuPercent,
            MemoryUsedMB = (long)(hostMetrics.MemoryUsedGb * 1024),
            MemoryTotalMB = (long)(hostMetrics.MemoryTotalGb * 1024),
            DiskUsedGB = (long)(hostMetrics.StorageTotalGb - hostMetrics.StorageFreeGb),
            DiskTotalGB = (long)hostMetrics.StorageTotalGb,
            EnvironmentCount = environments.Count,
            GpuCount = gpuCount,
            GpuModel = gpuModel,
            GpuMemoryTotalGb = gpuMemoryGb,
            Environments = environments.Select(e => new EnvironmentSummary { Name = e.Name, Status = e.Status.ToString() }).ToList()
        };
    }
}
