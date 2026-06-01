using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Thresh.Pm;

/// <summary>
/// CC-11a — local MCP server that exposes the PM agent's coordination tools
/// to the Copilot CLI inside a <c>pm-group</c> chat thread. Implements the
/// JSON-RPC 2.0 / MCP surface (<c>initialize</c>, <c>tools/list</c>,
/// <c>tools/call</c>, <c>ping</c>) over stdio.
///
/// Tools (CC-11a, interactive PM):
/// <list type="bullet">
///   <item><c>query_node(handle, q)</c> — read-only consult of a peer node's MCP.</item>
///   <item><c>instruct_node(handle, instruction)</c> — fire a child SDK session on a peer node and stream back its output.</item>
///   <item><c>pause(handle)</c> / <c>resume(handle)</c> — control a child session.</item>
///   <item><c>summarize(scope)</c> — synthesize state across the group.</item>
/// </list>
///
/// Hub wiring (delegating to actual peer nodes through the hub) is stubbed
/// for v1.6 — the surface is real, the network calls are not yet plumbed.
/// CC-11b (Ralph autonomous loop) is deferred to v1.6.1.
/// </summary>
public sealed class PmServer
{
    private readonly PmOptions _opts;

    public PmServer(PmOptions opts) => _opts = opts;

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        string? line;
        while (!ct.IsCancellationRequested && (line = await Console.In.ReadLineAsync(ct)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string response;
            try { response = Dispatch(line); }
            catch (Exception ex)
            {
                response = JsonRpcError(null, -32603, $"thresh-pm error: {ex.Message}");
            }
            if (string.IsNullOrEmpty(response)) continue;
            await Console.Out.WriteLineAsync(response);
            await Console.Out.FlushAsync(ct);
        }
    }

    private string Dispatch(string jsonRpcLine)
    {
        using var doc = JsonDocument.Parse(jsonRpcLine);
        var root = doc.RootElement;
        JsonNode? id = root.TryGetProperty("id", out var idEl) ? JsonNode.Parse(idEl.GetRawText()) : null;
        var method = root.TryGetProperty("method", out var mEl) ? mEl.GetString() : null;
        var paramsEl = root.TryGetProperty("params", out var pEl) ? pEl : default;

        return method switch
        {
            "initialize" => Initialize(id),
            "notifications/initialized" => string.Empty,
            "ping" => JsonRpcResult(id, new JsonObject()),
            "tools/list" => JsonRpcResult(id, ListTools()),
            "tools/call" => JsonRpcResult(id, CallTool(paramsEl)),
            _ => JsonRpcError(id, -32601, $"method not found: {method}")
        };
    }

    private string Initialize(JsonNode? id)
    {
        var result = new JsonObject
        {
            ["protocolVersion"] = "2024-11-05",
            ["serverInfo"] = new JsonObject
            {
                ["name"] = "thresh-pm",
                ["version"] = "1.6.0",
                ["tenantId"] = _opts.TenantId,
                ["threadId"] = _opts.ThreadId,
                ["groupId"] = _opts.GroupId,
            },
            ["capabilities"] = new JsonObject
            {
                ["tools"] = new JsonObject { ["listChanged"] = false }
            }
        };
        return JsonRpcResult(id, result);
    }

    private static JsonNode ListTools()
    {
        var tools = new JsonArray
        {
            ToolDescriptor(
                "query_node",
                "Read-only query of a peer node's MCP (e.g. fs.read, system.info). No side effects.",
                schema: new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray { "handle", "q" },
                    ["properties"] = new JsonObject
                    {
                        ["handle"] = new JsonObject { ["type"] = "string", ["description"] = "Node handle (e.g. @node-1)" },
                        ["q"] = new JsonObject { ["type"] = "string", ["description"] = "Question / read request" },
                    },
                }),
            ToolDescriptor(
                "instruct_node",
                "Fire a child Copilot SDK session on a peer node with an instruction. Streams the node's output back as a tool result.",
                schema: new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray { "handle", "instruction" },
                    ["properties"] = new JsonObject
                    {
                        ["handle"] = new JsonObject { ["type"] = "string" },
                        ["instruction"] = new JsonObject { ["type"] = "string" },
                    },
                }),
            ToolDescriptor(
                "pause",
                "Pause an in-flight child session on a node.",
                schema: new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray { "handle" },
                    ["properties"] = new JsonObject
                    {
                        ["handle"] = new JsonObject { ["type"] = "string" },
                    },
                }),
            ToolDescriptor(
                "resume",
                "Resume a paused child session on a node.",
                schema: new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray { "handle" },
                    ["properties"] = new JsonObject
                    {
                        ["handle"] = new JsonObject { ["type"] = "string" },
                    },
                }),
            ToolDescriptor(
                "summarize",
                "Synthesize current state across a scope (single node, group, or all). Returns a structured summary.",
                schema: new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray { "scope" },
                    ["properties"] = new JsonObject
                    {
                        ["scope"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray { "node", "group", "all" } },
                        ["handle"] = new JsonObject { ["type"] = "string", ["description"] = "Required when scope=node" },
                    },
                }),
        };
        return new JsonObject { ["tools"] = tools };
    }

    private static JsonObject ToolDescriptor(string name, string description, JsonObject schema)
        => new()
        {
            ["name"] = name,
            ["description"] = description,
            ["inputSchema"] = schema,
        };

    private JsonNode CallTool(JsonElement paramsEl)
    {
        if (paramsEl.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("tools/call requires params object");

        var name = paramsEl.GetProperty("name").GetString()
            ?? throw new InvalidOperationException("tools/call.params.name required");
        var args = paramsEl.TryGetProperty("arguments", out var a) ? a : default;

        // CC-11a stubs — surface is final, hub wiring lands with the E2E demo
        // gating item. Each stub returns a structured MCP tool result so the
        // PM persona's Copilot session can reason about the response.
        return name switch
        {
            "query_node"    => StubResult($"query_node[{Arg(args, "handle")}]: {Arg(args, "q")} (stub: hub wiring pending)"),
            "instruct_node" => StubResult($"instruct_node[{Arg(args, "handle")}]: dispatched (stub: hub wiring pending)"),
            "pause"         => StubResult($"pause[{Arg(args, "handle")}]: ok (stub)"),
            "resume"        => StubResult($"resume[{Arg(args, "handle")}]: ok (stub)"),
            "summarize"     => StubResult($"summarize[{Arg(args, "scope")}]: no nodes reporting (stub)"),
            _ => throw new InvalidOperationException($"unknown tool: {name}"),
        };
    }

    private static string Arg(JsonElement args, string key)
    {
        if (args.ValueKind != JsonValueKind.Object) return string.Empty;
        return args.TryGetProperty(key, out var v) ? (v.GetString() ?? string.Empty) : string.Empty;
    }

    private static JsonNode StubResult(string text) => new JsonObject
    {
        ["content"] = new JsonArray
        {
            new JsonObject { ["type"] = "text", ["text"] = text },
        },
        ["isError"] = false,
    };

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
