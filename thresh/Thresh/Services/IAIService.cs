namespace Thresh.Services;

/// <summary>
/// Interface for AI service providers (OpenAI, GitHub Copilot, etc.)
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Generate a blueprint from a natural language prompt
    /// </summary>
    /// <param name="prompt">User's natural language description</param>
    /// <param name="streaming">Enable streaming output (real-time)</param>
    /// <returns>Generated blueprint JSON</returns>
    Task<string> GenerateBlueprintAsync(string prompt, bool streaming = true);

    /// <summary>
    /// Interactive chat mode for blueprint assistance
    /// </summary>
    Task ChatModeAsync();

    /// <summary>
    /// Get the name of the AI provider (OpenAI, Copilot, etc.)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Get the model being used
    /// </summary>
    string ModelId { get; }
}
