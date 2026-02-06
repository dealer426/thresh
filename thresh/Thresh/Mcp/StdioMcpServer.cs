using System.Text;
using System.Text.Json;
using Thresh.Mcp.Models;
using Thresh.Services;

namespace Thresh.Mcp;

/// <summary>
/// MCP Server using STDIO transport (JSON-RPC over stdin/stdout)
/// Compatible with VS Code, Cursor, Windsurf, and other MCP clients
/// </summary>
public class StdioMcpServer
{
    private readonly IContainerService _containerService;
    private readonly BlueprintService _blueprintService;
    private readonly ConfigurationService _configService;
    private readonly CancellationTokenSource _cts;
    private bool _initialized;

    public StdioMcpServer()
    {
        _containerService = ContainerServiceFactory.Create();
        _configService = new ConfigurationService();
        var rootfsRegistry = new RootfsRegistry(_configService);
        _blueprintService = new BlueprintService(_containerService, rootfsRegistry);
        _cts = new CancellationTokenSource();
        _initialized = false;
    }

    /// <summary>
    /// Start the stdio MCP server
    /// </summary>
    public async Task RunAsync()
    {
        // Log to stderr (stdout is for JSON-RPC messages)
        await Console.Error.WriteLineAsync("🚀 thresh MCP server started (stdio mode)");
        await Console.Error.WriteLineAsync($"Platform: {_containerService.Platform}");
        await Console.Error.WriteLineAsync($"Runtime: {_containerService.RuntimeName}");
        await Console.Error.WriteLineAsync();

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var line = await Console.In.ReadLineAsync();
                if (line == null) break; // EOF

                if (string.IsNullOrWhiteSpace(line)) continue;

                var response = await ProcessMessageAsync(line);
                if (response != null)
                {
                    await Console.Out.WriteLineAsync(response);
                    await Console.Out.FlushAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop the server
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
    }

    /// <summary>
    /// Process a JSON-RPC message
    /// </summary>
    private async Task<string?> ProcessMessageAsync(string messageJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(messageJson);
            var root = doc.RootElement;

            // Extract JSON-RPC fields
            var id = root.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : (int?)null;
            var method = root.TryGetProperty("method", out var methodProp) ? methodProp.GetString() : null;
            var paramsElement = root.TryGetProperty("params", out var paramsProp) ? paramsProp : (JsonElement?)null;

            if (string.IsNullOrEmpty(method))
            {
                return CreateErrorResponse(id, -32600, "Invalid Request: missing method");
            }

            await Console.Error.WriteLineAsync($"📨 {method}");

            // Route to handler
            var result = method switch
            {
                "initialize" => HandleInitialize(paramsElement),
                "notifications/initialized" => HandleInitialized(),
                "tools/list" => HandleListTools(),
                "tools/call" => await HandleToolCallAsync(paramsElement),
                "ping" => CreateSuccessResponse(id, new { status = "ok" }),
                _ => CreateErrorResponse(id, -32601, $"Method not found: {method}")
            };

            return result;
        }
        catch (JsonException ex)
        {
            await Console.Error.WriteLineAsync($"⚠️  JSON parse error: {ex.Message}");
            return CreateErrorResponse(null, -32700, "Parse error");
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"❌ Error processing message: {ex.Message}");
            return CreateErrorResponse(null, -32603, $"Internal error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle initialize request
    /// </summary>
    private string HandleInitialize(JsonElement? paramsElement)
    {
        var response = new InitializeResult
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new
            {
                tools = new { }
            },
            ServerInfo = new ServerInfoResult
            {
                Name = "thresh",
                Version = "1.0.0"
            },
            Instructions = "thresh is a cross-platform development environment manager. " +
                          "Use it to create, manage, and destroy containerized dev environments."
        };

        var jsonResponse = new JsonRpcResponse<InitializeResult> { Result = response };
        return JsonSerializer.Serialize(jsonResponse, McpJsonContext.Default.JsonRpcResponseInitializeResult);
    }

    /// <summary>
    /// Handle initialized notification (no response needed)
    /// </summary>
    private string? HandleInitialized()
    {
        // Notification - no response
        return null;
    }

    /// <summary>
    /// Handle tools/list request
    /// </summary>
    private string HandleListTools()
    {
        object[] tools = new[]
        {
            new
            {
                name = "list_environments",
                description = $"List all {_containerService.RuntimeName} development environments managed by thresh",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        include_all = new
                        {
                            type = "boolean",
                            description = "Include all containers, not just thresh-managed ones"
                        }
                    }
                }
            },
            new
            {
                name = "create_environment",
                description = "Create a new development environment from a blueprint",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        blueprint = new
                        {
                            type = "string",
                            description = "Blueprint name (e.g., 'python-dev', 'node-dev', 'ubuntu-dev')"
                        },
                        name = new
                        {
                            type = "string",
                            description = "Name for the new environment"
                        },
                        verbose = new
                        {
                            type = "boolean",
                            description = "Show detailed provisioning output"
                        }
                    },
                    required = new[] { "blueprint", "name" }
                }
            },
            new
            {
                name = "destroy_environment",
                description = "Destroy/remove a development environment",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new
                        {
                            type = "string",
                            description = "Name of the environment to destroy"
                        }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "list_blueprints",
                description = "List all available blueprints for creating environments",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "get_blueprint",
                description = "Get detailed information about a specific blueprint",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new
                        {
                            type = "string",
                            description = "Blueprint name"
                        }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "get_version",
                description = "Get thresh version and runtime information",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "generate_blueprint",
                description = "Generate a custom blueprint using AI from a natural language description",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        prompt = new
                        {
                            type = "string",
                            description = "Natural language description of the desired environment"
                        },
                        model = new
                        {
                            type = "string",
                            description = "AI model to use (default: gpt-4o)"
                        }
                    },
                    required = new[] { "prompt" }
                }
            }
        };

        var listResult = new ToolsListResult { Tools = tools };
        var response = new JsonRpcResponse<ToolsListResult> { Result = listResult };
        return JsonSerializer.Serialize(response, McpJsonContext.Default.JsonRpcResponseToolsListResult);
    }

    /// <summary>
    /// Handle tools/call request
    /// </summary>
    private async Task<string> HandleToolCallAsync(JsonElement? paramsElement)
    {
        if (!paramsElement.HasValue)
        {
            return CreateErrorResponse(null, -32602, "Invalid params: missing");
        }

        var toolName = paramsElement.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
        var arguments = paramsElement.Value.TryGetProperty("arguments", out var argsProp) ? argsProp : (JsonElement?)null;

        if (string.IsNullOrEmpty(toolName))
        {
            return CreateErrorResponse(null, -32602, "Invalid params: missing tool name");
        }

        try
        {
            var result = toolName switch
            {
                "list_environments" => await ListEnvironmentsAsync(arguments),
                "create_environment" => await CreateEnvironmentAsync(arguments),
                "destroy_environment" => await DestroyEnvironmentAsync(arguments),
                "list_blueprints" => ListBlueprints(),
                "get_blueprint" => GetBlueprint(arguments),
                "get_version" => await GetVersionAsync(),
                "generate_blueprint" => await GenerateBlueprintAsync(arguments),
                _ => CreateToolError($"Unknown tool: {toolName}")
            };

            var genericResult = new GenericResult { Result = result };
            var response = new JsonRpcResponse<GenericResult> { Result = genericResult };
            return JsonSerializer.Serialize(response, McpJsonContext.Default.JsonRpcResponseGenericResult);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"❌ Tool error: {ex.Message}");
            return CreateToolErrorResponse(ex.Message);
        }
    }

    // Tool Implementations

    private async Task<object> ListEnvironmentsAsync(JsonElement? args)
    {
        var includeAll = args?.TryGetProperty("include_all", out var prop) == true && prop.GetBoolean();
        
        var environments = await _containerService.ListEnvironmentsAsync(includeAll);
        
        var sb = new StringBuilder();
        sb.AppendLine($"📦 {_containerService.RuntimeName} Environments ({environments.Count}):");
        sb.AppendLine();
        
        if (environments.Count == 0)
        {
            sb.AppendLine("  No environments found.");
            if (!includeAll)
                sb.AppendLine($"  Tip: Use include_all=true to see all {_containerService.RuntimeName} containers");
        }
        else
        {
            foreach (var env in environments)
            {
                var statusIcon = env.Status == Thresh.Models.EnvironmentStatus.Running ? "🟢" :
                               env.Status == Thresh.Models.EnvironmentStatus.Stopped ? "⚪" : "❓";
                sb.AppendLine($"  {statusIcon} {env.Name}");
                sb.AppendLine($"     Status: {env.Status}");
                if (!string.IsNullOrEmpty(env.Blueprint) && env.Blueprint != "unknown")
                    sb.AppendLine($"     Blueprint: {env.Blueprint}");
            }
        }

        return new
        {
            content = new[]
            {
                new { type = "text", text = sb.ToString() }
            }
        };
    }

    private async Task<object> CreateEnvironmentAsync(JsonElement? args)
    {
        if (!args.HasValue)
            return CreateToolError("Missing arguments");

        var blueprint = args.Value.TryGetProperty("blueprint", out var bpProp) ? bpProp.GetString() : null;
        var name = args.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
        var verbose = args.Value.TryGetProperty("verbose", out var verboseProp) && verboseProp.GetBoolean();

        if (string.IsNullOrEmpty(blueprint) || string.IsNullOrEmpty(name))
            return CreateToolError("Missing required arguments: blueprint and name");

        // Check if environment exists
        if (await _containerService.EnvironmentExistsAsync(name))
            return CreateToolError($"Environment '{name}' already exists");

        // Load and provision blueprint
        var bp = _blueprintService.LoadBundledBlueprint(blueprint);
        
        var output = new StringBuilder();
        output.AppendLine($"🚀 Creating environment '{name}' from blueprint '{blueprint}'...");
        output.AppendLine();
        
        // Capture provisioning output
        var originalOut = Console.Out;
        using (var writer = new StringWriter())
        {
            Console.SetOut(writer);
            await _blueprintService.ProvisionEnvironmentAsync(name, bp, verbose);
            Console.SetOut(originalOut);
            output.Append(writer.ToString());
        }

        output.AppendLine();
        output.AppendLine($"✅ Environment '{name}' created successfully!");

        return new
        {
            content = new[]
            {
                new { type = "text", text = output.ToString() }
            }
        };
    }

    private async Task<object> DestroyEnvironmentAsync(JsonElement? args)
    {
        if (!args.HasValue)
            return CreateToolError("Missing arguments");

        var name = args.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

        if (string.IsNullOrEmpty(name))
            return CreateToolError("Missing required argument: name");

        if (!await _containerService.EnvironmentExistsAsync(name))
            return CreateToolError($"Environment '{name}' not found");

        var success = await _containerService.RemoveEnvironmentAsync(name);
        
        var message = success 
            ? $"✅ Environment '{name}' destroyed successfully"
            : $"❌ Failed to destroy environment '{name}'";

        return new
        {
            content = new[]
            {
                new { type = "text", text = message }
            }
        };
    }

    private object ListBlueprints()
    {
        var blueprints = _blueprintService.ListBundledBlueprints();
        
        var sb = new StringBuilder();
        sb.AppendLine($"📋 Available Blueprints ({blueprints.Count}):");
        sb.AppendLine();

        foreach (var name in blueprints.OrderBy(b => b))
        {
            sb.AppendLine($"  • {name}");
        }

        return new
        {
            content = new[]
            {
                new { type = "text", text = sb.ToString() }
            }
        };
    }

    private object GetBlueprint(JsonElement? args)
    {
        if (!args.HasValue)
            return CreateToolError("Missing arguments");

        var name = args.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

        if (string.IsNullOrEmpty(name))
            return CreateToolError("Missing required argument: name");

        var blueprint = _blueprintService.LoadBundledBlueprint(name);
        
        var sb = new StringBuilder();
        sb.AppendLine($"📋 Blueprint: {blueprint.Name}");
        sb.AppendLine();
        sb.AppendLine($"Description: {blueprint.Description}");
        sb.AppendLine($"Base: {blueprint.Base}");
        sb.AppendLine();

        if (blueprint.Packages?.Count > 0)
        {
            sb.AppendLine($"Packages ({blueprint.Packages.Count}):");
            foreach (var pkg in blueprint.Packages)
                sb.AppendLine($"  - {pkg}");
            sb.AppendLine();
        }

        if (blueprint.Environment?.Count > 0)
        {
            sb.AppendLine("Environment Variables:");
            foreach (var kvp in blueprint.Environment)
                sb.AppendLine($"  {kvp.Key}={kvp.Value}");
            sb.AppendLine();
        }

        if (blueprint.Scripts != null)
        {
            if (!string.IsNullOrEmpty(blueprint.Scripts.Setup))
                sb.AppendLine($"Setup Script: ✓");
            if (!string.IsNullOrEmpty(blueprint.Scripts.PostInstall))
                sb.AppendLine($"Post-Install Script: ✓");
        }

        return new
        {
            content = new[]
            {
                new { type = "text", text = sb.ToString() }
            }
        };
    }

    private async Task<object> GetVersionAsync()
    {
        var runtimeInfo = await _containerService.GetRuntimeInfoAsync();
        
        var sb = new StringBuilder();
        sb.AppendLine("thresh v1.0.0-phase0");
        sb.AppendLine();
        sb.AppendLine($"Platform: {_containerService.Platform}");
        sb.AppendLine($"Runtime: {_containerService.RuntimeName}");
        
        if (runtimeInfo.IsAvailable)
        {
            sb.AppendLine($"Version: {runtimeInfo.Version}");
            if (runtimeInfo.Details != null)
                sb.AppendLine($"Details: {runtimeInfo.Details}");
            sb.AppendLine($"Environments: {runtimeInfo.ContainerCount}");
        }
        else
        {
            sb.AppendLine($"Status: ❌ Not available ({runtimeInfo.Version})");
        }

        return new
        {
            content = new[]
            {
                new { type = "text", text = sb.ToString() }
            }
        };
    }

    private async Task<object> GenerateBlueprintAsync(JsonElement? args)
    {
        if (!args.HasValue)
            return CreateToolError("Missing arguments");

        var prompt = args.Value.TryGetProperty("prompt", out var promptProp) ? promptProp.GetString() : null;
        var model = args.Value.TryGetProperty("model", out var modelProp) ? modelProp.GetString() : "gpt-4o";

        if (string.IsNullOrEmpty(prompt))
            return CreateToolError("Missing required argument: prompt");

        // This would call the AI service - for now, return a placeholder
        return new
        {
            content = new[]
            {
                new { type = "text", text = $"🤖 AI blueprint generation coming soon!\n\nPrompt: {prompt}\nModel: {model}" }
            }
        };
    }

    // Helper Methods

    private object CreateToolError(string message)
    {
        return new
        {
            content = new[]
            {
                new { type = "text", text = $"❌ Error: {message}" }
            },
            isError = true
        };
    }

    private string CreateToolErrorResponse(string message)
    {
        var result = new ToolErrorResult
        {
            Content = new[] { new ContentItem { Type = "text", Text = $"❌ Error: {message}" } },
            IsError = true
        };
        var response = new JsonRpcResponse<ToolErrorResult> { Result = result };
        return JsonSerializer.Serialize(response, McpJsonContext.Default.JsonRpcResponseToolErrorResult);
    }

    private string CreateSuccessResponse(int? id, object result)
    {
        var genericResult = new GenericResult { Result = result };
        var response = new JsonRpcResponse<GenericResult> { Id = id, Result = genericResult };
        return JsonSerializer.Serialize(response, McpJsonContext.Default.JsonRpcResponseGenericResult);
    }

    private string CreateErrorResponse(int? id, int code, string message)
    {
        var response = new JsonRpcErrorResponse
        {
            Id = id,
            Error = new JsonRpcError { Code = code, Message = message }
        };
        return JsonSerializer.Serialize(response, McpJsonContext.Default.JsonRpcErrorResponse);
    }
}
