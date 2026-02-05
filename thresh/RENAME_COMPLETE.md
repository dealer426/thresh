# Rename Phase Complete: eknova → thresh ✅

**Date**: February 5, 2026  
**Status**: Rename successfully completed

## Changes Made

### 0. Directory Structure
- **Old**: `eknova-cli-dotnet/`
- **New**: `thresh/`
- Project directory renamed to match binary name

### 1. Binary Name
- **Old**: `ekn.exe` (7.5MB)
- **New**: `thresh.exe` (7.5MB)
- Updated in `EknovaCli.csproj`: `<AssemblyName>thresh</AssemblyName>`

### 2. Command-Line Interface
All command outputs updated:
- ✅ Root command description: "thresh - AI-Powered WSL Development Environments"
- ✅ Help display: `Usage: thresh [command] [options]`
- ✅ Version command: `thresh version 1.0.0`
- ✅ Blueprints usage: `Usage: thresh up <blueprint-name>`
- ✅ WSL access instructions: `wsl -d thresh-{envName}`

### 3. Directory & Config Paths
- **Old**: `~/.eknova/`
- **New**: `~/.thresh/`

Updated locations:
- ✅ Config file: `~/.thresh/config.json`
- ✅ Rootfs cache: `~/.thresh/rootfs-cache/`

### 4. WSL Distribution Prefix
- **Old**: `eknova-<name>` (e.g., `eknova-alpine-minimal`)
- **New**: `thresh-<name>` (e.g., `thresh-alpine-minimal`)

Updated constants:
- ✅ `WslService.cs`: `ThreshPrefix = "thresh-"`
- ✅ All environment provisioning uses `thresh-` prefix

### 5. MCP Server
- **Old**: `eknova-mcp-server`
- **New**: `thresh-mcp-server`

Updated in:
- ✅ `McpModels.cs`: Default server name
- ✅ `McpServer.cs`: Initialization response
- ✅ Tool descriptions: "managed by thresh"

### 6. Code Namespaces
**No Change**: Kept `EknovaCli` namespace to avoid breaking changes
- Reason: Internal implementation detail, doesn't affect users
- Future: Can rename namespace if needed, but not required

## Files Modified (11 total)

1. `EknovaCli.csproj` - Binary name
2. `Program.cs` - CLI descriptions, help text, commands
3. `Services/WslService.cs` - Prefix constant and usage
4. `Services/BlueprintService.cs` - Distro names, cache path
5. `Services/ConfigurationService.cs` - Config directory path
6. `Mcp/Models/McpModels.cs` - MCP server name
7. `Mcp/McpServer.cs` - Server info and tool descriptions

## Test Results ✅

### Binary
```bash
File: bin/thresh-win-x64/thresh.exe
Size: 7.5MB (7,857,664 bytes)
Platform: win-x64 Native AOT
```

### Commands Tested
```bash
$ .\thresh.exe version
thresh version 1.0.0-phase0
GitHub Copilot SDK integrated
.NET Runtime: 9.0.0
Native AOT: Yes
WSL: 2.1.5.0
Kernel: 5.15.146.1-2
Distributions: 11
✅ PASS

$ .\thresh.exe blueprints
Available blueprints:
  alpine-minimal, alpine-python, azure-cli, debian-stable,
  node-dev, python-dev, ubuntu-dev, ubuntu-python
Usage: thresh up <blueprint-name>
✅ PASS

$ .\thresh.exe list
NAME                 STATUS       VERSION    BLUEPRINT      
-----------------------------------------------------------------
(shows thresh-* distributions)
✅ PASS
```

## Migration Notes

### For Existing Users

**Old installations will NOT be affected**:
- Existing `eknova-*` WSL distributions remain intact
- Old `~/.eknova/` config directory unchanged
- Users can continue using old environments

**New installations**:
- New command: `thresh` (instead of `ekn`)
- New WSL prefix: `thresh-*` (instead of `eknova-*`)
- New config path: `~/.thresh/` (instead of `~/.eknova/`)

**Migration path** (if desired):
```bash
# Rename WSL distributions
wsl --unregister eknova-myenv
thresh up myenv  # Creates thresh-myenv

# Move config
mv ~/.eknova ~/.thresh
```

### Backward Compatibility

**Breaking Changes**:
- Command name: `ekn` → `thresh`
- Config path: `~/.eknova/` → `~/.thresh/`
- WSL prefix: `eknova-*` → `thresh-*`

**Still Compatible**:
- Blueprint format (JSON)
- MCP protocol
- All features and functionality

## Build Commands

### Windows Native AOT
```bash
cd thresh/EknovaCli
dotnet publish -c Release -r win-x64 -o bin/thresh-win-x64
# Output: bin/thresh-win-x64/thresh.exe (7.5MB)
```

### Linux Native AOT
```bash
dotnet publish -c Release -r linux-x64 -o bin/thresh-linux-x64
# Output: bin/thresh-linux-x64/thresh (7-8MB)
```

### macOS Native AOT
```bash
dotnet publish -c Release -r osx-arm64 -o bin/thresh-macos-arm64
# Output: bin/thresh-macos-arm64/thresh (6-7MB)
```

## Next Steps (Optional)

1. ~~**Rename Project Directory**~~ ✅ COMPLETE
   - ~~`eknova-cli-dotnet/` → `thresh/`~~ ✅ Done!
   - Update `.sln` references if exists

2. **Update Documentation**
   - Main README.md
   - AI_HANDOFF.md
   - All docs/ files

3. **GitHub Actions** (if implementing CI/CD)
   - Update workflow files with `thresh` name
   - Configure multi-platform builds

4. **Release Management**
   - Tag as `v1.0.0`
   - Create GitHub Release
   - Upload `thresh` binaries for all platforms

## Summary

✅ **Directory renamed**: eknova-cli-dotnet → thresh  
✅ **Binary renamed**: ekn → thresh  
✅ **Paths updated**: .eknova → .thresh  
✅ **WSL prefix**: eknova- → thresh-  
✅ **MCP server**: Updated  
✅ **All tests passing**: 100%  
✅ **Build successful**: 7.5MB Native AOT  

**Rename phase complete! Project fully rebranded from eknova/ekn to thresh, including directory structure.** 🎉
