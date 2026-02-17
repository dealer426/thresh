---
slug: thresh-macos-beta
title: macOS Support is Here - Beta Testers Wanted!
authors: [thresh]
tags: [macos, beta, apple-silicon, testing]
---

We're excited to announce **beta support for macOS** in thresh 1.4.0! If you have an Apple Silicon Mac (M1, M2, M3), we need your help testing thresh on macOS.

<!-- truncate -->

## 🍎 macOS Support Status

thresh 1.4.0 includes **beta-quality macOS support** for Apple Silicon (ARM64):

- ✅ **Builds successfully** on macOS ARM64
- ✅ **Basic functionality** working (environment creation, blueprint management)
- ✅ **containerd integration** functional
- ✅ **Docker Desktop** compatibility
- ⚠️ **Limited real-world testing** - This is where you come in!

### Why Beta?

Unlike Windows (WSL2) and Linux (where we have extensive CI/CD testing), macOS testing has been limited to:
- Local development environment
- Automated builds via GitHub Actions (macOS runner)
- Basic smoke tests

We don't have access to a variety of macOS configurations, which is why we're calling on the community.

## 🛠️ What Works

Based on our testing, the following features are **confirmed working** on macOS:

### ✅ Core Functionality
- Environment provisioning with `thresh up`
- Blueprint management (`list`, `generate`, `delete`)
- System metrics
- Container lifecycle management
- AI-powered blueprint generation (with GitHub Copilot)

### ✅ Supported Runtimes
- **containerd** - Recommended for macOS
- **Docker Desktop** - Also tested and working

### ✅ Binary Distribution
- 13 MB native macOS ARM64 binary
- No UPX compression (to preserve Apple code signing)
- Single-file distribution
- No .NET runtime required

## ❓ What We Need Help Testing

We're looking for macOS users to test:

### Configuration Testing
- Different macOS versions (Sonoma, Ventura, Monterey)
- Different Apple Silicon chips (M1, M2, M3)
- containerd vs Docker Desktop
- Various Docker Desktop versions

### Feature Testing
- Blueprint generation with complex requirements
- Parallel environment creation
- Long-running environments (stability)
- MCP server integration with VS Code/Cursor
- Memory usage and performance

### Edge Cases
- Low disk space scenarios
- Network connectivity issues
- Container restart behavior
- Cleanup and garbage collection

## 📦 Installation on macOS

### Prerequisites

1. **Apple Silicon Mac** (M1, M2, M3)
2. **macOS 12.0 (Monterey) or later**
3. **Container runtime**:
   - **Option A:** containerd (recommended)
     ```bash
     brew install containerd
     sudo containerd
     ```
   - **Option B:** Docker Desktop
     ```bash
     brew install --cask docker
     # Launch Docker Desktop
     ```
4. **GitHub CLI + GitHub Copilot CLI** (for AI features):
   ```bash
   # Install GitHub CLI
   brew install gh
   
   # Authenticate
   gh auth login
   
   # Install Copilot CLI extension
   gh extension install github/gh-copilot
   ```
   
   **Info:** https://github.com/features/copilot/cli

### Download and Install

```bash
# Download thresh for macOS ARM64
curl -L https://github.com/dealer426/thresh/releases/download/v1.4.0/thresh-macos-arm64.tar.gz -o thresh.tar.gz

# Extract
tar -xzf thresh.tar.gz

# Install to /usr/local/bin
sudo mv thresh /usr/local/bin/
sudo chmod +x /usr/local/bin/thresh

# Verify installation
thresh version
# Output:
# thresh v1.4.0
# Platform: macOS
# Runtime: containerd (or Docker)
# .NET: 10.0
```

## 🧪 Testing Guide

### Basic Smoke Test

```bash
# 1. Verify installation
thresh version

# 2. List built-in blueprints
thresh blueprint list

# 3. Create a simple environment
thresh up alpine-minimal

# 4. Verify it's running
thresh list

# 5. Check metrics
thresh metrics

# 6. Clean up
thresh destroy alpine-minimal -y
```

### Advanced Testing

```bash
# Test AI blueprint generation
thresh blueprint generate "Python ML with Jupyter and scikit-learn" --output python-ml

# Test parallel creation
thresh up alpine-minimal python-dev node-dev

# Test MCP server
thresh serve --port 8080
# (Ctrl+C to stop)

# Test complex blueprint
thresh up ubuntu-dev

# Check long-term stability
thresh metrics --json > metrics.json
```

## 📝 How to Report Issues

If you encounter issues, please report them on GitHub:

### Issue Template

```markdown
**Environment:**
- macOS version: (e.g., Sonoma 14.2)
- Apple chip: (e.g., M2 Pro)
- Container runtime: (containerd or Docker Desktop)
- Docker Desktop version: (if applicable)
- thresh version: 1.4.0

**Issue Description:**
(What went wrong?)

**Steps to Reproduce:**
1. 
2. 
3. 

**Expected Behavior:**
(What should have happened?)

**Actual Behavior:**
(What actually happened?)

**Logs:**
```bash
thresh version
thresh up <blueprint> --verbose
```

**Additional Context:**
(Screenshots, error messages, etc.)
```

### Where to Report

- **GitHub Issues**: https://github.com/dealer426/thresh/issues
- **Tag**: `macos` and `beta`
- **Template**: Use "macOS Beta Testing" template

## 🎯 Success Stories

Have you successfully used thresh on macOS? We'd love to hear about it!

Share your experience:
- Twitter/X: [@dealer426](https://twitter.com/dealer426)
- GitHub Discussions: https://github.com/dealer426/thresh/discussions
- Tag: `#thresh #macOS`

## ⚠️ Known Limitations

Based on limited testing, we're aware of:

1. **Binary Size**: 13 MB (vs 5 MB on Windows/Linux)
   - Reason: No UPX compression to preserve code signing
   - Working on: Alternative compression methods

2. **ARM64 Only**: Intel Macs not supported
   - Apple Silicon is the future
   - Intel support may come later based on demand

3. **containerd Setup**: Requires manual installation
   - Investigating: Homebrew formula with dependencies

## 🗺️ Roadmap for macOS

### Short-term (v1.5.0)
- [ ] Address beta feedback
- [ ] Improve error messages for macOS-specific issues
- [ ] Better containerd setup documentation
- [ ] Performance optimizations

### Long-term (v2.0.0)
- [ ] Intel Mac support (if demand exists)
- [ ] Homebrew cask with automatic containerd setup
- [ ] macOS-specific optimizations
- [ ] Native macOS UI (maybe?)

## 🙏 Thank You!

We're incredibly excited to bring thresh to the macOS community. Your testing and feedback will make macOS support production-ready in future releases.

Every bug report, feature request, and success story helps us improve thresh for everyone.

## 📚 Resources

- [macOS Installation Guide](https://thresh.sh/docs/installation#macos)
- [Troubleshooting Guide](https://thresh.sh/docs/intro#troubleshooting)
- [GitHub Repository](https://github.com/dealer426/thresh)
- [Report Issues](https://github.com/dealer426/thresh/issues/new)

---

**Download, test, and let us know how it goes!** 🍎🚀

*P.S. - If you're interested in contributing to macOS support, we're looking for maintainers familiar with containerd and macOS development. Reach out via GitHub!*
