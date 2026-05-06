using Thresh.McpProxy;

// thresh-mcp-proxy — local MCP server spawned by the Copilot CLI inside the
// thresh-hub process. Per ChatThread, the CopilotClientPool boots one of these
// per (tenant, thread, node) tuple, and the Copilot CLI talks to it over stdio.
//
// Usage: thresh-mcp-proxy --node <id> --tenant <id> --thread <id> [--hub <url>] [--token <jwt>]
//
// stdio:  JSON-RPC 2.0 (MCP)        ← spoken to Copilot CLI
// upstream: HTTP POST to hub /api/mcp/proxy (CC-7 endpoint, stubbed for now)

var opts = ProxyOptions.Parse(args);
if (opts is null)
{
    Console.Error.WriteLine("usage: thresh-mcp-proxy --node <id> --tenant <id> --thread <id> [--hub <url>] [--token <jwt>]");
    return 2;
}

await Console.Error.WriteLineAsync($"thresh-mcp-proxy: tenant={opts.TenantId} thread={opts.ThreadId} node={opts.NodeId} hub={opts.HubUrl}");

using var hubClient = new HubClient(opts);
var proxy = new McpProxy(hubClient, opts);
await proxy.RunAsync();
return 0;
