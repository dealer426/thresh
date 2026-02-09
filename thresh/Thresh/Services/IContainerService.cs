using Thresh.Models;
using Thresh.Utilities;

namespace Thresh.Services;

/// <summary>
/// Interface for container runtime services (WSL, containerd, etc.)
/// Abstracts environment lifecycle management across different platforms
/// </summary>
public interface IContainerService
{
    /// <summary>
    /// Get the name of the container runtime provider
    /// </summary>
    string RuntimeName { get; }

    /// <summary>
    /// Get the platform this runtime runs on (Windows, Linux, macOS)
    /// </summary>
    string Platform { get; }

    /// <summary>
    /// Check if the runtime is available on the system
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Get runtime version and information
    /// </summary>
    Task<RuntimeInfo> GetRuntimeInfoAsync();

    /// <summary>
    /// List all environments managed by this runtime
    /// </summary>
    Task<List<Models.Environment>> ListEnvironmentsAsync();

    /// <summary>
    /// List environments with option to include system-managed containers
    /// </summary>
    /// <param name="includeAll">Include all containers, not just thresh-managed ones</param>
    Task<List<Models.Environment>> ListEnvironmentsAsync(bool includeAll);

    /// <summary>
    /// Find an environment by name
    /// </summary>
    Task<Models.Environment?> FindEnvironmentAsync(string name);

    /// <summary>
    /// Start an environment
    /// </summary>
    Task<bool> StartEnvironmentAsync(string environmentName);

    /// <summary>
    /// Stop a running environment
    /// </summary>
    Task<bool> StopEnvironmentAsync(string environmentName);

    /// <summary>
    /// Remove an environment permanently
    /// </summary>
    Task<bool> RemoveEnvironmentAsync(string environmentName);

    /// <summary>
    /// Import/create a new environment from an image or tar file
    /// </summary>
    /// <param name="environmentName">Name for the new environment</param>
    /// <param name="sourcePath">Path to tar file or image name</param>
    /// <param name="installPath">Installation path (may be ignored by some runtimes)</param>
    Task<bool> ImportEnvironmentAsync(string environmentName, string sourcePath, string installPath);

    /// <summary>
    /// Execute a command in an environment
    /// </summary>
    Task<ProcessHelper.ProcessResult> ExecuteCommandAsync(string environmentName, string command);

    /// <summary>
    /// Check if an environment exists
    /// </summary>
    Task<bool> EnvironmentExistsAsync(string environmentName);
}
