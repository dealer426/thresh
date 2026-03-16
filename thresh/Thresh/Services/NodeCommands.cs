using System.CommandLine;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Thresh.Models;

namespace Thresh.Services;

// ---------------------------------------------------------------------------
// AOT JSON context for node API DTOs
// ---------------------------------------------------------------------------
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(NodeListResponse))]
[JsonSerializable(typeof(NodeDetail))]
[JsonSerializable(typeof(NodeMetrics))]
[JsonSerializable(typeof(NodeRemoveResponse))]
[JsonSerializable(typeof(NodeCreateEnvironmentRequest))]
[JsonSerializable(typeof(NodeCommandOutput))]
internal partial class NodeJsonContext : JsonSerializerContext { }

internal class NodeListResponse
{
    public List<NodeDetail> Nodes { get; set; } = [];
}

internal class NodeDetail
{
    public string Id { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? AgentVersion { get; set; }
    public string? ConnectionTier { get; set; }
    public int? CpuCores { get; set; }
    public double? CpuPercent { get; set; }
    public int? RamTotalGb { get; set; }
    public double? RamUsedGb { get; set; }
    public int? DiskTotalGb { get; set; }
    public int? GpuCount { get; set; }
    public string? GpuModel { get; set; }
    public int? GpuMemoryTotalGb { get; set; }
    public int? EnvironmentCount { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

internal class NodeMetrics
{
    public string Id { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double? CpuPercent { get; set; }
    public double? RamUsedGb { get; set; }
    public int? RamTotalGb { get; set; }
    public int? DiskTotalGb { get; set; }
    public int? GpuCount { get; set; }
    public string? GpuModel { get; set; }
    public int? GpuMemoryTotalGb { get; set; }
    public int? EnvironmentCount { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

internal class NodeRemoveResponse
{
    public string? Message { get; set; }
    public string? Error { get; set; }
}

internal class NodeCreateEnvironmentRequest
{
    public string Blueprint { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

internal class NodeCommandOutput
{
    public string? Output { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// CLI node commands: list, info, metrics, remove
/// All require a valid CLI session (thresh auth login).
/// </summary>
public static class NodeCommands
{
    public static void Register(RootCommand rootCommand)
    {
        var nodeCommand = new Command("node", "Manage nodes connected to your thresh-hub account");

        nodeCommand.AddCommand(BuildListCommand());
        nodeCommand.AddCommand(BuildInfoCommand());
        nodeCommand.AddCommand(BuildMetricsCommand());
        nodeCommand.AddCommand(BuildRemoveCommand());
        nodeCommand.AddCommand(BuildUpCommand());
        nodeCommand.AddCommand(BuildBlueprintsCommand());

        rootCommand.AddCommand(nodeCommand);
    }

    // -------------------------------------------------------------------------
    // thresh node list
    // -------------------------------------------------------------------------
    private static Command BuildListCommand()
    {
        var cmd = new Command("list", "List all nodes connected to your account");

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            HttpResponseMessage resp;
            try { resp = await client.GetAsync("/api/v1/me/nodes"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode)
            {
                await PrintApiError(resp);
                return;
            }

            var result = await resp.Content.ReadFromJsonAsync(NodeJsonContext.Default.NodeListResponse);
            if (result == null || result.Nodes.Count == 0)
            {
                Console.WriteLine("No nodes found. Install and start the thresh agent on a machine to connect it.");
                return;
            }

            PrintNodeTable(result.Nodes);
        }, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh node info <node>
    // -------------------------------------------------------------------------
    private static Command BuildInfoCommand()
    {
        var cmd = new Command("info", "Show detailed information about a node");

        var nodeArg = new Argument<string>("node", "Node hostname or ID");
        cmd.AddArgument(nodeArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string node, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);
            var nodeId = await ResolveNodeId(client, node);
            if (nodeId == null) { NodeNotFound(node); return; }

            HttpResponseMessage resp;
            try { resp = await client.GetAsync($"/api/v1/me/nodes/{nodeId}"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var detail = await resp.Content.ReadFromJsonAsync(NodeJsonContext.Default.NodeDetail);
            if (detail == null) return;

            PrintNodeDetail(detail);
        }, nodeArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh node metrics <node>
    // -------------------------------------------------------------------------
    private static Command BuildMetricsCommand()
    {
        var cmd = new Command("metrics", "Show current resource metrics for a node");

        var nodeArg = new Argument<string>("node", "Node hostname or ID");
        cmd.AddArgument(nodeArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string node, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);
            var nodeId = await ResolveNodeId(client, node);
            if (nodeId == null) { NodeNotFound(node); return; }

            HttpResponseMessage resp;
            try { resp = await client.GetAsync($"/api/v1/me/nodes/{nodeId}/metrics"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var metrics = await resp.Content.ReadFromJsonAsync(NodeJsonContext.Default.NodeMetrics);
            if (metrics == null) return;

            PrintMetrics(metrics);
        }, nodeArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh node remove <node>
    // -------------------------------------------------------------------------
    private static Command BuildRemoveCommand()
    {
        var cmd = new Command("remove", "Remove a node from your account");

        var nodeArg = new Argument<string>("node", "Node hostname or ID");
        cmd.AddArgument(nodeArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        var forceOption = new Option<bool>("--force", "Skip confirmation prompt");
        cmd.AddOption(forceOption);

        cmd.SetHandler(async (string node, string? hub, bool force) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);
            var nodeId = await ResolveNodeId(client, node);
            if (nodeId == null) { NodeNotFound(node); return; }

            if (!force)
            {
                Console.Write($"Remove node '{node}'? This cannot be undone. [y/N] ");
                var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (answer != "y" && answer != "yes")
                {
                    Console.WriteLine("Cancelled.");
                    return;
                }
            }

            HttpResponseMessage resp;
            try { resp = await client.DeleteAsync($"/api/v1/me/nodes/{nodeId}"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Node '{node}' removed.");
            Console.ResetColor();
        }, nodeArg, hubOption, forceOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh node blueprints <node>
    // -------------------------------------------------------------------------
    private static Command BuildBlueprintsCommand()
    {
        var cmd = new Command("blueprints", "List blueprints available on a node");

        var nodeArg = new Argument<string>("node", "Node hostname or ID");
        cmd.AddArgument(nodeArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string node, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);
            var nodeId = await ResolveNodeId(client, node);
            if (nodeId == null) { NodeNotFound(node); return; }

            HttpResponseMessage resp;
            try { resp = await client.GetAsync($"/api/v1/me/nodes/{nodeId}/blueprints"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var result = await resp.Content.ReadFromJsonAsync(NodeJsonContext.Default.NodeCommandOutput);
            Console.WriteLine(result?.Output ?? "(no output)");
        }, nodeArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh node up <node> <blueprint> [--name <env-name>]
    // -------------------------------------------------------------------------
    private static Command BuildUpCommand()
    {
        var cmd = new Command("up", "Deploy a blueprint to a node as a new environment");

        var nodeArg = new Argument<string>("node", "Node hostname or ID");
        var blueprintArg = new Argument<string>("blueprint", "Blueprint name (e.g. alpine-minimal)");
        cmd.AddArgument(nodeArg);
        cmd.AddArgument(blueprintArg);

        var nameOption = new Option<string?>(
            aliases: ["--name", "-n"],
            description: "Name for the new environment (default: blueprint name)");
        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(nameOption);
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string node, string blueprint, string? name, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            // Give more time for Docker image pulls
            client.Timeout = TimeSpan.FromSeconds(130);

            var nodeId = await ResolveNodeId(client, node);
            if (nodeId == null) { NodeNotFound(node); return; }

            var envName = name ?? blueprint;

            Console.WriteLine($"🚀 Deploying '{blueprint}' → '{envName}' on {node}...");

            var body = JsonContent.Create(
                new NodeCreateEnvironmentRequest { Blueprint = blueprint, Name = envName },
                NodeJsonContext.Default.NodeCreateEnvironmentRequest);

            HttpResponseMessage resp;
            try { resp = await client.PostAsync($"/api/v1/me/nodes/{nodeId}/environments", body); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var result = await resp.Content.ReadFromJsonAsync(NodeJsonContext.Default.NodeCommandOutput);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(result?.Output ?? "Done.");
            Console.ResetColor();
        }, nodeArg, blueprintArg, nameOption, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // Display helpers
    // -------------------------------------------------------------------------
    private static void PrintNodeTable(List<NodeDetail> nodes)
    {
        // Column widths
        const int hostnameW = 24;
        const int platformW = 9;
        const int statusW   = 8;
        const int ipW       = 16;
        const int cpuW      = 6;
        const int ramW      = 10;
        const int diskW     = 8;
        const int envsW     = 6;
        const int seenW     = 12;

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(
            $"{"HOSTNAME",-hostnameW} {"PLATFORM",-platformW} {"STATUS",-statusW} {"IP",-ipW} {"CPU",-cpuW} {"RAM (GB)",-ramW} {"DISK",-diskW} {"ENVS",-envsW} {"LAST SEEN",-seenW}");
        Console.ResetColor();
        Console.WriteLine(new string('-', hostnameW + platformW + statusW + ipW + cpuW + ramW + diskW + envsW + seenW + 8));

        foreach (var n in nodes)
        {
            Console.ForegroundColor = n.Status == "online" ? ConsoleColor.Green : ConsoleColor.DarkGray;
            var status   = n.Status == "online" ? "online" : "offline";
            var cpu      = n.CpuPercent.HasValue ? $"{n.CpuPercent:F1}%" : "-";
            var ram      = n.RamUsedGb.HasValue && n.RamTotalGb.HasValue
                           ? $"{n.RamUsedGb:F1}/{n.RamTotalGb}" : n.RamTotalGb.HasValue ? $"-/{n.RamTotalGb}" : "-";
            var disk     = n.DiskTotalGb.HasValue ? $"{n.DiskTotalGb}GB" : "-";
            var envs     = n.EnvironmentCount?.ToString() ?? "-";
            var seen     = n.LastSeenAt.HasValue ? FormatAgo(n.LastSeenAt.Value) : "never";

            Console.WriteLine(
                $"{Truncate(n.Hostname, hostnameW - 1),-hostnameW} {n.Platform,-platformW} {status,-statusW} {(n.IpAddress ?? "-"),-ipW} {cpu,-cpuW} {ram,-ramW} {disk,-diskW} {envs,-envsW} {seen,-seenW}");
        }
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"{nodes.Count} node(s)  •  {nodes.Count(n => n.Status == "online")} online");
    }

    private static void PrintNodeDetail(NodeDetail n)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {n.Hostname}");
        Console.ResetColor();
        Console.WriteLine($"  {"ID:",-18} {n.Id}");
        Console.WriteLine($"  {"Platform:",-18} {n.Platform}");
        Console.WriteLine($"  {"Status:",-18} {StatusColored(n.Status)}");
        Console.ResetColor();
        Console.WriteLine($"  {"IP Address:",-18} {n.IpAddress ?? "-"}");
        Console.WriteLine($"  {"Agent Version:",-18} {n.AgentVersion ?? "-"}");
        Console.WriteLine($"  {"Connection:",-18} {n.ConnectionTier ?? "-"}");
        Console.WriteLine();
        Console.WriteLine($"  {"CPU Cores:",-18} {n.CpuCores?.ToString() ?? "-"}");
        if (n.CpuPercent.HasValue)
            Console.WriteLine($"  {"CPU Usage:",-18} {n.CpuPercent:F1}%");
        Console.WriteLine($"  {"RAM (GB):",-18} {(n.RamUsedGb.HasValue ? $"{n.RamUsedGb:F1} / {n.RamTotalGb} GB" : $"{n.RamTotalGb?.ToString() ?? "-"} GB total")}");
        Console.WriteLine($"  {"Disk:",-18} {(n.DiskTotalGb.HasValue ? $"{n.DiskTotalGb} GB" : "-")}");

        if (n.GpuCount is > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"  {"GPU(s):",-18} {n.GpuCount}× {n.GpuModel ?? "unknown"}");
            if (n.GpuMemoryTotalGb.HasValue)
                Console.WriteLine($"  {"GPU VRAM:",-18} {n.GpuMemoryTotalGb} GB");
        }

        Console.WriteLine();
        Console.WriteLine($"  {"Environments:",-18} {n.EnvironmentCount?.ToString() ?? "-"}");
        Console.WriteLine($"  {"Registered:",-18} {n.RegisteredAt:yyyy-MM-dd HH:mm} UTC");
        Console.WriteLine($"  {"Last Seen:",-18} {(n.LastSeenAt.HasValue ? $"{n.LastSeenAt:yyyy-MM-dd HH:mm} UTC ({FormatAgo(n.LastSeenAt.Value)})" : "never")}");
        Console.WriteLine();
    }

    private static void PrintMetrics(NodeMetrics m)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {m.Hostname}  [{StatusColored(m.Status)}]");
        Console.ResetColor();
        Console.WriteLine();

        PrintMetricBar("CPU", m.CpuPercent, 100, "%", m.CpuPercent.HasValue ? $"{m.CpuPercent:F1}%" : null);

        if (m.RamTotalGb.HasValue)
        {
            var ramPct = m.RamUsedGb.HasValue ? (m.RamUsedGb.Value / m.RamTotalGb.Value) * 100 : (double?)null;
            var ramLabel = m.RamUsedGb.HasValue ? $"{m.RamUsedGb:F1} / {m.RamTotalGb} GB" : $"- / {m.RamTotalGb} GB";
            PrintMetricBar("RAM", ramPct, 100, "%", ramLabel);
        }
        else
        {
            Console.WriteLine($"  {"RAM:",-10} -");
        }

        Console.WriteLine($"  {"Disk:",-10} {(m.DiskTotalGb.HasValue ? $"{m.DiskTotalGb} GB total" : "-")}");

        if (m.GpuCount is > 0)
            Console.WriteLine($"  {"GPU:",-10} {m.GpuCount}× {m.GpuModel ?? "unknown"}{(m.GpuMemoryTotalGb.HasValue ? $" ({m.GpuMemoryTotalGb} GB VRAM)" : "")}");

        Console.WriteLine($"  {"Envs:",-10} {m.EnvironmentCount?.ToString() ?? "-"}");
        Console.WriteLine($"  {"Updated:",-10} {(m.LastSeenAt.HasValue ? FormatAgo(m.LastSeenAt.Value) : "never")}");
        Console.WriteLine();
    }

    private static void PrintMetricBar(string label, double? pct, double max, string unit, string? valueLabel)
    {
        const int barWidth = 20;
        var filled = pct.HasValue ? (int)(pct.Value / max * barWidth) : 0;
        var bar = pct.HasValue
            ? "[" + new string('█', filled) + new string('░', barWidth - filled) + "]"
            : "[" + new string('░', barWidth) + "]";

        var color = pct switch
        {
            > 90 => ConsoleColor.Red,
            > 70 => ConsoleColor.Yellow,
            _    => ConsoleColor.Green
        };

        Console.Write($"  {label + ":",-10} ");
        Console.ForegroundColor = pct.HasValue ? color : ConsoleColor.DarkGray;
        Console.Write(bar);
        Console.ResetColor();
        Console.WriteLine($"  {valueLabel ?? "-"}");
    }

    // -------------------------------------------------------------------------
    // Utility helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Accepts hostname or ID prefix. Fetches the node list and resolves to an ID.
    /// </summary>
    private static async Task<string?> ResolveNodeId(HttpClient client, string nameOrId)
    {
        HttpResponseMessage resp;
        try { resp = await client.GetAsync("/api/v1/me/nodes"); }
        catch { return null; }

        if (!resp.IsSuccessStatusCode) return null;

        var list = await resp.Content.ReadFromJsonAsync(NodeJsonContext.Default.NodeListResponse);
        if (list == null) return null;

        // Exact ID match first, then hostname (case-insensitive), then ID prefix
        var node = list.Nodes.FirstOrDefault(n => n.Id == nameOrId)
                ?? list.Nodes.FirstOrDefault(n => n.Hostname.Equals(nameOrId, StringComparison.OrdinalIgnoreCase))
                ?? list.Nodes.FirstOrDefault(n => n.Id.StartsWith(nameOrId, StringComparison.OrdinalIgnoreCase));

        return node?.Id;
    }

    private static (string hubUrl, string? token) GetAuth(string? hubOverride)
    {
        var credService = new CredentialService();
        var creds = credService.Load();

        var hubUrl = hubOverride
            ?? System.Environment.GetEnvironmentVariable("THRESH_HUB_URL")
            ?? creds?.HubUrl
            ?? "https://thresh.io";

        var token = System.Environment.GetEnvironmentVariable("THRESH_CLI_TOKEN")
                 ?? (credService.IsValid(creds) ? creds!.Token : null);

        return (hubUrl, token);
    }

    private static HttpClient CreateClient(string hubUrl, string token)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(hubUrl),
            Timeout = TimeSpan.FromSeconds(15)
        };
        client.DefaultRequestHeaders.Add("User-Agent", "thresh-cli/1.0");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    private static void AuthError()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine("Not authenticated. Run 'thresh auth login' first.");
        Console.ResetColor();
    }

    private static void NodeNotFound(string name)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Node '{name}' not found. Run 'thresh node list' to see connected nodes.");
        Console.ResetColor();
    }

    private static void NetworkError(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Could not reach hub: {ex.Message}");
        Console.ResetColor();
    }

    private static async Task PrintApiError(HttpResponseMessage resp)
    {
        var body = await resp.Content.ReadAsStringAsync();
        Console.ForegroundColor = ConsoleColor.Red;

        // Give a friendlier message for the "not associated with an account" case
        if ((int)resp.StatusCode == 401 && body.Contains("not associated with an account", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Your user account is not linked to a thresh account.");
            Console.Error.WriteLine("Log in to the hub web interface to complete account setup.");
        }
        else
        {
            Console.Error.WriteLine($"Hub returned {(int)resp.StatusCode}: {body}");
        }

        Console.ResetColor();
    }

    private static string StatusColored(string status)
    {
        if (status == "online")
        {
            Console.ForegroundColor = ConsoleColor.Green;
            return "online";
        }
        Console.ForegroundColor = ConsoleColor.DarkGray;
        return status;
    }

    private static string FormatAgo(DateTime utc)
    {
        var ago = DateTime.UtcNow - utc;
        if (ago.TotalSeconds < 90)  return $"{(int)ago.TotalSeconds}s ago";
        if (ago.TotalMinutes < 90)  return $"{(int)ago.TotalMinutes}m ago";
        if (ago.TotalHours < 48)    return $"{(int)ago.TotalHours}h ago";
        return $"{(int)ago.TotalDays}d ago";
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max];
}
