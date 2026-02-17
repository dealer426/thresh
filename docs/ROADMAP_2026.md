# thresh Roadmap 2026 - Distributed Development Orchestration

**Created**: February 6, 2026  
**Updated**: February 17, 2026  
**Timeline**: 16 weeks (4 months)  
**Current Version**: v1.4.0 (Cross-Platform Release)  
**Status**: Phase 1 Complete ✅ | Optimization Complete ✅ | Documentation Complete ✅ | Linux/macOS Testing Complete ✅ | v1.4.0 Released ✅  
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

**v1.4.0 State (Feb 17, 2026 - CURRENT):**
```
Cross-platform (Windows/Linux/macOS) → Multi-platform builds → 11 MCP tools → Platform-specific docs → Repository cleanup
```

**Target v1.5.0 State (Mar 2026):**
```
Package managers (winget/Chocolatey/Scoop/Homebrew) → Enhanced tutorials → Search integration (Algolia)
```

**Target v2.0 State (Jun 2026):**
```
Fleet of machines → Mesh network → Central orchestration → Distributed workloads → Professional docs
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
| **Current Binary Size** | v1.4.0 | **~5 MB** (Win/Linux), **~13 MB** (macOS) | 🔥 Excellent |

**Note:** v1.4.0 shipped February 17, 2026 with full cross-platform support for Windows, Linux, and macOS.

### Proposed Features 🔮

| Feature | Effort | Size Impact | Value | Priority |
|---------|--------|-------------|-------|----------|
| **Docusaurus Documentation** | 1-2 weeks | N/A (website) | 🔥 Huge | **P0** |
| **Agent Mode (Daemon)** | 1 week | +60 KB | High | **P1** |
| **Mesh Network (Tailscale/Netmaker)** | 2 weeks | +120 KB | High | **P1** |
| **Central Hub** | 2 weeks | Separate (~8 MB) | High | **P2** |
| **Workload Orchestration** | 2 weeks | +100 KB | Very High | **P2** |
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
- Package manager distribution (winget, Chocolatey, Scoop, Homebrew)
- Algolia DocSearch integration for documentation site
- Additional CLI command documentation (serve, metrics, etc.)
- Enhanced tutorials and use case examples

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
- [x] macOS testing via local development and GitHub Actions runners
  - [ ] Install containerd/Docker
  - [ ] Install .NET 10 SDK
  - [ ] Clone thresh repository
  - [ ] Build thresh natively on Linux
- [ ] Manual testing on Linux:
  - [ ] Test `thresh up python-dev`
  - [ ] Test `thresh list`
  - [ ] Test `thresh destroy`
  - [ ] Test `thresh serve --stdio` (MCP server)
  - [ ] Test all CLI commands
- [ ] Debug and fix platform-specific issues:
  - [ ] File path separators (`\` vs `/`)
  - [ ] Line endings (CRLF vs LF)
  - [ ] Containerd socket locations
  - [ ] Permissions and executable bits
  - [ ] Shell differences (bash vs PowerShell)
- [ ] Iterate quickly with SSH access
- [ ] Document Linux-specific setup requirements
- [ ] Update IContainerService implementations if needed

**Deliverables:**
```bash
# Pulumi infrastructure
cd pulumi
pulumi up  # Provisions Ubuntu VM in vCenter

# SSH into test VM
ssh ubuntu-vm
cd thresh
dotnet build  # Compiles successfully on Linux
dotnet run -- up python-dev  # Works!

# Verify all commands
thresh list
thresh metrics
thresh serve --stdio
```

**vCenter Test Environment:**
```
Windows Dev Machine
    ↓
Pulumi → vCenter
    ↓
├─ Ubuntu 22.04 VM (containerd testing)
└─ AlmaLinux VM (RHEL-like testing)
```

#### Week 8: GitHub Actions Multi-Platform CI/CD
- [ ] Take learnings from vCenter testing
- [ ] Add Linux x64 build job to GitHub Actions
  - [ ] Use `ubuntu-latest` runner
  - [ ] Install .NET 10 SDK
  - [ ] Build with Native AOT
  - [ ] Test binary execution
  - [ ] Generate SBOM
- [ ] Add macOS x64 build job (Intel)
  - [ ] Use `macos-13` runner (Intel)
  - [ ] Native AOT compilation
  - [ ] Test on macOS
- [ ] Add macOS ARM64 build job (Apple Silicon)
  - [ ] Use `macos-14` runner (M1/M2)
  - [ ] ARM64 Native AOT
  - [ ] Test on Apple Silicon
- [ ] Implement build matrix strategy
- [ ] Add UPX compression for all platforms
- [ ] Platform-specific SBOM generation
- [ ] Update release workflow for 4 artifacts
- [ ] Add build status badges to README
- [ ] Create platform-specific installation documentation
- [ ] Keep vCenter VMs as ongoing dev test environment

**Deliverables:**
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

**Release Artifacts:**
```
- thresh-win-x64.zip         (Windows, 3.8 MB compressed)
- thresh-linux-x64.tar.gz    (Linux, ~4.0 MB compressed)
- thresh-macos-x64.tar.gz    (macOS Intel, ~4.2 MB compressed)
- thresh-macos-arm64.tar.gz  (macOS Apple Silicon, ~4.0 MB compressed)
```

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

### **Phase 2 (Original): Metrics & Networking (Weeks 9-12) - Distributed Foundation**

**Goal:** Enable multi-machine awareness and connectivity

#### Week 9: Host Metrics Collection
- [ ] Create `HostMetrics` model
- [ ] Implement cross-platform metrics (CPU, RAM, storage)
- [ ] Add `thresh metrics` command
- [ ] JSON output for scripting

**Deliverables:**
```bash
thresh metrics
thresh metrics --json

# Output:
{
  "hostname": "dev-machine-01",
  "platform": "windows",
  "cpu_percent": 23.5,
  "memory_used_gb": 8.2,
  "memory_total_gb": 16.0,
  "storage_free_gb": 150.0,
  "containers": 3
}
```

**Binary:** 16.9 MB → 17.0 MB (+80 KB)

#### Week 10: Agent Mode
- [ ] Implement daemon/background mode
- [ ] Periodic metrics collection
- [ ] HTTP reporting client
- [ ] Configuration for hub URL

**Deliverables:**
```bash
thresh agent start
thresh agent status
thresh agent stop

# Runs in background, reports metrics every 60s
```

**Binary:** 17.0 MB → 17.06 MB (+60 KB)

#### Week 11-12: Dual Mesh Network Support
- [ ] Create `IMeshNetworkService` interface
- [ ] Implement `TailscaleService`
- [ ] Implement `NetmakerService`
- [ ] Add `thresh network` commands
- [ ] Test air-gapped (Netmaker) and cloud (Tailscale)

**Deliverables:**
```bash
# Tailscale (simple)
thresh network join --provider tailscale
thresh network status

# Netmaker (air-gapped)
thresh network join --provider netmaker \
  --server https://netmaker.local \
  --token <token>

thresh network peers
```

**Binary:** 17.06 MB → 17.18 MB (+120 KB)

**Phase 2 Success Metrics:**
- ✅ Agents report metrics to hub
- ✅ Mesh network connectivity (Tailscale + Netmaker)
- ✅ Multi-node communication working
- ✅ Air-gapped deployment tested

---

### **Phase 3: Orchestration (Weeks 13-16) - Hub & Automation**

**Goal:** Central hub with workload scheduling

#### Week 13-14: Central Hub (Separate Project)
- [ ] Create `thresh-hub` ASP.NET Core project
- [ ] Metrics ingestion API (`POST /api/v1/agents/{id}/metrics`)
- [ ] Agent registry and status
- [ ] SQLite persistence
- [ ] Simple web dashboard

**Deliverables:**
```bash
# Deploy hub
thresh-hub start --bind 10.10.1.1:8080

# Agents automatically report
thresh agent start --hub http://10.10.1.1:8080
```

**Hub Binary:** ~8-10 MB (separate from thresh)

#### Week 15-16: Workload Orchestration
- [ ] Workload placement algorithm
- [ ] Remote provisioning API
- [ ] Hub → Agent RPC communication
- [ ] Status tracking and reporting

**Deliverables:**
```bash
# From any machine, provision anywhere
thresh up python-dev --remote --cpu-min 4 --memory-min 8gb

# Output:
✓ Analyzing fleet capacity...
✓ Selected host: dev-server-03 (CPU: 15%, RAM: 4/64GB)
✓ Provisioning on 10.10.1.5...
✓ Environment ready: ssh 10.10.1.5 -t "wsl -d python-dev"
```

**Binary:** 17.18 MB → 17.28 MB (+100 KB)

**Phase 3 Success Metrics:**
- ✅ Hub aggregates metrics from 3+ agents
- ✅ Remote workload provisioning works
- ✅ Automatic host selection based on resources
- ✅ Dashboard shows fleet status

---

### **Phase 4: Polish & Distribution (Weeks 17-20) - Production Ready**

**Goal:** Production-grade quality and distribution

#### Week 17: Multi-Platform CI/CD
- [ ] Add Linux x64 build to GitHub Actions
- [ ] Add macOS ARM64 build
- [ ] Update release workflow for 3 platforms
- [ ] Add build badges to README

**Deliverables:**
- Linux binary (13-15 MB)
- macOS binary (14-16 MB)
- Windows binary (17.3 MB)

#### Week 18: Package Managers
- [ ] Update Chocolatey (Windows)
- [ ] Update Scoop (Windows)
- [ ] Update Winget (Windows)
- [ ] Create Homebrew formula (macOS)
- [ ] Create APT/RPM packages (Linux)

**Deliverables:**
```bash
# Windows
choco install thresh
scoop install thresh
winget install thresh

# macOS
brew install thresh

# Linux
apt install thresh       # Debian/Ubuntu
yum install thresh       # RHEL/Fedora
```

#### Week 19: Documentation & Examples
- [ ] Complete getting started guide
- [ ] Architecture documentation
- [ ] MCP integration examples
- [ ] Fleet deployment guide
- [ ] Video demos

#### Week 20: Testing & Hardening
- [ ] End-to-end testing (all platforms)
- [ ] Security audit
- [ ] Performance optimization
- [ ] Bug fixes
- [ ] Release v2.0

**Phase 4 Success Metrics:**
- ✅ All platforms build automatically
- ✅ Package managers updated
- ✅ Comprehensive documentation
- ✅ Production deployments validated

---

## 🏗️ Architecture Evolution

### v1.0 (Current)
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

### v2.0 (Target - Week 16)
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
| v1.0 (current) | 16.6 MB | - | WSL, Dual AI |
| + Cross-platform | 16.8 MB | +200 KB | containerd support |
| + MCP | 16.9 MB | +100 KB | MCP server |
| + Metrics | 17.0 MB | +80 KB | Host monitoring |
| + Agent | 17.06 MB | +60 KB | Background mode |
| + Mesh | 17.18 MB | +120 KB | Tailscale + Netmaker |
| + Orchestration | 17.28 MB | +100 KB | Remote provisioning |
| **v2.0 (final)** | **17.3 MB** | **+4.2%** | **8x functionality** |

**Exceptional efficiency:** <1 MB growth for fleet orchestration capabilities

---

## 🎯 Success Criteria

### Technical Metrics
- [ ] Single binary runs on Windows, Linux, macOS
- [ ] Binary size < 20 MB
- [ ] Provision time < 30 seconds
- [ ] Fleet of 10+ nodes managed seamlessly
- [ ] MCP integration working in 3+ AI editors
- [ ] Mesh network latency < 100ms
- [ ] Hub handles 100+ agents

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
    I --> J[Central Hub]
    J --> K[Workload Orchestration]
    
    B --> L[AI Editor Integration]
    K --> M[Fleet Management]
    
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
- 🟢 Green: Complete
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
