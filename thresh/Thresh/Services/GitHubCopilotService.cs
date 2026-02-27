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
        var isWslMode = platformName == "Windows";
        
        // Build the system prompt with the correct thresh Blueprint schema
        var systemPrompt = $@"You are a development environment architect. Generate a JSON blueprint for {environmentType} environments following this exact schema:
{{
  ""name"": ""string (environment name)"",
  ""description"": ""string (brief description)"",
  ""base"": ""string (ubuntu-22.04, ubuntu-24.04, debian-12, alpine-3.19, etc.)"",
  ""packages"": [""array of package names""],
  ""ports"": [""optional array of ports""],
  ""volumes"": [{{""name"": ""volume-name"", ""mount"": ""/data/path""}}],{(isWslMode ? @"
  ""wslConfig"": ""optional: profile name (systemd|docker|database|web-server|minimal|development)""," : "")}
  ""environment"": {{""KEY"": ""value""}},
  ""scripts"": {{
    ""setup"": ""#!/bin/bash\nmulti-line shell script for setup"",
    ""postInstall"": ""#!/bin/bash\nmulti-line shell script for post-install""
  }}
}}

Platform context: {platformName} using {runtimeName}
{(isWslMode ? "WSL2 environments support systemd and full Linux distributions." : "Docker containers use lightweight Linux distributions optimized for containerization.")}

{(isWslMode ? @"WSL CONFIGURATION PROFILES - IMPORTANT:
WSL distros can be optimized with built-in profiles that configure /etc/wsl.conf. Use 'wslConfig' property:

PROFILES:
- ""database"" - FOR DATABASES (PostgreSQL, MySQL, MongoDB, Redis)
  * Disables Windows interop and automount to avoid Plan9 filesystem permission errors
  * Solves 'chmod' operation failures
  * Forces databases to run on native Linux filesystem only
  * USE THIS for ANY database server to avoid permission issues!

- ""docker"" - Auto-starts Docker daemon on boot
  * Enables systemd
  * Runs 'service docker start' automatically

- ""web-server"" - Auto-starts Nginx web server
  * Enables systemd
  * Runs 'service nginx start' automatically

- ""systemd"" - Minimal systemd enablement
  * Just enables systemd, no other changes

- ""minimal"" - Fast startup, no systemd
  * Lightweight, quick boot
  * No Windows integration overhead

- ""development"" - Full Windows integration
  * Windows PATH included
  * Can call Windows tools from Linux
  * Auto-mounts Windows drives at /mnt/

WHEN TO USE:
- Any database (PostgreSQL, MySQL, MongoDB, Redis, Cassandra) → USE ""database"" profile!
- Docker development → ""docker""
- Web servers (Nginx, Apache) → ""web-server""
- Full-stack dev needing Windows tools → ""development""
- CI/CD agents, minimal containers → ""minimal""
- Services needing systemd only → ""systemd""

CRITICAL: Always use ""database"" profile for database servers to avoid WSL Plan9 filesystem issues!" : "")}

PORT CONFIGURATION - CRITICAL:
{(isWslMode ? 
@"WSL2 MODE: Ports are automatically forwarded from WSL to Windows localhost.
- Include 'ports' array with desired port numbers: [""8090"", ""5432""]
- In 'setup' script, configure the service to listen on the specified port
- WSL2 will automatically forward any listening port to Windows localhost
- Examples:
  Nginx on 8090: sed -i 's/listen 80;/listen 8090;/' /etc/nginx/sites-enabled/default
  PostgreSQL on 5433: sed -i 's/port = 5432/port = 5433/' /etc/postgresql/*/main/postgresql.conf
  Node.js server: Configure app.listen(PORT) in setup script" :
@"DOCKER MODE: Use 'hostPort:containerPort' format for port mapping.
- Format: [""8080:80"", ""5433:5432""]
- Maps container internal port to different host port
- Container service listens on standard port, host accesses via mapped port")}

VOLUME/STORAGE CONFIGURATION - CRITICAL:
{(isWslMode ? 
@"WSL2 MODE: Volumes are Windows directories managed by thresh, persisting across distro deletion. No admin privileges required!
- Include 'volumes' array: [{{""name"": ""postgres-data"", ""mount"": ""/var/lib/postgresql/data""}}]
- thresh creates directories in ~/.thresh/volumes/ and mounts them into WSL distros
- Volumes are automatically created and mounted during provisioning
- In 'setup' script, ensure mount point directories exist with proper ownership/permissions
- Volumes persist independently - survive 'thresh destroy' and can be reattached to new environments
- Data is stored in Windows filesystem, accessible from both Windows and WSL
- Examples:
  PostgreSQL: mkdir -p /var/lib/postgresql/data && chown postgres:postgres /var/lib/postgresql/data
  MySQL: mkdir -p /var/lib/mysql && chown mysql:mysql /var/lib/mysql
  Custom app: mkdir -p /data/myapp && chmod 755 /data/myapp" :
@"DOCKER MODE: Use named volumes for persistent data.
- Format: [{{""name"": ""postgres-data"", ""mount"": ""/var/lib/postgresql/data""}}]
- Docker manages volume lifecycle independently of containers
- Volumes persist even after container deletion
- Use volumes for databases, uploads, generated data")}

Requirements:
- Use 'base' for the distribution (not 'distribution')
- Use 'environment' for variables (not 'environment_variables')
- Use 'scripts' with 'setup' and 'postInstall' properties
- Include all necessary packages for the platform
{(isWslMode ? 
@"- For databases, ALWAYS include ""wslConfig"": ""database"" to avoid Plan9 filesystem permission errors
- If ports specified, MUST include setup script to configure service to listen on those ports
- If volumes specified, MUST create mount point directories with proper ownership/permissions in setup script" : 
"- Use standard container ports, map to host via ports array")}
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
            var isWslMode = platformName == "Windows";
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
  ""ports"": [""optional array of ports""],
  ""volumes"": [{{""name"": ""volume-name"", ""mount"": ""/data/path""}}],{(isWslMode ? @"
  ""wslConfig"": ""optional: profile name (systemd|docker|database|web-server|minimal|development)""," : "")}
  ""environment"": {{""KEY"": ""value""}},
  ""scripts"": {{
    ""setup"": ""#!/bin/bash\ncommands"",
    ""postInstall"": ""#!/bin/bash\ncommands""
  }}
}}

{(isWslMode ? @"WSL CONFIGURATION PROFILES - IMPORTANT:
WSL distros can be optimized with built-in profiles. Use 'wslConfig' property:

PROFILES:
- ""database"" - FOR DATABASES (PostgreSQL, MySQL, MongoDB, Redis)
  * Disables Windows interop/automount to avoid Plan9 filesystem permission errors
  * CRITICAL: Always use for database servers to prevent 'chmod' failures!

- ""docker"" - Auto-starts Docker daemon on boot
- ""web-server"" - Auto-starts Nginx web server  
- ""systemd"" - Minimal systemd enablement only
- ""minimal"" - Fast startup, no systemd, lightweight
- ""development"" - Full Windows integration (PATH, tools, auto-mount drives)

WHEN TO USE:
- Any database → USE ""database"" profile (CRITICAL for PostgreSQL, MySQL, MongoDB, Redis)
- Docker development → ""docker""
- Web servers → ""web-server""
- Full-stack dev with Windows tools → ""development""
- Lightweight/CI → ""minimal""
" : "")}

PORT CONFIGURATION - CRITICAL:
{(isWslMode ? 
@"WSL2 MODE: Ports are automatically forwarded from WSL to Windows localhost.
- Include 'ports' array with desired port numbers: [""8090"", ""5432""]
- In 'setup' script, configure the service to listen on the specified port
- WSL2 will automatically forward any listening port to Windows localhost
- Examples:
  Nginx on 8090: sed -i 's/listen 80;/listen 8090;/' /etc/nginx/sites-enabled/default
  PostgreSQL on 5433: sed -i 's/port = 5432/port = 5433/' /etc/postgresql/*/main/postgresql.conf
  Node.js server: Configure app.listen(PORT) in setup script" :
@"DOCKER MODE: Use 'hostPort:containerPort' format for port mapping.
- Format: [""8080:80"", ""5433:5432""]
- Maps container internal port to different host port
- Container service listens on standard port, host accesses via mapped port")}

VOLUME/STORAGE CONFIGURATION - CRITICAL:
{(isWslMode ? 
@"WSL2 MODE: Volumes are Windows directories managed by thresh, persisting across distro deletion. No admin privileges required!
- Include 'volumes' array: [{{""name"": ""postgres-data"", ""mount"": ""/var/lib/postgresql/data""}}]
- thresh creates directories in ~/.thresh/volumes/ and mounts them into WSL distros
- Volumes are automatically created and mounted during provisioning
- In 'setup' script, ensure mount point directories exist with proper ownership/permissions
- Volumes persist independently - survive 'thresh destroy' and can be reattached to new environments
- Data is stored in Windows filesystem, accessible from both Windows and WSL
- Examples:
  PostgreSQL: mkdir -p /var/lib/postgresql/data && chown postgres:postgres /var/lib/postgresql/data
  MySQL: mkdir -p /var/lib/mysql && chown mysql:mysql /var/lib/mysql
  Custom app: mkdir -p /data/myapp && chmod 755 /data/myapp" :
@"DOCKER MODE: Use named volumes for persistent data.
- Format: [{{""name"": ""postgres-data"", ""mount"": ""/var/lib/postgresql/data""}}]
- Docker manages volume lifecycle independently of containers
- Volumes persist even after container deletion
- Use volumes for databases, uploads, generated data")}

JSON is preferred for compatibility. YAML is also supported but use JSON unless specifically requested.
{(isWslMode ? "WSL environments support systemd and full services." : "Docker containers should avoid systemd and focus on lightweight, single-process setups.")}

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
