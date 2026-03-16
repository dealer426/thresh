using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Thresh.Mcp;

/// <summary>
/// Stdio-to-hub MCP bridge.
/// Reads JSON-RPC 2.0 messages from stdin, forwards them to thresh-hub's /mcp endpoint,
/// and writes the responses back to stdout.
///
/// This lets VS Code connect to the fleet-wide hub MCP using a local stdio process:
///   "command": "thresh", "args": ["serve", "--hub"]
/// </summary>
public class HubMcpBridge
{
    private readonly string _hubUrl;
    private readonly string _authToken;
    private readonly HttpClient _http;
    private readonly CancellationTokenSource _cts = new();

    public HubMcpBridge(string hubUrl, string authToken, bool tlsVerify = true)
    {
        _hubUrl = hubUrl.TrimEnd('/');
        _authToken = authToken;

        var handler = new HttpClientHandler();
        if (!tlsVerify)
        {
            // Dev/self-signed cert mode — only skip verification when explicitly disabled
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        _http = new HttpClient(handler);
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
    }

    public void Stop() => _cts.Cancel();

    public async Task RunAsync()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var line = await ReadLineAsync(_cts.Token);
                if (line == null) break;              // stdin closed
                if (string.IsNullOrWhiteSpace(line)) continue;

                _ = Task.Run(() => ForwardAsync(line), _cts.Token);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task ForwardAsync(string jsonRpcLine)
    {
        string? responseJson = null;
        JsonElement? requestId = null;

        try
        {
            // Parse to get the id for error responses
            var doc = JsonDocument.Parse(jsonRpcLine);
            if (doc.RootElement.TryGetProperty("id", out var id))
                requestId = id;

            using var content = new StringContent(jsonRpcLine, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{_hubUrl}/mcp", content);

            responseJson = await resp.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            // Build a JSON-RPC error response so the client gets something useful
            var errId = requestId.HasValue ? requestId.Value.ToString() : "null";
            responseJson = "{\"jsonrpc\":\"2.0\",\"id\":" + errId + ",\"error\":{\"code\":-32603,\"message\":\"" + JsonEscape(ex.Message) + "\"}}";
            await Console.Error.WriteLineAsync($"[hub-bridge] Error: {ex.Message}");
        }

        if (!string.IsNullOrEmpty(responseJson))
        {
            await WriteLineAsync(responseJson);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static readonly SemaphoreSlim _writeLock = new(1, 1);

    private static async Task WriteLineAsync(string line)
    {
        await _writeLock.WaitAsync();
        try
        {
            await Console.Out.WriteLineAsync(line);
            await Console.Out.FlushAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static async Task<string?> ReadLineAsync(CancellationToken ct)
    {
        // Read until we have a complete JSON object (single line in MCP stdio protocol)
        return await Task.Run(() => Console.In.ReadLine(), ct);
    }

    private static string JsonEscape(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
}
