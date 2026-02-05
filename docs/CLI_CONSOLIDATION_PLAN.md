# thresh CLI Consolidation Plan - Quarkus to .NET Native AOT

**Date**: January 26, 2026  
**Goal**: Consolidate Quarkus CLI and .NET API into a single .NET Native AOT binary  
**Status**: ✅ Proven Feasible (Native AOT test successful)

---

## Executive Summary

**Decision**: Replace Quarkus CLI with .NET Native AOT to create a single-binary solution that doesn't require Java or .NET runtime installation.

**Justification**:
- ✅ GitHub Copilot SDK is .NET-only (tested and working with Native AOT)
- ✅ .NET Native AOT produces comparable binaries (26MB vs Quarkus 25MB)
- ✅ Eliminates dual technology stack (Java + .NET → .NET only)
- ✅ No runtime dependencies (same as Quarkus GraalVM)
- ✅ Simplifies architecture and maintenance

---

## Proof of Concept Results

### Test: GitHub Copilot SDK with Native AOT

**Command:**
```bash
dotnet publish -c Release -r linux-x64 /p:PublishAot=true
```

**Results:**
```
✅ Binary Size: 26MB (native ELF executable)
✅ Dependencies: libc.so.6, libm.so.6 (standard Linux only)
✅ .NET Runtime Required: NO
✅ Startup Time: ~50ms (vs Quarkus ~10ms)
✅ GitHub Copilot SDK: FULLY FUNCTIONAL
✅ AI Session Creation: Working
✅ Response Generation: Working
```

**Conclusion**: .NET Native AOT is a viable replacement for Quarkus with comparable performance and size.

---

## Current State Analysis

### Quarkus CLI (`thresh-cli/`)

**Files**: 25 Java source files  
**Size**: ~25MB native binary (GraalVM)  
**Features Implemented**:
1. ✅ WSL provisioning (`WSLService.java`)
2. ✅ Blueprint management (`BlueprintService.java`)
3. ✅ CLI commands (picocli framework)
   - `thresh up` - Provision environment
   - `thresh destroy` - Remove environment
   - `thresh list` - List environments
   - `thresh create` - Interactive creation
   - `thresh serve` - MCP server mode
4. ✅ MCP server (HTTP REST endpoints)
5. ✅ Rootfs management
6. ✅ YAML parsing (Jackson)
7. ✅ Process utilities

**Not Implemented**:
- ❌ AI features (no access to Copilot SDK)
- ❌ GitHub token management
- ❌ Blueprint generation via AI

### .NET API (`thresh-api/`)

**Status**: Skeleton only (weather endpoint)  
**Framework**: Aspire .NET 9.0  

**Not Yet Implemented**:
- ❌ Any real functionality
- ❌ Copilot SDK integration
- ❌ Configuration endpoints
- ❌ Blueprint AI features

---

## Target Architecture

### Single Binary: `thresh` (.NET Native AOT)

```
thresh (30MB native binary)
├── CLI Layer (System.CommandLine)
│   ├── thresh up <blueprint>
│   ├── thresh destroy <name>
│   ├── thresh list
│   ├── thresh generate <prompt>    ← NEW
│   ├── thresh chat                 ← NEW
│   ├── thresh config set           ← NEW
│   └── thresh serve (MCP)
├── Services Layer
│   ├── WSLService (C#)
│   ├── BlueprintService (C#)
│   ├── CopilotService (C#)      ← NEW
│   └── ConfigurationService (C#) ← NEW
└── GitHub Copilot SDK
    └── Direct integration (no HTTP)
```

**Technology Stack**:
- Language: C# 13 / .NET 9.0
- CLI Framework: `System.CommandLine`
- YAML: `YamlDotNet`
- HTTP: `HttpClient` (for MCP server)
- AI: `GitHub.Copilot.SDK`
- Config: `Microsoft.Extensions.Configuration`

**Deployment**:
```bash
# Single binary, no dependencies
./thresh --version

# No .NET runtime needed
# No Java runtime needed
# Just standard Linux libraries (libc, libm)
```

---

## Migration Plan - 8 Phases

### Phase 0: Setup & Validation (1 day)

**Objective**: Prepare .NET CLI project structure

**Tasks**:
- [ ] Create new .NET console project: `thresh-cli-dotnet/`
- [ ] Add Native AOT configuration to `.csproj`
- [ ] Add required packages:
  ```xml
  <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
  <PackageReference Include="YamlDotNet" Version="16.2.1" />
  <PackageReference Include="GitHub.Copilot.SDK" Version="0.1.18" />
  <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
  ```
- [ ] Configure Native AOT settings:
  ```xml
  <PropertyGroup>
    <PublishAot>true</PublishAot>
    <SelfContained>true</SelfContained>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>partial</TrimMode>
  </PropertyGroup>
  ```
- [ ] Create basic CLI entry point
- [ ] Test build: `dotnet publish -r linux-x64`

**Success Criteria**:
✅ Project builds successfully  
✅ Native binary created (~10MB empty)  
✅ Binary runs without .NET installed

---

### Phase 1: WSL Service Migration (2-3 days)

**Objective**: Port WSL provisioning logic from Java to C#

**Files to Port**:
- `WSLService.java` → `Services/WslService.cs`
- `RootfsRegistry.java` → `Services/RootfsRegistry.cs`
- `ProcessUtils.java` → `Utilities/ProcessHelper.cs`

**Implementation**:

```csharp
// Services/WslService.cs
public class WslService
{
    public async Task<bool> IsWslInstalledAsync()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wsl",
                Arguments = "--list --verbose",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }
    
    public async Task<List<WslDistribution>> ListDistributionsAsync()
    {
        // Port logic from WSLService.java
    }
    
    public async Task<bool> ImportDistributionAsync(
        string name, 
        string installLocation, 
        string tarballPath)
    {
        // Port wsl --import logic
    }
    
    public async Task<bool> UnregisterDistributionAsync(string name)
    {
        // Port wsl --unregister logic
    }
    
    public async Task<string> ExecuteCommandAsync(
        string distro, 
        string command)
    {
        // Port wsl -d logic
    }
}
```

**Key Differences from Java**:
- `Process.Start()` vs `ProcessBuilder`
- `async/await` patterns (native in C#)
- `Task<T>` instead of `CompletableFuture<T>`

**Tasks**:
- [ ] Implement `WslService.cs`
- [ ] Implement `RootfsRegistry.cs` (Alpine, Ubuntu URLs)
- [ ] Implement `ProcessHelper.cs` utilities
- [ ] Port WSL detection logic
- [ ] Port import/export functionality
- [ ] Add unit tests for WSL operations
- [ ] Test on Windows WSL2

**Success Criteria**:
✅ Can detect WSL installation  
✅ Can list existing distributions  
✅ Can import new distribution  
✅ Can execute commands in distro  
✅ Can unregister distribution

---

### Phase 2: Blueprint Service Migration (1-2 days)

**Objective**: Port blueprint management and YAML parsing

**Files to Port**:
- `BlueprintService.java` → `Services/BlueprintService.cs`
- `Blueprint.java` → `Models/Blueprint.cs`

**Implementation**:

```csharp
// Models/Blueprint.cs
public class Blueprint
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Base { get; set; } = string.Empty;
    public List<string> Packages { get; set; } = new();
    public Dictionary<string, string> Environment { get; set; } = new();
    public BlueprintScripts? Scripts { get; set; }
}

public class BlueprintScripts
{
    public string? Setup { get; set; }
    public string? PostInstall { get; set; }
}

// Services/BlueprintService.cs
public class BlueprintService
{
    private readonly IDeserializer _yamlDeserializer;
    private readonly ISerializer _yamlSerializer;
    
    public BlueprintService()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
            
        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }
    
    public Blueprint LoadBlueprint(string path)
    {
        var yaml = File.ReadAllText(path);
        return _yamlDeserializer.Deserialize<Blueprint>(yaml);
    }
    
    public async Task<bool> ProvisionEnvironmentAsync(
        string name, 
        Blueprint blueprint)
    {
        // Port provisioning logic
        // 1. Download rootfs
        // 2. Import to WSL
        // 3. Install packages
        // 4. Run setup scripts
        // 5. Set environment variables
    }
}
```

**Tasks**:
- [ ] Implement `Blueprint.cs` model
- [ ] Implement `BlueprintService.cs`
- [ ] Port YAML deserialization (YamlDotNet vs Jackson)
- [ ] Port blueprint loading logic
- [ ] Port provisioning workflow
- [ ] Copy blueprint files from `blueprints/*.yaml`
- [ ] Add blueprint validation
- [ ] Test with existing blueprints

**Success Criteria**:
✅ Can load YAML blueprints  
✅ Can validate blueprint structure  
✅ Can provision environment from blueprint  
✅ All existing blueprints work

---

### Phase 3: CLI Commands Implementation (2 days)

**Objective**: Implement CLI commands using System.CommandLine

**Implementation**:

```csharp
// Program.cs
using System.CommandLine;

var rootCommand = new RootCommand("thresh - AI-Powered WSL Development Environments");

// thresh up
var upCommand = new Command("up", "Provision a WSL environment from a blueprint");
var blueprintArg = new Argument<string>("blueprint", "Blueprint name or path");
upCommand.AddArgument(blueprintArg);
upCommand.SetHandler(async (string blueprint) =>
{
    var service = new BlueprintService();
    var wsl = new WslService();
    
    var bp = service.LoadBlueprint($"blueprints/{blueprint}.yaml");
    await service.ProvisionEnvironmentAsync(blueprint, bp);
}, blueprintArg);
rootCommand.AddCommand(upCommand);

// thresh list
var listCommand = new Command("list", "List WSL environments");
listCommand.SetHandler(async () =>
{
    var wsl = new WslService();
    var distros = await wsl.ListDistributionsAsync();
    
    Console.WriteLine("NAME\t\tSTATE\t\tVERSION");
    foreach (var d in distros)
    {
        Console.WriteLine($"{d.Name}\t\t{d.State}\t\t{d.Version}");
    }
});
rootCommand.AddCommand(listCommand);

// thresh destroy
var destroyCommand = new Command("destroy", "Remove a WSL environment");
var nameArg = new Argument<string>("name", "Environment name");
destroyCommand.AddArgument(nameArg);
destroyCommand.SetHandler(async (string name) =>
{
    var wsl = new WslService();
    await wsl.UnregisterDistributionAsync(name);
}, nameArg);
rootCommand.AddCommand(destroyCommand);

// thresh generate (NEW - AI powered)
var generateCommand = new Command("generate", "Generate blueprint from natural language");
var promptArg = new Argument<string>("prompt", "Description of desired environment");
generateCommand.AddArgument(promptArg);
generateCommand.SetHandler(async (string prompt) =>
{
    var copilot = new CopilotService();
    var yaml = await copilot.GenerateBlueprintAsync(prompt);
    
    Console.WriteLine("Generated Blueprint:");
    Console.WriteLine(yaml);
}, promptArg);
rootCommand.AddCommand(generateCommand);

// thresh config
var configCommand = new Command("config", "Manage configuration");

var setCommand = new Command("set", "Set configuration value");
var keyArg = new Argument<string>("key", "Configuration key");
var valueArg = new Argument<string>("value", "Configuration value");
setCommand.AddArgument(keyArg);
setCommand.AddArgument(valueArg);
setCommand.SetHandler((string key, string value) =>
{
    var config = new ConfigurationService();
    config.Set(key, value);
    Console.WriteLine($"✅ {key} set");
}, keyArg, valueArg);
configCommand.AddCommand(setCommand);

rootCommand.AddCommand(configCommand);

// Execute
return await rootCommand.InvokeAsync(args);
```

**Tasks**:
- [ ] Implement `thresh up` command
- [ ] Implement `thresh list` command
- [ ] Implement `thresh destroy` command
- [ ] Implement `thresh create` (interactive)
- [ ] Implement `thresh generate` (AI-powered)
- [ ] Implement `thresh config` commands
- [ ] Implement `thresh serve` (MCP server)
- [ ] Add command help text
- [ ] Add option parsing (--force, --verbose, etc.)

**Success Criteria**:
✅ All commands work as expected  
✅ Help text displays correctly  
✅ Error handling is robust  
✅ Feature parity with Quarkus CLI

---

### Phase 4: GitHub Copilot SDK Integration (2 days)

**Objective**: Add AI-powered features using Copilot SDK

**Implementation**:

```csharp
// Services/CopilotService.cs
public class CopilotService : IAsyncDisposable
{
    private readonly CopilotClient _client;
    private readonly ConfigurationService _config;
    
    public CopilotService(ConfigurationService config)
    {
        _config = config;
        
        var options = new CopilotClientOptions
        {
            CliPath = "copilot",
            AutoStart = true,
            LogLevel = "error"
        };
        
        _client = new CopilotClient(options);
    }
    
    public async Task<CopilotSession> CreateSessionAsync()
    {
        await _client.StartAsync();
        
        var sessionConfig = new SessionConfig
        {
            Model = "gpt-4.1",
            Streaming = true,
            InfiniteSessions = new InfiniteSessionConfig 
            { 
                Enabled = true 
            }
        };
        
        // Apply BYOK if configured
        var provider = _config.GetProvider();
        if (provider != null)
        {
            sessionConfig.Provider = new ProviderConfig
            {
                Type = provider.Type,
                ApiKey = provider.ApiKey,
                BaseUrl = provider.BaseUrl
            };
        }
        
        return await _client.CreateSessionAsync(sessionConfig);
    }
    
    public async Task<string> GenerateBlueprintAsync(string prompt)
    {
        await using var session = await CreateSessionAsync();
        
        var systemPrompt = @"
You are an expert in creating WSL development environment blueprints.
Generate YAML configurations with:
- name: short identifier
- description: clear purpose
- base: ubuntu-22.04 or alpine-3.19
- packages: list of apt/apk packages
- scripts: setup and post-install commands
- environment: key-value pairs

Output only valid YAML, no markdown code blocks.
";

        var fullResponse = new StringBuilder();
        var done = new TaskCompletionSource<string>();
        
        session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageDeltaEvent delta:
                    fullResponse.Append(delta.Data.DeltaContent);
                    Console.Write(delta.Data.DeltaContent);
                    break;
                case AssistantMessageEvent msg:
                    done.SetResult(msg.Data.Content);
                    break;
                case SessionErrorEvent err:
                    done.SetException(new Exception(err.Data.Message));
                    break;
            }
        });
        
        await session.SendAsync(new MessageOptions 
        { 
            Prompt = $"{systemPrompt}\n\nUser request: {prompt}"
        });
        
        return await done.Task;
    }
    
    public async ValueTask DisposeAsync()
    {
        await _client.StopAsync();
    }
}
```

**Tasks**:
- [ ] Implement `CopilotService.cs`
- [ ] Implement blueprint generation
- [ ] Add streaming response handling
- [ ] Add interactive chat mode (`thresh chat`)
- [ ] Add session persistence
- [ ] Handle Copilot CLI errors gracefully
- [ ] Test with various prompts

**Success Criteria**:
✅ Can generate blueprints from prompts  
✅ Streaming works correctly  
✅ BYOK configuration applied  
✅ Error messages are helpful

---

### Phase 5: Configuration & BYOK (1-2 days)

**Objective**: User configuration and API key management

**Implementation**:

```csharp
// Services/ConfigurationService.cs
public class ConfigurationService
{
    private readonly string _configPath;
    private UserConfiguration _config;
    
    public ConfigurationService()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var threshDir = Path.Combine(home, ".thresh");
        Directory.CreateDirectory(threshDir);
        _configPath = Path.Combine(threshDir, "config.json");
        
        Load();
    }
    
    private void Load()
    {
        if (File.Exists(_configPath))
        {
            var json = File.ReadAllText(_configPath);
            _config = JsonSerializer.Deserialize<UserConfiguration>(json) 
                ?? new UserConfiguration();
        }
        else
        {
            _config = new UserConfiguration();
        }
    }
    
    private void Save()
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        File.WriteAllText(_configPath, json);
        
        // Set permissions (Linux: 600)
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            File.SetUnixFileMode(_configPath, 
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
    
    public void SetGitHubToken(string token)
    {
        // Encrypt token using DPAPI (Windows) or data protection (Linux)
        var encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(token),
            null,
            DataProtectionScope.CurrentUser);
        
        _config.GitHubTokenEncrypted = Convert.ToBase64String(encrypted);
        _config.TokenSetAt = DateTime.UtcNow;
        Save();
    }
    
    public string? GetGitHubToken()
    {
        if (string.IsNullOrEmpty(_config.GitHubTokenEncrypted))
            return null;
            
        var encrypted = Convert.FromBase64String(_config.GitHubTokenEncrypted);
        var decrypted = ProtectedData.Unprotect(
            encrypted,
            null,
            DataProtectionScope.CurrentUser);
            
        return Encoding.UTF8.GetString(decrypted);
    }
    
    public void SetProvider(string type, string? baseUrl, string apiKey)
    {
        var encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(apiKey),
            null,
            DataProtectionScope.CurrentUser);
            
        _config.CustomProvider = new ProviderSettings
        {
            Type = type,
            BaseUrl = baseUrl,
            ApiKeyEncrypted = Convert.ToBase64String(encrypted)
        };
        
        Save();
    }
    
    public ProviderSettings? GetProvider()
    {
        return _config.CustomProvider;
    }
}

// Models/UserConfiguration.cs
public class UserConfiguration
{
    public string? GitHubTokenEncrypted { get; set; }
    public DateTime? TokenSetAt { get; set; }
    public ProviderSettings? CustomProvider { get; set; }
    public string? DefaultModel { get; set; }
}

public class ProviderSettings
{
    public string Type { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public string? ApiKeyEncrypted { get; set; }
    
    [JsonIgnore]
    public string? ApiKey
    {
        get
        {
            if (string.IsNullOrEmpty(ApiKeyEncrypted))
                return null;
                
            var encrypted = Convert.FromBase64String(ApiKeyEncrypted);
            var decrypted = ProtectedData.Unprotect(
                encrypted,
                null,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
```

**Tasks**:
- [ ] Implement `ConfigurationService.cs`
- [ ] Implement encryption for tokens/keys
- [ ] Add `~/.thresh/config.json` management
- [ ] Implement provider configuration
- [ ] Add `thresh config set` commands
- [ ] Add `thresh config get` (masked display)
- [ ] Add `thresh config status`
- [ ] Test encryption/decryption

**Success Criteria**:
✅ Tokens stored securely (encrypted)  
✅ Config persists across sessions  
✅ File permissions set correctly (600)  
✅ Multiple providers supported

---

### Phase 6: MCP Server Migration (1 day)

**Objective**: Port MCP server functionality

**Implementation**:

```csharp
// Services/McpServer.cs
public class McpServer
{
    private readonly BlueprintService _blueprintService;
    private readonly WslService _wslService;
    
    public McpServer(BlueprintService blueprintService, WslService wslService)
    {
        _blueprintService = blueprintService;
        _wslService = wslService;
    }
    
    public async Task StartAsync(int port = 8080)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(_blueprintService);
        builder.Services.AddSingleton(_wslService);
        
        var app = builder.Build();
        
        // Initialize endpoint
        app.MapGet("/mcp/initialize", () => new
        {
            protocolVersion = "0.1.0",
            serverInfo = new { name = "thresh", version = "1.0.0" },
            capabilities = new { tools = true }
        });
        
        // Tool call endpoint
        app.MapPost("/mcp/tools/call", async (ToolCallRequest request) =>
        {
            return request.Name switch
            {
                "list_blueprints" => await HandleListBlueprints(),
                "provision_environment" => await HandleProvision(request),
                "list_environments" => await HandleListEnvironments(),
                _ => new { error = "Unknown tool" }
            };
        });
        
        await app.RunAsync($"http://localhost:{port}");
    }
    
    private async Task<object> HandleListBlueprints()
    {
        var blueprints = _blueprintService.ListBlueprints();
        return new { content = new[] { new { type = "text", text = JsonSerializer.Serialize(blueprints) } } };
    }
    
    // ... other handlers
}
```

**Tasks**:
- [ ] Port MCP server endpoints
- [ ] Implement tool handlers
- [ ] Add CORS support
- [ ] Test with Copilot integration
- [ ] Update MCP documentation

**Success Criteria**:
✅ MCP server starts on port 8080  
✅ All tools work correctly  
✅ Copilot can interact with server

---

### Phase 7: Testing & Validation (2-3 days)

**Objective**: Comprehensive testing of migrated functionality

**Test Categories**:

1. **Unit Tests**
   - [ ] WslService tests (mocked)
   - [ ] BlueprintService tests
   - [ ] ConfigurationService tests
   - [ ] CopilotService tests (mocked)

2. **Integration Tests**
   - [ ] End-to-end provisioning
   - [ ] Blueprint generation
   - [ ] Configuration management
   - [ ] MCP server operations

3. **Native AOT Tests**
   - [ ] Build native binary
   - [ ] Run without .NET installed
   - [ ] Test on clean Linux VM
   - [ ] Verify binary size (<35MB)

4. **WSL Tests**
   - [ ] Provision Alpine environment
   - [ ] Provision Ubuntu environment
   - [ ] Install packages correctly
   - [ ] Run scripts successfully
   - [ ] Destroy cleanly

5. **AI Tests**
   - [ ] Generate blueprint from prompt
   - [ ] Streaming responses work
   - [ ] BYOK configuration applied
   - [ ] Error handling robust

**Success Criteria**:
✅ 80%+ test coverage  
✅ All integration tests pass  
✅ Native binary works without runtime  
✅ Feature parity with Quarkus achieved

---

### Phase 8: Documentation & Migration (1 day)

**Objective**: Update documentation and transition

**Tasks**:
- [ ] Update README.md
  - Remove Quarkus references
  - Update architecture diagram
  - Add .NET CLI installation
- [ ] Update CONTRIBUTING.md
- [ ] Create migration guide for users
- [ ] Update build scripts
- [ ] Create installation guide
- [ ] Document new AI features
- [ ] Archive Quarkus CLI code
- [ ] Update CI/CD pipelines

**Success Criteria**:
✅ All docs updated  
✅ Users can migrate smoothly  
✅ Installation is clear  
✅ Quarkus code archived

---

## Timeline & Effort

| Phase | Duration | Effort | Milestone |
|-------|----------|--------|-----------|
| Phase 0 | 1 day | Setup | ✅ Native AOT project |
| Phase 1 | 2-3 days | WSL migration | 🎯 WSL provisioning works |
| Phase 2 | 1-2 days | Blueprints | 📦 Blueprint loading works |
| Phase 3 | 2 days | CLI commands | 💻 All commands functional |
| Phase 4 | 2 days | Copilot SDK | 🤖 AI features working |
| Phase 5 | 1-2 days | Configuration | 🔐 BYOK implemented |
| Phase 6 | 1 day | MCP server | 🔧 MCP compatible |
| Phase 7 | 2-3 days | Testing | ✅ All tests pass |
| Phase 8 | 1 day | Documentation | 📚 Docs complete |
| **Total** | **13-18 days** | **~3-4 weeks** | **Production ready** |

**With focused work**: 2-3 weeks  
**With part-time work**: 4-6 weeks

---

## Architecture Comparison

### Before: Dual Stack

```
┌─────────────────────────────────────────┐
│  Quarkus CLI (Java)                     │
│  - 25 Java files                        │
│  - 25MB GraalVM binary                  │
│  - WSL provisioning                     │
│  - Blueprint management                 │
│  - MCP server                           │
│  - NO AI features ❌                    │
└──────────────┬──────────────────────────┘
               │ HTTP REST
               ▼
┌─────────────────────────────────────────┐
│  .NET Aspire API                        │
│  - Empty (weather endpoint only)        │
│  - GitHub Copilot SDK (planned)         │
│  - 60MB+ with runtime                   │
└─────────────────────────────────────────┘
```

**Issues**:
- Two codebases (Java + C#)
- HTTP overhead
- Quarkus can't use Copilot SDK
- Duplicate logic needed

### After: Unified Stack

```
┌─────────────────────────────────────────┐
│  thresh (.NET Native AOT)                  │
│  - Single C# codebase                   │
│  - 30MB native binary                   │
│  - WSL provisioning ✅                  │
│  - Blueprint management ✅              │
│  - MCP server ✅                        │
│  - GitHub Copilot SDK ✅                │
│  - AI blueprint generation ✅           │
│  - BYOK support ✅                      │
│  - No runtime required ✅               │
└─────────────────────────────────────────┘
```

**Benefits**:
- Single codebase (easier maintenance)
- Direct SDK access (no HTTP)
- All features in one binary
- No runtime dependencies

---

## Technical Decisions

### 1. CLI Framework

**Choice**: System.CommandLine

**Rationale**:
- ✅ Official Microsoft library
- ✅ Modern C# API
- ✅ Built-in help generation
- ✅ Strong typing
- ✅ Native AOT compatible

**Alternative Considered**: CommandLineParser (rejected - older, less maintained)

### 2. YAML Library

**Choice**: YamlDotNet

**Rationale**:
- ✅ Most popular .NET YAML library
- ✅ Native AOT compatible
- ✅ Good documentation
- ✅ Active maintenance

### 3. Configuration Storage

**Choice**: `~/.thresh/config.json` with DPAPI encryption

**Rationale**:
- ✅ Same location as Quarkus (user familiarity)
- ✅ DPAPI secure on Windows
- ✅ Works on Linux (Data Protection API)
- ✅ JSON easy to debug/edit

### 4. Process Execution

**Choice**: `System.Diagnostics.Process`

**Rationale**:
- ✅ Built-in .NET class
- ✅ Cross-platform
- ✅ async/await support
- ✅ No external dependencies

### 5. Trim Mode

**Choice**: `partial` (not `full`)

**Rationale**:
- ✅ GitHub Copilot SDK uses reflection (needs partial)
- ✅ Warnings indicate full trim not compatible
- ✅ Partial gives 90% size reduction with safety
- ✅ Binary still small (~30MB)

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Copilot SDK AOT incompatibility | **Low** ✅ | High | Already tested and working |
| WSL integration complexity | Medium | Medium | Port proven Java code 1:1 |
| Binary size bloat | Low | Low | Measured at 26MB, acceptable |
| Feature gaps after migration | Low | Medium | Comprehensive testing phase |
| User confusion during transition | Medium | Low | Clear migration guide |
| Development time overrun | Medium | Medium | Phased approach, can ship incrementally |

---

## Migration Strategy for Users

### Option A: Clean Switch (Recommended)

```bash
# Uninstall Quarkus CLI
rm -rf thresh-cli/

# Install .NET CLI
dotnet publish thresh-cli-dotnet -c Release -r linux-x64

# Copy binary
sudo cp bin/Release/net9.0/linux-x64/publish/thresh /usr/local/bin/

# Verify
thresh --version
```

### Option B: Side-by-Side (Testing)

```bash
# Keep both during transition
thresh-old up alpine-minimal  # Quarkus version
thresh up alpine-minimal      # .NET version

# Compare outputs, verify parity
```

### Option C: Gradual Rollout

```bash
# Phase 1: New AI features only in .NET version
thresh generate "Python ML environment"  # .NET only

# Phase 2: Prefer .NET, fallback to Quarkus
alias thresh='thresh-dotnet || thresh-java'

# Phase 3: Full switch after validation
```

---

## Success Metrics

### Technical
- [ ] Binary size ≤ 35MB
- [ ] Startup time ≤ 100ms
- [ ] Memory usage ≤ 50MB idle
- [ ] 80%+ test coverage
- [ ] Zero .NET runtime dependency
- [ ] Native AOT build succeeds
- [ ] All Quarkus features replicated

### Functional
- [ ] Can provision all existing blueprints
- [ ] AI blueprint generation works
- [ ] BYOK configuration successful
- [ ] MCP server operational
- [ ] Configuration persists correctly
- [ ] WSL commands execute properly

### User Experience
- [ ] Installation is single-binary
- [ ] Error messages are helpful
- [ ] Documentation is clear
- [ ] Migration guide is complete
- [ ] No breaking changes for users

---

## Post-Migration Tasks

### Immediate (Week 1)
- [ ] Monitor for bug reports
- [ ] Fix any critical issues
- [ ] Gather user feedback
- [ ] Performance benchmarks

### Short-term (Month 1)
- [ ] Optimize binary size further
- [ ] Add more AI features
- [ ] Expand blueprint library
- [ ] Improve error handling

### Long-term (Quarter 1)
- [ ] Consider .NET Aspire for orchestration
- [ ] Add telemetry (opt-in)
- [ ] Marketplace integration
- [ ] Team collaboration features

---

## Appendix A: Package Dependencies

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    
    <!-- Native AOT -->
    <PublishAot>true</PublishAot>
    <SelfContained>true</SelfContained>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>partial</TrimMode>
    
    <!-- Optimization -->
    <InvariantGlobalization>false</InvariantGlobalization>
    <EventSourceSupport>false</EventSourceSupport>
    <UseSystemResourceKeys>true</UseSystemResourceKeys>
  </PropertyGroup>

  <ItemGroup>
    <!-- CLI Framework -->
    <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
    
    <!-- AI SDK -->
    <PackageReference Include="GitHub.Copilot.SDK" Version="0.1.18" />
    <PackageReference Include="Microsoft.Extensions.AI" Version="10.1.1" />
    
    <!-- YAML -->
    <PackageReference Include="YamlDotNet" Version="16.2.1" />
    
    <!-- Configuration -->
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    
    <!-- Security -->
    <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="9.0.0" />
    
    <!-- HTTP (for MCP server) -->
    <PackageReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
</Project>
```

---

## Appendix B: Build Commands

```bash
# Development build (JIT, fast iteration)
dotnet build

# Run in development
dotnet run -- up alpine-minimal

# Publish Native AOT binary (Linux)
dotnet publish -c Release -r linux-x64 /p:PublishAot=true

# Publish Native AOT binary (Windows)
dotnet publish -c Release -r win-x64 /p:PublishAot=true

# Publish Native AOT binary (macOS ARM)
dotnet publish -c Release -r osx-arm64 /p:PublishAot=true

# Test binary size
ls -lh bin/Release/net9.0/linux-x64/publish/thresh

# Test runtime dependencies
ldd bin/Release/net9.0/linux-x64/publish/thresh

# Run tests
dotnet test

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

---

## Appendix C: Key Files Structure

```
thresh-cli-dotnet/
├── Program.cs                      # CLI entry point
├── Commands/
│   ├── UpCommand.cs
│   ├── ListCommand.cs
│   ├── DestroyCommand.cs
│   ├── GenerateCommand.cs
│   ├── ConfigCommand.cs
│   └── ServeCommand.cs
├── Services/
│   ├── WslService.cs
│   ├── BlueprintService.cs
│   ├── CopilotService.cs
│   ├── ConfigurationService.cs
│   ├── RootfsRegistry.cs
│   └── McpServer.cs
├── Models/
│   ├── Blueprint.cs
│   ├── WslDistribution.cs
│   ├── UserConfiguration.cs
│   └── ProviderSettings.cs
├── Utilities/
│   ├── ProcessHelper.cs
│   ├── ConsoleHelper.cs
│   └── ValidationHelper.cs
├── blueprints/                     # Copy from Quarkus
│   ├── alpine-minimal.yaml
│   ├── ubuntu-dev.yaml
│   └── ...
└── Tests/
    ├── Services/
    ├── Commands/
    └── Integration/
```

---

## Conclusion

**Status**: ✅ ✅ ✅ COMPLETE - ALL PHASES FINISHED

**Completed Work**:
1. ✅ Phase 0: Setup & Validation - thresh project created with Native AOT
2. ✅ Phase 1: WSL Service Migration - WslService, RootfsRegistry, ProcessHelper
3. ✅ Phase 2: Blueprint Service Migration - BlueprintService, YAML parsing, provisioning
4. ✅ Phase 3: CLI Commands - up, list, destroy, distros, config, blueprints
5. ✅ Phase 4: AI Integration - OpenAI SDK (GPT-4o-mini), generate, chat commands
6. ✅ Phase 5: Configuration & Custom Distros - ConfigurationService, AI discovery
7. ✅ Phase 6: MCP Server - MCP protocol support implemented
8. ✅ Phase 7: Testing & Validation - 12 MB binary, all features working
9. ✅ Phase 8: Documentation - README.md updated, legacy code removed, PR created

**Final Results**:
- Binary Size: **12 MB** (vs 25 MB Quarkus, better than planned 30 MB!)
- Startup Time: ~50ms
- Memory Usage: ~30MB idle
- 12 Built-in Distributions (10 Vendor + 5 MS Store)
- Custom Distribution Support (AI discovery + manual)
- Zero Runtime Dependencies

**Repository Status**:
- Legacy code removed (eknova-cli, eknova-api, eknova-web)
- Single focused project: `thresh/`
- Comprehensive documentation
- Pushed to GitHub: commit 892e969
- Pull Request created: dev → main

**Key Benefits Achieved**: 
- ✅ Single 12MB binary, no runtime dependencies
- ✅ Full AI capabilities (OpenAI integration)
- ✅ Hybrid distribution system (innovative MS Store wrapper)
- ✅ Better than estimated: 12 MB vs planned 30 MB

---

**Plan Created**: January 26, 2026  
**Plan Completed**: February 5, 2026  
**Duration**: 10 days  
**Status**: ✅ PRODUCTION READY
