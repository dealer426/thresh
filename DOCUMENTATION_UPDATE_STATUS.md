# Documentation Update Status for v1.4.0

**Date:** February 16, 2026  
**Progress:** Phase 1-2 Complete, Starting Phase 3

## ✅ Phase 1: Root Documentation (COMPLETED)

- [x] **CHANGELOG.md** - Complete rewrite with proper thresh history
- [x] **README.md** - Updated with v1.4.0 commands, multi-platform support
- [x] **GETTING_STARTED.md** - Updated with grouped commands, new examples

## ✅ Phase 2: Core Project Files (COMPLETED)

- [x] **thresh/README.md** - Updated with v1.4.0 features, version badges
- [x] All command examples updated to grouped structure
- [x] Added platform support table
- [x] Added migration guide

## 🔄 Phase 3: Website Documentation (IN PROGRESS)

### Critical CLI Reference Updates Needed

**Files requiring command structure updates:**
1. `website/docs/cli-reference/blueprints.md` - Change to `blueprint list`
2. `website/docs/cli-reference/generate.md` - Change to `blueprint generate`
3. Create: `website/docs/cli-reference/blueprint.md` - Parent command documentation
4. Create: `website/docs/cli-reference/blueprint-delete.md` - New delete command

**Files with `thresh blueprints` references (30+ matches):**
- website/docs/tutorials/index.md
- website/docs/tutorials/custom-blueprints.md
- website/docs/tutorials/quick-start.md
- website/docs/installation.md
- website/docs/cli-reference/config.md
- website/docs/cli-reference/generate.md

### Systematic Update Plan

**Priority 1: CLI Reference**
- [ ] Update blueprints.md → Document `thresh blueprint list`
- [ ] Update generate.md → Document `thresh blueprint generate`
- [ ] Create blueprint.md → Parent command with subcommands
- [ ] Create blueprint-delete.md → New delete command
- [ ] Update index.md → Add new commands to navigation

**Priority 2: Core Documentation**
- [ ] Update intro.md - Add v1.4.0 highlights
- [ ] Update installation.md - Update version, commands
- [ ] Update download.md - Update version, binary sizes
- [ ] Update mcp-integration.md - New parallel tools

**Priority 3: Tutorials**
- [ ] Update quick-start.md - All command examples
- [ ] Update custom-blueprints.md - Blueprint management workflow
- [ ] Update vscode-mcp.md - MCP tool updates
- [ ] Update copilot-sdk.md - Command examples

## ⏳ Phase 4: Docusaurus Versioning (PENDING)

- [ ] Run `npm run docusaurus docs:version 1.4.0`
- [ ] Verify version-1.4.0 folder created
- [ ] Update versions.json
- [ ] Test version switcher

## ⏳ Phase 5: Version Bumps (PENDING)

- [ ] thresh/Thresh/Thresh.csproj (1.2.0 → 1.4.0)
- [ ] website/package.json
- [ ] .github/workflows/*.yml

## 📊 Statistics

- **Files Updated:** 5
- **Files Remaining:** ~40
- **Lines Changed:** ~800
- **Breaking Changes Documented:** Yes
- **Migration Guide:** Yes

## 🎯 Next Actions

1. Create CLI reference docs for new blueprint subcommands
2. Update all `thresh blueprints` → `thresh blueprint list`
3. Update all `thresh generate` → `thresh blueprint generate`
4. Add `thresh blueprint delete` documentation
5. Test documentation builds locally
6. Create Docusaurus version 1.4.0
7. Final review and release

---

**Last Updated:** $(date)
