using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Thresh.Services;

/// <summary>
/// Cross-platform GPU detection via nvidia-smi, rocm-smi, and PowerShell/WMI.
/// Returns static capacity (count, model, VRAM) and live utilization.
/// </summary>
public static partial class GpuDetector
{
    public record GpuInfo(int Count, string? Model, int? MemoryTotalGb, double? UtilizationPercent);

    /// <summary>
    /// Detect GPUs and their current utilization. Returns null if no GPUs found.
    /// </summary>
    public static async Task<GpuInfo?> DetectAsync(TimeSpan? timeout = null)
    {
        var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(5));

        // 1) Try NVIDIA (most common for compute workloads)
        var nvidia = await TryNvidiaSmiAsync(cts.Token);
        if (nvidia != null) return nvidia;

        // 2) Try AMD ROCm
        var amd = await TryRocmSmiAsync(cts.Token);
        if (amd != null) return amd;

        // 3) Windows fallback via PowerShell/WMI
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var wmi = await TryWindowsWmiAsync(cts.Token);
            if (wmi != null) return wmi;
        }

        return null;
    }

    // ── NVIDIA ────────────────────────────────────────────────────────────────

    private static async Task<GpuInfo?> TryNvidiaSmiAsync(CancellationToken ct)
    {
        // Query: name, memory.total (MiB), utilization.gpu (%)
        var output = await RunProcessAsync(
            "nvidia-smi",
            "--query-gpu=name,memory.total,utilization.gpu --format=csv,noheader,nounits",
            ct);

        if (string.IsNullOrWhiteSpace(output)) return null;

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0) return null;

        string? model = null;
        int totalMemoryMb = 0;
        double totalUtil = 0;
        int count = 0;

        foreach (var line in lines)
        {
            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 3) continue;

            count++;
            model ??= parts[0]; // take first GPU model name

            if (int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var memMb))
                totalMemoryMb += memMb;

            if (double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var util))
                totalUtil += util;
        }

        if (count == 0) return null;

        return new GpuInfo(
            Count: count,
            Model: model,
            MemoryTotalGb: totalMemoryMb > 0 ? (int)Math.Round(totalMemoryMb / 1024.0) : null,
            UtilizationPercent: Math.Round(totalUtil / count, 1));
    }

    // ── AMD ROCm ──────────────────────────────────────────────────────────────

    private static async Task<GpuInfo?> TryRocmSmiAsync(CancellationToken ct)
    {
        // rocm-smi outputs a table; use --showid, --showuse, --showmeminfo vram flags
        var output = await RunProcessAsync("rocm-smi", "--showid --showuse --showmeminfo vram --csv", ct);
        if (string.IsNullOrWhiteSpace(output)) return null;

        // Parse CSV output from rocm-smi
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length < 2) return null; // need header + at least one row

        // Find column indices from header
        var header = lines[0].Split(',', StringSplitOptions.TrimEntries);
        int gpuIdx = Array.FindIndex(header, h => h.Contains("GPU", StringComparison.OrdinalIgnoreCase));
        int useIdx = Array.FindIndex(header, h => h.Contains("Use", StringComparison.OrdinalIgnoreCase) || h.Contains("GPU use", StringComparison.OrdinalIgnoreCase));
        int memIdx = Array.FindIndex(header, h => h.Contains("VRAM Total", StringComparison.OrdinalIgnoreCase) || h.Contains("vram", StringComparison.OrdinalIgnoreCase));

        int count = 0;
        double totalUtil = 0;
        long totalVramBytes = 0;
        string? model = null;

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',', StringSplitOptions.TrimEntries);
            count++;

            if (useIdx >= 0 && useIdx < cols.Length &&
                double.TryParse(cols[useIdx].TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out var util))
                totalUtil += util;

            if (memIdx >= 0 && memIdx < cols.Length &&
                long.TryParse(cols[memIdx], NumberStyles.Integer, CultureInfo.InvariantCulture, out var vram))
                totalVramBytes += vram;
        }

        if (count == 0) return null;

        // Get model name separately — rocm-smi --showproductname
        var nameOutput = await RunProcessAsync("rocm-smi", "--showproductname", ct);
        if (!string.IsNullOrWhiteSpace(nameOutput))
        {
            var match = ProductNameRegex().Match(nameOutput);
            if (match.Success)
                model = match.Groups[1].Value.Trim();
        }

        // VRAM from rocm-smi is in bytes
        int? memGb = totalVramBytes > 0 ? (int)Math.Round(totalVramBytes / (1024.0 * 1024 * 1024)) : null;

        return new GpuInfo(
            Count: count,
            Model: model,
            MemoryTotalGb: memGb,
            UtilizationPercent: Math.Round(totalUtil / count, 1));
    }

    // ── Windows WMI (via PowerShell) ──────────────────────────────────────────

    private static async Task<GpuInfo?> TryWindowsWmiAsync(CancellationToken ct)
    {
        // Get GPU info via WMI — Win32_VideoController for basic info
        var output = await RunProcessAsync(
            "powershell",
            "-NoProfile -Command \"Get-CimInstance Win32_VideoController | Select-Object Name, AdapterRAM | ConvertTo-Csv -NoTypeInformation\"",
            ct);

        if (string.IsNullOrWhiteSpace(output)) return null;

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length < 2) return null;

        int count = 0;
        string? model = null;
        long totalRam = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',', StringSplitOptions.TrimEntries);
            if (cols.Length < 2) continue;

            var name = cols[0].Trim('"');
            // Skip virtual/basic display adapters
            if (name.Contains("Basic Display", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Microsoft Remote", StringComparison.OrdinalIgnoreCase))
                continue;

            count++;
            model ??= name;

            if (long.TryParse(cols[1].Trim('"'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ram))
                totalRam += ram;
        }

        if (count == 0) return null;

        // WMI AdapterRAM is in bytes
        int? memGb = totalRam > 0 ? (int)Math.Round(totalRam / (1024.0 * 1024 * 1024)) : null;

        // WMI doesn't provide utilization — try nvidia-smi for that if NVIDIA card detected
        double? utilization = null;
        if (model != null && model.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
        {
            var nvResult = await TryNvidiaSmiAsync(ct);
            if (nvResult != null)
                utilization = nvResult.UtilizationPercent;
        }

        return new GpuInfo(
            Count: count,
            Model: model,
            MemoryTotalGb: memGb,
            UtilizationPercent: utilization);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<string?> RunProcessAsync(string fileName, string arguments, CancellationToken ct)
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            proc.Start();
            var output = await proc.StandardOutput.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            return proc.ExitCode == 0 ? output : null;
        }
        catch
        {
            // Tool not installed or not on PATH — that's fine
            return null;
        }
    }

    [GeneratedRegex(@"Card Series:\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex ProductNameRegex();
}
