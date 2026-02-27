using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Thresh.Services;

/// <summary>
/// Service for managing WSL configuration profiles and validation
/// </summary>
public class WslConfigService
{
    private readonly string _profilesDirectory;
    private readonly string _userProfilesDirectory;
    
    // Valid wsl.conf sections and their allowed keys (from Microsoft documentation)
    private static readonly Dictionary<string, HashSet<string>> ValidSections = new()
    {
        ["boot"] = new HashSet<string> 
        { 
            "systemd", 
            "command", 
            "protectBinfmt" 
        },
        ["automount"] = new HashSet<string>
        {
            "enabled",
            "mountFsTab",
            "root",
            "options"
        },
        ["network"] = new HashSet<string>
        {
            "generateHosts",
            "generateResolvConf",
            "hostname"
        },
        ["interop"] = new HashSet<string>
        {
            "enabled",
            "appendWindowsPath"
        },
        ["user"] = new HashSet<string>
        {
            "default"
        },
        ["gpu"] = new HashSet<string>
        {
            "enabled"
        },
        ["time"] = new HashSet<string>
        {
            "useWindowsTimezone"
        }
    };
    
    public WslConfigService()
    {
        // Embedded profiles directory (inside the executable)
        _profilesDirectory = Path.Combine(AppContext.BaseDirectory, "Profiles");
        
        // User custom profiles directory
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _userProfilesDirectory = Path.Combine(homeDirectory, ".thresh", "profiles");
        
        // Ensure user profiles directory exists
        Directory.CreateDirectory(_userProfilesDirectory);
    }
    
    /// <summary>
    /// List all available profiles (built-in and user-defined)
    /// </summary>
    public List<WslConfigProfile> ListProfiles()
    {
        var profiles = new List<WslConfigProfile>();
        
        // Add built-in profiles
        var builtInProfiles = new[]
        {
            "systemd",
            "docker",
            "database",
            "web-server",
            "minimal",
            "development"
        };
        
        foreach (var profileName in builtInProfiles)
        {
            var profilePath = Path.Combine(_profilesDirectory, $"{profileName}.wslconf");
            if (File.Exists(profilePath))
            {
                var content = File.ReadAllText(profilePath);
                var description = ExtractDescription(content);
                profiles.Add(new WslConfigProfile
                {
                    Name = profileName,
                    Description = description,
                    IsBuiltIn = true,
                    Path = profilePath
                });
            }
        }
        
        // Add user-defined profiles
        if (Directory.Exists(_userProfilesDirectory))
        {
            var userProfiles = Directory.GetFiles(_userProfilesDirectory, "*.wslconf");
            foreach (var profilePath in userProfiles)
            {
                var profileName = Path.GetFileNameWithoutExtension(profilePath);
                var content = File.ReadAllText(profilePath);
                var description = ExtractDescription(content);
                profiles.Add(new WslConfigProfile
                {
                    Name = profileName,
                    Description = description,
                    IsBuiltIn = false,
                    Path = profilePath
                });
            }
        }
        
        return profiles;
    }
    
    /// <summary>
    /// Get the content of a profile by name
    /// </summary>
    public string? GetProfileContent(string profileName)
    {
        // Check built-in profiles first
        var builtInPath = Path.Combine(_profilesDirectory, $"{profileName}.wslconf");
        if (File.Exists(builtInPath))
        {
            return File.ReadAllText(builtInPath);
        }
        
        // Check user profiles
        var userPath = Path.Combine(_userProfilesDirectory, $"{profileName}.wslconf");
        if (File.Exists(userPath))
        {
            return File.ReadAllText(userPath);
        }
        
        return null;
    }
    
    /// <summary>
    /// Validate wsl.conf content
    /// </summary>
    public ValidationResult ValidateConfig(string content)
    {
        var result = new ValidationResult { IsValid = true };
        var lines = content.Split('\n', StringSplitOptions.None);
        string? currentSection = null;
        var lineNumber = 0;
        
        foreach (var rawLine in lines)
        {
            lineNumber++;
            var line = rawLine.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;
            
            // Section header
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentSection = line[1..^1].Trim();
                
                if (!ValidSections.ContainsKey(currentSection))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Line {lineNumber}: Unknown section [{currentSection}]");
                }
                
                continue;
            }
            
            // Key-value pair
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                
                if (currentSection == null)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Line {lineNumber}: Key '{key}' is not inside a section");
                    continue;
                }
                
                if (!ValidSections[currentSection].Contains(key))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Line {lineNumber}: Unknown key '{key}' in section [{currentSection}]");
                }
                
                // Validate boolean values
                if (IsBooleanKey(currentSection, key))
                {
                    if (value.ToLower() != "true" && value.ToLower() != "false")
                    {
                        result.Warnings.Add($"Line {lineNumber}: Key '{key}' expects boolean value (true/false), got '{value}'");
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                result.IsValid = false;
                result.Errors.Add($"Line {lineNumber}: Invalid format - expected 'key=value'");
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Display all available wsl.conf options from Microsoft documentation
    /// </summary>
    public void DisplayOptions()
    {
        Console.WriteLine("WSL Configuration Options (wsl.conf)");
        Console.WriteLine("====================================");
        Console.WriteLine();
        Console.WriteLine("Based on Microsoft WSL documentation:");
        Console.WriteLine("https://learn.microsoft.com/en-us/windows/wsl/wsl-config");
        Console.WriteLine();
        
        DisplaySection("boot", "Boot settings (Windows 11+ and Server 2022)", new Dictionary<string, string>
        {
            ["systemd"] = "boolean (true/false) - Enable systemd support",
            ["command"] = "string - Command to run when WSL instance starts (runs as root)",
            ["protectBinfmt"] = "boolean (true/false) - Prevents WSL from generating systemd units"
        });
        
        DisplaySection("automount", "Automount settings for Windows drives", new Dictionary<string, string>
        {
            ["enabled"] = "boolean (true/false) - Auto-mount fixed drives under /mnt",
            ["mountFsTab"] = "boolean (true/false) - Process /etc/fstab on WSL start",
            ["root"] = "string (path) - Directory where drives will be mounted (default: /mnt/)",
            ["options"] = "comma-separated - DrvFs mount options (uid, gid, umask, fmask, dmask, metadata, case)"
        });
        
        DisplaySection("network", "Network host settings", new Dictionary<string, string>
        {
            ["generateHosts"] = "boolean (true/false) - Generate /etc/hosts file",
            ["generateResolvConf"] = "boolean (true/false) - Generate /etc/resolv.conf for DNS",
            ["hostname"] = "string - Custom hostname for WSL distribution"
        });
        
        DisplaySection("interop", "Windows interoperability settings", new Dictionary<string, string>
        {
            ["enabled"] = "boolean (true/false) - Support launching Windows processes from WSL",
            ["appendWindowsPath"] = "boolean (true/false) - Add Windows PATH to $PATH environment variable"
        });
        
        DisplaySection("user", "User settings", new Dictionary<string, string>
        {
            ["default"] = "string (username) - Default user when starting WSL session"
        });
        
        DisplaySection("gpu", "GPU settings", new Dictionary<string, string>
        {
            ["enabled"] = "boolean (true/false) - Allow Linux apps to access Windows GPU"
        });
        
        DisplaySection("time", "Time settings", new Dictionary<string, string>
        {
            ["useWindowsTimezone"] = "boolean (true/false) - Use and sync to Windows timezone"
        });
        
        Console.WriteLine();
        Console.WriteLine("Example wsl.conf:");
        Console.WriteLine("==================");
        Console.WriteLine();
        Console.WriteLine("# Enable systemd for modern services");
        Console.WriteLine("[boot]");
        Console.WriteLine("systemd=true");
        Console.WriteLine("command=service docker start");
        Console.WriteLine();
        Console.WriteLine("# Disable Windows integration for performance");
        Console.WriteLine("[interop]");
        Console.WriteLine("enabled=false");
        Console.WriteLine("appendWindowsPath=false");
        Console.WriteLine();
        Console.WriteLine("# Custom mount options");
        Console.WriteLine("[automount]");
        Console.WriteLine("enabled=true");
        Console.WriteLine("root=/mnt");
        Console.WriteLine("options=metadata,uid=1000,gid=1000,umask=077");
        Console.WriteLine();
        Console.WriteLine("# Network configuration");
        Console.WriteLine("[network]");
        Console.WriteLine("hostname=dev-server");
        Console.WriteLine("generateHosts=true");
        Console.WriteLine();
        Console.WriteLine("# Set default user");
        Console.WriteLine("[user]");
        Console.WriteLine("default=developer");
        Console.WriteLine();
    }
    
    private void DisplaySection(string sectionName, string description, Dictionary<string, string> keys)
    {
        Console.WriteLine($"[{sectionName}]");
        Console.WriteLine($"  {description}");
        Console.WriteLine();
        
        foreach (var kvp in keys)
        {
            Console.WriteLine($"  {kvp.Key}");
            Console.WriteLine($"    {kvp.Value}");
            Console.WriteLine();
        }
    }
    
    private bool IsBooleanKey(string section, string key)
    {
        var booleanKeys = new HashSet<string>
        {
            "systemd", "enabled", "mountFsTab", "generateHosts", 
            "generateResolvConf", "appendWindowsPath", "protectBinfmt",
            "useWindowsTimezone"
        };
        
        return booleanKeys.Contains(key);
    }
    
    private string ExtractDescription(string content)
    {
        var lines = content.Split('\n');
        var description = new StringBuilder();
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("#"))
            {
                var comment = trimmed[1..].Trim();
                if (!string.IsNullOrWhiteSpace(comment) && 
                    !comment.Contains("Profile") && 
                    !comment.Contains("===="))
                {
                    description.Append(comment);
                    description.Append(" ");
                }
            }
            else if (!string.IsNullOrWhiteSpace(trimmed))
            {
                break; // Stop at first non-comment line
            }
        }
        
        return description.ToString().Trim();
    }
}

/// <summary>
/// Represents a WSL configuration profile
/// </summary>
public class WslConfigProfile
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Result of wsl.conf validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
