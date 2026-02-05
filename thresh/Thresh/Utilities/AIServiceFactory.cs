using Thresh.Services;

namespace Thresh.Utilities;

/// <summary>
/// Factory for creating AI service instances based on configuration
/// </summary>
public static class AIServiceFactory
{
    /// <summary>
    /// Create an AI service based on the configured provider
    /// </summary>
    public static IAIService CreateAIService(ConfigurationService configService, string? modelId = null, string? providerOverride = null)
    {
        var provider = providerOverride ?? configService.GetValue("aiprovider") ?? "openai";
        
        return provider.ToLowerInvariant() switch
        {
            "copilot" => new GitHubCopilotService(configService, modelId),
            "openai" => new OpenAIService(configService, modelId, providerOverride),
            _ => new OpenAIService(configService, modelId, providerOverride) // Default to OpenAI
        };
    }
}
