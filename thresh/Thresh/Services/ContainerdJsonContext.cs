using System.Text.Json.Serialization;

namespace Thresh.Services;

/// <summary>
/// JSON serialization context for containerd/nerdctl types (AOT compatible)
/// </summary>
[JsonSerializable(typeof(NerdctlVersion))]
[JsonSerializable(typeof(NerdctlContainer))]
[JsonSerializable(typeof(ClientVersion))]
[JsonSerializable(typeof(ServerVersion))]
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
}
