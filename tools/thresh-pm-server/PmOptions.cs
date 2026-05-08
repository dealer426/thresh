namespace Thresh.Pm;

/// <summary>
/// CC-11 — startup options for <c>thresh-pm-server</c>. Spawned by the
/// CopilotClientPool when a thread's persona is <c>pm-group</c>; one process
/// per (tenant, thread) tuple. The hub URL + token allow the PM server to
/// fan out to peer node MCP endpoints via the hub.
/// </summary>
public sealed record PmOptions(
    string TenantId,
    string ThreadId,
    string GroupId,
    string HubUrl,
    string? Token)
{
    public static PmOptions? Parse(string[] args)
    {
        string? tenant = null, thread = null, group = null, hub = null, token = null;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--tenant": tenant = Next(args, ref i); break;
                case "--thread": thread = Next(args, ref i); break;
                case "--group":  group  = Next(args, ref i); break;
                case "--hub":    hub    = Next(args, ref i); break;
                case "--token":  token  = Next(args, ref i); break;
            }
        }
        if (tenant is null || thread is null || group is null) return null;
        return new PmOptions(tenant, thread, group, hub ?? "http://localhost:5000", token);
    }

    private static string? Next(string[] args, ref int i)
        => (i + 1 < args.Length) ? args[++i] : null;
}
