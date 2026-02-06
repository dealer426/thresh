using System.Runtime.InteropServices;

namespace Thresh.Services;

/// <summary>
/// Factory for creating the appropriate container service based on platform
/// </summary>
public static class ContainerServiceFactory
{
    /// <summary>
    /// Create the appropriate container service for the current platform
    /// </summary>
    /// <returns>IContainerService implementation (WslService on Windows, ContainerdService elsewhere)</returns>
    public static IContainerService Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WslService();
        }
        
        // Linux and macOS use containerd
        return new ContainerdService();
    }

    /// <summary>
    /// Get the expected runtime name for the current platform
    /// </summary>
    public static string GetExpectedRuntimeName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "WSL";
        
        return "containerd";
    }

    /// <summary>
    /// Get the current platform name
    /// </summary>
    public static string GetPlatformName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS";
        
        return "Unknown";
    }

    /// <summary>
    /// Check if the current platform is supported
    /// </summary>
    public static bool IsPlatformSupported()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
               RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
               RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}
