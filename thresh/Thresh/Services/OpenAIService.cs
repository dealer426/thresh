using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using Thresh.Models;

namespace Thresh.Services;

/// <summary>
/// OpenAI-based AI service implementation
/// Supports OpenAI, Azure OpenAI, and GitHub Models via Azure.AI.OpenAI SDK
/// </summary>
public class OpenAIService : IAIService
{
    private readonly ChatClient _chatClient;
    private readonly string _modelId;
    private readonly ConfigurationService _configService;

    public string ProviderName => "OpenAI";
    public string ModelId => _modelId;

    public OpenAIService(ConfigurationService configService, string? modelId = null, string? provider = null)
    {
        _configService = configService;
        var factory = new AiProviderFactory(configService);
        _chatClient = factory.CreateChatClient(modelId, provider);
        _modelId = modelId ?? configService.GetValue("default-model") ?? "gpt-4o";
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

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage($"Create a blueprint for: {prompt}")
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 2000
        };

        var fullResponse = new StringBuilder();

        if (streaming)
        {
            Console.WriteLine($"🤖 Generating blueprint with {ProviderName} ({_modelId})...\n");
            
            var updates = _chatClient.CompleteChatStreamingAsync(messages, options);
            await foreach (var update in updates)
            {
                foreach (var contentPart in update.ContentUpdate)
                {
                    var text = contentPart.Text;
                    Console.Write(text);
                    fullResponse.Append(text);
                }
            }
            
            Console.WriteLine("\n");
        }
        else
        {
            var response = await _chatClient.CompleteChatAsync(messages, options);
            var content = response.Value.Content[0].Text;
            fullResponse.Append(content);
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

        var conversationHistory = new List<ChatMessage>
        {
            new SystemChatMessage(@"You are an expert DevOps assistant helping users with WSL development environments and blueprints.

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

Be concise, practical, and provide actionable guidance.")
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 1000
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
                conversationHistory.Add(conversationHistory[0]); // Keep system message
                Console.WriteLine("\n🔄 Conversation history cleared.\n");
                continue;
            }

            // Add user message to history
            conversationHistory.Add(new UserChatMessage(userInput));

            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nAssistant> ");
                Console.ResetColor();

                var responseBuilder = new StringBuilder();
                var updates = _chatClient.CompleteChatStreamingAsync(conversationHistory, options);
                
                await foreach (var update in updates)
                {
                    foreach (var contentPart in update.ContentUpdate)
                    {
                        var text = contentPart.Text;
                        Console.Write(text);
                        responseBuilder.Append(text);
                    }
                }

                Console.WriteLine("\n");

                // Add assistant response to history
                conversationHistory.Add(new AssistantChatMessage(responseBuilder.ToString()));
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
            var jsonDoc = JsonDocument.Parse(cleaned);
            return JsonSerializer.Serialize(jsonDoc, BlueprintJsonContext.Default.JsonDocument);
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

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage($"Find official rootfs information for: {distroName}")
        };

        try
        {
            Console.WriteLine($"🔍 Discovering {distroName} distribution information...");
            
            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;
            
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
