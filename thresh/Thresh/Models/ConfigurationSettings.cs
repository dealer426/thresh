namespace Thresh.Models;

/// <summary>
/// Configuration settings for the thresh CLI
/// </summary>
public class ConfigurationSettings
{
    /// <summary>
    /// OpenAI API key for AI features
    /// </summary>
    public string? OpenAIApiKey { get; set; }

    /// <summary>
    /// Azure OpenAI endpoint
    /// </summary>
    public string? AzureOpenAIEndpoint { get; set; }

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string? AzureOpenAIApiKey { get; set; }

    /// <summary>
    /// Default AI model to use
    /// </summary>
    public string? DefaultModel { get; set; } = "gpt-4";

    /// <summary>
    /// AI provider to use: "openai" or "copilot"
    /// </summary>
    public string? AIProvider { get; set; } = "openai";

    /// <summary>
    /// GitHub token for Copilot features
    /// </summary>
    public string? GitHubToken { get; set; }

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
