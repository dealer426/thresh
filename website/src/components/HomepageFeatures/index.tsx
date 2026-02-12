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
    title: '3.8 MB Single Binary',
    Emoji: '🚀',
    description: (
      <>
        Native AOT compilation + UPX compression produces a tiny 3.8 MB binary.
        73% smaller with no runtime dependencies. Download and run immediately.
      </>
    ),
  },
  {
    title: 'AI-Powered Blueprints',
    Emoji: '🤖',
    description: (
      <>
        Generate custom development environments with natural language using GitHub Copilot SDK.
        Access 20+ models: GPT-4o, Claude 3.5, Gemini, o1, Llama, and more.
      </>
    ),
  },
  {
    title: 'Windows WSL Optimized',
    Emoji: '🪟',
    description: (
      <>
        Built specifically for Windows WSL environments. Provision Alpine, Ubuntu,
        Debian, and custom distributions in under 30 seconds.
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
