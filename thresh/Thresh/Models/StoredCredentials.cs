namespace Thresh.Models;

/// <summary>
/// Credentials stored in ~/.thresh/credentials.json after a successful
/// thresh auth login. The token is a thresh_cli_* session token issued
/// by the Hub device code flow.
/// </summary>
public class StoredCredentials
{
    /// <summary>Hub URL this token was issued by</summary>
    public string HubUrl { get; set; } = string.Empty;

    /// <summary>The CLI session token (thresh_cli_...)</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Email of the authenticated user</summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>Display name of the authenticated user</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>When this session expires (UTC)</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Device name reported to the Hub</summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>Roles the user holds (Admin, Developer, etc.)</summary>
    public List<string> Roles { get; set; } = new();
}
