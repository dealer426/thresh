using System.Text;
using System.Text.Json;
using GitHub.Copilot.SDK;

namespace Thresh.Services;

/// <summary>
/// GitHub Copilot SDK service for AI-powered blueprint generation
/// </summary>
public class GitHubCopilotService : IAIService
{
    private readonly ConfigurationService _configService;
    private readonly string _modelId;

    public GitHubCopilotService(ConfigurationService configService, string? modelId = null)
    {
        _configService = configService;
        _modelId = modelId ?? configService.GetValue("model") ?? "gpt-5";
    }

    public string ProviderName => "GitHub Copilot SDK";
    public string ModelId => _modelId;

    public async Task<string> GenerateBlueprintAsync(string prompt, bool streaming = true)
    {
        // Build the system prompt with the correct thresh Blueprint schema
        var systemPrompt = @"You are a WSL development environment architect. Generate a JSON blueprint following this exact schema:
{
  ""name"": ""string (environment name)"",
  ""description"": ""string (brief description)"",
  ""base"": ""string (ubuntu-22.04, ubuntu-24.04, debian-12, alpine-3.19, etc.)"",
  ""packages"": [""array of package names""],
  ""environment"": {""KEY"": ""value""},
  ""scripts"": {
    ""setup"": ""#!/bin/bash\nmulti-line shell script for setup"",
    ""postInstall"": ""#!/bin/bash\nmulti-line shell script for post-install""
  }
}

Requirements:
- Use 'base' for the distribution (not 'distribution')
- Use 'environment' for variables (not 'environment_variables')
- Use 'scripts' with 'setup' and 'postInstall' properties
- Include all necessary packages
- Keep it minimal but functional
- Return ONLY valid JSON";

        var fullPrompt = $"{systemPrompt}\n\nUser request: {prompt}\n\nGenerate the JSON blueprint:";

        try
        {
            // Create GitHub Copilot client (auto-detects CLI and auth)
            await using var client = new CopilotClient();
            await client.StartAsync();

            if (streaming)
            {
                // Create session with streaming enabled
                await using var session = await client.CreateSessionAsync(new SessionConfig
                {
                    Model = _modelId,
                    Streaming = true
                });

                var done = new TaskCompletionSource<string>();
                var fullResponse = new StringBuilder();

                // Subscribe to events (and dispose when done)
                using var subscription = session.On(evt =>
                {
                    switch (evt)
                    {
                        case AssistantMessageDeltaEvent delta:
                            // Handle incremental text chunks
                            var chunk = delta.Data.DeltaContent ?? "";
                            fullResponse.Append(chunk);
                            Console.Write(chunk);
                            break;
                        case AssistantMessageEvent msg:
                            // Handle final complete message (always sent)
                            break;
                        case SessionIdleEvent:
                            done.SetResult(fullResponse.ToString());
                            break;
                        case SessionErrorEvent error:
                            done.SetException(new Exception(error.Data.Message));
                            break;
                    }
                });

                await session.SendAsync(new MessageOptions { Prompt = fullPrompt });
                return await done.Task;
            }
            else
            {
                // Non-streaming mode
                await using var session = await client.CreateSessionAsync(new SessionConfig
                {
                    Model = _modelId,
                    Streaming = false
                });

                var done = new TaskCompletionSource<string>();
                var fullResponse = new StringBuilder();

                // Subscribe to events (and dispose when done)
                using var subscription = session.On(evt =>
                {
                    switch (evt)
                    {
                        case AssistantMessageEvent msg:
                            // Handle final complete message
                            fullResponse.Append(msg.Data.Content);
                            break;
                        case SessionIdleEvent:
                            done.SetResult(fullResponse.ToString());
                            break;
                        case SessionErrorEvent error:
                            done.SetException(new Exception(error.Data.Message));
                            break;
                    }
                });

                await session.SendAsync(new MessageOptions { Prompt = fullPrompt });
                return await done.Task;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"GitHub Copilot SDK error: {ex.Message}. Make sure GitHub Copilot CLI is installed (npm install -g @github/copilot) and authenticated (copilot login).",
                ex);
        }
    }

    public async Task ChatModeAsync()
    {
        Console.WriteLine($"🤖 Chat Mode with {ProviderName} ({_modelId})");
        Console.WriteLine("Type 'exit' or 'quit' to end the conversation.\n");

        try
        {
            // Create GitHub Copilot client (auto-detects CLI and auth)
            await using var client = new CopilotClient();
            await client.StartAsync();

            await using var session = await client.CreateSessionAsync(new SessionConfig
            {
                Model = _modelId,
                Streaming = true
            });

            // Subscribe once for the entire session (outside the loop)
            TaskCompletionSource? currentRequest = null;

            using var subscription = session.On(evt =>
            {
                switch (evt)
                {
                    case AssistantMessageDeltaEvent delta:
                        var chunk = delta.Data.DeltaContent ?? "";
                        Console.Write(chunk);
                        break;
                    case SessionIdleEvent:
                        Console.WriteLine("\n");
                        currentRequest?.SetResult();
                        break;
                    case SessionErrorEvent error:
                        Console.WriteLine($"\n❌ Error: {error.Data.Message}\n");
                        currentRequest?.SetException(new Exception(error.Data.Message));
                        break;
                }
            });

            while (true)
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput) ||
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Console.Write("Assistant: ");

                currentRequest = new TaskCompletionSource();
                await session.SendAsync(new MessageOptions { Prompt = userInput });
                await currentRequest.Task;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"GitHub Copilot SDK error: {ex.Message}. Make sure GitHub Copilot CLI is installed (npm install -g @github/copilot) and authenticated (copilot login).",
                ex);
        }
    }

    /// <summary>
    /// Clean JSON output by removing markdown code blocks and validating structure
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
            return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            // Return as-is if not valid JSON, let caller handle
            return cleaned;
        }
    }
}
