using System.CommandLine;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Thresh.Models;

namespace Thresh.Services;

// ---------------------------------------------------------------------------
// AOT source-generation context for auth HTTP DTOs
// ---------------------------------------------------------------------------
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(DeviceStartRequest))]
[JsonSerializable(typeof(DeviceStartResponse))]
[JsonSerializable(typeof(DeviceTokenRequest))]
[JsonSerializable(typeof(DeviceTokenResponse))]
internal partial class AuthJsonContext : JsonSerializerContext { }

internal class DeviceStartRequest
{
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceOs { get; set; } = string.Empty;
}

internal class DeviceStartResponse
{
    public string DeviceCode { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public int Interval { get; set; }
}

internal class DeviceTokenRequest
{
    public string DeviceCode { get; set; } = string.Empty;
}

internal class DeviceTokenResponse
{
    public string? Status { get; set; }
    public string? AccessToken { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string>? Roles { get; set; }
}

/// <summary>
/// CLI auth commands: login, logout, status, token
/// Implements Azure CLI-style device code flow against the thresh-hub.
/// </summary>
public static class AuthCommands
{
    private const string DefaultHubUrl = "https://thresh.io";
    private const int PollIntervalMs = 5000;
    private const int MaxPollSeconds = 300; // 5 minutes hard limit

    public static void Register(RootCommand rootCommand)
    {
        var authCommand = new Command("auth", "Manage authentication with thresh-hub");

        authCommand.AddCommand(BuildLoginCommand());
        authCommand.AddCommand(BuildLogoutCommand());
        authCommand.AddCommand(BuildStatusCommand());
        authCommand.AddCommand(BuildTokenCommand());

        rootCommand.AddCommand(authCommand);
    }

    // -------------------------------------------------------------------------
    // thresh auth login
    // -------------------------------------------------------------------------
    private static Command BuildLoginCommand()
    {
        var loginCommand = new Command("login", "Authenticate with a thresh-hub");

        var hubOption = new Option<string?>(
            aliases: ["--hub", "-h"],
            description: "Hub URL (default: from config or https://thresh.io)");

        var noBrowserOption = new Option<bool>(
            aliases: ["--no-browser"],
            description: "Print the activation URL instead of opening a browser");

        var tokenOption = new Option<string?>(
            aliases: ["--token", "-t"],
            description: "Login directly with an existing thresh_cli_* token");

        loginCommand.AddOption(hubOption);
        loginCommand.AddOption(noBrowserOption);
        loginCommand.AddOption(tokenOption);

        loginCommand.SetHandler(async (string? hub, bool noBrowser, string? token) =>
        {
            var hubUrl = hub ?? GetHubUrlFromConfig() ?? DefaultHubUrl;
            hubUrl = hubUrl.TrimEnd('/');

            // Direct token injection (CI / copy-paste scenario)
            if (!string.IsNullOrWhiteSpace(token))
            {
                await LoginWithDirectTokenAsync(hubUrl, token);
                return;
            }

            await LoginWithDeviceFlowAsync(hubUrl, noBrowser);

        }, hubOption, noBrowserOption, tokenOption);

        return loginCommand;
    }

    private static async Task LoginWithDirectTokenAsync(string hubUrl, string token)
    {
        Console.WriteLine("Validating token...");

        using var client = CreateHttpClient(hubUrl, token);
        try
        {
            var resp = await client.GetAsync("/api/auth/sessions");
            if (!resp.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Token rejected by hub ({(int)resp.StatusCode} {resp.ReasonPhrase})");
                Console.ResetColor();
                return;
            }

            // Parse user info from sessions response if available, otherwise just save
            var creds = new StoredCredentials
            {
                HubUrl = hubUrl,
                Token = token,
                UserEmail = "(direct token)",
                UserName = "(direct token)",
                ExpiresAt = DateTime.UtcNow.AddDays(90),
                DeviceName = GetDeviceName(),
                Roles = []
            };

            new CredentialService().Save(creds);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Logged in to {hubUrl}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Failed to validate token: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static async Task LoginWithDeviceFlowAsync(string hubUrl, bool noBrowser)
    {
        using var client = CreateHttpClient(hubUrl, token: null);

        // Step 1: Start device flow
        DeviceStartResponse? startResp;
        try
        {
            var startReq = new DeviceStartRequest
            {
                DeviceName = GetDeviceName(),
                DeviceOs = GetDeviceOs()
            };

            var body = JsonContent.Create(startReq, AuthJsonContext.Default.DeviceStartRequest);

            var resp = await client.PostAsync("/api/auth/device/start", body);
            if (!resp.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Hub returned {(int)resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
                Console.ResetColor();
                return;
            }

            startResp = await resp.Content.ReadFromJsonAsync(AuthJsonContext.Default.DeviceStartResponse);

            if (startResp == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Hub returned an empty response");
                Console.ResetColor();
                return;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Could not reach hub at {hubUrl}: {ex.Message}");
            Console.ResetColor();
            return;
        }

        // Step 2: Show user the code + activation URL
        var activateUrl = $"{hubUrl}/activate?code={Uri.EscapeDataString(startResp.UserCode)}";

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  To authenticate, open a browser and enter the code below:");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  Activation URL:  {activateUrl}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Code:            {startResp.UserCode}");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  Waiting for authentication...");
        Console.WriteLine();

        if (!noBrowser)
        {
            TryOpenBrowser(activateUrl);
        }

        // Step 3: Poll /api/auth/device/token
        var deadline = DateTime.UtcNow.AddSeconds(MaxPollSeconds);
        var pollReq = new DeviceTokenRequest { DeviceCode = startResp.DeviceCode };

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(PollIntervalMs);

            try
            {
                var body = JsonContent.Create(pollReq, AuthJsonContext.Default.DeviceTokenRequest);
                var pollResp = await client.PostAsync("/api/auth/device/token", body);

                if (!pollResp.IsSuccessStatusCode)
                    continue; // transient error, keep polling

                var tokenResp = await pollResp.Content.ReadFromJsonAsync(AuthJsonContext.Default.DeviceTokenResponse);

                if (tokenResp == null)
                    continue;

                switch (tokenResp.Status?.ToLowerInvariant())
                {
                    case "pending":
                        // Still waiting, print a dot and continue
                        Console.Write(".");
                        continue;

                    case "access_denied":
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("❌ Authentication was denied.");
                        Console.ResetColor();
                        return;

                    case "expired":
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("❌ Code expired. Run 'thresh auth login' again.");
                        Console.ResetColor();
                        return;

                    case "approved":
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✅ Authentication successful!");
                        Console.ResetColor();

                        var creds = new StoredCredentials
                        {
                            HubUrl = hubUrl,
                            Token = tokenResp.AccessToken ?? string.Empty,
                            UserEmail = tokenResp.UserEmail ?? string.Empty,
                            UserName = tokenResp.UserName ?? string.Empty,
                            ExpiresAt = tokenResp.ExpiresAt ?? DateTime.UtcNow.AddDays(90),
                            DeviceName = GetDeviceName(),
                            Roles = tokenResp.Roles ?? []
                        };

                        new CredentialService().Save(creds);

                        Console.WriteLine();
                        Console.WriteLine($"  Logged in as: {creds.UserEmail}");
                        Console.WriteLine($"  Hub:          {hubUrl}");
                        Console.WriteLine($"  Expires:      {creds.ExpiresAt:yyyy-MM-dd}");
                        if (creds.Roles.Count > 0)
                            Console.WriteLine($"  Roles:        {string.Join(", ", creds.Roles)}");
                        Console.WriteLine();
                        return;

                    default:
                        // Unknown status — continue polling
                        continue;
                }
            }
            catch
            {
                // Network blip — keep polling
                await Task.Delay(PollIntervalMs);
            }
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ Timed out waiting for authentication. Run 'thresh auth login' again.");
        Console.ResetColor();
    }

    // -------------------------------------------------------------------------
    // thresh auth logout
    // -------------------------------------------------------------------------
    private static Command BuildLogoutCommand()
    {
        var logoutCommand = new Command("logout", "Sign out and revoke the current CLI session");

        logoutCommand.SetHandler(async () =>
        {
            var credService = new CredentialService();
            var creds = credService.Load();

            if (creds == null || !credService.IsValid(creds))
            {
                Console.WriteLine("Not currently logged in.");
                credService.Clear(); // clean up any stale file
                return;
            }

            // Try to revoke the session on the hub
            using var client = CreateHttpClient(creds.HubUrl, creds.Token);
            try
            {
                var resp = await client.DeleteAsync("/api/auth/sessions/current");
                if (resp.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Session revoked on hub.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠️  Hub returned {(int)resp.StatusCode} — session may already be expired.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️  Could not reach hub to revoke session: {ex.Message}");
                Console.WriteLine("    Credentials cleared locally.");
                Console.ResetColor();
            }
            finally
            {
                credService.Clear();
            }

            Console.WriteLine("Logged out.");
        });

        return logoutCommand;
    }

    // -------------------------------------------------------------------------
    // thresh auth status
    // -------------------------------------------------------------------------
    private static Command BuildStatusCommand()
    {
        var statusCommand = new Command("status", "Show current authentication status");

        statusCommand.SetHandler(() =>
        {
            // Check THRESH_CLI_TOKEN env var first
            var envToken = System.Environment.GetEnvironmentVariable("THRESH_CLI_TOKEN");
            if (!string.IsNullOrWhiteSpace(envToken))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Authenticated via THRESH_CLI_TOKEN environment variable");
                Console.ResetColor();
                return;
            }

            var credService = new CredentialService();
            var creds = credService.Load();

            if (creds == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Not logged in. Run 'thresh auth login' to authenticate.");
                Console.ResetColor();
                return;
            }

            if (!credService.IsValid(creds))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Session expired ({creds.ExpiresAt:yyyy-MM-dd HH:mm} UTC).");
                Console.WriteLine("Run 'thresh auth login' to re-authenticate.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Logged in");
            Console.ResetColor();
            Console.WriteLine($"  User:     {creds.UserEmail}");
            Console.WriteLine($"  Hub:      {creds.HubUrl}");
            Console.WriteLine($"  Device:   {creds.DeviceName}");
            Console.WriteLine($"  Expires:  {creds.ExpiresAt:yyyy-MM-dd} ({DaysRemaining(creds.ExpiresAt)} days remaining)");
            if (creds.Roles.Count > 0)
                Console.WriteLine($"  Roles:    {string.Join(", ", creds.Roles)}");
        });

        return statusCommand;
    }

    // -------------------------------------------------------------------------
    // thresh auth token
    // -------------------------------------------------------------------------
    private static Command BuildTokenCommand()
    {
        var tokenCommand = new Command("token", "Print the current access token (for scripting)");

        tokenCommand.SetHandler(() =>
        {
            var token = new CredentialService().GetEffectiveToken();
            if (token == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Not authenticated. Run 'thresh auth login'.");
                Console.ResetColor();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine(token);
            }
        });

        return tokenCommand;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private static HttpClient CreateHttpClient(string hubUrl, string? token)
    {
        var handler = new HttpClientHandler
        {
            // Allow self-signed certs for local dev hubs
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(hubUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        client.DefaultRequestHeaders.Add("User-Agent", "thresh-cli/1.0");

        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        return client;
    }

    private static string GetDeviceName()
        => System.Environment.MachineName ?? "unknown";

    private static string GetDeviceOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return $"Windows {System.Environment.OSVersion.Version.Major}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS";
        return "Linux";
    }

    private static string? GetHubUrlFromConfig()
    {
        try
        {
            var configPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                ".thresh", "config.json");

            if (!File.Exists(configPath))
                return null;

            var json = File.ReadAllText(configPath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("hubUrl", out var val))
                return val.GetString();
        }
        catch { /* ignore */ }

        return null;
    }

    private static int DaysRemaining(DateTime expiresAt)
        => Math.Max(0, (int)(expiresAt - DateTime.UtcNow).TotalDays);

    private static void TryOpenBrowser(string url)
    {
        // Append ?popup=1 so the hub page hides its nav and closes after approve/deny
        var popupUrl = url.Contains('?') ? url + "&popup=1" : url + "?popup=1";

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Resolve via registry App Paths (same mechanism Windows uses for ShellExecute)
                // then fall back to well-known install paths, then default browser.
                var chromePath = GetWindowsBrowserPath("chrome.exe");
                var edgePath   = GetWindowsBrowserPath("msedge.exe");

                string[] candidates = [
                    chromePath ?? @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                    @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                    edgePath   ?? @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                    @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                ];

                var launched = false;
                foreach (var path in candidates)
                {
                    if (path != null && File.Exists(path) && TryLaunchAppWindow(path, popupUrl))
                    {
                        launched = true;
                        break;
                    }
                }

                if (!launched)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = popupUrl,
                        UseShellExecute = true
                    });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Try Chrome app mode on macOS
                var launched = TryLaunchAppWindow("/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", popupUrl)
                            || TryLaunchAppWindow("/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge", popupUrl);
                if (!launched)
                    System.Diagnostics.Process.Start("open", popupUrl);
            }
            else
            {
                System.Diagnostics.Process.Start("xdg-open", popupUrl);
            }
        }
        catch { /* user sees the URL printed above and can open manually */ }
    }

    private static bool TryLaunchAppWindow(string browser, string url)
    {
        try
        {
            // --user-data-dir points to a throw-away profile so Chrome/Edge creates a
            // fresh standalone app window even when the browser is already running.
            var tempProfile = Path.Combine(Path.GetTempPath(), "thresh-auth-popup");
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = browser,
                Arguments = $"--app={url} --window-size=480,640 --window-position=400,150 --disable-extensions --user-data-dir=\"{tempProfile}\"",
                UseShellExecute = false
            };
            var proc = System.Diagnostics.Process.Start(psi);
            return proc != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Looks up the install path for a browser exe via the Windows registry
    /// HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{exe}
    /// This is the same mechanism Windows uses for ShellExecute by name.
    /// Returns null if the key doesn't exist.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static string? GetWindowsBrowserPath(string exeName)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                $@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{exeName}");
            return key?.GetValue(null) as string;
        }
        catch { return null; }
    }
}

