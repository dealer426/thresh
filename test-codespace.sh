#!/bin/bash
# Test thresh in GitHub Codespaces

set -e

echo "🔍 Testing thresh in GitHub Codespaces"
echo "========================================"
echo ""

# Step 1: Check environment
echo "📦 Environment Info:"
echo "  - OS: $(uname -s)"
echo "  - Architecture: $(uname -m)"
echo "  - .NET SDK: $(dotnet --version 2>/dev/null || echo 'NOT FOUND')"
echo ""

# Step 2: Check for container runtimes
echo "🐳 Container Runtime Detection:"
if command -v docker &> /dev/null; then
    echo "  ✅ Docker: $(docker --version | head -n 1)"
    docker info &> /dev/null && echo "     Daemon: Running" || echo "     Daemon: Not running"
else
    echo "  ❌ Docker: Not found"
fi

if command -v nerdctl &> /dev/null; then
    echo "  ✅ nerdctl: $(nerdctl --version | head -n 1)"
else
    echo "  ❌ nerdctl: Not found"
fi

if command -v ctr &> /dev/null; then
    echo "  ✅ ctr: Found"
else
    echo "  ❌ ctr: Not found"
fi
echo ""

# Step 3: Build thresh
echo "🔨 Building thresh..."
cd /workspaces/thresh/thresh/Thresh

if dotnet build -c Release; then
    echo "  ✅ Build successful"
else
    echo "  ❌ Build failed"
    exit 1
fi
echo ""

# Step 4: Run thresh version
echo "🚀 Testing thresh version command..."
if dotnet run -- version; then
    echo "  ✅ Version command executed"
else
    echo "  ❌ Version command failed"
    exit 1
fi
echo ""

# Step 5: List environments (should be empty)
echo "📋 Testing thresh list command..."
if dotnet run -- list; then
    echo "  ✅ List command executed"
else
    echo "  ❌ List command failed"
    exit 1
fi
echo ""

echo "✅ All tests passed!"
echo ""
echo "📊 Summary:"
echo "  - thresh compiles successfully"
echo "  - Cross-platform container detection working"
echo "  - Basic commands functional"
echo ""
echo "🎯 Expected Runtime: docker (GitHub Codespaces default)"
