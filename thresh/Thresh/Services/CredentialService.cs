using System.Text.Json;
using Thresh.Models;

namespace Thresh.Services;

/// <summary>
/// Manages the CLI user credentials stored in ~/.thresh/credentials.json.
/// These are thresh_cli_* session tokens issued by the Hub device code flow
/// and are completely separate from the agent API keys in config.json.
/// </summary>
public class CredentialService
{
    private static readonly string CredentialsPath = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
        ".thresh", "credentials.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = AgentJsonContext.Default
    };

    /// <summary>
    /// Load stored credentials. Returns null if not logged in or credentials are corrupt.
    /// </summary>
    public StoredCredentials? Load()
    {
        if (!File.Exists(CredentialsPath))
            return null;

        try
        {
            var json = File.ReadAllText(CredentialsPath);
            return JsonSerializer.Deserialize(json, AgentJsonContext.Default.StoredCredentials);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Save credentials to disk. Sets file permissions to owner-read-only on Unix.
    /// </summary>
    public void Save(StoredCredentials creds)
    {
        var dir = Path.GetDirectoryName(CredentialsPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(creds, AgentJsonContext.Default.StoredCredentials);
        File.WriteAllText(CredentialsPath, json);

        // Restrict to owner only on Unix (chmod 600 equivalent)
        if (!OperatingSystem.IsWindows())
        {
            try
            {
                File.SetUnixFileMode(CredentialsPath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            catch { /* ignore if unsupported */ }
        }
    }

    /// <summary>
    /// Delete stored credentials (logout).
    /// </summary>
    public void Clear()
    {
        if (File.Exists(CredentialsPath))
            File.Delete(CredentialsPath);
    }

    /// <summary>
    /// Returns true if credentials are present and not yet expired.
    /// </summary>
    public bool IsValid(StoredCredentials? creds)
        => creds != null && creds.ExpiresAt > DateTime.UtcNow && !string.IsNullOrEmpty(creds.Token);

    /// <summary>
    /// Returns the THRESH_CLI_TOKEN env var if set, falling back to the stored token.
    /// This allows CI systems to inject a token without writing credentials.json.
    /// </summary>
    public string? GetEffectiveToken()
    {
        var envToken = System.Environment.GetEnvironmentVariable("THRESH_CLI_TOKEN");
        if (!string.IsNullOrWhiteSpace(envToken))
            return envToken;

        var creds = Load();
        return IsValid(creds) ? creds!.Token : null;
    }
}
