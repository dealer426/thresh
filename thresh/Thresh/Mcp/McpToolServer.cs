using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Thresh.Mcp.Models;

namespace Thresh.Mcp;

/// <summary>
/// CC-4 — agent-side MCP tool surface for the v1.6 conductor.
///
/// Whereas <see cref="StdioMcpServer"/> exposes fleet/blueprint operations to
/// human-facing MCP clients (VS Code, etc.), this server exposes the low-level
/// node primitives the Copilot conductor needs:
///   • fs.read / fs.list           — inspect filesystem
///   • system.info / system.metrics — host/container introspection
///   • proc.exec                   — run a shell command (gated by approval)
///   • pkg.list                    — installed package inventory
///   • git.status                  — VCS state of a working dir
///
/// The proxy (`thresh-mcp-proxy`) routes Copilot's tool calls through the hub
/// to this server. v1.6 ships read-mostly tools; mutating tools fail closed
/// unless the caller carries an approval token from the hub's ApprovalGateway.
/// </summary>
public sealed class McpToolServer
{
    private readonly CancellationTokenSource _cts = new();

    /// <summary>Catalogue of v1.6 conductor-facing tools.</summary>
    public IReadOnlyList<Tool> ListTools() =>
    [
        new Tool {
            Name = "fs.read",
            Description = "Read a file from the node filesystem.",
            InputSchema = new { type = "object", properties = new { path = new { type = "string" }, maxBytes = new { type = "integer" } }, required = new[] { "path" } }
        },
        new Tool {
            Name = "fs.list",
            Description = "List directory contents on the node filesystem.",
            InputSchema = new { type = "object", properties = new { path = new { type = "string" } }, required = new[] { "path" } }
        },
        new Tool {
            Name = "system.info",
            Description = "Return OS, CPU, memory, and uptime info for the node host.",
            InputSchema = new { type = "object", properties = new { } }
        },
        new Tool {
            Name = "system.metrics",
            Description = "Return current CPU/mem/disk metrics for the node host.",
            InputSchema = new { type = "object", properties = new { } }
        },
        new Tool {
            Name = "proc.exec",
            Description = "Execute a shell command on the node. Requires an approval token (CC-7).",
            InputSchema = new { type = "object", properties = new { command = new { type = "string" }, approvalToken = new { type = "string" } }, required = new[] { "command", "approvalToken" } }
        },
        new Tool {
            Name = "pkg.list",
            Description = "List installed packages (dpkg/rpm/apk depending on platform).",
            InputSchema = new { type = "object", properties = new { } }
        },
        new Tool {
            Name = "git.status",
            Description = "Return git status for a working directory.",
            InputSchema = new { type = "object", properties = new { path = new { type = "string" } }, required = new[] { "path" } }
        }
    ];

    /// <summary>Dispatch a single MCP tool call.</summary>
    public async Task<ToolCallResponse> CallAsync(string name, JsonElement? args, CancellationToken ct = default)
    {
        try
        {
            return name switch
            {
                "fs.read"        => await FsReadAsync(args, ct),
                "fs.list"        => FsList(args),
                "system.info"    => SystemInfo(),
                "system.metrics" => SystemMetrics(),
                "proc.exec"      => await ProcExecAsync(args, ct),
                "pkg.list"       => await PkgListAsync(ct),
                "git.status"     => await GitStatusAsync(args, ct),
                _                => ToolCallResponse.Error($"unknown tool: {name}")
            };
        }
        catch (Exception ex)
        {
            return ToolCallResponse.Error($"{name} failed: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Tool implementations
    // -------------------------------------------------------------------------
    private static async Task<ToolCallResponse> FsReadAsync(JsonElement? args, CancellationToken ct)
    {
        var path = RequiredString(args, "path");
        var maxBytes = OptionalInt(args, "maxBytes") ?? 64 * 1024;
        if (!File.Exists(path)) return ToolCallResponse.Error($"file not found: {path}");
        var fi = new FileInfo(path);
        if (fi.Length > maxBytes) return ToolCallResponse.Error($"file too large ({fi.Length} > {maxBytes} bytes); pass a higher maxBytes");
        var bytes = await File.ReadAllBytesAsync(path, ct);
        return ToolCallResponse.Success(Encoding.UTF8.GetString(bytes));
    }

    private static ToolCallResponse FsList(JsonElement? args)
    {
        var path = RequiredString(args, "path");
        if (!Directory.Exists(path)) return ToolCallResponse.Error($"directory not found: {path}");
        var entries = Directory.EnumerateFileSystemEntries(path)
            .Select(e =>
            {
                var info = new FileInfo(e);
                var isDir = (info.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
                return new { name = Path.GetFileName(e), type = isDir ? "dir" : "file", size = isDir ? 0 : info.Length };
            })
            .ToList();
        return ToolCallResponse.Success(JsonSerializer.Serialize(entries));
    }

    private static ToolCallResponse SystemInfo()
    {
        var info = new
        {
            os = Environment.OSVersion.ToString(),
            machine = Environment.MachineName,
            user = Environment.UserName,
            cpus = Environment.ProcessorCount,
            workingSet = Environment.WorkingSet,
            uptime = Environment.TickCount64 / 1000
        };
        return ToolCallResponse.Success(JsonSerializer.Serialize(info));
    }

    private static ToolCallResponse SystemMetrics()
    {
        // Lightweight metrics; richer telemetry comes from the existing MetricsService.
        var p = Process.GetCurrentProcess();
        var info = new
        {
            processCount = Process.GetProcesses().Length,
            agentRss = p.WorkingSet64,
            agentCpuMs = (long)p.TotalProcessorTime.TotalMilliseconds
        };
        return ToolCallResponse.Success(JsonSerializer.Serialize(info));
    }

    private static async Task<ToolCallResponse> ProcExecAsync(JsonElement? args, CancellationToken ct)
    {
        var cmd = RequiredString(args, "command");
        var approval = RequiredString(args, "approvalToken");
        if (string.IsNullOrWhiteSpace(approval))
            return ToolCallResponse.Error("proc.exec requires an approvalToken (see CC-7 ApprovalGateway).");

        // v1.6 placeholder: real verification of the approval token lives in CC-7.
        // For now, gate on token-presence so the surface compiles and is testable.
        var psi = new ProcessStartInfo(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            OperatingSystem.IsWindows() ? $"/c {cmd}" : $"-c \"{cmd.Replace("\"", "\\\"")}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var proc = Process.Start(psi)!;
        var stdout = await proc.StandardOutput.ReadToEndAsync(ct);
        var stderr = await proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        var result = new { exitCode = proc.ExitCode, stdout, stderr };
        return ToolCallResponse.Success(JsonSerializer.Serialize(result));
    }

    private static async Task<ToolCallResponse> PkgListAsync(CancellationToken ct)
    {
        // Best-effort: try dpkg, then rpm, then apk. On Windows return winget list.
        string? mgrCmd;
        if (OperatingSystem.IsWindows()) mgrCmd = "winget list";
        else if (File.Exists("/usr/bin/dpkg")) mgrCmd = "dpkg -l";
        else if (File.Exists("/usr/bin/rpm")) mgrCmd = "rpm -qa";
        else if (File.Exists("/sbin/apk")) mgrCmd = "apk list --installed";
        else return ToolCallResponse.Error("no supported package manager found");

        var psi = new ProcessStartInfo(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            OperatingSystem.IsWindows() ? $"/c {mgrCmd}" : $"-c \"{mgrCmd}\"")
        { RedirectStandardOutput = true, UseShellExecute = false };
        using var proc = Process.Start(psi)!;
        var stdout = await proc.StandardOutput.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        return ToolCallResponse.Success(stdout);
    }

    private static async Task<ToolCallResponse> GitStatusAsync(JsonElement? args, CancellationToken ct)
    {
        var path = RequiredString(args, "path");
        if (!Directory.Exists(path)) return ToolCallResponse.Error($"directory not found: {path}");
        var psi = new ProcessStartInfo("git", "status --porcelain=v1 --branch")
        {
            WorkingDirectory = path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var proc = Process.Start(psi)!;
        var stdout = await proc.StandardOutput.ReadToEndAsync(ct);
        var stderr = await proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        if (proc.ExitCode != 0) return ToolCallResponse.Error(stderr);
        return ToolCallResponse.Success(stdout);
    }

    // -------------------------------------------------------------------------
    // Argument helpers
    // -------------------------------------------------------------------------
    private static string RequiredString(JsonElement? args, string name)
    {
        if (args is not { } el || el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(name, out var p))
            throw new ArgumentException($"missing required argument: {name}");
        return p.GetString() ?? throw new ArgumentException($"argument {name} must be a string");
    }

    private static int? OptionalInt(JsonElement? args, string name)
    {
        if (args is not { } el || el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(name, out var p))
            return null;
        return p.ValueKind == JsonValueKind.Number ? p.GetInt32() : null;
    }

    public void Stop() => _cts.Cancel();
}
