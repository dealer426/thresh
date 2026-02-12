---
slug: software-supply-chain-security-sbom
title: Software Supply Chain Security - Why thresh Publishes SBOM with Every Release
authors: [thresh]
tags: [security, sbom, features]
---

In an era of increasing software supply chain attacks, transparency and trust are paramount. That's why **thresh publishes a Software Bill of Materials (SBOM)** with every release, giving you complete visibility into our dependencies and supply chain.

<!-- truncate -->

## 🔍 What is an SBOM?

A **Software Bill of Materials (SBOM)** is like an ingredients label for software. It's a formal record containing the details and supply chain relationships of the components used in building software.

Think of it as a detailed inventory that includes:

- All direct and transitive dependencies
- Exact version numbers and package identifiers
- Component licenses and origins
- Cryptographic hashes for verification

## 🛡️ Why SBOM Matters

### Vulnerability Management

When a security vulnerability is discovered (like Log4Shell), organizations need to quickly determine: **"Are we affected?"**

With an SBOM, you can:
- Instantly identify if vulnerable components are present
- Determine which versions of thresh are affected
- Make informed decisions about updates and patches

### Supply Chain Transparency

Modern software relies on dozens or hundreds of dependencies. SBOM provides:
- **Full visibility** into what's running in your environment
- **Trust verification** - confirm what we say we ship matches what you receive
- **License compliance** - understand all licenses in the dependency tree

### Regulatory Compliance

Organizations in regulated industries increasingly require SBOMs:
- **U.S. Executive Order 14028** mandates SBOMs for federal software
- **EU Cyber Resilience Act** emphasizes software supply chain security
- Many enterprises now require SBOMs from vendors

## 📦 thresh's SBOM Implementation

### What We Include

Every thresh release includes a comprehensive SBOM in **SPDX format**:

```json
{
  "bomFormat": "CycloneDX",
  "specVersion": "1.4",
  "version": 1,
  "components": [
    {
      "name": "GitHub.Copilot.SDK",
      "version": "0.1.23-preview.1",
      "purl": "pkg:nuget/GitHub.Copilot.SDK@0.1.23-preview.1"
    },
    {
      "name": "System.CommandLine",
      "version": "2.0.0-beta4.22272.1",
      "purl": "pkg:nuget/System.CommandLine@2.0.0-beta4.22272.1"
    }
    // ... all dependencies
  ]
}
```

### Our Minimal Dependency Surface

thresh v1.3.0 has an exceptionally **small dependency footprint**:

**Direct Dependencies (3):**
1. `GitHub.Copilot.SDK` 0.1.23-preview.1 - AI functionality
2. `System.CommandLine` 2.0.0-beta4.22272.1 - CLI framework
3. `System.Security.Cryptography.ProtectedData` 8.0.0 - Secure storage

**Why Small Matters:**
- Fewer dependencies = smaller attack surface
- Easier security audits
- Faster vulnerability response
- More transparent supply chain

### Version 1.3.0 Improvements

We recently **reduced dependencies from 6 to 3** by removing:
- ❌ OpenAI SDK (moved to GitHub Copilot SDK)
- ❌ Azure OpenAI SDK (consolidated)
- ❌ Microsoft.Extensions.Configuration (simplified)

This 50% reduction in dependencies means:
- **Smaller attack surface** - fewer components to monitor
- **Faster builds** - less to download and compile
- **Easier audits** - clearer dependency tree

## 📥 Accessing Our SBOM

### Download from GitHub Releases

Every thresh release includes `sbom.json`:

```powershell
# Download SBOM for v1.3.0
Invoke-WebRequest -Uri "https://github.com/dealer426/thresh/releases/download/v1.3.0/sbom.json" -OutFile "sbom.json"

# View contents
Get-Content sbom.json | ConvertFrom-Json
```

### Automated Verification

Integrate SBOM checking into your CI/CD:

```yaml
# Example GitHub Action
- name: Download and verify SBOM
  run: |
    curl -LO https://github.com/dealer426/thresh/releases/latest/download/sbom.json
    # Use tools like syft, grype, or your vulnerability scanner
    grype sbom:sbom.json
```

### SBOM Format

We use **CycloneDX 1.4** format:
- Industry-standard SBOM specification
- Widely supported by security tools
- Machine-readable JSON format
- Includes package URLs (purls) for precise identification

## 🔐 How We Generate SBOMs

Our SBOM generation is fully automated in our build pipeline:

```yaml
# .github/workflows/release.yml
- name: Generate SBOM
  run: |
    dotnet tool install --global CycloneDX
    dotnet CycloneDX thresh.csproj -o sbom.json
    
- name: Upload SBOM to Release
  uses: actions/upload-release-asset@v1
  with:
    asset_path: ./sbom.json
    asset_name: sbom.json
```

**Benefits of automated generation:**
- ✅ Always up-to-date with actual dependencies
- ✅ No manual maintenance required
- ✅ Consistent format across releases
- ✅ Generated from source of truth (project file)

## 🎯 Use Cases for thresh SBOM

### 1. Vulnerability Scanning

```powershell
# Scan thresh SBOM for known vulnerabilities
grype sbom:sbom.json --scope all-layers
```

### 2. License Compliance

```powershell
# Check all licenses in dependency tree
syft sbom.json -o json | jq '.artifacts[].metadata.licenses'
```

### 3. Policy Enforcement

```powershell
# Verify no GPL-licensed dependencies
cat sbom.json | jq '.components[] | select(.licenses[].license.id | contains("GPL"))'
```

### 4. Change Tracking

Compare SBOMs between versions to see dependency updates:

```powershell
# Diff v1.2.0 and v1.3.0
diff sbom-1.2.0.json sbom-1.3.0.json
```

## 📊 Our Commitment to Security

Publishing SBOMs is part of our broader security commitment:

- ✅ **Automated dependency updates** via Dependabot
- ✅ **Regular security scanning** in CI/CD
- ✅ **Minimal dependency footprint** (3 direct dependencies)
- ✅ **Native AOT compilation** - no runtime dependencies
- ✅ **Reproducible builds** - same source = same binary
- ✅ **Signed releases** (coming soon)

## 🚀 What's Next

We're continuously improving our security posture:

- **Signed binaries** - Code signing for Windows executables
- **Provenance attestation** - SLSA compliance for build provenance
- **Enhanced SBOM** - Include vulnerability data in SBOM
- **Automated notifications** - Alert users to vulnerable versions

## 📚 Learn More

- [Download thresh v1.3.0 with SBOM](https://github.com/dealer426/thresh/releases/tag/v1.3.0)
- [CycloneDX SBOM Specification](https://cyclonedx.org/)
- [NTIA SBOM Resources](https://www.ntia.gov/sbom)
- [CISA SBOM Guide](https://www.cisa.gov/sbom)

## 💬 We Want Your Feedback

How do you use SBOMs in your organization? What additional security features would you like to see in thresh?

Share your thoughts on [GitHub Discussions](https://github.com/dealer426/thresh/discussions) or open an issue with suggestions.

---

**Security and transparency are foundational to thresh. Download our latest release and inspect the SBOM for yourself!**
