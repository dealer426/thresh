using System.Text.Json.Serialization;

namespace Thresh.Models;

/// <summary>
/// JSON serialization context for metrics (AOT compatible)
/// </summary>
[JsonSerializable(typeof(HostMetrics))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<double>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true)]
internal partial class MetricsJsonContext : JsonSerializerContext
{
}
