using System.CommandLine;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Thresh.Services;

// ---------------------------------------------------------------------------
// AOT JSON context
// ---------------------------------------------------------------------------
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ClusterListResponse))]
[JsonSerializable(typeof(ClusterDetail))]
[JsonSerializable(typeof(ClusterSummary))]
[JsonSerializable(typeof(ClusterActionResponse))]
[JsonSerializable(typeof(CreateClusterPayload))]
[JsonSerializable(typeof(AddNodePayload))]
internal partial class ClusterJsonContext : JsonSerializerContext { }

internal class ClusterListResponse
{
    public List<ClusterSummary> Clusters { get; set; } = [];
}

internal class ClusterSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int NodeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

internal class ClusterDetail
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ClusterNodeInfo> Nodes { get; set; } = [];
}

internal class ClusterNodeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public int? CpuCores { get; set; }
    public double? CpuPercent { get; set; }
    public int? RamTotalGb { get; set; }
    public double? RamUsedGb { get; set; }
    public int? DiskTotalGb { get; set; }
    public int? EnvironmentCount { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

internal class ClusterActionResponse
{
    public string? Message { get; set; }
    public string? Error { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
}

internal class CreateClusterPayload
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

internal class AddNodePayload
{
    public string NodeId { get; set; } = string.Empty;
}

/// <summary>
/// CLI cluster commands: list, create, info, add-node, remove-node, delete
/// </summary>
public static class ClusterCommands
{
    public static void Register(RootCommand rootCommand)
    {
        var clusterCommand = new Command("cluster", "Manage clusters of nodes in your thresh-hub account");

        clusterCommand.AddCommand(BuildListCommand());
        clusterCommand.AddCommand(BuildCreateCommand());
        clusterCommand.AddCommand(BuildInfoCommand());
        clusterCommand.AddCommand(BuildAddNodeCommand());
        clusterCommand.AddCommand(BuildRemoveNodeCommand());
        clusterCommand.AddCommand(BuildDeleteCommand());

        rootCommand.AddCommand(clusterCommand);
    }

    // -------------------------------------------------------------------------
    // thresh cluster list
    // -------------------------------------------------------------------------
    private static Command BuildListCommand()
    {
        var cmd = new Command("list", "List all clusters in your account");
        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            HttpResponseMessage resp;
            try { resp = await client.GetAsync("/api/v1/me/clusters"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var result = await resp.Content.ReadFromJsonAsync(ClusterJsonContext.Default.ClusterListResponse);
            if (result == null || result.Clusters.Count == 0)
            {
                Console.WriteLine("No clusters yet. Create one with: thresh cluster create <name>");
                return;
            }

            PrintClusterTable(result.Clusters);
        }, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh cluster create <name> [--description <desc>]
    // -------------------------------------------------------------------------
    private static Command BuildCreateCommand()
    {
        var cmd = new Command("create", "Create a new cluster");
        var nameArg = new Argument<string>("name", "Cluster name");
        cmd.AddArgument(nameArg);

        var descOption = new Option<string?>("--description", "Optional description");
        cmd.AddOption(descOption);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string name, string? description, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            var payload = new CreateClusterPayload { Name = name, Description = description };
            var json = JsonSerializer.Serialize(payload, ClusterJsonContext.Default.CreateClusterPayload);

            HttpResponseMessage resp;
            try { resp = await client.PostAsync("/api/v1/me/clusters", new StringContent(json, Encoding.UTF8, "application/json")); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Cluster '{name}' created.");
            Console.ResetColor();
            Console.WriteLine($"   Add nodes with: thresh cluster add-node {name} <node>");
        }, nameArg, descOption, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh cluster info <cluster>
    // -------------------------------------------------------------------------
    private static Command BuildInfoCommand()
    {
        var cmd = new Command("info", "Show cluster details and live node metrics");
        var clusterArg = new Argument<string>("cluster", "Cluster name or ID");
        cmd.AddArgument(clusterArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string cluster, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            var clusterId = await ResolveClusterId(client, cluster);
            if (clusterId == null) { ClusterNotFound(cluster); return; }

            HttpResponseMessage resp;
            try { resp = await client.GetAsync($"/api/v1/me/clusters/{clusterId}"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var detail = await resp.Content.ReadFromJsonAsync(ClusterJsonContext.Default.ClusterDetail);
            if (detail == null) return;

            PrintClusterDetail(detail);
        }, clusterArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh cluster add-node <cluster> <node>
    // -------------------------------------------------------------------------
    private static Command BuildAddNodeCommand()
    {
        var cmd = new Command("add-node", "Add a node to a cluster");
        var clusterArg = new Argument<string>("cluster", "Cluster name or ID");
        var nodeArg = new Argument<string>("node", "Node hostname or ID");
        cmd.AddArgument(clusterArg);
        cmd.AddArgument(nodeArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string cluster, string node, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            var clusterId = await ResolveClusterId(client, cluster);
            if (clusterId == null) { ClusterNotFound(cluster); return; }

            var payload = new AddNodePayload { NodeId = node };
            var json = JsonSerializer.Serialize(payload, ClusterJsonContext.Default.AddNodePayload);

            HttpResponseMessage resp;
            try { resp = await client.PostAsync($"/api/v1/me/clusters/{clusterId}/nodes", new StringContent(json, Encoding.UTF8, "application/json")); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Node '{node}' added to cluster '{cluster}'.");
            Console.ResetColor();
        }, clusterArg, nodeArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh cluster remove-node <cluster> <node>
    // -------------------------------------------------------------------------
    private static Command BuildRemoveNodeCommand()
    {
        var cmd = new Command("remove-node", "Remove a node from a cluster");
        var clusterArg = new Argument<string>("cluster", "Cluster name or ID");
        var nodeArg = new Argument<string>("node", "Node hostname or ID");
        cmd.AddArgument(clusterArg);
        cmd.AddArgument(nodeArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string cluster, string node, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            var clusterId = await ResolveClusterId(client, cluster);
            if (clusterId == null) { ClusterNotFound(cluster); return; }

            // Resolve node to ID for the DELETE URL
            var nodeId = await ResolveNodeId(client, node);
            if (nodeId == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Node '{node}' not found. Run 'thresh node list' to see connected nodes.");
                Console.ResetColor();
                return;
            }

            HttpResponseMessage resp;
            try { resp = await client.DeleteAsync($"/api/v1/me/clusters/{clusterId}/nodes/{nodeId}"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Node '{node}' removed from cluster '{cluster}'.");
            Console.ResetColor();
        }, clusterArg, nodeArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh cluster delete <cluster> [--force]
    // -------------------------------------------------------------------------
    private static Command BuildDeleteCommand()
    {
        var cmd = new Command("delete", "Delete a cluster (nodes are not affected)");
        var clusterArg = new Argument<string>("cluster", "Cluster name or ID");
        cmd.AddArgument(clusterArg);

        var forceOption = new Option<bool>("--force", "Skip confirmation prompt");
        cmd.AddOption(forceOption);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string cluster, bool force, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            var clusterId = await ResolveClusterId(client, cluster);
            if (clusterId == null) { ClusterNotFound(cluster); return; }

            if (!force)
            {
                Console.Write($"Delete cluster '{cluster}'? Nodes will not be deleted. [y/N] ");
                var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (answer != "y" && answer != "yes")
                {
                    Console.WriteLine("Cancelled.");
                    return;
                }
            }

            HttpResponseMessage resp;
            try { resp = await client.DeleteAsync($"/api/v1/me/clusters/{clusterId}"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Cluster '{cluster}' deleted.");
            Console.ResetColor();
        }, clusterArg, forceOption, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // Display helpers
    // -------------------------------------------------------------------------
    private static void PrintClusterTable(List<ClusterSummary> clusters)
    {
        const int nameW  = 24;
        const int descW  = 32;
        const int nodesW = 8;
        const int dateW  = 14;

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{"NAME",-nameW} {"DESCRIPTION",-descW} {"NODES",-nodesW} {"CREATED",-dateW}");
        Console.ResetColor();
        Console.WriteLine(new string('-', nameW + descW + nodesW + dateW + 3));

        foreach (var c in clusters)
        {
            Console.WriteLine(
                $"{Truncate(c.Name, nameW - 1),-nameW} " +
                $"{Truncate(c.Description ?? "-", descW - 1),-descW} " +
                $"{c.NodeCount,-nodesW} " +
                $"{c.CreatedAt:yyyy-MM-dd}");
        }

        Console.WriteLine();
        Console.WriteLine($"{clusters.Count} cluster(s)");
    }

    private static void PrintClusterDetail(ClusterDetail c)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {c.Name}");
        Console.ResetColor();

        if (!string.IsNullOrEmpty(c.Description))
            Console.WriteLine($"  {"Description:",-18} {c.Description}");

        Console.WriteLine($"  {"ID:",-18} {c.Id}");
        Console.WriteLine($"  {"Created:",-18} {c.CreatedAt:yyyy-MM-dd HH:mm} UTC");
        Console.WriteLine($"  {"Nodes:",-18} {c.Nodes.Count}");

        if (c.Nodes.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("  No nodes in this cluster. Add one with:");
            Console.WriteLine($"    thresh cluster add-node {c.Name} <node>");
            Console.WriteLine();
            return;
        }

        Console.WriteLine();

        // Node metrics table
        const int hostnameW = 22;
        const int platformW = 9;
        const int statusW   = 8;
        const int cpuW      = 8;
        const int ramW      = 12;
        const int diskW     = 8;
        const int seenW     = 12;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            $"  {"HOSTNAME",-hostnameW} {"PLATFORM",-platformW} {"STATUS",-statusW} {"CPU",-cpuW} {"RAM (GB)",-ramW} {"DISK",-diskW} {"LAST SEEN",-seenW}");
        Console.ResetColor();
        Console.WriteLine("  " + new string('-', hostnameW + platformW + statusW + cpuW + ramW + diskW + seenW + 6));

        foreach (var n in c.Nodes)
        {
            Console.ForegroundColor = n.Status == "online" ? ConsoleColor.Green : ConsoleColor.DarkGray;

            var cpu  = n.CpuPercent.HasValue ? $"{n.CpuPercent:F1}%" : "-";
            var ram  = n.RamUsedGb.HasValue && n.RamTotalGb.HasValue
                       ? $"{n.RamUsedGb:F1}/{n.RamTotalGb}" : n.RamTotalGb.HasValue ? $"-/{n.RamTotalGb}" : "-";
            var disk = n.DiskTotalGb.HasValue ? $"{n.DiskTotalGb}GB" : "-";
            var seen = n.LastSeenAt.HasValue ? FormatAgo(n.LastSeenAt.Value) : "never";

            Console.WriteLine(
                $"  {Truncate(n.Hostname, hostnameW - 1),-hostnameW} " +
                $"{n.Platform,-platformW} " +
                $"{n.Status,-statusW} " +
                $"{cpu,-cpuW} " +
                $"{ram,-ramW} " +
                $"{disk,-diskW} " +
                $"{seen,-seenW}");
        }

        Console.ResetColor();

        // Aggregate summary
        var online  = c.Nodes.Count(n => n.Status == "online");
        var avgCpu  = c.Nodes.Where(n => n.CpuPercent.HasValue).Select(n => n.CpuPercent!.Value).ToList();
        var totalRam = c.Nodes.Where(n => n.RamTotalGb.HasValue).Sum(n => n.RamTotalGb!.Value);
        var usedRam  = c.Nodes.Where(n => n.RamUsedGb.HasValue).Sum(n => n.RamUsedGb!.Value);

        Console.WriteLine();
        Console.WriteLine($"  {online}/{c.Nodes.Count} online" +
            (avgCpu.Count > 0 ? $"  •  avg CPU {avgCpu.Average():F1}%" : "") +
            (totalRam > 0 ? $"  •  RAM {usedRam:F1}/{totalRam} GB" : ""));
        Console.WriteLine();
    }

    // -------------------------------------------------------------------------
    // Utility helpers
    // -------------------------------------------------------------------------
    private static async Task<string?> ResolveClusterId(HttpClient client, string nameOrId)
    {
        HttpResponseMessage resp;
        try { resp = await client.GetAsync("/api/v1/me/clusters"); }
        catch { return null; }

        if (!resp.IsSuccessStatusCode) return null;

        var list = await resp.Content.ReadFromJsonAsync(ClusterJsonContext.Default.ClusterListResponse);
        if (list == null) return null;

        var cluster = list.Clusters.FirstOrDefault(c => c.Id == nameOrId)
                   ?? list.Clusters.FirstOrDefault(c => c.Name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase))
                   ?? list.Clusters.FirstOrDefault(c => c.Id.StartsWith(nameOrId, StringComparison.OrdinalIgnoreCase));

        return cluster?.Id;
    }

    private static async Task<string?> ResolveNodeId(HttpClient client, string nameOrId)
    {
        HttpResponseMessage resp;
        try { resp = await client.GetAsync("/api/v1/me/nodes"); }
        catch { return null; }

        if (!resp.IsSuccessStatusCode) return null;

        var list = await resp.Content.ReadFromJsonAsync(NodeJsonContext.Default.NodeListResponse);
        if (list == null) return null;

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

    private static void ClusterNotFound(string name)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Cluster '{name}' not found. Run 'thresh cluster list' to see your clusters.");
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

    private static string FormatAgo(DateTime utc)
    {
        var ago = DateTime.UtcNow - utc;
        if (ago.TotalSeconds < 90)  return $"{(int)ago.TotalSeconds}s ago";
        if (ago.TotalMinutes < 90)  return $"{(int)ago.TotalMinutes}m ago";
        if (ago.TotalHours < 48)    return $"{(int)ago.TotalHours}h ago";
        return $"{(int)ago.TotalDays}d ago";
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
