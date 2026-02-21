using System.Text;
using System.Text.Json;
using Thresh.Mcp.Models;
using Thresh.Models;
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
    private readonly GitHubCopilotService _copilotService;
    private readonly MetricsService _metricsService;
    private readonly CancellationTokenSource _cts;

    public StdioMcpServer()
    {
        _containerService = ContainerServiceFactory.Create();
        _configService = new ConfigurationService();
        var rootfsRegistry = new RootfsRegistry(_configService);
        _blueprintService = new BlueprintService(_containerService, rootfsRegistry);
        _copilotService = new GitHubCopilotService(_configService);
        _metricsService = new MetricsService(_containerService);
        _cts = new CancellationTokenSource();
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
                "initialize" => HandleInitialize(id, paramsElement),
                "notifications/initialized" => HandleInitialized(),
                "tools/list" => HandleListTools(id),
                "tools/call" => await HandleToolCallAsync(id, paramsElement),
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
    private string HandleInitialize(int? id, JsonElement? paramsElement)
    {
        var response = new InitializeResult
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new CapabilitiesResult
            {
                Tools = new ToolsCapability()
            },
            ServerInfo = new ServerInfoResult
            {
                Name = "thresh",
                Version = "1.0.0"
            },
            Instructions = "thresh is a cross-platform development environment manager. " +
                          "Use it to create, manage, and destroy containerized dev environments."
        };

        var jsonResponse = new JsonRpcResponse<InitializeResult> { Id = id, Result = response };
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
    private string HandleListTools(int? id)
    {
        var tools = new Tool[]
        {
            new Tool
            {
                Name = "list_environments",
                Description = $"List all {_containerService.RuntimeName} development environments managed by thresh",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>
                    {
                        ["include_all"] = new JsonSchemaProperty
                        {
                            Type = "boolean",
                            Description = "Include all containers, not just thresh-managed ones"
                        }
                    }
                }
            },
            new Tool
            {
                Name = "create_environment",
                Description = "Create development environment(s) from a blueprint. IMPORTANT: To create multiple environments, use the 'names' array parameter (e.g., [\"env1\", \"env2\", \"env3\"]) which creates all environments in parallel simultaneously. Use 'name' only for a single environment.",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>
                    {
                        ["blueprint"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Blueprint name (e.g., 'alpine-minimal', 'ubuntu-dev', 'debian-stable') or requirements description (e.g., 'Alpine with Node.js')"
                        },
                        ["name"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Single environment name. For multiple environments, use 'names' array instead for parallel creation."
                        },
                        ["names"] = new JsonSchemaProperty
                        {
                            Type = "array",
                            Description = "Array of environment names for PARALLEL creation (recommended for multiple environments - all created simultaneously, much faster than calling tool multiple times). Example: [\"test-1\", \"test-2\", \"test-3\"]",
                            Items = new JsonSchemaProperty { Type = "string" }
                        },
                        ["verbose"] = new JsonSchemaProperty
                        {
                            Type = "boolean",
                            Description = "Show detailed provisioning output"
                        }
                    },
                    Required = new List<string> { "blueprint" }
                }
            },
            new Tool
            {
                Name = "destroy_environment",
                Description = "Destroy/remove a development environment by name, or destroy all environments with all=true. Either 'name' or 'all' must be provided.",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>
                    {
                        ["name"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Name of the environment to destroy (optional if all=true)"
                        },
                        ["all"] = new JsonSchemaProperty
                        {
                            Type = "boolean",
                            Description = "Set to true to destroy all environments (optional if name is provided)"
                        }
                    }
                }
            },
            new Tool
            {
                Name = "list_blueprints",
                Description = "List all available blueprints for creating environments",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>()
                }
            },
            new Tool
            {
                Name = "get_blueprint",
                Description = "Get detailed information about a specific blueprint",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>
                    {
                        ["name"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Blueprint name"
                        }
                    },
                    Required = new List<string> { "name" }
                }
            },
            new Tool
            {
                Name = "get_version",
                Description = "Get thresh version and runtime information",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>()
                }
            },
            new Tool
            {
                Name = "generate_blueprint",
                Description = "Generate a custom blueprint using AI from a natural language description",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>
                    {
                        ["prompt"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Natural language description of the desired environment"
                        },
                        ["model"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "AI model to use (default: gpt-4o)"
                        }
                    },
                    Required = new List<string> { "prompt" }
                }
            },
            new Tool
            {
                Name = "save_blueprint",
                Description = "Save a blueprint JSON to thresh's blueprints directory for reuse",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>
                    {
                        ["name"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Name for the blueprint file (without extension)"
                        },
                        ["blueprint_json"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Complete blueprint JSON content"
                        }
                    },
                    Required = new List<string> { "name", "blueprint_json" }
                }
            },
            new Tool
            {
                Name = "get_metrics",
                Description = "Get host system metrics including CPU, memory, storage, and container count",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>
                    {
                        ["format"] = new JsonSchemaProperty
                        {
                            Type = "string",
                            Description = "Output format: 'text' (default) or 'json'"
                        }
                    }
                }
            },
            new Tool
            {
                Name = "help",
                Description = "Display a menu of all available thresh commands with descriptions",
                InputSchema = new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchemaProperty>()
                }
            }
        };

        var listResult = new ToolsListResult { Tools = tools };
        var response = new JsonRpcResponse<ToolsListResult> { Id = id, Result = listResult };
        return JsonSerializer.Serialize(response, McpJsonContext.Default.JsonRpcResponseToolsListResult);
    }

    /// <summary>
    /// Handle tools/call request
    /// </summary>
    private async Task<string> HandleToolCallAsync(int? id, JsonElement? paramsElement)
    {
        if (!paramsElement.HasValue)
        {
            return CreateErrorResponse(id, -32602, "Invalid params: missing");
        }

        var toolName = paramsElement.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
        var arguments = paramsElement.Value.TryGetProperty("arguments", out var argsProp) ? argsProp : (JsonElement?)null;

        if (string.IsNullOrEmpty(toolName))
        {
            return CreateErrorResponse(id, -32602, "Invalid params: missing tool name");
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
                "save_blueprint" => await SaveBlueprintAsync(arguments),
                "get_metrics" => await GetMetricsAsync(arguments),
                "help" => GetHelp(),
                _ => CreateToolError($"Unknown tool: {toolName}")
            };

            // Result is already a ToolCallResponse, just wrap in JSON-RPC response
            var response = new JsonRpcResponse<ToolCallResponse> { Id = id, Result = (ToolCallResponse)result };
            return JsonSerializer.Serialize(response, McpJsonContext.Default.JsonRpcResponseToolCallResponse);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"❌ Tool error: {ex.Message}");
            return CreateToolErrorResponse(id, ex.Message);
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

        return ToolCallResponse.Success(sb.ToString());
    }

    private async Task<object> CreateEnvironmentAsync(JsonElement? args)
    {
        if (!args.HasValue)
            return CreateToolError("Missing arguments");

        var blueprint = args.Value.TryGetProperty("blueprint", out var bpProp) ? bpProp.GetString() : null;
        var name = args.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
        var verbose = args.Value.TryGetProperty("verbose", out var verboseProp) && verboseProp.GetBoolean();

        if (string.IsNullOrEmpty(blueprint))
            return CreateToolError("Missing required argument: blueprint");

        // Check for names array (parallel creation)
        List<string>? namesList = null;
        if (args.Value.TryGetProperty("names", out var namesProp) && namesProp.ValueKind == JsonValueKind.Array)
        {
            namesList = new List<string>();
            foreach (var nameElement in namesProp.EnumerateArray())
            {
                var n = nameElement.GetString();
                if (!string.IsNullOrEmpty(n))
                    namesList.Add(n);
            }
        }

        // Either name or names must be provided
        if (string.IsNullOrEmpty(name) && (namesList == null || namesList.Count == 0))
            return CreateToolError("Either 'name' or 'names' must be provided");

        // PARALLEL CREATION: Multiple environments
        if (namesList != null && namesList.Count > 0)
        {
            await Console.Error.WriteLineAsync($"🚀 Creating {namesList.Count} environment(s) in parallel...");
            
            var sb = new StringBuilder();
            sb.AppendLine($"🎯 Creating {namesList.Count} environment(s) in parallel from blueprint '{blueprint}'...");
            sb.AppendLine();

            // Create all environments in parallel
            var createTasks = namesList.Select(async envName =>
            {
                try
                {
                    // Check if environment exists
                    if (await _containerService.EnvironmentExistsAsync(envName))
                        return new { Name = envName, Success = false, Error = (string?)"already exists" };

                    // Determine blueprint
                    Blueprint bp;
                    var bundledBlueprints = _blueprintService.ListBundledBlueprints();
                    if (bundledBlueprints.Contains(blueprint, StringComparer.OrdinalIgnoreCase))
                    {
                        bp = _blueprintService.LoadBundledBlueprint(blueprint);
                    }
                    else
                    {
                        bp = GenerateCustomBlueprint(blueprint, envName);
                    }

                    await _blueprintService.ProvisionEnvironmentAsync(envName, bp, verbose: false);
                    return new { Name = envName, Success = true, Error = (string?)null };
                }
                catch (Exception ex)
                {
                    return new { Name = envName, Success = false, Error = (string?)ex.Message };
                }
            }).ToList();

            var results = await Task.WhenAll(createTasks);

            // Display results sorted by name
            var successCount = 0;
            var failureCount = 0;

            foreach (var result in results.OrderBy(r => r.Name))
            {
                if (result.Success)
                {
                    sb.AppendLine($"  ✅ Created: {result.Name}");
                    successCount++;
                }
                else
                {
                    sb.AppendLine($"  ❌ Failed: {result.Name} ({result.Error})");
                    failureCount++;
                }
            }

            sb.AppendLine();
            sb.AppendLine($"📊 Summary: {successCount} succeeded, {failureCount} failed");
            
            if (successCount > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Access environments:");
                foreach (var result in results.Where(r => r.Success).OrderBy(r => r.Name))
                {
                    sb.AppendLine($"  docker exec -it thresh-{result.Name} bash");
                }
            }

            return ToolCallResponse.Success(sb.ToString());
        }

        // SINGLE CREATION: Original behavior
        if (string.IsNullOrEmpty(name))
            return CreateToolError("Missing required argument: name");

        // Check if environment exists
        if (await _containerService.EnvironmentExistsAsync(name))
            return CreateToolError($"Environment '{name}' already exists");

        // Determine if blueprint is a bundled name or a requirements description
        Blueprint bp;
        var blueprintDescription = blueprint;
        
        // Check if it's a bundled blueprint
        var bundledBlueprints = _blueprintService.ListBundledBlueprints();
        if (bundledBlueprints.Contains(blueprint, StringComparer.OrdinalIgnoreCase))
        {
            // Use existing bundled blueprint
            bp = _blueprintService.LoadBundledBlueprint(blueprint);
        }
        else
        {
            // Generate custom blueprint from requirements description
            bp = GenerateCustomBlueprint(blueprint, name);
            blueprintDescription = "custom";
        }
        
        try
        {
            await _blueprintService.ProvisionEnvironmentAsync(name, bp, verbose);
            
            return ToolCallResponse.Success(
                $"✅ Environment '{name}' created successfully!\n\n" +
                $"Blueprint: {blueprintDescription}\n" +
                $"Base: {bp.Base}\n" +
                $"Packages: {bp.Packages?.Count ?? 0}\n\n" +
                $"Access: docker exec -it thresh-{name} bash"
            );
        }
        catch (Exception ex)
        {
            return CreateToolError($"Failed to create environment: {ex.Message}");
        }
    }

    private Blueprint GenerateCustomBlueprint(string requirements, string envName)
    {
        var reqLower = requirements.ToLowerInvariant();
        
        // Detect base distribution
        var baseDistro = "ubuntu-24.04";
        var packages = new List<string> { "curl", "wget", "git" };
        var setupScript = "#!/bin/bash\necho \"Environment ready\"";
        
        if (reqLower.Contains("alpine"))
        {
            baseDistro = "alpine-3.19";
            packages = new List<string> { "curl", "wget", "git", "bash" };
        }
        else if (reqLower.Contains("debian"))
        {
            baseDistro = "debian-12";
        }
        
        // Detect Node.js
        if (reqLower.Contains("node") || reqLower.Contains("nodejs"))
        {
            if (baseDistro.Contains("alpine"))
            {
                packages.AddRange(new[] { "nodejs", "npm" });
                setupScript = "#!/bin/bash\nnode --version\nnpm --version\necho \"Node.js installed\"";
            }
            else
            {
                packages.Add("ca-certificates");
                setupScript = "#!/bin/bash\ncurl -fsSL https://deb.nodesource.com/setup_20.x | bash -\napt-get install -y nodejs\nnode --version\nnpm --version";
            }
        }
        
        // Detect Python
        if (reqLower.Contains("python"))
        {
            packages.AddRange(baseDistro.Contains("alpine") 
                ? new[] { "python3", "py3-pip" } 
                : new[] { "python3", "python3-pip" });
        }
        
        // Detect Go
        if (reqLower.Contains("go") || reqLower.Contains("golang"))
        {
            packages.Add(baseDistro.Contains("alpine") ? "go" : "golang");
        }
        
        return new Blueprint
        {
            Name = envName,
            Description = requirements,
            Base = baseDistro,
            Packages = packages.Distinct().ToList(),
            Environment = new Dictionary<string, string>
            {
                ["PATH"] = "/usr/local/bin:/usr/bin:/bin"
            },
            Scripts = new BlueprintScripts
            {
                Setup = setupScript,
                PostInstall = "#!/bin/bash\necho \"Installation complete!\""
            }
        };
    }

    private async Task<object> DestroyEnvironmentAsync(JsonElement? args)
    {
        var destroyAll = args?.TryGetProperty("all", out var allProp) == true && allProp.GetBoolean();
        
        if (destroyAll)
        {
            // Destroy all environments
            var environments = await _containerService.ListEnvironmentsAsync(includeAll: false);
            
            if (environments.Count == 0)
            {
                return ToolCallResponse.Success("ℹ️  No environments to destroy");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"🗑️  Destroying {environments.Count} environment(s) in parallel...");
            sb.AppendLine();

            // Destroy all environments in parallel
            var destroyTasks = environments.Select(async env =>
            {
                var success = await _containerService.RemoveEnvironmentAsync(env.Name);
                return new { env.Name, Success = success };
            }).ToList();

            var results = await Task.WhenAll(destroyTasks);

            var successCount = 0;
            var failureCount = 0;

            foreach (var result in results.OrderBy(r => r.Name))
            {
                if (result.Success)
                {
                    sb.AppendLine($"  ✅ Destroyed: {result.Name}");
                    successCount++;
                }
                else
                {
                    sb.AppendLine($"  ❌ Failed: {result.Name}");
                    failureCount++;
                }
            }

            sb.AppendLine();
            sb.AppendLine($"📊 Summary: {successCount} succeeded, {failureCount} failed");
            
            return ToolCallResponse.Success(sb.ToString());
        }
        else
        {
            // Destroy single environment by name
            if (!args.HasValue)
                return CreateToolError("Missing arguments");

            var name = args.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

            if (string.IsNullOrEmpty(name))
                return CreateToolError("Missing required argument: name (or use all=true to destroy all environments)");

            if (!await _containerService.EnvironmentExistsAsync(name))
                return CreateToolError($"Environment '{name}' not found");

            var success = await _containerService.RemoveEnvironmentAsync(name);
            
            var message = success 
                ? $"✅ Environment '{name}' destroyed successfully"
                : $"❌ Failed to destroy environment '{name}'";

            return ToolCallResponse.Success(message);
        }
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

        return ToolCallResponse.Success(sb.ToString());
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

        return ToolCallResponse.Success(sb.ToString());
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

        return ToolCallResponse.Success(sb.ToString());
    }

    private async Task<object> GenerateBlueprintAsync(JsonElement? args)
    {
        if (!args.HasValue)
            return CreateToolError("Missing arguments");

        var prompt = args.Value.TryGetProperty("prompt", out var promptProp) ? promptProp.GetString() : null;
        var model = args.Value.TryGetProperty("model", out var modelProp) ? modelProp.GetString() : "gpt-4o";

        if (string.IsNullOrEmpty(prompt))
            return CreateToolError("Missing required argument: prompt");

        // Generate a template blueprint based on the prompt
        var blueprintTemplate = GenerateBlueprintTemplate(prompt);
        
        return ToolCallResponse.Success(
            $"✅ Generated Blueprint: {prompt}\n\n" +
            $"```json\n{blueprintTemplate}\n```\n\n" +
            $"**This blueprint is ready to use!**\n\n" +
            $"To create an environment from this blueprint:\n" +
            $"1. Save the JSON above to a file (e.g., `my-blueprint.json`)\n" +
            $"2. Run: `thresh up my-blueprint.json --name <environment-name>`\n\n" +
            $"Or ask me to create an environment using this configuration!"
        );
    }

    private string GenerateBlueprintTemplate(string prompt)
    {
        // Parse prompt to determine base distro and packages
        var promptLower = prompt.ToLowerInvariant();
        
        var baseName = "custom-dev";
        var baseDistro = "ubuntu-24.04";
        var description = prompt;
        var packages = new List<string> { "curl", "wget", "git" };
        var setupScript = "#!/bin/bash\\necho \\\"Setting up environment...\\\"";
        var postInstallScript = "#!/bin/bash\\necho \\\"Installation complete!\\\"";
        
        // Detect distribution
        var isAlpine = promptLower.Contains("alpine");
        var isDebian = promptLower.Contains("debian");
        var isUbuntu = !isAlpine && !isDebian; // Default to Ubuntu
        
        if (isAlpine)
        {
            baseName = "alpine-dev";
            baseDistro = "alpine-3.19";
            packages = new List<string> { "curl", "wget", "git", "bash" };
        }
        else if (isDebian)
        {
            baseName = "debian-dev";
            baseDistro = "debian-12";
        }
        
        // Detect Node.js with version handling
        if (promptLower.Contains("node") || promptLower.Contains("nodejs"))
        {
            if (isAlpine)
            {
                // Alpine: Use official nodejs package + handle versions if needed
                packages.AddRange(new[] { "nodejs", "npm" });
                setupScript = @"#!/bin/bash
# Alpine Node.js setup
node --version
npm --version
echo \""Node.js installed successfully\""";
            }
            else
            {
                // Ubuntu/Debian: Use NodeSource for specific versions
                packages.Add("ca-certificates");
                setupScript = @"#!/bin/bash
# Install Node.js 20.x from NodeSource
curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
apt-get install -y nodejs
node --version
npm --version";
            }
        }
        
        // Python detection
        if (promptLower.Contains("python"))
        {
            if (isAlpine)
            {
                packages.AddRange(new[] { "python3", "py3-pip" });
            }
            else
            {
                packages.AddRange(new[] { "python3", "python3-pip" });
            }
            
            if (setupScript.Contains("Setting up environment"))
            {
                setupScript = "#!/bin/bash\\npython3 --version\\npip3 --version";
            }
        }
        
        // Go detection
        if (promptLower.Contains("go") || promptLower.Contains("golang"))
        {
            if (isAlpine)
            {
                packages.Add("go");
            }
            else
            {
                packages.Add("golang");
            }
        }
        
        // Rust detection
        if (promptLower.Contains("rust"))
        {
            if (isAlpine)
            {
                packages.AddRange(new[] { "rust", "cargo" });
            }
            else
            {
                setupScript = @"#!/bin/bash
# Install Rust via rustup
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
source $HOME/.cargo/env
rustc --version";
            }
        }
        
        // Java detection
        if (promptLower.Contains("java"))
        {
            if (isAlpine)
            {
                packages.Add("openjdk17");
            }
            else
            {
                packages.AddRange(new[] { "openjdk-17-jdk", "maven" });
            }
        }
        
        // Manual JSON construction to avoid AOT issues
        var json = new StringBuilder();
        json.AppendLine("{");
        json.AppendLine($"  \"name\": \"{baseName}\",");
        json.AppendLine($"  \"description\": \"{description}\",");
        json.AppendLine($"  \"base\": \"{baseDistro}\",");
        json.AppendLine($"  \"packages\": [");
        var distinctPackages = packages.Distinct().ToList();
        for (int i = 0; i < distinctPackages.Count; i++)
        {
            var comma = i < distinctPackages.Count - 1 ? "," : "";
            json.AppendLine($"    \"{distinctPackages[i]}\"{comma}");
        }
        json.AppendLine("  ],");
        json.AppendLine("  \"environment\": {");
        json.AppendLine("    \"PATH\": \"/usr/local/bin:/usr/bin:/bin\"");
        json.AppendLine("  },");
        json.AppendLine("  \"scripts\": {");
        json.AppendLine($"    \"setup\": \"{setupScript}\",");
        json.AppendLine($"    \"postInstall\": \"{postInstallScript}\"");
        json.AppendLine("  }");
        json.AppendLine("}");
        
        return json.ToString();
    }

    private async Task<object> SaveBlueprintAsync(JsonElement? args)
    {
        if (!args.HasValue)
            return CreateToolError("Missing arguments");

        var name = args.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
        var blueprintJson = args.Value.TryGetProperty("blueprint_json", out var jsonProp) ? jsonProp.GetString() : null;

        if (string.IsNullOrEmpty(name))
            return CreateToolError("Missing required argument: name");
        
        if (string.IsNullOrEmpty(blueprintJson))
            return CreateToolError("Missing required argument: blueprint_json");

        try
        {
            // Validate JSON - use JsonDocument for AOT compatibility
            using var testParse = JsonDocument.Parse(blueprintJson);
            
            // Get blueprints directory
            var blueprintsDir = Path.Combine(AppContext.BaseDirectory, "blueprints");
            if (!Directory.Exists(blueprintsDir))
            {
                Directory.CreateDirectory(blueprintsDir);
            }
            
            // Sanitize filename
            var safeName = name.Replace(" ", "-").ToLowerInvariant();
            safeName = new string(safeName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
            
            var filePath = Path.Combine(blueprintsDir, $"{safeName}.json");
            
            // Check if already exists
            if (File.Exists(filePath))
            {
                return CreateToolError($"Blueprint '{safeName}' already exists. Choose a different name.");
            }
            
            // Write file - pretty print the JSON
            var prettyJson = PrettyPrintJson(blueprintJson);
            await File.WriteAllTextAsync(filePath, prettyJson);
            
            return ToolCallResponse.Success(
                $"✅ Blueprint saved successfully!\n\n" +
                $"Name: {safeName}\n" +
                $"Location: {filePath}\n\n" +
                $"You can now use this blueprint:\n" +
                $"• From MCP: create_environment with blueprint=\"{safeName}\"\n" +
                $"• From CLI: thresh up {safeName} --name <env-name>\n" +
                $"• View: thresh blueprint list"
            );
        }
        catch (JsonException)
        {
            return CreateToolError("Invalid JSON format. Please provide valid blueprint JSON.");
        }
        catch (Exception ex)
        {
            return CreateToolError($"Failed to save blueprint: {ex.Message}");
        }
    }

    private string PrettyPrintJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        doc.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Get help menu showing all available commands
    /// </summary>
    private object GetHelp()
    {
        var sb = new StringBuilder();
        sb.AppendLine("📖 thresh MCP Commands");
        sb.AppendLine("═══════════════════════");
        sb.AppendLine();
        sb.AppendLine("🌍 Environment Management:");
        sb.AppendLine("  • list_environments     - List all development environments");
        sb.AppendLine("  • create_environment    - Create a new environment from a blueprint");
        sb.AppendLine("  • destroy_environment   - Remove an environment (or all with all=true)");
        sb.AppendLine();
        sb.AppendLine("📋 Blueprint Operations:");
        sb.AppendLine("  • list_blueprints       - Show all available blueprints");
        sb.AppendLine("  • get_blueprint         - Get detailed blueprint information");
        sb.AppendLine("  • generate_blueprint    - Generate custom blueprint using AI");
        sb.AppendLine("  • save_blueprint        - Save a blueprint for reuse");
        sb.AppendLine();
        sb.AppendLine("📊 System Information:");
        sb.AppendLine("  • get_metrics           - Display host system metrics");
        sb.AppendLine("  • get_version           - Show thresh version info");
        sb.AppendLine();
        sb.AppendLine("❓ Help:");
        sb.AppendLine("  • help                  - Display this menu");
        sb.AppendLine();
        sb.AppendLine("💡 Tips:");
        sb.AppendLine($"  • Platform: {_containerService.Platform}");
        sb.AppendLine($"  • Runtime: {_containerService.RuntimeName}");
        sb.AppendLine("  • Use natural language to describe what you want!");
        sb.AppendLine("  • Example: 'Create an Alpine environment with Node.js'");
        
        return ToolCallResponse.Success(sb.ToString());
    }

    /// <summary>
    /// Get host system metrics
    /// </summary>
    private async Task<object> GetMetricsAsync(JsonElement? args)
    {
        var format = args?.TryGetProperty("format", out var formatProp) == true 
            ? formatProp.GetString()?.ToLowerInvariant() 
            : "text";

        try
        {
            var metrics = await _metricsService.CollectMetricsAsync();

            if (format == "json")
            {
                // Return metrics as JSON string
                var jsonText = JsonSerializer.Serialize(metrics, MetricsJsonContext.Default.HostMetrics);
                return ToolCallResponse.Success(jsonText);
            }
            else
            {
                // Return metrics as formatted text (matching CLI output)
                var sb = new StringBuilder();
                sb.AppendLine("📊 Host Metrics");
                sb.AppendLine("═══════════════");
                sb.AppendLine();
                sb.AppendLine($"🖥️  Hostname: {metrics.Hostname}");
                sb.AppendLine($"🔧 Platform: {metrics.Platform}");
                sb.AppendLine($"📦 Runtime: {metrics.Runtime} {metrics.RuntimeVersion}");
                sb.AppendLine();
                sb.AppendLine($"⚙️  CPU:");
                sb.AppendLine($"   Cores: {metrics.CpuCores}");
                sb.AppendLine($"   Usage: {metrics.CpuPercent:F1}%");
                sb.AppendLine();
                sb.AppendLine($"💾 Memory:");
                sb.AppendLine($"   Total: {metrics.MemoryTotalGb:F2} GB");
                sb.AppendLine($"   Used:  {metrics.MemoryUsedGb:F2} GB");
                sb.AppendLine($"   Usage: {metrics.MemoryPercent:F1}%");
                sb.AppendLine();
                sb.AppendLine($"💿 Storage:");
                sb.AppendLine($"   Total: {metrics.StorageTotalGb:F2} GB");
                sb.AppendLine($"   Free:  {metrics.StorageFreeGb:F2} GB");
                sb.AppendLine($"   Usage: {metrics.StoragePercent:F1}%");
                sb.AppendLine();
                
                if (!string.IsNullOrEmpty(metrics.IpAddress))
                {
                    sb.AppendLine($"🌐 Network:");
                    sb.AppendLine($"   IP Address: {metrics.IpAddress}");
                    
                    if (metrics.IpAddresses != null && metrics.IpAddresses.Count > 1)
                    {
                        sb.AppendLine($"   All IPs: {string.Join(", ", metrics.IpAddresses)}");
                    }
                    
                    if (!string.IsNullOrEmpty(metrics.ExternalIp))
                    {
                        sb.AppendLine($"   External IP: {metrics.ExternalIp}");
                    }
                    sb.AppendLine();
                }
                
                if (metrics.LoadAverage != null && metrics.LoadAverage.Count == 3)
                {
                    sb.AppendLine($"📈 Load Average:");
                    sb.AppendLine($"   1 min:  {metrics.LoadAverage[0]:F2}");
                    sb.AppendLine($"   5 min:  {metrics.LoadAverage[1]:F2}");
                    sb.AppendLine($"   15 min: {metrics.LoadAverage[2]:F2}");
                    sb.AppendLine();
                }
                
                sb.AppendLine($"📦 Containers: {metrics.Containers}");
                
                if (!string.IsNullOrEmpty(metrics.DockerStorageDriver))
                {
                    sb.AppendLine($"🐳 Docker:");
                    sb.AppendLine($"   Storage Driver: {metrics.DockerStorageDriver}");
                    if (!string.IsNullOrEmpty(metrics.DockerRootDir))
                    {
                        sb.AppendLine($"   Root Directory: {metrics.DockerRootDir}");
                    }
                }
                
                if (metrics.UptimeSeconds.HasValue)
                {
                    var uptime = TimeSpan.FromSeconds(metrics.UptimeSeconds.Value);
                    sb.AppendLine($"⏱️  Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
                }
                
                sb.AppendLine();
                sb.AppendLine($"🕐 Collected: {metrics.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
                
                return ToolCallResponse.Success(sb.ToString());
            }
        }
        catch (Exception ex)
        {
            return CreateToolError($"Failed to collect metrics: {ex.Message}");
        }
    }

    // Helper Methods

    private object CreateToolError(string message)
    {
        return ToolCallResponse.Error(message);
    }

    private string CreateToolErrorResponse(int? id, string message)
    {
        var result = new ToolErrorResult
        {
            Content = new[] { new ContentItem { Type = "text", Text = $"❌ Error: {message}" } },
            IsError = true
        };
        var response = new JsonRpcResponse<ToolErrorResult> { Id = id, Result = result };
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
