using System.Diagnostics;
using System.Runtime.InteropServices;
using Thresh.Models;
using Thresh.Utilities;

namespace Thresh.Services;

/// <summary>
/// Service for collecting cross-platform system metrics
/// </summary>
public class MetricsService
{
    private readonly IContainerService _containerService;

    public MetricsService(IContainerService containerService)
    {
        _containerService = containerService;
    }

    /// <summary>
    /// Collect current host metrics
    /// </summary>
    public async Task<HostMetrics> CollectMetricsAsync()
    {
        var metrics = new HostMetrics
        {
            Hostname = System.Environment.MachineName,
            Platform = _containerService.Platform,
            Runtime = _containerService.RuntimeName,
            CpuCores = System.Environment.ProcessorCount,
            Timestamp = DateTime.UtcNow
        };

        // Get runtime info
        var runtimeInfo = await _containerService.GetRuntimeInfoAsync();
        metrics.RuntimeVersion = runtimeInfo.Version;
        metrics.Containers = runtimeInfo.ContainerCount;

        // Collect CPU metrics
        metrics.CpuPercent = await GetCpuUsageAsync();

        // Collect memory metrics
        var (memUsed, memTotal) = await GetMemoryMetricsAsync();
        metrics.MemoryUsedGb = memUsed;
        metrics.MemoryTotalGb = memTotal;

        // Collect storage metrics
        var (storageFree, storageTotal) = await GetStorageMetricsAsync();
        metrics.StorageFreeGb = storageFree;
        metrics.StorageTotalGb = storageTotal;

        // Collect uptime (platform-specific)
        metrics.UptimeSeconds = await GetUptimeSecondsAsync();

        // Collect metadata
        metrics.Metadata = new Dictionary<string, string>
        {
            ["os_description"] = RuntimeInformation.OSDescription,
            ["os_architecture"] = RuntimeInformation.OSArchitecture.ToString(),
            ["framework_description"] = RuntimeInformation.FrameworkDescription,
            ["process_architecture"] = RuntimeInformation.ProcessArchitecture.ToString()
        };

        return metrics;
    }

    /// <summary>
    /// Get CPU usage percentage (cross-platform)
    /// </summary>
    private async Task<double> GetCpuUsageAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await GetWindowsCpuUsageAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await GetLinuxCpuUsageAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await GetMacOSCpuUsageAsync();
            }

            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    private async Task<double> GetWindowsCpuUsageAsync()
    {
        try
        {
            // Use wmic on Windows
            var result = await ProcessHelper.ExecuteAsync("wmic", "cpu", "get", "loadpercentage");
            if (result.IsSuccess && result.Output.Count >= 2)
            {
                var line = result.Output[1].Trim();
                if (double.TryParse(line, out var cpuPercent))
                {
                    return cpuPercent;
                }
            }
        }
        catch
        {
            // Fallback: Use PerformanceCounter if available
        }

        return 0.0;
    }

    private async Task<double> GetLinuxCpuUsageAsync()
    {
        try
        {
            // Read /proc/stat for CPU usage
            if (File.Exists("/proc/stat"))
            {
                var lines = await File.ReadAllLinesAsync("/proc/stat");
                var cpuLine = lines.FirstOrDefault(l => l.StartsWith("cpu "));
                
                if (cpuLine != null)
                {
                    var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5)
                    {
                        var user = long.Parse(parts[1]);
                        var nice = long.Parse(parts[2]);
                        var system = long.Parse(parts[3]);
                        var idle = long.Parse(parts[4]);
                        var total = user + nice + system + idle;
                        var used = total - idle;
                        
                        // This is a snapshot; for accurate % we'd need two samples
                        // For now, return a rough estimate
                        return total > 0 ? (double)used / total * 100 : 0;
                    }
                }
            }
        }
        catch
        {
            // Ignore
        }

        return 0.0;
    }

    private async Task<double> GetMacOSCpuUsageAsync()
    {
        try
        {
            // Use top command
            var result = await ProcessHelper.ExecuteAsync("top", "-l", "1", "-n", "0");
            if (result.IsSuccess)
            {
                foreach (var line in result.Output)
                {
                    if (line.Contains("CPU usage"))
                    {
                        // Parse line like "CPU usage: 10.5% user, 5.2% sys, 84.3% idle"
                        var parts = line.Split(',');
                        double totalUsed = 0;
                        
                        foreach (var part in parts)
                        {
                            if (!part.Contains("idle"))
                            {
                                var percentIndex = part.IndexOf('%');
                                if (percentIndex > 0)
                                {
                                    var valueStr = part.Substring(0, percentIndex).Trim();
                                    valueStr = valueStr.Split(' ').Last();
                                    if (double.TryParse(valueStr, out var value))
                                    {
                                        totalUsed += value;
                                    }
                                }
                            }
                        }
                        
                        return totalUsed;
                    }
                }
            }
        }
        catch
        {
            // Ignore
        }

        return 0.0;
    }

    /// <summary>
    /// Get memory metrics (used, total) in GB
    /// </summary>
    private async Task<(double used, double total)> GetMemoryMetricsAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await GetWindowsMemoryAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await GetLinuxMemoryAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await GetMacOSMemoryAsync();
            }
        }
        catch
        {
            // Ignore
        }

        return (0, 0);
    }

    private async Task<(double used, double total)> GetWindowsMemoryAsync()
    {
        try
        {
            var result = await ProcessHelper.ExecuteAsync("wmic", "OS", "get", "FreePhysicalMemory,TotalVisibleMemorySize", "/Value");
            if (result.IsSuccess)
            {
                long free = 0, total = 0;
                
                foreach (var line in result.Output)
                {
                    if (line.StartsWith("FreePhysicalMemory="))
                    {
                        free = long.Parse(line.Split('=')[1].Trim());
                    }
                    else if (line.StartsWith("TotalVisibleMemorySize="))
                    {
                        total = long.Parse(line.Split('=')[1].Trim());
                    }
                }
                
                // Values are in KB, convert to GB
                var totalGb = total / 1024.0 / 1024.0;
                var freeGb = free / 1024.0 / 1024.0;
                var usedGb = totalGb - freeGb;
                
                return (usedGb, totalGb);
            }
        }
        catch
        {
            // Ignore
        }

        return (0, 0);
    }

    private async Task<(double used, double total)> GetLinuxMemoryAsync()
    {
        try
        {
            if (File.Exists("/proc/meminfo"))
            {
                var lines = await File.ReadAllLinesAsync("/proc/meminfo");
                long memTotal = 0, memAvailable = 0;
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                    {
                        var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        var valueStr = parts[1].Replace("kB", "").Trim();
                        memTotal = long.Parse(valueStr);
                    }
                    else if (line.StartsWith("MemAvailable:"))
                    {
                        var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        var valueStr = parts[1].Replace("kB", "").Trim();
                        memAvailable = long.Parse(valueStr);
                    }
                }
                
                // Values are in KB, convert to GB
                var totalGb = memTotal / 1024.0 / 1024.0;
                var availableGb = memAvailable / 1024.0 / 1024.0;
                var usedGb = totalGb - availableGb;
                
                return (usedGb, totalGb);
            }
        }
        catch
        {
            // Ignore
        }

        return (0, 0);
    }

    private async Task<(double used, double total)> GetMacOSMemoryAsync()
    {
        try
        {
            var result = await ProcessHelper.ExecuteAsync("sysctl", "hw.memsize");
            if (result.IsSuccess && result.Output.Count > 0)
            {
                var line = result.Output[0];
                var parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    var bytes = long.Parse(parts[1].Trim());
                    var totalGb = bytes / 1024.0 / 1024.0 / 1024.0;
                    
                    // For used memory, we'd need vm_stat command
                    // For now, return total with 0 used (will improve later)
                    return (0, totalGb);
                }
            }
        }
        catch
        {
            // Ignore
        }

        return (0, 0);
    }

    /// <summary>
    /// Get storage metrics (free, total) in GB for root filesystem
    /// </summary>
    private async Task<(double free, double total)> GetStorageMetricsAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsStorageMetrics();
            }
            else
            {
                return await GetUnixStorageMetricsAsync();
            }
        }
        catch
        {
            // Ignore
        }

        return (0, 0);
    }

    private (double free, double total) GetWindowsStorageMetrics()
    {
        try
        {
            var drive = new DriveInfo("C:");
            var freeGb = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
            var totalGb = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
            return (freeGb, totalGb);
        }
        catch
        {
            return (0, 0);
        }
    }

    private async Task<(double free, double total)> GetUnixStorageMetricsAsync()
    {
        try
        {
            var result = await ProcessHelper.ExecuteAsync("df", "-k", "/");
            if (result.IsSuccess && result.Output.Count >= 2)
            {
                // Output format: Filesystem 1K-blocks Used Available Use% Mounted on
                var line = result.Output[1];
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length >= 4)
                {
                    var totalKb = long.Parse(parts[1]);
                    var availableKb = long.Parse(parts[3]);
                    
                    var totalGb = totalKb / 1024.0 / 1024.0;
                    var freeGb = availableKb / 1024.0 / 1024.0;
                    
                    return (freeGb, totalGb);
                }
            }
        }
        catch
        {
            // Ignore
        }

        return (0, 0);
    }

    /// <summary>
    /// Get system uptime in seconds
    /// </summary>
    private async Task<long?> GetUptimeSecondsAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists("/proc/uptime"))
                {
                    var content = await File.ReadAllTextAsync("/proc/uptime");
                    var parts = content.Split(' ');
                    if (parts.Length > 0 && double.TryParse(parts[0], out var uptime))
                    {
                        return (long)uptime;
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var result = await ProcessHelper.ExecuteAsync("sysctl", "kern.boottime");
                if (result.IsSuccess && result.Output.Count > 0)
                {
                    // Parse output like: kern.boottime: { sec = 1234567890, usec = 0 }
                    var line = result.Output[0];
                    var secIndex = line.IndexOf("sec = ");
                    if (secIndex >= 0)
                    {
                        var startIndex = secIndex + 6;
                        var endIndex = line.IndexOf(',', startIndex);
                        var secStr = line.Substring(startIndex, endIndex - startIndex);
                        if (long.TryParse(secStr, out var bootTime))
                        {
                            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            return now - bootTime;
                        }
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Use Environment.TickCount64 but it's in milliseconds
                return System.Environment.TickCount64 / 1000;
            }
        }
        catch
        {
            // Ignore
        }

        return null;
    }
}
