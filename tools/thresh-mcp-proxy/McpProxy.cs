using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Thresh.McpProxy;

/// <summary>
/// CC-4 — MCP server side of the proxy. Speaks JSON-RPC 2.0 over stdio with the
/// Copilot CLI; translates each request into a hub HTTP call. Only the surface
/// the conductor actually drives is implemented (<c>initialize</c>,
/// <c>tools/list</c>, <c>tools/call</c>, <c>ping</c>).
/// </summary>
public sealed class McpProxy
{
    private readonly HubClient _hub;
    private readonly ProxyOptions _opts;

    public McpProxy(HubClient hub, ProxyOptions opts)
    {
        _hub = hub;
        _opts = opts;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        string? line;
        while (!ct.IsCancellationRequested && (line = await Console.In.ReadLineAsync(ct)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string response;
            try { response = await DispatchAsync(line, ct); }
            catch (Exception ex)
            {
                response = JsonRpcError(null, -32603, $"proxy error: {ex.Message}");
            }
            await Console.Out.WriteLineAsync(response);
            await Console.Out.FlushAsync(ct);
        }
    }

    private async Task<string> DispatchAsync(string jsonRpcLine, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(jsonRpcLine);
        var root = doc.RootElement;
        JsonNode? id = root.TryGetProperty("id", out var idEl) ? JsonNode.Parse(idEl.GetRawText()) : null;
        var method = root.TryGetProperty("method", out var mEl) ? mEl.GetString() : null;
        var paramsEl = root.TryGetProperty("params", out var pEl) ? pEl : default;

        return method switch
        {
            "initialize" => Initialize(id),
            "notifications/initialized" => string.Empty, // no response for notifications
            "ping" => JsonRpcResult(id, new JsonObject()),
            "tools/list" => JsonRpcResult(id, await _hub.ListToolsAsync(ct) ?? new JsonObject()),
            "tools/call" => JsonRpcResult(id, await CallAsync(paramsEl, ct)),
            _ => JsonRpcError(id, -32601, $"method not found: {method}")
        };
    }

    private async Task<JsonNode?> CallAsync(JsonElement paramsEl, CancellationToken ct)
    {
        if (paramsEl.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("tools/call requires params object");

        var name = paramsEl.GetProperty("name").GetString()
            ?? throw new InvalidOperationException("tools/call.params.name required");
        var args = paramsEl.TryGetProperty("arguments", out var a)
            ? JsonNode.Parse(a.GetRawText())
            : null;
        return await _hub.CallToolAsync(name, args, ct);
    }

    private string Initialize(JsonNode? id)
    {
        var result = new JsonObject
        {
            ["protocolVersion"] = "2024-11-05",
            ["serverInfo"] = new JsonObject
            {
                ["name"] = "thresh-mcp-proxy",
                ["version"] = "1.6.0",
                ["nodeId"] = _opts.NodeId,
                ["tenantId"] = _opts.TenantId,
                ["threadId"] = _opts.ThreadId
            },
            ["capabilities"] = new JsonObject
            {
                ["tools"] = new JsonObject { ["listChanged"] = false }
            }
        };
        return JsonRpcResult(id, result);
    }

    // -------------------------------------------------------------------------
    // JSON-RPC helpers
    // -------------------------------------------------------------------------
    private static string JsonRpcResult(JsonNode? id, JsonNode? result)
    {
        var obj = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["result"] = result?.DeepClone() ?? new JsonObject()
        };
        return obj.ToJsonString();
    }

    private static string JsonRpcError(JsonNode? id, int code, string message)
    {
        var obj = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["error"] = new JsonObject { ["code"] = code, ["message"] = message }
        };
        return obj.ToJsonString();
    }
}
