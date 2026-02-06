#!/bin/bash
# Quick commit script for all Phase 1 + Phase 2 Week 5 changes

cd /workspaces/thresh

echo "📦 Staging all changes..."
git add -A

echo ""
echo "📋 Changes to commit:"
git status --short

echo ""
echo "💾 Committing..."
git commit -m "feat: Complete Phase 1 + add metrics and MCP bug fixes

This commit includes:

**Phase 1 Week 1-2: Cross-Platform Container Abstraction**
- IContainerService interface for runtime abstraction
- WslService: Windows WSL implementation
- ContainerdService: Linux/macOS with nerdctl→docker→ctr fallback
- ContainerServiceFactory: Platform detection
- RuntimeInfo model for cross-platform runtime metadata
- ContainerdJsonContext for AOT-compatible JSON serialization

**Phase 1 Week 3-4: MCP Server Completion**
- StdioMcpServer: JSON-RPC 2.0 over stdin/stdout
- 7 MCP tools with full input schemas
- StdioResponseTypes: AOT-compatible response types
- ToolsListResult: Generic result wrapper
- Updated McpServer to use IContainerService
- Updated McpJsonContext with new serialization types
- Program.cs: Added --stdio flag to serve command

**Phase 2 Week 5: Host Metrics**
- HostMetrics model for system monitoring
- MetricsService: Cross-platform metrics collection
- MetricsJsonContext for AOT-compatible JSON serialization
- Program.cs: Added metrics command with JSON output

**CLI Integration:**
- All commands updated to use ContainerServiceFactory
- Cross-platform version command
- Metrics command (text and JSON output)
- MCP serve command (HTTP and stdio modes)

**Documentation:**
- ROADMAP_2026.md: 16-week transformation plan
- MCP_INTEGRATION.md: Complete MCP integration guide
- vscode-mcp-config.json: VS Code configuration example

**Testing:**
- test-docker.sh: Container runtime detection
- test-codespace.sh: GitHub Codespaces validation
- commit-phase1-week1.sh: Phase 1 commit script
- commit-phase1-week3.sh: MCP commit script

**Bug Fixes:**
- Fixed StdioMcpServer compilation errors:
  - Added missing _initialized field
  - Fixed InitializeResult object creation
  - Fixed tools array type inference

**Key Features:**
✅ Single binary works on Windows (WSL), Linux (containerd/docker), macOS
✅ MCP server with stdio transport for AI editors
✅ Cross-platform metrics collection
✅ AOT-compatible with Native AOT compilation
✅ Zero compilation errors

Closes #N/A"

echo ""
echo "📤 Pushing to dev branch..."
git push origin dev

echo ""
echo "✅ Done! All changes committed and pushed to dev branch"
