using System.Runtime.InteropServices;
using System.Text.Json;
using Thresh.Models;
using Thresh.Utilities;

namespace Thresh.Services;

/// <summary>
/// Service for Docker-compatible container runtimes on Linux and macOS
/// Supports: nerdctl (containerd-native) and docker (Docker Engine)
/// Both tools share the same CLI interface for consistency
/// </summary>
public class ContainerdService : IContainerService
{
    private const string ThreshPrefix = "thresh-";
    private string? _detectedTool;

    public ContainerdService()
    {
        // Tool will be auto-detected: nerdctl → docker
    }

    /// <summary>
    /// Runtime name for this service
    /// </summary>
    public string RuntimeName
    {
        get
        {
            if (_detectedTool == "docker") return "docker";
            if (_detectedTool == "nerdctl") return "nerdctl";
            return "container-runtime";
        }
    }

    /// <summary>
    /// Platform this runtime operates on
    /// </summary>
    public string Platform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macOS";
            return "Unknown";
        }
    }

    /// <summary>
    /// Check if the runtime is available on the system
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        // Try nerdctl first (containerd-native, Docker-compatible)
        if (await ProcessHelper.IsCommandAvailableAsync("nerdctl"))
        {
            _detectedTool = "nerdctl";
            return true;
        }

        // Try Docker (Docker Engine, common everywhere)
        if (await ProcessHelper.IsCommandAvailableAsync("docker"))
        {
            _detectedTool = "docker";
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get runtime version and information
    /// </summary>
    public async Task<RuntimeInfo> GetRuntimeInfoAsync()
    {
        if (!await IsAvailableAsync())
        {
            return RuntimeInfo.Unavailable("container runtime not available");
        }

        try
        {
            var tool = await GetAvailableToolAsync();

            // Both nerdctl and docker support the same version API
            var versionResult = await ProcessHelper.ExecuteAsync(tool, "version", "--format", "json");
            if (versionResult.IsSuccess && versionResult.HasOutput())
            {
                try
                {
                    var output = versionResult.GetOutputAsString();
                    var versionInfo = JsonSerializer.Deserialize(output, ContainerdJsonContext.Default.NerdctlVersion);
                    if (versionInfo?.Server?.Version != null)
                    {
                        var details = $"{tool} {versionInfo.Client?.Version}";
                        return RuntimeInfo.Available(
                            versionInfo.Server.Version,
                            await GetContainerCountAsync(),
                            details,
                            output);
                    }
                }
                catch
                {
                    // JSON parsing failed, try text output
                }
            }

            // Fallback: Try text version output
            versionResult = await ProcessHelper.ExecuteAsync(tool, "version");
            if (versionResult.IsSuccess && versionResult.HasOutput())
            {
                var output = versionResult.GetOutputAsString();
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("Version:", StringComparison.OrdinalIgnoreCase))
                    {
                        var version = line.Split(':')[1].Trim();
                        return RuntimeInfo.Available(version, await GetContainerCountAsync(), tool, output);
                    }
                }
            }

            return RuntimeInfo.Available("unknown", await GetContainerCountAsync());
        }
        catch (Exception ex)
        {
            return RuntimeInfo.Unavailable($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// List all environments managed by this runtime
    /// </summary>
    public async Task<List<Models.Environment>> ListEnvironmentsAsync()
    {
        return await ListEnvironmentsAsync(false);
    }

    /// <summary>
    /// List environments with option to include all containers
    /// </summary>
    public async Task<List<Models.Environment>> ListEnvironmentsAsync(bool includeAll)
    {
        var environments = new List<Models.Environment>();
        var tool = await GetAvailableToolAsync();

        try
        {
            // List containers using docker/nerdctl ps (both support --format json)
            var result = await ProcessHelper.ExecuteAsync(tool, "ps", "-a", "--format", "json");

            if (!result.IsSuccess || !result.HasOutput())
            {
                return environments;
            }

                var output = result.GetOutputAsString();
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    try
                    {
                        var container = JsonSerializer.Deserialize(line, ContainerdJsonContext.Default.NerdctlContainer);
                        if (container == null) continue;

                        // Filter thresh-managed containers unless includeAll
                        if (!includeAll && !container.Names.StartsWith(ThreshPrefix))
                            continue;

                        var envName = container.Names.StartsWith(ThreshPrefix) 
                            ? container.Names[ThreshPrefix.Length..] 
                            : container.Names;

                        // Parse blueprint from labels
                        // Docker format: "key=value,key2=value2" (comma-separated string)
                        // nerdctl format: {"key":"value","key2":"value2"} (JSON object)
                        string? blueprint = null;
                        var labelsString = container.GetLabelsAsString();
                        if (!string.IsNullOrEmpty(labelsString))
                        {
                            if (labelsString.StartsWith("{"))
                            {
                                // nerdctl JSON format
                                try
                                {
                                    using var doc = JsonDocument.Parse(labelsString);
                                    if (doc.RootElement.TryGetProperty("thresh.blueprint", out var blueprintProp))
                                    {
                                        blueprint = blueprintProp.GetString();
                                    }
                                }
                                catch
                                {
                                    // Ignore JSON parsing errors
                                }
                            }
                            else
                            {
                                // Docker comma-separated format
                                var labels = labelsString.Split(',');
                                var blueprintLabel = labels.FirstOrDefault(l => l.StartsWith("thresh.blueprint="));
                                if (blueprintLabel != null)
                                {
                                    blueprint = blueprintLabel.Substring("thresh.blueprint=".Length);
                                }
                            }
                        }

                        // nerdctl uses "Status" field, docker uses "State" field
                        var state = string.IsNullOrEmpty(container.State) ? container.Status : container.State;
                        
                        environments.Add(new Models.Environment
                        {
                            Name = envName,
                            WslDistributionName = container.Names,
                            Status = MapContainerState(state),
                            Version = tool,
                            Blueprint = blueprint ?? "unknown"
                        });
                    }
                    catch
                    {
                        // Skip malformed JSON lines
                    }
                }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error listing containers: {ex.Message}");
        }

        return environments;
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
    /// Start an environment
    /// </summary>
    public async Task<bool> StartEnvironmentAsync(string environmentName)
    {
        var containerName = ThreshPrefix + environmentName;
        var tool = await GetAvailableToolAsync();
        var result = await ProcessHelper.ExecuteAsync(tool, "start", containerName);
        return result.IsSuccess;
    }

    /// <summary>
    /// Stop a running environment
    /// </summary>
    public async Task<bool> StopEnvironmentAsync(string environmentName)
    {
        var containerName = ThreshPrefix + environmentName;
        var tool = await GetAvailableToolAsync();
        var result = await ProcessHelper.ExecuteAsync(tool, "stop", containerName);
        return result.IsSuccess;
    }

    /// <summary>
    /// Remove an environment permanently
    /// </summary>
    public async Task<bool> RemoveEnvironmentAsync(string environmentName)
    {
        var containerName = ThreshPrefix + environmentName;
        var tool = await GetAvailableToolAsync();
        
        // Stop first, then remove
        await StopEnvironmentAsync(environmentName);
        var removeResult = await ProcessHelper.ExecuteAsync(tool, "rm", containerName);
        return removeResult.IsSuccess;
    }

    /// <summary>
    /// Import/create a new environment from an image or rootfs tarball
    /// </summary>
    public async Task<bool> ImportEnvironmentAsync(string environmentName, string sourcePath, string installPath, string? blueprintName = null)
    {
        var containerName = ThreshPrefix + environmentName;
        var tool = await GetAvailableToolAsync();
        
        // sourcePath can be:
        // 1. Docker image name (e.g., "ubuntu:22.04")
        // 2. Rootfs tar/tar.gz file (e.g., "/path/to/rootfs.tar.gz")
        
        ProcessHelper.ProcessResult result;
        List<string> createArgs = new();
        
        if (File.Exists(sourcePath))
        {
            // Import rootfs tarball as Docker image using 'docker import'
            // This creates an image from a filesystem tarball (not a Docker image tar)
            var imageName = $"thresh/{environmentName}:latest";
            
            result = await ProcessHelper.ExecuteAsync(300, tool, "import", sourcePath, imageName);
            if (!result.IsSuccess) return false;
            
            // Create a container from the imported image with a shell command
            // Rootfs images don't have a default CMD, so we need to provide one
            createArgs.Add(tool);
            createArgs.AddRange(new[] { "create", "--name", containerName, "-it" });
            
            // Add blueprint label if provided
            if (!string.IsNullOrEmpty(blueprintName))
            {
                createArgs.AddRange(new[] { "--label", $"thresh.blueprint={blueprintName}" });
            }
            
            createArgs.AddRange(new[] { imageName, "/bin/sh" });
            result = await ProcessHelper.ExecuteAsync(createArgs.ToArray());
        }
        else
        {
            // Assume it's a Docker image name (e.g., "ubuntu:22.04")
            createArgs.Add(tool);
            createArgs.AddRange(new[] { "create", "--name", containerName, "-it" });
            
            // Add blueprint label if provided
            if (!string.IsNullOrEmpty(blueprintName))
            {
                createArgs.AddRange(new[] { "--label", $"thresh.blueprint={blueprintName}" });
            }
            
            // Add image name and shell command
            // Use /bin/sh for compatibility (works on Alpine, Ubuntu, Debian, etc.)
            createArgs.AddRange(new[] { sourcePath, "/bin/sh" });
            result = await ProcessHelper.ExecuteAsync(createArgs.ToArray());
        }
        
        return result.IsSuccess;
    }

    /// <summary>
    /// Execute a command in an environment
    /// </summary>
    public async Task<ProcessHelper.ProcessResult> ExecuteCommandAsync(string environmentName, string command, int timeoutSeconds = 30)
    {
        var containerName = ThreshPrefix + environmentName;
        var tool = await GetAvailableToolAsync();
        
        // Check if container is running, start if not
        var inspectResult = await ProcessHelper.ExecuteAsync(tool, "inspect", "-f", "{{.State.Running}}", containerName);
        if (inspectResult.IsSuccess && inspectResult.GetOutputAsString().Trim().ToLower() != "true")
        {
            // Container not running, start it
            await ProcessHelper.ExecuteAsync(tool, "start", containerName);
        }
        
        return await ProcessHelper.ExecuteAsync(timeoutSeconds, tool, "exec", containerName, "sh", "-c", command);
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
    /// Get the available container tool (nerdctl or docker)
    /// </summary>
    private async Task<string> GetAvailableToolAsync()
    {
        if (_detectedTool != null)
            return _detectedTool;

        // Trigger detection
        await IsAvailableAsync();
        return _detectedTool ?? "nerdctl";
    }

    /// <summary>
    /// Get count of all containers
    /// </summary>
    private async Task<int> GetContainerCountAsync()
    {
        try
        {
            var tool = await GetAvailableToolAsync();
            var result = await ProcessHelper.ExecuteAsync(tool, "ps", "-a", "-q");
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
    /// Map containerd container state to EnvironmentStatus
    /// Handles both Docker ("running") and nerdctl ("Up") status formats
    /// </summary>
    private static EnvironmentStatus MapContainerState(string state)
    {
        return state.ToLowerInvariant() switch
        {
            "running" => EnvironmentStatus.Running,
            "up" => EnvironmentStatus.Running,  // nerdctl format
            "created" => EnvironmentStatus.Stopped,
            "exited" => EnvironmentStatus.Stopped,
            "paused" => EnvironmentStatus.Stopped,
            _ => EnvironmentStatus.Unknown
        };
    }
}
