#!/bin/bash
# Test thresh on Ubuntu VM after SSH

set -e

# Add .NET to PATH
export DOTNET_ROOT=/home/thresh/.dotnet
export PATH=$PATH:$DOTNET_ROOT

echo "=== Testing thresh on Ubuntu Linux VM ==="
echo ""

# Check current directory
echo "Current directory: $(pwd)"
echo "Current user: $(whoami)"
echo ""

# Clone thresh repository if not exists
if [ ! -d ~/thresh ]; then
    echo "Cloning thresh repository..."
    git clone https://github.com/dealer426/thresh.git ~/thresh
    echo "✓ Repository cloned"
else
    echo "✓ Repository already exists"
fi

echo ""
echo "=== Building thresh ==="
cd ~/thresh/thresh/Thresh
dotnet build

echo ""
echo "=== Testing thresh commands ==="
echo ""

# Test version
echo "1. Version check:"
./bin/Debug/net10.0/thresh --version

echo ""
echo "2. Help command:"
./bin/Debug/net10.0/thresh --help | head -20

echo ""
echo "3. List command (should work without Docker):"
./bin/Debug/net10.0/thresh list

echo ""
echo "=== System Information ==="
echo "OS: $(uname -a)"
echo "Docker: $(docker --version)"
echo "dotnet: $(dotnet --version)"
echo ""

echo "✅ SUCCESS! Thresh is working on Ubuntu Linux!"
echo ""
echo "Next steps:"
echo "  - Test other thresh commands"
echo "  - Test Docker integration"
echo "  - Document any platform-specific issues"
