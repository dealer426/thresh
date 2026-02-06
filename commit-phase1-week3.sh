#!/bin/bash
# Commit Phase 1, Week 3-4 changes to dev branch

set -e

echo "📋 Phase 1, Week 3-4: MCP Server Completion"
echo "============================================"
echo ""

echo "📊 Files to be committed:"
echo ""
echo "NEW FILES:"
echo "  + thresh/Thresh/Mcp/StdioMcpServer.cs          (615 lines)"
echo "  + docs/vscode-mcp-config.json                   (22 lines)"
echo "  + docs/MCP_INTEGRATION.md                      (468 lines)"
echo ""
echo "MODIFIED FILES:"
echo "  ~ thresh/Thresh/Mcp/McpServer.cs               (updated to use IContainerService)"
echo "  ~ thresh/Thresh/Program.cs                     (added --stdio flag to serve command)"
echo "  ~ docs/ROADMAP_2026.md                         (marked Phase 1 complete)"
echo ""
echo "📦 Total Changes: 3 new files, 3 modified files"
echo ""

# Check we're on dev branch
CURRENT_BRANCH=$(git branch --show-current)
if [ "$CURRENT_BRANCH" != "dev" ]; then
    echo "⚠️  Warning: Currently on branch '$CURRENT_BRANCH', not 'dev'"
    echo "   Switching to dev branch..."
    git checkout dev
fi

echo "🔍 Current branch: $(git branch --show-current)"
echo ""

# Stage all changes
echo "📦 Staging changes..."
git add thresh/Thresh/Mcp/StdioMcpServer.cs
git add thresh/Thresh/Mcp/McpServer.cs
git add thresh/Thresh/Program.cs
git add docs/vscode-mcp-config.json
git add docs/MCP_INTEGRATION.md
git add docs/ROADMAP_2026.md
echo "  ✅ All changes staged"
echo ""

# Show what's staged
echo "📋 Staged changes:"
git status --short
echo ""

# Create commit
COMMIT_MESSAGE="feat: Complete MCP server with stdio transport for AI editor integration

Phase 1, Week 3-4 Implementation:

**New MCP Server:**
- StdioMcpServer: JSON-RPC 2.0 over stdin/stdout for VS Code/Cursor/Windsurf
- 7 MCP tools with full input schemas
- Error handling with stderr logging
- Cross-platform via IContainerService

**MCP Tools:**
- list_environments (with include_all parameter)
- create_environment (blueprint, name, verbose)
- destroy_environment (name)
- list_blueprints
- get_blueprint (name)
- get_version
- generate_blueprint (prompt, model) - placeholder for Phase 2

**Updated Existing MCP Server:**
- McpServer.cs: Updated to use IContainerService instead of WslService
- Dynamic runtime name in tool descriptions
- Cross-platform compatibility

**CLI Updates:**
- Program.cs: Added --stdio flag to serve command
- Conditional routing: stdio mode uses StdioMcpServer, HTTP mode uses McpServer

**Documentation:**
- MCP_INTEGRATION.md: Complete guide with examples, testing, troubleshooting
- vscode-mcp-config.json: VS Code configuration example
- ROADMAP_2026.md: Marked Phase 1 tasks complete

**Key Features:**
✅ JSON-RPC protocol (MCP 2024-11-05 spec)
✅ Full input schemas for all tools
✅ VS Code/Cursor/Windsurf integration ready
✅ Cross-platform MCP server (works on all platforms)
✅ Zero compilation errors
✅ Binary size impact: +100 KB (16.8 MB → 16.9 MB estimated)

**Testing:**
- Compiles cleanly
- Ready for VS Code integration testing
- HTTP mode available for debugging

Closes #N/A (future: create issue for Phase 1 tracking)"

echo "💬 Commit message:"
echo "---"
echo "$COMMIT_MESSAGE"
echo "---"
echo ""

read -p "🤔 Proceed with commit? [y/N] " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    git commit -m "$COMMIT_MESSAGE"
    echo ""
    echo "✅ Changes committed to dev branch"
    echo ""
    echo "📤 Next steps:"
    echo "  1. Run: git push origin dev"
    echo "  2. Test MCP: Add vscode-mcp-config.json to VS Code settings"
    echo "  3. Test: thresh serve --stdio"
    echo "  4. Begin Phase 2 (Week 5: Host Metrics)"
else
    echo "❌ Commit cancelled"
    echo "   Run 'git reset' to unstage changes if needed"
fi
