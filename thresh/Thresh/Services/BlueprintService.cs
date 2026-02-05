using System.Text.Json;
using Thresh.Models;
using Thresh.Utilities;

namespace Thresh.Services;

/// <summary>
/// Service for managing and provisioning blueprints
/// </summary>
public class BlueprintService
{
    private readonly WslService _wslService;
    private readonly RootfsRegistry _rootfsRegistry;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public BlueprintService(WslService wslService, RootfsRegistry rootfsRegistry)
    {
        _wslService = wslService;
        _rootfsRegistry = rootfsRegistry;
    }

    /// <summary>
    /// Load a blueprint from a file path
    /// </summary>
    public Blueprint LoadBlueprint(string blueprintPath)
    {
        if (!File.Exists(blueprintPath))
            throw new FileNotFoundException($"Blueprint file not found: {blueprintPath}");

        var json = File.ReadAllText(blueprintPath);
        return JsonSerializer.Deserialize(json, BlueprintJsonContext.Default.Blueprint)
            ?? throw new InvalidOperationException($"Failed to deserialize blueprint: {blueprintPath}");
    }

    /// <summary>
    /// Load a blueprint from the bundled blueprints directory
    /// </summary>
    public Blueprint LoadBundledBlueprint(string blueprintName)
    {
        var blueprintPath = Path.Combine("blueprints", $"{blueprintName}.json");

        if (!File.Exists(blueprintPath))
            throw new FileNotFoundException($"Bundled blueprint not found: {blueprintName}");

        return LoadBlueprint(blueprintPath);
    }

    /// <summary>
    /// List available bundled blueprints
    /// </summary>
    public List<string> ListBundledBlueprints()
    {
        var blueprintsDir = "blueprints";
        if (!Directory.Exists(blueprintsDir))
            return new List<string>();

        return Directory.GetFiles(blueprintsDir, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .ToList();
    }

    /// <summary>
    /// Provision an environment from a blueprint synchronously
    /// </summary>
    public async Task ProvisionEnvironmentAsync(string environmentName, Blueprint blueprint, bool verbose = false)
    {
        Console.WriteLine($"Creating environment '{environmentName}' from blueprint '{blueprint.Name}'");

        // Step 1: Install base distribution
        Console.WriteLine($"[1/5] Installing base distribution: {blueprint.Base}");
        await InstallBaseDistributionAsync(environmentName, blueprint.Base, verbose);

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

    private async Task InstallBaseDistributionAsync(string environmentName, string baseDistro, bool verbose)
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
            await InstallMicrosoftStoreDistroAsync(distroName, distroInfo, verbose);
        }
        else
        {
            await InstallVendorDistroAsync(distroName, environmentName, distroInfo, verbose);
        }

        Console.WriteLine("  [OK] Base distribution installed");
    }

    private async Task InstallMicrosoftStoreDistroAsync(string distroName, RootfsRegistry.DistributionInfo distroInfo, bool verbose)
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
        await ImportDistroAsync(distroName, installPath, tempExport, verbose);

        // Clean up temp export and original MS Store distro
        File.Delete(tempExport);
        await ProcessHelper.ExecuteAsync(60, "wsl", "--unregister", distroInfo.WslInstallName);
    }

    private async Task InstallVendorDistroAsync(string distroName, string environmentName, RootfsRegistry.DistributionInfo distroInfo, bool verbose)
    {
        // Setup cache directory
        var homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
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
            Console.WriteLine("  [CACHE HIT] Using cached rootfs (fast!)");
        }

        // Import as our custom-named environment
        var installPath = Path.Combine(homeDir, "AppData", "Local", "thresh", environmentName);

        if (verbose)
        {
            Console.WriteLine($"  Importing as: {distroName}");
            Console.WriteLine($"  Install path: {installPath}");
        }

        await ImportDistroAsync(distroName, installPath, cachedRootfs, verbose);
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

    private async Task ImportDistroAsync(string distroName, string installPath, string tarballPath, bool verbose)
    {
        // Create install directory
        Directory.CreateDirectory(installPath);

        Console.WriteLine($"  Importing as {distroName} (this may take 2-3 minutes)...");

        var result = await ProcessHelper.ExecuteAsync(300, "wsl", "--import", distroName, installPath, tarballPath);

        if (!result.IsSuccess)
        {
            var error = result.Error ?? result.GetOutputAsString();
            throw new Exception($"Failed to import distribution: {error}");
        }

        if (verbose && result.HasOutput())
        {
            foreach (var line in result.Output)
            {
                Console.WriteLine($"    {line}");
            }
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

        await ExecuteInDistroAsync(distroName, detectCmd, verbose);
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

    private async Task ExecuteInDistroAsync(string distroName, string command, bool verbose)
    {
        var result = await ProcessHelper.ExecuteAsync(300, "wsl", "-d", distroName, "--", "sh", "-c", command);

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
