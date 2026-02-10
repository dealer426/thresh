using Thresh.Services;

namespace Thresh.Utilities;

/// <summary>
/// Legacy factory for creating AI service instances
/// Deprecated: Use AiProviderFactory in Services namespace instead
/// </summary>
[Obsolete("Use AiProviderFactory.CreateAIService() instead")]
public static class AIServiceFactory
{
    /// <summary>
    /// Create an AI service based on the configured provider
    /// </summary>
    public static IAIService CreateAIService(ConfigurationService configService, string? modelId = null, string? providerOverride = null)
    {
        // Delegate to the new factory
        var factory = new AiProviderFactory(configService);
        return factory.CreateAIService(modelId, providerOverride);
    }
}
