# Docusaurus Documentation Plan

**Created**: February 9, 2026  
**Status**: Planning  
**Goal**: Create professional documentation site with Docusaurus + GitHub Pages

---

## 🎯 Why Docusaurus?

### Key Benefits

1. **GitHub Integration** ✅
   - Native GitHub Pages deployment via `gh-pages` package
   - Automated deployment via GitHub Actions
   - Markdown-based content (seamless migration from existing `.md` files)
   - Version control built-in (Git)

2. **Documentation Features** 🚀
   - **Versioned Docs**: Support multiple versions (v1.0, v1.1, v1.2, etc.)
   - **Search**: Built-in Algolia DocSearch integration
   - **Dark Mode**: Automatic dark/light theme switching
   - **MDX Support**: Interactive React components in markdown
   - **API Documentation**: Automatic generation from code
   - **Blog**: Integrated blog for release announcements
   - **Internationalization**: Multi-language support (future)

3. **Developer Experience** 💻
   - Hot-reload during development
   - TypeScript support
   - Plugin ecosystem (diagrams, code blocks, tabs)
   - SEO optimized
   - Mobile responsive
   - Fast static site generation

4. **Maintenance** 🔧
   - Used by Meta, Microsoft, Supabase, Redwood.js
   - Active development and community
   - Simple `npm` dependency management
   - Easy content updates (just edit `.md` files)

---

## 📊 Current Documentation Structure

### Existing Files (12 markdown files)

**Root Level:**
- `README.md` - Main project overview
- `CHANGELOG.md` - Version history
- `CONTRIBUTING.md` - Contribution guidelines
- `GETTING_STARTED.md` - Quick start guide
- `SESSION_STATUS.md` - Development status (internal, exclude from docs)

**docs/ Directory:**
- `DUAL_AI_PROVIDERS.md` - AI provider configuration guide
- `MCP_INTEGRATION.md` - Model Context Protocol setup
- `ROADMAP_2026.md` - Product roadmap
- `GITHUB_ACTIONS_PLAN.md` - CI/CD documentation

**thresh/ Directory:**
- `thresh/README.md` - CLI-specific documentation

**packages/ Directory:**
- `packages/README.md` - Package manager overview
- `packages/SUBMISSION_GUIDE.md` - Package submission guide

---

## 🏗️ Proposed Docusaurus Structure

### Site Organization

```
website/
├── docs/                           # Documentation content
│   ├── intro.md                    # Getting Started (from GETTING_STARTED.md)
│   ├── installation/
│   │   ├── windows.md              # Windows installation (Winget, Chocolatey, Scoop)
│   │   ├── linux.md                # Linux installation (APT, RPM, binary)
│   │   └── macos.md                # macOS installation (Homebrew, binary)
│   ├── cli-reference/
│   │   ├── overview.md             # CLI overview
│   │   ├── environments.md         # up, destroy, list, exec, status
│   │   ├── blueprints.md           # blueprints, blueprint, generate
│   │   ├── configuration.md        # config set, config list
│   │   ├── metrics.md              # metrics command
│   │   ├── mcp.md                  # serve command (MCP)
│   │   └── utilities.md            # init, version, doctor
│   ├── blueprints/
│   │   ├── overview.md             # Blueprint system introduction
│   │   ├── built-in.md             # Built-in blueprints (8 blueprints)
│   │   ├── custom.md               # Creating custom blueprints
│   │   └── schema.md               # JSON schema reference
│   ├── ai-providers/
│   │   ├── overview.md             # AI provider introduction
│   │   ├── openai.md               # OpenAI configuration
│   │   ├── azure-openai.md         # Azure OpenAI setup
│   │   ├── github-copilot.md       # GitHub Copilot SDK setup
│   │   └── comparison.md           # Provider comparison table
│   ├── mcp-integration/
│   │   ├── overview.md             # MCP protocol introduction
│   │   ├── vscode.md               # VS Code setup
│   │   ├── cursor.md               # Cursor setup
│   │   ├── windsurf.md             # Windsurf setup
│   │   └── tools.md                # MCP tools reference (7 tools)
│   ├── advanced/
│   │   ├── cross-platform.md       # WSL vs containerd
│   │   ├── metrics.md              # Metrics collection
│   │   ├── security.md             # DPAPI encryption, secrets
│   │   └── troubleshooting.md      # Common issues and solutions
│   ├── contributing/
│   │   ├── overview.md             # Contribution guidelines
│   │   ├── development.md          # Development setup
│   │   ├── building.md             # Building from source
│   │   └── testing.md              # Testing guidelines
│   └── roadmap.md                  # Product roadmap (ROADMAP_2026.md)
│
├── blog/                           # Blog posts (release announcements)
│   ├── 2026-02-09-v1.2.0.md        # v1.2.0 release (AOT optimization)
│   ├── 2026-02-09-v1.1.0.md        # v1.1.0 release (MCP integration)
│   └── 2026-02-01-v1.0.0.md        # v1.0.0 release (initial)
│
├── src/
│   ├── components/                 # React components
│   │   ├── HomepageFeatures/
│   │   └── CodeBlock/
│   ├── css/
│   │   └── custom.css              # Custom styling
│   └── pages/
│       ├── index.tsx               # Homepage
│       └── download.tsx            # Download page (binaries, packages)
│
├── static/
│   ├── img/
│   │   ├── logo.svg                # thresh logo
│   │   ├── favicon.ico
│   │   └── screenshots/            # CLI screenshots
│   └── files/
│       └── blueprints/             # Example blueprint downloads
│
├── docusaurus.config.js            # Docusaurus configuration
├── sidebars.js                     # Sidebar navigation
├── package.json                    # NPM dependencies
└── .github/
    └── workflows/
        └── deploy-docs.yml         # GitHub Pages deployment
```

---

## 🚀 Implementation Plan

### Phase 1: Setup (Week 1)

**Day 1-2: Initialize Docusaurus**
```bash
cd c:/Users/burns/source/repos/thresh
npx create-docusaurus@latest website classic --typescript

cd website
npm install
npm start  # Test locally at http://localhost:3000
```

**Day 3-4: Content Migration**
- [ ] Migrate `GETTING_STARTED.md` → `docs/intro.md`
- [ ] Create CLI reference from `thresh/README.md`
- [ ] Migrate `DUAL_AI_PROVIDERS.md` → `docs/ai-providers/`
- [ ] Migrate `MCP_INTEGRATION.md` → `docs/mcp-integration/`
- [ ] Create installation guides (Windows, Linux, macOS)

**Day 5: Styling & Branding**
- [ ] Create thresh logo/favicon
- [ ] Customize color scheme (match GitHub README)
- [ ] Add code syntax highlighting themes
- [ ] Configure navigation sidebar

---

### Phase 2: Enhanced Content (Week 2)

**Day 1-2: CLI Reference**
- [ ] Create detailed command references
- [ ] Add code examples for each command
- [ ] Include screenshots and demos
- [ ] Add troubleshooting sections

**Day 3-4: Tutorials**
- [ ] "Quick Start: 5-Minute Setup"
- [ ] "Creating Your First Blueprint"
- [ ] "Setting Up AI Provider"
- [ ] "VS Code MCP Integration"

**Day 5: Interactive Features**
- [ ] Add Mermaid diagrams (architecture)
- [ ] Add code tabs (multi-platform examples)
- [ ] Add callouts/admonitions (tips, warnings)
- [ ] Add live code playgrounds (optional)

---

### Phase 3: GitHub Pages Deployment (Week 3)

**Automatic Deployment via GitHub Actions**

Create `.github/workflows/deploy-docs.yml`:

```yaml
name: Deploy Docusaurus

on:
  push:
    branches: [main]
    paths:
      - 'website/**'
      - '.github/workflows/deploy-docs.yml'
  workflow_dispatch:

jobs:
  deploy:
    name: Deploy to GitHub Pages
    runs-on: ubuntu-latest
    permissions:
      contents: write
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: 20
          cache: npm
          cache-dependency-path: website/package-lock.json
      
      - name: Install dependencies
        working-directory: website
        run: npm ci
      
      - name: Build website
        working-directory: website
        run: npm run build
      
      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./website/build
          user_name: github-actions[bot]
          user_email: github-actions[bot]@users.noreply.github.com
```

**Configuration in `docusaurus.config.js`:**

```javascript
module.exports = {
  title: 'thresh',
  tagline: 'Lightweight development environment orchestration for Windows, Linux, and macOS',
  url: 'https://dealer426.github.io',
  baseUrl: '/thresh/',
  organizationName: 'dealer426',
  projectName: 'thresh',
  deploymentBranch: 'gh-pages',
  trailingSlash: false,
  
  themeConfig: {
    navbar: {
      title: 'thresh',
      logo: {
        alt: 'thresh Logo',
        src: 'img/logo.svg',
      },
      items: [
        {
          type: 'doc',
          docId: 'intro',
          position: 'left',
          label: 'Docs',
        },
        {to: '/blog', label: 'Blog', position: 'left'},
        {to: '/download', label: 'Download', position: 'left'},
        {
          href: 'https://github.com/dealer426/thresh',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {label: 'Getting Started', to: '/docs/intro'},
            {label: 'CLI Reference', to: '/docs/cli-reference/overview'},
            {label: 'MCP Integration', to: '/docs/mcp-integration/overview'},
          ],
        },
        {
          title: 'Community',
          items: [
            {label: 'GitHub Issues', href: 'https://github.com/dealer426/thresh/issues'},
            {label: 'Discussions', href: 'https://github.com/dealer426/thresh/discussions'},
          ],
        },
        {
          title: 'More',
          items: [
            {label: 'Blog', to: '/blog'},
            {label: 'Changelog', to: '/docs/changelog'},
            {label: 'Roadmap', to: '/docs/roadmap'},
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} thresh. Built with Docusaurus.`,
    },
    prism: {
      theme: require('prism-react-renderer/themes/github'),
      darkTheme: require('prism-react-renderer/themes/dracula'),
      additionalLanguages: ['bash', 'powershell', 'csharp', 'json'],
    },
    algolia: {
      // Add Algolia DocSearch when ready
      // appId: 'YOUR_APP_ID',
      // apiKey: 'YOUR_API_KEY',
      // indexName: 'thresh',
    },
  },
};
```

---

### Phase 4: Advanced Features (Week 4)

**Versioned Documentation**
```bash
cd website
npm run docusaurus docs:version 1.2.0
```

Creates `versioned_docs/version-1.2.0/` for archive.

**Search Integration (Algolia DocSearch)**
- [ ] Apply for Algolia DocSearch (free for open source)
- [ ] Configure search index
- [ ] Add search bar to navbar

**API Documentation**
- [ ] Generate API docs from C# XML comments
- [ ] Integrate with Docusaurus (docfx → markdown)
- [ ] Add code reference section

**Blog Posts**
- [ ] Release announcements (v1.0.0, v1.1.0, v1.2.0)
- [ ] Tutorial posts
- [ ] Use case showcases

---

## 📦 NPM Dependencies

**Core:**
```json
{
  "dependencies": {
    "@docusaurus/core": "^3.1.0",
    "@docusaurus/preset-classic": "^3.1.0",
    "@mdx-js/react": "^3.0.0",
    "clsx": "^2.0.0",
    "prism-react-renderer": "^2.3.0",
    "react": "^18.2.0",
    "react-dom": "^18.2.0"
  }
}
```

**Plugins:**
```bash
npm install --save @docusaurus/plugin-content-blog
npm install --save @docusaurus/plugin-content-docs
npm install --save @docusaurus/theme-mermaid
npm install --save remark-math rehype-katex  # Math equations
```

---

## 🌐 GitHub Pages Configuration

### Repository Settings

1. Go to: `https://github.com/dealer426/thresh/settings/pages`
2. Source: **Deploy from a branch**
3. Branch: **gh-pages** / **/ (root)**
4. Save

### Custom Domain (Optional)

If you own `thresh.dev` or similar:
```
# In website/static/CNAME
thresh.dev
```

Then configure DNS:
```
CNAME: www.thresh.dev → dealer426.github.io
A:     thresh.dev → GitHub Pages IPs
```

---

## 🎨 Homepage Design

### Hero Section
```
┌─────────────────────────────────────────────┐
│                                             │
│           ████████╗██╗  ██╗██████╗         │
│           ╚══██╔══╝██║  ██║██╔══██╗        │
│              ██║   ███████║██████╔╝        │
│              ██║   ██╔══██║██╔══██╗        │
│              ██║   ██║  ██║██║  ██║        │
│              ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝        │
│                                             │
│   Lightweight Development Environment      │
│         Orchestration for Windows,         │
│            Linux, and macOS                │
│                                             │
│   [Get Started]  [View on GitHub]          │
│                                             │
└─────────────────────────────────────────────┘
```

### Features Grid
```
┌──────────────┬──────────────┬──────────────┐
│ 🚀 Fast Setup│ 🤖 AI-Powered│ 🔧 MCP Ready │
│ Single binary│ Generate     │ VS Code, Cursor│
│ 14 MB, no    │ blueprints   │ & Windsurf    │
│ dependencies │ with AI      │ integration   │
├──────────────┼──────────────┼──────────────┤
│ 📦 8 Built-in│ 🌍 Cross-    │ 📊 Metrics   │
│ Blueprints   │ Platform     │ Monitor CPU,  │
│ Python, Node,│ WSL, Docker, │ RAM, storage │
│ Ubuntu, etc. │ containerd   │ in real-time │
└──────────────┴──────────────┴──────────────┘
```

---

## 📈 Success Metrics

### Documentation Quality
- [ ] All CLI commands documented with examples
- [ ] Installation guides for all platforms
- [ ] Troubleshooting section with common issues
- [ ] 5+ tutorial articles
- [ ] Search functionality working

### GitHub Pages
- [ ] Site deployed at `https://dealer426.github.io/thresh/`
- [ ] Automatic deployment on main branch push
- [ ] Mobile responsive (test on phone)
- [ ] Fast load times (<2s first load)

### User Experience
- [ ] Clear navigation structure
- [ ] Dark mode working
- [ ] Code examples copy-pasteable
- [ ] Screenshots and diagrams included
- [ ] Version dropdown (v1.0, v1.1, v1.2)

---

## ⚡ Quick Start Commands

### Local Development
```bash
cd website
npm start                    # http://localhost:3000
npm run build                # Test production build
npm run serve                # Serve production build locally
```

### Deployment
```bash
# Manual deployment (if needed)
cd website
npm run deploy

# Automatic (GitHub Actions handles this)
git add website/
git commit -m "docs: Update documentation"
git push origin main
# GitHub Actions deploys automatically
```

### Maintenance
```bash
# Update dependencies
cd website
npm update

# Create new version
npm run docusaurus docs:version 1.3.0

# Clear cache
npm run clear
```

---

## 🎯 Next Steps

1. **Immediate** (This Week):
   - [ ] Initialize Docusaurus project
   - [ ] Migrate existing markdown content
   - [ ] Set up GitHub Pages deployment
   - [ ] Test local build and deployment

2. **Short-term** (Next 2 Weeks):
   - [ ] Create comprehensive CLI reference
   - [ ] Add installation guides for all platforms
   - [ ] Write 3-5 tutorial articles
   - [ ] Add screenshots and diagrams

3. **Long-term** (Next Month):
   - [ ] Set up Algolia DocSearch
   - [ ] Create versioned docs (v1.0, v1.1, v1.2)
   - [ ] Add API reference documentation
   - [ ] Blog posts for each release

---

## 🔗 Resources

- **Docusaurus**: https://docusaurus.io/
- **GitHub Pages**: https://pages.github.com/
- **Algolia DocSearch**: https://docsearch.algolia.com/
- **Mermaid Diagrams**: https://mermaid.js.org/
- **Example Sites**:
  - https://redux.js.org/ (Docusaurus)
  - https://supabase.com/docs (Docusaurus)
  - https://redwoodjs.com/docs (Docusaurus)

---

**Status**: ✅ Ready to implement  
**Owner**: sburns  
**Priority**: High (improves user experience significantly)
