import type {ReactNode} from 'react';
import {useState} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import HomepageFeatures from '@site/src/components/HomepageFeatures';
import Heading from '@theme/Heading';

import styles from './index.module.css';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    await navigator.clipboard.writeText('winget install dealer426.thresh');
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <Heading as="h1" className="hero__title">
          {siteConfig.title}
        </Heading>
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg"
            to="/docs/intro">
            Get Started - 5min ⏱️
          </Link>
          <Link
            className="button button--primary button--lg margin-left--md"
            to="https://github.com/dealer426/thresh/releases/latest">
            Download v1.4.0
          </Link>
        </div>
        <div className={styles.installCommand}>
          <span>winget install dealer426.thresh</span>
          <button 
            onClick={handleCopy}
            className={styles.copyButton}
            aria-label="Copy install command"
            title={copied ? "Copied!" : "Copy to clipboard"}
          >
            {copied ? (
              <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor">
                <path d="M13.78 4.22a.75.75 0 010 1.06l-7.25 7.25a.75.75 0 01-1.06 0L2.22 9.28a.75.75 0 011.06-1.06L6 10.94l6.72-6.72a.75.75 0 011.06 0z"/>
              </svg>
            ) : (
              <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor">
                <path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 010 1.5h-1.5a.25.25 0 00-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 00.25-.25v-1.5a.75.75 0 011.5 0v1.5A1.75 1.75 0 019.25 16h-7.5A1.75 1.75 0 010 14.25v-7.5z"/>
                <path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0114.25 11h-7.5A1.75 1.75 0 015 9.25v-7.5zm1.75-.25a.25.25 0 00-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 00.25-.25v-7.5a.25.25 0 00-.25-.25h-7.5z"/>
              </svg>
            )}
          </button>
        </div>
      </div>
    </header>
  );
}

function QuickDemo() {
  return (
    <section className={styles.quickDemo}>
      <div className="container">
        <Heading as="h2" className="text--center margin-bottom--lg">
          See thresh in Action
        </Heading>
        <div className="row">
          <div className="col col--6">
            <div className={styles.codeBlock}>
              <div className={styles.codeHeader}>
                <span className={styles.codeTitle}>Install & Run Python Environment</span>
              </div>
              <pre className={styles.codeContent}>
{`# Install (Windows)
> winget install dealer426.thresh

# Provision Python dev environment
> thresh up python-dev

Creating container environment: thresh-python-dev
Distribution: Alpine Linux 3.19
Installing: python3 pip git vim curl

✓ Environment ready in 28s

# List environments
> thresh list

NAME             STATUS    DISTRO        CPU    MEM
thresh-python-dev  Running   Alpine 3.19   0.5%   64MB

# Enter environment (platform-specific)
> thresh exec python-dev  # or: wsl -d, docker exec, nerdctl exec
(thresh-python-dev)$ python3 --version
Python 3.12.1

(thresh-python-dev)$ pip install flask
Successfully installed flask-3.0.0`}
              </pre>
            </div>
          </div>
          <div className="col col--6">
            <div className={styles.codeBlock}>
              <div className={styles.codeHeader}>
                <span className={styles.codeTitle}>AI-Powered Custom Blueprint</span>
              </div>
              <pre className={styles.codeContent}>
{`# Ask GitHub Copilot to generate a blueprint
> "Create a Node.js 20 + PostgreSQL development 
   environment with TypeScript and testing tools"

# Copilot generates blueprint:
{
  "name": "fullstack-js",
  "distribution": "alpine:3.19",
  "packages": [
    "nodejs", "npm", "postgresql-client", "git"
  ],
  "postInstall": [
    "npm install -g typescript tsx jest",
    "npm install -g @types/node @types/jest"
  ],
  "environment": {
    "NODE_ENV": "development"
  },
  "ports": [
    {"container": 3000, "host": 3000}
  ]
}

# Provision it
> thresh up fullstack-js

✓ Environment ready in 32s

# Access from host browser
http://localhost:3000`}
              </pre>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

function UseCases() {
  return (
    <section className={styles.useCases}>
      <div className="container">
        <Heading as="h2" className="text--center margin-bottom--lg">
          What You Can Build
        </Heading>
        <div className="row">
          <div className="col col--4">
            <div className={styles.useCase}>
              <div className={styles.useCaseIcon}>🐍</div>
              <Heading as="h3">Python Development</Heading>
              <p>
                Flask, Django, FastAPI apps with PostgreSQL. Isolated dependencies
                per project. No virtualenv conflicts.
              </p>
              <code className={styles.useCaseCommand}>thresh up python-dev</code>
            </div>
          </div>
          <div className="col col--4">
            <div className={styles.useCase}>
              <div className={styles.useCaseIcon}>⚡</div>
              <Heading as="h3">Full-Stack JavaScript</Heading>
              <p>
                Next.js, React, Vue with Node.js backends. Multiple Node versions
                side-by-side without nvm.
              </p>
              <code className={styles.useCaseCommand}>thresh up node-dev</code>
            </div>
          </div>
          <div className="col col--4">
            <div className={styles.useCase}>
              <div className={styles.useCaseIcon}>☁️</div>
              <Heading as="h3">Cloud CLI Testing</Heading>
              <p>
                Azure CLI, AWS CLI, kubectl in isolated environments. Test
                scripts without polluting host system.
              </p>
              <code className={styles.useCaseCommand}>thresh up azure-cli</code>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

export default function Home(): ReactNode {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title="Lightweight Development Environment Orchestration"
      description="Cross-platform CLI for provisioning isolated development environments with AI-powered blueprint generation">
      <HomepageHeader />
      <main>
        <HomepageFeatures />
        <QuickDemo />
        <UseCases />
      </main>
    </Layout>
  );
}
