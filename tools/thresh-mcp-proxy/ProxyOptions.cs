namespace Thresh.McpProxy;

/// <summary>
/// CC-4 — argv parsing for thresh-mcp-proxy. Required: tenant, thread, node.
/// Hub URL and token fall back to env vars so the hub can pass creds without
/// exposing them on the command line (visible via /proc on Linux).
/// </summary>
public sealed record ProxyOptions
{
    public required string TenantId { get; init; }
    public required string ThreadId { get; init; }
    public required string NodeId { get; init; }
    public required string HubUrl { get; init; }
    public string? Token { get; init; }

    /// <summary>If true, skip TLS cert validation (dev only).</summary>
    public bool InsecureTls { get; init; }

    public static ProxyOptions? Parse(string[] args)
    {
        string? tenant = null, thread = null, node = null;
        string? hub = Environment.GetEnvironmentVariable("THRESH_HUB_URL") ?? "https://localhost:7200";
        string? token = Environment.GetEnvironmentVariable("THRESH_HUB_TOKEN");
        var insecure = string.Equals(Environment.GetEnvironmentVariable("THRESH_HUB_INSECURE"), "1", StringComparison.Ordinal);

        for (var i = 0; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"missing value for {args[i]}");
            switch (args[i])
            {
                case "--tenant": tenant = Next(); break;
                case "--thread": thread = Next(); break;
                case "--node":   node   = Next(); break;
                case "--hub":    hub    = Next(); break;
                case "--token":  token  = Next(); break;
                case "--insecure": insecure = true; break;
                default: Console.Error.WriteLine($"unknown arg: {args[i]}"); return null;
            }
        }

        if (string.IsNullOrEmpty(tenant) || string.IsNullOrEmpty(thread) || string.IsNullOrEmpty(node) || string.IsNullOrEmpty(hub))
            return null;

        return new ProxyOptions
        {
            TenantId = tenant!,
            ThreadId = thread!,
            NodeId = node!,
            HubUrl = hub!.TrimEnd('/'),
            Token = token,
            InsecureTls = insecure
        };
    }
}
