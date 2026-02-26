namespace Thresh.Models;

/// <summary>
/// Information about a container volume
/// </summary>
public class VolumeInfo
{
    /// <summary>
    /// Volume name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Volume driver (e.g., "local")
    /// </summary>
    public string Driver { get; set; } = "local";

    /// <summary>
    /// Mount point on host
    /// </summary>
    public string Mountpoint { get; set; } = string.Empty;

    /// <summary>
    /// Volume creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Volume size in bytes (if available)
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Volume scope (local, global, etc.)
    /// </summary>
    public string Scope { get; set; } = "local";

    /// <summary>
    /// Labels attached to the volume
    /// </summary>
    public Dictionary<string, string>? Labels { get; set; }

    public override string ToString()
    {
        var sizeStr = Size.HasValue ? $"{Size.Value / (1024.0 * 1024.0):F2} MB" : "unknown";
        return $"Volume{{Name='{Name}', Driver='{Driver}', Size={sizeStr}}}";
    }
}
