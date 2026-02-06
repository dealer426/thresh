#!/bin/bash
# Test which container runtimes are available in this environment

echo "🔍 Checking for container runtimes..."
echo ""

if command -v nerdctl &> /dev/null; then
    echo "✅ nerdctl found:"
    nerdctl version | head -n 3
    echo ""
else
    echo "❌ nerdctl not found"
    echo ""
fi

if command -v docker &> /dev/null; then
    echo "✅ docker found:"
    docker --version
    docker info 2>&1 | grep -E "Server Version|Operating System|Kernel" || echo "  (daemon may not be running)"
    echo ""
else
    echo "❌ docker not found"
    echo ""
fi

if command -v ctr &> /dev/null; then
    echo "✅ ctr found:"
    ctr version | head -n 3
    echo ""
else
    echo "❌ ctr not found"
    echo ""
fi

echo "🎯 Thresh will use the first available tool in this order: nerdctl → docker → ctr"
