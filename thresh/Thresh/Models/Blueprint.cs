using System.Text.Json.Serialization;

namespace Thresh.Models;

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
    
    // Networking configuration
    [JsonPropertyName("ports")]
    public List<string>? Ports { get; set; }
    
    [JsonPropertyName("expose")]
    public List<string>? Expose { get; set; }
    
    [JsonPropertyName("network")]
    public string? Network { get; set; }
    
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }
    
    // Storage configuration
    [JsonPropertyName("volumes")]
    public List<BlueprintVolume>? Volumes { get; set; }
    
    [JsonPropertyName("bind_mounts")]
    public List<BlueprintBindMount>? BindMounts { get; set; }
    
    [JsonPropertyName("tmpfs")]
    public List<string>? Tmpfs { get; set; }
    
    // WSL Configuration (Windows only)
    [JsonPropertyName("wslConfig")]
    public string? WslConfig { get; set; }
    
    [JsonPropertyName("wslConfigFile")]
    public string? WslConfigFile { get; set; }
    
    [JsonPropertyName("wslConfigCustom")]
    public string? WslConfigCustom { get; set; }

    public override string ToString()
    {
        return $"Blueprint{{Name='{Name}', Base='{Base}', Packages={Packages?.Count ?? 0}, Ports={Ports?.Count ?? 0}, Volumes={Volumes?.Count ?? 0}, WslConfig='{WslConfig}'}}";
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

/// <summary>
/// Volume configuration for blueprint
/// </summary>
public class BlueprintVolume
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("mount")]
    public string Mount { get; set; } = string.Empty;
}

/// <summary>
/// Bind mount configuration for blueprint
/// </summary>
public class BlueprintBindMount
{
    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;
    
    [JsonPropertyName("container")]
    public string Container { get; set; } = string.Empty;
    
    [JsonPropertyName("readonly")]
    public bool ReadOnly { get; set; } = false;
}
