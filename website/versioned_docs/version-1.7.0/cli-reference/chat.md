---
sidebar_position: 5
title: thresh chat
description: Interactive AI chat for environment planning
---

# thresh chat

Interactive AI chat mode for planning and designing development environments.

## Synopsis

```powershell
thresh chat [options]
```

## Description

The `chat` command starts an interactive AI session where you can:

- Discuss environment requirements
- Refine blueprint specifications
- Get recommendations for packages and tools
- Troubleshoot environment issues
- Learn about distribution options

:::tip Conversation Memory
The chat maintains context throughout the session, allowing you to iteratively refine your environment design.
:::

## Options

| Option | Description |
|--------|-------------|
| `--model <name>`, `-m` | Use specific AI model |
| `--help`, `-h` | Show help information |

## Examples

### Basic Chat Session

```powershell
thresh chat
```

**Example interaction:**
```
thresh AI Chat (type 'exit' to quit, 'help' for commands)

Chat> I need a PHP development environment

AI: I can help you set up a PHP environment. Which version of PHP do you need?
The latest LTS is PHP 8.2. Would you also like a database (MySQL/PostgreSQL)
and a web server (nginx/Apache)?

Chat> PHP 8.2 with nginx and MySQL

AI: Great choice! Here's what I recommend:

Base: Ubuntu 22.04
Packages:
- php8.2-fpm
- php8.2-mysql
- php8.2-cli
- nginx
- mysql-server

Would you like me to generate the complete blueprint?

Chat> yes

AI: [Generates blueprint JSON]

Chat> also add Redis for caching

AI: [Updates blueprint with Redis]

Chat> save this as php-dev.json

AI: I'll generate the complete blueprint. Run this command:

thresh blueprint generate "PHP 8.2 with nginx, MySQL, and Redis" --output php-dev

Chat> exit

Goodbye!
```

### Use Specific Model

```powershell
# Use Claude for complex discussions
thresh chat --model claude-3.5-sonnet

# Use GPT-4o for quick questions
thresh chat --model gpt-4o
```

## Chat Commands

### Built-in Commands

| Command | Description |
|---------|-------------|
| `exit`, `quit` | Exit chat session |
| `help` | Show available commands |
| `clear` | Clear conversation history |
| `model <name>` | Switch AI model mid-session |
| `save <filename>` | Save last blueprint to file |
| `history` | Show conversation history |

### Example Commands

```
Chat> help
Available commands:
- exit/quit: Exit chat
- clear: Clear conversation
- model <name>: Switch model
- save <file>: Save last blueprint
- history: Show conversation

Chat> model gpt-4o
Switched to model: gpt-4o

Chat> clear
Conversation history cleared.
```

## Use Cases

### Environment Planning

```powershell
thresh chat
```
```
Chat> I'm building a microservices backend

AI: That's a great architecture! For microservices, I'd recommend:
1. What language? (Node.js, Go, Python, Java)
2. Message queue? (RabbitMQ, Kafka, Redis)
3. Database? (PostgreSQL, MongoDB, MySQL)
4. Service mesh? (Istio, Linkerd)

Chat> Node.js with RabbitMQ and PostgreSQL

AI: Perfect! Here's a suggested setup...
```

### Package Discovery

```
Chat> What packages do I need for Python data science?

AI: For data science with Python, you'll need:

Core:
- python3, python3-pip
- jupyter, ipython

Data manipulation:
- pandas, numpy

Visualization:
- matplotlib, seaborn, plotly

Machine learning:
- scikit-learn, tensorflow, pytorch

Would you like me to generate a complete blueprint?
```

### Troubleshooting

```
Chat> My Python environment can't install packages

AI: Let's troubleshoot pip installation. Can you tell me:
1. What error message do you see?
2. Which distribution? (Ubuntu, Alpine, Debian)
3. Python version?

Chat> Ubuntu 22.04, Python 3.11, "externally-managed-environment" error

AI: Ah, that's Ubuntu's new PEP 668 protection. You have three options:
1. Use venv: python3 -m venv myenv
2. Use --break-system-packages (not recommended)
3. Install python3-pip with apt

I recommend option 1. Would you like me to update your blueprint?
```

### Multi-turn Refinement

```
Chat> Create a Go development environment

AI: [Suggests Go 1.21 setup]

Chat> Add Docker support

AI: [Updates with Docker]

Chat> Also need PostgreSQL client libraries

AI: [Adds libpq-dev and PostgreSQL client]

Chat> And make sure git is configured

AI: [Adds git with default configuration]

Chat> Perfect, generate the blueprint

AI: [Outputs complete JSON]
```

## Tips for Effective Chats

### Be Conversational

```
✅ "I'm building a web scraper, what do I need?"
✅ "Add Redis to that environment"
✅ "Can you explain why you chose Ubuntu over Alpine?"
```

### Ask Follow-up Questions

```
Chat> Why did you choose nginx over Apache?

AI: For your use case (Node.js reverse proxy), nginx is better because:
- Lower memory footprint
- Better async handling
- Simpler configuration for reverse proxying
- Industry standard for Node.js apps
```

### Request Changes Incrementally

```
Chat> Start with a minimal Python setup
AI: [Suggests basic Python]

Chat> Add web development packages
AI: [Adds Flask, Django, etc.]

Chat> Make it production-ready
AI: [Adds gunicorn, nginx, systemd]
```

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+C` | Exit chat immediately |
| `Ctrl+D` | Exit chat gracefully |
| `Up/Down` | Navigate command history |
| `Tab` | Auto-complete commands |

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Chat session ended normally |
| `1` | AI error |
| `5` | Configuration error |

## See Also

- [`thresh blueprint generate`](./generate) - One-shot blueprint generation
- [`thresh config`](./config) - Configure AI settings
- [`thresh up`](./up) - Provision environments
