using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.Text;
using System.Text.Json;
using Thresh.Models;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;
using OpenAIChatClient = OpenAI.Chat.ChatClient;

namespace Thresh.Services;

/// <summary>
/// OpenAI AI service implementation
/// Uses OpenAI API with API key authentication
/// Now using Microsoft.Extensions.AI.IChatClient for unified interface
/// </summary>
public class OpenAIService : IAIService
{
    private readonly IChatClient _chatClient;
    private readonly string _modelId;
    private readonly ConfigurationService _configService;

    public string ProviderName => "OpenAI";
    public string ModelId => _modelId;

    public OpenAIService(ConfigurationService configService, string? modelId = null)
    {
        _configService = configService;
        _modelId = modelId ?? configService.GetValue("default-model") ?? "gpt-4o";
        
        var apiKey = configService.GetSecretValue("openai-api-key");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key not configured. Set it with:\n" +
                "  thresh config set openai-api-key <your-key>\n" +
                "Get your key from: https://platform.openai.com/api-keys");
        }

        try
        {
            // Create OpenAI ChatClient and wrap with Microsoft.Extensions.AI
            var openaiClient = new OpenAIClient(apiKey);
            var chatClient = openaiClient.GetChatClient(_modelId);
            
            // Wrap with IChatClient using Microsoft.Extensions.AI
            _chatClient = new ChatClientBuilder(chatClient.AsIChatClient())
                .UseLogging() // Add structured logging
                .Build();
        }
        catch (Exception ex)
        {
            var errorMsg = $"OpenAI SDK initialization failed: {ex.GetType().Name}\n" +
                          $"Message: {ex.Message}\n";
            
            if (ex.InnerException != null)
            {
                errorMsg += $"Inner: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}\n";
            }
            
            errorMsg += "\nOptions:\n" +
                       "1. Use GitHub Copilot (FREE, fully AOT-compatible):\n" +
                       "   gh auth login\n" +
                       "   thresh config set default-provider github-copilot\n\n" +
                       "2. Use Debug build (OpenAI SDK compatible):\n" +
                       "   cd thresh/Thresh\n" +
                       "   dotnet run -c Debug -- generate 'your prompt'";
            
            throw new InvalidOperationException(errorMsg, ex);
        }
    }

    /// <summary>
    /// Generate a blueprint from a natural language prompt with streaming output
    /// </summary>
    public async Task<string> GenerateBlueprintAsync(string prompt, bool streaming = true)
    {
        var systemPrompt = @"You are an expert DevOps engineer helping users create WSL development environment blueprints.

Generate a JSON blueprint based on the user's request. The blueprint must follow this exact structure:

{
  ""name"": ""environment-name"",
  ""description"": ""Brief description"",
  ""base"": ""ubuntu-22.04"",
  ""packages"": [""package1"", ""package2""],
  ""scripts"": {
    ""setup"": ""#!/bin/bash\necho 'Setting up...'"",
    ""postInstall"": ""#!/bin/bash\necho 'Post-install...'"" 
  },
  ""environment"": {
    ""VAR_NAME"": ""value""
  }
}

Available base distributions:
- ubuntu-22.04, ubuntu-24.04 (general purpose)
- alpine-3.19 (minimal, ~5MB)
- debian-12 (stability)

Common packages by use case:
- Python: python3, python3-pip, python3-venv, python3-dev
- Node.js: nodejs, npm, curl
- Go: golang, git
- System: build-essential, gcc, make, cmake, git
- Tools: curl, wget, vim, nano, htop

Rules:
1. Return ONLY valid JSON, no markdown code blocks
2. Use appropriate packages for the requested technology stack
3. Keep scripts concise and functional
4. Use descriptive names and clear descriptions
5. Include environment variables when relevant";

        // Use Microsoft.Extensions.AI ChatMessage format
        var messages = new List<AIChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, $"Create a blueprint for: {prompt}")
        };

        var options = new ChatOptions
        {
            Temperature = 0.7f,
            MaxOutputTokens = 2000
        };

        var fullResponse = new StringBuilder();

        if (streaming)
        {
            Console.WriteLine($"🤖 Generating blueprint with {ProviderName} ({_modelId})...\n");
            
            // Use IChatClient streaming API
            await foreach (var update in _chatClient.GetStreamingResponseAsync(messages, options))
            {
                if (update.Text != null)
                {
                    Console.Write(update.Text);
                    fullResponse.Append(update.Text);
                }
            }
            
            Console.WriteLine("\n");
        }
        else
        {
            // Use IChatClient non-streaming API
            var response = await _chatClient.GetResponseAsync(messages, options);
            fullResponse.Append(response.Text);
        }

        return fullResponse.ToString();
    }

    /// <summary>
    /// Interactive chat mode with streaming responses
    /// </summary>
    public async Task ChatModeAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════╗");
        Console.WriteLine("║     Thresh AI Chat - Blueprint Assistant     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine($"🤖 Provider: {ProviderName}");
        Console.WriteLine($"📦 Model: {_modelId}");
        Console.WriteLine("💬 Ask about blueprints, WSL environments, or development setups");
        Console.WriteLine("⌨️  Type 'exit' or 'quit' to end the session");
        Console.WriteLine("🔄 Type 'clear' to reset conversation history");
        Console.WriteLine();

        // Use Microsoft.Extensions.AI ChatMessage format
        var systemMessage = new AIChatMessage(ChatRole.System, @"You are an expert DevOps assistant helping users with WSL development environments and blueprints.

You help with:
- Creating and customizing blueprint configurations
- Recommending packages and tools for specific use cases
- Explaining WSL, Linux distributions, and container technologies
- Troubleshooting environment setup issues
- Best practices for development environments

Available thresh commands:
- thresh up <blueprint>: Create environment from blueprint
- thresh list: List all environments
- thresh destroy <name>: Remove environment
- thresh blueprints: List available blueprints
- thresh generate <prompt>: AI-generate blueprint
- thresh chat: This interactive mode
- thresh config: Manage configuration
- thresh serve: Start MCP server

Base distributions:
- ubuntu-22.04, ubuntu-24.04 (most packages)
- alpine-3.19 (minimal, fast)
- debian-12 (stable, long-term support)

Be concise, practical, and provide actionable guidance.");

        var conversationHistory = new List<AIChatMessage> { systemMessage };

        var options = new ChatOptions
        {
            Temperature = 0.7f,
            MaxOutputTokens = 1000
        };

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("You> ");
            Console.ResetColor();
            
            var userInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(userInput))
                continue;

            if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\n👋 Goodbye!");
                break;
            }

            if (userInput.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                conversationHistory.Clear();
                conversationHistory.Add(systemMessage); // Keep system message
                Console.WriteLine("\n🔄 Conversation history cleared.\n");
                continue;
            }

            // Add user message to history
            conversationHistory.Add(new AIChatMessage(ChatRole.User, userInput));

            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nAssistant> ");
                Console.ResetColor();

                var responseBuilder = new StringBuilder();
                
                // Use IChatClient streaming API
                await foreach (var update in _chatClient.GetStreamingResponseAsync(conversationHistory, options))
                {
                    if (update.Text != null)
                    {
                        Console.Write(update.Text);
                        responseBuilder.Append(update.Text);
                    }
                }

                Console.WriteLine("\n");

                // Add assistant response to history
                conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, responseBuilder.ToString()));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error: {ex.Message}\n");
                Console.ResetColor();
                
                // Remove the failed user message
                conversationHistory.RemoveAt(conversationHistory.Count - 1);
            }
        }
    }

    /// <summary>
    /// Extract and validate JSON blueprint from LLM response
    /// </summary>
    public string CleanJsonOutput(string rawOutput)
    {
        // Remove markdown code blocks if present
        var cleaned = rawOutput.Trim();
        
        if (cleaned.Contains("```"))
        {
            var lines = cleaned.Split('\n');
            var jsonLines = new List<string>();
            var inCodeBlock = false;

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                if (inCodeBlock)
                {
                    jsonLines.Add(line);
                }
            }

            cleaned = string.Join("\n", jsonLines).Trim();
        }

        // Validate JSON
        try
        {
            using var jsonDoc = JsonDocument.Parse(cleaned);

            try
            {
                // Normalized JSON via source-generated context
                return JsonSerializer.Serialize(jsonDoc, BlueprintJsonContext.Default.JsonDocument);
            }
            catch (Exception)
            {
                // AOT-safe fallback: if serialization fails, return raw JSON text
                return jsonDoc.RootElement.GetRawText();
            }
        }
        catch (JsonException)
        {
            // Return as-is if not valid JSON, let caller handle
            return cleaned;
        }
    }

    /// <summary>
    /// Discover distribution information using AI
    /// </summary>
    public async Task<CustomDistribution?> DiscoverDistributionAsync(string distroName)
    {
        var systemPrompt = @"You are a Linux distribution expert. When given a distribution name, find the official rootfs download URL and provide accurate information.

Response must be valid JSON with this structure:
{
  ""name"": ""distribution-name"",
  ""version"": ""version-number"",
  ""rootfsUrl"": ""https://official-url-to-rootfs.tar.gz"",
  ""packageManager"": ""apt|apk|dnf|yum|pacman|zypper"",
  ""description"": ""brief description""
}

Requirements:
- Use ONLY official sources (official distribution sites, not third-party)
- rootfsUrl must be a direct download link to a tar.gz or tar.xz file
- For Rocky Linux: use official rocky-linux.org sources
- For Arch: use official archlinux.org sources
- For Fedora: use official fedoraproject.org sources
- Verify the URL format is correct for WSL rootfs import
- Include version number in the response

Return ONLY the JSON, no explanations.";

        var messages = new AIChatMessage[]
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, $"Find official rootfs information for: {distroName}")
        };

        try
        {
            Console.WriteLine($"🔍 Discovering {distroName} distribution information...");
            
            var response = await _chatClient.GetResponseAsync(messages);
            var content = response.Text;
            
            // Clean and parse response
            var cleaned = CleanJsonOutput(content);
            var distro = JsonSerializer.Deserialize<CustomDistribution>(cleaned, BlueprintJsonContext.Default.CustomDistribution);
            
            if (distro != null && !string.IsNullOrEmpty(distro.RootfsUrl))
            {
                // Generate key from name and version
                distro.Key = $"{distro.Name.ToLowerInvariant()}-{distro.Version}";
                Console.WriteLine($"✅ Found: {distro.Name} {distro.Version}");
                Console.WriteLine($"   URL: {distro.RootfsUrl}");
                Console.WriteLine($"   Package Manager: {distro.PackageManager}");
                return distro;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Discovery failed: {ex.Message}");
            return null;
        }
    }
}
