# Multi-Provider AI Support

## Overview

thresh supports **three AI provider options** for blueprint generation and chat interactions:

1. **OpenAI** (default) - Direct OpenAI API with all latest models
2. **Azure OpenAI** - Enterprise OpenAI via Azure with compliance features
3. **GitHub Copilot SDK** - Integrated with Copilot subscription

## Supported Models

### OpenAI Provider

**GPT-4o Family** (Recommended):
- `gpt-4o` - Latest multimodal model (128K context)
- `gpt-4o-mini` - Fast, cost-effective (128K context)
- `gpt-4o-2024-08-06` - Specific version snapshot

**GPT-4 Turbo**:
- `gpt-4-turbo` - Latest GPT-4 Turbo
- `gpt-4-turbo-preview` - Preview version with newest features
- `gpt-4-turbo-2024-04-09` - Specific version

**GPT-4**:
- `gpt-4` - Original GPT-4 (8K context)
- `gpt-4-0613` - June 2023 snapshot
- `gpt-4-32k` - Extended 32K context window

**GPT-3.5 Turbo**:
- `gpt-3.5-turbo` - Fast, economical (16K context)
- `gpt-3.5-turbo-16k` - Extended context version

**Reasoning Models**:
- `o1-preview` - Advanced reasoning (128K context)
- `o1-mini` - Faster reasoning model

### Azure OpenAI Provider

Supports all OpenAI models via Azure deployments. Model names match your Azure deployment names.

**Benefits**:
- Enterprise compliance (SOC 2, HIPAA, etc.)
- Private networking and VNet integration
- Azure billing and cost management
- Regional data residency

### GitHub Copilot SDK Provider

**OpenAI Models**:
- `gpt-4o` - Latest GPT-4o (recommended)
- `o1-preview` - Advanced reasoning
- `o1-mini` - Faster reasoning

**Anthropic Models**:
- `claude-3.5-sonnet` - Latest Claude (best for code)
- `claude-3-opus` - Most capable Claude model
- `claude-3-sonnet` - Balanced performance

**Note**: Requires active GitHub Copilot subscription

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

### Provider Setup

#### 1. OpenAI (Direct API)

```bash
# Set OpenAI as provider
thresh config set default-provider openai

# Configure API key (get from https://platform.openai.com/api-keys)
thresh config set openai-api-key sk-proj-xxxxx

# Select model (optional, defaults to gpt-4o)
thresh config set default-model gpt-4o-mini      # Cost-effective
thresh config set default-model gpt-4o           # Most capable
thresh config set default-model gpt-4-turbo      # Latest GPT-4 Turbo
thresh config set default-model o1-preview       # Advanced reasoning
```

#### 2. Azure OpenAI

```bash
# Set Azure OpenAI as provider
thresh config set default-provider azure

# Configure Azure credentials
thresh config set azure-openai-endpoint https://your-resource.openai.azure.com
thresh config set azure-openai-key xxxxx

# Set your deployment name as model
thresh config set default-model your-gpt4o-deployment
```

#### 3. GitHub Copilot SDK

```bash
# Set Copilot SDK as provider
thresh config set aiprovider copilot

# Optional: Configure GitHub token
thresh config set github-token ghp_xxxxx

# Select model
thresh config set default-model gpt-4o              # OpenAI GPT-4o
thresh config set default-model claude-3.5-sonnet   # Anthropic Claude
thresh config set default-model o1-preview          # Reasoning model
```

**Requirements**:
- Active GitHub Copilot subscription
- GitHub Copilot CLI installed
- GitHub authentication configured

### Switch AI Provider

```bash
# Use OpenAI (default)
thresh config set default-provider openai

# Use Azure OpenAI
thresh config set default-provider azure

# Use GitHub Copilot SDK
thresh config set aiprovider copilot
```

### View Current Provider

```bash
thresh config list
```

Output example:
```
Configuration:
  default-provider: openai
  default-model: gpt-4o-mini
  openai-api-key: sk-proj-xxxxx
```

## Requirements by Provider

### 1. OpenAI Provider
- ✅ OpenAI API key: `thresh config set openai-api-key sk-proj-xxxxx`
- ✅ Model selection (optional): defaults to `gpt-4o`
- ✅ Internet access to OpenAI API

### 2. Azure OpenAI Provider
- ✅ Azure OpenAI resource with endpoint
- ✅ Azure OpenAI API key
- ✅ Model deployment created in Azure Portal
- ✅ VNet/private endpoint support (optional)

### 3. GitHub Copilot SDK Provider
- ✅ Active GitHub Copilot subscription ($10/month or $100/year)
- ✅ GitHub Copilot CLI installed
- ✅ GitHub authentication configured
- ✅ Optional: GitHub token for programmatic access

### Installing GitHub Copilot CLI

```bash
# Option 1: Via GitHub CLI extension
gh extension install github/gh-copilot

# Option 2: Direct download
# Visit: https://github.com/github/copilot-cli
```

## Usage Examples

### Generate Blueprint with Different Providers

#### Using OpenAI
```bash
thresh config set default-provider openai
thresh config set default-model gpt-4o-mini
thresh generate "Python data science environment with pandas and jupyter"
```

#### Using Azure OpenAI
```bash
thresh config set default-provider azure
thresh config set default-model my-gpt4o-deployment
thresh generate "Node.js microservices environment"
```

#### Using GitHub Copilot SDK
```bash
thresh config set aiprovider copilot
thresh config set default-model claude-3.5-sonnet
thresh generate "Rust development environment"
```

### Chat Mode with Different Models

```bash
# OpenAI with reasoning model
thresh config set default-provider openai
thresh config set default-model o1-preview
thresh chat

# Copilot with Claude
thresh config set aiprovider copilot
thresh config set default-model claude-3.5-sonnet
thresh chat
```

### Provider-Specific Use Cases

#### Enterprise/Compliance (Azure OpenAI)
```bash
# Use Azure for compliance requirements
thresh config set default-provider azure
thresh config set azure-openai-endpoint https://company.openai.azure.com
thresh generate "HIPAA-compliant Python environment"
```

#### Best Code Generation (Claude via Copilot)
```bash
# Use Claude for superior code understanding
thresh config set aiprovider copilot
thresh config set default-model claude-3.5-sonnet
thresh generate "Complex TypeScript monorepo setup"
```

#### Advanced Reasoning (o1 Models)
```bash
# Use o1-preview for complex blueprint logic
thresh config set default-provider openai
thresh config set default-model o1-preview
thresh generate "Multi-stage build environment with optimization"
```

## Features Comparison

| Feature | OpenAI | Azure OpenAI | GitHub Copilot SDK |
|---------|--------|--------------|-------------------|
| **Available Models** | GPT-4o, GPT-4 Turbo, GPT-4, GPT-3.5 Turbo, o1 | Same as OpenAI | gpt-4o, claude-3.5-sonnet, o1 |
| **Streaming** | ✅ | ✅ | ✅ |
| **Blueprint generation** | ✅ | ✅ | ✅ |
| **Chat mode** | ✅ | ✅ | ✅ |
| **Distribution discovery** | ✅ | ✅ | ❌ |
| **Authentication** | API key | Endpoint + key | GitHub auth |
| **Cost** | Pay per token | Azure billing | Copilot subscription |
| **Best For** | All models access | Enterprise/compliance | Copilot users |
| **Rate Limits** | Tier-based | Custom quotas | Copilot limits |
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
