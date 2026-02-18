# Package Manager Submission Guide

Complete guide for submitting **thresh** to popular package managers across Windows, macOS, and Linux platforms.

---

## Table of Contents

- [Windows Package Managers](#windows-package-managers)
  - [Winget (Windows Package Manager)](#winget-windows-package-manager)
  - [Chocolatey](#chocolatey)
  - [Scoop](#scoop)
- [macOS Package Managers](#macos-package-managers)
  - [Homebrew](#homebrew)
  - [MacPorts](#macports)
- [Linux Package Managers](#linux-package-managers)
  - [Snap Store](#snap-store)
  - [Flatpak (Flathub)](#flatpak-flathub)
  - [AppImage](#appimage)
  - [Debian/Ubuntu (APT)](#debianubuntu-apt)
  - [Fedora/RHEL (DNF/YUM)](#fedorarhel-dnfyum)
  - [Arch Linux (AUR)](#arch-linux-aur)
- [Cross-Platform](#cross-platform)
  - [Nix/NixOS](#nixnixos)
  - [asdf Version Manager](#asdf-version-manager)
- [Container Registries](#container-registries)
  - [Docker Hub](#docker-hub)
  - [GitHub Container Registry](#github-container-registry)

---

## Windows Package Managers

### Winget (Windows Package Manager)

**Official Microsoft package manager for Windows 11+**

#### Prerequisites
- Package manifests ready (already in `packages/winget/`)
- GitHub release with artifacts published
- Microsoft account

#### Submission Process

1. **Fork the winget-pkgs repository**
   ```bash
   gh repo fork microsoft/winget-pkgs --clone
   cd winget-pkgs
   ```

2. **Create a new branch**
   ```bash
   git checkout -b thresh-1.4.0
   ```

3. **Add package manifests to the repository**
   ```bash
   # Winget uses a structured path: manifests/<first-letter>/<publisher>/<package>/<version>/
   mkdir -p manifests/d/dealer426/thresh/1.4.0
   
   # Copy your manifests
   cp /path/to/thresh/packages/winget/dealer426.thresh.yaml manifests/d/dealer426/thresh/1.4.0/
   cp /path/to/thresh/packages/winget/dealer426.thresh.installer.yaml manifests/d/dealer426/thresh/1.4.0/
   cp /path/to/thresh/packages/winget/dealer426.thresh.locale.en-US.yaml manifests/d/dealer426/thresh/1.4.0/
   ```

4. **Validate manifests**
   ```powershell
   # Install winget-create
   winget install Microsoft.WingetCreate
   
   # Validate your submission
   winget validate --manifest manifests/d/dealer426/thresh/1.4.0
   ```

5. **Commit and push**
   ```bash
   git add manifests/d/dealer426/thresh/1.4.0/
   git commit -m "New version: dealer426.thresh version 1.4.0"
   git push origin thresh-1.4.0
   ```

6. **Create Pull Request**
   ```bash
   gh pr create --repo microsoft/winget-pkgs --title "New version: dealer426.thresh version 1.4.0" --body "Update thresh to version 1.4.0"
   ```

7. **Wait for automated checks**
   - Azure Pipelines will validate your manifests
   - SmartScreen/Defender checks will run
   - Typically takes 24-48 hours for review

8. **Address review feedback** (if any)
   - Respond to comments from maintainers
   - Update manifests as needed

#### Update Process for Future Versions

```bash
# Use winget-create to generate updated manifests
winget-create update dealer426.thresh --version 1.5.0 --urls https://github.com/dealer426/thresh/releases/download/v1.5.0/thresh-windows-x64.zip --submit
```

#### Resources
- **Repository**: https://github.com/microsoft/winget-pkgs
- **Documentation**: https://docs.microsoft.com/windows/package-manager/
- **Manifest Schema**: https://aka.ms/winget-manifest-schema

---

### Chocolatey

**Popular community-driven Windows package manager**

#### Prerequisites
- Chocolatey account: https://community.chocolatey.org/account/register
- API key from your Chocolatey account
- Package specification ready (already in `packages/chocolatey/`)

#### Submission Process

1. **Install Chocolatey tools**
   ```powershell
   choco install chocolatey.extension
   ```

2. **Test package locally**
   ```powershell
   cd packages/chocolatey
   choco pack thresh.nuspec
   
   # Test installation locally
   choco install thresh -s . --force
   ```

3. **Set API key** (one-time setup)
   ```powershell
   choco apikey --key YOUR-API-KEY --source https://push.chocolatey.org/
   ```

4. **Push package to Chocolatey**
   ```powershell
   choco push thresh.1.4.0.nupkg --source https://push.chocolatey.org/
   ```

5. **Package goes into moderation queue**
   - Automated tests run (virus scan, validation)
   - Human moderator reviews (for first submission)
   - Typically takes 24-72 hours for approval

6. **Once approved, package is live**
   - Users can install: `choco install thresh`

#### Updating for New Versions

1. **Update nuspec file**
   ```xml
   <!-- packages/chocolatey/thresh.nuspec -->
   <version>1.5.0</version>
   <releaseNotes>https://github.com/dealer426/thresh/releases/tag/v1.5.0</releaseNotes>
   ```

2. **Update install script**
   ```powershell
   # packages/chocolatey/tools/chocolateyinstall.ps1
   $url64 = 'https://github.com/dealer426/thresh/releases/download/v1.5.0/thresh-windows-x64.zip'
   ```

3. **Update checksums**
   ```powershell
   # Generate new SHA256 checksum
   certutil -hashfile thresh-windows-x64.zip SHA256
   ```

4. **Pack and push**
   ```powershell
   choco pack
   choco push thresh.1.5.0.nupkg --source https://push.chocolatey.org/
   ```

#### Automatic Moderation
After 3-5 trusted packages, you'll get automatic moderation (no human review needed).

#### Resources
- **Community Repository**: https://community.chocolatey.org/
- **Package Repository**: https://github.com/chocolatey-community/chocolatey-packages
- **Documentation**: https://docs.chocolatey.org/en-us/create/create-packages
- **Best Practices**: https://docs.chocolatey.org/en-us/create/create-packages-best-practices

---

### Scoop

**Command-line installer for Windows with minimal UAC prompts**

#### Prerequisites
- Package manifest ready (already in `packages/scoop/thresh.json`)
- GitHub account

#### Submission Process

1. **Fork the scoop-extras bucket** (for third-party apps)
   ```bash
   gh repo fork ScoopInstaller/Extras --clone
   cd Extras
   ```

2. **Create a new branch**
   ```bash
   git checkout -b add-thresh
   ```

3. **Add your manifest**
   ```bash
   cp /path/to/thresh/packages/scoop/thresh.json bucket/thresh.json
   ```

4. **Test locally**
   ```powershell
   # Install from local manifest
   scoop install bucket/thresh.json
   
   # Verify installation
   thresh --version
   
   # Uninstall
   scoop uninstall thresh
   ```

5. **Validate manifest**
   ```powershell
   # Install scoop's test tools
   scoop install checkver
   
   # Check manifest
   checkver -u thresh
   ```

6. **Commit and push**
   ```bash
   git add bucket/thresh.json
   git commit -m "thresh: Add version 1.4.0"
   git push origin add-thresh
   ```

7. **Create Pull Request**
   ```bash
   gh pr create --repo ScoopInstaller/Extras --title "thresh: Add version 1.4.0" --body "Cross-platform development environment manager with AI-powered blueprint generation

**Description**: thresh is a .NET 10 Native AOT command-line tool for provisioning container-based development environments using AI-generated blueprints.

**Homepage**: https://thresh.sh
**License**: MIT
**Platform**: Windows x64"
   ```

8. **Wait for review**
   - Maintainers will review and test
   - Usually approved within a few days
   - May request changes to manifest

#### Updating for New Versions

Scoop has **autoupdate** functionality. Update the autoupdate section in your manifest:

```json
{
  "autoupdate": {
    "url": "https://github.com/dealer426/thresh/releases/download/v$version/thresh-windows-x64.zip",
    "hash": {
      "url": "$url.sha256"
    }
  }
}
```

Or manually:
```bash
cd Extras
git checkout main
git pull upstream main
git checkout -b update-thresh-1.5.0
# Edit bucket/thresh.json with new version
git commit -am "thresh: Update to version 1.5.0"
git push origin update-thresh-1.5.0
gh pr create
```

#### Resources
- **Extras Bucket**: https://github.com/ScoopInstaller/Extras
- **Documentation**: https://github.com/ScoopInstaller/Scoop/wiki
- **Manifest Reference**: https://github.com/ScoopInstaller/Scoop/wiki/App-Manifests
- **Contribution Guide**: https://github.com/ScoopInstaller/.github/blob/main/.github/CONTRIBUTING.md

---

## macOS Package Managers

### Homebrew

**The most popular package manager for macOS**

#### Prerequisites
- Homebrew formula ready
- GitHub release with macOS binaries
- GitHub account

#### Submission Process

1. **Create a formula**
   ```bash
   cd /tmp
   cat > thresh.rb << 'EOF'
class Thresh < Formula
  desc "AI-powered cross-platform development environment manager"
  homepage "https://thresh.sh"
  url "https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-macos-arm64.tar.gz"
  sha256 "REPLACE_WITH_ACTUAL_SHA256"
  license "MIT"
  version "1.4.0"

  depends_on "containerd" => :optional

  def install
    bin.install "thresh"
  end

  test do
    system "#{bin}/thresh", "--version"
  end
end
EOF
   ```

2. **Generate SHA256**
   ```bash
   curl -L https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-macos-arm64.tar.gz | shasum -a 256
   ```

3. **Test formula locally**
   ```bash
   brew install --build-from-source ./thresh.rb
   thresh --version
   brew uninstall thresh
   ```

4. **For official Homebrew submission** (homebrew-core)
   
   **Note**: Homebrew-core has strict requirements:
   - Must be notable/popular software
   - Active development and maintenance
   - No pre-built binaries (must compile from source)
   - No dependencies on other taps

   For thresh (pre-built binary), it's better to use a **tap** instead.

5. **Create your own Homebrew Tap** (Recommended)

   ```bash
   # Create a tap repository
   gh repo create homebrew-tap --public --description "Homebrew tap for thresh"
   git clone https://github.com/dealer426/homebrew-tap.git
   cd homebrew-tap
   
   # Create Formula directory
   mkdir Formula
   cp /tmp/thresh.rb Formula/thresh.rb
   
   # Commit and push
   git add Formula/thresh.rb
   git commit -m "Add thresh formula"
   git push origin main
   ```

6. **Users can now install from your tap**
   ```bash
   brew tap dealer426/tap
   brew install thresh
   ```

#### Intel + ARM64 Universal Binary

For both architectures:

```ruby
class Thresh < Formula
  desc "AI-powered cross-platform development environment manager"
  homepage "https://thresh.sh"
  version "1.4.0"
  license "MIT"

  on_arm do
    url "https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-macos-arm64.tar.gz"
    sha256 "ARM64_SHA256_HERE"
  end

  on_intel do
    url "https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-macos-x64.tar.gz"
    sha256 "X64_SHA256_HERE"
  end

  def install
    bin.install "thresh"
  end

  test do
    assert_match version.to_s, shell_output("#{bin}/thresh --version")
  end
end
```

#### Updating for New Versions

```bash
cd homebrew-tap
git checkout main
git pull

# Update Formula/thresh.rb with new version and URLs
# Generate new SHA256s

git commit -am "thresh: Update to 1.5.0"
git push origin main
```

#### Auto-update with brew bump

```bash
brew bump-formula-pr --url=https://github.com/dealer426/thresh/releases/download/v1.5.0/thresh-macos-arm64.tar.gz thresh
```

#### Resources
- **Homebrew Docs**: https://docs.brew.sh/
- **Formula Cookbook**: https://docs.brew.sh/Formula-Cookbook
- **Creating Taps**: https://docs.brew.sh/How-to-Create-and-Maintain-a-Tap
- **Acceptable Formulae**: https://docs.brew.sh/Acceptable-Formulae

---

### MacPorts

**Alternative macOS package manager**

#### Prerequisites
- MacPorts Portfile
- GitHub account

#### Submission Process

1. **Create a Portfile**
   ```bash
   cat > Portfile << 'EOF'
# -*- coding: utf-8; mode: tcl; tab-width: 4; indent-tabs-mode: nil; c-basic-offset: 4 -*- vim:fenc=utf-8:ft=tcl:et:sw=4:ts=4:sts=4

PortSystem          1.0

name                thresh
version             1.4.0
categories          sysutils devel
platforms           darwin
license             MIT
maintainers         {@dealer426 github.com:dealer426} openmaintainer
description         AI-powered cross-platform development environment manager
long_description    {*}${description}. thresh is a Native AOT command-line tool \
                    for provisioning container-based development environments \
                    using AI-generated blueprints.

homepage            https://thresh.sh
master_sites        https://github.com/dealer426/thresh/releases/download/v${version}

distname            thresh-macos-arm64
dist_subdir         ${name}/${version}
use_tar.gz          yes

checksums           rmd160  RIPEMD160_HERE \
                    sha256  SHA256_HERE \
                    size    SIZE_HERE

depends_run         port:containerd

use_configure       no

build {}

destroot {
    xinstall -m 755 ${worksrcpath}/thresh ${destroot}${prefix}/bin/
}

test.run            yes
test.cmd            ${prefix}/bin/thresh
test.target         --version

livecheck.type      regex
livecheck.url       https://github.com/dealer426/thresh/releases
livecheck.regex     v(\[0-9.\]+)
EOF
   ```

2. **Test locally**
   ```bash
   sudo port install
   ```

3. **Submit to MacPorts**
   
   Fork and clone the ports repository:
   ```bash
   gh repo fork macports/macports-ports --clone
   cd macports-ports
   git checkout -b add-thresh
   ```

4. **Add Portfile**
   ```bash
   mkdir -p sysutils/thresh
   cp /tmp/Portfile sysutils/thresh/
   ```

5. **Create PR**
   ```bash
   git add sysutils/thresh/Portfile
   git commit -m "thresh: new port, version 1.4.0"
   git push origin add-thresh
   gh pr create --repo macports/macports-ports
   ```

#### Resources
- **MacPorts Guide**: https://guide.macports.org/
- **Contributing**: https://trac.macports.org/wiki/UsingGit
- **Portfile Reference**: https://guide.macports.org/chunked/reference.html

---

## Linux Package Managers

### Snap Store

**Universal Linux package manager by Canonical**

#### Prerequisites
- Ubuntu One account: https://login.ubuntu.com/
- Snapcraft installed: `sudo snap install snapcraft --classic`

#### Submission Process

1. **Create snapcraft.yaml**
   ```bash
   mkdir -p thresh-snap
   cd thresh-snap
   snapcraft init
   ```

2. **Edit snap/snapcraft.yaml**
   ```yaml
   name: thresh
   base: core22
   version: '1.4.0'
   summary: AI-powered development environment manager
   description: |
     thresh is a cross-platform CLI tool for provisioning container-based 
     development environments using AI-generated blueprints. Supports 
     Windows/WSL2, Linux/Docker/nerdctl, and macOS/containerd.
   
   grade: stable
   confinement: classic  # classic for full system access
   
   architectures:
     - build-on: amd64
   
   parts:
     thresh:
       plugin: dump
       source: https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-linux-x64.tar.gz
       stage:
         - thresh
   
   apps:
     thresh:
       command: thresh
       plugs:
         - network
         - home
         - docker  # For Docker integration
   ```

3. **Build the snap**
   ```bash
   snapcraft
   ```

4. **Test locally**
   ```bash
   sudo snap install thresh_1.4.0_amd64.snap --dangerous --classic
   thresh --version
   ```

5. **Login to Snap Store**
   ```bash
   snapcraft login
   ```

6. **Register the name** (one-time)
   ```bash
   snapcraft register thresh
   ```

7. **Upload to Snap Store**
   ```bash
   snapcraft upload thresh_1.4.0_amd64.snap --release=stable
   ```

8. **Users can now install**
   ```bash
   sudo snap install thresh --classic
   ```

#### Automatic Updates

Create a GitHub Action to auto-publish:

```yaml
name: Publish to Snap Store

on:
  release:
    types: [published]

jobs:
  publish-snap:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: snapcore/action-build@v1
      - uses: snapcore/action-publish@v1
        with:
          snap: thresh_${{ github.ref_name }}_amd64.snap
          release: stable
        env:
          SNAPCRAFT_STORE_CREDENTIALS: ${{ secrets.SNAPCRAFT_TOKEN }}
```

#### Resources
- **Snapcraft**: https://snapcraft.io/
- **Documentation**: https://snapcraft.io/docs
- **Dashboard**: https://dashboard.snapcraft.io/

---

### Flatpak (Flathub)

**Universal Linux app distribution**

#### Prerequisites
- Flathub account
- Flatpak manifest

#### Submission Process

1. **Create Flatpak manifest** (`io.github.dealer426.thresh.yml`)
   ```yaml
   app-id: io.github.dealer426.thresh
   runtime: org.freedesktop.Platform
   runtime-version: '23.08'
   sdk: org.freedesktop.Sdk
   command: thresh
   
   finish-args:
     - --share=network
     - --filesystem=home
     - --socket=system-bus    # For Docker/containerd
   
   modules:
     - name: thresh
       buildsystem: simple
       build-commands:
         - install -D thresh /app/bin/thresh
       sources:
         - type: archive
           url: https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-linux-x64.tar.gz
           sha256: SHA256_HERE
   ```

2. **Test locally**
   ```bash
   flatpak-builder --force-clean build-dir io.github.dealer426.thresh.yml
   flatpak-builder --user --install --force-clean build-dir io.github.dealer426.thresh.yml
   flatpak run io.github.dealer426.thresh
   ```

3. **Fork Flathub repository**
   ```bash
   gh repo fork flathub/flathub --clone
   cd flathub
   ```

4. **Create app repository**
   ```bash
   mkdir io.github.dealer426.thresh
   cp /path/to/io.github.dealer426.thresh.yml io.github.dealer426.thresh/
   ```

5. **Submit to Flathub**
   - Follow Flathub submission process: https://github.com/flathub/flathub/wiki/App-Submission

#### Resources
- **Flathub**: https://flathub.org/
- **Documentation**: https://docs.flatpak.org/
- **Submission**: https://github.com/flathub/flathub/wiki/App-Submission

---

### AppImage

**Portable Linux application format**

#### Prerequisites
- AppImage build tools

#### Creation Process

1. **Install appimagetool**
   ```bash
   wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
   chmod +x appimagetool-x86_64.AppImage
   ```

2. **Create AppDir structure**
   ```bash
   mkdir -p thresh.AppDir/usr/bin
   mkdir -p thresh.AppDir/usr/share/applications
   mkdir -p thresh.AppDir/usr/share/icons/hicolor/256x256/apps
   
   # Copy binary
   cp thresh thresh.AppDir/usr/bin/
   ```

3. **Create desktop file** (`thresh.AppDir/usr/share/applications/thresh.desktop`)
   ```ini
   [Desktop Entry]
   Type=Application
   Name=thresh
   Exec=thresh
   Icon=thresh
   Categories=Development;Utility;
   Terminal=true
   ```

4. **Create AppRun** (`thresh.AppDir/AppRun`)
   ```bash
   #!/bin/bash
   SELF=$(readlink -f "$0")
   HERE=${SELF%/*}
   exec "${HERE}/usr/bin/thresh" "$@"
   ```

5. **Make executable**
   ```bash
   chmod +x thresh.AppDir/AppRun
   ```

6. **Build AppImage**
   ```bash
   ./appimagetool-x86_64.AppImage thresh.AppDir thresh-1.4.0-x86_64.AppImage
   ```

7. **Distribute**
   - Upload to GitHub releases
   - List on AppImageHub: https://appimage.github.io/

#### Resources
- **AppImage**: https://appimage.org/
- **Documentation**: https://docs.appimage.org/
- **AppImageHub**: https://www.appimagehub.com/

---

### Debian/Ubuntu (APT)

**For .deb packages**

#### Prerequisites
- Debian packaging tools

#### Creation Process

1. **Install build tools**
   ```bash
   sudo apt install build-essential devscripts debhelper
   ```

2. **Create package structure**
   ```bash
   mkdir -p thresh-1.4.0/DEBIAN
   mkdir -p thresh-1.4.0/usr/local/bin
   
   # Copy binary
   cp thresh thresh-1.4.0/usr/local/bin/
   chmod +x thresh-1.4.0/usr/local/bin/thresh
   ```

3. **Create control file** (`thresh-1.4.0/DEBIAN/control`)
   ```
   Package: thresh
   Version: 1.4.0
   Section: utils
   Priority: optional
   Architecture: amd64
   Depends: docker.io | containerd
   Maintainer: Your Name <your.email@example.com>
   Description: AI-powered development environment manager
    thresh is a cross-platform CLI tool for provisioning container-based
    development environments using AI-generated blueprints.
   Homepage: https://thresh.sh
   ```

4. **Build .deb package**
   ```bash
   dpkg-deb --build thresh-1.4.0
   ```

5. **Test installation**
   ```bash
   sudo dpkg -i thresh-1.4.0.deb
   thresh --version
   ```

6. **For official repos** - Submit to:
   - **Ubuntu PPA**: https://launchpad.net/
   - **Debian mentors**: https://mentors.debian.net/

#### Create PPA (Ubuntu)

1. **Create Launchpad account**: https://launchpad.net/
2. **Import GPG key**: https://launchpad.net/~/+editpgpkeys
3. **Create PPA**: https://launchpad.net/~/+activate-ppa
4. **Upload source package**:
   ```bash
   dput ppa:dealer426/thresh thresh_1.4.0_source.changes
   ```

#### Resources
- **Debian Packaging**: https://www.debian.org/doc/manuals/maint-guide/
- **Ubuntu PPA**: https://help.launchpad.net/Packaging/PPA

---

### Fedora/RHEL (DNF/YUM)

**For .rpm packages**

#### Prerequisites
- RPM build tools

#### Creation Process

1. **Install tools**
   ```bash
   sudo dnf install rpm-build rpmdevtools
   ```

2. **Setup build environment**
   ```bash
   rpmdev-setuptree
   ```

3. **Create spec file** (`~/rpmbuild/SPECS/thresh.spec`)
   ```spec
   Name:           thresh
   Version:        1.4.0
   Release:        1%{?dist}
   Summary:        AI-powered development environment manager
   
   License:        MIT
   URL:            https://thresh.sh
   Source0:        https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-linux-x64.tar.gz
   
   Requires:       docker
   
   %description
   thresh is a cross-platform CLI tool for provisioning container-based
   development environments using AI-generated blueprints.
   
   %prep
   %setup -q -n thresh-linux-x64
   
   %install
   mkdir -p %{buildroot}%{_bindir}
   install -m 755 thresh %{buildroot}%{_bindir}/thresh
   
   %files
   %{_bindir}/thresh
   
   %changelog
   * Tue Feb 17 2026 Your Name <your.email@example.com> - 1.4.0-1
   - Initial package
   ```

4. **Download source**
   ```bash
   cd ~/rpmbuild/SOURCES
   wget https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-linux-x64.tar.gz
   ```

5. **Build RPM**
   ```bash
   cd ~/rpmbuild/SPECS
   rpmbuild -ba thresh.spec
   ```

6. **Install and test**
   ```bash
   sudo dnf install ~/rpmbuild/RPMS/x86_64/thresh-1.4.0-1.*.rpm
   thresh --version
   ```

7. **Submit to Fedora** (optional)
   - https://docs.fedoraproject.org/en-US/package-maintainers/

#### Resources
- **RPM Packaging Guide**: https://rpm-packaging-guide.github.io/
- **Fedora Package**: https://docs.fedoraproject.org/en-US/package-maintainers/

---

### Arch Linux (AUR)

**Arch User Repository**

#### Prerequisites
- AUR account: https://aur.archlinux.org/register/
- SSH key uploaded

#### Submission Process

1. **Create PKGBUILD**
   ```bash
   cat > PKGBUILD << 'EOF'
   # Maintainer: Your Name <your.email@example.com>
   
   pkgname=thresh
   pkgver=1.4.0
   pkgrel=1
   pkgdesc="AI-powered development environment manager"
   arch=('x86_64')
   url="https://thresh.sh"
   license=('MIT')
   depends=('docker' 'containerd')
   source=("https://github.com/dealer426/thresh/releases/download/v${pkgver}/thresh-linux-x64.tar.gz")
   sha256sums=('SHA256_HERE')
   
   package() {
       install -Dm755 "${srcdir}/thresh" "${pkgdir}/usr/bin/thresh"
   }
   EOF
   ```

2. **Test build**
   ```bash
   makepkg -si
   ```

3. **Create .SRCINFO**
   ```bash
   makepkg --printsrcinfo > .SRCINFO
   ```

4. **Initialize AUR repo**
   ```bash
   git clone ssh://aur@aur.archlinux.org/thresh.git
   cd thresh
   ```

5. **Add files and push**
   ```bash
   cp /path/to/PKGBUILD .
   cp /path/to/.SRCINFO .
   git add PKGBUILD .SRCINFO
   git commit -m "Initial commit: thresh 1.4.0"
   git push
   ```

6. **Users can now install**
   ```bash
   yay -S thresh
   # or
   paru -S thresh
   ```

#### Updating

```bash
cd thresh
# Update PKGBUILD with new version
makepkg --printsrcinfo > .SRCINFO
git commit -am "Update to 1.5.0"
git push
```

#### Resources
- **AUR Submission**: https://wiki.archlinux.org/title/AUR_submission_guidelines
- **PKGBUILD**: https://wiki.archlinux.org/title/PKGBUILD

---

## Cross-Platform

### Nix/NixOS

**Reproducible package manager for Linux and macOS**

#### Prerequisites
- Nix installed
- nixpkgs fork

#### Submission Process

1. **Create derivation** (`thresh.nix`)
   ```nix
   { lib
   , stdenv
   , fetchurl
   }:
   
   stdenv.mkDerivation rec {
     pname = "thresh";
     version = "1.4.0";
   
     src = fetchurl {
       url = "https://github.com/dealer426/thresh/releases/download/v${version}/thresh-linux-x64.tar.gz";
       sha256 = "SHA256_HERE";
     };
   
     installPhase = ''
       mkdir -p $out/bin
       cp thresh $out/bin/
       chmod +x $out/bin/thresh
     '';
   
     meta = with lib; {
       description = "AI-powered development environment manager";
       homepage = "https://thresh.sh";
       license = licenses.mit;
       platforms = platforms.linux;
       maintainers = [ maintainers.dealer426 ];
     };
   }
   ```

2. **Test locally**
   ```bash
   nix-build -E 'with import <nixpkgs> {}; callPackage ./thresh.nix {}'
   ```

3. **Submit to nixpkgs**
   ```bash
   gh repo fork NixOS/nixpkgs --clone
   cd nixpkgs
   git checkout -b add-thresh
   ```

4. **Add to pkgs/by-name**
   ```bash
   mkdir -p pkgs/by-name/th/thresh
   cp /path/to/thresh.nix pkgs/by-name/th/thresh/package.nix
   ```

5. **Create PR**
   ```bash
   git add pkgs/by-name/th/thresh/package.nix
   git commit -m "thresh: init at 1.4.0"
   git push origin add-thresh
   gh pr create --repo NixOS/nixpkgs
   ```

#### Resources
- **Nixpkgs**: https://github.com/NixOS/nixpkgs
- **Contributing**: https://github.com/NixOS/nixpkgs/blob/master/CONTRIBUTING.md

---

### asdf Version Manager

**Multi-language version manager**

#### Create an asdf Plugin

1. **Create plugin repository**
   ```bash
   gh repo create asdf-thresh --public
   git clone https://github.com/dealer426/asdf-thresh.git
   cd asdf-thresh
   ```

2. **Create bin/install**
   ```bash
   #!/usr/bin/env bash
   set -e
   
   install_thresh() {
     local version=$1
     local install_path=$2
     
     local platform=$(uname -s | tr '[:upper:]' '[:lower:]')
     local arch=$(uname -m)
     
     if [ "$platform" = "darwin" ]; then
       local url="https://github.com/dealer426/thresh/releases/download/v${version}/thresh-macos-arm64.tar.gz"
     else
       local url="https://github.com/dealer426/thresh/releases/download/v${version}/thresh-linux-x64.tar.gz"
     fi
     
     mkdir -p "$install_path/bin"
     curl -L "$url" | tar xz -C "$install_path/bin"
     chmod +x "$install_path/bin/thresh"
   }
   
   install_thresh "$ASDF_INSTALL_VERSION" "$ASDF_INSTALL_PATH"
   ```

3. **Create bin/list-all**
   ```bash
   #!/usr/bin/env bash
   curl -s https://api.github.com/repos/dealer426/thresh/releases | \
     grep -oP '"tag_name": "v\K(.*)(?=")' | \
     tr '\n' ' '
   ```

4. **Make executable**
   ```bash
   chmod +x bin/install bin/list-all
   ```

5. **Register plugin**
   https://github.com/asdf-vm/asdf-plugins

#### Resources
- **Plugin Guide**: https://asdf-vm.com/plugins/create.html

---

## Container Registries

### Docker Hub

**For containerized distribution**

#### Prerequisites
- Docker Hub account
- Docker installed

#### Process

1. **Create Dockerfile**
   ```dockerfile
   FROM alpine:latest
   COPY thresh /usr/local/bin/thresh
   RUN chmod +x /usr/local/bin/thresh
   ENTRYPOINT ["thresh"]
   CMD ["--help"]
   ```

2. **Build and tag**
   ```bash
   docker build -t dealer426/thresh:1.4.0 .
   docker tag dealer426/thresh:1.4.0 dealer426/thresh:latest
   ```

3. **Push to Docker Hub**
   ```bash
   docker login
   docker push dealer426/thresh:1.4.0
   docker push dealer426/thresh:latest
   ```

4. **Users can run**
   ```bash
   docker run dealer426/thresh:latest --version
   ```

---

### GitHub Container Registry

**GitHub's container registry**

#### Process

1. **Login**
   ```bash
   echo $GITHUB_TOKEN | docker login ghcr.io -u dealer426 --password-stdin
   ```

2. **Tag and push**
   ```bash
   docker tag dealer426/thresh:1.4.0 ghcr.io/dealer426/thresh:1.4.0
   docker push ghcr.io/dealer426/thresh:1.4.0
   ```

---

## Automation Recommendations

### GitHub Actions for Automated Publishing

Create `.github/workflows/publish-packages.yml`:

```yaml
name: Publish to Package Managers

on:
  release:
    types: [published]

jobs:
  publish-chocolatey:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - name: Publish to Chocolatey
        run: |
          choco apikey --key ${{ secrets.CHOCOLATEY_API_KEY }} --source https://push.chocolatey.org/
          choco pack packages/chocolatey/thresh.nuspec
          choco push thresh.${{ github.ref_name }}.nupkg --source https://push.chocolatey.org/

  publish-snap:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: snapcore/action-build@v1
      - uses: snapcore/action-publish@v1
        env:
          SNAPCRAFT_STORE_CREDENTIALS: ${{ secrets.SNAPCRAFT_TOKEN }}

  publish-docker:
    runs-on: ubuntu-latest
    steps:
      - uses: docker/login-action@v3
        with:
          username: ${{ secrets.DOCKER_USERNAME }}
          password: ${{ secrets.DOCKER_PASSWORD }}
      - uses: docker/build-push-action@v5
        with:
          push: true
          tags: |
            dealer426/thresh:${{ github.ref_name }}
            dealer426/thresh:latest
```

---

## Quick Reference Table

| Platform | Package Manager | Auto-Update | Approval Time | Popularity |
|----------|----------------|-------------|---------------|------------|
| **Windows** | Winget | ✅ | 24-48h | ⭐⭐⭐⭐⭐ |
| **Windows** | Chocolatey | ❌ | 24-72h | ⭐⭐⭐⭐⭐ |
| **Windows** | Scoop | ✅ | Few days | ⭐⭐⭐⭐ |
| **macOS** | Homebrew | ✅ | Instant (tap) | ⭐⭐⭐⭐⭐ |
| **macOS** | MacPorts | ❌ | Weeks | ⭐⭐⭐ |
| **Linux** | Snap | ❌ | Hours | ⭐⭐⭐⭐ |
| **Linux** | Flatpak | ❌ | Days | ⭐⭐⭐⭐ |
| **Linux** | AppImage | N/A | Instant | ⭐⭐⭐ |
| **Linux** | APT (PPA) | ❌ | Days | ⭐⭐⭐⭐⭐ |
| **Linux** | AUR | Manual | Hours | ⭐⭐⭐⭐ |
| **Cross** | Nix | ❌ | Weeks | ⭐⭐⭐ |
| **Cross** | Docker Hub | ❌ | Instant | ⭐⭐⭐⭐⭐ |

---

## Recommended Priority

### Phase 1 (Immediate - High Impact)
1. **Chocolatey** - Most popular Windows package manager
2. **Homebrew Tap** - Easy to maintain, instant publishing
3. **Scoop** - Growing Windows user base
4. **Snap** - Wide Linux distribution reach

### Phase 2 (Short-term)
5. **Winget** - Official Microsoft, growing adoption
6. **AUR** - Easy submission, Arch users expect it
7. **Docker Hub** - For containerized workflows

### Phase 3 (Long-term)
8. **Flatpak** - Universal Linux
9. **APT (Ubuntu PPA)** - Debian/Ubuntu users
10. **Nix** - Reproducible builds community

---

## Support and Maintenance

After initial submission, you'll need to:

1. **Monitor issues** on each package manager platform
2. **Update package manifests** for each new release
3. **Respond to maintainer feedback**
4. **Keep checksums/hashes updated**
5. **Test on each platform** before submission

Consider automating updates with GitHub Actions where possible.

---

## Additional Resources

- **Package Manager Comparison**: https://repology.org/
- **Cross-platform packaging**: https://github.com/goreleaser/goreleaser
- **Release automation**: https://semantic-release.gitbook.io/

---

**Questions?** Open an issue at https://github.com/dealer426/thresh/issues
