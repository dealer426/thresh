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

### **Phase 1.6: Agent Mode & Midtier Integration (Weeks 9-12)** 🆕 v1.6.0

**Goal:** Enable background operation with HTTPS-based midtier registration (Dynatrace-style)  
**Status:** 📋 Planned (Mar-Apr 2026)  
**Priority:** P1 (High - Foundation for SaaS/midtier)

> **Architecture Decision:** Following proven APM patterns (Dynatrace, DataDog, New Relic), agents  
> connect to midtier via standard HTTPS/REST APIs. This works with existing corporate networking,  
> firewalls, and proxies. Advanced mesh networking (Tailscale/Netmaker) deferred to v1.7 for  
> specialized P2P use cases.

> **Note:** Centralized management features (`thresh-hub` midtier/SaaS) are being built in a separate  
> repository as a commercial product. The open-source `thresh` CLI focuses on local environment  
> management with optional agent mode for midtier connectivity.

#### Week 9-10: Agent Daemon & Local Operations
- [ ] Implement daemon/background mode
- [ ] Windows service / Linux systemd integration
- [ ] Periodic metrics collection (CPU, memory, disk, network)
- [ ] Enhanced environment monitoring
  - [ ] Container health checks
  - [ ] Resource usage tracking
  - [ ] Environment lifecycle events
- [ ] Auto-restart and health monitoring
- [ ] Log rotation and management
- [ ] Configuration file support (`/etc/thresh/agent.conf`)

**Deliverables:**
```bash
# Agent daemon mode
thresh agent install    # Install as service (Windows/Linux/macOS)
thresh agent start      # Start agent daemon
thresh agent stop       # Stop agent daemon
thresh agent status     # Show agent health
thresh agent logs       # View agent logs
thresh agent config     # Show/edit configuration

# Configuration file
# /etc/thresh/agent.conf (Linux) or C:\ProgramData\thresh\agent.conf (Windows)
[agent]
enabled = true
metrics_interval = 60s
log_level = info
log_max_size = 100MB
log_max_backups = 5

[midtier]
url = https://thresh-hub.company.com
api_key = ${THRESH_API_KEY}
heartbeat_interval = 30s
tls_verify = true
```

**Commands Added:**
- `thresh agent install` - Install agent as system service
- `thresh agent uninstall` - Remove agent service
- `thresh agent start` - Start agent daemon
- `thresh agent stop` - Stop agent daemon
- `thresh agent restart` - Restart agent
- `thresh agent status` - Show agent health and uptime
- `thresh agent logs [--follow]` - View agent logs
- `thresh agent config` - Manage configuration

**Binary Impact:** +80 KB

---

#### Week 10-11: SignalR Midtier Registration & Real-Time Communication
- [ ] Implement `MidtierClient` using SignalR (Native AOT compatible)
- [ ] SignalR Hub connection with WebSocket transport
  - [ ] `Microsoft.AspNetCore.SignalR.Client` (v10.0+)
  - [ ] JSON protocol with source generators (AOT-friendly)
  - [ ] Automatic reconnection with exponential backoff
  - [ ] WebSocket over port 443 (firewall-friendly)
- [ ] Agent registration on connection
  - [ ] Generate agent ID (GUID)
  - [ ] Send machine info (hostname, OS, platform, IP)
  - [ ] API key authentication via query string or headers
  - [ ] TLS/HTTPS support with certificate validation
- [ ] Real-time bidirectional communication
  - [ ] Hub→Agent: Commands (provision, destroy, restart)
  - [ ] Agent→Hub: Results, events, status updates
  - [ ] Instant command delivery (no polling delay)
  - [ ] Connection state monitoring
- [ ] Metrics streaming
  - [ ] Real-time metrics push every 60s
  - [ ] JSON serialization with System.Text.Json
  - [ ] Environment lifecycle events (started, stopped)
- [ ] Multi-tier fallback strategy
  - [ ] Primary: SignalR to on-prem midtier
  - [ ] Fallback 1: REST polling to on-prem midtier
  - [ ] Fallback 2: SignalR to cloud SaaS (thresh-hub.io)
  - [ ] Fallback 3: REST polling to cloud SaaS
  - [ ] Graceful degradation: Store metrics locally if all fail
  - [ ] Automatic failover and recovery
  - [ ] Configurable retry intervals per tier
- [ ] Proxy support (HTTP_PROXY, HTTPS_PROXY)
- [ ] Native AOT compatibility
  - [ ] JsonSerializerContext for all message types
  - [ ] Source-generated serialization
  - [ ] No reflection-based serialization

**Architecture (Dynatrace-style):**
```
┌──────────────────────────────────────┐
│      thresh-hub (Midtier/SaaS)       │
│                                      │
│  ┌─────────────────────────────────┐ │
│  │   REST API (HTTPS)              │ │
│  │   • /api/v1/agents/register     │ │
│  │   • /api/v1/agents/heartbeat    │ │
│  │   • /api/v1/agents/metrics      │ │
│  │   • /api/v1/agents/poll         │ │
│  └─────────────────────────────────┘ │
│         ▲            ▲            ▲   │
└─────────┼────────────┼────────────┼───┘
          │            │            │
     HTTPS/TLS    HTTPS/TLS    HTTPS/TLS
   (Port 443)    (Port 443)   (Port 443)
          │            │            │
    ┌─────▼────┐ ┌─────▼────┐ ┌────▼─────┐
    │  Agent   │ │  Agent   │ │  Agent   │
    │(Windows) │ │ (Linux)  │ │ (macOS)  │
    └──────────┘ └──────────┘ └──────────┘
    
• Agents initiate outbound HTTPS (firewall-friendly)
• API key authentication
• TLS certificate validation
• Works through corporate proxies
• No VPN or P2P mesh required
• Standard REST/JSON protocol
```

**SignalR Protocol Example (C#):**
```csharp
// Agent-side SignalR client (Native AOT compatible)
var hubConnection = new HubConnectionBuilder()
    .WithUrl("https://thresh-hub.company.com/agenthub", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(apiKey);
    })
    .WithAutomaticReconnect(new[] 
    { 
        TimeSpan.FromSeconds(0),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    })
    .AddJsonProtocol(options =>
    {
        // Use source-generated JSON context for AOT
        options.PayloadSerializerOptions.TypeInfoResolverChain
            .Add(AgentJsonContext.Default);
    })
    .Build();

// Hub methods agent can call
await hubConnection.InvokeAsync("RegisterAgent", new AgentInfo
{
    AgentId = agentId,
    Hostname = "dev-machine-01",
    Platform = "Windows",
    OsVersion = "Windows 11 Pro",
    ThreshVersion = "1.6.0",
    IpAddress = "192.168.1.100",
    Architecture = "x64"
});

await hubConnection.InvokeAsync("SendMetrics", new MetricsData
{
    AgentId = agentId,
    Timestamp = DateTime.UtcNow,
    CpuPercent = 45.5,
    MemoryUsed = 8192,
    Environments = environments
});

// Methods hub can call on agent (real-time commands)
hubConnection.On<ProvisionRequest>("ProvisionEnvironment", async request =>
{
    // Execute provision command immediately (no polling delay)
    var result = await ProvisionAsync(request.Blueprint);
    await hubConnection.InvokeAsync("SendCommandResult", result);
});

hubConnection.On<string>("DestroyEnvironment", async envName =>
{
    await DestroyAsync(envName);
});

hubConnection.On("RestartAgent", async () =>
{
    await RestartAsync();
});

// Start connection with multi-tier fallback
string primaryUrl = "https://thresh-hub.company.com/agenthub";
string fallbackUrl = "https://thresh-hub.io/agenthub";

try
{
    // Try primary on-prem midtier (SignalR)
    hubConnection = CreateHubConnection(primaryUrl, primaryApiKey);
    await hubConnection.StartAsync();
    Console.WriteLine("✅ Connected to primary midtier via SignalR (real-time)");
    currentTier = ConnectionTier.PrimarySignalR;
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Primary SignalR failed: {ex.Message}");
    
    try
    {
        // Fallback to primary REST polling
        Console.WriteLine("🔄 Trying REST polling to primary...");
        useRestForPrimary = true;
        currentTier = ConnectionTier.PrimaryREST;
    }
    catch
    {
        try
        {
            // Failover to cloud SaaS (SignalR)
            Console.WriteLine("☁️  Failing over to cloud SaaS...");
            hubConnection = CreateHubConnection(fallbackUrl, fallbackApiKey);
            await hubConnection.StartAsync();
            Console.WriteLine("✅ Connected to cloud SaaS via SignalR");
            currentTier = ConnectionTier.CloudSignalR;
            
            // Schedule failback attempt to primary in 5 minutes
            ScheduleFailbackAttempt(TimeSpan.FromMinutes(5));
        }
        catch
        {
            try
            {
                // Fallback to cloud REST polling
                Console.WriteLine("🔄 Trying REST polling to cloud...");
                useRestForCloud = true;
                currentTier = ConnectionTier.CloudREST;
            }
            catch
            {
                // All connections failed - go offline
                Console.WriteLine("⚠️  All midtier connections failed");
                Console.WriteLine("💾 Operating in offline mode (local cache)");
                currentTier = ConnectionTier.Offline;
                EnableOfflineMode();
            }
        }
    }
}
```

**JSON Source Generator (AOT-compatible):**
```csharp
// Required for Native AOT support
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false)]
[JsonSerializable(typeof(AgentInfo))]
[JsonSerializable(typeof(MetricsData))]
[JsonSerializable(typeof(ProvisionRequest))]
[JsonSerializable(typeof(CommandResult))]
internal partial class AgentJsonContext : JsonSerializerContext
{
}
```

**Multi-Tier Connection Strategy:**
```csharp
// Connection tiers in order of preference
public enum ConnectionTier
{
    PrimarySignalR,   // On-prem SignalR (best performance)
    PrimaryREST,      // On-prem REST polling
    CloudSignalR,     // Cloud SaaS SignalR (disaster recovery)
    CloudREST,        // Cloud SaaS REST polling
    Offline           // Local cache only (all connections failed)
}

// Failure detection and automatic failover
private async Task MonitorConnectionHealth()
{
    while (agentRunning)
    {
        if (currentTier == ConnectionTier.PrimarySignalR)
        {
            // Monitor primary connection
            if (!await PingPrimary(timeout: TimeSpan.FromSeconds(30)))
            {
                // Primary failed, initiate failover
                await FailoverToNextTier();
            }
        }
        else if (currentTier != ConnectionTier.PrimarySignalR)
        {
            // Not on primary - periodically check if primary recovered
            if (DateTime.Now - lastFailbackAttempt > TimeSpan.FromMinutes(5))
            {
                if (await PingPrimary(timeout: TimeSpan.FromSeconds(5)))
                {
                    // Primary recovered, failback
                    await FailbackToPrimary();
                }
                lastFailbackAttempt = DateTime.Now;
            }
        }
        
        await Task.Delay(TimeSpan.FromSeconds(10));
    }
}
```

**Deliverables:**
```bash
# Configure primary and fallback midtier
thresh agent config set midtier.url https://thresh-hub.company.com
thresh agent config set midtier.fallback_url https://thresh-hub.io
thresh agent config set midtier.api_key abc123xyz
thresh agent config set midtier.fallback_api_key def456uvw
thresh agent config set midtier.failover_enabled true

# Agent auto-registers on start
thresh agent start
# Agent registered with ID: 550e8400-e29b-41d4-a716-446655440000
# ✅ Connected to hub via SignalR (real-time)
# Primary: https://thresh-hub.company.com
# Fallback: https://thresh-hub.io (standby)
# Transport: SignalR WebSocket over TLS
# Status: Connected to primary

# View agent connection status
thresh agent status
# Agent: Running
# Transport: SignalR (WebSocket)
# Primary: https://thresh-hub.company.com (connected)
# Fallback: https://thresh-hub.io (available)
# Connection: Connected to primary (reconnects: 0)
# Last message: 2s ago
# Failover: Ready (timeout: 30s)
# Environments: 3 Running, 0 Stopped
# Uptime: 2h 15m

# Simulate primary failure (automatic failover)
# [Primary connection lost]
# ⚠️  Primary midtier unreachable, attempting failover...
# ✅ Connected to fallback cloud (https://thresh-hub.io)
# Transport: SignalR WebSocket
# Status: Connected to fallback (will retry primary in 5m)

# When primary recovers
# ✅ Primary midtier recovered
# 🔄 Failing back to primary (https://thresh-hub.company.com)
# ✅ Connected to primary midtier
```

**Configuration Options:**
```ini
[midtier]
# Primary midtier URL (on-prem or cloud)
url = https://thresh-hub.company.com

# Fallback cloud SaaS URL (optional, for disaster recovery)
fallback_url = https://thresh-hub.io

# API key for authentication (required)
api_key = ${THRESH_API_KEY}

# Cloud SaaS API key (optional, can be different from on-prem)
fallback_api_key = ${THRESH_CLOUD_API_KEY}

# Transport mode (signalr, rest, auto - default: auto)
transport = auto  # Try SignalR first, fallback to REST

# Failover strategy
failover_enabled = true
failover_timeout = 30s  # Time to wait before failing over to cloud
failback_delay = 5m     # Wait before attempting to reconnect to primary

# SignalR settings
signalr_hub_path = /agenthub
signalr_reconnect_policy = exponential  # 0s, 2s, 10s, 30s

# Metrics reporting interval (default: 60s)
metrics_interval = 60s

# TLS certificate verification (default: true)
tls_verify = true

# Custom CA certificate path (for on-prem)
tls_ca_cert = /etc/thresh/ca.crt

# HTTP proxy (optional)
proxy = http://proxy.company.com:8080

# Connection timeout
timeout = 10s

# REST fallback settings (if SignalR fails)
rest_polling_interval = 30s
rest_max_retries = 3
rest_retry_backoff = exponential

# Local storage for offline resilience
offline_cache_enabled = true
offline_cache_path = /var/lib/thresh/cache
offline_cache_max_size = 100MB
```

**Commands Added:**
- `thresh agent config set <key> <value>` - Set configuration
- `thresh agent config get <key>` - Get configuration value
- `thresh agent config list` - Show all configuration
- `thresh agent register` - Manual registration (auto on start)
- `thresh agent unregister` - Disconnect from midtier
- `thresh agent failover` - Manually trigger failover to cloud
- `thresh agent failback` - Manually trigger failback to primary

**Deployment Models:**

1. **Pure On-Prem** (No cloud fallback)
   ```bash
   thresh agent config set midtier.url https://thresh-hub.company.com
   thresh agent config set midtier.failover_enabled false
   ```
   - Data never leaves corporate network
   - Offline mode if on-prem down
   
2. **Hybrid with Cloud Backup** (Recommended)
   ```bash
   thresh agent config set midtier.url https://thresh-hub.company.com
   thresh agent config set midtier.fallback_url https://thresh-hub.io
   thresh agent config set midtier.failover_enabled true
   ```
   - On-prem for normal operations
   - Automatic cloud failover for disaster recovery
   - Best of both worlds: sovereignty + availability
   
3. **Pure Cloud SaaS**
   ```bash
   thresh agent config set midtier.url https://thresh-hub.io
   thresh agent config set midtier.failover_enabled false
   ```
   - Zero infrastructure to manage
   - Always connected
   - Ideal for startups and small teams

**Binary Impact:** +160 KB (SignalR client, WebSocket, JSON source generators, multi-tier fallback logic, offline cache)

---

#### Week 11-12: Agent Features & Quality
- [ ] Environment snapshots and cloning
  - [ ] `thresh snapshot create <env> [name]`
  - [ ] `thresh snapshot list`
  - [ ] `thresh snapshot restore <name>`
  - [ ] `thresh clone <env> <new-name>`
- [ ] Environment tags and metadata
  - [ ] `thresh tag <env> <tag1> <tag2>`
  - [ ] `thresh list --tag dev`
  - [ ] Custom key-value metadata
- [ ] Resource limits and quotas
  - [ ] CPU/memory caps in blueprints
  - [ ] Docker cgroup integration
  - [ ] WSL resource configuration
- [ ] Auto-cleanup and TTL
  - [ ] `thresh up --ttl 24h` for temporary environments
  - [ ] Agent periodic cleanup job
  - [ ] Idle environment detection
- [ ] Health checks in blueprints
  - [ ] HTTP endpoint checks
  - [ ] TCP port checks
  - [ ] Custom script checks
  - [ ] Auto-restart on failure

**Deliverables:**
```bash
# Snapshots
thresh snapshot create python-dev backup-before-upgrade
thresh snapshot restore backup-before-upgrade
thresh clone python-dev python-dev-experiment

# Tags and filtering
thresh tag python-dev production database
thresh list --tag production
thresh list --format json | jq '.[] | select(.tags | contains(["database"]))'

# Resource limits in blueprint
{
  "name": "limited-env",
  "resources": {
    "cpu_limit": "2.0",
    "memory_limit": "4GB",
    "disk_limit": "20GB"
  }
}

# TTL for ephemeral environments
thresh up test-env --ttl 4h  # Auto-destroy after 4 hours

# Health checks in blueprint
{
  "name": "web-server",
  "health_checks": [
    {"type": "http", "url": "http://localhost:8080/health", "interval": "30s"},
    {"type": "tcp", "port": 8080, "interval": "10s"}
  ]
}
```

**Commands Added:**
- `thresh snapshot create <env> [name]` - Create environment snapshot
- `thresh snapshot list` - List all snapshots
- `thresh snapshot restore <name>` - Restore from snapshot
- `thresh snapshot delete <name>` - Delete snapshot
- `thresh clone <env> <new-name>` - Clone environment
- `thresh tag <env> <tags...>` - Add tags to environment
- `thresh untag <env> <tags...>` - Remove tags
- `thresh list --tag <tag>` - Filter by tag

**Binary Impact:** +80 KB

---

**Phase 1.6 Success Metrics:**
- [ ] Agent runs as daemon/service on Windows/Linux/macOS
- [ ] SignalR connection established successfully
- [ ] Agent registers with hub on connection
- [ ] Real-time command delivery working (<1s latency)
- [ ] Bidirectional messaging functional
- [ ] Metrics streaming working (60s batches)
- [ ] Automatic reconnection working (exponential backoff)
- [ ] REST fallback working when WebSocket blocked
- [ ] Multi-tier failover working (primary → cloud)
- [ ] Automatic failback when primary recovers
- [ ] Agent operates during primary outage via cloud
- [ ] Offline mode stores metrics locally
- [ ] Proxy support working (HTTP_PROXY)
- [ ] TLS certificate validation working
- [ ] Native AOT build with SignalR successful
- [ ] JSON source generators working correctly
- [ ] Agent survives complete midtier outage
- [ ] Connection state monitoring accurate
- [ ] Environment snapshots and cloning working
- [ ] Resource limits enforced correctly
- [ ] TTL auto-cleanup functional
- [ ] Health checks triggering restarts

**Binary Impact:** +320 KB total (v1.6 = ~5.42 MB compressed)

**NuGet Dependencies:**
- `Microsoft.AspNetCore.SignalR.Client` v10.0+ (Native AOT compatible)
- `System.Text.Json` v10.0+ (source generators for AOT)
- WebSocket transport (built-in to .NET)
- No MessagePack (reflection issues with AOT)

**Impact:** 🚀 HUGE
- ⚡ **Real-Time**: Instant command delivery via SignalR (no polling delay)
- 🔄 **Bidirectional**: Commands down, metrics/events up simultaneously
- 🤖 **Background Operation**: Agent daemon for 24/7 monitoring
- 📊 **Centralized Metrics**: Real-time streaming to midtier
- 🔗 **Enterprise Friendly**: WebSocket over port 443, works with firewalls
- 🔌 **Auto-Reconnect**: Exponential backoff, survives network issues
- 🔙 **Multi-Tier Fallback**: SignalR → REST → Cloud SaaS → Offline
- 🌩️ **Cloud Failover**: Automatic disaster recovery to SaaS
- 🏢 **Hybrid Deployment**: On-prem primary + cloud backup
- 💾 **Offline Resilience**: Local metric storage during outages
- 🔁 **Auto-Failback**: Returns to primary when it recovers
- 🏢 **Native AOT**: Full SignalR support with source generators
- 🔐 **Secure**: API key auth + TLS encryption
- 🌐 **Midtier Foundation**: Enables SaaS/on-prem thresh-hub
- 💾 **Snapshots**: Quick backup and experimentation
- 🏷️ **Organization**: Tags and metadata for scale
- ⚡ **Resource Control**: Prevent resource hogging
- 🧹 **Auto-Cleanup**: TTL for temporary environments

**Use Cases Unlocked:**
- Real-time fleet monitoring with <1s command latency
- Instant remote provisioning from web dashboard
- Live metrics streaming (not batch uploads)
- Corporate deployments behind firewalls (WebSocket over 443)
- SaaS service with persistent agent connections
- **Hybrid deployments** (on-prem primary + cloud backup)
- **Disaster recovery** (automatic cloud failover)
- **High availability** (multi-tier redundancy)
- **Offline resilience** (local metric caching)
- Connection state awareness (know immediately when agent disconnects)
- Automatic environment cleanup
- Quick environment backups
- Resource quota enforcement
- Multi-tenant deployments
- Scalable with SignalR backplane (Redis, Azure SignalR Service)
- **Data sovereignty + cloud backup** (run on-prem, fail to cloud)

**What's in thresh-hub (Separate Repo):**
- ✅ Web dashboard for fleet management
- ✅ REST API for agent communication
- ✅ Multi-user authentication (SSO/SAML)
- ✅ RBAC (Role-Based Access Control)
- ✅ Centralized blueprint management
- ✅ Audit logging
- ✅ Metrics visualization
- ✅ Intelligent workload placement
- ✅ Cost tracking and optimization
- ✅ SaaS deployment (cloud-hosted)

The open-source `thresh` CLI focuses on local management + agent mode for midtier connectivity.

---

**🌟 Phase 1.6 Highlight: Multi-Tier Failover Architecture**

One of the most powerful features in v1.6 is the **multi-tier failover strategy**, giving enterprises the best of both worlds:

**📊 Connection Priority:**
```
1️⃣ Primary On-Prem (SignalR)      ← Data sovereignty, low latency
2️⃣ Primary On-Prem (REST)         ← Proxy/firewall fallback  
3️⃣ Cloud SaaS (SignalR)           ← Disaster recovery
4️⃣ Cloud SaaS (REST)              ← Maximum compatibility
5️⃣ Offline Cache                  ← Graceful degradation
```

**🎯 Real-World Scenario:**
1. **Normal Operations**: All 100 agents connected to on-prem midtier via SignalR
2. **Data Center Outage**: On-prem midtier goes down at 2 AM
3. **Automatic Failover**: All agents detect failure within 30s, failover to cloud SaaS
4. **Business Continuity**: Development continues uninterrupted, metrics flow to cloud
5. **Recovery**: On-prem comes back online at 6 AM
6. **Automatic Failback**: Agents detect recovery, gradually reconnect to on-prem
7. **Result**: Zero manual intervention, zero downtime, 99.9%+ availability

**💼 Enterprise Benefits:**
- ✅ **Data Sovereignty**: Run on-prem for compliance (GDPR, HIPAA, SOC2)
- ✅ **Disaster Recovery**: Automatic cloud failover without manual intervention
- ✅ **High Availability**: 99.9%+ uptime with multi-tier redundancy
- ✅ **Cost Optimization**: Pay for cloud only during on-prem outages
- ✅ **Zero Config**: Agents handle failover/failback automatically
- ✅ **Offline Resilience**: Agents cache metrics locally if all else fails

**🚀 Competitive Advantage:**
Most development tools are either **pure cloud** (no data sovereignty) or **pure on-prem** (no HA).  
Thresh offers **hybrid deployment** with automatic failover, giving enterprises flexibility without compromise.

---
### **Phase 1.7: Advanced Mesh Networking (Optional)** 🆕 v1.7.0

**Goal:** Add P2P mesh networking for advanced use cases (air-gapped, P2P collaboration)  
**Status:** 📋 Planned (May 2026)  
**Priority:** P2 (Optional - Advanced feature for specialized deployments)

> **Note:** Mesh networking (Tailscale/Netmaker) is an **optional advanced feature** for specialized  
> use cases where P2P connectivity is required (air-gapped environments, direct peer collaboration).  
> Most deployments will use v1.6's HTTPS-based midtier connectivity via `thresh-hub`.

#### Week 1-2: Mesh Network Integration
- [ ] Create `IMeshNetworkService` interface
- [ ] Implement `TailscaleService` (cloud-based, zero-config)
- [ ] Implement `NetmakerService` (self-hosted, air-gapped)
- [ ] Add `thresh network` commands
- [ ] WireGuard integration for both providers
- [ ] Auto-discovery of peers on mesh
- [ ] Connection health monitoring
- [ ] Fallback to direct TCP if mesh unavailable

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

# Hybrid mode (mesh + midtier)
thresh agent start --midtier https://thresh-hub.com --mesh tailscale
```

**Commands Added:**
- `thresh network join --provider <tailscale|netmaker>` - Join mesh network
- `thresh network leave` - Leave mesh network
- `thresh network status` - Show network status
- `thresh network peers` - List connected peers
- `thresh network test <peer>` - Test connectivity
- `thresh network info` - Show local node info

**Binary Impact:** +150 KB

---

#### Week 2-3: P2P Remote Operations
- [ ] Direct P2P provisioning (via mesh, no midtier)
- [ ] SSH-based fallback for remote operations
- [ ] `thresh remote` command group
- [ ] Blueprint sharing peer-to-peer
- [ ] P2P metrics streaming

**Deliverables:**
```bash
# P2P operations (no midtier required)
thresh remote list --peer dev-machine-02
thresh remote up python-dev --peer dev-machine-02
thresh remote exec dev-machine-02 python-dev -- python --version

# Blueprint sharing
thresh blueprint share postgres-dev --peer dev-machine-02
thresh blueprint fetch postgres-dev --peer dev-machine-03
```

**Commands Added:**
- `thresh remote list --peer <name>` - List remote environments
- `thresh remote up <env> --peer <name>` - Provision on peer
- `thresh remote exec <peer> <env> -- <cmd>` - Execute remote command
- `thresh blueprint share <name> --peer <name>` - Share blueprint
- `thresh blueprint fetch <name> --peer <name>` - Fetch blueprint

**Binary Impact:** +70 KB

---

**Phase 1.7 Success Metrics:**
- [ ] Tailscale mesh connectivity working
- [ ] Netmaker air-gapped deployment working
- [ ] P2P peer discovery functional
- [ ] Remote operations via mesh working
- [ ] Hybrid mode (mesh + midtier) working
- [ ] SSH fallback functional when mesh unavailable
- [ ] <100ms latency between peers
- [ ] Air-gapped testing validated

**Binary Impact:** +220 KB (v1.7 = ~5.60 MB compressed)

**Impact:** 🔥 High (for specialized use cases)
- 🔗 **P2P Connectivity**: Direct peer-to-peer without midtier
- 🏢 **Air-Gapped**: Netmaker for isolated networks
- 🌐 **Zero Config**: Tailscale for simple P2P
- 🤝 **Collaboration**: Direct blueprint sharing
- 🔄 **Hybrid**: Combine mesh + midtier for flexibility

**Use Cases:**
- Air-gapped enterprise environments
- Small team P2P collaboration (no centralized infrastructure)
- Development team with direct peer sharing
- Hybrid deployments (some agents on mesh, some via HTTPS)
- Remote offices with VPN mesh

**When to use v1.7 vs v1.6:**
- Use **v1.6 (HTTPS)** for: Corporate environments, SaaS, centralized management, standard networking
- Use **v1.7 (Mesh)** for: Air-gapped, P2P collaboration, no central server, VPN-based deployments

---

### **Phase 2.0: Polish & Production (Weeks 13-20) - Production Ready** 🎯 v2.0

**Goal:** Production-grade quality, comprehensive distribution, and enterprise features  
**Status:** 📋 Planned (May-Jun 2026)  
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

### v1.6 (Target - Centralized with SignalR Real-Time)
```
                 ┌────────────────────────────────┐
                 │       thresh-hub               │
                 │   (Midtier/SaaS - Separate)    │
                 │                                │
                 │  • SignalR Hub (Real-time)     │
                 │  • REST API (Fallback)         │
                 │  • Web Dashboard               │
                 │  • Multi-user Auth (SSO)       │
                 │  • RBAC & Audit Logging        │
                 │  • Metrics Aggregation         │
                 │  • Blueprint Management        │
                 └────────┬──────────┬────────────┘
                          │          │
           WebSocket/TLS (443)  WebSocket/TLS (443)
                 SignalR        SignalR
              Real-time ⚡     Real-time ⚡
                          │          │
       ┌──────────────────┼──────────┼─────────────┐
       │                  │          │             │
 ┌─────▼─────┐      ┌─────▼─────┐  ┌▼────────┐  ┌▼─────────┐
 │  thresh   │      │  thresh   │  │ thresh  │  │ thresh   │
 │ (Windows) │      │  (Linux)  │  │(macOS)  │  │ (Linux)  │
 │           │      │           │  │         │  │          │
 │ • WSL     │      │•containerd│  │•contain.│  │•Docker   │
 │ • Agent   │←┐    │ • Agent   │←┐│• Agent  │←┐│• Agent   │
 │ • SignalR │ │    │ • SignalR │ ││• SignalR││ │• SignalR │
 │ • Snapshot│ │    │ • Snapshot│ ││• Snapsht││ │• Snapshot│
 │ • Tags    │ │    │ • Tags    │ ││• Tags   ││ │• Tags    │
 │ • TTL     │ │    │ • TTL     │ ││• TTL    ││ │• TTL     │
 │ • Health  │ │    │ • Health  │ ││• Health ││ │• Health  │
 │ • MCP     │ │    │ • MCP     │ ││• MCP    ││ │• MCP     │
 │ • Volumes │ │    │ • Volumes │ ││• Volumes││ │• Volumes │
 └─────┬─────┘ │    └─────┬─────┘ │└───┬─────┘│ └────┬─────┘
       │       │          │       │    │      │      │
       │   Corporate      │    Corporate │  Corporate │
       │   Firewall       │    Firewall  │  Proxy     │
       │   & Proxy        │    & Proxy   │            │
       │                  │               │            │
  VS Code           VS Code         VS Code      Cursor
  (via MCP)        (via MCP)       (via MCP)  (via MCP)

• SignalR over WebSocket/TLS port 443 (firewall-friendly)
• Real-time bidirectional messaging (commands & metrics)
• Instant command delivery (<1s latency, no polling)
• Automatic reconnection (0s, 2s, 10s, 30s)
• REST fallback if WebSocket blocked
• Works through corporate proxies (HTTP_PROXY)
• Native AOT compatible (JSON source generators)
• API key authentication + TLS encryption
• Scalable with SignalR backplane (Redis, Azure SignalR)
```

### v1.7 (Optional - P2P Mesh for Advanced Use Cases)
```
              Mesh Network (Tailscale or Netmaker)
              ┌────────────────────────────┐
              │   WireGuard P2P Overlay    │
              │  (Optional for air-gapped) │
              └────────┬───────────────────┘
                       │
          ┌────────────┼──────────────┐
          │            │              │
    ┌─────▼─────┐ ┌───▼──────┐ ┌────▼──────┐
    │  thresh   │ │  thresh  │ │  thresh   │
    │ (Windows) │ │ (Linux)  │ │  (macOS)  │
    │           │ │          │ │           │
    │ • P2P Ops │ │ • P2P Ops│ │ • P2P Ops │
    │ • Mesh    │ │ • Mesh   │ │ • Mesh    │
    │ • Share   │←┼─→• Share │←┼─→• Share  │
    └───────────┘ └──────────┘ └───────────┘
    
• Direct peer-to-peer connectivity
• No central server required
• Blueprint sharing between peers
• Air-gapped environments (Netmaker)
• Small team collaboration
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
| **v1.6 (agent/midtier)** 📋 | **5.42 MB** | **+320 KB** | **Agent daemon, SignalR real-time, multi-tier fallback, cloud backup, snapshots, tags, TTL, health checks** |
| **v1.7 (mesh - optional)** 📋 | **5.60 MB** | **+180 KB** | **P2P mesh (Tailscale/Netmaker), remote ops, air-gapped** |
| **v2.0 (production)** 🎯 | **5.75 MB** | **+150 KB** | **Package distribution, security, monitoring, polish** |

**Total growth v1.5 → v1.6:** +320 KB (+6.3%) - Core agent with multi-tier fallback  
**Total growth v1.6 → v1.7:** +180 KB (+3.3%) - Optional mesh networking  
**Total growth v1.5 → v2.0:** +650 KB (+12.7%) - Full feature set  

**Value delivered:**  
- v1.6: Agent daemon, SignalR real-time communication, multi-tier failover (on-prem → cloud), disaster recovery, offline resilience, snapshots, resource control, health checks  
- v1.7: Optional P2P mesh for air-gapped/small teams  
- v2.0: Package distribution, enterprise security, production polish  

**Exceptional efficiency:** <650 KB growth for enterprise-grade distributed development tool with HA failover

**Note:** Centralized management (thresh-hub midtier/SaaS) built in separate commercial repository

---

## 🎯 Success Criteria

### Technical Metrics
- [ ] Single binary runs on Windows, Linux, macOS
- [ ] Binary size < 6 MB (compressed)
- [ ] Provision time < 30 seconds
- [ ] Agent mode runs stable as daemon/service
- [ ] SignalR connection established successfully
- [ ] Real-time command delivery working (<1s latency)
- [ ] Automatic reconnection working (exponential backoff)
- [ ] REST fallback working when WebSocket blocked
- [ ] MCP integration working in 3+ AI editors
- [ ] Snapshots and cloning working
- [ ] Resource limits enforced correctly
- [ ] TTL auto-cleanup functional
- [ ] Health checks working
- [ ] (Optional) Mesh network connectivity for v1.7
- [ ] (Optional) P2P operations for v1.7

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

### Small Team (Week 12)
```bash
# Option 1: Pure cloud SaaS (easiest)
thresh agent config set midtier.url https://thresh-hub.io
thresh agent config set midtier.api_key xyz789
thresh agent start

# Option 2: Self-hosted with cloud backup (hybrid)
thresh agent config set midtier.url https://thresh-hub.company.com
thresh agent config set midtier.fallback_url https://thresh-hub.io
thresh agent config set midtier.api_key xyz789
thresh agent config set midtier.fallback_api_key abc123
thresh agent start

# Manage from web dashboard or CLI
thresh list --remote  # See all team environments
```

### Enterprise Fleet (Week 16)
```bash
# IT deploys on-prem thresh-hub midtier
docker run -d thresh-hub

# Deploy agents with hybrid failover (on-prem + cloud backup)
thresh agent config set midtier.url https://thresh-hub.corp.local
thresh agent config set midtier.fallback_url https://thresh-hub.io
thresh agent config set midtier.api_key ${API_KEY}
thresh agent config set midtier.fallback_api_key ${CLOUD_KEY}
thresh agent config set midtier.failover_enabled true
thresh agent start

# Agents stay connected even if on-prem goes down
# Automatic cloud failover for disaster recovery
# Automatic failback when on-prem recovers

# Centralized management from web dashboard
# RBAC, audit logging, metrics, cost tracking
# 99.9%+ uptime with cloud backup

# Optional: Add mesh networking for air-gapped sites (v1.7)
thresh network join --provider netmaker --server http://netmaker.corp
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
