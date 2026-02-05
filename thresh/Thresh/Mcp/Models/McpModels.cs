using System.Text.Json.Serialization;

namespace Thresh.Mcp.Models;

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "thresh-mcp-server";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}

public class Tool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public object? InputSchema { get; set; }
}

public class Capabilities
{
    [JsonPropertyName("tools")]
    public List<Tool> Tools { get; set; } = new();
}

public class InitializeResponse
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("serverInfo")]
    public ServerInfo ServerInfo { get; set; } = new();

    [JsonPropertyName("capabilities")]
    public Capabilities Capabilities { get; set; } = new();
}

public class ToolCallRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public Dictionary<string, object?>? Arguments { get; set; }
}

public class TextContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class ToolCallResponse
{
    [JsonPropertyName("content")]
    public List<TextContent> Content { get; set; } = new();

    [JsonPropertyName("isError")]
    public bool IsError { get; set; }

    public static ToolCallResponse Success(string text)
    {
        return new ToolCallResponse
        {
            Content = new List<TextContent> { new() { Text = text } },
            IsError = false
        };
    }

    public static ToolCallResponse Error(string message)
    {
        return new ToolCallResponse
        {
            Content = new List<TextContent> { new() { Text = message } },
            IsError = true
        };
    }
}
