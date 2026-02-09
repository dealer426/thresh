using Azure;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Thresh.Services;

/// <summary>
/// Factory for creating OpenAI chat clients based on configured provider
/// Supports OpenAI, Azure OpenAI, and GitHub Models (all via Azure.AI.OpenAI SDK)
/// </summary>
public class AiProviderFactory
{
    private readonly ConfigurationService _configService;

    public AiProviderFactory(ConfigurationService configService)
    {
        _configService = configService;
    }

    /// <summary>
    /// Create a ChatClient based on configuration
    /// Priority: 1) Explicit provider parameter, 2) configured default-provider, 3) detect from available keys
    /// </summary>
    public ChatClient CreateChatClient(string? modelId = null, string? provider = null)
    {
        // Determine provider
        provider ??= _configService.GetValue("default-provider") ?? DetectProvider();

        // Determine model
        modelId ??= _configService.GetValue("default-model") ?? "gpt-4o";

        return provider.ToLowerInvariant() switch
        {
            "openai" => CreateOpenAIChatClient(modelId),
            "azure" or "azure-openai" => CreateAzureOpenAIChatClient(modelId),
            "github" or "github-models" => CreateGitHubModelsChatClient(modelId),
            _ => throw new InvalidOperationException($"Unknown AI provider: {provider}. Supported: openai, azure, github")
        };
    }

    private ChatClient CreateOpenAIChatClient(string modelId)
    {
        var apiKey = _configService.GetSecretValue("openai-api-key");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key not configured. Set it with:\n" +
                "  thresh config set openai-api-key <your-key>\n" +
                "Get your key from: https://platform.openai.com/api-keys");
        }

        var client = new OpenAIClient(apiKey);
        return client.GetChatClient(modelId);
    }

    private ChatClient CreateAzureOpenAIChatClient(string modelId)
    {
        var endpoint = _configService.GetValue("azure-openai-endpoint");
        var apiKey = _configService.GetSecretValue("azure-openai-key");

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException(
                "Azure OpenAI not configured. Set credentials with:\n" +
                "  thresh config set azure-openai-endpoint <your-endpoint>\n" +
                "  thresh config set azure-openai-key <your-key>\n" +
                "Get your credentials from Azure Portal: https://portal.azure.com");
        }

        var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
        return client.GetChatClient(modelId);
    }

    private ChatClient CreateGitHubModelsChatClient(string modelId)
    {
        var githubToken = _configService.GetSecretValue("github-token");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new InvalidOperationException(
                "GitHub token not configured. Set it with:\n" +
                "  thresh config set github-token <your-token>\n" +
                "Create a token at: https://github.com/settings/tokens\\n" +
                "Requires 'models:read' scope for GitHub Models access");
        }

        // GitHub Models uses Azure OpenAI endpoint with GitHub token
        var endpoint = new Uri("https://models.inference.ai.azure.com");
        var client = new AzureOpenAIClient(endpoint, new ApiKeyCredential(githubToken));
        
        // For GitHub Models, use their model IDs (e.g., gpt-4o, gpt-4o-mini)
        return client.GetChatClient(modelId);
    }

    private string DetectProvider()
    {
        // Auto-detect based on configured keys
        if (!string.IsNullOrEmpty(_configService.GetValue("github-token")))
            return "github";
        if (!string.IsNullOrEmpty(_configService.GetValue("azure-openai-endpoint")))
            return "azure";
        if (!string.IsNullOrEmpty(_configService.GetValue("openai-api-key")))
            return "openai";

        throw new InvalidOperationException(
            "No AI provider configured. Choose one:\n\n" +
            "1. GitHub Models (FREE for public repos):\n" +
            "   thresh config set github-token <token>\n" +
            "   thresh config set default-provider github\n\n" +
            "2. Azure OpenAI:\n" +
            "   thresh config set azure-openai-endpoint <endpoint>\n" +
            "   thresh config set azure-openai-key <key>\n" +
            "   thresh config set default-provider azure\n\n" +
            "3. OpenAI:\n" +
            "   thresh config set openai-api-key <key>\n" +
            "   thresh config set default-provider openai");
    }

    /// <summary>
    /// Get information about the configured provider
    /// </summary>
    public string GetProviderInfo()
    {
        try
        {
            var provider = DetectProvider();
            var model = _configService.GetValue("default-model") ?? "gpt-4o";
            return $"Provider: {provider}, Model: {model}";
        }
        catch
        {
            return "No AI provider configured";
        }
    }
}
