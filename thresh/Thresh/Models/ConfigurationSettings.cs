namespace Thresh.Models;

/// <summary>
/// Configuration settings for the thresh CLI
/// </summary>
public class ConfigurationSettings
{
    /// <summary>
    /// Default AI model to use (for GitHub Copilot)
    /// </summary>
    public string? DefaultModel { get; set; } = "gpt-4o";

    /// <summary>
    /// Enable telemetry
    /// </summary>
    public bool EnableTelemetry { get; set; } = true;

    /// <summary>
    /// Default base distribution for new environments
    /// </summary>
    public string? DefaultBase { get; set; } = "ubuntu-22.04";

    /// <summary>
    /// Custom distributions registered by user
    /// </summary>
    public Dictionary<string, CustomDistribution> CustomDistributions { get; set; } = new();

    /// <summary>
    /// Additional custom settings
    /// </summary>
    public Dictionary<string, string> CustomSettings { get; set; } = new();
}
