namespace Thresh.Models;

/// <summary>
/// Information about a container runtime (WSL, containerd, etc.)
/// </summary>
public record RuntimeInfo(
    bool IsAvailable,
    string Version,
    string? Details = null,
    string? RawOutput = null,
    int ContainerCount = 0)
{
    /// <summary>
    /// Create RuntimeInfo for unavailable runtime
    /// </summary>
    public static RuntimeInfo Unavailable(string reason = "Not available") =>
        new(false, reason);

    /// <summary>
    /// Create RuntimeInfo for available runtime
    /// </summary>
    public static RuntimeInfo Available(string version, int containerCount = 0, string? details = null, string? rawOutput = null) =>
        new(true, version, details, rawOutput, containerCount);
}
