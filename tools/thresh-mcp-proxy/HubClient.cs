using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Thresh.McpProxy;

/// <summary>
/// CC-4 — thin HTTP client that forwards MCP <c>tools/list</c> and
/// <c>tools/call</c> requests to the hub's <c>/api/mcp/proxy</c> endpoint
/// (added in CC-7). The hub is responsible for routing the call to the
/// correct agent over the existing AgentHub SignalR fabric.
/// </summary>
public sealed class HubClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly ProxyOptions _opts;

    public HubClient(ProxyOptions opts)
    {
        _opts = opts;
        var handler = new HttpClientHandler();
        if (opts.InsecureTls)
        {
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };
        _http.DefaultRequestHeaders.Add("X-Tenant", opts.TenantId);
        _http.DefaultRequestHeaders.Add("X-Thread", opts.ThreadId);
        _http.DefaultRequestHeaders.Add("X-Node", opts.NodeId);
        if (!string.IsNullOrEmpty(opts.Token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opts.Token);
    }

    /// <summary>Forwards an MCP <c>tools/list</c> for the bound node.</summary>
    public async Task<JsonNode?> ListToolsAsync(CancellationToken ct)
    {
        var url = $"{_opts.HubUrl}/api/mcp/proxy/tools/list";
        using var resp = await _http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"hub tools/list failed: {(int)resp.StatusCode} {resp.ReasonPhrase}");
        return await resp.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: ct);
    }

    /// <summary>Forwards an MCP <c>tools/call</c> for the bound node.</summary>
    public async Task<JsonNode?> CallToolAsync(string toolName, JsonNode? arguments, CancellationToken ct)
    {
        var url = $"{_opts.HubUrl}/api/mcp/proxy/tools/call";
        var payload = new JsonObject
        {
            ["name"] = toolName,
            ["arguments"] = arguments?.DeepClone()
        };
        using var resp = await _http.PostAsJsonAsync(url, payload, ct);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"hub tools/call failed: {(int)resp.StatusCode} {resp.ReasonPhrase}");
        return await resp.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: ct);
    }

    public void Dispose() => _http.Dispose();
}
