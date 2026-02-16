# Linux Testing Handover - February 16, 2026

## Overview
This document outlines recent changes to thresh and critical areas requiring Linux validation before production release.

## Recent Changes Summary

### 1. Command Structure Refactoring ✅ (Windows Tested)
**Breaking Change**: Migrated from flat command structure to grouped blueprint commands.

**Before:**
```bash
thresh blueprints              # List blueprints
thresh generate "description"  # Generate blueprint
# No delete command existed
```

**After:**
```bash
thresh blueprint list                        # List blueprints
thresh blueprint generate "description"      # Generate blueprint
thresh blueprint delete <name>               # Delete blueprint
```

**Implementation:**
- Modified `Program.cs` - Added `AddBlueprintCommand()` with subcommands
- Removed all backwards-compatible aliases (`blueprints`, `generate`)
- System.CommandLine provides automatic typo suggestions
- Updated all code references across codebase

**Files Changed:**
- [thresh/Thresh/Program.cs](thresh/Thresh/Program.cs) - Main command routing
- [thresh/Thresh/Services/GitHubCopilotService.cs](thresh/Thresh/Services/GitHubCopilotService.cs) - Output messages
- [thresh/Thresh/Mcp/StdioMcpServer.cs](thresh/Thresh/Mcp/StdioMcpServer.cs) - MCP notifications
- [README.md](README.md) - User documentation

### 2. Auto-Save to Bundled Blueprints Folder ✅ (Windows Tested)
**Feature**: AI-generated blueprints automatically save to bundled `blueprints/` directory.

**Behavior:**
- `thresh blueprint generate "desc" --output name` → Saves to `blueprints/name.json`
- `thresh chat` → Auto-saves any generated blueprints to `blueprints/{name}.json`
- No timestamp suffixes (overwrites existing)
- Immediately available in `thresh blueprint list`

**Implementation:**
- `GitHubCopilotService.TrySaveBlueprintFromResponse()` - Saves to `AppContext.BaseDirectory/blueprints/`
- Uses regex to extract JSON from AI responses
- Validates JSON structure before saving

**Cross-Platform Concern**: Path separator handling (`/` vs `\`)

### 3. GitHub Copilot CLI Integration ⚠️ (Windows-Specific)
**Platform Issue**: Windows npm package installs as PowerShell script, not executable.

**Windows Solution:**
```csharp
var options = new CopilotClientOptions
{
    CliPath = "cmd.exe",
    CliArgs = new[] { "/c", "copilot" }
};
```

**Linux Requirement**: Needs different configuration:
```csharp
// Expected Linux configuration (NEEDS TESTING)
var options = new CopilotClientOptions
{
    CliPath = "copilot",  // Bash script or symlink
    CliArgs = Array.Empty<string>()
};
```

**Files to Check:**
- [thresh/Thresh/Services/GitHubCopilotService.cs](thresh/Thresh/Services/GitHubCopilotService.cs) - Lines 30-35 (GenerateBlueprintAsync)
- [thresh/Thresh/Services/GitHubCopilotService.cs](thresh/Thresh/Services/GitHubCopilotService.cs) - Lines 100-105 (ChatModeAsync)

### 4. Blueprint Delete Command ✅ (Windows Tested)
**Feature**: Users can delete generated blueprints.

**Usage:**
```bash
thresh blueprint delete <name>
```

**Behavior:**
- Deletes `blueprints/{name}.json`
- Shows confirmation message: "✓ Blueprint deleted: {name}.json"
- Error if blueprint not found with list of available blueprints
- Only deletes from blueprints folder (doesn't touch bundled system blueprints)

**Implementation:**
- Added `DeleteBlueprintHandler()` in `Program.cs`
- File deletion using `File.Delete()`

**Cross-Platform Concern**: File path construction and permissions

### 5. YAML Blueprint Conversion ✅ (Completed)
**Issue**: YamlDotNet incompatible with Native AOT (uses reflection).

**Solution**: Converted `python-yaml-test.yaml` → `python-yaml-test.json`

**Status**: All bundled blueprints now JSON format

## Critical Linux Testing Areas

### Priority 1: High Risk 🔴

#### 1.1 GitHub Copilot CLI Configuration
**Why Critical**: Platform-specific CLI invocation, Windows uses cmd.exe wrapper.

**Test Cases:**
```bash
# Verify copilot CLI is installed and accessible
which copilot
copilot --version

# Test blueprint generation
thresh blueprint generate "nginx web server" --output linux-test

# Test interactive chat mode
thresh chat
# > create a minimal redis environment
# > exit

# Verify auto-save worked
thresh blueprint list | grep linux-test
thresh blueprint list | grep redis
```

**Expected Behavior:**
- AI generates valid JSON blueprints
- Blueprints auto-save to `blueprints/` folder
- No errors about missing `cmd.exe` or PowerShell

**Debug Steps if Failed:**
1. Check copilot installation: `npm list -g @githubnext/github-copilot-cli`
2. Test copilot directly: `copilot "create hello world script"`
3. Update `GitHubCopilotService.cs` CopilotClientOptions for Linux
4. Check stderr for detailed error messages

#### 1.2 Blueprint Path Handling
**Why Critical**: Auto-save uses platform paths, risk of Windows-style backslashes.

**Test Cases:**
```bash
# Generate blueprint and verify file location
thresh blueprint generate "minimal alpine" --output path-test
find ~/.local/share/thresh -name "path-test.json"
# OR
find /usr/local/share/thresh -name "path-test.json"

# Verify path in output messages
thresh blueprint generate "test" --output test | grep "Available in"

# Test deletion
thresh blueprint delete path-test
ls ~/.local/share/thresh/blueprints/ | grep path-test
# Should not appear
```

**Expected Behavior:**
- Blueprints save to correct Linux directory
- Forward slashes in all paths
- No Windows-style path separators (`\`)
- Deletion works correctly

**Debug Steps if Failed:**
1. Check `AppContext.BaseDirectory` value: Add debug logging
2. Verify `Path.Combine()` uses correct separators
3. Check file permissions on blueprints directory

#### 1.3 MCP Server Integration
**Why Critical**: MCP typically runs on Linux servers, core feature.

**Test Cases:**
```bash
# Start MCP server
thresh serve &
MCP_PID=$!

# Test via stdin/stdout (manual verification)
echo '{"jsonrpc": "2.0", "method": "tools/list", "id": 1}' | thresh serve

# Check for proper shutdown
kill $MCP_PID

# Test with VS Code GitHub Copilot Agent
# (Requires manual VS Code configuration)
```

**Expected Behavior:**
- Server starts without errors
- Responds to JSON-RPC requests
- Tools include blueprint operations with new command structure
- Output messages reference `thresh blueprint list`

**Debug Steps if Failed:**
1. Check stderr for startup errors
2. Verify JSON-RPC message format
3. Test tool invocations through MCP protocol
4. Check `StdioMcpServer.cs` for command references

### Priority 2: Medium Risk 🟡

#### 2.1 Grouped Command Structure
**Test Cases:**
```bash
# Test all blueprint subcommands
thresh blueprint list
thresh blueprint generate "simple nodejs app" --output node-test
thresh blueprint delete node-test

# Test error handling
thresh blueprints  # Should suggest "blueprint"
thresh generate    # Should suggest "blueprint generate"

# Test help output
thresh blueprint --help
thresh blueprint generate --help
thresh blueprint delete --help
```

**Expected Behavior:**
- All subcommands work identically to Windows
- Typo suggestions appear for old commands
- Help text displays correctly
- Tab completion works (if bash-completion configured)

#### 2.2 Blueprint List Display
**Test Cases:**
```bash
# List all blueprints
thresh blueprint list

# Verify all bundled blueprints appear
thresh blueprint list | wc -l
# Should show 12+ blueprints

# Check for specific blueprints
thresh blueprint list | grep -E "alpine-minimal|python-dev|ubuntu-dev"
```

**Expected Behavior:**
- Table formatting displays correctly
- All bundled blueprints listed
- Generated blueprints appear after creation

#### 2.3 Configuration Management
**Test Cases:**
```bash
thresh config list
thresh config get default-model
thresh config set default-model gpt-4o
thresh config get default-model  # Should show gpt-4o
```

**Expected Behavior:**
- Config file stored in correct Linux location
- Values persist across runs
- No permission errors

### Priority 3: Low Risk 🟢

#### 3.1 Core WSL Operations
**Note**: These are Windows-specific but verify graceful handling on Linux.

**Test Cases:**
```bash
thresh list        # Should handle no WSL gracefully
thresh distros     # May show empty or error
thresh metrics     # Should show Linux host metrics
```

**Expected Behavior:**
- No crashes
- Informative error messages if WSL unavailable
- Metrics show Linux system information

#### 3.2 Version Information
**Test Cases:**
```bash
thresh version
thresh --version
thresh -v
```

**Expected Behavior:**
- Shows version 1.2.0+{commit}
- GitHub Copilot SDK integrated
- .NET Runtime version
- Native AOT: Yes

## Testing Checklist

### Pre-Testing Setup
- [ ] Install .NET 10.0 SDK
- [ ] Install GitHub Copilot CLI: `npm install -g @githubnext/github-copilot-cli`
- [ ] Authenticate: `copilot auth`
- [ ] Clone thresh repository
- [ ] Checkout `dev` branch (contains all changes)

### Build Process
```bash
cd thresh/Thresh
dotnet publish -c Release -r linux-x64 --self-contained
# Binary location: bin/Release/net10.0/linux-x64/publish/thresh
```

### Installation Test
```bash
# Copy to user bin
mkdir -p ~/.local/bin
cp bin/Release/net10.0/linux-x64/publish/thresh ~/.local/bin/
export PATH="$HOME/.local/bin:$PATH"

# Verify installation
which thresh
thresh version
```

### Systematic Test Execution

#### Phase 1: Basic Commands (5 min)
- [ ] `thresh version` - Verify build info
- [ ] `thresh --help` - Check command list
- [ ] `thresh blueprint --help` - Verify subcommands
- [ ] `thresh config list` - Check configuration

#### Phase 2: Blueprint Operations (10 min)
- [ ] `thresh blueprint list` - Verify bundled blueprints
- [ ] `thresh blueprint generate "redis cache" --output redis-test` - Test AI generation
- [ ] `thresh blueprint list | grep redis-test` - Verify auto-save
- [ ] `cat ~/.local/share/thresh/blueprints/redis-test.json` - Verify file contents
- [ ] `thresh blueprint delete redis-test` - Test deletion
- [ ] `thresh blueprint list | grep redis-test` - Verify removal

#### Phase 3: Chat Mode (10 min)
- [ ] `thresh chat` - Start interactive mode
  - [ ] Enter: "create a postgresql database environment"
  - [ ] Verify JSON blueprint appears
  - [ ] Check for auto-save message
  - [ ] Enter: "exit"
- [ ] `thresh blueprint list` - Verify chat-generated blueprint saved

#### Phase 4: MCP Server (15 min)
- [ ] `thresh serve` - Start MCP server
- [ ] Test JSON-RPC communication (manual or scripted)
- [ ] Verify tool list includes blueprint operations
- [ ] Test blueprint generation through MCP
- [ ] Stop server gracefully

#### Phase 5: Error Handling (5 min)
- [ ] `thresh invalid-command` - Check error message
- [ ] `thresh blueprints` - Verify suggestion for "blueprint"
- [ ] `thresh blueprint generate` - Missing argument error
- [ ] `thresh blueprint delete nonexistent` - Blueprint not found error

### Performance Benchmarks
```bash
# Binary size
ls -lh ~/.local/bin/thresh

# Startup time
time thresh --version

# Blueprint generation time
time thresh blueprint generate "minimal environment" --output perf-test
```

### Cross-Platform Validation
Compare behavior with Windows test results:
- [ ] All commands produce identical output (except platform-specific)
- [ ] Blueprint JSON structure matches
- [ ] Auto-save locations appropriate for each platform
- [ ] Error messages consistent

## Known Platform Differences

### File System Paths
- **Windows**: `C:\Users\burns\AppData\Local\Programs\thresh\blueprints\`
- **Linux**: `~/.local/share/thresh/blueprints/` or `/usr/local/share/thresh/blueprints/`

### GitHub Copilot CLI
- **Windows**: PowerShell script wrapper, requires `cmd.exe /c copilot`
- **Linux**: Bash script or symlink, direct invocation

### Configuration Storage
- **Windows**: `%APPDATA%\thresh\config.json`
- **Linux**: `~/.config/thresh/config.json`

## Rollback Plan

If critical issues found on Linux:

1. **Copilot CLI Issues**: 
   - Add platform detection in `GitHubCopilotService.cs`
   - Separate `CopilotClientOptions` for Windows/Linux

2. **Path Issues**:
   - Review all `Path.Combine()` calls
   - Test `AppContext.BaseDirectory` on Linux

3. **Command Structure Issues**:
   - Temporarily restore aliases (not recommended)
   - Fix System.CommandLine configuration

## Success Criteria

### Minimum Requirements
- ✅ `thresh blueprint list` shows all bundled blueprints
- ✅ `thresh blueprint generate` creates valid JSON and saves to blueprints folder
- ✅ `thresh blueprint delete` removes blueprints successfully
- ✅ `thresh chat` interactive mode works with auto-save
- ✅ MCP `serve` command starts without errors

### Ideal Outcome
- ✅ All commands work identically to Windows version
- ✅ Performance within 10% of Windows build
- ✅ No platform-specific error messages
- ✅ GitHub Copilot SDK integration fully functional
- ✅ MCP server handles all tool invocations correctly

## Next Steps After Testing

### If All Tests Pass ✅
1. Merge `dev` → `main`
2. Tag release as `v1.3.0` (breaking change justifies minor bump)
3. Update website documentation (version-1.3.0 still has old commands)
4. Update package managers (chocolatey, scoop, winget)
5. Publish release notes highlighting command structure changes

### If Tests Fail ❌
1. Document specific failure modes
2. Create GitHub issues for each platform-specific bug
3. Implement fixes with platform detection where needed
4. Retest on both Windows and Linux
5. Consider feature flags for unstable functionality

## Files Requiring Attention

### Code Files
- [thresh/Thresh/Services/GitHubCopilotService.cs](thresh/Thresh/Services/GitHubCopilotService.cs) - Copilot CLI configuration
- [thresh/Thresh/Program.cs](thresh/Thresh/Program.cs) - Blueprint command structure
- [thresh/Thresh/Services/BlueprintService.cs](thresh/Thresh/Services/BlueprintService.cs) - Path handling
- [thresh/Thresh/Mcp/StdioMcpServer.cs](thresh/Thresh/Mcp/StdioMcpServer.cs) - MCP tool definitions

### Documentation Files
- [README.md](README.md) - Updated with new commands ✅
- [website/versioned_docs/version-1.3.0/](website/versioned_docs/version-1.3.0/) - Needs updating ⚠️
- [docs/mcp-integration.md](docs/mcp-integration.md) - Verify command references

## Contact Information

**Handover From**: Windows validation completed February 16, 2026
**Version**: thresh 1.2.0+8a3b3244ade44e2b6f1f8df05ea87c7caa2418db
**Branch**: `dev` (3 commits ahead of main)
**Commits**: 60f1442, 386b210, 1adb1d7

**Questions**: Refer to conversation history for detailed technical decisions and rationale for command structure refactoring.
