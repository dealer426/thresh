# Package Submission Quick Reference

## ✅ Pre-Submission Checklist

- [ ] v1.0.0 GitHub Release is complete and published
- [ ] `thresh-windows-x64.zip` is available in release assets
- [ ] Release workflow succeeded (check GitHub Actions)
- [ ] Binary tested locally (`thresh --version`)

## 🚀 Step-by-Step Submission

### Step 1: Update SHA256 Hashes (5 minutes)

```powershell
# From repository root
cd packages
.\update-hashes.ps1 -Version "1.0.0"

# Verify changes
git diff packages/

# Commit
git add packages/
git commit -m "chore: Update package hashes for v1.0.0"
git push origin main
```

---

### Step 2: Submit to Scoop (Fastest - 1-2 days)

**Why Scoop first?** Fastest approval, auto-update support, developer audience

```bash
# 1. Fork https://github.com/ScoopInstaller/Main
# 2. Clone your fork
git clone https://github.com/YOUR_USERNAME/Main.git scoop-main
cd scoop-main

# 3. Copy manifest
cp ../thresh/packages/scoop/thresh.json bucket/

# 4. Commit and push
git checkout -b add-thresh
git add bucket/thresh.json
git commit -m "thresh: Add version 1.0.0"
git push origin add-thresh

# 5. Create PR at https://github.com/ScoopInstaller/Main/compare
```

**PR Title**: `thresh: Add version 1.0.0`

**PR Description**:
```markdown
Adds thresh v1.0.0 - Fast, native WSL2 environment provisioning tool

- 12 MB Native AOT binary
- WSL2 integration for Linux environment provisioning
- AI-powered blueprint generation
- 12 built-in distributions

GitHub: https://github.com/dealer426/thresh
License: MIT
```

---

### Step 3: Submit to Chocolatey (Medium - 1-3 days)

**Requirements**: 
- Chocolatey account at https://community.chocolatey.org/
- API key from https://community.chocolatey.org/account

```powershell
# 1. Install chocolatey CLI (if not installed)
# See: https://chocolatey.org/install

# 2. Set API key (one-time)
choco apikey --key YOUR_API_KEY_HERE --source https://push.chocolatey.org/

# 3. Build package
cd packages/chocolatey
choco pack thresh.nuspec

# 4. Push to chocolatey
choco push thresh.1.0.0.nupkg --source https://push.chocolatey.org/

# 5. Wait for moderation (1-3 days)
# Track at: https://community.chocolatey.org/packages/thresh
```

**Notes**:
- First submission requires manual approval
- Future updates are automated after trust is established
- Check moderation comments at https://community.chocolatey.org/packages/thresh/1.0.0

---

### Step 4: Submit to winget (Slowest - 3-5 days)

**Why winget last?** Slowest approval but most official (Microsoft)

```bash
# 1. Fork https://github.com/microsoft/winget-pkgs
# 2. Clone your fork
git clone https://github.com/YOUR_USERNAME/winget-pkgs.git
cd winget-pkgs

# 3. Create manifest directory
mkdir -p manifests/d/dealer426/thresh/1.0.0

# 4. Copy manifests
cp ../thresh/packages/winget/*.yaml manifests/d/dealer426/thresh/1.0.0/

# 5. Commit and push
git checkout -b dealer426-thresh-1.0.0
git add manifests/d/dealer426/thresh/1.0.0/
git commit -m "New package: dealer426.thresh version 1.0.0"
git push origin dealer426-thresh-1.0.0

# 6. Create PR at https://github.com/microsoft/winget-pkgs/compare
```

**PR Title**: `New package: dealer426.thresh version 1.0.0`

**PR Description**:
```markdown
Adds thresh v1.0.0 - Fast, native WSL2 environment provisioning tool with AI-powered blueprints

Automated validation should pass all checks.

- 16.6 MB Native AOT binary (.NET 9)
- WSL2 integration for Linux environments
- AI-powered blueprint generation (OpenAI GPT-4o-mini)
- 12 built-in distributions + custom support

Repository: https://github.com/dealer426/thresh
License: MIT
```

**Validation**:
- GitHub Actions will automatically validate the manifest
- Fix any issues reported by the bot
- Wait for Microsoft reviewer approval (3-5 days)

---

## 📊 Tracking Submissions

| Package Manager | Status | Link | ETA |
|----------------|--------|------|-----|
| **Scoop** | ⏳ Pending | [PR Link] | 1-2 days |
| **Chocolatey** | ⏳ Pending | [Package Link] | 1-3 days |
| **winget** | ⏳ Pending | [PR Link] | 3-5 days |

---

## ✅ Post-Approval Verification

Once approved, test installation:

```powershell
# Scoop
scoop install thresh
thresh --version

# Chocolatey  
choco install thresh
thresh --version

# winget
winget install dealer426.thresh
thresh --version
```

All should install to PATH and show:
```
thresh 1.0.0
.NET Native AOT (16.6 MB)
https://github.com/dealer426/thresh
```

---

## 🔄 Future Updates (v1.1.0, v2.0.0, etc.)

### Scoop (Automatic)
✅ **No action needed!** Auto-update support built-in.

### Chocolatey (Semi-Automatic)
```powershell
# Update version in thresh.nuspec
# Update SHA256 in chocolateyinstall.ps1
choco pack thresh.nuspec
choco push thresh.X.Y.Z.nupkg --source https://push.chocolatey.org/
```

### winget (Manual PR)
```bash
# Create new version directory
mkdir -p manifests/d/dealer426/thresh/X.Y.Z
# Copy and update manifests
# Create PR
```

**Tip**: Set up GitHub Actions to automate future package updates!

---

## 🆘 Troubleshooting

**Scoop PR rejected?**
- Check manifest syntax: `scoop checkver -u thresh.json`
- Ensure SHA256 matches exactly
- Follow naming conventions (lowercase, no special chars)

**Chocolatey moderation failed?**
- Check package size (<200 MB recommended)
- Verify install script doesn't require admin
- Review moderation comments carefully

**winget validation failed?**
- Run local validation: `winget validate --manifest manifests/d/dealer426/thresh/1.0.0`
- Check YAML syntax (strict indentation)
- Ensure all URLs are accessible

---

## 📚 Documentation Links

- **Scoop**: https://github.com/ScoopInstaller/Scoop/wiki/App-Manifests
- **Chocolatey**: https://docs.chocolatey.org/en-us/create/create-packages
- **winget**: https://docs.microsoft.com/en-us/windows/package-manager/package/manifest

---

**Last Updated**: February 5, 2026  
**Next Review**: After v1.0.0 submission complete
