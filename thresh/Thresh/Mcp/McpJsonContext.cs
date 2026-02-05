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
internal partial class McpJsonContext : JsonSerializerContext
{
}
