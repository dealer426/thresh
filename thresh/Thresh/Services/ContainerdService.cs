using System.Runtime.InteropServices;
using System.Text.Json;
using Thresh.Models;
using Thresh.Utilities;

namespace Thresh.Services;

/// <summary>
/// Service for managing containerd/nerdctl containers on Linux and macOS
/// </summary>
public class ContainerdService : IContainerService
{
    private const string ThreshPrefix = "thresh-";
    private string? _detectedTool;

    public ContainerdService()
    {
        // Tool will be auto-detected: nerdctl → docker → ctr
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
            if (_detectedTool == "ctr") return "containerd";
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
        // Try nerdctl first (best for containerd)
        if (await ProcessHelper.IsCommandAvailableAsync("nerdctl"))
        {
            _detectedTool = "nerdctl";
            return true;
        }

        // Try Docker (common in Codespaces, Docker Desktop)
        if (await ProcessHelper.IsCommandAvailableAsync("docker"))
        {
            _detectedTool = "docker";
            return true;
        }

        // Fallback to ctr (containerd's native CLI)
        if (await ProcessHelper.IsCommandAvailableAsync("ctr"))
        {
            _detectedTool = "ctr";
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

            // Try nerdctl or docker (both support JSON format)
            if (tool == "nerdctl" || tool == "docker")
            {
                var nerdctlResult = await ProcessHelper.ExecuteAsync(tool, "version", "--format", "json");
                if (nerdctlResult.IsSuccess && nerdctlResult.HasOutput())
                {
                    try
                    {
                        var output = nerdctlResult.GetOutputAsString();
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
                nerdctlResult = await ProcessHelper.ExecuteAsync(tool, "version");
                if (nerdctlResult.IsSuccess && nerdctlResult.HasOutput())
                {
                    var output = nerdctlResult.GetOutputAsString();
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
            }

            // Try ctr as last resort
            if (tool == "ctr")
            {
                var ctrResult = await ProcessHelper.ExecuteAsync("ctr", "version");
                if (ctrResult.IsSuccess && ctrResult.HasOutput())
                {
                    var output = ctrResult.GetOutputAsString();
                    var lines = output.Split('\n');
                    if (lines.Length > 0)
                    {
                        return RuntimeInfo.Available(lines[0].Trim(), await GetContainerCountAsync(), "ctr", output);
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
            // List containers using nerdctl/docker ps (both support --format json)
            if (tool == "nerdctl" || tool == "docker")
            {
                var result = await ProcessHelper.ExecuteAsync(tool, "ps", "-a", "--format", "json");

                if (!result.IsSuccess || !result.HasOutput())
                {
                    // Fallback to ctr if nerdctl/docker fails
                    if (tool != "ctr")
                        return await ListEnvironmentsWithCtrAsync(includeAll);
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

                        // Parse blueprint from labels (Docker format: "key=value,key2=value2")
                        string? blueprint = null;
                        if (!string.IsNullOrEmpty(container.Labels))
                        {
                            var labels = container.Labels.Split(',');
                            var blueprintLabel = labels.FirstOrDefault(l => l.StartsWith("thresh.blueprint="));
                            if (blueprintLabel != null)
                            {
                                blueprint = blueprintLabel.Substring("thresh.blueprint=".Length);
                            }
                        }

                        environments.Add(new Models.Environment
                        {
                            Name = envName,
                            WslDistributionName = container.Names,
                            Status = MapContainerState(container.State),
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
            else if (tool == "ctr")
            {
                return await ListEnvironmentsWithCtrAsync(includeAll);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error listing containers: {ex.Message}");
        }

        return environments;
    }

    /// <summary>
    /// Fallback method to list containers using ctr
    /// </summary>
    private async Task<List<Models.Environment>> ListEnvironmentsWithCtrAsync(bool includeAll)
    {
        var environments = new List<Models.Environment>();
        
        try
        {
            var result = await ProcessHelper.ExecuteAsync("ctr", "containers", "list");
            if (!result.IsSuccess) return environments;

            foreach (var line in result.Output.Skip(1)) // Skip header
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                var containerName = parts[0];
                
                if (!includeAll && !containerName.StartsWith(ThreshPrefix))
                    continue;

                var envName = containerName.StartsWith(ThreshPrefix)
                    ? containerName[ThreshPrefix.Length..]
                    : containerName;

                environments.Add(new Models.Environment
                {
                    Name = envName,
                    WslDistributionName = containerName,
                    Status = EnvironmentStatus.Unknown,
                    Version = "containerd",
                    Blueprint = "unknown"
                });
            }
        }
        catch
        {
            // Ignore ctr errors
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
        
        if (tool == "ctr")
        {
            // ctr doesn't have a simple start command like docker/nerdctl
            return false;
        }
        
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
        
        if (tool == "ctr")
        {
            return false;
        }
        
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
        
        if (tool == "ctr")
        {
            var result = await ProcessHelper.ExecuteAsync("ctr", "containers", "delete", containerName);
            return result.IsSuccess;
        }
        
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
        
        if (tool == "ctr")
        {
            // ctr has different syntax - skip for now
            return false;
        }
        
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
            // Assume it's an image name, create container directly
            createArgs.Add(tool);
            createArgs.AddRange(new[] { "create", "--name", containerName });
            
            // Add blueprint label if provided
            if (!string.IsNullOrEmpty(blueprintName))
            {
                createArgs.AddRange(new[] { "--label", $"thresh.blueprint={blueprintName}" });
            }
            
            createArgs.Add(sourcePath);
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
        
        if (tool == "ctr")
        {
            // ctr exec syntax is different
            return await ProcessHelper.ExecuteAsync(timeoutSeconds, "ctr", "tasks", "exec", "--exec-id", Guid.NewGuid().ToString(), containerName, "sh", "-c", command);
        }
        
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
    /// Get count of all containers
    /// </summary>
    /// <summary>
    /// Get the available container tool (nerdctl, docker, or ctr)
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
            
            if (tool == "ctr")
            {
                var result = await ProcessHelper.ExecuteAsync("ctr", "containers", "list", "-q");
                if (result.IsSuccess)
                {
                    return result.Output.Count(line => !string.IsNullOrWhiteSpace(line));
                }
            }
            else
            {
                var result = await ProcessHelper.ExecuteAsync(tool, "ps", "-a", "-q");
                if (result.IsSuccess)
                {
                    return result.Output.Count(line => !string.IsNullOrWhiteSpace(line));
                }
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
    /// </summary>
    private static EnvironmentStatus MapContainerState(string state)
    {
        return state.ToLowerInvariant() switch
        {
            "running" => EnvironmentStatus.Running,
            "created" => EnvironmentStatus.Stopped,
            "exited" => EnvironmentStatus.Stopped,
            "paused" => EnvironmentStatus.Stopped,
            _ => EnvironmentStatus.Unknown
        };
    }
}
