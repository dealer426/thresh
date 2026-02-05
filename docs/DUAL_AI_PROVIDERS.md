# Dual AI Provider Support

## Overview

thresh now supports **two AI providers** for blueprint generation and chat interactions:

1. **OpenAI** (default) - Uses OpenAI's GPT models via Azure.AI.OpenAI SDK
2. **GitHub Copilot SDK** - Uses GitHub's models (GPT-4, Claude, etc.) via GitHub Copilot CLI

## Architecture

### Interface Abstraction

The implementation uses the **IAIService** interface to abstract AI provider functionality:

```csharp
public interface IAIService
{
    Task<string> GenerateBlueprintAsync(string prompt, bool streaming = true);
    Task ChatModeAsync();
    string ProviderName { get; }
    string ModelId { get; }
}
```

### Implementations

1. **OpenAIService** - Azure OpenAI implementation
   - Uses `Azure.AI.OpenAI` SDK
   - Supports streaming responses
   - Blueprint generation with JSON validation
   - Interactive chat mode
   - Custom distribution discovery

2. **GitHubCopilotService** - GitHub Copilot SDK implementation
   - Uses `GitHub.Copilot.SDK` v0.1.22
   - Requires GitHub Copilot CLI installed
   - Supports streaming via `AssistantMessageDeltaEvent`
   - Session management with `CopilotClient`
   - GitHub token authentication

### Factory Pattern

The **AIServiceFactory** selects the appropriate provider based on configuration:

```csharp
public static IAIService CreateAIService(
    ConfigurationService configService, 
    string? modelId = null, 
    string? providerOverride = null)
{
    var provider = providerOverride ?? configService.GetValue("aiprovider") ?? "openai";
    
    return provider.ToLowerInvariant() switch
    {
        "copilot" => new GitHubCopilotService(configService, modelId),
        "openai" => new OpenAIService(configService, modelId, providerOverride),
        _ => new OpenAIService(configService, modelId, providerOverride)
    };
}
```

## Configuration

### Switch AI Provider

```bash
# Use OpenAI (default)
thresh config set aiprovider openai

# Use GitHub Copilot SDK
thresh config set aiprovider copilot
```

### View Current Provider

```bash
thresh config list
```

Output includes:
```
Configuration:
  aiprovider: openai
  default-model: gpt-4o-mini
  github-token: ghp_...
  openai-api-key: sk-...
```

## Requirements

### OpenAI Provider
- ✅ OpenAI API key configured: `thresh config set openai-api-key sk-...`
- ✅ Model selection (optional): `thresh config set default-model gpt-4o-mini`

### GitHub Copilot SDK Provider
- ✅ GitHub Copilot CLI installed
- ✅ GitHub authentication configured
- ✅ GitHub token (optional): `thresh config set github-token ghp_...`
- ✅ Model selection (optional): `thresh config set default-model gpt-5`

### Installing GitHub Copilot CLI

```bash
# Option 1: Via GitHub CLI extension
gh extension install github/gh-copilot

# Option 2: Direct download
# Visit: https://github.com/github/copilot-cli
```

## Usage Examples

### Generate Blueprint with OpenAI

```bash
thresh config set aiprovider openai
thresh generate "Python development environment"
```

### Generate Blueprint with GitHub Copilot SDK

```bash
thresh config set aiprovider copilot
thresh generate "Node.js development environment"
```

### Chat Mode

```bash
# With OpenAI
thresh config set aiprovider openai
thresh chat

# With GitHub Copilot SDK
thresh config set aiprovider copilot
thresh chat
```

## Features Comparison

| Feature | OpenAI | GitHub Copilot SDK |
|---------|--------|-------------------|
| Streaming responses | ✅ | ✅ |
| Blueprint generation | ✅ | ✅ |
| Chat mode | ✅ | ✅ |
| Distribution discovery | ✅ | ❌ (OpenAI only) |
| Custom JSON cleaning | ✅ | ❌ (OpenAI only) |
| Model selection | GPT-4, GPT-3.5 | GPT-5, GPT-4, Claude |
| Authentication | API key | GitHub token or logged-in user |
| External dependency | None | GitHub Copilot CLI required |

## Implementation Details

### Package Dependencies

- **Azure.AI.OpenAI** v2.1.0 - OpenAI provider
- **GitHub.Copilot.SDK** v0.1.22 - GitHub Copilot provider
  - StreamJsonRpc v2.24.84
  - MessagePack v2.6.100
  - Nerdbank.Streams v2.14.94
  - Microsoft.VisualStudio.Threading v17.13.73

### Code Structure

```
thresh/Thresh/
├── Services/
│   ├── IAIService.cs              # Interface abstraction
│   ├── OpenAIService.cs           # OpenAI implementation
│   ├── GitHubCopilotService.cs    # GitHub Copilot implementation
│   └── ConfigurationService.cs    # Config management
├── Utilities/
│   └── AIServiceFactory.cs        # Provider factory
└── Models/
    └── ConfigurationSettings.cs    # Config model with AIProvider property
```

### Error Handling

Both providers include error handling with helpful messages:

**OpenAI errors:**
```
❌ OpenAI error: <message>
💡 Make sure your OpenAI API key is configured: thresh config set openai-api-key sk-...
```

**GitHub Copilot errors:**
```
❌ GitHub Copilot SDK error: <message>
💡 Make sure GitHub Copilot CLI is installed and authenticated.
```

## Testing

### Build Verification

```bash
cd thresh/Thresh
dotnet build
```

**Expected output:**
```
Build succeeded.
    3 Warning(s)
    0 Error(s)
```

### Configuration Tests

```bash
# Test provider switching
thresh config set aiprovider openai
thresh config list

thresh config set aiprovider copilot
thresh config list
```

### Runtime Tests

```bash
# Test OpenAI provider
thresh config set aiprovider openai
thresh generate "test blueprint for Python"

# Test GitHub Copilot SDK (requires CLI installed)
thresh config set aiprovider copilot
thresh generate "test blueprint for Node.js"
```

## Migration Notes

### From Single Provider (OpenAI)

The original `CopilotService` was refactored into:
1. **IAIService** interface (abstraction)
2. **OpenAIService** (renamed from CopilotService)
3. **GitHubCopilotService** (new implementation)

All existing functionality is preserved in `OpenAIService`:
- Streaming responses ✅
- Blueprint generation ✅
- Chat mode ✅
- Distribution discovery ✅
- JSON cleaning ✅

### Configuration Changes

New configuration property added:
```json
{
  "aiprovider": "openai"  // or "copilot"
}
```

**Default behavior:** If `aiprovider` is not set, defaults to `"openai"`.

## Future Enhancements

1. **Add DiscoverDistributionAsync to IAIService**
   - Currently only in OpenAIService
   - Would enable GitHub Copilot SDK to discover custom distros

2. **Provider-specific model validation**
   - Validate model IDs against provider capabilities
   - Provide helpful suggestions for invalid models

3. **Provider auto-detection**
   - Auto-select provider based on configured credentials
   - Fallback chain: Copilot → OpenAI → Error

4. **Provider status command**
   - `thresh provider status` - Check provider availability
   - Show configured vs. available providers

5. **Model selection per provider**
   - `thresh config set openai-model gpt-4o`
   - `thresh config set copilot-model gpt-5`

## Troubleshooting

### "GitHub Copilot CLI not found"

**Problem:** GitHubCopilotService can't connect to CLI

**Solutions:**
1. Install GitHub Copilot CLI: `gh extension install github/gh-copilot`
2. Verify installation: `copilot --version`
3. Ensure CLI is in PATH
4. Switch to OpenAI: `thresh config set aiprovider openai`

### "Authentication failed"

**OpenAI:**
```bash
thresh config set openai-api-key sk-proj-...
```

**GitHub Copilot:**
```bash
# Option 1: Use logged-in GitHub user (default)
gh auth login

# Option 2: Provide explicit token
thresh config set github-token ghp_...
```

### Build Errors

If you encounter build errors after updating:
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## Release Notes

**Version:** 1.0.1+  
**Date:** January 2025

### Added
- ✅ GitHub Copilot SDK support (v0.1.22)
- ✅ IAIService abstraction interface
- ✅ AIServiceFactory for provider selection
- ✅ GitHubCopilotService implementation
- ✅ `aiprovider` configuration property
- ✅ Dual AI provider architecture

### Changed
- 🔄 Refactored CopilotService → OpenAIService
- 🔄 Program.cs updated to use AIServiceFactory
- 🔄 ConfigurationSettings with AIProvider property

### Dependencies
- ➕ GitHub.Copilot.SDK v0.1.22 (new)
- ➕ StreamJsonRpc v2.24.84 (transitive)
- ➕ MessagePack v2.6.100 (transitive)
- ➕ Nerdbank.Streams v2.14.94 (transitive)
- ➕ Microsoft.VisualStudio.Threading v17.13.73 (transitive)

## Support

For issues or questions:
- GitHub Issues: https://github.com/dealer426/thresh/issues
- Documentation: https://github.com/dealer426/thresh/blob/main/README.md
