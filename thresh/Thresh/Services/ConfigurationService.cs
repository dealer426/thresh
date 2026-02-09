using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Thresh.Models;

namespace Thresh.Services;

/// <summary>
/// Service for managing user configuration and secure API key storage
/// Config file: ~/.thresh/config.json
/// Sensitive values are encrypted at rest
/// </summary>
public class ConfigurationService
{
    private static readonly string ConfigDirectory = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
        ".thresh"
    );
    
    private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");
    private static readonly byte[] AdditionalEntropy = Encoding.UTF8.GetBytes("thresh-cli-v1");

    private ConfigurationSettings? _cachedSettings;

    /// <summary>
    /// Initialize configuration directory
    /// </summary>
    public void Initialize()
    {
        if (!Directory.Exists(ConfigDirectory))
        {
            Directory.CreateDirectory(ConfigDirectory);
            
            // Set permissions on Unix systems
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    File.SetUnixFileMode(ConfigDirectory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                }
                catch
                {
                    // Ignore if setting permissions fails
                }
            }
        }
    }

    /// <summary>
    /// Load configuration settings
    /// </summary>
    public ConfigurationSettings Load()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        Initialize();

        if (!File.Exists(ConfigFilePath))
        {
            _cachedSettings = new ConfigurationSettings();
            return _cachedSettings;
        }

        try
        {
            var json = File.ReadAllText(ConfigFilePath);
            var settings = JsonSerializer.Deserialize(json, ConfigurationJsonContext.Default.ConfigurationSettings) ?? new ConfigurationSettings();
            
            // Decrypt sensitive values
            settings.OpenAIApiKey = DecryptValue(settings.OpenAIApiKey);
            settings.AzureOpenAIApiKey = DecryptValue(settings.AzureOpenAIApiKey);
            settings.GitHubToken = DecryptValue(settings.GitHubToken);

            _cachedSettings = settings;
            return settings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to load config: {ex.Message}");
            _cachedSettings = new ConfigurationSettings();
            return _cachedSettings;
        }
    }

    /// <summary>
    /// Save configuration settings
    /// </summary>
    public void Save(ConfigurationSettings settings)
    {
        Initialize();

        try
        {
            // Clone settings to avoid modifying the original
            var settingsToSave = new ConfigurationSettings
            {
                OpenAIApiKey = EncryptValue(settings.OpenAIApiKey),
                AzureOpenAIEndpoint = settings.AzureOpenAIEndpoint,
                AzureOpenAIApiKey = EncryptValue(settings.AzureOpenAIApiKey),
                DefaultModel = settings.DefaultModel,
                GitHubToken = EncryptValue(settings.GitHubToken),
                EnableTelemetry = settings.EnableTelemetry,
                DefaultBase = settings.DefaultBase,
                CustomDistributions = new Dictionary<string, CustomDistribution>(settings.CustomDistributions),
                CustomSettings = new Dictionary<string, string>(settings.CustomSettings)
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                TypeInfoResolver = ConfigurationJsonContext.Default
            };

            var json = JsonSerializer.Serialize(settingsToSave, ConfigurationJsonContext.Default.ConfigurationSettings);
            File.WriteAllText(ConfigFilePath, json);

            // Set file permissions on Unix systems
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    File.SetUnixFileMode(ConfigFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
                catch
                {
                    // Ignore if setting permissions fails
                }
            }

            _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Set a configuration value
    /// </summary>
    public void SetValue(string key, string value)
    {
        var settings = Load();
        var normalizedKey = key.ToLowerInvariant().Replace("_", "-");

        switch (normalizedKey)
        {
            case "openai-api-key":
            case "openai-key":
                settings.OpenAIApiKey = value;
                break;

            case "azure-openai-endpoint":
            case "azure-endpoint":
                settings.AzureOpenAIEndpoint = value;
                break;

            case "azure-openai-api-key":
            case "azure-key":
                settings.AzureOpenAIApiKey = value;
                break;

            case "default-model":
            case "model":
                settings.DefaultModel = value;
                break;

            case "github-token":
            case "gh-token":
                settings.GitHubToken = value;
                break;

            case "enable-telemetry":
            case "telemetry":
                settings.EnableTelemetry = bool.Parse(value);
                break;

            case "default-base":
            case "base":
                settings.DefaultBase = value;
                break;

            default:
                settings.CustomSettings[key] = value;
                break;
        }

        Save(settings);
    }

    /// <summary>
    /// Get a configuration value
    /// </summary>
    public string? GetValue(string key)
    {
        var settings = Load();
        var normalizedKey = key.ToLowerInvariant().Replace("_", "-");

        return normalizedKey switch
        {
            "openai-api-key" or "openai-key" => MaskSensitiveValue(settings.OpenAIApiKey),
            "azure-openai-endpoint" or "azure-endpoint" => settings.AzureOpenAIEndpoint,
            "azure-openai-api-key" or "azure-key" => MaskSensitiveValue(settings.AzureOpenAIApiKey),
            "default-model" or "model" => settings.DefaultModel,
            "github-token" or "gh-token" => MaskSensitiveValue(settings.GitHubToken),
            "enable-telemetry" or "telemetry" => settings.EnableTelemetry.ToString(),
            "default-base" or "base" => settings.DefaultBase,
            _ => settings.CustomSettings.TryGetValue(key, out var value) ? value : null
        };
    }

    /// <summary>
    /// Get the actual unmasked value for a secret/API key (for internal use only)
    /// </summary>
    public string? GetSecretValue(string key)
    {
        var settings = Load();
        var normalizedKey = key.ToLowerInvariant().Replace("_", "-");

        return normalizedKey switch
        {
            "openai-api-key" or "openai-key" => settings.OpenAIApiKey,
            "azure-openai-api-key" or "azure-key" => settings.AzureOpenAIApiKey,
            "github-token" or "gh-token" => settings.GitHubToken,
            _ => null
        };
    }

    /// <summary>
    /// Delete a configuration value
    /// </summary>
    public void DeleteValue(string key)
    {
        var settings = Load();
        var normalizedKey = key.ToLowerInvariant().Replace("_", "-");

        switch (normalizedKey)
        {
            case "openai-api-key":
            case "openai-key":
                settings.OpenAIApiKey = null;
                break;

            case "azure-openai-endpoint":
            case "azure-endpoint":
                settings.AzureOpenAIEndpoint = null;
                break;

            case "azure-openai-api-key":
            case "azure-key":
                settings.AzureOpenAIApiKey = null;
                break;

            case "github-token":
            case "gh-token":
                settings.GitHubToken = null;
                break;

            default:
                settings.CustomSettings.Remove(key);
                break;
        }

        Save(settings);
    }

    /// <summary>
    /// List all configuration values
    /// </summary>
    public Dictionary<string, string?> ListAll()
    {
        var settings = Load();
        var result = new Dictionary<string, string?>
        {
            ["openai-api-key"] = MaskSensitiveValue(settings.OpenAIApiKey),
            ["azure-openai-endpoint"] = settings.AzureOpenAIEndpoint,
            ["azure-openai-api-key"] = MaskSensitiveValue(settings.AzureOpenAIApiKey),
            ["default-model"] = settings.DefaultModel,
            ["github-token"] = MaskSensitiveValue(settings.GitHubToken),
            ["enable-telemetry"] = settings.EnableTelemetry.ToString(),
            ["default-base"] = settings.DefaultBase
        };

        foreach (var (key, value) in settings.CustomSettings)
        {
            result[key] = value;
        }

        return result;
    }

    /// <summary>
    /// Encrypt a sensitive value using platform-specific encryption
    /// </summary>
    private string? EncryptValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                // Use DPAPI on Windows
                var bytes = Encoding.UTF8.GetBytes(value);
                var encrypted = ProtectedData.Protect(bytes, AdditionalEntropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            else
            {
                // Simple base64 encoding on Unix (could be enhanced with keyring integration)
                // In production, consider using platform keychains (libsecret, Keychain Access, etc.)
                var bytes = Encoding.UTF8.GetBytes(value);
                return "B64:" + Convert.ToBase64String(bytes);
            }
        }
        catch
        {
            // If encryption fails, store as plain text (not ideal but better than failing)
            return value;
        }
    }

    /// <summary>
    /// Decrypt a sensitive value
    /// </summary>
    private string? DecryptValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        try
        {
            // Check if it's a base64-encoded Unix value
            if (value.StartsWith("B64:"))
            {
                var base64 = value.Substring(4);
                var bytes = Convert.FromBase64String(base64);
                var decrypted = Encoding.UTF8.GetString(bytes);
                return decrypted;
            }

            if (OperatingSystem.IsWindows())
            {
                // Decrypt using DPAPI on Windows
                var encrypted = Convert.FromBase64String(value);
                var bytes = ProtectedData.Unprotect(encrypted, AdditionalEntropy, DataProtectionScope.CurrentUser);
                var decrypted = Encoding.UTF8.GetString(bytes);
                
                // Validate that decryption worked - check if result is printable ASCII/UTF-8
                if (IsValidDecryption(decrypted))
                {
                    return decrypted;
                }
                else
                {
                    Console.Error.WriteLine($"⚠️  Decryption produced invalid output. Value may be corrupted.");
                    throw new InvalidOperationException("Decryption failed - invalid output");
                }
            }
            else
            {
                // If not prefixed, it might be plain text (legacy)
                return value;
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            // Log the actual error
            Console.Error.WriteLine($"❌ Decryption failed: {ex.GetType().Name}");
            Console.Error.WriteLine($"   This usually means the key was encrypted on a different machine or user account.");
            Console.Error.WriteLine($"   Please delete and re-set the configuration value.");
            
            // Don't silently return encrypted value - throw so the caller knows something is wrong
            throw new InvalidOperationException(
                $"Failed to decrypt configuration value. It may have been encrypted on a different machine or user account. " +
                $"Please delete and re-set the value using 'thresh config set <key> <value>'.", ex);
        }
    }

    /// <summary>
    /// Check if decrypted value is valid (contains printable characters)
    /// </summary>
    private bool IsValidDecryption(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // Check if value contains mostly printable ASCII characters (API keys should be)
        int printableCount = value.Count(c => c >= 32 && c <= 126);
        double printableRatio = (double)printableCount / value.Length;
        
        return printableRatio > 0.95; // At least 95% printable characters
    }

    /// <summary>
    /// Mask a value for debug output
    /// </summary>
    private string MaskForDebug(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "[empty]";
        
        if (value.Length <= 8)
            return new string('*', value.Length);
        
        return $"{value.Substring(0, 4)}...{value.Substring(value.Length - 4)}";
    }

    /// <summary>
    /// Mask sensitive values for display
    /// </summary>
    private string? MaskSensitiveValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (value.Length <= 8)
            return "***";

        return value.Substring(0, 4) + "..." + value.Substring(value.Length - 4);
    }

    /// <summary>
    /// Get the raw (decrypted) value for internal use
    /// </summary>
    public string? GetRawValue(string key)
    {
        var settings = Load();
        var normalizedKey = key.ToLowerInvariant().Replace("_", "-");

        return normalizedKey switch
        {
            "openai-api-key" or "openai-key" => settings.OpenAIApiKey,
            "azure-openai-endpoint" or "azure-endpoint" => settings.AzureOpenAIEndpoint,
            "azure-openai-api-key" or "azure-key" => settings.AzureOpenAIApiKey,
            "default-model" or "model" => settings.DefaultModel,
            "github-token" or "gh-token" => settings.GitHubToken,
            "enable-telemetry" or "telemetry" => settings.EnableTelemetry.ToString(),
            "default-base" or "base" => settings.DefaultBase,
            _ => settings.CustomSettings.TryGetValue(key, out var value) ? value : null
        };
    }

    /// <summary>
    /// Reset all configuration to defaults
    /// </summary>
    public void Reset()
    {
        if (File.Exists(ConfigFilePath))
        {
            File.Delete(ConfigFilePath);
        }

        _cachedSettings = null;
    }
}
