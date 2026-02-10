using System.Text;
using System.Text.Json;
using Thresh.Models;

namespace Thresh.Services;

/// <summary>
/// Facade service for AI-powered blueprint generation and interactive chat
/// Delegates to provider-specific IAIService implementations
/// Supports OpenAI, Azure OpenAI, and GitHub Copilot SDK
/// </summary>
public class CopilotService
{
    private readonly IAIService _aiService;
    private readonly ConfigurationService _configService;

    public CopilotService(ConfigurationService configService, string? modelId = null, string? provider = null)
    {
        _configService = configService;
        var factory = new AiProviderFactory(configService);
        _aiService = factory.CreateAIService(modelId, provider);
    }

    /// <summary>
    /// Get the underlying AI service instance (for advanced scenarios)
    /// </summary>
    public IAIService AIService => _aiService;

    /// <summary>
    /// Generate a blueprint from a natural language prompt with streaming output
    /// </summary>
    public async Task<string> GenerateBlueprintAsync(string prompt, bool streaming = true)
    {
        return await _aiService.GenerateBlueprintAsync(prompt, streaming);
    }

    /// <summary>
    /// Interactive chat mode with streaming responses
    /// </summary>
    public async Task ChatModeAsync()
    {
        await _aiService.ChatModeAsync();
    }

    /// <summary>
    /// Extract and validate JSON blueprint from LLM response
    /// </summary>
    public string CleanJsonOutput(string rawOutput)
    {
        // Delegate to underlying service if it has the method
        if (_aiService is OpenAIService openAIService)
            return openAIService.CleanJsonOutput(rawOutput);
        if (_aiService is AzureOpenAIService azureService)
            return azureService.CleanJsonOutput(rawOutput);
        if (_aiService is GitHubCopilotService copilotService)
            return copilotService.CleanJsonOutput(rawOutput);

        // Fallback for unknown services
        return rawOutput.Trim();
    }

    /// <summary>
    /// Discover distribution information using AI
    /// </summary>
    public async Task<CustomDistribution?> DiscoverDistributionAsync(string distroName)
    {
        // Delegate to underlying service if it has the method
        if (_aiService is OpenAIService openAIService)
            return await openAIService.DiscoverDistributionAsync(distroName);
        if (_aiService is AzureOpenAIService azureService)
            return await azureService.DiscoverDistributionAsync(distroName);

        // GitHub Copilot SDK doesn't support distribution discovery
        // Fallback for other services
        Console.WriteLine($"❌ Discovery not supported by {_aiService.ProviderName}");
        return null;
    }
}
