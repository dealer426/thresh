# Package Distribution Guide

This directory contains package manifests for distributing thresh via Windows package managers.

## 📦 Package Managers

### 1. **winget** (Microsoft Official)
**Status**: ⏳ Pending submission  
**Install command**: `winget install dealer426.thresh`

**Files**:
- `dealer426.thresh.yaml` - Version manifest
- `dealer426.thresh.locale.en-US.yaml` - Package metadata
- `dealer426.thresh.installer.yaml` - Installer configuration

**Submission Process**:
1. Wait for v1.0.0 GitHub release to complete
2. Get SHA256 hash of `thresh-windows-x64.zip`
3. Update `<TO_BE_UPDATED_AFTER_RELEASE>` in `dealer426.thresh.installer.yaml`
4. Fork https://github.com/microsoft/winget-pkgs
5. Create PR with manifests in `manifests/d/dealer426/thresh/1.0.0/`
6. Wait for automated validation and Microsoft review (~3-5 days)

**Documentation**: https://docs.microsoft.com/en-us/windows/package-manager/package/manifest

---

### 2. **Chocolatey** (Community)
**Status**: ⏳ Pending submission  
**Install command**: `choco install thresh`

**Files**:
- `thresh.nuspec` - Package metadata
- `tools/chocolateyinstall.ps1` - Installation script
- `tools/chocolateyuninstall.ps1` - Uninstallation script

**Submission Process**:
1. Wait for v1.0.0 GitHub release to complete
2. Get SHA256 hash of `thresh-windows-x64.zip`
3. Update `<TO_BE_UPDATED_AFTER_RELEASE>` in `chocolateyinstall.ps1`
4. Create Chocolatey account at https://community.chocolatey.org/
5. Package: `choco pack thresh.nuspec`
6. Push: `choco push thresh.1.0.0.nupkg --source https://push.chocolatey.org/`
7. Wait for moderation approval (~1-3 days)

**Documentation**: https://docs.chocolatey.org/en-us/create/create-packages

---

### 3. **Scoop** (Developer-Focused)
**Status**: ⏳ Pending submission  
**Install command**: `scoop install thresh`

**Files**:
- `thresh.json` - Manifest with auto-update support

**Submission Process**:
1. Wait for v1.0.0 GitHub release to complete
2. Get SHA256 hash of `thresh-windows-x64.zip`
3. Update `<TO_BE_UPDATED_AFTER_RELEASE>` in `thresh.json`
4. Fork https://github.com/ScoopInstaller/Main
5. Add `thresh.json` to `bucket/` directory
6. Create PR
7. Wait for review (~1-2 days, fastest approval)

**Documentation**: https://github.com/ScoopInstaller/Scoop/wiki/App-Manifests

---

## 🚀 Quick Start (After v1.0.0 Release)

### Step 1: Get Release SHA256 Hashes
```powershell
# Download the release zip
Invoke-WebRequest -Uri "https://github.com/dealer426/thresh/releases/download/v1.0.0/thresh-windows-x64.zip" -OutFile "thresh.zip"

# Calculate SHA256
Get-FileHash thresh.zip -Algorithm SHA256 | Select-Object -ExpandProperty Hash
```

### Step 2: Update Manifests
Replace all `<TO_BE_UPDATED_AFTER_RELEASE>` placeholders with the SHA256 hash.

**Files to update**:
- `winget/dealer426.thresh.installer.yaml` (line 13)
- `chocolatey/tools/chocolateyinstall.ps1` (line 6)
- `scoop/thresh.json` (line 6)

### Step 3: Submit to Package Managers

**Winget** (slowest, 3-5 days):
```bash
# Fork https://github.com/microsoft/winget-pkgs
git clone https://github.com/YOUR_USERNAME/winget-pkgs.git
cd winget-pkgs
mkdir -p manifests/d/dealer426/thresh/1.0.0
cp ../thresh/packages/winget/*.yaml manifests/d/dealer426/thresh/1.0.0/
git add .
git commit -m "Add thresh version 1.0.0"
git push
# Create PR on GitHub
```

**Chocolatey** (medium, 1-3 days):
```powershell
cd packages/chocolatey
choco pack thresh.nuspec
choco apikey --key YOUR_API_KEY --source https://push.chocolatey.org/
choco push thresh.1.0.0.nupkg --source https://push.chocolatey.org/
```

**Scoop** (fastest, 1-2 days):
```bash
# Fork https://github.com/ScoopInstaller/Main
git clone https://github.com/YOUR_USERNAME/Main.git
cd Main
cp ../thresh/packages/scoop/thresh.json bucket/
git add bucket/thresh.json
git commit -m "thresh: Add version 1.0.0"
git push
# Create PR on GitHub
```

---

## 🔄 Future Release Updates

### Automated Updates (Scoop)
Scoop manifest includes `autoupdate` - future releases are detected automatically!

### Manual Updates (Winget & Chocolatey)
For each new version:
1. Update version numbers in manifests
2. Update SHA256 hashes
3. Submit new PRs/packages

**Tip**: Set up GitHub Actions to automate this process in the future.

---

## ✅ Verification

After submission approval, users can install with:

```powershell
# Winget (Microsoft Official)
winget install dealer426.thresh

# Chocolatey (Community)
choco install thresh

# Scoop (Developer)
scoop install thresh
```

All three methods install the same 12 MB native binary from GitHub Releases.

---

## 📊 Package Manager Comparison

| Feature | winget | Chocolatey | Scoop |
|---------|--------|------------|-------|
| **Approval Time** | 3-5 days | 1-3 days | 1-2 days |
| **Auto-Update** | Manual PR | Manual push | Automatic |
| **Popularity** | Official MS | Most popular | Developer-focused |
| **User Base** | ~100M+ | ~20M+ | ~5M+ |
| **Review Process** | Automated + Manual | Manual moderation | Community review |
| **Best For** | Enterprise | General users | Developers |

**Recommendation**: Submit to all three for maximum reach.
