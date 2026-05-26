---
sidebar_position: 3
title: Tutorials
description: Step-by-step guides for thresh development workflows
---

# Tutorials

Comprehensive guides to get the most out of thresh, from quick starts to advanced AI integration.

## Getting Started

### [Quick Start (5 Minutes)](/docs/tutorials/quick-start)

Get your first thresh environment running in just 5 minutes.

**You'll learn:**
- Install thresh on Windows
- Choose and provision a blueprint
- Enter and use your environment
- Basic environment management

**Time:** 5 minutes  
**Difficulty:** Beginner

---

## Environment Creation

### [Creating Custom Blueprints](/docs/tutorials/custom-blueprints)

Build your own environment templates tailored to your exact workflow.

**You'll learn:**
- Blueprint file structure and syntax
- Package management for different distributions
- Post-install scripts and environment variables
- Real-world blueprint examples
- Testing and sharing blueprints

**Time:** 20 minutes  
**Difficulty:** Intermediate

---

## Networking & Storage

### [Networking & Port Mapping](/docs/tutorials/networking)

Configure port mapping, exposed ports, and network settings for containerized workloads.

**You'll learn:**
- Map host ports to container ports
- Bind to specific network interfaces
- Configure container hostnames
- Automatic netsh forwarding on Windows

**Time:** 15 minutes  
**Difficulty:** Intermediate

### [Persistent Volumes](/docs/tutorials/volumes)

Set up persistent storage for data that survives environment recreation.

**You'll learn:**
- Named volumes for database data
- Bind mounts for live code editing
- Tmpfs for fast temporary storage
- Volume lifecycle management

**Time:** 15 minutes  
**Difficulty:** Intermediate

---

## Stack Orchestration (Hub-Managed)

### [Deploying Multi-Service Stacks](/docs/tutorials/stacks)

Deploy multi-service applications through the Thresh Hub dashboard and API.

**You'll learn:**
- Write stack definition JSON files with dependency ordering
- Use `${service.host}` / `${service.port}` variable injection
- Deploy stacks through the Hub UI and REST API
- Perform rolling updates on individual services
- Use CLI commands (`thresh auth`, `thresh node`, `thresh cluster`) alongside Hub-managed stacks

**Time:** 20 minutes  
**Difficulty:** Intermediate

---

## Fleet Management

### [Fleet Management with Thresh Hub](/docs/tutorials/fleet-management)

Connect your thresh nodes to a centralized Hub for fleet-wide visibility and orchestration.

**You'll learn:**
- Deploy and configure Thresh Hub and mid-tier
- Connect agents with API key authentication (`thresh_live_*` / `thresh_mid_*`)
- Monitor node health and metrics in real-time
- Manage remote nodes with `thresh node` and `thresh cluster`
- Three-tier architecture (Hub → Mid-Tier → Agents)

**Time:** 30 minutes  
**Difficulty:** Advanced

### [Midtier ELF Migration — Test Plan](/docs/tutorials/midtier-elf-test-plan)

Full test plan for validating thresh-midtier after conversion to a native ELF binary (Native AOT).

**You'll learn:**
- Verify ELF binary format, size, and trimming requirements
- Test Hub connectivity and mid-tier API key enforcement
- Validate agent handshake, metrics routing, and command dispatch
- Run resilience and failover scenarios
- Confirm the platform matrix (x86-64, arm64, Alpine musl)
- Integrate ELF validation into CI

**Time:** Reference document  
**Difficulty:** Advanced

### [Hub — Copilot SDK Agent Flow & K8s Architecture](/docs/tutorials/hub-copilot-sdk-agent)

How the GitHub Copilot SDK spins up an agent from the Hub UI, with code-linked diagrams and a Kubernetes deployment reference architecture.

**You'll learn:**
- The full code call-chain from VS Code → HubMcpBridge → Hub → mid-tier → AgentService → GitHubCopilotService
- How the Copilot CLI subprocess model works
- Class responsibility map across all key source files
- Kubernetes deployment topology (Hub, mid-tier, PostgreSQL, Ingress, HPA, Secrets)
- Ready-to-paste Kubernetes YAML for each component
- Agent connectivity from nodes to a K8s-hosted Hub

**Time:** Reference document  
**Difficulty:** Advanced

---

## AI Integration

### [GitHub Copilot SDK Configuration](/docs/tutorials/copilot-sdk)

Set up AI-powered environment management with GitHub Copilot in VS Code.

**You'll learn:**
- Configure thresh as an MCP server
- Use natural language to manage environments
- Generate blueprints with AI assistance
- Best practices for AI-driven development

**Time:** 10 minutes  
**Difficulty:** Beginner

### [VS Code MCP Integration](/docs/tutorials/vscode-mcp)

Deep dive into Model Context Protocol integration for advanced AI workflows.

**You'll learn:**
- MCP protocol fundamentals
- Configure multiple AI clients (Copilot, Claude, Cline)
- Available tools and capabilities
- Advanced server configuration
- Debugging and troubleshooting

**Time:** 30 minutes  
**Difficulty:** Advanced

---

## Tutorial Path

### Beginner Track

1. [Quick Start](/docs/tutorials/quick-start) - Get started (5 min)
2. [GitHub Copilot SDK](/docs/tutorials/copilot-sdk) - AI integration (10 min)
3. [Creating Custom Blueprints](/docs/tutorials/custom-blueprints) - Customization (20 min)

**Total: 35 minutes**

### Intermediate Track

1. [Networking & Port Mapping](/docs/tutorials/networking) - Networking (15 min)
2. [Persistent Volumes](/docs/tutorials/volumes) - Storage (15 min)
3. [Deploying Multi-Service Stacks](/docs/tutorials/stacks) - Stacks (20 min)

**Total: 50 minutes**

### Advanced Track

1. [Creating Custom Blueprints](/docs/tutorials/custom-blueprints) - Blueprint development (20 min)
2. [VS Code MCP Integration](/docs/tutorials/vscode-mcp) - Advanced AI (30 min)
3. [Fleet Management](/docs/tutorials/fleet-management) - Hub & fleet (30 min)
4. [Hub Copilot SDK & K8s Architecture](/docs/tutorials/hub-copilot-sdk-agent) - Reference architecture

**Total: 80 minutes**

### Team Onboarding

1. [Quick Start](/docs/tutorials/quick-start) - Everyone (5 min)
2. [Creating Custom Blueprints](/docs/tutorials/custom-blueprints) - Team leads (20 min)
3. [Deploying Stacks](/docs/tutorials/stacks) - DevOps (20 min)
4. [Fleet Management](/docs/tutorials/fleet-management) - Infra team (30 min)

**Total: 75 minutes**

---

## Additional Resources

**CLI Reference:**
- [thresh up](/docs/cli-reference/up) - Provision environments
- [thresh auth](/docs/cli-reference/auth) - Hub authentication
- [thresh node](/docs/cli-reference/node) - Remote node management
- [thresh cluster](/docs/cli-reference/cluster) - Cluster management
- [thresh agent](/docs/cli-reference/agent) - Fleet agent connectivity
- [thresh blueprints](/docs/cli-reference/blueprints) - List blueprints
- [thresh generate](/docs/cli-reference/generate) - AI blueprint generation

**Guides:**
- [Installation](/docs/installation) - Platform-specific setup
- [MCP Integration](/docs/mcp-integration) - AI assistant configuration
- [Getting Started](/docs/intro) - Overview and concepts

**Community:**
- [GitHub Discussions](https://github.com/dealer426/thresh/discussions) - Ask questions
- [GitHub Issues](https://github.com/dealer426/thresh/issues) - Report bugs
- [Blog](https://thresh.sh/blog) - Latest updates and tips

---

## Contributing Tutorials

Have a great thresh workflow? Share it with the community!

**How to contribute:**
1. Fork the [thresh repository](https://github.com/dealer426/thresh)
2. Create tutorial in `website/docs/tutorials/`
3. Follow existing format and style
4. Submit pull request

See [CONTRIBUTING.md](https://github.com/dealer426/thresh/blob/main/CONTRIBUTING.md) for details.

---

Need help? Visit [GitHub Discussions](https://github.com/dealer426/thresh/discussions) or open an issue.
