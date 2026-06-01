using GitHub.Copilot.SDK;

namespace Thresh;

/// <summary>
/// Simple test class to verify GitHub Copilot SDK works
/// </summary>
public static class CopilotSdkTest
{
    public static async Task RunAsync()
    {
        Console.WriteLine("Testing GitHub Copilot SDK...");
        Console.WriteLine();

        try
        {
            // Basic initialization test
            Console.WriteLine("Step 1: Creating CopilotClient...");
            
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
            
            Console.WriteLine("Step 2: Starting client...");
            await client.StartAsync();
            
            Console.WriteLine("✅ Client started successfully!");
            Console.WriteLine();

            // Try creating a session
            Console.WriteLine("Step 3: Creating session...");
            await using var session = await client.CreateSessionAsync(new SessionConfig
            {
                Model = "gpt-5",
                Streaming = false,
                OnPermissionRequest = PermissionHandler.ApproveAll
            });
            
            Console.WriteLine("✅ Session created successfully!");
            Console.WriteLine();

            // Try a simple prompt
            Console.WriteLine("Step 4: Sending test prompt...");
            var done = new TaskCompletionSource<string>();
            var response = new System.Text.StringBuilder();

            using var subscription = session.On(evt =>
            {
                switch (evt)
                {
                    case AssistantMessageEvent msg:
                        response.Append(msg.Data.Content);
                        break;
                    case SessionIdleEvent:
                        done.SetResult(response.ToString());
                        break;
                    case SessionErrorEvent error:
                        done.SetException(new Exception(error.Data.Message));
                        break;
                }
            });

            await session.SendAsync(new MessageOptions { Prompt = "Say hello in one word" });
            var result = await done.Task;

            Console.WriteLine($"✅ Received response: {result}");
            Console.WriteLine();
            Console.WriteLine("🎉 GitHub Copilot SDK test PASSED!");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ Test FAILED: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Exception details:");
            Console.WriteLine(ex.ToString());
        }
    }
}
