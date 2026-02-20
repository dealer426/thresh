using System.Text.RegularExpressions;
using Thresh.Models;
using Thresh.Utilities;

namespace Thresh.Services;

/// <summary>
/// Service for managing WSL distributions and thresh environments
/// </summary>
public partial class WslService : IContainerService
{
    private const string ThreshPrefix = "thresh-";

    [GeneratedRegex(@"\s*(\*?)\s*(.+?)\s+(Running|Stopped|Installing|Terminated)", RegexOptions.IgnoreCase)]
    private static partial Regex WslListPattern();

    /// <summary>
    /// Runtime name for this service
    /// </summary>
    public string RuntimeName => "WSL";

    /// <summary>
    /// Platform this runtime operates on
    /// </summary>
    public string Platform => "Windows";

    /// <summary>
    /// Check if the runtime is available on the system
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        return await ProcessHelper.IsCommandAvailableAsync("wsl");
    }

    /// <summary>
    /// Check if WSL is available on the system (legacy)
    /// </summary>
    public async Task<bool> IsWslAvailableAsync() => await IsAvailableAsync();

    /// <summary>
    /// Get runtime version and information
    /// </summary>
    public async Task<RuntimeInfo> GetRuntimeInfoAsync()
    {
        if (!await IsAvailableAsync())
        {
            return RuntimeInfo.Unavailable("WSL not available");
        }

        try
        {
            // Try to get detailed version info - try both wsl.exe and wsl
            var versionResult = await ProcessHelper.ExecuteAsync("wsl.exe", "--version");
            if (!versionResult.IsSuccess || !versionResult.HasOutput())
            {
                versionResult = await ProcessHelper.ExecuteAsync("wsl", "--version");
            }

            if (versionResult.IsSuccess && versionResult.HasOutput())
            {
                var output = versionResult.GetOutputAsString();

                var wslVersion = ParseVersionLine(output, "WSL version:");
                var kernelVersion = ParseVersionLine(output, "Kernel version:");

                if (wslVersion != null)
                {
                    var details = kernelVersion != null ? $"Kernel: {kernelVersion}" : null;
                    return RuntimeInfo.Available(wslVersion, await GetDistributionCountAsync(), details, output);
                }

                // If parsing failed, check if output contains version info differently
                if (output.Contains("wsl version", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("kernel version", StringComparison.OrdinalIgnoreCase))
                {
                    var lines = output.Split('\n');
                    if (lines.Length > 0)
                    {
                        return RuntimeInfo.Available(lines[0].Trim(), await GetDistributionCountAsync(), null, output);
                    }
                }
            }

            // Fallback: Try to get WSL status
            var statusResult = await ProcessHelper.ExecuteAsync("wsl", "--status");
            if (statusResult.IsSuccess && statusResult.HasOutput())
            {
                var output = statusResult.GetOutputAsString();
                if (output.Contains("WSL 2"))
                {
                    return RuntimeInfo.Available("WSL 2", await GetDistributionCountAsync());
                }
                else if (output.Contains("WSL 1"))
                {
                    return RuntimeInfo.Available("WSL 1", await GetDistributionCountAsync());
                }
            }

            // Fallback: just check if we can list distributions
            var listResult = await ProcessHelper.ExecuteAsync("wsl", "--list", "--quiet");
            if (listResult.IsSuccess)
            {
                return RuntimeInfo.Available("WSL (version unknown)", await GetDistributionCountAsync());
            }

            return RuntimeInfo.Unavailable("WSL not functional");
        }
        catch (Exception ex)
        {
            return RuntimeInfo.Unavailable($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get WSL version information (legacy, preserved for compatibility)
    /// </summary>
    public async Task<WslInfo> GetWslInfoAsync()
    {
        var runtimeInfo = await GetRuntimeInfoAsync();
        return new WslInfo(
            runtimeInfo.IsAvailable,
            runtimeInfo.Version,
            runtimeInfo.Details,
            runtimeInfo.RawOutput,
            runtimeInfo.ContainerCount);
    }

    /// <summary>
    /// Parse a version line from wsl --version output
    /// </summary>
    private static string? ParseVersionLine(string output, string prefix)
    {
        var lines = output.Split('\n');

        foreach (var line in lines)
        {
            // Remove any non-printable characters and BOM
            var cleaned = new string(line.Where(c => !char.IsControl(c) || c == '\n').ToArray()).Trim();
            if (cleaned.Contains(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var prefixIndex = cleaned.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
                var value = cleaned[(prefixIndex + prefix.Length)..].Trim();
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
        }
        return null;
    }

    /// <summary>
    /// Get count of all WSL distributions
    /// </summary>
    private async Task<int> GetDistributionCountAsync()
    {
        try
        {
            var result = await ProcessHelper.ExecuteAsync("wsl", "--list", "--quiet");
            if (result.IsSuccess)
            {
                return result.Output.Count(line => !string.IsNullOrWhiteSpace(line));
            }
        }
        catch
        {
            // Ignore
        }
        return 0;
    }

    /// <summary>
    /// List all thresh environments
    /// </summary>
    public async Task<List<Models.Environment>> ListEnvironmentsAsync()
    {
        return await ListEnvironmentsAsync(false);
    }

    /// <summary>
    /// List environments with option to include all WSL distributions
    /// </summary>
    public async Task<List<Models.Environment>> ListEnvironmentsAsync(bool includeAll)
    {
        var environments = new List<Models.Environment>();

        try
        {
            var result = await ProcessHelper.ExecuteAsync("wsl", "--list", "--verbose");
            if (!result.IsSuccess)
            {
                return environments; // Return empty list if WSL command fails
            }

            foreach (var line in result.Output)
            {
                // Clean the line first to handle UTF-16 encoding from WSL
                var cleanLine = new string(line.Where(c => !char.IsControl(c) || c == '\n').ToArray()).Trim();

                // Skip empty lines and headers after cleaning
                if (string.IsNullOrWhiteSpace(cleanLine) ||
                    cleanLine.Contains("NAME", StringComparison.OrdinalIgnoreCase) ||
                    cleanLine.Contains("STATE", StringComparison.OrdinalIgnoreCase) ||
                    cleanLine.Contains("----"))
                {
                    continue;
                }

                var env = ParseWslDistributionLine(line, includeAll);
                if (env != null)
                {
                    // Include if: it's a thresh environment OR we're including all
                    if (includeAll || env.WslDistributionName.StartsWith(ThreshPrefix))
                    {
                        environments.Add(env);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error listing WSL distributions: {ex.Message}");
        }

        return environments;
    }

    /// <summary>
    /// Parse a line from 'wsl --list --verbose' output
    /// </summary>
    private Models.Environment? ParseWslDistributionLine(string line, bool includeAll)
    {
        try
        {
            // Remove special characters and normalize whitespace
            var cleanLine = new string(line.Where(c => !char.IsControl(c) || c == '\n').ToArray()).Trim();

            // Split on whitespace, but handle names with spaces
            var parts = cleanLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return null;

            // Check if first part is default marker (*)
            var nameIndex = parts[0] == "*" ? 1 : 0;

            var distributionName = parts[nameIndex];
            var statusStr = parts[nameIndex + 1];
            var version = parts.Length > nameIndex + 2 ? parts[nameIndex + 2] : "Unknown";

            var status = EnvironmentStatusExtensions.FromWslState(statusStr);

            // Extract environment name from distribution name
            var envName = distributionName;
            var blueprint = "system"; // Default for non-thresh distributions

            if (distributionName.StartsWith(ThreshPrefix))
            {
                envName = distributionName[ThreshPrefix.Length..];
                blueprint = BlueprintService.LoadBlueprintName(envName) ?? "unknown";
            }

            var env = new Models.Environment
            {
                Name = envName,
                WslDistributionName = distributionName,
                Status = status,
                Version = version,
                Created = null, // TODO: Get from metadata file for thresh envs
                Blueprint = blueprint
            };

            return env;
        }
        catch
        {
            return null; // Skip malformed lines
        }
    }

    /// <summary>
    /// Find an environment by name
    /// </summary>
    public async Task<Models.Environment?> FindEnvironmentAsync(string name)
    {
        var environments = await ListEnvironmentsAsync();
        return environments.FirstOrDefault(env => env.Name == name);
    }

    /// <summary>
    /// Start a WSL distribution
    /// </summary>
    public async Task<bool> StartEnvironmentAsync(string environmentName)
    {
        var distributionName = ThreshPrefix + environmentName;
        var result = await ProcessHelper.ExecuteAsync("wsl", "-d", distributionName, "echo", "started");
        
        if (!result.IsSuccess)
            return false;
        
        // Apply port forwarding if configured
        var metadata = LoadMetadata(environmentName);
        if (metadata?.Ports != null && metadata.Ports.Count > 0)
        {
            await ApplyPortForwardingAsync(environmentName, metadata.Ports);
        }
        
        return true;
    }

    /// <summary>
    /// Stop a WSL distribution
    /// </summary>
    public async Task<bool> StopEnvironmentAsync(string environmentName)
    {
        // Remove port forwarding before stopping
        var metadata = LoadMetadata(environmentName);
        if (metadata?.Ports != null && metadata.Ports.Count > 0)
        {
            await RemovePortForwardingAsync(metadata.Ports);
        }
        
        var distributionName = ThreshPrefix + environmentName;
        var result = await ProcessHelper.ExecuteAsync("wsl", "--terminate", distributionName);
        return result.IsSuccess;
    }

    /// <summary>
    /// Remove a WSL distribution
    /// </summary>
    public async Task<bool> RemoveEnvironmentAsync(string environmentName)
    {
        var distributionName = ThreshPrefix + environmentName;
        var result = await ProcessHelper.ExecuteAsync("wsl", "--unregister", distributionName);
        return result.IsSuccess;
    }

    /// <summary>
    /// Import a new WSL distribution from a tar file
    /// </summary>
    public async Task<bool> ImportEnvironmentAsync(string environmentName, string tarPath, string installPath, string? blueprintName = null, Blueprint? blueprint = null)
    {
        var distributionName = ThreshPrefix + environmentName;
        var result = await ProcessHelper.ExecuteAsync("wsl", "--import", distributionName, installPath, tarPath);
        
        // Note: WSL networking is handled at Start/Stop time via netsh port proxy
        // Blueprint metadata is stored separately via BlueprintService.SaveMetadata()
        
        return result.IsSuccess;
    }

    /// <summary>
    /// Execute a command in a WSL distribution
    /// </summary>
    public async Task<ProcessHelper.ProcessResult> ExecuteCommandAsync(string environmentName, string command, int timeoutSeconds = 30)
    {
        var distributionName = ThreshPrefix + environmentName;
        return await ProcessHelper.ExecuteAsync(timeoutSeconds, "wsl", "-d", distributionName, "sh", "-c", command);
    }

    /// <summary>
    /// Check if an environment exists
    /// </summary>
    public async Task<bool> EnvironmentExistsAsync(string environmentName)
    {
        var env = await FindEnvironmentAsync(environmentName);
        return env != null;
    }

    /// <summary>
    /// Get the IP address of a WSL distribution
    /// </summary>
    private async Task<string?> GetWslIpAddressAsync(string environmentName)
    {
        var distributionName = ThreshPrefix + environmentName;
        var result = await ProcessHelper.ExecuteAsync("wsl", "-d", distributionName, "hostname", "-I");
        
        if (!result.IsSuccess || !result.HasOutput())
            return null;
        
        var output = result.GetOutputAsString();
        if (string.IsNullOrWhiteSpace(output))
            return null;
        
        // hostname -I can return multiple IPs, take the first one
        var ips = output.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return ips.Length > 0 ? ips[0] : null;
    }

    /// <summary>
    /// Apply port forwarding for an environment using netsh
    /// </summary>
    public async Task<bool> ApplyPortForwardingAsync(string environmentName, List<string> ports)
    {
        if (ports == null || ports.Count == 0)
            return true;

        // Get WSL IP address
        var wslIp = await GetWslIpAddressAsync(environmentName);
        if (string.IsNullOrEmpty(wslIp))
        {
            Console.WriteLine($"[WARN] Failed to get WSL IP for {environmentName}, skipping port forwarding");
            return false;
        }

        Console.WriteLine($"[INFO] Setting up port forwarding to WSL IP: {wslIp}");

        foreach (var portMapping in ports)
        {
            // Parse port mapping (e.g., "8080:80" or "8080")
            var parts = portMapping.Split(':');
            var hostPort = parts[0].Trim();
            var containerPort = parts.Length > 1 ? parts[1].Trim() : hostPort;

            try
            {
                // Add netsh port proxy rule
                // netsh interface portproxy add v4tov4 listenport=8080 listenaddress=0.0.0.0 connectport=80 connectaddress=<WSL_IP>
                var result = await ProcessHelper.ExecuteAsync(
                    "netsh", "interface", "portproxy", "add", "v4tov4",
                    $"listenport={hostPort}",
                    "listenaddress=0.0.0.0",
                    $"connectport={containerPort}",
                    $"connectaddress={wslIp}"
                );

                if (result.IsSuccess)
                {
                    Console.WriteLine($"[OK] Port forwarding: localhost:{hostPort} -> {wslIp}:{containerPort}");
                }
                else
                {
                    Console.WriteLine($"[WARN] Failed to set up port forwarding for {hostPort}:{containerPort}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception setting up port {portMapping}: {ex.Message}");
            }
        }

        return true;
    }

    /// <summary>
    /// Remove port forwarding for an environment
    /// </summary>
    public async Task<bool> RemovePortForwardingAsync(List<string> ports)
    {
        if (ports == null || ports.Count == 0)
            return true;

        foreach (var portMapping in ports)
        {
            // Parse port mapping to get host port
            var parts = portMapping.Split(':');
            var hostPort = parts[0].Trim();

            try
            {
                // Remove netsh port proxy rule
                // netsh interface portproxy delete v4tov4 listenport=8080 listenaddress=0.0.0.0
                var result = await ProcessHelper.ExecuteAsync(
                    "netsh", "interface", "portproxy", "delete", "v4tov4",
                    $"listenport={hostPort}",
                    "listenaddress=0.0.0.0"
                );

                if (result.IsSuccess)
                {
                    Console.WriteLine($"[OK] Removed port forwarding for port {hostPort}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Failed to remove port forwarding for {hostPort}: {ex.Message}");
            }
        }

        return true;
    }

    /// <summary>
    /// Load environment metadata from disk
    /// </summary>
    private EnvironmentMetadata? LoadMetadata(string environmentName)
    {
        try
        {
            var homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            var metadataFile = Path.Combine(homeDir, ".thresh", "metadata", $"{environmentName}.json");
            
            if (!File.Exists(metadataFile))
                return null;
            
            var json = File.ReadAllText(metadataFile);
            return System.Text.Json.JsonSerializer.Deserialize(json, BlueprintJsonContext.Default.EnvironmentMetadata);
        }
        catch
        {
            return null;
        }
    }
}
