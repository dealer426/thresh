namespace Thresh.Models;

/// <summary>
/// Status of a thresh environment
/// </summary>
public enum EnvironmentStatus
{
    Running,
    Stopped,
    Installing,
    Terminated,
    Unknown
}

public static class EnvironmentStatusExtensions
{
    public static string GetDisplayName(this EnvironmentStatus status)
    {
        return status switch
        {
            EnvironmentStatus.Running => "Running",
            EnvironmentStatus.Stopped => "Stopped",
            EnvironmentStatus.Installing => "Installing",
            EnvironmentStatus.Terminated => "Terminated",
            _ => "Unknown"
        };
    }

    public static EnvironmentStatus FromWslState(string? wslState)
    {
        if (string.IsNullOrWhiteSpace(wslState))
            return EnvironmentStatus.Unknown;

        return wslState.Trim().ToLowerInvariant() switch
        {
            "running" => EnvironmentStatus.Running,
            "stopped" => EnvironmentStatus.Stopped,
            "installing" => EnvironmentStatus.Installing,
            "terminated" => EnvironmentStatus.Terminated,
            _ => EnvironmentStatus.Unknown
        };
    }
}
