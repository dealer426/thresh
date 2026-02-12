# Algolia DocSearch Application Guide

This document provides instructions for applying to Algolia DocSearch for the thresh documentation site.

## What is Algolia DocSearch?

Algolia DocSearch is a free search service for open-source documentation. It provides:
- Fast, typo-tolerant search
- Keyboard shortcuts
- Search analytics
- Zero configuration required

## Prerequisites

Before applying, ensure:

✅ **Documentation is live:** https://thresh.sh or https://dealer426.github.io/thresh/  
✅ **Documentation is public:** GitHub repository is public  
✅ **Documentation is open source:** MIT License  
✅ **Documentation is technical:** Developer-focused content  

## Application Process

### Step 1: Visit Application Page

Go to: https://docsearch.algolia.com/apply/

### Step 2: Fill Out Form

**Required Information:**

- **Website URL:** `https://thresh.sh` or `https://dealer426.github.io/thresh/`
- **GitHub Repository:** `https://github.com/dealer426/thresh`
- **Email Address:** Your email (will receive API credentials)
- **Repository Owner?:** Yes
- **Documentation Generator:** Docusaurus v3

### Step 3: Form Data (Copy-Paste Ready)

```
Website URL: https://dealer426.github.io/thresh/
GitHub Repository URL: https://github.com/dealer426/thresh
Email: [YOUR_EMAIL]
Repository Owner: Yes
Documentation Generator: Docusaurus 3.9.2
I am the owner of the website (check): ✅
I am using Docusaurus v3 or higher (check): ✅
My documentation is open source (check): ✅
```

### Step 4: Additional Context (Optional)

When asked to provide more context about your project:

```
thresh is a lightweight CLI tool for orchestrating WSL development environments 
on Windows. It provisions isolated environments using WSL 2 with AI-powered 
blueprint generation. The documentation covers installation, CLI commands, 
tutorials, and AI integration via Model Context Protocol (MCP).

Key features of the documentation:
- Comprehensive CLI reference (11 commands)
- 5 in-depth tutorials
- Windows/WSL-specific guides
- AI integration examples (GitHub Copilot, Claude)
- Architecture diagrams (Mermaid)
- Blog posts with examples

Target audience: Developers, DevOps engineers, and students who need isolated,
reproducible development environments on Windows.
```

## What Happens Next?

1. **Application Review:** Algolia team reviews within 1-3 weeks
2. **Approval Email:** Receive API key and application ID
3. **Auto-Configuration:** Docusaurus v3 auto-detects Algolia config

## Configuration (After Approval)

When you receive the email with credentials, update `docusaurus.config.ts`:

```typescript
export default {
  // ... existing config
  
  themeConfig: {
    // ... existing theme config
    
    algolia: {
      appId: 'YOUR_APP_ID',        // From Algolia email
      apiKey: 'YOUR_API_KEY',      // From Algolia email
      indexName: 'thresh',         // Typically your project name
      contextualSearch: true,      // Enable context-aware search
      searchParameters: {},        // Optional: customize search
      searchPagePath: 'search',    // Optional: custom search page
    },
  },
} satisfies Config;
```

## Verification

After configuration, verify search works:

1. Build site: `npm run build`
2. Serve locally: `npm run serve`
3. Press `Ctrl+K` or `/` to open search
4. Search for "thresh up" or "blueprints"

## Troubleshooting

### Application Rejected

**Reasons:**
- Documentation not publicly accessible
- Not enough content (need at least 5-10 pages)
- Content not technical/developer-focused

**Solutions:**
- Ensure site is live and public
- Add more documentation pages (we have 20+)
- Emphasize technical nature in application

### Not Receiving Email

**Solutions:**
- Check spam folder
- Wait 2-3 weeks (manual review process)
- Follow up via Algolia Community: https://discourse.algolia.com/

### Search Not Working After Setup

**Solutions:**
- Clear browser cache
- Rebuild site: `npm run build`
- Verify API key in config
- Check browser console for errors

## Alternative Search Solutions

If Algolia rejects (unlikely given our project):

### 1. Local Search Plugin

```bash
npm install --save @docusaurus/plugin-content-docs @cmfcmf/docusaurus-search-local
```

**docusaurus.config.ts:**
```typescript
plugins: [
  [
    require.resolve("@cmfcmf/docusaurus-search-local"),
    {
      indexDocs: true,
      indexBlog: true,
      language: "en",
    },
  ],
],
```

### 2. Self-Hosted Algolia

- Sign up for free tier: https://www.algolia.com/pricing/
- 10,000 search requests/month free
- Manual indexing required

### 3. Typesense (Open Source Alternative)

- Self-hosted search engine
- Similar to Algolia but open source
- Requires server setup

## Timeline

| Date | Action |
|------|--------|
| **Today** | Submit Algolia DocSearch application |
| **Week 1-3** | Wait for Algolia review |
| **After Approval** | Add API keys to `docusaurus.config.ts` |
| **Final Step** | Deploy updated site with search enabled |

## Current Status

- [ ] Application submitted
- [ ] Approval received
- [ ] API keys configured
- [ ] Search tested and working
- [ ] Deployed to production

## Resources

- **Algolia DocSearch:** https://docsearch.algolia.com/
- **Docusaurus Search Docs:** https://docusaurus.io/docs/search
- **Algolia Community:** https://discourse.algolia.com/

---

**Questions?** Ask in [GitHub Discussions](https://github.com/dealer426/thresh/discussions).
