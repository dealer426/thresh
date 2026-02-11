namespace Thresh.Services;

/// <summary>
/// Factory for creating AI service instances
/// Optimized for GitHub Copilot with AOT compatibility
/// Factory pattern maintained for future extensibility
/// </summary>
public class AiProviderFactory
{
    private readonly ConfigurationService _configService;

    public AiProviderFactory(ConfigurationService configService)
    {
        _configService = configService;
    }

    /// <summary>
    /// Create an AI service instance
    /// Currently only supports GitHub Copilot
    /// </summary>
    public IAIService CreateAIService(string? modelId = null, string? provider = null)
    {
        // Get model ID, default to gpt-4o if not specified
        modelId ??= _configService.GetValue("default-model") ?? "gpt-4o";

        // Only GitHub Copilot is supported
        return new GitHubCopilotService(_configService, modelId);
    }

    /// <summary>
    /// Get information about the configured provider
    /// </summary>
    public string GetProviderInfo()
    {
        var model = _configService.GetValue("default-model") ?? "gpt-4o";
        return $"Provider: GitHub Copilot, Model: {model}";
    }
}
