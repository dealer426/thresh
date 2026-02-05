# GitHub Actions CI/CD Workflow Plan

**Created**: January 26, 2026  
**Status**: To be implemented after consolidation + rename  
**Timeline**: Phase 3 (2-3 hours)

---

## Objectives

1. **Multi-Platform Builds**: Linux, Windows, macOS (x64 + ARM64)
2. **Automated Releases**: Binary artifacts on GitHub Releases
3. **Version Management**: Semantic versioning with tags
4. **Quality Gates**: Build verification on every PR

---

## Workflow Structure

### 1. Build Workflow (`build.yml`)

**Triggers:**
- Push to `main` branch
- Pull requests
- Manual dispatch

**Jobs:**
```yaml
jobs:
  build-linux-x64:
    runs-on: ubuntu-latest
    steps:
      - Checkout code
      - Setup .NET 9
      - Restore packages
      - Publish Native AOT (linux-x64)
      - Upload artifact
      
  build-windows-x64:
    runs-on: windows-latest
    steps:
      - Checkout code
      - Setup .NET 9
      - Restore packages
      - Publish Native AOT (win-x64)
      - Upload artifact
      
  build-macos-arm64:
    runs-on: macos-14 (M1)
    steps:
      - Checkout code
      - Setup .NET 9
      - Restore packages
      - Publish Native AOT (osx-arm64)
      - Upload artifact
```

### 2. Release Workflow (`release.yml`)

**Triggers:**
- Git tag push: `v*` (e.g., `v1.0.0`)

**Steps:**
1. Build all platforms
2. Create GitHub Release
3. Upload binaries as assets
4. Generate changelog
5. Publish release notes

---

## Expected Binary Sizes

| Platform | Architecture | Expected Size | Compression |
|----------|--------------|---------------|-------------|
| Linux | x64 | 4.5MB | ~2MB .tar.gz |
| Linux | ARM64 | 4-5MB | ~2MB .tar.gz |
| Windows | x64 | 5-8MB | ~3MB .zip |
| Windows | ARM64 | 5-8MB | ~3MB .zip |
| macOS | x64 | 5-7MB | ~2.5MB .tar.gz |
| macOS | ARM64 | 4-6MB | ~2MB .tar.gz |

**Total release size**: ~15-20MB (all 6 binaries compressed)

---

## Workflow Files

### `.github/workflows/build.yml`

```yaml
name: Build

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

env:
  DOTNET_VERSION: '9.0.x'

jobs:
  build-linux-x64:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore thresh/thresh.csproj
    
    - name: Build
      run: dotnet build thresh/thresh.csproj -c Release --no-restore
    
    - name: Publish Native AOT
      run: dotnet publish thresh/thresh.csproj -c Release -r linux-x64 --no-restore
    
    - name: Upload artifact
      uses: actions/upload-artifact@v4
      with:
        name: thresh-linux-x64
        path: thresh/bin/Release/net9.0/linux-x64/publish/thresh
        retention-days: 7

  build-windows-x64:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore thresh/thresh.csproj
    
    - name: Publish Native AOT
      run: dotnet publish thresh/thresh.csproj -c Release -r win-x64
    
    - name: Upload artifact
      uses: actions/upload-artifact@v4
      with:
        name: thresh-windows-x64
        path: thresh/bin/Release/net9.0/win-x64/publish/thresh.exe
        retention-days: 7

  build-macos-arm64:
    runs-on: macos-14  # M1/M2/M3 runners
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore thresh/thresh.csproj
    
    - name: Publish Native AOT
      run: dotnet publish thresh/thresh.csproj -c Release -r osx-arm64
    
    - name: Upload artifact
      uses: actions/upload-artifact@v4
      with:
        name: thresh-macos-arm64
        path: thresh/bin/Release/net9.0/osx-arm64/publish/thresh
        retention-days: 7
```

### `.github/workflows/release.yml`

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

env:
  DOTNET_VERSION: '9.0.x'

jobs:
  create-release:
    runs-on: ubuntu-latest
    outputs:
      upload_url: ${{ steps.create_release.outputs.upload_url }}
      version: ${{ steps.get_version.outputs.version }}
    
    steps:
    - name: Get version from tag
      id: get_version
      run: echo "version=${GITHUB_REF#refs/tags/v}" >> $GITHUB_OUTPUT
    
    - name: Create Release
      id: create_release
      uses: actions/create-release@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        tag_name: ${{ github.ref }}
        release_name: thresh ${{ steps.get_version.outputs.version }}
        draft: false
        prerelease: false

  build-and-upload:
    needs: create-release
    strategy:
      matrix:
        include:
          - os: ubuntu-latest
            runtime: linux-x64
            artifact: thresh
            archive: thresh-linux-x64.tar.gz
          - os: windows-latest
            runtime: win-x64
            artifact: thresh.exe
            archive: thresh-windows-x64.zip
          - os: macos-14
            runtime: osx-arm64
            artifact: thresh
            archive: thresh-macos-arm64.tar.gz
    
    runs-on: ${{ matrix.os }}
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Publish
      run: dotnet publish thresh/thresh.csproj -c Release -r ${{ matrix.runtime }}
    
    - name: Create Archive (Linux/macOS)
      if: runner.os != 'Windows'
      run: |
        cd thresh/bin/Release/net9.0/${{ matrix.runtime }}/publish
        tar -czf ${{ matrix.archive }} ${{ matrix.artifact }}
        mv ${{ matrix.archive }} ${{ github.workspace }}/
    
    - name: Create Archive (Windows)
      if: runner.os == 'Windows'
      run: |
        cd thresh/bin/Release/net9.0/${{ matrix.runtime }}/publish
        Compress-Archive -Path ${{ matrix.artifact }} -DestinationPath ${{ github.workspace }}/${{ matrix.archive }}
    
    - name: Upload Release Asset
      uses: actions/upload-release-asset@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        upload_url: ${{ needs.create-release.outputs.upload_url }}
        asset_path: ./${{ matrix.archive }}
        asset_name: ${{ matrix.archive }}
        asset_content_type: application/octet-stream
```

---

## Additional Workflows (Optional)

### 3. Test Workflow (`test.yml`)

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

### 4. Security Scan (`security.yml`)

```yaml
name: Security Scan

on:
  schedule:
    - cron: '0 0 * * 0'  # Weekly
  workflow_dispatch:

jobs:
  scan:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: 'fs'
        scan-ref: '.'
```

---

## Release Process

### Manual Release

```bash
# 1. Update version in code
# Edit thresh/thresh.csproj
<Version>1.0.0</Version>

# 2. Commit changes
git add .
git commit -m "Release v1.0.0"

# 3. Create and push tag
git tag v1.0.0
git push origin v1.0.0

# 4. GitHub Actions automatically:
#    - Builds for all platforms
#    - Creates GitHub Release
#    - Uploads binaries
```

### Automated Version Bump (Future)

```yaml
# .github/workflows/auto-version.yml
- name: Bump version
  uses: phips28/gh-action-bump-version@master
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

---

## Download Links

After release, users can download binaries from:

```
https://github.com/USERNAME/thresh/releases/latest/download/thresh-linux-x64.tar.gz
https://github.com/USERNAME/thresh/releases/latest/download/thresh-windows-x64.zip
https://github.com/USERNAME/thresh/releases/latest/download/thresh-macos-arm64.tar.gz
```

### Installation Script

```bash
# install.sh
#!/bin/bash
curl -fsSL https://github.com/USERNAME/thresh/releases/latest/download/thresh-linux-x64.tar.gz | tar -xz
sudo mv thresh /usr/local/bin/
thresh --version
```

---

## Build Matrix (Full Coverage)

| OS | Architecture | Runner | Binary Name | Size |
|----|--------------|--------|-------------|------|
| Linux | x64 | ubuntu-latest | thresh | 4.5MB |
| Linux | ARM64 | ubuntu-latest (cross-compile) | thresh | 4-5MB |
| Windows | x64 | windows-latest | thresh.exe | 5-8MB |
| Windows | ARM64 | windows-latest (cross-compile) | thresh.exe | 5-8MB |
| macOS | x64 | macos-13 | thresh | 5-7MB |
| macOS | ARM64 | macos-14 | thresh | 4-6MB |

**Note**: ARM64 cross-compilation may require additional toolchain setup.

---

## Benefits

✅ **Automated Builds**: Every commit tested  
✅ **Multi-Platform**: 6 platforms from one codebase  
✅ **Easy Distribution**: GitHub Releases integration  
✅ **Version Control**: Semantic versioning  
✅ **Quality Assurance**: Pre-merge testing  
✅ **Fast Iterations**: <10 minute build times  

---

## Timeline

| Task | Duration |
|------|----------|
| Create build.yml | 30 mins |
| Create release.yml | 45 mins |
| Test workflows | 30 mins |
| Add badges to README | 15 mins |
| Documentation | 30 mins |
| **Total** | **2-3 hours** |

---

## Success Metrics

- [ ] All platforms build successfully
- [ ] Binaries run on target OS
- [ ] Release artifacts uploaded correctly
- [ ] Download links work
- [ ] Build time <10 minutes per platform
- [ ] CI passes on every PR

---

**Implementation Phase**: 3 (After consolidation + rename)  
**Priority**: Medium (nice-to-have but important for distribution)  
**Status**: ⏳ Planned
