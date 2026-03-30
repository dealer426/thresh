using System.Text.Json.Serialization;

namespace Thresh.Models;

/// <summary>
/// JSON source generation context for Native AOT support
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AgentInfo))]
[JsonSerializable(typeof(MetricsData))]
[JsonSerializable(typeof(EnvironmentSummary))]
[JsonSerializable(typeof(ProvisionRequest))]
[JsonSerializable(typeof(DestroyRequest))]
[JsonSerializable(typeof(AgentCommand))]
[JsonSerializable(typeof(CommandResult))]
[JsonSerializable(typeof(AgentHeartbeat))]
[JsonSerializable(typeof(AgentConfiguration))]
[JsonSerializable(typeof(AgentStatus))]
[JsonSerializable(typeof(StoredCredentials))]
[JsonSerializable(typeof(List<EnvironmentSummary>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(StackDeployPayloadAgent))]
[JsonSerializable(typeof(StackServiceDefAgent))]
[JsonSerializable(typeof(List<StackServiceDefAgent>))]
[JsonSerializable(typeof(StackServiceStatusArgs))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(string))]
public partial class AgentJsonContext : JsonSerializerContext
{
    // Node command DTOs are serialized via NodeJsonContext (in NodeCommands.cs)
}
