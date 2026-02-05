namespace EknovaCli.Models;

/// <summary>
/// Represents an eknova environment with its metadata
/// </summary>
public class Environment
{
    public string Name { get; set; } = string.Empty;
    public string WslDistributionName { get; set; } = string.Empty;
    public EnvironmentStatus Status { get; set; } = EnvironmentStatus.Unknown;
    public string? Blueprint { get; set; }
    public DateTime? Created { get; set; }
    public string? BaseImage { get; set; }
    public string? Version { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is Environment env &&
               Name == env.Name &&
               WslDistributionName == env.WslDistributionName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, WslDistributionName);
    }

    public override string ToString()
    {
        return $"Environment{{Name='{Name}', Distribution='{WslDistributionName}', Status={Status}}}";
    }
}
