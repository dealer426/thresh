namespace Thresh.Mcp;

/// <summary>
/// AOT-compatible response types for stdio MCP server
/// </summary>

internal class JsonRpcResponse<T>
{
    public int? Id { get; set; }
    public T? Result { get; set; }
}

internal class JsonRpcErrorResponse
{
    public int? Id { get; set; }
    public JsonRpcError? Error { get; set; }
}

internal class JsonRpcError
{
    public int Code { get; set; }
    public string? Message { get; set; }
}

internal class InitializeResult
{
    public string? ProtocolVersion { get; set; }
    public object? Capabilities { get; set; }
    public ServerInfoResult? ServerInfo { get; set; }
    public string? Instructions { get; set; }
}

internal class ServerInfoResult
{
    public string? Name { get; set; }
    public string? Version { get; set; }
}

internal class ToolErrorResult
{
    public ContentItem[]? Content { get; set; }
    public bool IsError { get; set; }
}

internal class ContentItem
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}
