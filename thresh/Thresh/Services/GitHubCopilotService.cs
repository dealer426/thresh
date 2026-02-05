using GitHub.Copilot.SDK;
using System.Text;

namespace Thresh.Services;

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
        try
        {
            var githubToken = _configService.GetValue("githubtoken");
            
            // Create GitHub Copilot client
            await using var client = new CopilotClient(new CopilotClientOptions
            {
                GithubToken = githubToken,
                UseLoggedInUser = string.IsNullOrEmpty(githubToken)
            });
            await client.StartAsync();
            
            var fullPrompt = $@"Generate a Docker blueprint in JSON format based on this request:

{prompt}

Return ONLY valid JSON with this structure:
{{
  ""blueprint"": {{
    ""name"": ""container-name"",
    ""description"": ""brief description"",
    ""base_image"": ""base:tag"",
    ""packages"": [""pkg1"", ""pkg2""],
    ""environment"": {{""KEY"": ""value""}},
    ""volumes"": [""/path/in/container""],
    ""ports"": [""8080:80""],
    ""commands"": [""command1"", ""command2""]
  }}
}}";

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

                session.On(evt =>
                {
                    switch (evt)
                    {
                        case AssistantMessageDeltaEvent delta:
                            var chunk = delta.Data.DeltaContent ?? "";
                            fullResponse.Append(chunk);
                            Console.Write(chunk);
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
                    Model = _modelId
                });

                var done = new TaskCompletionSource<string>();
                var fullResponse = new StringBuilder();

                session.On(evt =>
                {
                    if (evt is AssistantMessageEvent msg)
                    {
                        fullResponse.Append(msg.Data.Content);
                    }
                    else if (evt is SessionIdleEvent)
                    {
                        done.SetResult(fullResponse.ToString());
                    }
                    else if (evt is SessionErrorEvent error)
                    {
                        done.SetException(new Exception(error.Data.Message));
                    }
                });

                await session.SendAsync(new MessageOptions { Prompt = fullPrompt });
                return await done.Task;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"GitHub Copilot SDK error: {ex.Message}. Make sure GitHub Copilot CLI is installed and authenticated.",
                ex);
        }
    }

    public async Task ChatModeAsync()
    {
        Console.WriteLine($"🤖 Chat Mode with {ProviderName} ({_modelId})");
        Console.WriteLine("Type 'exit' or 'quit' to end the conversation.\n");

        try
        {
            var githubToken = _configService.GetValue("githubtoken");
            
            await using var client = new CopilotClient(new CopilotClientOptions
            {
                GithubToken = githubToken,
                UseLoggedInUser = string.IsNullOrEmpty(githubToken)
            });
            await client.StartAsync();

            await using var session = await client.CreateSessionAsync(new SessionConfig
            {
                Model = _modelId,
                Streaming = true
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

                var done = new TaskCompletionSource();

                session.On(evt =>
                {
                    switch (evt)
                    {
                        case AssistantMessageDeltaEvent delta:
                            var chunk = delta.Data.DeltaContent ?? "";
                            Console.Write(chunk);
                            break;
                        case SessionIdleEvent:
                            Console.WriteLine("\n");
                            done.SetResult();
                            break;
                        case SessionErrorEvent error:
                            Console.WriteLine($"\n❌ Error: {error.Data.Message}\n");
                            done.SetException(new Exception(error.Data.Message));
                            break;
                    }
                });

                await session.SendAsync(new MessageOptions { Prompt = userInput });
                await done.Task;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"GitHub Copilot SDK error: {ex.Message}. Make sure GitHub Copilot CLI is installed and authenticated.",
                ex);
        }
    }
}
