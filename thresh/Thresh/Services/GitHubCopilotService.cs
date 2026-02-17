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
        // Detect platform to provide context-appropriate guidance
        var platformName = ContainerServiceFactory.GetPlatformName();
        var runtimeName = ContainerServiceFactory.GetExpectedRuntimeName();
        var environmentType = platformName == "Windows" ? "WSL" : "Docker container";
        
        // Build the system prompt with the correct thresh Blueprint schema
        var systemPrompt = $@"You are a development environment architect. Generate a JSON blueprint for {environmentType} environments following this exact schema:
{{
  ""name"": ""string (environment name)"",
  ""description"": ""string (brief description)"",
  ""base"": ""string (ubuntu-22.04, ubuntu-24.04, debian-12, alpine-3.19, etc.)"",
  ""packages"": [""array of package names""],
  ""environment"": {{""KEY"": ""value""}},
  ""scripts"": {{
    ""setup"": ""#!/bin/bash\nmulti-line shell script for setup"",
    ""postInstall"": ""#!/bin/bash\nmulti-line shell script for post-install""
  }}
}}

Platform context: {platformName} using {runtimeName}
{(platformName == "Windows" ? "WSL2 environments support systemd and full Linux distributions." : "Docker containers use lightweight Linux distributions optimized for containerization.")}

Requirements:
- Use 'base' for the distribution (not 'distribution')
- Use 'environment' for variables (not 'environment_variables')
- Use 'scripts' with 'setup' and 'postInstall' properties
- Include all necessary packages for the platform
- Keep it minimal but functional
- Return ONLY valid JSON";

        var fullPrompt = $"{systemPrompt}\n\nUser request: {prompt}\n\nGenerate the JSON blueprint:";

        try
        {
            // Create GitHub Copilot client with system CLI path
            // On Windows, npm installs as PowerShell script, so we need to use pwsh/powershell
            var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows);
            
            var options = new CopilotClientOptions
            {
                UseStdio = true,
                AutoStart = true
            };
            
            // On Windows with npm-installed copilot, use command shell wrapper
            if (isWindows)
            {
                // Use cmd.exe to execute copilot (works with npm PowerShell wrappers)
                options.CliPath = "cmd.exe";
                options.CliArgs = ["/c", "copilot"];
            }
            else
            {
                // On Linux/macOS, copilot is a native binary
                options.CliPath = "copilot";
            }
            
            await using var client = new CopilotClient(options);
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
            // Detect platform to provide context
            var platformName = ContainerServiceFactory.GetPlatformName();
            var runtimeName = ContainerServiceFactory.GetExpectedRuntimeName();
            var environmentType = platformName == "Windows" ? "WSL" : "Docker container";
            
            // Create GitHub Copilot client with system CLI path
            var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows);
            
            var options = new CopilotClientOptions
            {
                UseStdio = true,
                AutoStart = true
            };
            
            // On Windows with npm-installed copilot, use command shell wrapper
            if (isWindows)
            {
                options.CliPath = "cmd.exe";
                options.CliArgs = ["/c", "copilot"];
            }
            else
            {
                options.CliPath = "copilot";
            }
            
            await using var client = new CopilotClient(options);
            await client.StartAsync();

            await using var session = await client.CreateSessionAsync(new SessionConfig
            {
                Model = _modelId,
                Streaming = true
            });

            // Subscribe once for the entire session (outside the loop)
            TaskCompletionSource? currentRequest = null;
            var responseBuffer = new StringBuilder(); // Accumulate response for blueprint detection

            using var subscription = session.On(evt =>
            {
                switch (evt)
                {
                    case AssistantMessageDeltaEvent delta:
                        var chunk = delta.Data.DeltaContent ?? "";
                        Console.Write(chunk);
                        responseBuffer.Append(chunk); // Accumulate for saving
                        break;
                    case SessionIdleEvent:
                        Console.WriteLine("\n");
                        
                        // Try to extract and save blueprint if response contains valid JSON
                        TrySaveBlueprintFromResponse(responseBuffer.ToString());
                        responseBuffer.Clear(); // Clear for next response
                        
                        currentRequest?.SetResult();
                        break;
                    case SessionErrorEvent error:
                        Console.WriteLine($"\n❌ Error: {error.Data.Message}\n");
                        responseBuffer.Clear();
                        currentRequest?.SetException(new Exception(error.Data.Message));
                        break;
                }
            });

            // Send initial system context message (non-streaming to avoid showing it)
            var systemContext = $@"You are helping a user with thresh, a tool that provisions {environmentType} environments.

CRITICAL CONTEXT:
- Platform: {platformName} running {runtimeName}
- Environment type: {environmentType}
- Blueprint format: JSON (preferred) or YAML (supported)

IMPORTANT: When user asks for a blueprint, you MUST output the complete JSON blueprint in your response. 
DO NOT just say you ""created"" a file - actually OUTPUT the JSON content so it can be auto-saved.

Blueprint JSON format:
{{
  ""name"": ""environment-name"",
  ""description"": ""brief description"",
  ""base"": ""ubuntu-22.04"",  // or alpine-3.19, debian-12, etc.
  ""packages"": [""package1"", ""package2""],
  ""environment"": {{""KEY"": ""value""}},
  ""scripts"": {{
    ""setup"": ""#!/bin/bash\ncommands"",
    ""postInstall"": ""#!/bin/bash\ncommands""
  }}
}}

JSON is preferred for compatibility. YAML is also supported but use JSON unless specifically requested.
{(platformName == "Windows" ? "WSL environments support systemd and full services." : "Docker containers should avoid systemd and focus on lightweight, single-process setups.")}

Available base distributions: ubuntu-22.04, ubuntu-24.04, alpine-3.19, debian-12, and more.

REMEMBER: Always output the actual JSON blueprint content. Blueprints containing valid JSON will be automatically saved to a file for the user.";

            currentRequest = new TaskCompletionSource();
            await session.SendAsync(new MessageOptions { Prompt = systemContext });
            await currentRequest.Task; // Wait for acknowledgment
            currentRequest = null;

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
    /// Try to extract and save a blueprint from AI response
    /// </summary>
    private void TrySaveBlueprintFromResponse(string response)
    {
        try
        {
            // Clean the response to extract JSON
            var cleaned = CleanJsonOutput(response);
            
            // Try to parse as JSON
            using var jsonDoc = JsonDocument.Parse(cleaned);
            var root = jsonDoc.RootElement;
            
            // Check if it looks like a blueprint (has required fields)
            if (!root.TryGetProperty("name", out var nameElement) ||
                !root.TryGetProperty("base", out _))
            {
                return; // Not a blueprint, skip saving
            }
            
            var blueprintName = nameElement.GetString() ?? "unnamed";
            
            // Sanitize filename (remove invalid characters)
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("-", blueprintName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            var filename = $"{sanitized}.json";
            
            // Get blueprints directory (next to executable)
            var baseDir = AppContext.BaseDirectory;
            var blueprintsDir = Path.Combine(baseDir, "blueprints");
            
            // Ensure blueprints directory exists
            if (!Directory.Exists(blueprintsDir))
            {
                Directory.CreateDirectory(blueprintsDir);
            }
            
            var fullPath = Path.Combine(blueprintsDir, filename);
            
            // Check if blueprint already exists
            if (File.Exists(fullPath))
            {
                Console.WriteLine($"💾 Blueprint updated: {filename}");
            }
            else
            {
                Console.WriteLine($"💾 Blueprint saved: {filename}");
            }
            
            // Save to blueprints folder
            File.WriteAllText(fullPath, cleaned);
            
            Console.WriteLine($"   Available in: thresh blueprint list");
            Console.WriteLine($"   To provision: thresh up {sanitized} --name my-env");
            Console.WriteLine();
        }
        catch (JsonException)
        {
            // Not valid JSON or not a blueprint, ignore
        }
        catch (Exception ex)
        {
            // Log error but don't interrupt chat
            Console.WriteLine($"⚠️  Failed to auto-save blueprint: {ex.Message}");
        }
    }
}
