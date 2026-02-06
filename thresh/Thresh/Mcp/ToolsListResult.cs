namespace Thresh.Mcp;

/// <summary>
/// Additional response types for tools/list result
/// </summary>
internal class ToolsListResult
{
    public object[]? Tools { get; set; }
}

/// <summary>
/// Generic result wrapper
/// </summary>
internal class GenericResult
{
    public object? Result { get; set; }
}
