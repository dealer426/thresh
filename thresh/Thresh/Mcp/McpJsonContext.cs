using System.Text.Json.Serialization;
using Thresh.Mcp.Models;

namespace Thresh.Mcp;

/// <summary>
/// JSON serialization context for MCP models (AOT compatibility)
/// </summary>
[JsonSerializable(typeof(InitializeResponse))]
[JsonSerializable(typeof(ServerInfo))]
[JsonSerializable(typeof(Capabilities))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(ToolCallRequest))]
[JsonSerializable(typeof(ToolCallResponse))]
[JsonSerializable(typeof(TextContent))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(JsonRpcResponse<InitializeResult>))]
[JsonSerializable(typeof(JsonRpcResponse<ToolErrorResult>))]
[JsonSerializable(typeof(JsonRpcResponse<ToolsListResult>))]
[JsonSerializable(typeof(JsonRpcResponse<GenericResult>))]
[JsonSerializable(typeof(JsonRpcErrorResponse))]
[JsonSerializable(typeof(JsonRpcError))]
[JsonSerializable(typeof(InitializeResult))]
[JsonSerializable(typeof(ServerInfoResult))]
[JsonSerializable(typeof(ToolErrorResult))]
[JsonSerializable(typeof(ContentItem))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class McpJsonContext : JsonSerializerContext
{
}
