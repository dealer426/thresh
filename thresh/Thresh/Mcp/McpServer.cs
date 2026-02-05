using System.Net;
using System.Text;
using System.Text.Json;
using EknovaCli.Mcp.Models;
using EknovaCli.Services;

namespace EknovaCli.Mcp;

/// <summary>
/// MCP (Model Context Protocol) Server for eknova
/// Exposes CLI functionality to AI agents via HTTP endpoints
/// </summary>
public class McpServer
{
    private readonly WslService _wslService;
    private readonly BlueprintService _blueprintService;
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts;
    private readonly int _port;
    private readonly string _host;

    public McpServer(int port = 8080, string host = "localhost")
    {
        _port = port;
        _host = host;
        _wslService = new WslService();
        var rootfsRegistry = new RootfsRegistry();
        _blueprintService = new BlueprintService(_wslService, rootfsRegistry);
        _listener = new HttpListener();
        _cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Start the MCP server
    /// </summary>
    public async Task StartAsync()
    {
        // Add HTTP listener prefix
        var prefix = $"http://{_host}:{_port}/";
        _listener.Prefixes.Add(prefix);
        
        try
        {
            _listener.Start();
            Console.WriteLine($"✅ MCP Server started on {prefix}");
            Console.WriteLine();
            Console.WriteLine("Available endpoints:");
            Console.WriteLine($"  GET  {prefix}mcp/initialize");
            Console.WriteLine($"  POST {prefix}mcp/tools/call");
            Console.WriteLine();
            Console.WriteLine("Available tools:");
            Console.WriteLine("  - list_environments: List all WSL environments");
            Console.WriteLine("  - provision_environment: Create new environment from blueprint");
            Console.WriteLine("  - list_blueprints: Show available blueprints");
            Console.WriteLine("  - get_blueprint: Get blueprint details");
            Console.WriteLine("  - destroy_environment: Remove an environment");
            Console.WriteLine("  - check_requirements: Verify system setup");
            Console.WriteLine();
            Console.WriteLine("Press Ctrl+C to stop");
            Console.WriteLine();

            // Handle requests
            await HandleRequestsAsync();
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 5)
        {
            Console.WriteLine("❌ Access denied. Try running with administrator privileges or use a different port.");
            Console.WriteLine($"   On Linux: sudo dotnet run -- serve --port {_port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to start server: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop the MCP server
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
        _listener.Close();
        Console.WriteLine("\n✅ MCP Server stopped");
    }

    /// <summary>
    /// Handle incoming HTTP requests
    /// </summary>
    private async Task HandleRequestsAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => ProcessRequestAsync(context), _cts.Token);
            }
            catch (HttpListenerException) when (_cts.Token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error handling request: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Process individual HTTP request
    /// </summary>
    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var path = request.Url?.AbsolutePath ?? "";
            var method = request.HttpMethod;

            Console.WriteLine($"{method} {path}");

            // Set CORS headers
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            // Handle OPTIONS preflight
            if (method == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            // Route requests
            object? responseData = null;

            if (method == "GET" && path == "/mcp/initialize")
            {
                responseData = HandleInitialize();
            }
            else if (method == "POST" && path == "/mcp/tools/call")
            {
                responseData = await HandleToolCallAsync(request);
            }
            else
            {
                response.StatusCode = 404;
                responseData = new { error = "Not found" };
            }

            // Send response
            response.ContentType = "application/json";
            response.StatusCode = responseData != null ? 200 : 500;
            
            string json;
            if (responseData is InitializeResponse initResp)
            {
                json = JsonSerializer.Serialize(initResp, McpJsonContext.Default.InitializeResponse);
            }
            else if (responseData is ToolCallResponse toolResp)
            {
                json = JsonSerializer.Serialize(toolResp, McpJsonContext.Default.ToolCallResponse);
            }
            else
            {
                json = "{}";
            }
            
            var buffer = Encoding.UTF8.GetBytes(json);
            
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing request: {ex.Message}");
            try
            {
                response.StatusCode = 500;
                var errorResponse = ToolCallResponse.Error(ex.Message);
                var error = JsonSerializer.Serialize(errorResponse, McpJsonContext.Default.ToolCallResponse);
                var buffer = Encoding.UTF8.GetBytes(error);
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            catch
            {
                // Ignore if we can't send error response
            }
        }
    }

    /// <summary>
    /// Handle /mcp/initialize endpoint
    /// </summary>
    private InitializeResponse HandleInitialize()
    {
        return new InitializeResponse
        {
            ProtocolVersion = "2024-11-05",
            ServerInfo = new ServerInfo
            {
                Name = "thresh-mcp-server",
                Version = "1.0.0"
            },
            Capabilities = new Capabilities
            {
                Tools = new List<Tool>
                {
                    new() { Name = "list_environments", Description = "List all WSL environments managed by thresh" },
                    new() { Name = "provision_environment", Description = "Provision a new WSL environment from a blueprint" },
                    new() { Name = "list_blueprints", Description = "List all available blueprints" },
                    new() { Name = "get_blueprint", Description = "Get details of a specific blueprint" },
                    new() { Name = "destroy_environment", Description = "Destroy a WSL environment" },
                    new() { Name = "check_requirements", Description = "Check system requirements for thresh" }
                }
            }
        };
    }

    /// <summary>
    /// Handle /mcp/tools/call endpoint
    /// </summary>
    private async Task<ToolCallResponse> HandleToolCallAsync(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        
        var toolRequest = JsonSerializer.Deserialize(body, McpJsonContext.Default.ToolCallRequest);
        if (toolRequest == null)
        {
            return ToolCallResponse.Error("Invalid request body");
        }

        try
        {
            return toolRequest.Name switch
            {
                "list_environments" => await ListEnvironmentsAsync(),
                "provision_environment" => await ProvisionEnvironmentAsync(toolRequest),
                "list_blueprints" => ListBlueprints(),
                "get_blueprint" => await GetBlueprintAsync(toolRequest),
                "destroy_environment" => await DestroyEnvironmentAsync(toolRequest),
                "check_requirements" => await CheckRequirementsAsync(),
                _ => ToolCallResponse.Error($"Unknown tool: {toolRequest.Name}")
            };
        }
        catch (Exception ex)
        {
            return ToolCallResponse.Error($"Error executing tool: {ex.Message}");
        }
    }

    // Tool Implementations

    private async Task<ToolCallResponse> ListEnvironmentsAsync()
    {
        try
        {
            var environments = await _wslService.ListEnvironmentsAsync();
            var result = new StringBuilder();
            result.AppendLine($"WSL Environments ({environments.Count}):");
            result.AppendLine();

            foreach (var env in environments)
            {
                result.AppendLine($"  - {env.Name} [{env.Status}]");
            }

            return ToolCallResponse.Success(result.ToString());
        }
        catch (Exception ex)
        {
            return ToolCallResponse.Error($"Failed to list environments: {ex.Message}");
        }
    }

    private async Task<ToolCallResponse> ProvisionEnvironmentAsync(ToolCallRequest request)
    {
        var args = request.Arguments;
        if (args == null || !args.TryGetValue("blueprint", out var blueprintObj) || !args.TryGetValue("name", out var nameObj))
        {
            return ToolCallResponse.Error("Missing required arguments: blueprint and name");
        }

        var blueprintName = blueprintObj?.ToString();
        var name = nameObj?.ToString();

        if (string.IsNullOrEmpty(blueprintName) || string.IsNullOrEmpty(name))
        {
            return ToolCallResponse.Error("Invalid blueprint or name");
        }

        try
        {
            var blueprint = _blueprintService.LoadBundledBlueprint(blueprintName);
            await _blueprintService.ProvisionEnvironmentAsync(name, blueprint, verbose: false);
            
            return ToolCallResponse.Success($"Environment '{name}' successfully provisioned from blueprint: {blueprintName}");
        }
        catch (Exception ex)
        {
            return ToolCallResponse.Error($"Failed to provision environment: {ex.Message}");
        }
    }

    private ToolCallResponse ListBlueprints()
    {
        var result = new StringBuilder();
        result.AppendLine("Available Blueprints:");
        result.AppendLine();
        result.AppendLine("  - alpine-minimal: Alpine Linux minimal environment");
        result.AppendLine("  - alpine-python: Alpine with Python");
        result.AppendLine("  - ubuntu-dev: Ubuntu development environment");
        result.AppendLine("  - ubuntu-python: Ubuntu with Python tools");
        result.AppendLine("  - debian-stable: Debian stable environment");
        result.AppendLine("  - node-dev: Node.js development");
        result.AppendLine("  - python-dev: Python development");
        result.AppendLine("  - azure-cli: Azure CLI tools");

        return ToolCallResponse.Success(result.ToString());
    }

    private async Task<ToolCallResponse> GetBlueprintAsync(ToolCallRequest request)
    {
        var args = request.Arguments;
        if (args == null || !args.TryGetValue("name", out var nameObj))
        {
            return ToolCallResponse.Error("Missing required argument: name");
        }

        var blueprintName = nameObj?.ToString();
        if (string.IsNullOrEmpty(blueprintName))
        {
            return ToolCallResponse.Error("Invalid blueprint name");
        }

        try
        {
            var blueprint = _blueprintService.LoadBundledBlueprint(blueprintName);

            var result = new StringBuilder();
            result.AppendLine($"Blueprint: {blueprintName}");
            result.AppendLine();
            result.AppendLine($"Description: {blueprint.Description}");
            result.AppendLine($"Base: {blueprint.Base}");
            result.AppendLine();

            if (blueprint.Packages?.Count > 0)
            {
                result.AppendLine($"Packages ({blueprint.Packages.Count}):");
                foreach (var pkg in blueprint.Packages)
                {
                    result.AppendLine($"  - {pkg}");
                }
            }

            if (blueprint.Environment?.Count > 0)
            {
                result.AppendLine();
                result.AppendLine("Environment Variables:");
                foreach (var kvp in blueprint.Environment)
                {
                    result.AppendLine($"  {kvp.Key}={kvp.Value}");
                }
            }

            var hasScripts = !string.IsNullOrEmpty(blueprint.Scripts?.Setup) || !string.IsNullOrEmpty(blueprint.Scripts?.PostInstall);
            if (hasScripts)
            {
                result.AppendLine();
                result.AppendLine("Scripts:");
                if (!string.IsNullOrEmpty(blueprint.Scripts?.Setup))
                    result.AppendLine("  - setup");
                if (!string.IsNullOrEmpty(blueprint.Scripts?.PostInstall))
                    result.AppendLine("  - postInstall");
            }

            return ToolCallResponse.Success(result.ToString());
        }
        catch (Exception ex)
        {
            return ToolCallResponse.Error($"Failed to get blueprint: {ex.Message}");
        }
    }

    private async Task<ToolCallResponse> DestroyEnvironmentAsync(ToolCallRequest request)
    {
        var args = request.Arguments;
        if (args == null || !args.TryGetValue("name", out var nameObj))
        {
            return ToolCallResponse.Error("Missing required argument: name");
        }

        var envName = nameObj?.ToString();
        if (string.IsNullOrEmpty(envName))
        {
            return ToolCallResponse.Error("Invalid environment name");
        }

        try
        {
            await _wslService.RemoveEnvironmentAsync(envName);
            return ToolCallResponse.Success($"Environment '{envName}' destroyed successfully");
        }
        catch (Exception ex)
        {
            return ToolCallResponse.Error($"Failed to destroy environment: {ex.Message}");
        }
    }

    private async Task<ToolCallResponse> CheckRequirementsAsync()
    {
        try
        {
            var result = new StringBuilder();
            result.AppendLine("System Requirements Check:");
            result.AppendLine();

            // Check WSL availability
            var wslAvailable = await _wslService.IsWslAvailableAsync();
            result.AppendLine($"  WSL: {(wslAvailable ? "✅ Available" : "❌ Not available")}");

            if (wslAvailable)
            {
                var wslInfo = await _wslService.GetWslInfoAsync();
                if (wslInfo != null)
                {
                    result.AppendLine($"  WSL Version: {wslInfo.Version}");
                    result.AppendLine($"  Distributions: {wslInfo.DistributionCount}");
                }
            }

            // Check .NET
            result.AppendLine($"  .NET: ✅ {System.Environment.Version}");

            // Check OS
            result.AppendLine($"  OS: {System.Environment.OSVersion}");

            return ToolCallResponse.Success(result.ToString());
        }
        catch (Exception ex)
        {
            return ToolCallResponse.Error($"Failed to check requirements: {ex.Message}");
        }
    }
}
