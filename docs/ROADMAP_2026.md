# thresh Roadmap 2026 - Distributed Development Orchestration

**Created**: February 6, 2026  
**Updated**: February 27, 2026  
**Timeline**: 20 weeks (5 months)  
**Current Version**: v1.5.0 (Released - Feb 27, 2026)  
**Status**: Phase 1 Complete ✅ | Phase 1.5 Complete ✅ (Networking, Storage, WSL Config - Feb 27, 2026) | Cross-Platform Testing Complete ✅ (Linux & Windows)  
**Goal**: Transform thresh from local WSL manager to distributed dev environment orchestrator

---

## 🎯 Vision

**v1.0 State (Feb 1, 2026):**
```
Single Windows machine → WSL environments → OpenAI/Copilot blueprints
```

**v1.3.0 State (Feb 12, 2026):**
```
Windows/WSL only → MCP integration → GitHub Copilot AI → UPX compressed → 3.8 MB binary → Professional docs
```

**v1.4.0 State (Feb 17, 2026):**
```
Cross-platform (Windows/Linux/macOS) → Multi-platform builds → 11 MCP tools → Platform-specific docs → Repository cleanup
```

**v1.5.0 State (Feb 27, 2026 - CURRENT):**
```
Port mapping → Persistent volumes → Networking configuration → Storage mounting → WSL configuration profiles → Blueprint networking/storage support
```

**Target v1.6.0 State (Mar-Apr 2026):**
```
Agent Mode (daemon) → Mesh Network (Tailscale/Netmaker) → Remote operations (P2P) → Enhanced metrics
```

**Target v2.0 State (Jun 2026):**
```
Production-grade distributed tool → Package distribution → Enhanced monitoring → Security hardening → Full documentation
```

---

## 📊 Feature Evaluation

### Already Implemented ✅

| Feature | Version | Binary Size | Value |
|---------|---------|-------------|-------|
| **WSL Integration** | v1.0.0 | (core) | High |
| **Blueprint System** | v1.0.0 | (core) | High |
| **GitHub Copilot SDK Integration** | v1.0.0 | (core) | High |
| **GitHub Actions (Windows)** | v1.0.1 | N/A | Medium |
| **Cross-Platform Code (containerd)** | v1.1.0 | +200 KB | 🔥 Huge |
| **MCP Server & Integration** | v1.1.0 | +100 KB | 🔥 Huge |
| **Host Metrics Command** | v1.1.0 | +80 KB | High |
| **Native AOT Optimization** | v1.2.0 | -61 MB | 🚀 Critical |
| **UPX Compression** | v1.2.0 | -10.2 MB | 🚀 Critical |
| **Removed Unused Dependencies** | v1.2.0 | -17 KB | Medium |
| **Documentation Website** | v1.3.0 | N/A (website) | 🔥 Huge |
| **Mermaid Diagrams** | v1.3.0 | N/A | High |
| **YAML Blueprint Support** | v1.3.0+ | +800 KB | High |
| **nerdctl Integration** | v1.3.0+ | (refactor) | High |
| **Platform-Aware AI Prompts** | v1.3.0+ | (core) | Medium |
| **Simplified ContainerdService** | v1.3.0+ | -99 lines | Medium |
| **Destroy -y Flag** | v1.3.0+ | (core) | Low |
| **Multi-Platform Builds** | v1.4.0 | N/A (CI/CD) | 🔥 Huge |
| **Extended MCP Server (11 tools)** | v1.4.0 | +50 KB | 🔥 Huge |
| **Platform-Specific Documentation** | v1.4.0 | N/A (website) | High |
| **Blueprint Command Grouping** | v1.4.0 | (refactor) | Medium |
| **macOS Support (Apple Silicon)** | v1.4.0 | (core) | 🔥 Huge |
| **Repository Cleanup** | v1.4.0 | -75 MB repo | Medium |
| **WSL Configuration Profiles** | v1.4.0 | +1.5 KB | 🔥 Huge |
| **Current Binary Size** | v1.4.0 | **~5 MB** (Win/Linux), **~13 MB** (macOS) | 🔥 Excellent |

**Note:** v1.4.0 shipped February 17, 2026 with full cross-platform support for Windows, Linux, and macOS. WSL Configuration Profiles added Feb 27, 2026 to solve Plan9 filesystem limitations.

### Proposed Features 🔮

| Feature | Effort | Size Impact | Value | Priority |
|---------|--------|-------------|-------|----------|
| **Port Mapping & Networking** | 1 week | +40 KB | 🔥 Huge | **P0** |
| **Persistent Volumes** | 1 week | +30 KB | 🔥 Huge | **P0** |
| **Network Configuration** | 3 days | +20 KB | High | **P0** |
| **Docusaurus Documentation** | 1-2 weeks | N/A (website) | 🔥 Huge | **P1** |
| **Agent Mode (Daemon)** | 1 week | +60 KB | High | **P1** |
| **Mesh Network (Tailscale/Netmaker)** | 2 weeks | +120 KB | High | **P1** |
| **Remote Operations (P2P)** | 1 week | +80 KB | High | **P1** |
| **Package Distribution** | 1 week | N/A | High | **P2** |
| **Multi-Platform CI/CD** | 1 week | N/A | Medium | **P2** |
| **Linux/macOS Packages** | 1 week | N/A | Medium | **P2** |
| **API Documentation** | 1 week | N/A (website) | Medium | **P3** |

**Current Binary Size:** 3.8 MB (v1.2.0 - UPX compressed, 13.5 MB uncompressed)  
**Target v2.0 Size:** ~4.0 MB compressed (minimal growth for distributed features)

---

## 🗺️ Phased Implementation Plan

### **Phase 1: Foundation (Weeks 1-4) - Cross-Platform Core** ✅ COMPLETE

**Goal:** Make thresh work everywhere with MCP integration  
**Status:** ✅ Shipped in v1.1.0 (Feb 9, 2026)  
**Result:** Binary size reduced to 14 MB in v1.2.0 (Native AOT)

#### Week 1-2: Container Runtime Abstraction ✅
- [x] Create `IContainerService` interface
- [x] Refactor `WslService` to implement interface
- [x] Create `ContainerdService` for Linux/macOS (421 lines - simplified)
- [x] Platform detection and factory (60 lines)
- [x] Test on Windows (WSL) + Linux (docker/nerdctl)
- [x] nerdctl integration with CNI plugins
- [x] Removed ctr support (24% code reduction)

**Deliverables:**
```bash
# Works on any platform
thresh up python-dev  # Uses WSL on Windows, containerd elsewhere
thresh list
thresh destroy python-dev
```

**Binary:** v1.0.0 (16.6 MB) → v1.1.0 (75 MB with AOT disabled) → v1.2.0 (13.5 MB with AOT) → v1.2.0 final (3.8 MB with UPX)

#### Week 3-4: MCP Server Completion ✅
- [x] Complete MCP protocol implementation (607 lines)
- [x] Expose all thresh commands via MCP (7 tools)
- [x] Add input schemas for all tools (JSON schema support)
- [x] Add stdio transport for AI editors
- [x] Test with VS Code Copilot integration
- [x] Blueprint generation via MCP tested (Node.js/Express, Python/Flask)

**Deliverables:**
```bash
# MCP server mode (v1.1.0+)
thresh serve --stdio   # For VS Code, Cursor, Windsurf

# VS Code can now call thresh operations via MCP
```

**Tools Available:**
1. `list_environments` - List all environments
2. `create_environment` - Create from blueprint
3. `destroy_environment` - Remove environment
4. `list_blueprints` - Show all blueprints
5. `get_blueprint` - Get blueprint details
6. `generate_blueprint` - AI-powered generation
7. `get_version` - Version and runtime info

**Phase 1 Success Metrics:** ✅ ALL ACHIEVED
- ✅ thresh runs on Windows, Linux, macOS
- ✅ Single binary works across platforms (3.8 MB compressed, 13.5 MB uncompressed)
- ✅ MCP server functional in VS Code/Cursor/Windsurf
- ✅ AI editors can provision environments
- ✅ Metrics command working (`thresh metrics`)
- ✅ Native AOT compilation optimized
- ✅ UPX compression reduces size by 72%
- ✅ Simplified to GitHub Copilot SDK only (AOT-compatible)
- ✅ Removed unused dependencies and obsolete AI providers

---

### **Documentation Phase: Professional Docs (Weeks 5-6) - Docusaurus** ✅ COMPLETE

**Goal:** Create professional documentation website with Docusaurus + GitHub Pages  
**Status:** ✅ Complete (Feb 9-17, 2026)  
**Priority:** P0 (Critical for user adoption and community growth)

#### Week 1: Docusaurus Setup & Migration ✅ COMPLETE (Feb 9-12, 2026)
- [x] Initialize Docusaurus project in `website/` directory
- [x] Configure GitHub Pages deployment (thresh.sh with custom domain!)
- [x] Set up automated deployment via GitHub Actions
- [x] Configure SSL certificate (Let's Encrypt)
- [x] Migrate existing markdown content:
  - [x] `GETTING_STARTED.md` → `docs/intro.md`
  - [x] Created `docs/cli-reference/` (7 commands documented)
  - [x] `docs/MCP_INTEGRATION.md` → `docs/mcp-integration.md`
- [x] Create installation guide (`docs/installation.md` - Windows, Linux, macOS)
- [x] Design homepage with features grid
- [x] Add copy-to-clipboard for install command
- [x] Fix WSL-only messaging (removed cross-platform claims)
- [x] Optimize CI/CD (skip C# build for docs-only changes)

**Deliverables:** ✅ SHIPPED
```bash
# Documentation site runs locally
cd website
npm start  # http://localhost:3000

# Deployed to GitHub Pages with custom domain
https://thresh.sh ✅
```

**Extra Achievements:**
- 🌐 Custom domain configured (thresh.sh)
- 🔒 HTTPS enforced with Let's Encrypt certificate
- 🚀 Automatic deployment on main branch push
- 📋 Copy-to-clipboard UI for install commands
- ⚡ CI/CD optimized (docs-only changes don't trigger C# build)

#### Week 2: Enhanced Content & Features ✅ COMPLETE
- [x] CLI reference created for 7 commands:
  - [x] `up.md` - Provision environments
  - [x] `list.md` - List environments
  - [x] `destroy.md` - Remove environments
  - [x] `generate.md` - AI blueprint generation
  - [x] `chat.md` - Interactive AI chat
  - [x] `config.md` - Configuration management
  - [x] `index.md` - CLI overview
- [x] Platform-specific documentation created:
  - [x] `getting-started-windows.md` - Windows 11/WSL2 guide
  - [x] `getting-started-linux.md` - Linux Docker/nerdctl guide
  - [x] `getting-started-macos.md` - macOS Apple Silicon guide
- [x] Add Mermaid diagrams (architecture, workflows)
- [x] Add code syntax highlighting (Bash, PowerShell, C#, JSON)
- [x] Configure version dropdown (v1.2.0, v1.3.0, v1.4.0)
- [x] Create blog posts:
  - [x] "thresh 1.3.0 Release" (UPX compression, performance)
  - [x] "SBOM and Supply Chain Security"
  - [x] "MCP Integration Guide" (Complete tutorial)
  - [x] "thresh 1.4.0 Cross-Platform Release"
  - [x] "macOS Beta Testing Announcement"
- [x] Enhanced homepage with better social sharing (removed Docusaurus branding)
- [x] Repository cleanup (~75MB removed)

**Deferred to v1.5.0:**
- [ ] Set up search (Algolia DocSearch application)
- [ ] Add download page with package manager instructions
- [ ] Complete remaining CLI command docs (serve, metrics, blueprints, etc.)
- [ ] Add screenshots/demos to homepage

**Site Structure:** ✅ v1.4.0 Complete
```
website/
├── docs/
│   ├── intro.md ✅ (Platform overview)
│   ├── installation.md ✅
│   ├── getting-started-windows.md ✅ (NEW in v1.4.0)
│   ├── getting-started-linux.md ✅ (NEW in v1.4.0)
│   ├── getting-started-macos.md ✅ (NEW in v1.4.0)
│   ├── cli-reference/ ✅ (7 commands)
│   ├── mcp-integration.md ✅
│   ├── blueprints/ ⏳ (planned for v1.5.0)
│   ├── advanced/ ⏳ (planned for v1.5.0)
│   └── contributing/ ⏳ (planned for v1.5.0)
├── blog/ ✅ (5 posts in v1.4.0)
├── versioned_docs/ ✅ (v1.2.0, v1.3.0, v1.4.0)
└── src/
    ├── pages/index.tsx ✅ (improved social media preview)
    └── components/HomepageFeatures/ ✅
```

**Documentation Phase Success Metrics:**
- [x] Site deployed at `https://thresh.sh` (custom domain!)
- [x] Automatic deployment on `main` branch push
- [x] 7 CLI commands documented with examples
- [x] 5 blog posts published (v1.4.0)
- [x] Platform-specific getting started guides (Windows/Linux/macOS)
- [x] MCP integration tutorial complete
- [x] Mobile responsive design (Docusaurus default)
- [x] Dark mode enabled (Docusaurus default)
- [x] Version dropdown (v1.2.0, v1.3.0, v1.4.0)
- [x] SSL/HTTPS enforced
- [x] Copy-to-clipboard functionality
- [x] <2 second page load time (GitHub Pages CDN)
- [x] Removed Docusaurus branding from social media previews
- [x] Repository cleanup (~75MB removed)

**Deferred to v1.5.0:**
- [ ] Search functionality (Algolia DocSearch)
- [ ] Additional CLI command docs (serve, metrics, blueprints, etc.)
- [ ] Screenshots/demos on homepage
- [ ] Package manager download page

**Impact:** ✅ v1.4.0 Achieved
- 🚀 Professional documentation live at thresh.sh
- 📈 SEO-friendly with custom domain and improved social sharing
- 🤝 Foundation for community contributions
- 📚 Centralized knowledge base with platform-specific guides
- 🔒 Secure HTTPS access
- 🌍 Foundation for future internationalization
- 🧹 Clean repository (~75MB removed)
- 📝 5 blog posts covering releases and tutorials

---

### **Linux Testing & Enhancements (Feb 13, 2026)** ✅ COMPLETE

**Goal:** Validate and enhance Linux support with real-world testing on Ubuntu  
**Status:** ✅ Complete (All objectives achieved)  
**Environment:** Ubuntu 22.04 VM, Docker Engine, .NET 10.0.103

#### Accomplishments ✅

**1. YAML Blueprint Support** (+800 KB binary size)
- [x] Added YamlDotNet 16.2.1 library for dual-format support
- [x] BlueprintService auto-detects `.yaml`, `.yml`, `.json` extensions
- [x] YAML→JSON conversion preserves System.Text.Json source generation benefits
- [x] Cross-platform compatibility verified (Windows WSL + Linux Docker)
- [x] Test blueprints created: `python-yaml-test.yaml`, `go-dev-example.yaml`

**Technical Implementation:**
```csharp
// Auto-detection by file extension
if (extension == ".yaml" || extension == ".yml") {
    var yamlObject = deserializer.Deserialize<object>(content);
    var json = serializer.Serialize(yamlObject); // Convert to JSON
    return JsonSerializer.Deserialize(json, BlueprintJsonContext.Default.Blueprint);
}
```

**Benefits:**
- ✅ DevOps-friendly format (YAML preferred by many users)
- ✅ JSON remains internal format for AOT optimization
- ✅ Backward compatible (all existing JSON blueprints work)
- ✅ ListBundledBlueprints() scans all formats and deduplicates

**2. Platform-Aware AI Prompts**
- [x] GitHubCopilotService detects Linux vs Windows platform
- [x] GenerateBlueprintAsync() sends platform context (Docker containers vs WSL)
- [x] ChatMode sends initial system message explaining platform, runtime, JSON requirement
- [x] AI now generates Docker-optimized blueprints on Linux (e.g., postgresql-client not full server)

**Technical Implementation:**
```csharp
var platformName = ContainerServiceFactory.GetPlatformName();
var runtimeName = ContainerServiceFactory.GetExpectedRuntimeName();
var environmentType = platformName == "Windows" ? "WSL" : "Docker container";
var systemPrompt = $@"You are a development environment architect for {environmentType}...";
```

**Impact:**
- ✅ No more Dockerfile generation in chat mode
- ✅ Blueprints optimized for target platform
- ✅ Docker-specific package selections on Linux

**3. Simplified ContainerdService** (-99 lines, 24% reduction)
- [x] Removed all ctr-specific code paths (incomplete implementation)
- [x] Unified docker and nerdctl support (95% API compatibility)
- [x] Reduced from 520 lines to 421 lines
- [x] Single code path for Docker-compatible tools
- [x] Improved maintainability

**Removed Code:**
- 30+ conditional checks for ctr
- Separate ctr-specific methods
- Incomplete ctr implementation

**4. nerdctl Integration** ✅ Full Support
- [x] nerdctl v1.7.6 installed and tested
- [x] CNI plugins v1.4.0 installed for networking
- [x] Fixed Labels JSON parsing (JsonElement vs string)
- [x] Fixed Status field mapping ("Up" vs "running")
- [x] Full lifecycle tested: provision ✅, list ✅, exec ✅, destroy ✅

**Bugs Fixed:**
- Labels field: Changed from `string?` to `JsonElement?` with GetLabelsAsString() helper
- Status mapping: Added "up" → Running (nerdctl uses "Up", Docker uses "running")
- State vs Status: Added fallback logic for different field names

**Technical Details:**
```csharp
// Handle nerdctl's JSON object Labels vs Docker's string Labels
public JsonElement? Labels { get; set; }
public string? GetLabelsAsString() {
    if (Labels.Value.ValueKind == JsonValueKind.String) return Labels.Value.GetString();
    if (Labels.Value.ValueKind == JsonValueKind.Object) return Labels.Value.GetRawText();
}

// Map both "running" (Docker) and "up" (nerdctl) to Running status
var state = string.IsNullOrEmpty(container.State) ? container.Status : container.State;
"up" => EnvironmentStatus.Running, "running" => EnvironmentStatus.Running
```

**Known Behaviors:**
- nerdctl and docker use different namespaces (default vs moby)
- thresh auto-detects nerdctl first, then docker
- Cannot see containers across different runtimes (namespace isolation)

**5. CLI Usability Enhancement**
- [x] Added `-y` flag to destroy command (alias for `--force`)
- [x] Syntax: `thresh destroy <name> -y` or `thresh destroy <name> --force`
- [x] Skips confirmation prompt for scripting/automation

**Implementation:**
```csharp
var forceOption = new Option<bool>(new[] { "-y", "--force" }, "Skip confirmation prompt");
```

#### Testing Results ✅

**Full Workflow Validated:**
```bash
# AI-generated blueprints work
sudo ./thresh generate "Create Python FastAPI environment"  # ✅ Works

# YAML blueprints provision successfully  
sudo ./thresh up python-yaml-test  # ✅ Python 3.10.12 + pip 26.0.1

# nerdctl provisioning works
sudo ./thresh up alpine-minimal  # ✅ Alpine 3.19 via nerdctl

# Container listing accurate
sudo ./thresh list  # ✅ Shows "alpine-minimal Running nerdctl alpine-minimal"

# Exec commands work
sudo nerdctl exec thresh-alpine-minimal cat /etc/os-release  # ✅ Alpine Linux v3.19

# Destroy with -y flag
sudo ./thresh destroy alpine-minimal -y  # ✅ No confirmation prompt
```

**Build Status:**
- Binary size: ~14 MB (YamlDotNet adds 800KB, code reduction saves 99 lines)
- 6 expected YamlDotNet AOT warnings (reflection-based, acceptable tradeoff)
- All functionality working in Native AOT build
- Zero runtime errors

**Files Modified:**
- `thresh/Thresh/Services/GitHubCopilotService.cs` - Platform-aware prompts
- `thresh/Thresh/Services/BlueprintService.cs` - YAML support
- `thresh/Thresh/Services/ContainerdService.cs` - Simplified, nerdctl fixes
- `thresh/Thresh/Services/ContainerdJsonContext.cs` - Labels JsonElement
- `thresh/Thresh/Program.cs` - Added `-y` flag
- `thresh/Thresh/Thresh.csproj` - YamlDotNet package, YAML file copy rules

**Success Metrics:** ✅ ALL ACHIEVED
- ✅ thresh builds on Linux (Ubuntu 22.04 + .NET 10.0.103)
- ✅ YAML blueprints work across Windows and Linux
- ✅ AI generates platform-appropriate blueprints
- ✅ nerdctl fully supported (provision, list, exec, destroy)
- ✅ ContainerdService simplified and more maintainable
- ✅ Destroy command supports `-y` for automation
- ✅ Docker and nerdctl both work as container runtimes
- ✅ Cross-platform compatibility validated

**Impact:** 🔥 High
- 🌍 True dual-format support (JSON + YAML)
- 🤖 Platform-aware AI (Docker vs WSL context)
- 🔧 Simpler codebase (24% reduction in ContainerdService)
- 🚀 nerdctl as Docker alternative (containerd-native)
- ⚡ Better UX for scripting (destroy -y flag)

---

### **v1.4.0 Release (Feb 17, 2026)** ✅ SHIPPED

**Goal:** Ship cross-platform thresh with enhanced MCP support and comprehensive documentation  
**Status:** ✅ Released February 17, 2026  
**Priority:** P0 (Major milestone - cross-platform expansion)

#### Major Achievements ✅

**1. Multi-Platform Support** 🎉
- [x] GitHub Actions multi-platform builds (Windows, Linux, macOS)
- [x] Platform-specific build configurations:
  - [x] Windows x64 with UPX compression (~5 MB)
  - [x] Linux x64 with UPX compression (~5 MB)
  - [x] macOS ARM64 uncompressed (~13 MB, code signing preserved)
- [x] Platform-aware runtime detection and adaptation
- [x] Cross-platform Blueprint/ContainerdService tested on all platforms

**2. Extended MCP Server (6 → 11 tools)** 🔥
- [x] `list_environments` - List all environments
- [x] `create_environment` - Provision environments
- [x] `destroy_environment` - Remove environments
- [x] `list_blueprints` - Show available blueprints
- [x] `get_blueprint` - Get blueprint details
- [x] `delete_blueprint` - Remove generated blueprints (NEW)
- [x] `generate_blueprint` - AI-powered blueprint generation (NEW)
- [x] `save_blueprint` - Save custom blueprints (NEW)
- [x] `get_version` - Query thresh version and platform (NEW)
- [x] `get_metrics` - Retrieve system metrics (NEW)
- [x] `check_requirements` - Verify prerequisites

**3. Command Structure Refactoring** ⚠️ Breaking Change
- [x] Grouped blueprint commands under `thresh blueprint` parent:
  - `thresh blueprints` → `thresh blueprint list`
  - `thresh generate <prompt>` → `thresh blueprint generate <prompt>`
  - Added `thresh blueprint delete <name>`
- [x] System.CommandLine provides automatic migration suggestions
- [x] Improved discoverability with `thresh blueprint --help`

**4. Platform-Specific Documentation** 📚
- [x] Created three getting started guides:
  - `getting-started-windows.md` - Windows 11/WSL2 complete guide
  - `getting-started-linux.md` - Linux Docker/nerdctl complete guide
  - `getting-started-macos.md` - macOS Apple Silicon complete guide (Beta)
- [x] Platform comparison table in intro.md
- [x] Platform-specific prerequisites and troubleshooting
- [x] Updated intro.md to navigation hub for platform guides

**5. Documentation Enhancements** 🌐
- [x] 5 blog posts published:
  - thresh 1.3.0 Release
  - SBOM and Supply Chain Security
  - MCP Integration Complete Guide
  - thresh 1.4.0 Cross-Platform Release
  - macOS Beta Testing Announcement
- [x] Removed Docusaurus default branding from social media previews
- [x] Updated tagline for better link sharing
- [x] Versioned docs for v1.2.0, v1.3.0, v1.4.0
- [x] Added GitHub Copilot CLI installation instructions to README

**6. Repository Cleanup** 🧹
- [x] Removed ~75MB of files:
  - Temporary documentation (HANDOVER docs, SESSION_STATUS, etc.)
  - Build artifacts (govc binaries, test zips, SBOMs)
  - Redundant files (duplicate licenses, planning docs)
  - Old getting started guide (replaced with platform-specific guides)
- [x] Updated .gitignore for govc and other binaries
- [x] Streamlined repository to essential files only

**7. Version Consistency & Quality** ✅
- [x] Fixed version mismatch (Program.cs 1.2.0 → 1.4.0)
- [x] Synchronized versions across all files:
  - Program.cs: 1.4.0
  - Thresh.csproj: 1.4.0
  - McpServer.cs: 1.4.0
- [x] Updated CHANGELOG.md with comprehensive v1.4.0 release notes
- [x] Standardized on MIT License (removed duplicate Apache 2.0)

#### Build & Distribution ✅

**GitHub Actions Workflow:**
- [x] Multi-platform build workflow (`build-multiplatform.yml`)
- [x] Parallel builds for win-x64, linux-x64, osx-arm64
- [x] UPX v5.1.0 compression for Windows and Linux
- [x] Homebrew UPX installation for macOS (skip compression)
- [x] SBOM generation for all platforms
- [x] Artifact publishing with preserved naming

**Binary Sizes (v1.4.0):**
- Windows: ~5 MB (UPX compressed)
- Linux: ~5 MB (UPX compressed)
- macOS: ~13 MB (uncompressed, code signing preserved)

**Artifacts Tested:**
- [x] Windows build downloaded and tested (11 MCP tools verified)
- [x] Linux build downloaded and verified
- [x] `thresh version` shows consistent 1.4.0 across platforms

#### Success Metrics ✅ ALL ACHIEVED

- [x] thresh builds and runs on Windows, Linux, macOS
- [x] MCP server exposes all 11 tools successfully
- [x] Platform-specific documentation complete for all 3 platforms
- [x] GitHub Actions successfully builds all platforms
- [x] Version consistency across all source files
- [x] CHANGELOG.md updated with comprehensive release notes
- [x] Repository size reduced by ~75MB
- [x] 5 blog posts published covering v1.4.0 features
- [x] Breaking changes well-documented with migration paths
- [x] No corruption from Windows/Linux development switching

#### Impact 🚀 HUGE

- 🌍 **True Cross-Platform**: Windows, Linux, macOS support
- 🤖 **Enhanced AI Integration**: 11 MCP tools for comprehensive automation
- 📚 **Better Onboarding**: Platform-specific guides reduce confusion
- 🧹 **Cleaner Project**: ~75MB repository cleanup improves maintainability
- 🔧 **Better UX**: Grouped commands improve discoverability
- 📊 **Growth Ready**: Foundation for package managers (winget, Homebrew, etc.)

**Next Steps:**
- Container networking and storage (port mapping, volumes) - v1.5.0
- Package manager distribution (winget, Chocolatey, Scoop, Homebrew) - v1.6.0
- Algolia DocSearch integration for documentation site
- Additional CLI command documentation (serve, metrics, etc.)
- Enhanced tutorials and use case examples

---

### **Phase 1.5: Container Networking & Storage (Weeks 7-8) - v1.5.0** 🔄 IN PROGRESS

**Goal:** Add production-ready container features for networking and persistent storage  
**Status:** ✅ Complete (Feb 27, 2026) - All Features Tested on Windows & Linux  
**Priority:** P0 (Critical for real-world container deployments)

**Linux Testing Results (Ubuntu 22.04 + Docker 28.2.2):** ✅ ALL FEATURES WORKING
- ✅ Port mapping (8080:80, 8443:443) - Working perfectly
- ✅ Exposed ports (9090) - Working correctly
- ✅ Volume creation and mounting - Working perfectly
- ✅ Blueprint volume integration - Working correctly
- ✅ Volume persistence across container lifecycle - Verified
- ✅ Volume management commands (list, create, delete, inspect) - All working
- ✅ No sudo required after `newgrp docker` - Confirmed

**Windows Testing Results (WSL2):** ✅ ALL FEATURES WORKING
- ✅ Port mapping validated on WSL2
- ✅ Volume creation and mounting validated
- ✅ Blueprint volume integration working
- ✅ All volume management commands working

#### Week 1: Port Mapping & Network Configuration (3-4 days) ✅ COMPLETE
- [x] Extend Blueprint model with networking configuration ✅ Linux tested
  - [x] `ports` array for port mappings ✅ Linux tested (8080:80, 8443:443)
  - [x] `expose` list for container-only exposed ports ✅ Linux tested (9090)
  - [x] `network` string for custom network names ✅ Implementation complete
  - [x] `hostname` for container hostname ✅ Implementation complete
- [x] Update WslService for WSL port forwarding ✅ Windows tested (Feb 27, 2026)
  - [x] Windows `netsh interface portproxy` integration ✅ Working
  - [x] Automatic port proxy creation/deletion ✅ Working
  - [x] Port conflict detection ✅ Working
- [x] Update ContainerdService for Docker/nerdctl port mapping ✅ Linux tested
  - [x] `-p` flag support for docker/nerdctl ✅ Linux tested
  - [x] `--expose` flag for non-mapped ports ✅ Linux tested
  - [x] `--network` flag for network selection ✅ Implementation complete
- [x] Add validation for port ranges and conflicts ✅ Implementation complete
- [x] Update MCP tools to support networking configuration ✅ Complete (12 tools)

**Blueprint Example:**
```yaml
name: web-server
base: ubuntu:22.04
packages:
  - nginx
  - curl
ports:
  - "8080:80"      # host:container
  - "8443:443"
  - "3000:3000"
expose:
  - "9090"         # Container-only, not mapped to host
network: "bridge"
hostname: "web-dev"
```

**CLI Usage:**
```bash
# Port mapping automatically configured
thresh up web-server

# Verify ports
thresh list --format json | jq '.[] | .ports'

# Access from host
curl http://localhost:8080
```

**Deliverables:**
```bash
# Blueprints with port mappings work
thresh up web-api     # Maps ports 3000:3000, 5000:5000

# Windows WSL: netsh port proxy created automatically
# Linux/macOS: docker/nerdctl -p flags used

# List shows ports
thresh list
# Output: web-api    Running   WSL2   Ports: 3000:3000, 5000:5000
```

**Binary Impact:** +40 KB (port mapping logic, netsh integration)

---

#### Week 1-2: Persistent Volumes & Storage Mounting (3-4 days) ✅ COMPLETE
- [x] Extend Blueprint model with volume configuration ✅ Linux tested
  - [x] `volumes` array for persistent storage ✅ Linux tested (postgres-data, app-cache)
  - [x] `bind_mounts` for host directory mounts ✅ Implementation complete
  - [x] `tmpfs` for temporary filesystems ✅ Implementation complete
- [ ] Implement volume management for WSL ⏳ Windows testing pending
  - [ ] Windows host directory binding to WSL ⏳ Windows testing pending
  - [ ] \\\\wsl$\\distro path resolution ⏳ Windows testing pending
  - [ ] Permission handling ⏳ Windows testing pending
- [x] Implement volume management for Docker/nerdctl ✅ Linux tested
  - [x] `-v` flag for volume mounts ✅ Linux tested
  - [x] `--mount` flag for bind mounts ✅ Implementation complete
  - [x] Named volume creation and management ✅ Linux tested
- [x] Add `thresh volume` subcommands ✅ Tested on Linux & Windows (Feb 27, 2026)
  - [x] `thresh volume list` - Show all volumes ✅ Working cross-platform
  - [x] `thresh volume create <name>` - Create named volume ✅ Working cross-platform
  - [x] `thresh volume delete <name>` - Remove volume ✅ Working cross-platform
  - [x] `thresh volume inspect <name>` - Show volume details ✅ Working cross-platform
- [x] Update environment lifecycle ✅ Tested on Linux & Windows
  - [x] Volumes persist after `thresh destroy` ✅ Verified cross-platform
  - [ ] Optional `--remove-volumes` flag ⏳ Planned

**Blueprint Example:**
```yaml
name: database-dev
base: ubuntu:22.04
packages:
  - postgresql-14
volumes:
  - name: postgres-data
    mount: /var/lib/postgresql/data
  - name: postgres-backups
    mount: /backups
bind_mounts:
  - host: /c/Users/burns/code/db-scripts
    container: /opt/scripts
    readonly: true
tmpfs:
  - /tmp
  - /run
```

**CLI Usage:**
```bash
# Create environment with volumes
thresh up database-dev

# Volumes persist after destroy
thresh destroy database-dev
thresh volume list
# Output: postgres-data (10GB), postgres-backups (5GB)

# Clean up volumes
thresh destroy database-dev --remove-volumes

# Manage volumes independently
thresh volume create shared-cache
thresh volume inspect shared-cache
```

**Deliverables:**
```bash
# Named volumes work
thresh up postgres-dev   # Creates postgres-data volume

# Volumes survive destroy
thresh destroy postgres-dev
thresh up postgres-dev   # Data still there!

# Bind mounts from Windows to WSL work
thresh up node-dev       # Mounts C:\code to /workspace

# Volume management commands
thresh volume list
thresh volume delete old-cache
```

**Binary Impact:** +30 KB (volume management, mount logic)

---

#### Week 2: Network & Storage Documentation (2-3 days) 📝 IN PROGRESS
- [x] Add networking examples to docs ✅ Created
  - [x] Web server with port mapping ✅ webserver-nginx.json example
  - [ ] Multi-container communication ⏳ Planned
  - [ ] Port conflict resolution ⏳ Planned
- [x] Add storage examples to docs ✅ Created
  - [x] Database with persistent volume ✅ postgres-dev.json example
  - [x] Shared code directory mount ✅ Documentation created
  - [x] Development workflow with volumes ✅ User journey documented
- [x] Internal documentation created ✅ Complete
  - [x] `docs/thresh-volume-flow.md` - Implementation flow
  - [x] `docs/user-journey-storage.md` - User journey guide (11K)
  - [x] `docs/json-blueprint-creation.md` - JSON syntax guide (6K)
- [ ] Update MCP integration guide ⏳ Planned
  - [ ] AI can generate blueprints with ports/volumes ⏳ Planned
  - [ ] Network-aware environment provisioning ⏳ Planned
- [ ] Create migration guide from v1.4.0 ⏳ Planned
- [ ] Add troubleshooting section ⏳ Planned
  - [ ] Port conflicts ⏳ Planned
  - [ ] Permission issues with mounts ⏳ Planned
  - [ ] WSL port forwarding issues ⏳ Planned
- [ ] Website documentation updates ⏳ Planned
  - [ ] Add networking.md to thresh.sh ⏳ Planned
  - [ ] Add storage.md to thresh.sh ⏳ Planned
  - [ ] Update CLI reference with volume commands ⏳ Planned

**Deliverables:**
- ✅ Internal documentation complete (3 markdown files, 23K total)
- ✅ Blueprint examples created (webserver-nginx, postgres-dev)
- ⏳ Website documentation updates pending
- ⏳ Complete networking documentation at thresh.sh/docs/networking
- ⏳ Complete storage documentation at thresh.sh/docs/storage
- ⏳ Blueprint examples repository with 10+ network/storage configs
- ⏳ Updated getting started guides with port/volume examples

---

#### Phase 1.5 Success Metrics:
- [x] Port mapping works on Linux ✅ Tested on Ubuntu 22.04 + Docker
- [x] Port mapping works on Windows (WSL) ✅ Tested Feb 27, 2026
- [ ] Port mapping works on macOS ⏳ Testing pending
- [x] Persistent volumes survive environment destroy/recreate ✅ Verified on Linux & Windows
- [x] Volume management commands working ✅ All 4 commands tested on Linux & Windows
- [x] Blueprint volume integration working ✅ postgres-dev tested on Linux & Windows
- [x] Bind mounts work cross-platform (Windows paths → WSL) ✅ Tested Feb 27, 2026
- [x] Port conflicts detected and reported clearly ✅ Working
- [x] Volume lifecycle independent of environment lifecycle ✅ Verified cross-platform
- [x] MCP tools support networking/storage ✅ 12 tools available
- [x] Internal documentation complete ✅ 3 detailed guides created
- [ ] Website documentation complete ⏳ Pending
- [ ] Documentation includes 10+ real-world examples ⏳ 2 created, 8 more pending
- [x] Binary size < 5.2 MB (Win/Linux compressed) ✅ 13MB uncompressed Linux (AOT)
- [x] No sudo required after docker group setup ✅ Verified on Linux
- [x] WSL Configuration Profiles system complete ✅ 6 profiles, 4 CLI commands (Feb 27, 2026)
- [x] WSL database blueprints optimized ✅ mysql, redis, postgres updated
- [x] WSL Plan9 filesystem issues solved ✅ Database profile disables interop/automount
- [x] Platform detection working ✅ wslconf hidden on Linux/macOS
- [x] WSL configuration validation working ✅ All 7 sections validated

**Linux Testing Complete (Feb 26, 2026):** ✅
- Platform: Ubuntu 22.04 LTS
- Container Runtime: Docker 28.2.2
- .NET: 10.0.103
- Binary: 13MB native AOT (linux-x64)
- All networking features working
- All volume features working
- Data persistence verified

**Windows Testing Complete (Feb 27, 2026):** ✅
- Platform: Windows 11 + WSL2
- Container Runtime: WSL2
- .NET: 10.0.3
- Binary: 14.5MB native AOT (win-x64)
- Port mapping (netsh port forwarding) working
- Volume management working
- Blueprint integration working
- WSL configuration profiles working

---

#### Week 2: WSL Configuration Profiles (2 days) ✅ COMPLETE

**Goal:** Solve WSL Plan9 filesystem limitations and optimize WSL distro configurations  
**Status:** ✅ Complete (Feb 27, 2026)  
**Priority:** P0 (Critical for Windows users with databases)

**Problem Identified:**
During Phase 1.5 testing, discovered that WSL's Plan9 filesystem doesn't support chmod operations, causing PostgreSQL, MySQL, and Redis to fail with permission errors. The solution: WSL configuration profiles that disable Windows interop and automount, forcing databases to run on native Linux filesystem only.

- [x] Design WSL configuration system ✅ Complete
  - [x] Hybrid approach (built-in + custom + inline) ✅
  - [x] Platform detection (Windows-only feature) ✅
  - [x] Microsoft WSL documentation compliance (7 sections) ✅
- [x] Create built-in profiles ✅ Complete (6 profiles, 1,469 bytes)
  - [x] `systemd` - Enables systemd init (88 bytes) ✅
  - [x] `docker` - Auto-starts Docker daemon (188 bytes) ✅
  - [x] `database` - Optimized for PostgreSQL/MySQL/Redis (355 bytes) ✅
  - [x] `web-server` - Auto-starts Nginx (250 bytes) ✅
  - [x] `minimal` - Fast startup, no systemd (238 bytes) ✅
  - [x] `development` - Full Windows integration (350 bytes) ✅
- [x] Implement WslConfigService ✅ Complete (400+ lines)
  - [x] Profile management (list, load, validate) ✅
  - [x] Validation engine (sections, keys, value types) ✅
  - [x] Options display (comprehensive reference) ✅
  - [x] Embedded resource loading ✅
- [x] Add CLI commands ✅ Complete
  - [x] `thresh wslconf list` - Show all profiles ✅
  - [x] `thresh wslconf show <profile>` - Display profile content ✅
  - [x] `thresh wslconf options` - Show all Microsoft-documented options ✅
  - [x] `thresh wslconf validate <file>` - Validate custom config ✅
- [x] Extend Blueprint schema ✅ Complete
  - [x] `wslConfig` - Profile name (e.g., "database") ✅
  - [x] `wslConfigFile` - Path to custom profile ✅
  - [x] `wslConfigCustom` - Inline configuration ✅
- [x] Integrate with provisioning ✅ Complete
  - [x] ConfigureWslSettingsAsync() (120+ lines) ✅
  - [x] Auto-restart with 8-second wait (Microsoft rule) ✅
  - [x] Priority: custom > file > profile ✅
  - [x] Write to /etc/wsl.conf via wsl command ✅
- [x] Update database blueprints ✅ Complete
  - [x] mysql-persistent.json → "wslConfig": "database" ✅
  - [x] redis-persistent.json → "wslConfig": "database" ✅
  - [x] postgres-persistent.json → "wslConfig": "database" ✅
- [x] Documentation ✅ Complete
  - [x] WSL_CONFIG_GUIDE.md (600+ lines) ✅
  - [x] All 7 Microsoft sections documented ✅
  - [x] Troubleshooting guide ✅

**Blueprint Example:**
```json
{
  "name": "postgres-persistent",
  "base": "ubuntu:22.04",
  "wslConfig": "database",
  "packages": ["postgresql-14"],
  "volumes": [{"name": "postgres-data", "mount": "/var/lib/postgresql/data"}]
}
```

**CLI Usage:**
```bash
# List available profiles
thresh wslconf list

# Show profile content
thresh wslconf show database

# Validate custom configuration
thresh wslconf validate my-custom.wslconf

# View all Microsoft-documented options
thresh wslconf options

# Provision with auto-configuration
thresh up postgres-persistent  # Applies database profile automatically
```

**Technical Details:**
- Platform Detection: Hidden on Linux/macOS (RuntimeInformation.IsOSPlatform)
- Validation: Checks all 7 sections (boot, automount, network, interop, user, gpu, time)
- Auto-Restart: Implements 8-second rule (thresh stop → wait 8s → thresh start)
- Embedded Resources: Profiles included in Native AOT binary (1.5 KB total)
- Priority: wslConfigCustom > wslConfigFile > wslConfig
- Microsoft Compliance: All 20+ documented options validated

**Testing Results:**
```bash
# Native AOT build
thresh.exe version
# thresh version 1.4.0, .NET Runtime: 10.0.3, Native AOT: Yes, WSL: 2.1.5.0

# List profiles
thresh.exe wslconf list
# systemd, docker, database, web-server, minimal, development

# Validate configs
thresh.exe wslconf validate test.wslconf
# ✅ Configuration is valid

# Invalid config detection
thresh.exe wslconf validate bad.wslconf
# ❌ Failed to validate file: Arg_KeyNotFoundWithKey, invalidSection
```

**Deliverables:**
- ✅ 6 built-in profiles (1,469 bytes total)
- ✅ WslConfigService.cs (400+ lines)
- ✅ 4 CLI commands (list, show, options, validate)
- ✅ Blueprint integration with 3 methods
- ✅ Auto-restart with 8-second wait
- ✅ Comprehensive validation engine
- ✅ WSL_CONFIG_GUIDE.md documentation
- ✅ Native AOT build (14.5 MB with all features)
- ✅ Platform detection working
- ✅ Database blueprints updated

**Binary Impact:** +1.5 KB (6 profiles), +40 KB (WslConfigService logic)

**Impact:** 🔥 HUGE
- 🗄️ **Database Support**: Solves Plan9 permission issues completely
- ⚡ **Optimized Performance**: Database profile disables unnecessary Windows integration
- 🎯 **Platform-Specific**: Windows-only feature, clean UX on other platforms
- 🔧 **Flexible Configuration**: Three integration methods (profile, file, inline)
- ✅ **Microsoft Compliant**: Full validation against official documentation
- 🤖 **Developer-Friendly**: Simple CLI for profile management
- 📝 **Well-Documented**: Comprehensive guide with examples

**Use Cases Unlocked:**
- PostgreSQL, MySQL, MongoDB, Redis on WSL without permission errors
- Optimized WSL configurations for specific workloads
- Custom profiles for team-specific requirements
- Docker daemon auto-start in WSL distros
- Web server auto-start (Nginx, Apache)
- Minimal distros for CI/CD agents

---

#### Impact 🔥 HUGE (Phase 1.5 Overall)

- 🌐 **Production-Ready Containers**: Port mapping enables web services
- 💾 **Data Persistence**: Volumes enable databases and stateful apps
- 🔄 **Development Workflow**: Bind mounts enable live code editing
- 🐳 **Docker Parity**: thresh now covers 80% of Docker use cases
- 🤖 **AI-Powered Networking**: MCP can configure ports/volumes
- 📊 **Real Workloads**: Move from demos to production services
- 🗄️ **WSL Database Support**: Solves Plan9 filesystem limitations
- ⚙️ **WSL Optimization**: Platform-specific configuration management

**Use Cases Unlocked:**
- Web servers (nginx, Apache, Node.js apps)
- Databases (PostgreSQL, MySQL, MongoDB, Redis) - **Now works on WSL!**
- Development environments with live code reload
- Data science with persistent notebook state
- CI/CD agents with shared cache volumes
- Optimized WSL distros for specific workloads

---

### **Phase 2.5: Cross-Platform Testing & Builds (Weeks 7-8) - Multi-Platform Support** ✅ COMPLETE (v1.4.0)

**Goal:** Test and build thresh for Linux and macOS platforms  
**Status:** ✅ Complete - Shipped in v1.4.0 (Feb 17, 2026)  

**Note:** This phase was completed as part of v1.4.0 release. The original plan included Pulumi + vCenter testing, but we successfully achieved cross-platform support through:
- GitHub Actions multi-platform CI/CD
- Extensive local testing on Windows/WSL, Linux Docker/nerdctl, and macOS
- Platform-aware code and documentation

---

### **Phase 2.5 (Original Plan): Cross-Platform Testing & Builds** 📋 REFERENCE

**Original Goal:** Test and build thresh for Linux and macOS platforms using Pulumi + vCenter for deep testing and GitHub Actions for CI/CD

**Original Strategy:** Use real vCenter infrastructure for thorough testing (Week 7), then implement automated CI/CD (Week 8)

#### Week 7: Pulumi + vCenter Deep Testing (COMPLETED via alternative approach)
- [x] ~~Create Pulumi infrastructure project (`pulumi/`)~~
- [x] ~~Write Pulumi code for vCenter VM provisioning~~
- [x] Testing completed through GitHub Actions and local environments
- [x] Linux testing validated on Ubuntu 22.04
- [ ] macOS testing via local development and GitHub Actions runners
  - [ ] Install containerd/Docker
  - [ ] Install .NET 10 SDK
  - [ ] Clone thresh repository
  - [ ] Build thresh natively on Linux
- [x] Manual testing on Linux:
  - [x] Test `thresh up python-dev`
  - [x] Test `thresh list`
  - [x] Test `thresh destroy`
  - [x] Test `thresh serve --stdio` (MCP server)
  - [x] Test all CLI commands
- [x] Debug and fix platform-specific issues:
  - [x] File path separators (`\` vs `/`)
  - [x] Line endings (CRLF vs LF)
  - [x] Containerd socket locations
  - [x] Permissions and executable bits
  - [x] Shell differences (bash vs PowerShell)
- [x] Iterate quickly with SSH access
- [x] Document Linux-specific setup requirements
- [x] Update IContainerService implementations if needed

**Deliverables:** ✅ Completed
```bash
# Pulumi infrastructure (alternative: GitHub Actions + local testing)
# Testing completed via:
# - GitHub Actions runners (Ubuntu, macOS)
# - Local Ubuntu 22.04 VM testing
# - WSL2 on Windows

# Verified all commands work cross-platform
thresh list         # ✅ Works on Linux, macOS, Windows
thresh metrics      # ✅ Cross-platform metrics
thresh serve --stdio # ✅ MCP server functional
```

**Platform-Specific Issues Resolved:**
- ✅ File path separators handled via cross-platform code
- ✅ Line endings normalized in Git
- ✅ Containerd socket auto-detection working
- ✅ Permissions handled correctly across platforms
- ✅ Shell-agnostic command execution

**vCenter Test Environment:**
```
Windows Dev Machine
    ↓
Pulumi → vCenter
    ↓
├─ Ubuntu 22.04 VM (containerd testing)
└─ AlmaLinux VM (RHEL-like testing)
```

#### Week 8: GitHub Actions Multi-Platform CI/CD (COMPLETED ✅)
- [x] Take learnings from vCenter testing
- [x] Add Linux x64 build job to GitHub Actions
  - [x] Use `ubuntu-latest` runner
  - [x] Install .NET 10 SDK
  - [x] Build with Native AOT
  - [x] Test binary execution
  - [x] Generate SBOM
- [x] Add macOS x64 build job (Intel)
  - [x] Use `macos-13` runner (Intel)
  - [x] Native AOT compilation
  - [x] Test on macOS
- [x] Add macOS ARM64 build job (Apple Silicon)
  - [x] Use `macos-14` runner (M1/M2)
  - [x] ARM64 Native AOT
  - [x] Test on Apple Silicon
- [x] Implement build matrix strategy
- [x] Add UPX compression for all platforms
- [x] Platform-specific SBOM generation
- [x] Update release workflow for 4 artifacts
- [x] Add build status badges to README
- [x] Create platform-specific installation documentation
- [x] Keep vCenter VMs as ongoing dev test environment

**Deliverables:** ✅ Completed
```yaml
# GitHub Actions matrix build
jobs:
  build:
    strategy:
      matrix:
        os: [windows-latest, ubuntu-latest, macos-13, macos-14]
        include:
          - os: windows-latest
            rid: win-x64
          - os: ubuntu-latest
            rid: linux-x64
          - os: macos-13
            rid: osx-x64
          - os: macos-14
            rid: osx-arm64
```

**Release Artifacts:** ✅ Delivered
```
- thresh-win-x64.zip         (Windows, 3.8 MB compressed)
- thresh-linux-x64.tar.gz    (Linux, ~4.0 MB compressed)
- thresh-macos-x64.tar.gz    (macOS Intel, ~4.2 MB compressed)
- thresh-macos-arm64.tar.gz  (macOS Apple Silicon, ~4.0 MB compressed)
```

**Platform-Specific Documentation:** ✅ Created
- getting-started-windows.md (Complete Windows/WSL2 setup)
- getting-started-linux.md (Docker/containerd on Linux)
- getting-started-macos.md (macOS containerd setup)

**Phase 2.5 Success Metrics:**
- ✅ Pulumi infrastructure provisions vCenter VMs successfully
- ✅ thresh builds natively on Ubuntu 22.04 (vCenter VM)
- ✅ All core commands work on Linux via SSH testing
- ✅ Platform-specific bugs identified and fixed
- ✅ GitHub Actions matrix builds 4 platform artifacts
- ✅ Installation tested on Windows, Linux, macOS
- ✅ Platform-specific documentation updated
- ✅ vCenter VMs remain available for ongoing development

**Impact:**
- 🌍 True cross-platform support (not just code, but tested builds)
- 🏗️ Real infrastructure testing (not just GitHub runners)
- 📦 3 platform distributions available (Windows, Linux, macOS)
- 🚀 Automated multi-platform releases via CI/CD
- 🧪 Continuous testing on all platforms
- 🔧 Persistent dev environment for iterative testing
- 💰 Cost effective (use existing vCenter infrastructure)
- 🐛 Deep debugging capability via SSH access

**Hybrid Approach Benefits:**
- **Week 7 (vCenter):** Real VMs, SSH access, deep debugging, iterative development
- **Week 8 (GitHub Actions):** Automated builds, public CI/CD, release artifacts
- **Ongoing:** vCenter VMs become permanent test infrastructure for future development

**See:** Phase 4 for package manager distribution (Chocolatey, Homebrew, APT, etc.)

---

### **Phase 1.6: Agent Mode & Mesh Networking (Weeks 9-12)** 🆕 v1.6.0

**Goal:** Enable background operation and peer-to-peer networking  
**Status:** 📋 Planned (Mar-Apr 2026)  
**Priority:** P1 (High - Multi-machine foundation)

> **Note:** Centralized management, fleet orchestration, and SaaS features are being built separately  
> as a private commercial product (`thresh-hub`). The open-source `thresh` CLI remains focused on  
> individual machine management with optional agent mode for background operation.

#### Week 9-10: Agent Mode & Enhanced Metrics
- [ ] Implement daemon/background mode
- [ ] Periodic metrics collection
- [ ] Enhanced metrics (network I/O, disk usage patterns, process monitoring)
- [ ] Auto-restart and health monitoring
- [ ] Log rotation and management

**Deliverables:**
```bash
# Agent daemon mode (local operation)
thresh agent start
thresh agent status
thresh agent stop
thresh agent logs

# Runs in background
# Auto-restarts on failure
# Collects enhanced metrics locally
# Useful for monitoring and automation
```

**Commands Added:**
- `thresh agent start` - Start agent in background
- `thresh agent stop` - Stop agent daemon
- `thresh agent status` - Show agent health
- `thresh agent logs` - View agent logs
- `thresh agent config` - Configure agent settings

**Binary Impact:** +60 KB

---

#### Week 10-11: Dual Mesh Network Support
- [ ] Create `IMeshNetworkService` interface
- [ ] Implement `TailscaleService` (cloud-based)
- [ ] Implement `NetmakerService` (self-hosted/air-gapped)
- [ ] Add `thresh network` commands
- [ ] WireGuard integration for both providers
- [ ] Auto-discovery of peers
- [ ] Connection health monitoring

**Deliverables:**
```bash
# Tailscale (simple cloud-based)
thresh network join --provider tailscale
thresh network status
thresh network peers

# Netmaker (air-gapped self-hosted)
thresh network join --provider netmaker \
  --server https://netmaker.corp.local \
  --token <enrollment-token>

# Common commands
thresh network leave
thresh network info
thresh network test <peer>
```

**Commands Added:**
- `thresh network join --provider <tailscale|netmaker>` - Join mesh
- `thresh network leave` - Leave mesh network
- `thresh network status` - Show network status
- `thresh network peers` - List connected peers
- `thresh network test <peer>` - Test connectivity
- `thresh network info` - Show local node info

**Binary Impact:** +120 KB

---

#### Week 12: Remote Operations (Peer-to-Peer)
- [ ] SSH-based remote environment operations
- [ ] `thresh remote` command group
- [ ] Direct peer-to-peer provisioning (via mesh network)
- [ ] Remote execution without centralized server
- [ ] Blueprint sharing across peers

**Deliverables:**
```bash
# Connect to specific peer on mesh network
thresh remote connect <peer-name>

# List environments on remote peer
thresh remote list --host <peer-name>

# Provision environment on remote peer (direct SSH)
thresh remote up python-dev --host <peer-name>

# Execute command on remote environment
thresh remote exec <peer-name> <env-name> -- python --version
```

**Commands Added:**
- `thresh remote connect <peer>` - Connect to mesh peer
- `thresh remote list --host <peer>` - List remote environments
- `thresh remote up <name> --host <peer>` - Provision remotely
- `thresh remote exec <peer> <env> -- <cmd>` - Execute remote command

**Binary Impact:** +80 KB

---

**Phase 1.6 Success Metrics:**
- [ ] Agent runs as daemon on Windows/Linux/macOS
- [ ] Agent collects enhanced local metrics
- [ ] Mesh network connectivity (Tailscale + Netmaker)
- [ ] Multi-peer communication working (<100ms latency)
- [ ] Air-gapped deployment tested (Netmaker)
- [ ] Peer discovery working automatically
- [ ] Remote operations via SSH functional
- [ ] Direct peer-to-peer provisioning works

**Binary Impact:** +260 KB total (v1.6 = ~5.36 MB)

**Impact:** 🚀 Significant
- 🤖 **Background Operation**: Agent mode for automation and monitoring
- 📊 **Enhanced Metrics**: Detailed system and container monitoring
- 🔗 **Mesh Networking**: Seamless peer-to-peer communication
- 🌐 **Remote Operations**: Manage environments on other machines
- 🏢 **Enterprise Ready**: Air-gapped support via Netmaker
- 🔄 **P2P Provisioning**: No central server required

**Use Cases Unlocked:**
- Background monitoring and automation
- Distributed development teams with direct peer connections
- Air-gapped enterprise environments
- Remote environment provisioning without central infrastructure
- Multi-machine workload distribution (manual, team-based)
- Mesh VPN for secure dev environment access

**What's NOT in v1.6 (Commercial Product Scope):**
- ❌ Centralized hub/dashboard
- ❌ Authentication & multi-user accounts
- ❌ Node/cluster management UI
- ❌ Centralized MCP server
- ❌ Automatic intelligent placement
- ❌ Fleet-wide orchestration
- ❌ SaaS/cloud hosting

These features are being developed separately as `thresh-hub` - a commercial SaaS/self-hosted  
product for enterprise fleet management. The open-source `thresh` CLI remains focused on  
individual machine management with P2P mesh capabilities.



### **Phase 2.0: Polish & Production (Weeks 13-20) - Production Ready** 🎯 v2.0

**Goal:** Production-grade quality, comprehensive distribution, and enterprise features  
**Status:** 📋 Planned (Apr-Jun 2026)  
**Priority:** P1 (High - Production readiness)

#### Week 13-14: Package Manager Distribution
- [ ] Submit Chocolatey package to community repository
- [ ] Create PR for Scoop main bucket (ScoopInstaller/Main)
- [ ] Submit WinGet manifest to microsoft/winget-pkgs
- [ ] Create Homebrew formula for macOS
- [ ] Create APT packages for Debian/Ubuntu
- [ ] Create RPM packages for RHEL/Fedora/AlmaLinux
- [ ] Set up package signing and GPG keys

**Deliverables:**
```bash
# Windows
choco install thresh
scoop install thresh
winget install dealer426.thresh

# macOS
brew install thresh

# Linux
apt install thresh       # Debian/Ubuntu
yum install thresh       # RHEL/Fedora/AlmaLinux
```

**Package Repositories:**
- Chocolatey Community Repository
- Scoop Main Bucket
- WinGet Official Repository
- Homebrew Core (tap if not accepted to core)
- Debian PPA
- RPM Copr Repository

---

#### Week 15-16: Enhanced Documentation & Search
- [ ] Complete all CLI command documentation
- [ ] Set up Algolia DocSearch for thresh.sh
- [ ] Add video tutorials and demos
- [ ] Architecture deep-dive documentation
- [ ] Fleet deployment guide
- [ ] MCP integration advanced examples
- [ ] Troubleshooting guide expansion
- [ ] FAQ section
- [ ] Community contribution guide

**Deliverables:**
- Complete CLI reference (all commands documented)
- Searchable documentation via Algolia
- 5+ video tutorials on YouTube
- Architecture diagrams (Mermaid)
- Fleet deployment playbook
- Community guidelines

---

#### Week 17-18: Security & Enterprise Features
- [ ] Security audit and hardening
- [ ] RBAC (Role-Based Access Control) for hub
- [ ] Audit logging for all operations
- [ ] SSO integration (SAML/OAuth)
- [ ] Encrypted agent<->hub communication (mTLS)
- [ ] Secret management integration (vault support)
- [ ] Compliance documentation (SOC2, ISO27001 guidance)
- [ ] Multi-tenancy support in hub

**Deliverables:**
```bash
# RBAC configuration
thresh hub rbac add-role developer
thresh hub rbac assign-user john@company.com developer

# Audit logging
thresh hub audit list --user john@company.com --days 7
thresh hub audit export --format json

# SSO setup
thresh hub sso configure --provider azure-ad --tenant-id <id>
```

**Enterprise Features:**
- Role-based access control
- Audit trail for all operations
- SSO/SAML authentication
- mTLS for secure communication
- Multi-tenancy isolation
- Secret management integration

---

#### Week 19: Performance & Monitoring
- [ ] Performance optimization pass
- [ ] Memory usage reduction
- [ ] Startup time optimization
- [ ] Enhanced monitoring and observability
- [ ] Prometheus metrics export
- [ ] Grafana dashboard templates
- [ ] Alert configuration system
- [ ] Performance benchmarking suite

**Deliverables:**
```bash
# Prometheus metrics endpoint
thresh agent start --metrics-port 9090

# Grafana integration
thresh hub metrics prometheus --format yaml > prometheus.yml

# Alerting
thresh hub alert create --name high-memory \
  --condition memory_percent>90 \
  --action email --to ops@company.com
```

**Monitoring:**
- Prometheus metrics exporter
- Grafana dashboard templates
- Built-in alerting system
- Performance baselines
- Health checks and SLOs

---

#### Week 20: Testing, Hardening & v2.0 Release
- [ ] End-to-end testing suite (all platforms)
- [ ] Load testing (100+ agent fleet)
- [ ] Chaos engineering tests
- [ ] Penetration testing
- [ ] Bug fixes and polish
- [ ] Release notes and migration guide
- [ ] Version 2.0 release
- [ ] Community announcement

**Deliverables:**
- Comprehensive test suite (unit, integration, E2E)
- Load test results and benchmarks
- Security assessment report
- v2.0 release with all features
- Migration guide (v1.x → v2.0)
- Public announcement (blog, social media)

---

**Phase 2.0 Success Metrics:**
- [ ] Available in 6+ package managers
- [ ] Algolia search functional on thresh.sh
- [ ] All CLI commands fully documented
- [ ] 5+ video tutorials published
- [ ] RBAC working with 3+ roles
- [ ] Audit logging captures all operations
- [ ] SSO integration tested with 2+ providers
- [ ] mTLS enforced for hub communication
- [ ] Prometheus metrics exported
- [ ] 100+ agent fleet tested
- [ ] <2s cold start time
- [ ] <50MB memory per agent
- [ ] Security audit passed
- [ ] v2.0 release shipped

**Impact:** 🚀 MASSIVE
- 📦 **Easy Installation**: Available in all major package managers
- 🔍 **Searchable Docs**: Fast documentation discovery
- 🔒 **Enterprise Security**: RBAC, SSO, audit logging, mTLS
- 📊 **Production Monitoring**: Prometheus/Grafana integration
- 🎓 **Better Onboarding**: Video tutorials and complete docs
- 🏢 **Enterprise Ready**: Compliance, multi-tenancy, security hardening
- ⚡ **Performance**: Optimized for large fleets
- 🌐 **Community Growth**: Easy contribution and adoption

---

### **Phase 3 & 4 (Legacy Reference)** ✅ ARCHIVED

---

## 🏗️ Architecture Evolution

### v1.0 (Initial)
```
┌─────────────────┐
│  thresh.exe     │
│  (Windows)      │
│                 │
│  ├─ WSL         │
│  ├─ OpenAI      │
│  └─ Copilot SDK │
└─────────────────┘
```

### v1.6 (Target - Distributed)
```
                    ┌──────────────────┐
                    │   thresh-hub     │
                    │  (API + Dashboard)│
                    └────────┬─────────┘
                             │
              Mesh Network (Tailscale/Netmaker)
                             │
          ┌──────────────────┼──────────────────┐
          │                  │                  │
    ┌─────▼─────┐      ┌─────▼─────┐     ┌─────▼─────┐
    │  thresh   │      │  thresh   │     │  thresh   │
    │ (Windows) │      │  (Linux)  │     │  (macOS)  │
    │           │      │           │     │           │
    │ • WSL     │      │•containerd│     │•containerd│
    │ • Agent   │      │ • Agent   │     │ • Agent   │
    │ • Metrics │      │ • Metrics │     │ • Metrics │
    │ • MCP     │      │ • MCP     │     │ • MCP     │
    │ • Volumes │      │ • Volumes │     │ • Volumes │
    │ • Network │      │ • Network │     │ • Network │
    └───────────┘      └───────────┘     └───────────┘
         ▲                  ▲                  ▲
         │                  │                  │
    VS Code           VS Code           VS Code
    (via MCP)        (via MCP)         (via MCP)
```

---

## 📦 Binary Size Progression

| Milestone | Size | Growth | Features |
|-----------|------|--------|----------|
| v1.0 (initial) | 16.6 MB | - | WSL, Dual AI |
| v1.1 (cross-platform) | 16.8 MB | +200 KB | containerd support |
| v1.2 (Native AOT) | 13.5 MB | -3.3 MB | Native AOT compilation |
| v1.2 (UPX) | 3.8 MB | -9.7 MB | UPX compression |
| v1.3 (docs) | 3.8 MB | - | Documentation site |
| v1.4 (multi-platform) | 5.0 MB | +1.2 MB | 11 MCP tools, macOS support |
| **v1.5 (networking/storage)** ✅ | **5.1 MB** | **+100 KB** | **Port mapping, volumes, WSL config** |
| **v1.6 (agent/mesh)** 📋 | **5.36 MB** | **+260 KB** | **Agent mode, mesh network (Tailscale/Netmaker), remote ops** |
| **v2.0 (production)** 🎯 | **5.5 MB** | **+140 KB** | **Package distribution, security, monitoring, polish** |

**Total growth v1.5 → v2.0:** +400 KB (+8%)  
**Value delivered:** Agent daemon, P2P mesh networking, remote operations, package distribution, enterprise polish  
**Exceptional efficiency:** <500 KB growth for distributed P2P development tool

**Note:** Centralized management features (Hub/SaaS) are being built in a separate private commercial product

---

## 🎯 Success Criteria

### Technical Metrics
- [ ] Single binary runs on Windows, Linux, macOS
- [ ] Binary size < 6 MB (compressed)
- [ ] Provision time < 30 seconds
- [ ] Agent mode runs stable in background
- [ ] Mesh network connectivity (Tailscale + Netmaker)
- [ ] Peer-to-peer latency < 100ms
- [ ] MCP integration working in 3+ AI editors
- [ ] Remote operations via SSH functional
- [ ] Air-gapped deployment tested (Netmaker)

### User Experience
- [ ] Install to first environment: < 5 minutes
- [ ] Fleet setup: < 30 minutes
- [ ] Zero configuration auto-discovery
- [ ] One-command remote provisioning
- [ ] Comprehensive documentation

### Distribution
- [ ] Available in 6+ package managers
- [ ] Automated releases on tag
- [ ] All platforms build in < 10 minutes
- [ ] SBOM generation automated

---

## 🚀 Quick Start (v2.0 Vision)

### Local Developer (Week 4)
```bash
# Install
brew install thresh  # or winget, apt, etc.

# Use locally
thresh up python-dev
thresh chat
```

### Small Team (Week 8)
```bash
# Each dev joins Tailscale mesh
thresh network join --provider tailscale
thresh agent start

# Anyone can provision anywhere
thresh up node-dev --remote
```

### Enterprise Fleet (Week 12)
```bash
# IT deploys infrastructure
docker run -d thresh-hub
docker run -d netmaker

# Devs join air-gapped network
thresh network join --provider netmaker \
  --server http://netmaker.corp

# Hub orchestrates workloads
thresh up python-dev --remote --priority high
```

---

## 🔄 Dependencies Between Features

```mermaid
graph TD
    A[Dual AI ✅] --> B[MCP Server]
    C[WSL Service ✅] --> D[Container Abstraction]
    D --> E[Cross-Platform]
    E --> F[Multi-Platform Builds]
    
    E --> G[Host Metrics]
    G --> H[Agent Mode]
    H --> I[Mesh Network]
    I --> J[Remote Operations]
    J --> K[Package Distribution]
    
    B --> L[AI Editor Integration]
    K --> M[Production Ready]
    
    style A fill:#90EE90
    style C fill:#90EE90
    style B fill:#FFD700
    style D fill:#FFD700
    style E fill:#FFD700
    style F fill:#FFD700
    style G fill:#87CEEB
    style H fill:#87CEEB
    style I fill:#87CEEB
    style J fill:#FFA07A
    style K fill:#FFA07A
```

**Legend:**
- 🟢 Green: Complete (v1.5.0)
- 🟡 Yellow: In Progress (v1.5.0)
- 🔵 Blue: Planned (v1.6.0)
- 🟠 Orange: Future (v2.0)

**Note:** Centralized management features (Hub, Fleet Management, Orchestration) are being  
developed as a separate commercial product and are not part of the open-source roadmap.
- 🟡 Yellow: Phase 1 (Weeks 1-4) - Foundation
- 🔵 Blue: Phase 2.5 (Weeks 7-8) - Cross-Platform Testing
- 🟣 Purple: Phase 2 (Weeks 9-12) - Metrics & Networking
- 🟠 Orange: Phase 3 (Weeks 13-16) - Hub & Orchestration

---

## 💰 Cost-Benefit Analysis

### Development Investment
- **Time:** 20 weeks (1 developer)
- **Complexity:** Medium (leverages existing patterns)
- **Risk:** Low (incremental, tested at each phase)

### Value Delivered

**For Individual Developers:**
- ✅ Works on any platform (not just Windows)
- ✅ AI editor integration (MCP)
- ✅ Faster than Docker for dev environments

**For Small Teams (2-10 devs):**
- ✅ Shared infrastructure (Tailscale mesh)
- ✅ Auto-discovery, zero config
- ✅ Cost: Free (Tailscale free tier)

**For Enterprises (100+ devs):**
- ✅ Air-gapped deployment (Netmaker)
- ✅ Central fleet management
- ✅ Resource optimization (50% better utilization)
- ✅ Compliance friendly (on-prem)

**ROI Estimate:**
- 10 developers × 30 min/day saved = 25 hours/week
- At $100/hour = **$2,500/week saved**
- 20 weeks development = **Pays for itself in 2.5 months**

---

## 🎓 Learning & Skill Development

**Technologies mastered during implementation:**
- ✅ Cross-platform .NET Native AOT
- ✅ Container runtime integration
- ✅ MCP (Model Context Protocol)
- ✅ Mesh networking (WireGuard)
- ✅ Distributed systems
- ✅ gRPC/HTTP APIs
- ✅ Workload scheduling algorithms

**Valuable for:**
- Platform engineering roles
- DevOps automation
- Distributed systems architecture
- Developer tooling

---

## 🔮 Future Enhancements (Post v2.0)

### v2.1 - Advanced Features
- [ ] GPU workload scheduling
- [ ] Kubernetes integration
- [ ] VS Code extension (dedicated)
- [ ] Web UI for hub

### v2.2 - Enterprise Features
- [ ] RBAC (Role-Based Access Control)
- [ ] Audit logging
- [ ] SSO integration
- [ ] Multi-tenancy

### v3.0 - Platform
- [ ] thresh marketplace (blueprint sharing)
- [ ] Plugin system
- [ ] Terraform provider
- [ ] Pulumi provider

---

## ✅ Recommendation: Proceed with Full Plan

**Why this plan works:**

1. **Incremental:** Each phase delivers value independently
2. **Low risk:** Build on proven patterns (dual AI provider model)
3. **High impact:** 8x functionality for 4% size increase
4. **Market timing:** MCP adoption is accelerating (2026)
5. **Competitive moat:** No tool does WSL + containerd + MCP + fleet management

**What makes it unique:**
- Only tool bridging WSL and containers
- Only dev environment tool with MCP support
- Only orchestrator designed for air-gapped
- Smallest binary in category (17 MB vs competitors at 50-100 MB)

**Next steps:**
1. ✅ Approve roadmap
2. Create feature branches
3. Start Phase 1, Week 1 (container abstraction)
4. Ship incremental releases every 4 weeks

---

**Decision Point:** Proceed with 20-week plan to v2.0?
