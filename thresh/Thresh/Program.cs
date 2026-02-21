using System.CommandLine;
using Thresh.Models;
using Thresh.Services;

namespace Thresh;

class Program
{
    private const string Version = "1.4.0";
    
    static async Task<int> Main(string[] args)
    {
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var description = isWindows 
            ? "thresh - AI-Powered WSL Development Environments" 
            : "thresh - AI-Powered Container Development Environments";
        
        var rootCommand = new RootCommand(description);
        
        // Verbose option
        var verboseOption = new Option<bool>(
            aliases: ["--verbose"],
            description: "Enable verbose logging");
        rootCommand.AddGlobalOption(verboseOption);
        
        // Add commands
        AddUpCommand(rootCommand);
        AddListCommand(rootCommand);
        AddStartCommand(rootCommand);
        AddStopCommand(rootCommand);
        AddDestroyCommand(rootCommand);
        AddBlueprintCommand(rootCommand);
        AddChatCommand(rootCommand);
        AddConfigCommand(rootCommand);
        AddDistroCommand(rootCommand);
        AddMetricsCommand(rootCommand);
        AddServeCommand(rootCommand);
        AddVersionCommand(rootCommand);
        AddTestSdkCommand(rootCommand);
        
        // Root handler (when no command specified)
        rootCommand.SetHandler((bool verbose) =>
        {
            DisplayHelp();
        }, verboseOption);
        
        return await rootCommand.InvokeAsync(args);
    }
    
    private static void DisplayHelp()
    {
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var envType = isWindows ? "WSL environment" : "container environment";
        var envTypePlural = isWindows ? "WSL environments" : "container environments";
        var title = isWindows ? "thresh - AI-Powered WSL Development Environments" : "thresh - AI-Powered Container Development Environments";
        
        Console.WriteLine(title);
        Console.WriteLine();
        Console.WriteLine("Usage: thresh [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine($"  up          Provision a {envType} from a blueprint");
        Console.WriteLine($"  list        List {envTypePlural}");
        Console.WriteLine($"  destroy     Remove a {envType}");
        Console.WriteLine("  blueprint   Manage blueprints (list, generate, delete)");
        Console.WriteLine("  chat        Interactive AI chat mode for blueprint help");
        Console.WriteLine("  config      Manage configuration");
        Console.WriteLine("  distro      Manage custom distributions");
        Console.WriteLine("  distros     List all available distributions");
        Console.WriteLine("  metrics     Display host system metrics");
        Console.WriteLine("  serve       Start MCP server");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --verbose        Enable verbose logging");
        Console.WriteLine("  --help           Display help information");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  thresh version");
        Console.WriteLine("  thresh up alpine-minimal");
        Console.WriteLine("  thresh blueprint list");
        Console.WriteLine("  thresh blueprint generate 'Python ML with Jupyter'");
        Console.WriteLine("  thresh blueprint delete alpine-test");
        Console.WriteLine("  thresh list");
        Console.WriteLine("  thresh config set default-model gpt-4o");
        Console.WriteLine("  thresh config set default-base ubuntu-24.04");
    }
    
    private static void AddVersionCommand(RootCommand rootCommand)
    {
        var versionCommand = new Command("version", "Display version information");
        
        versionCommand.SetHandler(async () =>
        {
            Console.WriteLine($"thresh version {Version}");
            Console.WriteLine("GitHub Copilot SDK integrated");
            Console.WriteLine($".NET Runtime: {System.Environment.Version}");
            Console.WriteLine("Native AOT: Yes");
            Console.WriteLine();
            
            // Show runtime info
            var containerService = Services.ContainerServiceFactory.Create();
            var runtimeInfo = await containerService.GetRuntimeInfoAsync();
            
            if (runtimeInfo.IsAvailable)
            {
                Console.WriteLine($"{containerService.RuntimeName}: {runtimeInfo.Version}");
                if (runtimeInfo.Details != null)
                    Console.WriteLine($"Details: {runtimeInfo.Details}");
                Console.WriteLine($"Environments: {runtimeInfo.ContainerCount}");
            }
            else
            {
                Console.WriteLine($"{containerService.RuntimeName}: Not available ({runtimeInfo.Version})");
            }
        });
        
        rootCommand.AddCommand(versionCommand);
    }
    
    private static void AddUpCommand(RootCommand rootCommand)
    {
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var envType = isWindows ? "WSL environment" : "container environment";
        
        var upCommand = new Command("up", $"Provision a {envType} from a blueprint");
        var blueprintArg = new Argument<string>("blueprint", "Blueprint name or path to JSON file");
        var nameOption = new Option<string?>("--name", "Custom name for the environment");
        var verboseOption = new Option<bool>("--verbose", "Show detailed output");
        
        upCommand.AddArgument(blueprintArg);
        upCommand.AddOption(nameOption);
        upCommand.AddOption(verboseOption);
        
        upCommand.SetHandler(async (string blueprint, string? name, bool verbose) =>
        {
            var containerService = Services.ContainerServiceFactory.Create();
            var configService = new Services.ConfigurationService();
            var rootfsRegistry = new Services.RootfsRegistry(configService);
            var blueprintService = new Services.BlueprintService(containerService, rootfsRegistry);
            
            // Check if runtime is available
            if (!await containerService.IsAvailableAsync())
            {
                Console.WriteLine($"❌ {containerService.RuntimeName} is not available on this system");
                Console.WriteLine($"   Platform: {Services.ContainerServiceFactory.GetPlatformName()}");
                return;
            }
            
            try
            {
                // Load blueprint
                Blueprint bp;
                if (File.Exists(blueprint))
                {
                    Console.WriteLine($"Loading blueprint from file: {blueprint}");
                    bp = blueprintService.LoadBlueprint(blueprint);
                }
                else
                {
                    Console.WriteLine($"Loading bundled blueprint: {blueprint}");
                    bp = blueprintService.LoadBundledBlueprint(blueprint);
                }
                
                // Determine environment name
                var envName = name ?? blueprint.Replace(".json", "").Replace("blueprints/", "");
                
                // Check if environment already exists
                if (await containerService.EnvironmentExistsAsync(envName))
                {
                    Console.WriteLine($"❌ Environment '{envName}' already exists");
                    Console.WriteLine($"   Remove it first: thresh destroy {envName}");
                    return;
                }
                
                Console.WriteLine();
                Console.WriteLine($"Blueprint: {bp.Name}");
                Console.WriteLine($"Description: {bp.Description}");
                Console.WriteLine($"Base: {bp.Base}");
                Console.WriteLine();
                
                // Provision the environment
                await blueprintService.ProvisionEnvironmentAsync(envName, bp, verbose);
                
                Console.WriteLine();
                Console.WriteLine($"Access your environment:");
                
                // Show platform-appropriate access instructions
                if (containerService.Platform == "Windows")
                {
                    Console.WriteLine($"  wsl -d thresh-{envName}");
                }
                else if (containerService.RuntimeName == "docker")
                {
                    Console.WriteLine($"  docker exec -it thresh-{envName} bash");
                    Console.WriteLine($"  # Or use: docker exec -it thresh-{envName} sh");
                }
                else if (containerService.RuntimeName == "nerdctl")
                {
                    Console.WriteLine($"  nerdctl exec -it thresh-{envName} bash");
                    Console.WriteLine($"  # Or use: nerdctl exec -it thresh-{envName} sh");
                }
                else
                {
                    // Fallback for containerd (ctr) or unknown
                    Console.WriteLine($"  # Container: thresh-{envName}");
                    Console.WriteLine($"  # Use your container runtime to access");
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"❌ {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Available blueprints:");
                foreach (var b in blueprintService.ListBundledBlueprints())
                {
                    Console.WriteLine($"  - {b}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Provisioning failed: {ex.Message}");
                if (verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Stack trace:");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }, blueprintArg, nameOption, verboseOption);
        
        rootCommand.AddCommand(upCommand);
    }
    
    private static void AddListCommand(RootCommand rootCommand)
    {
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var envTypePlural = isWindows ? "WSL environments" : "container environments";
        var allOptionDesc = isWindows ? "Include all WSL distributions, not just thresh environments" : "Include all containers, not just thresh environments";
        
        var listCommand = new Command("list", $"List {envTypePlural}");
        var allOption = new Option<bool>("--all", allOptionDesc);
        listCommand.AddOption(allOption);
        
        listCommand.SetHandler(async (bool all) =>
        {
            var containerService = Services.ContainerServiceFactory.Create();
            
            // Check if runtime is available
            if (!await containerService.IsAvailableAsync())
            {
                Console.WriteLine($"❌ {containerService.RuntimeName} is not available on this system");
                Console.WriteLine($"   Platform: {Services.ContainerServiceFactory.GetPlatformName()}");
                return;
            }
            
            var environments = await containerService.ListEnvironmentsAsync(all);
            
            if (environments.Count == 0)
            {
                Console.WriteLine("No environments found.");
                if (!all)
                    Console.WriteLine("Use --all to see all environments.");
                return;
            }
            
            Console.WriteLine($"{"NAME",-20} {"STATUS",-12} {"VERSION",-10} {"BLUEPRINT",-15}");
            Console.WriteLine(new string('-', 65));
            
            foreach (var env in environments)
            {
                Console.WriteLine($"{env.Name,-20} {env.Status.GetDisplayName(),-12} {env.Version,-10} {env.Blueprint,-15}");
            }
        }, allOption);
        
        rootCommand.AddCommand(listCommand);
    }

    private static void AddStartCommand(RootCommand rootCommand)
    {
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var envType = isWindows ? "WSL environment" : "container environment";
        
        var startCommand = new Command("start", $"Start a {envType}");
        var nameArg = new Argument<string>("name", "Environment name to start");
        
        startCommand.AddArgument(nameArg);
        
        startCommand.SetHandler(async (string name) =>
        {
            var containerService = Services.ContainerServiceFactory.Create();
            
            // Check if runtime is available
            if (!await containerService.IsAvailableAsync())
            {
                Console.WriteLine($"❌ {containerService.RuntimeName} is not available on this system");
                return;
            }

            Console.WriteLine($"Starting environment '{name}'...");
            var success = await containerService.StartEnvironmentAsync(name);
            
            if (success)
            {
                Console.WriteLine($"✅ Environment '{name}' started successfully");
            }
            else
            {
                Console.WriteLine($"❌ Failed to start environment '{name}'");
            }
        }, nameArg);
        
        rootCommand.AddCommand(startCommand);
    }

    private static void AddStopCommand(RootCommand rootCommand)
    {
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var envType = isWindows ? "WSL environment" : "container environment";
        
        var stopCommand = new Command("stop", $"Stop a {envType}");
        var nameArg = new Argument<string>("name", "Environment name to stop");
        
        stopCommand.AddArgument(nameArg);
        
        stopCommand.SetHandler(async (string name) =>
        {
            var containerService = Services.ContainerServiceFactory.Create();
            
            // Check if runtime is available
            if (!await containerService.IsAvailableAsync())
            {
                Console.WriteLine($"❌ {containerService.RuntimeName} is not available on this system");
                return;
            }

            Console.WriteLine($"Stopping environment '{name}'...");
            var success = await containerService.StopEnvironmentAsync(name);
            
            if (success)
            {
                Console.WriteLine($"✅ Environment '{name}' stopped successfully");
            }
            else
            {
                Console.WriteLine($"❌ Failed to stop environment '{name}'");
            }
        }, nameArg);
        
        rootCommand.AddCommand(stopCommand);
    }

    private static void AddDestroyCommand(RootCommand rootCommand)
    {
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var envType = isWindows ? "WSL environment" : "container environment";
        
        var destroyCommand = new Command("destroy", $"Remove a {envType} or all environments");
        var nameArg = new Argument<string?>("name", () => null, "Environment name to remove (not required if --all is used)");
        var forceOption = new Option<bool>(new[] { "-y", "--force" }, "Skip confirmation prompt");
        var allOption = new Option<bool>(new[] { "--all" }, "Destroy all environments");
        
        destroyCommand.AddArgument(nameArg);
        destroyCommand.AddOption(forceOption);
        destroyCommand.AddOption(allOption);
        
        destroyCommand.SetHandler(async (string? name, bool force, bool destroyAll) =>
        {
            var containerService = Services.ContainerServiceFactory.Create();
            
            // Check if runtime is available
            if (!await containerService.IsAvailableAsync())
            {
                Console.WriteLine($"❌ {containerService.RuntimeName} is not available on this system");
                return;
            }

            if (destroyAll)
            {
                // Destroy all environments
                var environments = await containerService.ListEnvironmentsAsync(includeAll: false);
                
                if (environments.Count == 0)
                {
                    Console.WriteLine("ℹ️  No environments to destroy");
                    return;
                }

                // Confirm unless --force
                if (!force)
                {
                    Console.Write($"⚠️  Are you sure you want to destroy ALL {environments.Count} environment(s)? (y/N): ");
                    var response = Console.ReadLine()?.Trim().ToLowerInvariant();
                    if (response != "y" && response != "yes")
                    {
                        Console.WriteLine("Cancelled.");
                        return;
                    }
                }

                Console.WriteLine($"🗑️  Destroying {environments.Count} environment(s) in parallel...");
                Console.WriteLine();

                // Destroy all environments in parallel
                var destroyTasks = environments.Select(async env =>
                {
                    var success = await containerService.RemoveEnvironmentAsync(env.Name);
                    return new { env.Name, Success = success };
                }).ToList();

                var results = await Task.WhenAll(destroyTasks);

                var successCount = 0;
                var failureCount = 0;

                foreach (var result in results.OrderBy(r => r.Name))
                {
                    if (result.Success)
                    {
                        Console.WriteLine($"  ✅ Destroyed: {result.Name}");
                        successCount++;
                    }
                    else
                    {
                        Console.WriteLine($"  ❌ Failed: {result.Name}");
                        failureCount++;
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"📊 Summary: {successCount} succeeded, {failureCount} failed");
            }
            else
            {
                // Destroy single environment
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("❌ Error: Environment name is required (or use --all to destroy all environments)");
                    return;
                }

                // Check if environment exists
                if (!await containerService.EnvironmentExistsAsync(name))
                {
                    Console.WriteLine($"❌ Environment '{name}' not found");
                    return;
                }
                
                // Confirm unless --force
                if (!force)
                {
                    Console.Write($"Are you sure you want to destroy '{name}'? (y/N): ");
                    var response = Console.ReadLine()?.Trim().ToLowerInvariant();
                    if (response != "y" && response != "yes")
                    {
                        Console.WriteLine("Cancelled.");
                        return;
                    }
                }
                
                Console.WriteLine($"Removing environment: {name}");
                if (await containerService.RemoveEnvironmentAsync(name))
                {
                    Console.WriteLine($"✅ Environment '{name}' removed successfully");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to remove environment '{name}'");
                }
            }
        }, nameArg, forceOption, allOption);
        
        rootCommand.AddCommand(destroyCommand);
    }
    
    private static void AddBlueprintCommand(RootCommand rootCommand)
    {
        var blueprintCommand = new Command("blueprint", "Manage blueprints");
        
        // Shared list handler action
        Action listBlueprintsAction = () =>
        {
            var containerService = Services.ContainerServiceFactory.Create();
            var configService = new Services.ConfigurationService();
            var rootfsRegistry = new Services.RootfsRegistry(configService);
            var blueprintService = new Services.BlueprintService(containerService, rootfsRegistry);
            
            var blueprints = blueprintService.ListBundledBlueprints();
            
            if (blueprints.Count == 0)
            {
                Console.WriteLine("No blueprints found.");
                return;
            }
            
            Console.WriteLine("Available blueprints:");
            Console.WriteLine();
            
            foreach (var name in blueprints.OrderBy(b => b))
            {
                try
                {
                    var bp = blueprintService.LoadBundledBlueprint(name);
                    Console.WriteLine($"  {name,-20} - {bp.Description}");
                }
                catch
                {
                    Console.WriteLine($"  {name,-20} - (error loading)");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Usage: thresh up <blueprint-name>");
        };
        
        // Subcommand: blueprint list
        var listCommand = new Command("list", "List available blueprints");
        listCommand.SetHandler(listBlueprintsAction);
        blueprintCommand.AddCommand(listCommand);
        
        // Subcommand: blueprint delete
        var deleteCommand = new Command("delete", "Delete a blueprint");
        var deleteBlueprintArg = new Argument<string>("blueprint", "Name of blueprint to delete");
        deleteCommand.AddArgument(deleteBlueprintArg);
        deleteCommand.SetHandler((string blueprintName) =>
        {
            var blueprintsDir = Path.Combine(AppContext.BaseDirectory, "blueprints");
            
            if (!Directory.Exists(blueprintsDir))
            {
                Console.WriteLine("No blueprints folder found.");
                return;
            }
            
            // Try to find the blueprint file (JSON, YAML, or YML)
            var jsonPath = Path.Combine(blueprintsDir, $"{blueprintName}.json");
            var yamlPath = Path.Combine(blueprintsDir, $"{blueprintName}.yaml");
            var ymlPath = Path.Combine(blueprintsDir, $"{blueprintName}.yml");
            
            string? fileToDelete = null;
            if (File.Exists(jsonPath))
                fileToDelete = jsonPath;
            else if (File.Exists(yamlPath))
                fileToDelete = yamlPath;
            else if (File.Exists(ymlPath))
                fileToDelete = ymlPath;
            
            if (fileToDelete == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Blueprint not found: {blueprintName}");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Available blueprints:");
                
                var blueprintService = new Services.BlueprintService(
                    Services.ContainerServiceFactory.Create(), 
                    new Services.RootfsRegistry(new Services.ConfigurationService()));
                var blueprints = blueprintService.ListBundledBlueprints();
                
                foreach (var bp in blueprints.OrderBy(b => b))
                {
                    Console.WriteLine($"  {bp}");
                }
                return;
            }
            
            try
            {
                File.Delete(fileToDelete);
                var filename = Path.GetFileName(fileToDelete);
                Console.WriteLine($"🗑️  Blueprint deleted: {filename}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Failed to delete blueprint: {ex.Message}");
                Console.ResetColor();
            }
        }, deleteBlueprintArg);
        blueprintCommand.AddCommand(deleteCommand);
        
        // Subcommand: blueprint generate
        var generateCommand = new Command("generate", "Generate blueprint from natural language using AI");
        var generatePromptArg = new Argument<string>("prompt", "Description of desired environment");
        var generateOutputOption = new Option<string?>("--output", "Save blueprint to file");
        var generateModelOption = new Option<string?>("--model", "AI model to use (default: gpt-4o)");
        var generateProviderOption = new Option<string?>("--provider", "AI provider: openai, azure, or github (auto-detect if not specified)");
        var generateNoStreamOption = new Option<bool>("--no-stream", "Disable streaming output");
        
        generateCommand.AddArgument(generatePromptArg);
        generateCommand.AddOption(generateOutputOption);
        generateCommand.AddOption(generateModelOption);
        generateCommand.AddOption(generateProviderOption);
        generateCommand.AddOption(generateNoStreamOption);
        
        generateCommand.SetHandler(async (string prompt, string? output, string? model, string? provider, bool noStream) =>
        {
            await GenerateBlueprintHandler(prompt, output, model, provider, noStream);
        }, generatePromptArg, generateOutputOption, generateModelOption, generateProviderOption, generateNoStreamOption);
        
        blueprintCommand.AddCommand(generateCommand);
        
        // Add grouped command
        rootCommand.AddCommand(blueprintCommand);
    }
    
    private static async Task GenerateBlueprintHandler(string prompt, string? output, string? model, string? provider, bool noStream)
    {
        Console.WriteLine($"🎯 Generating blueprint for: '{prompt}'");
        Console.WriteLine();
        
        try
        {
            var configService = new Services.ConfigurationService();
            var factory = new Services.AiProviderFactory(configService);
            var aiService = factory.CreateAIService(model, provider);
            
            var jsonContent = await aiService.GenerateBlueprintAsync(prompt, streaming: !noStream);
            
            if (noStream)
            {
                Console.WriteLine(jsonContent);
                Console.WriteLine();
            }
            
            // Clean the output (remove markdown code blocks)
            var cleanedJson = aiService switch
            {
                GitHubCopilotService copilot => copilot.CleanJsonOutput(jsonContent),
                _ => CleanJsonOutput(jsonContent)
            };
            
            // Save to file if requested
            if (!string.IsNullOrEmpty(output))
            {
                // Ensure .json extension
                var filename = output.EndsWith(".json", StringComparison.OrdinalIgnoreCase) 
                    ? output 
                    : $"{output}.json";
                
                // Save to blueprints directory
                var baseDir = AppContext.BaseDirectory;
                var blueprintsDir = Path.Combine(baseDir, "blueprints");
                
                if (!Directory.Exists(blueprintsDir))
                {
                    Directory.CreateDirectory(blueprintsDir);
                }
                
                var fullPath = Path.Combine(blueprintsDir, filename);
                File.WriteAllText(fullPath, cleanedJson);
                
                Console.WriteLine($"💾 Blueprint saved: {filename}");
                Console.WriteLine($"   Available in: thresh blueprint list");
                
                // Extract blueprint name from filename for usage message
                var blueprintName = Path.GetFileNameWithoutExtension(filename);
                Console.WriteLine($"   To provision: thresh up {blueprintName} --name my-env");
            }
            else
            {
                Console.WriteLine("To save this blueprint:");
                Console.WriteLine($"  thresh blueprint generate '{prompt}' --output my-blueprint");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Generation failed: {ex.Message}");
            Console.ResetColor();
        }
    }
    
    private static void AddChatCommand(RootCommand rootCommand)
    {
        var chatCommand = new Command("chat", "Interactive AI chat mode for blueprint assistance");
        var modelOption = new Option<string?>("--model", "AI model to use (default: gpt-4o)");
        var providerOption = new Option<string?>("--provider", "AI provider: openai, azure, or github (auto-detect if not specified)");
        
        chatCommand.AddOption(modelOption);
        chatCommand.AddOption(providerOption);
        
        chatCommand.SetHandler(async (string? model, string? provider) =>
        {
            try
            {
                var configService = new Services.ConfigurationService();
                var factory = new Services.AiProviderFactory(configService);
                var aiService = factory.CreateAIService(model, provider);
                await aiService.ChatModeAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Chat mode failed: {ex.Message}");
                Console.ResetColor();
            }
        }, modelOption, providerOption);
        
        rootCommand.AddCommand(chatCommand);
    }
    
    private static void AddConfigCommand(RootCommand rootCommand)
    {
        var configCommand = new Command("config", "Manage configuration");
        var configService = new Services.ConfigurationService();
        
        // config set
        var setCommand = new Command("set", "Set configuration value");
        var keyArg = new Argument<string>("key", "Configuration key (e.g., default-model, default-base, enable-telemetry)");
        var valueArg = new Argument<string>("value", "Configuration value");
        setCommand.AddArgument(keyArg);
        setCommand.AddArgument(valueArg);
        setCommand.SetHandler((string key, string value) =>
        {
            try
            {
                configService.SetValue(key, value);
                Console.WriteLine($"✅ Set {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to set {key}: {ex.Message}");
            }
        }, keyArg, valueArg);
        configCommand.AddCommand(setCommand);
        
        // config get
        var getCommand = new Command("get", "Get configuration value");
        var getKeyArg = new Argument<string>("key", "Configuration key");
        getCommand.AddArgument(getKeyArg);
        getCommand.SetHandler((string key) =>
        {
            try
            {
                var value = configService.GetValue(key);
                if (value != null)
                {
                    Console.WriteLine($"{key}: {value}");
                }
                else
                {
                    Console.WriteLine($"❌ Configuration key '{key}' not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to get {key}: {ex.Message}");
            }
        }, getKeyArg);
        configCommand.AddCommand(getCommand);
        
        // config list
        var listCommand = new Command("list", "List all configuration");
        listCommand.SetHandler(() =>
        {
            try
            {
                var settings = configService.ListAll();
                Console.WriteLine("Configuration:");
                Console.WriteLine();
                
                foreach (var (key, value) in settings.OrderBy(x => x.Key))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Console.WriteLine($"  {key}: {value}");
                    }
                }
                
                Console.WriteLine();
                Console.WriteLine($"Config file: ~/.thresh/config.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to list configuration: {ex.Message}");
            }
        });
        configCommand.AddCommand(listCommand);
        
        // config delete
        var deleteCommand = new Command("delete", "Delete configuration value");
        var deleteKeyArg = new Argument<string>("key", "Configuration key to delete");
        deleteCommand.AddArgument(deleteKeyArg);
        deleteCommand.SetHandler((string key) =>
        {
            try
            {
                configService.DeleteValue(key);
                Console.WriteLine($"✅ Deleted {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to delete {key}: {ex.Message}");
            }
        }, deleteKeyArg);
        configCommand.AddCommand(deleteCommand);
        
        // config reset
        var resetCommand = new Command("reset", "Reset all configuration to defaults");
        resetCommand.SetHandler(() =>
        {
            try
            {
                Console.Write("⚠️  This will delete all configuration. Continue? (y/N): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                
                if (response == "y" || response == "yes")
                {
                    configService.Reset();
                    Console.WriteLine("✅ Configuration reset");
                }
                else
                {
                    Console.WriteLine("Cancelled");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to reset configuration: {ex.Message}");
            }
        });
        configCommand.AddCommand(resetCommand);
        
        rootCommand.AddCommand(configCommand);
    }

    private static void AddDistroCommand(RootCommand rootCommand)
    {
        var distroCommand = new Command("distro", "Manage custom distributions");
        
        // distro add subcommand
        var addCommand = new Command("add", "Add a custom distribution");
        var nameArg = new Argument<string>("name", "Distribution name (e.g., rocky, arch)");
        var urlOption = new Option<string?>("--url", "Direct URL to rootfs tarball");
        var versionOption = new Option<string?>("--version", "Distribution version");
        var packageManagerOption = new Option<string?>("--package-manager", "Package manager (apt, apk, dnf, yum, pacman, zypper)");
        var aiOption = new Option<bool>("--ai", "Use AI to discover distribution info automatically");
        
        addCommand.AddArgument(nameArg);
        addCommand.AddOption(urlOption);
        addCommand.AddOption(versionOption);
        addCommand.AddOption(packageManagerOption);
        addCommand.AddOption(aiOption);
        
        addCommand.SetHandler(async (string name, string? url, string? version, string? pkgMgr, bool useAi) =>
        {
            var configService = new Services.ConfigurationService();
            
            if (useAi)
            {
                // AI-powered discovery  
                Console.WriteLine($"🤖 Using AI to discover {name} distribution...");
                var factory = new Services.AiProviderFactory(configService);
                var aiService = factory.CreateAIService();
                
                // Discovery is not currently implemented
                Console.WriteLine($"❌ Distribution discovery not yet implemented");
                return;
            }
            else
            {
                // Manual method
                if (string.IsNullOrEmpty(url))
                {
                    Console.WriteLine("❌ Error: --url is required when not using --ai");
                    Console.WriteLine("Example:");
                    Console.WriteLine($"  thresh distro add {name} --url https://example.com/rootfs.tar.gz --version 9 --package-manager dnf");
                    return;
                }
                
                var customDistro = new Models.CustomDistribution
                {
                    Name = name,
                    Version = version ?? "latest",
                    RootfsUrl = url,
                    PackageManager = pkgMgr ?? "apt",
                    Key = $"{name.ToLowerInvariant()}-{(version ?? "latest")}"
                };
                
                var settings = configService.Load();
                settings.CustomDistributions[customDistro.Key] = customDistro;
                configService.Save(settings);
                
                Console.WriteLine($"✅ Added custom distribution: {customDistro.Key}");
                Console.WriteLine($"\nYou can now use it in blueprints:");
                Console.WriteLine($"  \"base\": \"{customDistro.Key}\"");
            }
        }, nameArg, urlOption, versionOption, packageManagerOption, aiOption);
        
        // distro list subcommand
        var listCommand = new Command("list", "List all available distributions");
        var customOnlyOption = new Option<bool>("--custom-only", "Show only custom distributions");
        listCommand.AddOption(customOnlyOption);
        
        listCommand.SetHandler((bool customOnly) =>
        {
            var configService = new Services.ConfigurationService();
            var settings = configService.Load();
            
            if (customOnly)
            {
                // Show only custom distributions
                if (settings.CustomDistributions.Count == 0)
                {
                    Console.WriteLine("No custom distributions configured.");
                    Console.WriteLine("\nAdd one with:");
                    Console.WriteLine("  thresh distro add rocky --ai");
                    Console.WriteLine("  thresh distro add mylinux --url https://... --version 1.0 --package-manager dnf");
                    return;
                }
                
                Console.WriteLine("Custom distributions:\n");
                foreach (var (key, distro) in settings.CustomDistributions)
                {
                    Console.WriteLine($"  {key,-20} - {distro.Name} {distro.Version} ({distro.PackageManager})");
                    if (!string.IsNullOrEmpty(distro.Description))
                        Console.WriteLine($"    {distro.Description}");
                }
            }
            else
            {
                // Show all distributions
                var registry = new Services.RootfsRegistry(configService);
                var allDistroKeys = registry.GetSupportedDistributions();
                
                Console.WriteLine("Available distributions:\n");
                Console.WriteLine($"{"NAME",-25} {"VERSION",-15} {"SOURCE",-20} {"PKG MANAGER",-15}");
                Console.WriteLine(new string('-', 80));
                
                // Build list with full info
                var distroInfoList = new List<(string Key, Services.RootfsRegistry.DistributionInfo Info)>();
                foreach (var key in allDistroKeys)
                {
                    var info = registry.GetDistribution(key);
                    if (info != null)
                    {
                        distroInfoList.Add((key, info));
                    }
                }
                
                // Group by source for better readability
                var vendorDistros = distroInfoList.Where(d => d.Info.Source == Services.RootfsRegistry.DistributionSource.Vendor).OrderBy(d => d.Key);
                var msStoreDistros = distroInfoList.Where(d => d.Info.Source == Services.RootfsRegistry.DistributionSource.MicrosoftStore).OrderBy(d => d.Key);
                
                // Vendor distributions
                if (vendorDistros.Any())
                {
                    foreach (var (key, info) in vendorDistros)
                    {
                        Console.WriteLine($"{key,-25} {info.Version,-15} {"Vendor",-20} {info.PackageManager,-15}");
                    }
                }
                
                // Microsoft Store distributions
                if (msStoreDistros.Any())
                {
                    Console.WriteLine(); // Separator
                    foreach (var (key, info) in msStoreDistros)
                    {
                        Console.WriteLine($"{key,-25} {info.Version,-15} {"Microsoft Store",-20} {info.PackageManager,-15}");
                    }
                }
                
                // Custom distributions
                if (settings.CustomDistributions.Count > 0)
                {
                    Console.WriteLine(); // Separator
                    foreach (var (key, distro) in settings.CustomDistributions.OrderBy(d => d.Key))
                    {
                        Console.WriteLine($"{key,-25} {distro.Version,-15} {"Custom",-20} {distro.PackageManager,-15}");
                    }
                }
                
                Console.WriteLine();
                Console.WriteLine($"Total: {allDistroKeys.Length} built-in + {settings.CustomDistributions.Count} custom");
            }
        }, customOnlyOption);
        
        // distro remove subcommand
        var removeCommand = new Command("remove", "Remove a custom distribution");
        var keyArg = new Argument<string>("key", "Distribution key to remove");
        removeCommand.AddArgument(keyArg);
        removeCommand.SetHandler((string key) =>
        {
            var configService = new Services.ConfigurationService();
            var settings = configService.Load();
            
            if (settings.CustomDistributions.Remove(key))
            {
                configService.Save(settings);
                Console.WriteLine($"✅ Removed custom distribution: {key}");
            }
            else
            {
                Console.WriteLine($"❌ Distribution not found: {key}");
            }
        }, keyArg);
        
        distroCommand.AddCommand(addCommand);
        distroCommand.AddCommand(listCommand);
        distroCommand.AddCommand(removeCommand);
        rootCommand.AddCommand(distroCommand);
    }
    
    private static void AddServeCommand(RootCommand rootCommand)
    {
        var serveCommand = new Command("serve", "Start MCP server for AI agent integration");
        var portOption = new Option<int>("--port", () => 8080, "Port to listen on (HTTP mode only)");
        var hostOption = new Option<string>("--host", () => "localhost", "Host to bind to (HTTP mode only)");
        var stdioOption = new Option<bool>("--stdio", "Use stdio transport (for VS Code, Cursor, Windsurf)");
        
        serveCommand.AddOption(portOption);
        serveCommand.AddOption(hostOption);
        serveCommand.AddOption(stdioOption);
        
        serveCommand.SetHandler(async (int port, string host, bool stdio) =>
        {
            try
            {
                if (stdio)
                {
                    // STDIO mode for VS Code and other MCP clients
                    var server = new Mcp.StdioMcpServer();
                    
                    // Handle Ctrl+C gracefully
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        server.Stop();
                    };

                    await server.RunAsync();
                }
                else
                {
                    // HTTP mode for testing and debugging
                    var server = new Mcp.McpServer(port, host);
                    
                    // Handle Ctrl+C gracefully
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        server.Stop();
                    };

                    await server.StartAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MCP server failed: {ex.Message}");
            }
        }, portOption, hostOption, stdioOption);
        
        rootCommand.AddCommand(serveCommand);
    }
    
    private static void AddMetricsCommand(RootCommand rootCommand)
    {
        var metricsCommand = new Command("metrics", "Display host system metrics");
        var jsonOption = new Option<bool>("--json", "Output as JSON");
        
        metricsCommand.AddOption(jsonOption);
        
        metricsCommand.SetHandler(async (bool json) =>
        {
            var containerService = Services.ContainerServiceFactory.Create();
            var metricsService = new Services.MetricsService(containerService);
            
            try
            {
                var metrics = await metricsService.CollectMetricsAsync();
                
                if (json)
                {
                    // Output as JSON using source-generated context for AOT compatibility
                    var jsonText = System.Text.Json.JsonSerializer.Serialize(metrics, Models.MetricsJsonContext.Default.HostMetrics);
                    Console.WriteLine(jsonText);
                }
                else
                {
                    // Output as formatted text
                    Console.WriteLine("📊 Host Metrics");
                    Console.WriteLine("═══════════════");
                    Console.WriteLine();
                    Console.WriteLine($"🖥️  Hostname: {metrics.Hostname}");
                    Console.WriteLine($"🔧 Platform: {metrics.Platform}");
                    Console.WriteLine($"📦 Runtime: {metrics.Runtime} {metrics.RuntimeVersion}");
                    Console.WriteLine();
                    
                    Console.WriteLine( $"⚙️  CPU:");
                    Console.WriteLine($"   Cores: {metrics.CpuCores}");
                    Console.WriteLine($"   Usage: {metrics.CpuPercent:F1}%");
                    Console.WriteLine();
                    
                    Console.WriteLine($"💾 Memory:");
                    Console.WriteLine($"   Total: {metrics.MemoryTotalGb:F2} GB");
                    Console.WriteLine($"   Used:  {metrics.MemoryUsedGb:F2} GB");
                    Console.WriteLine($"   Usage: {metrics.MemoryPercent:F1}%");
                    Console.WriteLine();
                    
                    Console.WriteLine($"💿 Storage:");
                    Console.WriteLine($"   Total: {metrics.StorageTotalGb:F2} GB");
                    Console.WriteLine($"   Free:  {metrics.StorageFreeGb:F2} GB");
                    Console.WriteLine($"   Usage: {metrics.StoragePercent:F1}%");
                    Console.WriteLine();
                    
                    if (!string.IsNullOrEmpty(metrics.IpAddress))
                    {
                        Console.WriteLine($"🌐 Network:");
                        Console.WriteLine($"   IP Address: {metrics.IpAddress}");
                        
                        if (metrics.IpAddresses != null && metrics.IpAddresses.Count > 1)
                        {
                            Console.WriteLine($"   All IPs: {string.Join(", ", metrics.IpAddresses)}");
                        }
                        
                        if (!string.IsNullOrEmpty(metrics.ExternalIp))
                        {
                            Console.WriteLine($"   External IP: {metrics.ExternalIp}");
                        }
                        Console.WriteLine();
                    }
                    
                    if (metrics.LoadAverage != null && metrics.LoadAverage.Count == 3)
                    {
                        Console.WriteLine($"📈 Load Average:");
                        Console.WriteLine($"   1 min:  {metrics.LoadAverage[0]:F2}");
                        Console.WriteLine($"   5 min:  {metrics.LoadAverage[1]:F2}");
                        Console.WriteLine($"   15 min: {metrics.LoadAverage[2]:F2}");
                        Console.WriteLine();
                    }
                    
                    Console.WriteLine($"📦 Containers: {metrics.Containers}");
                    
                    if (!string.IsNullOrEmpty(metrics.DockerStorageDriver))
                    {
                        Console.WriteLine($"🐳 Docker:");
                        Console.WriteLine($"   Storage Driver: {metrics.DockerStorageDriver}");
                        if (!string.IsNullOrEmpty(metrics.DockerRootDir))
                        {
                            Console.WriteLine($"   Root Directory: {metrics.DockerRootDir}");
                        }
                    }
                    
                    if (metrics.UptimeSeconds.HasValue)
                    {
                        var uptime = TimeSpan.FromSeconds(metrics.UptimeSeconds.Value);
                        Console.WriteLine($"⏱️  Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
                    }
                    
                    Console.WriteLine();
                    Console.WriteLine($"🕐 Collected: {metrics.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to collect metrics: {ex.Message}");
            }
        }, jsonOption);
        
        rootCommand.AddCommand(metricsCommand);
    }

    private static void AddTestSdkCommand(RootCommand rootCommand)
    {
        var testCommand = new Command("test-sdk", "Test GitHub Copilot SDK connection");
        
        testCommand.SetHandler(async () =>
        {
            await CopilotSdkTest.RunAsync();
        });
        
        rootCommand.AddCommand(testCommand);
    }

    /// <summary>
    /// Clean JSON output by removing markdown code blocks
    /// </summary>
    private static string CleanJsonOutput(string rawOutput)
    {
        var cleaned = rawOutput.Trim();
        
        // Remove markdown code blocks if present
        if (!cleaned.Contains("```"))
            return cleaned;

        var lines = cleaned.Split('\n');
        var jsonLines = new List<string>();
        var inCodeBlock = false;

        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("```"))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock)
            {
                jsonLines.Add(line);
            }
        }

        return string.Join("\n", jsonLines).Trim();
    }
}
