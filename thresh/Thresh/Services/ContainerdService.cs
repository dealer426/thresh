using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        // Try Docker first (Docker Engine, widely available and configured)
        if (await ProcessHelper.IsCommandAvailableAsync("docker"))
        {
            _detectedTool = "docker";
            return true;
        }

        // Try nerdctl as fallback (containerd-native, Docker-compatible)
        if (await ProcessHelper.IsCommandAvailableAsync("nerdctl"))
        {
            _detectedTool = "nerdctl";
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
    public async Task<bool> ImportEnvironmentAsync(string environmentName, string sourcePath, string installPath, string? blueprintName = null, Blueprint? blueprint = null, bool serviceMode = false)
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
            
            // Add networking configuration from blueprint
            AddContainerArgs(createArgs, blueprint);
            
            createArgs.AddRange(new[] { imageName, "/bin/sh" });
            result = await ProcessHelper.ExecuteAsync(createArgs.ToArray());
        }
        else
        {
            // Assume it's a Docker image name (e.g., "ubuntu:22.04")
            createArgs.Add(tool);

            if (serviceMode)
            {
                // Service mode: use detached mode, use image's default entrypoint
                createArgs.AddRange(new[] { "run", "-d", "--name", containerName });
            }
            else
            {
                // Interactive mode: create with -it and /bin/sh for interactive environments
                createArgs.AddRange(new[] { "create", "--name", containerName, "-it" });
            }
            
            // Add blueprint label if provided
            if (!string.IsNullOrEmpty(blueprintName))
            {
                createArgs.AddRange(new[] { "--label", $"thresh.blueprint={blueprintName}" });
            }
            
            // Add networking configuration from blueprint
            AddContainerArgs(createArgs, blueprint);
            
            if (serviceMode)
            {
                // Service mode: just add image, use its default CMD
                createArgs.Add(sourcePath);
            }
            else
            {
                // Interactive mode: add image and shell command
                createArgs.AddRange(new[] { sourcePath, "/bin/sh" });
            }

            result = await ProcessHelper.ExecuteAsync(createArgs.ToArray());
        }
        
        return result.IsSuccess;
    }
    
    /// <summary>
    /// Add networking arguments to container create command from blueprint
    /// </summary>
    /// <summary>
    /// Add container configuration arguments from blueprint (networking, volumes, storage, etc.)
    /// </summary>
    private void AddContainerArgs(List<string> args, Blueprint? blueprint)
    {
        if (blueprint == null) return;
        
        // Add port mappings (-p HOST:CONTAINER)
        if (blueprint.Ports != null && blueprint.Ports.Count > 0)
        {
            foreach (var port in blueprint.Ports)
            {
                args.AddRange(new[] { "-p", port });
            }
        }
        
        // Add exposed ports (--expose CONTAINER)
        if (blueprint.Expose != null && blueprint.Expose.Count > 0)
        {
            foreach (var port in blueprint.Expose)
            {
                args.AddRange(new[] { "--expose", port });
            }
        }
        
        // Add network name (--network NAME)
        if (!string.IsNullOrEmpty(blueprint.Network))
        {
            args.AddRange(new[] { "--network", blueprint.Network });
        }
        
        // Add hostname (--hostname NAME)
        if (!string.IsNullOrEmpty(blueprint.Hostname))
        {
            args.AddRange(new[] { "--hostname", blueprint.Hostname });
        }
        
        // Add volumes (-v VOLUME:MOUNT_PATH)
        if (blueprint.Volumes != null && blueprint.Volumes.Count > 0)
        {
            foreach (var volume in blueprint.Volumes)
            {
                args.AddRange(new[] { "-v", $"{volume.Name}:{volume.Mount}" });
            }
        }
        
        // Add bind mounts (-v HOST_PATH:CONTAINER_PATH[:ro])
        if (blueprint.BindMounts != null && blueprint.BindMounts.Count > 0)
        {
            foreach (var bindMount in blueprint.BindMounts)
            {
                var mountSpec = bindMount.ReadOnly 
                    ? $"{bindMount.Host}:{bindMount.Container}:ro"
                    : $"{bindMount.Host}:{bindMount.Container}";
                args.AddRange(new[] { "-v", mountSpec });
            }
        }
        
        // Add tmpfs mounts (--tmpfs PATH)
        if (blueprint.Tmpfs != null && blueprint.Tmpfs.Count > 0)
        {
            foreach (var tmpfs in blueprint.Tmpfs)
            {
                args.AddRange(new[] { "--tmpfs", tmpfs });
            }
        }

        // Add environment variables (-e KEY=VALUE)
        if (blueprint.Environment != null && blueprint.Environment.Count > 0)
        {
            foreach (var (key, value) in blueprint.Environment)
            {
                args.AddRange(new[] { "-e", $"{key}={value}" });
            }
        }
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

    /// <summary>
    /// List all volumes managed by the runtime
    /// </summary>
    public async Task<List<VolumeInfo>> ListVolumesAsync()
    {
        var volumes = new List<VolumeInfo>();
        var tool = await GetAvailableToolAsync();

        var result = await ProcessHelper.ExecuteAsync(tool, "volume", "ls", "--format", "{{.Name}}");
        if (!result.IsSuccess)
            return volumes;

        foreach (var line in result.Output)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var volumeName = line.Trim();
            var volumeInfo = await InspectVolumeAsync(volumeName);
            if (volumeInfo != null)
            {
                volumes.Add(volumeInfo);
            }
        }

        return volumes;
    }

    /// <summary>
    /// Create a named volume
    /// </summary>
    public async Task<bool> CreateVolumeAsync(string volumeName)
    {
        var tool = await GetAvailableToolAsync();
        var result = await ProcessHelper.ExecuteAsync(tool, "volume", "create", volumeName);
        return result.IsSuccess;
    }

    /// <summary>
    /// Delete a volume
    /// </summary>
    public async Task<bool> DeleteVolumeAsync(string volumeName)
    {
        var tool = await GetAvailableToolAsync();
        var result = await ProcessHelper.ExecuteAsync(tool, "volume", "rm", volumeName);
        return result.IsSuccess;
    }

    /// <summary>
    /// Get detailed information about a volume
    /// </summary>
    public async Task<VolumeInfo?> InspectVolumeAsync(string volumeName)
    {
        var tool = await GetAvailableToolAsync();
        var result = await ProcessHelper.ExecuteAsync(tool, "volume", "inspect", volumeName);
        
        if (!result.IsSuccess)
            return null;

        try
        {
            var jsonOutput = string.Join("", result.Output);
            var volumeArray = JsonSerializer.Deserialize(jsonOutput, ContainerdJsonContext.Default.ListDockerVolumeInspect);
            
            if (volumeArray == null || volumeArray.Count == 0)
                return null;

            var dockerVolume = volumeArray[0];
            return new VolumeInfo
            {
                Name = dockerVolume.Name ?? volumeName,
                Driver = dockerVolume.Driver ?? "local",
                Mountpoint = dockerVolume.Mountpoint ?? "",
                CreatedAt = DateTime.TryParse(dockerVolume.CreatedAt, out var created) ? created : DateTime.MinValue,
                Scope = dockerVolume.Scope ?? "local",
                Labels = dockerVolume.Labels
            };
        }
        catch (JsonException)
        {
            // Fallback to basic info
            return new VolumeInfo
            {
                Name = volumeName,
                Driver = "local",
                Scope = "local"
            };
        }
    }
}
