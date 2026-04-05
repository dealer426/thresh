using System.CommandLine;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Thresh.Services;

// ---------------------------------------------------------------------------
// AOT JSON context for stack API DTOs
// ---------------------------------------------------------------------------
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(StackListResponse))]
[JsonSerializable(typeof(StackResponse))]
[JsonSerializable(typeof(StackServiceDef))]
[JsonSerializable(typeof(StackCreateRequest))]
[JsonSerializable(typeof(ServiceUpdateRequest))]
[JsonSerializable(typeof(StackMessageResponse))]
[JsonSerializable(typeof(List<StackServiceDef>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(List<string>))]
internal partial class StackJsonContext : JsonSerializerContext { }

internal class StackListResponse
{
    public List<StackResponse> Stacks { get; set; } = [];
}

internal class StackResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TargetNodeId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeployedAt { get; set; }
    public List<StackServiceDef> Services { get; set; } = [];
}

internal class StackServiceDef
{
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public List<string> Ports { get; set; } = [];
    public List<string> Volumes { get; set; } = [];
    public Dictionary<string, string> Env { get; set; } = [];
    public List<string> DependsOn { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

internal class StackCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TargetNode { get; set; }
    public List<StackServiceDef> Services { get; set; } = [];
}

internal class ServiceUpdateRequest
{
    public string? Image { get; set; }
}

internal class StackMessageResponse
{
    public string? Message { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// CLI stack commands: up, down, destroy, list, info, update
/// All require a valid CLI session (thresh auth login).
/// </summary>
public static class StackCommands
{
    public static void Register(RootCommand rootCommand)
    {
        var stackCommand = new Command("stack", "Manage multi-service stacks on your nodes");

        stackCommand.AddCommand(BuildUpCommand());
        stackCommand.AddCommand(BuildDownCommand());
        stackCommand.AddCommand(BuildDestroyCommand());
        stackCommand.AddCommand(BuildListCommand());
        stackCommand.AddCommand(BuildInfoCommand());
        stackCommand.AddCommand(BuildUpdateCommand());

        rootCommand.AddCommand(stackCommand);
    }

    // -------------------------------------------------------------------------
    // thresh stack up <file.json>
    // -------------------------------------------------------------------------
    private static Command BuildUpCommand()
    {
        var cmd = new Command("up", "Deploy a stack from a JSON definition file");

        var fileArg = new Argument<FileInfo>("file", "Path to stack definition JSON file");
        cmd.AddArgument(fileArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (FileInfo file, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            if (!file.Exists)
            {
                PrintError($"File not found: {file.FullName}");
                return;
            }

            string json;
            try { json = await File.ReadAllTextAsync(file.FullName); }
            catch (Exception ex) { PrintError($"Could not read file: {ex.Message}"); return; }

            StackCreateRequest? request;
            try
            {
                request = JsonSerializer.Deserialize(json, StackJsonContext.Default.StackCreateRequest);
            }
            catch (Exception ex)
            {
                PrintError($"Invalid stack JSON: {ex.Message}");
                return;
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                PrintError("Stack JSON must have a 'name' field.");
                return;
            }

            Console.WriteLine($"🚀 Deploying stack '{request.Name}'...");
            if (!string.IsNullOrEmpty(request.TargetNode))
                Console.WriteLine($"   Target node: {request.TargetNode}");
            Console.WriteLine($"   Services:    {string.Join(", ", request.Services.Select(s => s.Name))}");
            Console.WriteLine();

            using var client = CreateClient(hubUrl, token);
            client.Timeout = TimeSpan.FromSeconds(60);

            var body = JsonContent.Create(request, StackJsonContext.Default.StackCreateRequest);

            HttpResponseMessage resp;
            try { resp = await client.PostAsync("/api/v1/stacks", body); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var result = await resp.Content.ReadFromJsonAsync(StackJsonContext.Default.StackResponse);
            if (result == null) { PrintError("No response from hub."); return; }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Stack '{result.Name}' created (status: {result.Status})");
            Console.ResetColor();
            Console.WriteLine($"  ID:      {result.Id}");
            Console.WriteLine($"  Node:    {result.TargetNodeId ?? "unassigned"}");
            Console.WriteLine($"  Run 'thresh stack info {result.Name}' to track deployment.");
        }, fileArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh stack down <name>
    // -------------------------------------------------------------------------
    private static Command BuildDownCommand()
    {
        var cmd = new Command("down", "Stop a running stack (keeps volumes)");

        var nameArg = new Argument<string>("name", "Stack name");
        cmd.AddArgument(nameArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string name, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            HttpResponseMessage resp;
            try { resp = await client.DeleteAsync($"/api/v1/stacks/{Uri.EscapeDataString(name)}"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var result = await resp.Content.ReadFromJsonAsync(StackJsonContext.Default.StackMessageResponse);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⏹  {result?.Message ?? $"Stack '{name}' stopped"}");
            Console.ResetColor();
        }, nameArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh stack destroy <name>
    // -------------------------------------------------------------------------
    private static Command BuildDestroyCommand()
    {
        var cmd = new Command("destroy", "Stop a stack and remove all volumes");

        var nameArg = new Argument<string>("name", "Stack name");
        cmd.AddArgument(nameArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        var yesOption = new Option<bool>(["--yes", "-y"], "Skip confirmation prompt");
        cmd.AddOption(hubOption);
        cmd.AddOption(yesOption);

        cmd.SetHandler(async (string name, string? hub, bool yes) =>
        {
            if (!yes)
            {
                Console.Write($"Destroy stack '{name}' and remove all volumes? [y/N] ");
                var answer = Console.ReadLine();
                if (!string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Aborted.");
                    return;
                }
            }

            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            HttpResponseMessage resp;
            try { resp = await client.DeleteAsync($"/api/v1/stacks/{Uri.EscapeDataString(name)}?destroy=true"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var result = await resp.Content.ReadFromJsonAsync(StackJsonContext.Default.StackMessageResponse);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"💥 {result?.Message ?? $"Stack '{name}' destroyed"}");
            Console.ResetColor();
        }, nameArg, hubOption, yesOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh stack list
    // -------------------------------------------------------------------------
    private static Command BuildListCommand()
    {
        var cmd = new Command("list", "List all stacks in your account");

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            HttpResponseMessage resp;
            try { resp = await client.GetAsync("/api/v1/stacks"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var result = await resp.Content.ReadFromJsonAsync(StackJsonContext.Default.StackListResponse);
            if (result == null || result.Stacks.Count == 0)
            {
                Console.WriteLine("No stacks found. Use 'thresh stack up <file.json>' to deploy one.");
                return;
            }

            PrintStackTable(result.Stacks);
        }, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh stack info <name>
    // -------------------------------------------------------------------------
    private static Command BuildInfoCommand()
    {
        var cmd = new Command("info", "Show per-service status for a stack");

        var nameArg = new Argument<string>("name", "Stack name");
        cmd.AddArgument(nameArg);

        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string name, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            HttpResponseMessage resp;
            try { resp = await client.GetAsync($"/api/v1/stacks/{Uri.EscapeDataString(name)}"); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            var stack = await resp.Content.ReadFromJsonAsync(StackJsonContext.Default.StackResponse);
            if (stack == null) { PrintError("No response from hub."); return; }

            PrintStackDetail(stack);
        }, nameArg, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // thresh stack update <name> --service <svc> --image <img>
    // -------------------------------------------------------------------------
    private static Command BuildUpdateCommand()
    {
        var cmd = new Command("update", "Rolling update — change the image for a service in a stack");

        var nameArg = new Argument<string>("name", "Stack name");
        cmd.AddArgument(nameArg);

        var serviceOption = new Option<string>("--service", "Service name to update") { IsRequired = true };
        var imageOption = new Option<string>("--image", "New image (e.g. docker:my-app:v2)") { IsRequired = true };
        var hubOption = new Option<string?>("--hub", "Hub URL (overrides stored credentials)");
        cmd.AddOption(serviceOption);
        cmd.AddOption(imageOption);
        cmd.AddOption(hubOption);

        cmd.SetHandler(async (string name, string service, string image, string? hub) =>
        {
            var (hubUrl, token) = GetAuth(hub);
            if (token == null) { AuthError(); return; }

            using var client = CreateClient(hubUrl, token);

            var body = JsonContent.Create(
                new ServiceUpdateRequest { Image = image },
                StackJsonContext.Default.ServiceUpdateRequest);

            HttpResponseMessage resp;
            try { resp = await client.PatchAsync($"/api/v1/stacks/{Uri.EscapeDataString(name)}/services/{Uri.EscapeDataString(service)}", body); }
            catch (Exception ex) { NetworkError(ex); return; }

            if (!resp.IsSuccessStatusCode) { await PrintApiError(resp); return; }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"🔄 Rolling update started: '{service}' → {image}");
            Console.WriteLine($"   Run 'thresh stack info {name}' to track progress.");
            Console.ResetColor();
        }, nameArg, serviceOption, imageOption, hubOption);

        return cmd;
    }

    // -------------------------------------------------------------------------
    // Display helpers
    // -------------------------------------------------------------------------
    private static void PrintStackTable(List<StackResponse> stacks)
    {
        const int nameW   = 24;
        const int statusW = 12;
        const int nodeW   = 24;
        const int svcsW   = 8;
        const int ageW    = 12;

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{"NAME",-nameW} {"STATUS",-statusW} {"NODE",-nodeW} {"SVCS",-svcsW} {"AGE",-ageW}");
        Console.ResetColor();
        Console.WriteLine(new string('-', nameW + statusW + nodeW + svcsW + ageW + 4));

        foreach (var s in stacks)
        {
            Console.ForegroundColor = StatusColor(s.Status);
            var node = Truncate(s.TargetNodeId ?? "-", nodeW - 1);
            var age  = FormatAgo(s.CreatedAt);
            Console.WriteLine($"{Truncate(s.Name, nameW - 1),-nameW} {s.Status,-statusW} {node,-nodeW} {s.Services.Count,-svcsW} {age,-ageW}");
        }

        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"{stacks.Count} stack(s)  •  {stacks.Count(s => s.Status == "running")} running");
    }

    private static void PrintStackDetail(StackResponse s)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {s.Name}");
        Console.ResetColor();
        Console.WriteLine($"  {"ID:",-18} {s.Id}");
        Console.WriteLine($"  {"Status:",-18} ");
        Console.ForegroundColor = StatusColor(s.Status);
        Console.Write(s.Status);
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"  {"Node:",-18} {s.TargetNodeId ?? "unassigned"}");
        Console.WriteLine($"  {"Created:",-18} {s.CreatedAt:yyyy-MM-dd HH:mm} UTC");
        if (s.DeployedAt.HasValue)
            Console.WriteLine($"  {"Deployed:",-18} {s.DeployedAt:yyyy-MM-dd HH:mm} UTC");
        if (!string.IsNullOrEmpty(s.ErrorMessage))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  {"Error:",-18} {s.ErrorMessage}");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("  Services:");
        Console.WriteLine($"  {"  NAME",-20} {"IMAGE",-40} {"STATUS",-12} {"PORTS"}");
        Console.WriteLine("  " + new string('-', 90));

        foreach (var svc in s.Services)
        {
            Console.ForegroundColor = StatusColor(svc.Status);
            var ports = svc.Ports.Count > 0 ? string.Join(", ", svc.Ports) : "-";
            var img   = Truncate(svc.Image, 38);
            Console.WriteLine($"  {Truncate(svc.Name, 18),-20} {img,-40} {svc.Status,-12} {ports}");
            if (!string.IsNullOrEmpty(svc.ErrorMessage))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"    ↳ error: {svc.ErrorMessage}");
            }
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    // -------------------------------------------------------------------------
    // Utility helpers
    // -------------------------------------------------------------------------
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

    private static void PrintError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(msg);
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

        switch ((int)resp.StatusCode)
        {
            case 401 when body.Contains("not associated with an account", StringComparison.OrdinalIgnoreCase):
                Console.Error.WriteLine("Your user account is not linked to a thresh account.");
                Console.Error.WriteLine("Log in to the hub web interface to complete account setup.");
                break;
            case 401:
                Console.Error.WriteLine("Authentication failed. Run 'thresh auth login' to refresh your session.");
                break;
            case 403:
                Console.Error.WriteLine("Permission denied. Your account may not have access to this stack.");
                break;
            case 404:
                Console.Error.WriteLine("Stack not found. Use 'thresh stack list' to see available stacks.");
                break;
            case 409:
                Console.Error.WriteLine($"Conflict: {body}");
                break;
            default:
                Console.Error.WriteLine($"Hub returned {(int)resp.StatusCode}: {body}");
                break;
        }

        Console.ResetColor();
    }

    private static ConsoleColor StatusColor(string status) => status switch
    {
        "running"   => ConsoleColor.Green,
        "deploying" => ConsoleColor.Cyan,
        "stopped"   => ConsoleColor.DarkGray,
        "error"     => ConsoleColor.Red,
        _           => ConsoleColor.Yellow
    };

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
