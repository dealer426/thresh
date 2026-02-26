using System.Text.Json.Serialization;
using System.Text.Json;

namespace Thresh.Services;

/// <summary>
/// JSON serialization context for containerd/nerdctl types (AOT compatible)
/// </summary>
[JsonSerializable(typeof(NerdctlVersion))]
[JsonSerializable(typeof(NerdctlContainer))]
[JsonSerializable(typeof(ClientVersion))]
[JsonSerializable(typeof(ServerVersion))]
[JsonSerializable(typeof(List<DockerVolumeInspect>))]
[JsonSerializable(typeof(DockerVolumeInspect))]
internal partial class ContainerdJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Nerdctl/Docker version information
/// </summary>
internal class NerdctlVersion
{
    public ClientVersion? Client { get; set; }
    public ServerVersion? Server { get; set; }
}

internal class ClientVersion
{
    public string? Version { get; set; }
}

internal class ServerVersion
{
    public string? Version { get; set; }
}

/// <summary>
/// Nerdctl/Docker container information
/// </summary>
internal class NerdctlContainer
{
    public string Names { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Labels can be either:
    // - Docker: "thresh.blueprint=value" (comma-separated string)
    // - nerdctl: {"thresh.blueprint":"value"} (JSON object)
    public JsonElement? Labels { get; set; }
    
    /// <summary>
    /// Get labels as a string (handles both Docker string and nerdctl JSON object formats)
    /// </summary>
    public string? GetLabelsAsString()
    {
        if (Labels == null) return null;
        
        if (Labels.Value.ValueKind == JsonValueKind.String)
        {
            // Docker format: return as-is
            return Labels.Value.GetString();
        }
        else if (Labels.Value.ValueKind == JsonValueKind.Object)
        {
            // nerdctl format: return JSON string
            return Labels.Value.GetRawText();
        }
        
        return null;
    }
}
/// <summary>
/// Docker volume inspect JSON structure
/// </summary>
internal class DockerVolumeInspect
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Driver")]
    public string? Driver { get; set; }

    [JsonPropertyName("Mountpoint")]
    public string? Mountpoint { get; set; }

    [JsonPropertyName("CreatedAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("Scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("Labels")]
    public Dictionary<string, string>? Labels { get; set; }
}