using Thresh.Pm;

// thresh-pm-server — local MCP server spawned by the Copilot CLI inside a
// thresh-hub `pm-group` chat thread. One process per (tenant, thread) tuple.
// CC-11a interactive surface: query_node / instruct_node / pause / resume /
// summarize. Hub fan-out wiring is stubbed in v1.6 — surface is real, real
// peer-node delegation lands with the E2E demo gating item. CC-11b (Ralph
// autonomous loop) is deferred to v1.6.1 per docs/ARCH_V1.6_COPILOT_CONDUCTOR.md.
//
// Usage: thresh-pm-server --tenant <id> --thread <id> --group <id> [--hub <url>] [--token <jwt>]

var opts = PmOptions.Parse(args);
if (opts is null)
{
    Console.Error.WriteLine("usage: thresh-pm-server --tenant <id> --thread <id> --group <id> [--hub <url>] [--token <jwt>]");
    return 2;
}

await Console.Error.WriteLineAsync(
    $"thresh-pm-server: tenant={opts.TenantId} thread={opts.ThreadId} group={opts.GroupId} hub={opts.HubUrl}");

var server = new PmServer(opts);
await server.RunAsync();
return 0;
