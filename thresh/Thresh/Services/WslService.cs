using System.Text.RegularExpressions;
using Thresh.Models;
using Thresh.Utilities;

namespace Thresh.Services;

/// <summary>
/// Service for managing WSL distributions and thresh environments
/// </summary>
public partial class WslService
{
    private const string ThreshPrefix = "thresh-";

    [GeneratedRegex(@"\s*(\*?)\s*(.+?)\s+(Running|Stopped|Installing|Terminated)", RegexOptions.IgnoreCase)]
    private static partial Regex WslListPattern();

    /// <summary>
    /// Check if WSL is available on the system
    /// </summary>
    public async Task<bool> IsWslAvailableAsync()
    {
        return await ProcessHelper.IsCommandAvailableAsync("wsl");
    }

    /// <summary>
    /// Get WSL version information
    /// </summary>
    public async Task<WslInfo> GetWslInfoAsync()
    {
        if (!await IsWslAvailableAsync())
        {
            return new WslInfo(false, "Not available");
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
                    return new WslInfo(true, wslVersion, kernelVersion, output, await GetDistributionCountAsync());
                }

                // If parsing failed, check if output contains version info differently
                if (output.Contains("wsl version", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("kernel version", StringComparison.OrdinalIgnoreCase))
                {
                    var lines = output.Split('\n');
                    if (lines.Length > 0)
                    {
                        return new WslInfo(true, lines[0].Trim(), null, output, await GetDistributionCountAsync());
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
                    return new WslInfo(true, "WSL 2", null, null, await GetDistributionCountAsync());
                }
                else if (output.Contains("WSL 1"))
                {
                    return new WslInfo(true, "WSL 1", null, null, await GetDistributionCountAsync());
                }
            }

            // Fallback: just check if we can list distributions
            var listResult = await ProcessHelper.ExecuteAsync("wsl", "--list", "--quiet");
            if (listResult.IsSuccess)
            {
                return new WslInfo(true, "WSL (version unknown)", null, null, await GetDistributionCountAsync());
            }

            return new WslInfo(false, "Not functional");
        }
        catch (Exception ex)
        {
            return new WslInfo(false, $"Error: {ex.Message}");
        }
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
        return result.IsSuccess;
    }

    /// <summary>
    /// Stop a WSL distribution
    /// </summary>
    public async Task<bool> StopEnvironmentAsync(string environmentName)
    {
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
    public async Task<bool> ImportEnvironmentAsync(string environmentName, string tarPath, string installPath)
    {
        var distributionName = ThreshPrefix + environmentName;
        var result = await ProcessHelper.ExecuteAsync("wsl", "--import", distributionName, installPath, tarPath);
        return result.IsSuccess;
    }

    /// <summary>
    /// Execute a command in a WSL distribution
    /// </summary>
    public async Task<ProcessHelper.ProcessResult> ExecuteCommandAsync(string environmentName, string command)
    {
        var distributionName = ThreshPrefix + environmentName;
        return await ProcessHelper.ExecuteAsync("wsl", "-d", distributionName, "sh", "-c", command);
    }

    /// <summary>
    /// Check if an environment exists
    /// </summary>
    public async Task<bool> EnvironmentExistsAsync(string environmentName)
    {
        var env = await FindEnvironmentAsync(environmentName);
        return env != null;
    }
}
