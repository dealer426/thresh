import type {ReactNode} from 'react';
import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  Emoji: string;
  description: ReactNode;
};

const FeatureList: FeatureItem[] = [
  {
    title: '14 MB Single Binary',
    Emoji: '🚀',
    description: (
      <>
        Native AOT compilation produces a tiny 14 MB binary with no runtime dependencies.
        Download and run immediately - no installation required.
      </>
    ),
  },
  {
    title: 'AI-Powered Blueprints',
    Emoji: '🤖',
    description: (
      <>
        Generate custom development environments with natural language. 
        Supports OpenAI, Azure OpenAI, and GitHub Copilot SDK.
      </>
    ),
  },
  {
    title: 'Cross-Platform',
    Emoji: '🌍',
    description: (
      <>
        Works on Windows (WSL), Linux, and macOS. Supports both WSL and
        container-based environments (containerd/Docker).
      </>
    ),
  },
  {
    title: 'MCP Integration',
    Emoji: '🔧',
    description: (
      <>
        Model Context Protocol server built-in. Use thresh from your AI editor
        (VS Code, Cursor, Windsurf) with 7 MCP tools.
      </>
    ),
  },
  {
    title: '8 Ready-to-Use Environments',
    Emoji: '📦',
    description: (
      <>
        Python, Node.js, Alpine, Ubuntu, Debian, Azure CLI - all pre-configured.
        Or create your own custom blueprints.
      </>
    ),
  },
  {
    title: 'System Metrics',
    Emoji: '📊',
    description: (
      <>
        Monitor CPU, memory, disk usage. Track environment health with built-in
        metrics collection and JSON export.
      </>
    ),
  },
];

function Feature({title, Emoji, description}: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <div className={styles.featureEmoji}>{Emoji}</div>
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): ReactNode {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
