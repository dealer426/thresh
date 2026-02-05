# Distribution List Command - Complete

## Summary
Added `thresh distros` command to show all available distributions with their sources.

## Changes Made

### 1. New Command: `thresh distros`
- **Location**: `Program.cs` - `AddDistrosCommand()` method
- **Purpose**: Display all built-in and custom distributions with source information
- **Usage**: `thresh distros`

### 2. Output Format
```
NAME                      VERSION         SOURCE               PKG MANAGER
--------------------------------------------------------------------------------
alpine-3.18               3.18            Vendor               Apk
alpine-3.19               3.19            Vendor               Apk
...
kali                      rolling         Microsoft Store      Apt
opensuse-leap             15.6            Microsoft Store      Zypper
...
```

### 3. Distribution Sources Tracked
- **Vendor**: Direct rootfs downloads (Ubuntu, Alpine, Debian, Fedora, Rocky)
- **Microsoft Store**: WSL distributions via `wsl --install` wrapper (Kali, Oracle, openSUSE)
- **Custom**: User-added distributions via `thresh distro add`

## Implementation Details

### Key Features
1. **Grouped Display**: Distributions are grouped by source for readability
2. **Complete Information**: Shows name, version, source, and package manager
3. **Total Count**: Displays total built-in + custom distributions
4. **Sorted Output**: Alphabetically sorted within each group

### Code Structure
```csharp
private static void AddDistrosCommand(RootCommand rootCommand)
{
    var distrosCommand = new Command("distros", "List all available distributions");
    
    distrosCommand.SetHandler(() =>
    {
        // 1. Get all built-in distribution keys
        var registry = new Services.RootfsRegistry(new Services.ConfigurationService());
        var allDistroKeys = registry.GetSupportedDistributions();
        
        // 2. Build list with full distribution info
        var distroInfoList = new List<(string Key, Services.RootfsRegistry.DistributionInfo Info)>();
        foreach (var key in allDistroKeys)
        {
            var info = registry.GetDistribution(key);
            if (info != null)
                distroInfoList.Add((key, info));
        }
        
        // 3. Group by source (Vendor vs Microsoft Store)
        var vendorDistros = distroInfoList.Where(...).OrderBy(...);
        var msStoreDistros = distroInfoList.Where(...).OrderBy(...);
        
        // 4. Display each group
        // 5. Show custom distributions
        // 6. Display total count
    });
}
```

## Current Distribution Count
- **10 Vendor Distributions**: ubuntu (3), alpine (3), debian (2), fedora (1), rocky (1)
- **5 Microsoft Store Distributions**: kali, oracle-8, oracle-9, opensuse-leap, opensuse-tumbleweed
- **Total Built-in**: 15 distributions
- **Custom**: User-configurable via `thresh distro add`

## Testing Results

### Build Status
✅ Build successful with Native AOT
- Binary Size: **12 MB** (Native AOT enabled)
- Warnings: 3 (acceptable - 1 async, 2 Azure SDK AOT)

### Command Output
```bash
$ thresh distros
Available distributions:

NAME                      VERSION         SOURCE               PKG MANAGER
--------------------------------------------------------------------------------
# Vendor Distributions
alpine-3.18               3.18            Vendor               Apk
alpine-3.19               3.19            Vendor               Apk
alpine-edge               edge            Vendor               Apk
debian-11                 11              Vendor               Apt
debian-12                 12              Vendor               Apt
fedora-41                 41              Vendor               Dnf
rocky-9                   9               Vendor               Dnf
ubuntu-20.04              20.04           Vendor               Apt
ubuntu-22.04              22.04           Vendor               Apt
ubuntu-24.04              24.04           Vendor               Apt

# Microsoft Store Distributions
kali                      rolling         Microsoft Store      Apt
opensuse-leap             15.6            Microsoft Store      Zypper
opensuse-tumbleweed       tumbleweed      Microsoft Store      Zypper
oracle-8                  8.10            Microsoft Store      Dnf
oracle-9                  9.5             Microsoft Store      Dnf

Total: 15 built-in + 0 custom
```

### Help Integration
```bash
$ thresh --help
Commands:
  ...
  distro             Manage custom distributions
  distros            List all available distributions  ← NEW
  serve              Start MCP server
  version            Display version information
```

## User Experience

### Before
- Users could only see custom distributions via `thresh distro list`
- No visibility into built-in distributions
- Unknown which distros were from vendor vs Microsoft Store

### After
- Single command shows ALL available distributions
- Clear source attribution (Vendor/Microsoft Store/Custom)
- Easy to see which distributions are available out-of-the-box
- Helps users make informed choices when running `thresh up`

## Related Commands

### Existing Commands
- `thresh distro list` - Show custom distributions only
- `thresh distro add <name>` - Add custom distribution
- `thresh distro remove <key>` - Remove custom distribution

### New Command
- `thresh distros` - Show ALL distributions (built-in + custom) with sources

## Technical Notes

### Distribution Source Enum
```csharp
public enum DistributionSource
{
    Vendor,          // Direct rootfs downloads
    MicrosoftStore   // WSL --install wrapper
}
```

### Metadata Tracking
Each provisioned environment stores its distribution source in:
- **File**: `~/.thresh/metadata/{environmentName}.json`
- **Field**: `DistributionSource` property

## Future Enhancements
1. Add filtering: `thresh distros --source vendor`
2. Add search: `thresh distros --search ubuntu`
3. Add details: `thresh distros --details <name>` (show download URL, install method)
4. Color-coded output: Different colors for Vendor/MS Store/Custom
5. Remove duplicate custom distros (fedora-41, rocky-9 now built-in)

## Status
✅ **COMPLETE** - Ready for use

---
**Date**: 2025-02-05  
**Binary Size**: 12 MB (Native AOT)  
**Built-in Distros**: 15 (10 Vendor + 5 Microsoft Store)
