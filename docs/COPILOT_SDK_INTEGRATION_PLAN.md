# GitHub Copilot SDK Integration Plan - thresh

**Date**: January 26, 2026  
**Goal**: Integrate GitHub Copilot SDK (.NET) with Aspire API backend + Quarkus CLI

---

## Current State

- ✅ .NET SDK: **9.0.203** (Latest stable)
- ✅ Target Framework: **net9.0** 
- ⚠️ .NET 10: **Not yet released** (still in preview/RC)
- ✅ Aspire: Compatible with .NET 9.0
- ❌ AI Integration: **Not implemented** (Semantic Kernel mentioned but not present)
- ✅ **GitHub Copilot SDK**: Available as `GitHub.Copilot.SDK` NuGet package

**Recommendation**: **Stay on .NET 9.0** for now. .NET 10 isn't GA yet, and .NET 9 is LTS with full Aspire support.

**Key Insight**: GitHub Copilot SDK provides a **programmatic control layer** for the Copilot CLI, with built-in BYOK support via `ProviderConfig`!

---

## Critical Information from GitHub Copilot SDK

### SDK Status & Requirements
- ⚠️ **Technical Preview** - Not yet production-ready (use with caution)
- ✅ **Subscription Required** - GitHub Copilot subscription OR BYOK with custom provider
- ✅ **Free Tier Available** - Limited usage included with Copilot CLI
- 📊 **Billing Model** - Premium request quota (same as Copilot CLI)
- 🔧 **Copilot CLI Required** - Must be installed separately (`gh extension install github/gh-copilot`)

### Key Features Confirmed
1. **Custom Tools** - Define C# methods that AI can invoke using `AIFunctionFactory`
2. **Streaming** - Real-time response chunks via `AssistantMessageDeltaEvent`
3. **Session Persistence** - Save/resume sessions across restarts
4. **Multiple Sessions** - Independent conversations simultaneously
5. **Infinite Sessions** - Auto-compaction for long conversations (background checkpoints)
6. **Tool Control** - Enable/disable specific tools (default: `--allow-all`)
7. **BYOK Support** - OpenAI, Azure OpenAI, Anthropic via `ProviderConfig`

### Default Tools Available
When `--allow-all` is enabled (SDK default):
- File system operations (read, write, edit)
- Git operations (commit, diff, status)
- Web requests (fetch URLs)
- View tool (read images, files)
- Bash/shell execution
- And more...

### Architecture Pattern Confirmed
```
thresh CLI (Quarkus)
    ↓ HTTP/REST API calls
.NET Aspire API (Port 5000)
    ↓ GitHub.Copilot.SDK (C#)
Copilot CLI (auto-managed process)
    ↓ JSON-RPC
GitHub Models API / Custom Provider (OpenAI, Azure, Anthropic)
```

### Community SDK Note
- ⚠️ **Java SDK exists** (unofficial, community-maintained)
- Link: https://github.com/copilot-community-sdk/copilot-sdk-java
- Could be alternative for Quarkus CLI, but unofficial
- **Recommendation**: Stick with .NET SDK (official, supported) + Quarkus as lightweight HTTP shell

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                   User Layer                            │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Quarkus CLI │  │ Web UI       │  │ Direct API   │  │
│  │   (thresh)     │  │  (Next.js)   │  │   Calls      │  │
│  └──────┬──────┘  └──────┬───────┘  └──────┬───────┘  │
└─────────┼─────────────────┼──────────────────┼──────────┘
          │                 │                  │
          │ HTTP/REST       │                  │
          ▼                 ▼                  ▼
┌─────────────────────────────────────────────────────────┐
│          .NET Aspire API Backend (Port 5000)            │
│  ┌──────────────────────────────────────────────────┐  │
│  │ CopilotService (GitHub.Copilot.SDK)              │  │
│  │  - CopilotClient (session management)            │  │
│  │  - ProviderConfig (BYOK support)                 │  │
│  │  - Streaming support                             │  │
│  │  - Tool registration                             │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Configuration Service                            │  │
│  │  - User token storage (~/.thresh/config.json)    │  │
│  │  - Encryption (DPAPI/Keyring)                    │  │
│  │  - Token validation                              │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Blueprint AI Endpoints                           │  │
│  │  - POST /api/blueprints/generate                 │  │
│  │  - POST /api/blueprints/validate                 │  │
│  │  - POST /api/chat/stream (SSE)                   │  │
│  │  - POST /api/config/github-token (set BYOK)      │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ GitHub Copilot SDK
                        ▼
┌─────────────────────────────────────────────────────────┐
│            GitHub Copilot CLI                           │
│  - Managed by SDK (auto-start/stop)                    │
│  - Session management                                   │
│  - Model selection (gpt-5, claude-sonnet-4.5, etc.)    │
│  - BYOK via ProviderConfig                             │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│          GitHub Models / Custom Providers               │
│  - Default: GitHub Models API (free tier)              │
│  - BYOK: OpenAI, Azure OpenAI, Anthropic, etc.         │
│  - User's GitHub token or custom API keys              │
└─────────────────────────────────────────────────────────┘
```

**Key Architecture Notes:**
- **Quarkus CLI**: Lightweight shell, calls .NET API endpoints
- **.NET Aspire API**: Business logic + GitHub Copilot SDK integration
- **GitHub Copilot SDK**: Programmatic control of Copilot CLI
- **BYOK**: Users bring GitHub token OR custom provider (OpenAI, etc.)

---

## Phase 0: Infrastructure Preparation ⏱️ 1-2 hours

**Objective**: Ensure .NET environment is optimal

### Tasks
- [x] ~~Check .NET 10 availability~~ → **Stay on .NET 9.0** (10 not GA)
- [ ] Verify Aspire workload is installed: `dotnet workload list`
- [ ] Install Aspire if needed: `dotnet workload install aspire`
- [ ] Update NuGet packages to latest stable versions
- [ ] Run baseline build: `dotnet build thresh-api.sln`

### Success Criteria
✅ All projects build successfully  
✅ No package version conflicts  
✅ Aspire dashboard accessible  

---

## Phase 1: Core GitHub Copilot SDK Integration ⏱️ 3-4 hours

**Objective**: Basic AI connectivity using GitHub Copilot SDK

### Tasks
- [ ] Add NuGet packages to `nova-api.ApiService`:
  ```bash
  dotnet add package GitHub.Copilot.SDK
  dotnet add package Microsoft.Extensions.AI
  ```
- [ ] Create `Services/CopilotService.cs`:
  - Singleton `CopilotClient` instance
  - Session management (create/resume/dispose)
  - Event handling for streaming responses
  - Default model configuration
- [ ] Add configuration to `appsettings.json`:
  ```json
  {
    "Copilot": {
      "Model": "gpt-5",
      "Streaming": true,
      "InfiniteSessions": {
        "Enabled": true,
        "BackgroundCompactionThreshold": 0.80,
        "BufferExhaustionThreshold": 0.95
      },
      "CliPath": "copilot",
      "LogLevel": "info"
    }
  }
  ```
- [ ] Register service in DI container (`Program.cs`):
  ```csharp
  builder.Services.AddSingleton<CopilotService>();
  ```
- [ ] Create test endpoint: `GET /api/copilot/test`
- [ ] Test basic session creation and message sending

### Code Sample
```csharp
public class CopilotService : IAsyncDisposable
{
    private readonly CopilotClient _client;
    private readonly ILogger<CopilotService> _logger;
    
    public CopilotService(IConfiguration config, ILogger<CopilotService> logger)
    {
        _logger = logger;
        _client = new CopilotClient(new CopilotClientOptions
        {
            CliPath = config["Copilot:CliPath"] ?? "copilot",
            LogLevel = config["Copilot:LogLevel"] ?? "info",
            AutoStart = true,
            AutoRestart = true
        });
    }
    
    public async Task<CopilotSession> CreateSessionAsync(SessionConfig? config = null)
    {
        config ??= new SessionConfig
        {
            Model = "gpt-5",
            Streaming = true,
            InfiniteSessions = new InfiniteSessionConfig { Enabled = true }
        };
        
        await _client.StartAsync();
        return await _client.CreateSessionAsync(config);
    }
    
    public async Task<string> SendMessageAsync(
        CopilotSession session, 
        string prompt, 
        Action<string>? onDelta = null)
    {
        var done = new TaskCompletionSource<string>();
        var fullResponse = new StringBuilder();
        
        session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageDeltaEvent delta:
                    fullResponse.Append(delta.Data.DeltaContent);
                    onDelta?.Invoke(delta.Data.DeltaContent);
                    break;
                case AssistantMessageEvent msg:
                    done.SetResult(msg.Data.Content);
                    break;
                case SessionErrorEvent err:
                    done.SetException(new Exception(err.Data.Message));
                    break;
            }
        });
        
        await session.SendAsync(new MessageOptions { Prompt = prompt });
        return await done.Task;
    }
    
    public async ValueTask DisposeAsync()
    {
        await _client.StopAsync();
    }
}
```

### Success Criteria
✅ Copilot CLI auto-starts when service initializes  
✅ Sessions can be created and messages sent  
✅ Streaming responses work correctly  
✅ Test endpoint returns AI-generated response  

---

## Phase 2: BYOK Configuration System ⏱️ 4-6 hours

**Objective**: Secure user token storage + GitHub Copilot SDK ProviderConfig

### Tasks
- [ ] Create `Models/UserConfiguration.cs`:
  ```csharp
  public class UserConfiguration
  {
      public string? GitHubToken { get; set; }
      public ProviderSettings? CustomProvider { get; set; }
      public DateTime? TokenSetAt { get; set; }
      public string? DefaultModel { get; set; }
  }
  
  public class ProviderSettings
  {
      public string Type { get; set; } // "openai", "azure", "anthropic"
      public string? BaseUrl { get; set; }
      public string? ApiKey { get; set; }
  }
  ```
- [ ] Create `Services/ConfigurationService.cs`:
  - Load/Save user config from `~/.thresh/config.json`
  - Encrypt tokens/keys using DPAPI (Windows) or keyring (Linux)
  - Validate token/key formats
  - Build `ProviderConfig` from stored settings
- [ ] Add endpoints:
  - `POST /api/config/github-token` - Set GitHub token
  - `POST /api/config/provider` - Set custom provider (OpenAI, Azure, etc.)
  - `GET /api/config/status` - Check configuration status
  - `DELETE /api/config` - Clear all configuration
- [ ] Update `CopilotService.CreateSessionAsync()` to use `ProviderConfig`:
  ```csharp
  var session = await _client.CreateSessionAsync(new SessionConfig
  {
      Model = config.Model,
      Provider = userConfig.CustomProvider != null 
          ? new ProviderConfig
          {
              Type = userConfig.CustomProvider.Type,
              BaseUrl = userConfig.CustomProvider.BaseUrl,
              ApiKey = userConfig.CustomProvider.ApiKey
          }
          : null, // Use default GitHub Models
      Streaming = true
  });
  ```
- [ ] Add middleware to check configuration before AI operations

### User Config Location
- **Windows**: `%USERPROFILE%\.thresh\config.json`
- **Linux/WSL**: `~/.thresh/config.json`

### Provider Priority
1. **Custom Provider** (from user config): OpenAI, Azure, Anthropic
2. **GitHub Token** (from user config): GitHub Models API with user's token
3. **Default GitHub Models**: Free tier (if available)
4. **Fail with clear error** if none configured

### BYOK Scenarios Supported
- ✅ GitHub Copilot subscription (user's GitHub token)
- ✅ OpenAI API key (custom provider)
- ✅ Azure OpenAI (custom provider with endpoint)
- ✅ Anthropic Claude (custom provider)
- ✅ Default free tier (no config needed, if supported)

### Success Criteria
✅ User can configure GitHub token or custom provider via API  
✅ Configuration persists across restarts  
✅ Tokens/keys encrypted at rest  
✅ Sessions use user's provider configuration  
✅ Clear errors when configuration missing  

---

## Phase 3: Blueprint AI Endpoints ⏱️ 6-8 hours

**Objective**: AI-powered blueprint generation and validation

### Tasks
- [ ] Create `Controllers/BlueprintAIController.cs`
- [ ] Implement endpoints:
  
  #### 1. Generate Blueprint
  ```
  POST /api/blueprints/generate
  Body: { "prompt": "Ubuntu with Python ML tools and Jupyter" }
  Response: { "yaml": "...", "name": "...", "description": "..." }
  ```
  
  #### 2. Validate/Improve Blueprint
  ```
  POST /api/blueprints/validate
  Body: { "yaml": "..." }
  Response: { "isValid": true, "suggestions": [...], "improved": "..." }
  ```
  
  #### 3. Chat Completion (Streaming)
  ```
  POST /api/chat
  Body: { "messages": [...], "stream": true }
  Response: Server-Sent Events (SSE)
  ```

- [ ] Create prompt templates for blueprint generation
- [ ] Add YAML parsing and validation logic
- [ ] Implement streaming support
- [ ] Add error handling for rate limits

### Prompt Engineering
```
System Prompt:
"You are an expert in creating WSL development environment blueprints.
Generate YAML configurations with:
- name: short identifier
- description: clear purpose
- base: ubuntu-22.04 or alpine-3.19
- packages: list of apt/apk packages
- scripts: setup and post-install commands
- environment: key-value pairs

User prompt: {user_input}

Output only valid YAML, no markdown code blocks."
```

### Success Criteria
✅ Generate valid blueprints from natural language  
✅ Validation catches common errors  
✅ Streaming works for chat interface  

---

## Phase 4: CLI Integration ⏱️ 3-4 hours

**Objective**: Quarkus CLI commands for configuration management

### Tasks
- [ ] Add REST client to Quarkus CLI for API calls
- [ ] Implement configuration commands:
  ```bash
  # GitHub token (for GitHub Models)
  thresh config set github-token <TOKEN>
  
  # Custom provider (OpenAI, Azure, Anthropic)
  thresh config set provider openai --api-key <KEY>
  thresh config set provider azure --endpoint <URL> --api-key <KEY>
  thresh config set provider anthropic --api-key <KEY>
  
  # View configuration
  thresh config get              # Shows masked values
  thresh config status           # Shows activation status
  
  # Clear configuration
  thresh config clear
  ```
- [ ] Add AI-powered blueprint generation command:
  ```bash
  thresh generate "Python ML environment with Jupyter"
  thresh generate --interactive  # Chat mode
  ```
- [ ] Add activation check before AI operations
- [ ] Display helpful error when not configured
- [ ] Create `thresh activate` wizard for first-time setup

### REST Client Example (Quarkus)
```java
@Path("/api/config")
@RegisterRestClient(configKey = "thresh-api")
public interface ConfigClient {
    
    @POST
    @Path("/github-token")
    Response setGitHubToken(TokenRequest request);
    
    @POST
    @Path("/provider")
    Response setProvider(ProviderRequest request);
    
    @GET
    @Path("/status")
    ConfigStatus getStatus();
}

@Command(name = "config")
public class ConfigCommand implements Runnable {
    
    @RestClient
    ConfigClient configClient;
    
    @CommandLine.Command(name = "set")
    public void set(
        @CommandLine.Parameters(index = "0") String key,
        @CommandLine.Parameters(index = "1") String value) {
        
        if ("github-token".equals(key)) {
            configClient.setGitHubToken(new TokenRequest(value));
            System.out.println("✅ GitHub token configured");
        }
    }
}
```

### Example Flow
```bash
$ thresh generate "Python ML environment"
❌ Error: No AI provider configured

Get started with one of:
  1. GitHub token:  thresh config set github-token <TOKEN>
     Get token: https://github.com/settings/tokens
     
  2. OpenAI key:    thresh config set provider openai --api-key <KEY>
  
  3. Azure OpenAI:  thresh config set provider azure --endpoint <URL> --api-key <KEY>

Or run: thresh activate
```

### Success Criteria
✅ CLI can configure providers via API  
✅ Commands display clear guidance  
✅ Configuration persists across sessions  
✅ Generate command creates blueprints from natural language  

---

## Phase 5: Security & Best Practices ⏱️ 2-3 hours

**Objective**: Secure and responsible AI usage

### Tasks
- [ ] Token validation (format: `ghp_`, `gho_`, `github_pat_`)
- [ ] Implement token rotation support
- [ ] Add rate limiting (e.g., 10 requests/minute per user)
- [ ] Secure token storage review
  - DPAPI encryption on Windows
  - Keyring/Secret Service on Linux
- [ ] Audit logging for token operations
- [ ] Add `.thresh/` to `.gitignore` globally
- [ ] Sanitize logs (never log tokens)

### Security Checklist
- [ ] Tokens never logged or displayed in plaintext
- [ ] Config file permissions: `600` (user read/write only)
- [ ] No tokens in source control
- [ ] API validates token before use
- [ ] Failed auth attempts are logged

### Success Criteria
✅ Security audit passes  
✅ No tokens leak in logs/errors  
✅ Rate limiting prevents abuse  

---

## Phase 6: User Experience ⏱️ 2-3 hours

**Objective**: Smooth onboarding and clear guidance

### Tasks
- [ ] Create activation wizard:
  ```bash
  thresh activate
  
  Welcome to thresh! 🚀
  
  To use AI features, you need a GitHub token.
  
  1. Visit: https://github.com/settings/tokens/new
  2. Select: 'public_repo' scope (or 'repo' for private)
  3. Generate token
  4. Paste below:
  
  GitHub Token: ____
  ✅ Token validated and saved!
  ```
- [ ] Add status indicators:
  ```bash
  thresh status
  
  thresh Status:
  ✅ WSL installed
  ✅ GitHub token configured
  ✅ API running (http://localhost:5000)
  ⚠️  Docker not running
  ```
- [ ] Improve error messages with suggestions
- [ ] Add `--help` documentation for config commands
- [ ] Create troubleshooting guide

### Success Criteria
✅ First-time users can activate easily  
✅ Status command shows clear health check  
✅ Errors provide actionable next steps  

---

## Phase 7: Testing & Validation ⏱️ 4-5 hours

**Objective**: Comprehensive test coverage

### Unit Tests
- [ ] `GitHubModelsService` tests
  - Mock API responses
  - Test token selection logic
  - Error handling
- [ ] `ConfigurationService` tests
  - Token encryption/decryption
  - File I/O
  - Validation

### Integration Tests
- [ ] End-to-end blueprint generation
- [ ] Token activation flow
- [ ] API endpoint responses
- [ ] Streaming chat completion

### Load/Stress Tests
- [ ] Rate limiting behavior
- [ ] Concurrent requests
- [ ] Token rotation under load

### Manual Testing
- [ ] Test with free tier GitHub account
- [ ] Test with GitHub Copilot subscription
- [ ] Test error cases (invalid token, rate limit, network failure)

### Success Criteria
✅ 80%+ code coverage  
✅ All integration tests pass  
✅ Manual test scenarios validated  

---

## Phase 8: Documentation ⏱️ 2-3 hours

**Objective**: Complete user and developer documentation

### User Documentation
- [ ] Update `README.md` with GitHub Models setup
- [ ] Create `docs/GITHUB_MODELS.md`:
  - Token creation guide
  - Configuration options
  - Troubleshooting
  - FAQ
- [ ] Add examples to `docs/EXAMPLES.md`:
  - Blueprint generation
  - Custom prompts
  - API usage

### Developer Documentation
- [ ] Architecture diagram (include GitHub Models)
- [ ] API endpoint documentation (Swagger/OpenAPI)
- [ ] Configuration schema reference
- [ ] Security guidelines

### Video/Guides
- [ ] Quick start video (optional)
- [ ] Token setup walkthrough
- [ ] Blueprint generation demo

### Success Criteria
✅ New users can get started without help  
✅ All features documented  
✅ Troubleshooting covers common issues  

---

## Timeline & Milestones

| Phase | Duration | Milestone |
|-------|----------|-----------|
| Phase 0 | 1-2 hours | ✅ Infrastructure ready |
| Phase 1 | 3-4 hours | 🎯 Basic AI works |
| Phase 2 | 4-6 hours | 🔐 BYOK functional |
| Phase 3 | 6-8 hours | 🤖 Blueprint AI ready |
| Phase 4 | 3-4 hours | 💻 CLI integrated |
| Phase 5 | 2-3 hours | 🔒 Security hardened |
| Phase 6 | 2-3 hours | 😊 UX polished |
| Phase 7 | 4-5 hours | ✅ Tested & validated |
| Phase 8 | 2-3 hours | 📚 Documented |
| **Total** | **27-38 hours** | **Production ready** |

---

## Dependencies & Prerequisites

### Required
- ✅ .NET 9.0 SDK
- ✅ Aspire workload
- ✅ GitHub account (for testing)
- ⚠️ GitHub token with appropriate scopes

### NuGet Packages
```xml
<PackageReference Include="GitHub.Copilot.SDK" Version="*" />
<PackageReference Include="Microsoft.Extensions.AI" Version="*" />
<PackageReference Include="System.Security.Cryptography.ProtectedData" Version="9.0.0" />
```

### Prerequisites
- **GitHub Copilot CLI**: Must be installed and in PATH
  ```bash
  # Option 1: Via GitHub CLI extension (recommended)
  gh extension install github/gh-copilot
  
  # Option 2: Standalone binary
  # Download from https://github.com/cli/cli/releases
  
  # Verify installation
  copilot --version
  ```
- **GitHub Token** (for GitHub Models BYOK): GitHub PAT
  - Get from: https://github.com/settings/tokens
  - Scopes needed: None (for models access) or `read:user` (recommended)
- **Custom API Keys** (optional BYOK): OpenAI, Azure OpenAI, Anthropic
- **.NET 9.0 SDK**: ✅ Already installed (9.0.203)
- **Quarkus CLI**: ✅ Already in project (thresh-cli)

### Installation Steps
```bash
# 1. Ensure Copilot CLI is installed
copilot --version || gh extension install github/gh-copilot

# 2. Add SDK to .NET project
cd thresh-api/nova-api.ApiService
dotnet add package GitHub.Copilot.SDK
dotnet add package Microsoft.Extensions.AI

# 3. Build and test
cd ../..
dotnet build thresh-api/thresh-api.sln
```

### GitHub Token Scopes
- **Minimum**: No scopes needed (GitHub Models access)
- **Recommended**: `read:user` (for user identification)
- **Note**: Token is for GitHub Models API, not repo access

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Rate limiting on free tier | High | Clear error messages, caching, retry logic |
| Token security breach | Critical | Encryption, secure storage, audit logging |
| .NET 10 breaking changes | Low | Stay on .NET 9 LTS |
| GitHub Models API changes | Medium | Version pinning, error handling |
| User confusion with BYOK | Medium | Excellent UX and docs |

---

## Success Metrics

### Technical
- [ ] 100% uptime for API service
- [ ] <500ms response time for non-AI endpoints
- [ ] <5s response time for blueprint generation
- [ ] Zero token leaks in logs

### User Experience
- [ ] <5 minutes to activate (first time)
- [ ] <30 seconds to generate blueprint
- [ ] 90%+ success rate for valid prompts
- [ ] Clear errors for all failure modes

### Business
- [ ] Users can use their own GitHub quota
- [ ] No uncontrolled API costs
- [ ] Community can self-serve

---

## Next Steps After Completion

1. **Marketplace Integration**: Share blueprints in community
2. **Advanced Prompts**: Multi-turn conversations for complex environments
3. **Local Model Support**: Add Ollama for offline use
4. **Team Features**: Shared token pools for organizations
5. **Analytics**: Usage tracking (opt-in)

---

## Questions for Review

1. **Copilot CLI Installation**: Bundle in setup script or require manual installation?
2. **Default Configuration**: Use GitHub Models free tier initially, or require immediate activation?
3. **Storage**: DPAPI (Windows-only) vs cross-platform solution for WSL compatibility?
4. **Models**: Default to `gpt-4.1` (latest) or `gpt-5` (as shown in SDK examples)?
5. **Provider Preference**: GitHub token first, or offer OpenAI/Azure as primary option?
6. **Session Persistence**: Enable infinite sessions by default (recommended for long conversations)?
7. **Tool Safety**: Use `--allow-all` default or restrict tools initially?
8. **Java SDK**: Consider unofficial Java SDK for Quarkus, or keep .NET + Quarkus HTTP pattern?

### Recommendations
- ✅ **Use GitHub Models free tier** initially (easier onboarding)
- ✅ **Default to gpt-4.1** (faster, cheaper) with gpt-5 option
- ✅ **Enable infinite sessions** (better UX for blueprint iteration)
- ⚠️ **Allow-all tools** but document security implications
- ✅ **Stick with .NET SDK** (official, better support) + Quarkus as HTTP shell

---

**Plan Status**: 📋 Ready for Review  
**Start Date**: TBD  
**Estimated Completion**: 3-5 days (with focused work)
