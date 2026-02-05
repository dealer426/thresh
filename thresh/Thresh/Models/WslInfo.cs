namespace EknovaCli.Models;

/// <summary>
/// WSL system information
/// </summary>
public class WslInfo
{
    public bool Available { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? KernelVersion { get; set; }
    public string? WslgVersion { get; set; }
    public string? MsrdcVersion { get; set; }
    public string? Direct3DVersion { get; set; }
    public string? DxCoreVersion { get; set; }
    public string? WindowsVersion { get; set; }
    public string? FullVersionOutput { get; set; }
    public int DistributionCount { get; set; }

    public WslInfo(bool available, string version, string? kernelVersion = null, 
                   string? fullVersionOutput = null, int distributionCount = 0)
    {
        Available = available;
        Version = version;
        KernelVersion = kernelVersion;
        FullVersionOutput = fullVersionOutput;
        DistributionCount = distributionCount;

        // Parse additional version details if we have full output
        if (!string.IsNullOrEmpty(fullVersionOutput))
        {
            WslgVersion = ParseVersionFromOutput(fullVersionOutput, "WSLg version:");
            MsrdcVersion = ParseVersionFromOutput(fullVersionOutput, "MSRDC version:");
            Direct3DVersion = ParseVersionFromOutput(fullVersionOutput, "Direct3D version:");
            DxCoreVersion = ParseVersionFromOutput(fullVersionOutput, "DXCore version:");
            WindowsVersion = ParseVersionFromOutput(fullVersionOutput, "Windows version:");
        }
    }

    private static string? ParseVersionFromOutput(string output, string prefix)
    {
        foreach (var line in output.Split('\n'))
        {
            var cleaned = new string(line.Where(c => !char.IsControl(c) || c == '\n').ToArray()).Trim();
            if (cleaned.Contains(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var prefixIndex = cleaned.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
                var value = cleaned[(prefixIndex + prefix.Length)..].Trim();
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
        }
        return null;
    }

    public override string ToString()
    {
        return $"WSL {Version} (Kernel: {KernelVersion ?? "Unknown"}, Distributions: {DistributionCount})";
    }
}
