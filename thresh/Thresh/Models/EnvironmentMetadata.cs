namespace EknovaCli.Models;

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
}
