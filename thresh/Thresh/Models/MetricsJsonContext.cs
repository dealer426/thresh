using System.Text.Json.Serialization;

namespace Thresh.Models;

/// <summary>
/// JSON serialization context for metrics (AOT compatible)
/// </summary>
[JsonSerializable(typeof(HostMetrics))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true)]
internal partial class MetricsJsonContext : JsonSerializerContext
{
}
