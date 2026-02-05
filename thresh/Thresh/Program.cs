using System.CommandLine;
using Thresh.Models;
using Thresh.Services;

namespace Thresh;

class Program
{
    private const string Version = "1.0.0-phase0";
    
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("thresh - AI-Powered WSL Development Environments");
        
        // Verbose option
        var verboseOption = new Option<bool>(
            aliases: ["--verbose"],
            description: "Enable verbose logging");
        rootCommand.AddGlobalOption(verboseOption);
        
        // Add commands
        AddUpCommand(rootCommand);
        AddListCommand(rootCommand);
        AddDestroyCommand(rootCommand);
        AddBlueprintsCommand(rootCommand);
        AddGenerateCommand(rootCommand);
        AddChatCommand(rootCommand);
        AddConfigCommand(rootCommand);
        AddDistroCommand(rootCommand);
        AddDistrosCommand(rootCommand);
        AddServeCommand(rootCommand);
        AddVersionCommand(rootCommand);
        
        // Root handler (when no command specified)
        rootCommand.SetHandler((bool verbose) =>
        {
            DisplayHelp();
        }, verboseOption);
        
        return await rootCommand.InvokeAsync(args);
    }
    
    private static void DisplayHelp()
    {
        Console.WriteLine("thresh - AI-Powered WSL Development Environments");
        Console.WriteLine();
        Console.WriteLine("Usage: thresh [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  up          Provision a WSL environment from a blueprint");
        Console.WriteLine("  list        List WSL environments");
        Console.WriteLine("  destroy     Remove a WSL environment");
        Console.WriteLine("  blueprints  List available blueprints");
        Console.WriteLine("  generate    Generate blueprint from natural language (AI)");
        Console.WriteLine("  chat        Interactive AI chat mode for blueprint help");
        Console.WriteLine("  config      Manage configuration");
        Console.WriteLine("  distro      Manage custom distributions");
        Console.WriteLine("  distros     List all available distributions");
        Console.WriteLine("  serve       Start MCP server");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --verbose        Enable verbose logging");
        Console.WriteLine("  --help           Display help information");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  thresh version");
        Console.WriteLine("  thresh up alpine-minimal");
        Console.WriteLine("  thresh generate 'Python ML environment with Jupyter'");
        Console.WriteLine("  thresh list");
        Console.WriteLine("  thresh config set openai-api-key <key>");
        Console.WriteLine("  thresh config set aiprovider openai");  // or 'copilot'
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
            
            // Show WSL info
            var wslService = new Services.WslService();
            var wslInfo = await wslService.GetWslInfoAsync();
            
            if (wslInfo.Available)
            {
                Console.WriteLine($"WSL: {wslInfo.Version}");
                if (wslInfo.KernelVersion != null)
                    Console.WriteLine($"Kernel: {wslInfo.KernelVersion}");
                Console.WriteLine($"Distributions: {wslInfo.DistributionCount}");
            }
            else
            {
                Console.WriteLine($"WSL: Not available ({wslInfo.Version})");
            }
        });
        
        rootCommand.AddCommand(versionCommand);
    }
    
    private static void AddUpCommand(RootCommand rootCommand)
    {
        var upCommand = new Command("up", "Provision a WSL environment from a blueprint");
        var blueprintArg = new Argument<string>("blueprint", "Blueprint name or path to JSON file");
        var nameOption = new Option<string?>("--name", "Custom name for the environment");
        var verboseOption = new Option<bool>("--verbose", "Show detailed output");
        
        upCommand.AddArgument(blueprintArg);
        upCommand.AddOption(nameOption);
        upCommand.AddOption(verboseOption);
        
        upCommand.SetHandler(async (string blueprint, string? name, bool verbose) =>
        {
            var wslService = new Services.WslService();
            var configService = new Services.ConfigurationService();
            var rootfsRegistry = new Services.RootfsRegistry(configService);
            var blueprintService = new Services.BlueprintService(wslService, rootfsRegistry);
            
            // Check if WSL is available
            if (!await wslService.IsWslAvailableAsync())
            {
                Console.WriteLine("❌ WSL is not available on this system");
                Console.WriteLine("   Install WSL: wsl --install");
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
                if (await wslService.EnvironmentExistsAsync(envName))
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
                Console.WriteLine($"  wsl -d thresh-{envName}");
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
        var listCommand = new Command("list", "List WSL environments");
        var allOption = new Option<bool>("--all", "Include all WSL distributions, not just thresh environments");
        listCommand.AddOption(allOption);
        
        listCommand.SetHandler(async (bool all) =>
        {
            var wslService = new Services.WslService();
            
            // Check if WSL is available
            if (!await wslService.IsWslAvailableAsync())
            {
                Console.WriteLine("❌ WSL is not available on this system");
                Console.WriteLine("   Install WSL: wsl --install");
                return;
            }
            
            var environments = await wslService.ListEnvironmentsAsync(all);
            
            if (environments.Count == 0)
            {
                Console.WriteLine("No environments found.");
                if (!all)
                    Console.WriteLine("Use --all to see all WSL distributions.");
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
    
    private static void AddDestroyCommand(RootCommand rootCommand)
    {
        var destroyCommand = new Command("destroy", "Remove a WSL environment");
        var nameArg = new Argument<string>("name", "Environment name to remove");
        var forceOption = new Option<bool>("--force", "Skip confirmation prompt");
        
        destroyCommand.AddArgument(nameArg);
        destroyCommand.AddOption(forceOption);
        
        destroyCommand.SetHandler(async (string name, bool force) =>
        {
            var wslService = new Services.WslService();
            
            // Check if WSL is available
            if (!await wslService.IsWslAvailableAsync())
            {
                Console.WriteLine("❌ WSL is not available on this system");
                return;
            }
            
            // Check if environment exists
            if (!await wslService.EnvironmentExistsAsync(name))
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
            if (await wslService.RemoveEnvironmentAsync(name))
            {
                Console.WriteLine($"✅ Environment '{name}' removed successfully");
            }
            else
            {
                Console.WriteLine($"❌ Failed to remove environment '{name}'");
            }
        }, nameArg, forceOption);
        
        rootCommand.AddCommand(destroyCommand);
    }
    
    private static void AddBlueprintsCommand(RootCommand rootCommand)
    {
        var blueprintsCommand = new Command("blueprints", "List available blueprints");
        
        blueprintsCommand.SetHandler(() =>
        {
            var wslService = new Services.WslService();
            var configService = new Services.ConfigurationService();
            var rootfsRegistry = new Services.RootfsRegistry(configService);
            var blueprintService = new Services.BlueprintService(wslService, rootfsRegistry);
            
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
        });
        
        rootCommand.AddCommand(blueprintsCommand);
    }
    
    private static void AddGenerateCommand(RootCommand rootCommand)
    {
        var generateCommand = new Command("generate", "Generate blueprint from natural language using AI");
        var promptArg = new Argument<string>("prompt", "Description of desired environment");
        var outputOption = new Option<string?>("--output", "Save blueprint to file");
        var modelOption = new Option<string?>("--model", "AI model to use (default: gpt-4o)");
        var providerOption = new Option<string?>("--provider", "AI provider: openai, azure, or github (auto-detect if not specified)");
        var noStreamOption = new Option<bool>("--no-stream", "Disable streaming output");
        
        generateCommand.AddArgument(promptArg);
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(modelOption);
        generateCommand.AddOption(providerOption);
        generateCommand.AddOption(noStreamOption);
        
        generateCommand.SetHandler(async (string prompt, string? output, string? model, string? provider, bool noStream) =>
        {
            Console.WriteLine($"🎯 Generating blueprint for: '{prompt}'");
            Console.WriteLine();
            
            try
            {
                var configService = new Services.ConfigurationService();
                var aiService = Utilities.AIServiceFactory.CreateAIService(configService, model, provider);
                
                var jsonContent = await aiService.GenerateBlueprintAsync(prompt, streaming: !noStream);
                
                if (noStream)
                {
                    Console.WriteLine(jsonContent);
                    Console.WriteLine();
                }
                
                // Clean the output (OpenAI service has CleanJsonOutput method)
                var cleanedJson = (aiService as OpenAIService)?.CleanJsonOutput(jsonContent) ?? jsonContent;
                
                // Save to file if requested
                if (!string.IsNullOrEmpty(output))
                {
                    File.WriteAllText(output, cleanedJson);
                    Console.WriteLine($"✅ Blueprint saved to: {output}");
                    Console.WriteLine();
                    Console.WriteLine("To provision this environment:");
                    Console.WriteLine($"  thresh up {output}");
                }
                else
                {
                    Console.WriteLine("To save this blueprint:");
                    Console.WriteLine($"  thresh generate '{prompt}' --output my-blueprint.json");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Generation failed: {ex.Message}");
                Console.ResetColor();
            }
        }, promptArg, outputOption, modelOption, providerOption, noStreamOption);
        
        rootCommand.AddCommand(generateCommand);
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
                var copilot = new Services.CopilotService(configService, model, provider);
                await copilot.ChatModeAsync();
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
        var keyArg = new Argument<string>("key", "Configuration key (e.g., openai-api-key, github-token, default-model)");
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
                var aiService = Utilities.AIServiceFactory.CreateAIService(configService) as OpenAIService;
                var distro = await aiService?.DiscoverDistributionAsync(name);
                
                if (distro == null)
                {
                    Console.WriteLine("❌ Could not discover distribution information");
                    return;
                }
                
                // Save to config
                var settings = configService.Load();
                settings.CustomDistributions[distro.Key] = distro;
                configService.Save(settings);
                
                Console.WriteLine($"✅ Added custom distribution: {distro.Key}");
                Console.WriteLine($"\nYou can now use it in blueprints:");
                Console.WriteLine($"  \"base\": \"{distro.Key}\"");
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
        var listCommand = new Command("list", "List custom distributions");
        listCommand.SetHandler(() =>
        {
            var configService = new Services.ConfigurationService();
            var settings = configService.Load();
            
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
        });
        
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

    private static void AddDistrosCommand(RootCommand rootCommand)
    {
        var distrosCommand = new Command("distros", "List all available distributions");
        
        distrosCommand.SetHandler(() =>
        {
            var registry = new Services.RootfsRegistry(new Services.ConfigurationService());
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
            var configService = new Services.ConfigurationService();
            var settings = configService.Load();
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
        });
        
        rootCommand.AddCommand(distrosCommand);
    }
    
    private static void AddServeCommand(RootCommand rootCommand)
    {
        var serveCommand = new Command("serve", "Start MCP server");
        var portOption = new Option<int>("--port", () => 8080, "Port to listen on");
        var hostOption = new Option<string>("--host", () => "localhost", "Host to bind to");
        
        serveCommand.AddOption(portOption);
        serveCommand.AddOption(hostOption);
        
        serveCommand.SetHandler(async (int port, string host) =>
        {
            try
            {
                var server = new Mcp.McpServer(port, host);
                
                // Handle Ctrl+C gracefully
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    server.Stop();
                };

                await server.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MCP server failed: {ex.Message}");
            }
        }, portOption, hostOption);
        
        rootCommand.AddCommand(serveCommand);
    }
}
