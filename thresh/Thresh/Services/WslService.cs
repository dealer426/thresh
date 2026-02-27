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
    private static readonly string VolumeDirectory = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
        ".thresh",
        "volumes"
    );

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
        
        // WSL2 automatically forwards ports to Windows localhost - no netsh needed!
        return result.IsSuccess;
    }

    /// <summary>
    /// Stop a WSL distribution
    /// </summary>
    public async Task<bool> StopEnvironmentAsync(string environmentName)
    {
        // WSL2 automatically forwards ports - nothing to clean up
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
    /// List all directory-based volumes
    /// </summary>
    public Task<List<VolumeInfo>> ListVolumesAsync()
    {
        var volumes = new List<VolumeInfo>();
        
        try
        {
            if (!Directory.Exists(VolumeDirectory))
            {
                return Task.FromResult(volumes);
            }

            var volumeDirs = Directory.GetDirectories(VolumeDirectory);
            
            foreach (var volumePath in volumeDirs)
            {
                var volumeName = Path.GetFileName(volumePath);
                var dirInfo = new DirectoryInfo(volumePath);
                
                // Calculate directory size
                long size = 0;
                try
                {
                    var files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                    size = files.Sum(f => f.Length);
                }
                catch
                {
                    // Size calculation failed, continue with 0
                }
                
                volumes.Add(new VolumeInfo
                {
                    Name = volumeName,
                    Driver = "local",
                    Mountpoint = volumePath,
                    CreatedAt = dirInfo.CreationTime,
                    Size = size
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to list volumes: {ex.Message}");
        }
        
        return Task.FromResult(volumes);
    }

    /// <summary>
    /// Create a directory-based volume (no admin required)
    /// </summary>
    public Task<bool> CreateVolumeAsync(string volumeName)
    {
        try
        {
            // Ensure volume directory exists
            if (!Directory.Exists(VolumeDirectory))
            {
                Directory.CreateDirectory(VolumeDirectory);
            }

            var volumePath = Path.Combine(VolumeDirectory, volumeName);
            
            // Check if volume already exists
            if (Directory.Exists(volumePath))
            {
                Console.WriteLine($"Volume '{volumeName}' already exists");
                return Task.FromResult(true);
            }

            Console.WriteLine($"Creating volume '{volumeName}'...");
            
            // Create volume directory
            Directory.CreateDirectory(volumePath);

            Console.WriteLine($"✅ Volume '{volumeName}' created successfully");
            Console.WriteLine($"   Location: {volumePath}");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to create volume: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Delete a directory-based volume
    /// </summary>
    public Task<bool> DeleteVolumeAsync(string volumeName)
    {
        try
        {
            var volumePath = Path.Combine(VolumeDirectory, volumeName);
            
            if (!Directory.Exists(volumePath))
            {
                Console.WriteLine($"Volume '{volumeName}' not found");
                return Task.FromResult(false);
            }

            Console.WriteLine($"Deleting volume '{volumeName}'...");
            Directory.Delete(volumePath, recursive: true);
            
            Console.WriteLine($"✅ Volume '{volumeName}' deleted successfully");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to delete volume: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Inspect a directory-based volume
    /// </summary>
    public Task<VolumeInfo?> InspectVolumeAsync(string volumeName)
    {
        try
        {
            var volumePath = Path.Combine(VolumeDirectory, volumeName);
            
            if (!Directory.Exists(volumePath))
            {
                return Task.FromResult<VolumeInfo?>(null);
            }

            var dirInfo = new DirectoryInfo(volumePath);
            
            // Calculate directory size
            long size = 0;
            try
            {
                var files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                size = files.Sum(f => f.Length);
            }
            catch
            {
                // Size calculation failed, continue with 0
            }
            
            var volume = new VolumeInfo
            {
                Name = volumeName,
                Driver = "local",
                Mountpoint = volumePath,
                CreatedAt = dirInfo.CreationTime,
                Size = size
            };
            
            return Task.FromResult<VolumeInfo?>(volume);
        }
        catch
        {
            return Task.FromResult<VolumeInfo?>(null);
        }
    }

    /// <summary>
    /// Mount a Windows directory volume to a WSL distro using bind mount (no admin required)
    /// </summary>
    public async Task<bool> MountVolumeToDistroAsync(string volumeName, string distroName, string mountPoint)
    {
        try
        {
            var volumePath = Path.Combine(VolumeDirectory, volumeName);
            
            if (!Directory.Exists(volumePath))
            {
                Console.WriteLine($"Volume '{volumeName}' not found");
                return false;
            }

            Console.WriteLine($"  Mounting volume '{volumeName}'...");
            
            // Convert Windows path to WSL path format
            // C:\Users\burns\.thresh\volumes\data -> /mnt/c/Users/burns/.thresh/volumes/data
            var wslPath = ConvertWindowsPathToWsl(volumePath);
            
            // Create bind mount inside the distro (use Unix line endings)
            var bindMountScript = $"mkdir -p {mountPoint} && mount --bind {wslPath} {mountPoint} && if mountpoint -q {mountPoint}; then echo 'Volume mounted successfully'; else echo 'Mount verification failed'; exit 1; fi";
            
            var bindResult = await ProcessHelper.ExecuteAsync(
                "wsl",
                "-d", distroName,
                "sh", "-c", bindMountScript
            );

            if (!bindResult.IsSuccess)
            {
                Console.WriteLine($"  ⚠️  Failed to create bind mount: {bindResult.Error}");
                return false;
            }

            // Create persistent mount script in /etc/profile.d/
            // This ensures volumes are remounted when the distro starts
            await CreatePersistentMountScriptAsync(distroName, volumeName, wslPath, mountPoint);

            Console.WriteLine($"  ✅ Volume '{volumeName}' mounted to {mountPoint}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  Failed to mount volume: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Create a startup script that remounts volumes on distro boot/login
    /// </summary>
    private async Task CreatePersistentMountScriptAsync(string distroName, string volumeName, string wslPath, string mountPoint)
    {
        try
        {
            var scriptPath = "/etc/profile.d/thresh-volumes.sh";
            
            // Create mount command that's idempotent (safe to run multiple times)
            var mountCommand = $"[ ! -d '{mountPoint}' ] && mkdir -p '{mountPoint}'; ! mountpoint -q '{mountPoint}' 2>/dev/null && mount --bind '{wslPath}' '{mountPoint}' 2>/dev/null || true";
            
            // Check if script exists
            var checkScript = $"test -f {scriptPath} && echo exists || echo new";
            var checkResult = await ProcessHelper.ExecuteAsync("wsl", "-d", distroName, "sh", "-c", checkScript);
            var isNew = checkResult.GetOutputAsString().Trim() == "new";
            
            if (isNew)
            {
                // Create new script with header using printf (ensures Unix line endings)
                var createScript = $@"printf '#!/bin/sh\n# thresh - Persistent volume mounts\n# Auto-generated - do not edit manually\n\n# Mount: {volumeName} -> {mountPoint}\n{mountCommand}\n' > {scriptPath} && chmod +x {scriptPath}";
                
                await ProcessHelper.ExecuteAsync("wsl", "-d", distroName, "sh", "-c", createScript);
            }
            else
            {
                // Check if this mount is already in the script
                var grepScript = $"grep -q '{mountPoint}' {scriptPath}";
                var grepResult = await ProcessHelper.ExecuteAsync("wsl", "-d", distroName, "sh", "-c", grepScript);
                
                if (!grepResult.IsSuccess) // grep returns non-zero when pattern not found
                {
                    // Append new mount to existing script using printf
                    var appendScript = $"printf '\n# Mount: {volumeName} -> {mountPoint}\n{mountCommand}\n' >> {scriptPath}";
                    await ProcessHelper.ExecuteAsync("wsl", "-d", distroName, "sh", "-c", appendScript);
                }
            }
        }
        catch (Exception ex)
        {
            // Non-fatal - mount still works for current session
            Console.WriteLine($"  Warning: Could not create persistent mount script: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Convert Windows path to WSL path format
    /// </summary>
    private static string ConvertWindowsPathToWsl(string windowsPath)
    {
        // Normalize path separators
        var normalized = windowsPath.Replace("\\", "/");
        
        // Convert drive letter (C: -> /mnt/c)
        if (normalized.Length >= 2 && normalized[1] == ':')
        {
            var driveLetter = char.ToLower(normalized[0]);
            normalized = $"/mnt/{driveLetter}{normalized.Substring(2)}";
        }
        
        return normalized;
    }

    /// <summary>
    /// Initialize VHD with ext4 filesystem
    /// </summary>
}
