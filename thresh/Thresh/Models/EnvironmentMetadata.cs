namespace Thresh.Models;

/// <summary>
/// Metadata for tracking environment provisioning details
/// </summary>
public class EnvironmentMetadata
{
    public string EnvironmentName { get; set; } = string.Empty;
    public string BlueprintName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string Base { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DistributionSource { get; set; }  // "Vendor" or "MicrosoftStore"
    
    // Networking configuration
    public List<string>? Ports { get; set; }
    public List<string>? Expose { get; set; }
    public string? Network { get; set; }
    public string? Hostname { get; set; }
    
    // Storage configuration
    public List<BlueprintVolume>? Volumes { get; set; }
    public List<BlueprintBindMount>? BindMounts { get; set; }
    public List<string>? Tmpfs { get; set; }
}
