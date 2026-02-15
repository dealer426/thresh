using System.Text.Json;
using Thresh.Models;
using Thresh.Utilities;
using YamlDotNet.Serialization;

namespace Thresh.Services;

/// <summary>
/// Service for managing and provisioning blueprints
/// Supports both JSON and YAML formats
/// </summary>
public class BlueprintService
{
    private readonly IContainerService _containerService;
    private readonly RootfsRegistry _rootfsRegistry;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public BlueprintService(IContainerService containerService, RootfsRegistry rootfsRegistry)
    {
        _containerService = containerService;
        _rootfsRegistry = rootfsRegistry;
    }

    /// <summary>
    /// Load a blueprint from a file path (supports JSON and YAML)
    /// </summary>
    public Blueprint LoadBlueprint(string blueprintPath)
    {
        if (!File.Exists(blueprintPath))
            throw new FileNotFoundException($"Blueprint file not found: {blueprintPath}");

        var content = File.ReadAllText(blueprintPath);
        var extension = Path.GetExtension(blueprintPath).ToLowerInvariant();

        // If YAML, convert to JSON first (maintaining JSON source generation benefits)
        if (extension == ".yaml" || extension == ".yml")
        {
            try
            {
                var deserializer = new DeserializerBuilder().Build();
                var yamlObject = deserializer.Deserialize<object>(content);
                
                var serializer = new SerializerBuilder()
                    .JsonCompatible()
                    .Build();
                var json = serializer.Serialize(yamlObject);
                
                return JsonSerializer.Deserialize(json, BlueprintJsonContext.Default.Blueprint)
                    ?? throw new InvalidOperationException($"Failed to deserialize YAML blueprint: {blueprintPath}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse YAML blueprint: {blueprintPath}. {ex.Message}", ex);
            }
        }

        // Direct JSON parsing (fastest path)
        return JsonSerializer.Deserialize(content, BlueprintJsonContext.Default.Blueprint)
            ?? throw new InvalidOperationException($"Failed to deserialize JSON blueprint: {blueprintPath}");
    }

    /// <summary>
    /// Load a blueprint from the bundled blueprints directory
    /// Tries .json, .yaml, and .yml extensions in order
    /// </summary>
    public Blueprint LoadBundledBlueprint(string blueprintName)
    {
        var blueprintsDir = Path.Combine(AppContext.BaseDirectory, "blueprints");
        
        // Try JSON first (fastest)
        var jsonPath = Path.Combine(blueprintsDir, $"{blueprintName}.json");
        if (File.Exists(jsonPath))
            return LoadBlueprint(jsonPath);
        
        // Try YAML variants
        var yamlPath = Path.Combine(blueprintsDir, $"{blueprintName}.yaml");
        if (File.Exists(yamlPath))
            return LoadBlueprint(yamlPath);
        
        var ymlPath = Path.Combine(blueprintsDir, $"{blueprintName}.yml");
        if (File.Exists(ymlPath))
            return LoadBlueprint(ymlPath);

        throw new FileNotFoundException($"Bundled blueprint not found: {blueprintName} (tried .json, .yaml, .yml)");
    }

    /// <summary>
    /// List available bundled blueprints (JSON and YAML)
    /// </summary>
    public List<string> ListBundledBlueprints()
    {
        var blueprintsDir = Path.Combine(AppContext.BaseDirectory, "blueprints");
        if (!Directory.Exists(blueprintsDir))
            return new List<string>();

        var blueprints = new HashSet<string>();
        
        // Add JSON blueprints
        foreach (var file in Directory.GetFiles(blueprintsDir, "*.json"))
            blueprints.Add(Path.GetFileNameWithoutExtension(file)!);
        
        // Add YAML blueprints
        foreach (var file in Directory.GetFiles(blueprintsDir, "*.yaml"))
            blueprints.Add(Path.GetFileNameWithoutExtension(file)!);
        
        // Add YML blueprints
        foreach (var file in Directory.GetFiles(blueprintsDir, "*.yml"))
            blueprints.Add(Path.GetFileNameWithoutExtension(file)!);

        return blueprints.OrderBy(b => b).ToList();
    }

    /// <summary>
    /// Provision an environment from a blueprint synchronously
    /// </summary>
    public async Task ProvisionEnvironmentAsync(string environmentName, Blueprint blueprint, bool verbose = false)
    {
        Console.WriteLine($"Creating environment '{environmentName}' from blueprint '{blueprint.Name}'");

        // Step 1: Install base distribution
        Console.WriteLine($"[1/5] Installing base distribution: {blueprint.Base}");
        await InstallBaseDistributionAsync(environmentName, blueprint.Base, blueprint.Name, verbose);

        // Step 2: Install packages FIRST (before scripts that may depend on them)
        if (blueprint.Packages != null && blueprint.Packages.Count > 0)
        {
            Console.WriteLine($"[2/5] Installing packages ({blueprint.Packages.Count} packages)...");
            await InstallPackagesAsync(environmentName, blueprint.Packages, verbose);
        }
        else
        {
            Console.WriteLine("[2/5] No packages to install [SKIP]");
        }

        // Step 3: Run setup script (after packages are installed)
        if (!string.IsNullOrEmpty(blueprint.Scripts?.Setup))
        {
            Console.WriteLine("[3/5] Running setup script...");
            await RunScriptAsync(environmentName, blueprint.Scripts.Setup, verbose);
        }
        else
        {
            Console.WriteLine("[3/5] No setup script [SKIP]");
        }

        // Step 4: Set environment variables
        if (blueprint.Environment != null && blueprint.Environment.Count > 0)
        {
            Console.WriteLine("[4/5] Configuring environment variables...");
            await ConfigureEnvironmentAsync(environmentName, blueprint.Environment, verbose);
        }
        else
        {
            Console.WriteLine("[4/5] No environment variables [SKIP]");
        }

        // Step 5: Run post-install script
        if (!string.IsNullOrEmpty(blueprint.Scripts?.PostInstall))
        {
            Console.WriteLine("[5/5] Running post-install script...");
            await RunScriptAsync(environmentName, blueprint.Scripts.PostInstall, verbose);
        }
        else
        {
            Console.WriteLine("[5/5] No post-install script [SKIP]");
        }

        Console.WriteLine();
        Console.WriteLine($"[OK] Environment '{environmentName}' provisioned successfully!");
        
        // Save metadata for tracking
        SaveMetadata(environmentName, blueprint);
    }

    private void SaveMetadata(string environmentName, Blueprint blueprint)
    {
        var homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        var metadataDir = Path.Combine(homeDir, ".thresh", "metadata");
        Directory.CreateDirectory(metadataDir);
        
        var distroInfo = _rootfsRegistry.GetDistribution(_rootfsRegistry.NormalizeDistributionKey(blueprint.Base));
        
        var metadataFile = Path.Combine(metadataDir, $"{environmentName}.json");
        var metadata = new EnvironmentMetadata
        {
            EnvironmentName = environmentName,
            BlueprintName = blueprint.Name,
            Created = DateTime.UtcNow,
            Base = blueprint.Base,
            Description = blueprint.Description,
            DistributionSource = distroInfo?.Source.ToString()
        };
        
        var json = JsonSerializer.Serialize(metadata, BlueprintJsonContext.Default.EnvironmentMetadata);
        File.WriteAllText(metadataFile, json);
    }
    
    public static string? LoadBlueprintName(string environmentName)
    {
        try
        {
            var homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            var metadataFile = Path.Combine(homeDir, ".thresh", "metadata", $"{environmentName}.json");
            
            if (!File.Exists(metadataFile))
                return null;
            
            var json = File.ReadAllText(metadataFile);
            var metadata = JsonSerializer.Deserialize(json, BlueprintJsonContext.Default.EnvironmentMetadata);
            
            return metadata?.BlueprintName;
        }
        catch
        {
            return null;
        }
    }

    private async Task InstallBaseDistributionAsync(string environmentName, string baseDistro, string blueprintName, bool verbose)
    {
        var distroName = "thresh-" + environmentName;

        // Normalize the distribution key
        var distroKey = _rootfsRegistry.NormalizeDistributionKey(baseDistro);
        var distroInfo = _rootfsRegistry.GetDistribution(distroKey);

        if (distroInfo == null)
        {
            var supported = string.Join(", ", _rootfsRegistry.GetSupportedDistributions());
            throw new Exception($"Unsupported distribution: {baseDistro}. Supported: {supported}");
        }

        if (verbose)
        {
            Console.WriteLine($"  Distribution: {distroInfo.GetFullName()}");
            Console.WriteLine($"  Package Manager: {distroInfo.PackageManager}");
            Console.WriteLine($"  Source: {distroInfo.Source}");
        }

        // Handle Microsoft Store distributions differently
        if (distroInfo.Source == RootfsRegistry.DistributionSource.MicrosoftStore)
        {
            await InstallMicrosoftStoreDistroAsync(distroName, distroInfo, blueprintName, verbose);
        }
        else
        {
            await InstallVendorDistroAsync(distroName, environmentName, distroInfo, blueprintName, verbose);
        }

        Console.WriteLine("  [OK] Base distribution installed");
    }

    private async Task InstallMicrosoftStoreDistroAsync(string distroName, RootfsRegistry.DistributionInfo distroInfo, string blueprintName, bool verbose)
    {
        if (string.IsNullOrEmpty(distroInfo.WslInstallName))
            throw new InvalidOperationException($"MS Store distribution {distroInfo.Name} is missing WslInstallName");

        Console.WriteLine($"  [MS STORE] Installing via wsl --install {distroInfo.WslInstallName}...");
        
        // Use wsl --install to download and install from Microsoft Store
        var result = await ProcessHelper.ExecuteAsync(600, "wsl", "--install", distroInfo.WslInstallName, "--no-launch");
        
        if (result.ExitCode != 0)
        {
            throw new Exception($"Failed to install {distroInfo.WslInstallName} from Microsoft Store: {result.Output}");
        }

        if (verbose)
        {
            Console.WriteLine($"  MS Store installation complete");
        }

        // Now export and re-import with our custom name
        var homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        var tempExport = Path.Combine(homeDir, ".thresh", "temp", $"{distroInfo.WslInstallName}.tar");
        Directory.CreateDirectory(Path.GetDirectoryName(tempExport)!);

        Console.WriteLine($"  Exporting as {distroName}...");
        result = await ProcessHelper.ExecuteAsync(300, "wsl", "--export", distroInfo.WslInstallName, tempExport);
        
        if (result.ExitCode != 0)
        {
            throw new Exception($"Failed to export {distroInfo.WslInstallName}: {result.Output}");
        }

        var installPath = Path.Combine(homeDir, "AppData", "Local", "thresh", distroName);
        await ImportDistroAsync(distroName, installPath, tempExport, blueprintName, verbose);

        // Clean up temp export and original MS Store distro
        File.Delete(tempExport);
        await ProcessHelper.ExecuteAsync(60, "wsl", "--unregister", distroInfo.WslInstallName);
    }

    private async Task InstallVendorDistroAsync(string distroName, string environmentName, RootfsRegistry.DistributionInfo distroInfo, string blueprintName, bool verbose)
    {
        var homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        var installPath = Path.Combine(homeDir, "AppData", "Local", "thresh", environmentName);
        
        // Check if we should use Docker Hub image (Linux only)
        var useDockerImage = _containerService.Platform != "Windows" 
                             && !string.IsNullOrEmpty(distroInfo.DockerImage);
        
        if (useDockerImage)
        {
            // LINUX PATH: Use Docker Hub for faster, smaller downloads
            Console.WriteLine($"  Pulling from Docker Hub: {distroInfo.DockerImage}");
            
            var tool = _containerService.RuntimeName == "WSL" ? "docker" : _containerService.RuntimeName;
            var pullResult = await ProcessHelper.ExecuteAsync(120, tool, "pull", distroInfo.DockerImage!);
            
            if (!pullResult.IsSuccess)
            {
                var error = pullResult.Error ?? pullResult.GetOutputAsString();
                throw new Exception($"Failed to pull {distroInfo.DockerImage}: {error}");
            }
            
            if (verbose)
            {
                Console.WriteLine($"  Successfully pulled Docker image");
                Console.WriteLine($"  Importing as: {distroName}");
            }
            
            // Pass Docker image name directly - ContainerdService will create container from it
            await ImportDistroAsync(distroName, installPath, distroInfo.DockerImage!, blueprintName, verbose);
        }
        else
        {
            // WINDOWS/WSL PATH: Use traditional rootfs download (unchanged)
            var cacheDir = Path.Combine(homeDir, ".thresh", "rootfs-cache");
            Directory.CreateDirectory(cacheDir);

            var cacheFilename = _rootfsRegistry.GetCacheFilename(_rootfsRegistry.NormalizeDistributionKey(distroInfo.GetFullName()));
            var cachedRootfs = Path.Combine(cacheDir, cacheFilename);

            // Download rootfs if not cached
            if (!File.Exists(cachedRootfs))
            {
                Console.WriteLine("  [CACHE MISS] Downloading rootfs (first time only)...");
                await DownloadRootfsAsync(distroInfo.RootfsUrl, cachedRootfs, verbose);
                Console.WriteLine($"  [OK] Rootfs cached at: {cachedRootfs}");
            }
            else
            {
                Console.WriteLine("  Using cached rootfs");
            }

            if (verbose)
            {
                Console.WriteLine($"  Importing as: {distroName}");
                Console.WriteLine($"  Install path: {installPath}");
            }

            await ImportDistroAsync(distroName, installPath, cachedRootfs, blueprintName, verbose);
        }
    }

    private async Task DownloadRootfsAsync(string url, string destinationPath, bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine($"    URL: {url}");
            Console.WriteLine($"    Destination: {destinationPath}");
        }

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(10);

        try
        {
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            await contentStream.CopyToAsync(fileStream);

            if (verbose)
            {
                Console.WriteLine("    Download complete");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download rootfs from {url}: {ex.Message}", ex);
        }
    }

    private async Task ImportDistroAsync(string distroName, string installPath, string tarballPath, string blueprintName, bool verbose)
    {
        // Create install directory
        Directory.CreateDirectory(installPath);

        // Platform-specific import time estimates
        var importMessage = _containerService.Platform == "Windows"
            ? $"  Importing as {distroName} (this may take 2-3 minutes)..."
            : $"  Importing as {distroName}...";
        
        Console.WriteLine(importMessage);

        // Use container service for platform-agnostic import
        var success = await _containerService.ImportEnvironmentAsync(
            distroName.Replace("thresh-", ""), 
            tarballPath, 
            installPath,
            blueprintName);

        if (!success)
        {
            throw new Exception($"Failed to import distribution via {_containerService.RuntimeName}");
        }

        if (verbose)
        {
            Console.WriteLine($"    Imported via {_containerService.RuntimeName}");
        }

        Console.WriteLine("  Import complete");
    }

    private async Task InstallPackagesAsync(string environmentName, List<string> packages, bool verbose)
    {
        var distroName = "thresh-" + environmentName;
        var packageList = string.Join(" ", packages);

        if (verbose)
        {
            Console.WriteLine($"  Packages: {packageList}");
        }

        // Try to detect package manager and run appropriate command
        var detectCmd = $@"if command -v apt-get >/dev/null 2>&1; then 
apt-get update -qq && apt-get install -y -qq {packageList}; 
elif command -v apk >/dev/null 2>&1; then 
apk update -q && apk add --no-cache {packageList}; 
elif command -v dnf >/dev/null 2>&1; then 
dnf install -y -q {packageList}; 
elif command -v yum >/dev/null 2>&1; then 
yum install -y -q {packageList}; 
else 
echo 'ERROR: No supported package manager found'; exit 1; 
fi";

        // Package installation can take longer, especially for apt-get update
        await ExecuteInDistroAsync(distroName, detectCmd, verbose, timeoutSeconds: 300);
        Console.WriteLine("  [OK] Packages installed");
    }

    private async Task RunScriptAsync(string environmentName, string script, bool verbose)
    {
        var distroName = "thresh-" + environmentName;

        if (verbose)
        {
            Console.WriteLine("  Script content:");
            foreach (var line in script.Split('\n'))
            {
                Console.WriteLine($"    {line}");
            }
        }

        // Write script with bash shebang
        var scriptContent = "#!/bin/bash\nset -e\n" + script;
        await ExecuteInDistroAsync(distroName, scriptContent, verbose);
        Console.WriteLine("  [OK] Script executed");
    }

    private async Task ConfigureEnvironmentAsync(string environmentName, Dictionary<string, string> envVars, bool verbose)
    {
        var distroName = "thresh-" + environmentName;

        // Create environment configuration
        var envConfig = string.Join("\n", envVars.Select(kv => $"export {kv.Key}=\"{kv.Value}\""));

        if (verbose)
        {
            foreach (var kv in envVars)
            {
                Console.WriteLine($"  {kv.Key}={kv.Value}");
            }
        }

        // Append to /etc/profile
        var command = $"echo '{envConfig}' >> /etc/profile";
        await ExecuteInDistroAsync(distroName, command, verbose);
        Console.WriteLine("  [OK] Environment configured");
    }

    private async Task ExecuteInDistroAsync(string distroName, string command, bool verbose, int timeoutSeconds = 30)
    {
        // Normalize line endings to Unix format (LF only) to avoid sh interpretation issues
        var normalizedCommand = command.Replace("\r\n", "\n").Replace("\r", "\n");
        
        // Use container service for platform-agnostic execution
        var envName = distroName.Replace("thresh-", "");
        var result = await _containerService.ExecuteCommandAsync(envName, normalizedCommand, timeoutSeconds);

        if (verbose && result.HasOutput())
        {
            foreach (var line in result.Output)
            {
                Console.WriteLine($"    {line}");
            }
        }

        if (!result.IsSuccess)
        {
            var error = result.Error ?? result.GetOutputAsString();
            throw new Exception($"Command failed with exit code {result.ExitCode}: {error}");
        }
    }

    /// <summary>
    /// Save a blueprint to a JSON file
    /// </summary>
    public void SaveBlueprint(Blueprint blueprint, string filePath)
    {
        var json = JsonSerializer.Serialize(blueprint, BlueprintJsonContext.Default.Blueprint);
        File.WriteAllText(filePath, json);
    }
}
