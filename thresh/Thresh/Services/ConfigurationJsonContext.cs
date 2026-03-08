using System.Text.Json.Serialization;
using System.Text.Json;
using Thresh.Models;

namespace Thresh.Services;

/// <summary>
/// JSON serialization context for Native AOT compilation - Configuration
/// </summary>
[JsonSerializable(typeof(ConfigurationSettings))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, CustomDistribution>))]
[JsonSerializable(typeof(CustomDistribution))]
[JsonSerializable(typeof(AgentConfiguration))]
internal partial class ConfigurationJsonContext : JsonSerializerContext
{
}

/// <summary>
/// JSON serialization context for Native AOT compilation - Blueprints
/// </summary>
[JsonSerializable(typeof(Blueprint))]
[JsonSerializable(typeof(BlueprintScripts))]
[JsonSerializable(typeof(BlueprintVolume))]
[JsonSerializable(typeof(BlueprintBindMount))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<BlueprintVolume>))]
[JsonSerializable(typeof(List<BlueprintBindMount>))]
[JsonSerializable(typeof(EnvironmentMetadata))]
[JsonSerializable(typeof(CustomDistribution))]
[JsonSerializable(typeof(JsonDocument))]
internal partial class BlueprintJsonContext : JsonSerializerContext
{
}
