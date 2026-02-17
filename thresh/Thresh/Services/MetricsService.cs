using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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

        // Collect network/IP information
        var (primaryIp, allIps) = GetIpAddresses();
        metrics.IpAddress = primaryIp;
        metrics.IpAddresses = allIps;
        metrics.ExternalIp = await GetExternalIpAsync();

        // Collect load average (Linux/macOS)
        metrics.LoadAverage = await GetLoadAverageAsync();

        // Collect Docker storage info
        if (_containerService.RuntimeName == "docker")
        {
            var (storageDriver, rootDir) = await GetDockerStorageInfoAsync();
            metrics.DockerStorageDriver = storageDriver;
            metrics.DockerRootDir = rootDir;
        }

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

    /// <summary>
    /// Get IP addresses (primary and all)
    /// </summary>
    private (string? primary, List<string> all) GetIpAddresses()
    {
        var allIps = new List<string>();
        string? primaryIp = null;

        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            
            foreach (var ip in host.AddressList)
            {
                // Filter out IPv6 link-local and loopback
                if (ip.AddressFamily == AddressFamily.InterNetwork || 
                    (ip.AddressFamily == AddressFamily.InterNetworkV6 && !ip.IsIPv6LinkLocal))
                {
                    var ipStr = ip.ToString();
                    
                    // Skip loopback
                    if (ipStr != "127.0.0.1" && ipStr != "::1")
                    {
                        allIps.Add(ipStr);
                        
                        // First non-loopback IPv4 is primary
                        if (primaryIp == null && ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            primaryIp = ipStr;
                        }
                    }
                }
            }

            // If no primary found yet, use first IP
            if (primaryIp == null && allIps.Count > 0)
            {
                primaryIp = allIps[0];
            }
        }
        catch
        {
            // Ignore errors
        }

        return (primaryIp, allIps);
    }

    /// <summary>
    /// Get external/public IP address
    /// </summary>
    private async Task<string?> GetExternalIpAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var response = await client.GetStringAsync("https://api.ipify.org");
            return response?.Trim();
        }
        catch
        {
            // External IP detection failed - not critical
            return null;
        }
    }

    /// <summary>
    /// Get load average (Linux/macOS)
    /// </summary>
    private async Task<List<double>?> GetLoadAverageAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists("/proc/loadavg"))
                {
                    var content = await File.ReadAllTextAsync("/proc/loadavg");
                    var parts = content.Split(' ');
                    
                    if (parts.Length >= 3)
                    {
                        var loadAvg = new List<double>();
                        for (int i = 0; i < 3; i++)
                        {
                            if (double.TryParse(parts[i], out var load))
                            {
                                loadAvg.Add(load);
                            }
                        }
                        return loadAvg.Count == 3 ? loadAvg : null;
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var result = await ProcessHelper.ExecuteAsync("sysctl", "vm.loadavg");
                if (result.IsSuccess && result.Output.Count > 0)
                {
                    // Parse: vm.loadavg: { 1.50 1.75 2.00 }
                    var line = result.Output[0];
                    var startIndex = line.IndexOf('{');
                    var endIndex = line.IndexOf('}');
                    
                    if (startIndex >= 0 && endIndex > startIndex)
                    {
                        var values = line.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                        var parts = values.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        
                        if (parts.Length >= 3)
                        {
                            var loadAvg = new List<double>();
                            for (int i = 0; i < 3; i++)
                            {
                                if (double.TryParse(parts[i], out var load))
                                {
                                    loadAvg.Add(load);
                                }
                            }
                            return loadAvg.Count == 3 ? loadAvg : null;
                        }
                    }
                }
            }
        }
        catch
        {
            // Not critical
        }

        return null;
    }

    /// <summary>
    /// Get Docker storage driver and root directory
    /// </summary>
    private async Task<(string? driver, string? rootDir)> GetDockerStorageInfoAsync()
    {
        try
        {
            var result = await ProcessHelper.ExecuteAsync("docker", "info", "--format", "{{.Driver}}|{{.DockerRootDir}}");
            if (result.IsSuccess && result.Output.Count > 0)
            {
                var parts = result.Output[0].Split('|');
                if (parts.Length == 2)
                {
                    return (parts[0].Trim(), parts[1].Trim());
                }
            }
        }
        catch
        {
            // Ignore
        }

        return (null, null);
    }
}
