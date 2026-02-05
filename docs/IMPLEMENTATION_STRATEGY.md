# Implementation Strategy - Consolidate Then Rename

**Date**: January 26, 2026  
**Decision**: Consolidate to .NET first, then rename to "thresh"

---

## Why This Order?

### ❌ Bad: Rename First
```
eknova → thresh (rename everything)
  ├── thresh-cli/ (Quarkus - will delete later!)
  ├── thresh-cli-dotnet/ (new .NET)
  └── thresh-api/

Then consolidate... but we just renamed stuff we're deleting!
```

### ✅ Good: Consolidate First
```
eknova (keep current names)
  ├── eknova-cli/ (Quarkus - legacy)
  ├── eknova-cli-dotnet/ (new .NET - build this)
  └── eknova-api/

After consolidation:
eknova-cli-dotnet/ → thresh/ (single rename, clean)
Delete: eknova-cli/, eknova-api/ (obsolete)
```

---

## Implementation Plan

### Phase 1: Consolidation (2-4 weeks)
Follow `docs/CLI_CONSOLIDATION_PLAN.md`:
1. ✅ Create new .NET CLI project (Phase 0 - DONE!)
2. ⏳ Port WSL service (Phase 1)
3. ⏳ Port Blueprint service (Phase 2)
4. ⏳ Add GitHub Copilot SDK (Phase 4)
5. ⏳ Test everything (Phase 7)
6. ⏳ Build native AOT binary

**Result**: Working `eknova-cli-dotnet/bin/ekn` binary

### Phase 2: Rename (1-2 hours)
Once consolidation is complete:
1. Rename `eknova-cli-dotnet/` → `thresh/`
2. Binary output: `ekn` → `thresh`
3. Update all documentation once
4. Update namespaces, config paths
5. Clean rename, no confusion

### Phase 3: CI/CD Workflow (2-3 hours)
Add GitHub Actions for multi-platform builds:
1. Create `.github/workflows/build.yml`
2. Build for Linux (x64, ARM64)
3. Build for Windows (x64, ARM64)
4. Build for macOS (x64, ARM64)
5. Create GitHub Releases with binaries
6. Add version tagging

**Result**: Automated builds for all platforms on every commit/release

---

## Current Status

✅ Plans created:
- `docs/CLI_CONSOLIDATION_PLAN.md` (34KB, 8 phases)
- `docs/COPILOT_SDK_INTEGRATION_PLAN.md` (27KB)

⏳ Ready to start: Phase 0 - Project Setup

---

## Next Steps

**Start consolidation with Phase 0:**

```bash
# Create new .NET CLI project
cd /mnt/c/Users/burns/source/repos/eknova
mkdir eknova-cli-dotnet
cd thresh-cli-dotnet

# Initialize .NET console app
dotnet new console -n EknovaCli

# Add Native AOT configuration
# Add GitHub.Copilot.SDK
# Test basic build
```

After consolidation is done, we'll do a clean rename to "thresh" in one go.

---

**Approved Strategy**: ✅ Consolidate → Rename  
**Status**: Ready to begin Phase 0
