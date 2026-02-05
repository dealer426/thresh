namespace Thresh.Models;

/// <summary>
/// Custom distribution configuration
/// </summary>
public class CustomDistribution
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string RootfsUrl { get; set; } = string.Empty;
    public string PackageManager { get; set; } = "apt";
    public string? Description { get; set; }
}
