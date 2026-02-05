using System.Text.Json.Serialization;

namespace EknovaCli.Models;

/// <summary>
/// Blueprint model representing an environment template
/// </summary>
public class Blueprint
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;
    
    [JsonPropertyName("packages")]
    public List<string>? Packages { get; set; }
    
    [JsonPropertyName("scripts")]
    public BlueprintScripts? Scripts { get; set; }
    
    [JsonPropertyName("environment")]
    public Dictionary<string, string>? Environment { get; set; }

    public override string ToString()
    {
        return $"Blueprint{{Name='{Name}', Base='{Base}', Packages={Packages?.Count ?? 0}}}";
    }
}

/// <summary>
/// Scripts for blueprint provisioning
/// </summary>
public class BlueprintScripts
{
    [JsonPropertyName("setup")]
    public string? Setup { get; set; }
    
    [JsonPropertyName("postInstall")]
    public string? PostInstall { get; set; }
}
