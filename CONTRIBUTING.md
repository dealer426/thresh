# Contributing to thresh

Thank you for your interest in contributing to thresh! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites

Before contributing, ensure you have the development environment set up:

```bash
# Clone the repository
git clone https://github.com/dealer426/thresh.git
cd thresh

# Run the development environment setup
./scripts/setup-dev-env.sh
```

### Development Environment

thresh is a unified .NET 9 Native AOT CLI application:

- **thresh** - .NET 9 Native AOT (16.6 MB single binary)
- **AI providers** - OpenAI SDK + GitHub Copilot SDK
- **Distribution support** - 12+ built-in distros + custom support

## 🛠️ Development Workflow

### 1. Fork and Clone

```bash
# Fork the repo on GitHub, then clone your fork
git clone https://github.com/YOUR-USERNAME/thresh.git
cd thresh

# Add upstream remote
git remote add upstream https://github.com/dealer426/thresh.git
```

### 2. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 3. Make Changes

Follow the coding standards for each component:

#### CLI (.NET 9 Native AOT)
- Follow C# coding conventions
- Use System.CommandLine for commands
- Add XML documentation comments
- Include unit tests for new functionality
- Ensure Native AOT compatibility
- Test both JIT and AOT builds

### 4. Test Your Changes

```bash
# Test thresh CLI
cd thresh/Thresh
dotnet test

# Build and test Native AOT
dotnet build -c Release
dotnet publish -c Release -r win-x64

# Run CLI tests
./bin/Release/net9.0/win-x64/publish/thresh.exe --version
```

### 5. Commit and Push

```bash
# Stage changes
git add .

# Commit with descriptive message
git commit -m "feat: add blueprint validation in CLI"

# Push to your fork
git push origin feature/your-feature-name
```

### 6. Create Pull Request

1. Go to GitHub and create a Pull Request
2. Fill out the PR template completely
3. Link any related issues
4. Wait for review and address feedback

## 📝 Coding Standards

### General Guidelines

- **Clear naming** - Use descriptive variable and function names
- **Documentation** - Document public APIs and complex logic
- **Error handling** - Handle errors gracefully with user-friendly messages
- **Performance** - Consider performance implications, especially for CLI startup
- **Security** - Follow security best practices for input validation

### Commit Messages

Use conventional commits format:

```
type(scope): description

feat(cli): add blueprint validation
fix(api): resolve authentication issue  
docs(readme): update installation instructions
test(web): add component tests for blueprint editor
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

## 🐛 Bug Reports

When reporting bugs, please include:

- **OS and version** (Windows 11, WSL2 distro)
- **thresh version** (`thresh --version`)
- **Steps to reproduce** 
- **Expected vs actual behavior**
- **Error messages or logs**
- **Environment details** (.NET version, Java version, Node.js version)

Use the bug report template in GitHub Issues.

## 💡 Feature Requests

For feature requests:

- Check existing issues first
- Describe the problem you're trying to solve
- Provide example use cases
- Consider implementation complexity
- Be open to discussion and alternatives

Use the feature request template in GitHub Issues.

## 🧪 Testing

### Running Tests

```bash
# Run all tests
cd thresh/Thresh
dotnet test

# Build and verify
dotnet build -c Release
dotnet publish -c Release -r win-x64
```

### Writing Tests

- **Unit tests** - Test individual functions/methods
- **Integration tests** - Test component interactions
- **E2E tests** - Test complete user workflows
- **Performance tests** - Validate CLI startup times

### Test Coverage

Aim for:
- **CLI**: >80% coverage on core logic
- **Services**: >85% coverage on business logic
- **Models**: >90% coverage on data models

## 📦 Blueprint Contributions

### Adding Template Blueprints

1. Create blueprint YAML in `thresh/Thresh/blueprints/`
2. Test with multiple WSL environments
3. Add documentation and examples
4. Submit PR with blueprint details

### Blueprint Schema

```yaml
name: "ubuntu-python-ml"
version: "1.0.0"
description: "Ubuntu with Python ML stack"
base:
  os: "ubuntu"
  version: "22.04"
profiles:
  - name: "python-ml"
    layers:
      - type: "apt"
        packages: ["python3", "python3-pip"]
      - type: "pip"  
        packages: ["torch", "jupyter", "pandas"]
      - type: "vscode"
        extensions: ["ms-python.python", "ms-toolsai.jupyter"]
```

## 🚀 Release Process

### Versioning

We use semantic versioning (semver):
- **Major** (1.0.0) - Breaking changes
- **Minor** (0.1.0) - New features, backward compatible  
- **Patch** (0.0.1) - Bug fixes, backward compatible

### Release Checklist

- [ ] All tests passing
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] Version bumped in all components
- [ ] Release notes prepared
- [ ] Binaries built and tested

## 🤝 Community Guidelines

### Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Help newcomers get started
- Celebrate diverse perspectives
- Report inappropriate behavior

### Communication

- **GitHub Issues** - Bug reports, feature requests
- **GitHub Discussions** - General questions, ideas
- **Discord** - Real-time community chat
- **Twitter** - Updates and announcements

## 📚 Learning Resources

### Technologies Used

- **.NET 9** - [dotnet.microsoft.com](https://dotnet.microsoft.com)
- **Native AOT** - [learn.microsoft.com/dotnet/core/deploying/native-aot](https://learn.microsoft.com/dotnet/core/deploying/native-aot)
- **System.CommandLine** - [github.com/dotnet/command-line-api](https://github.com/dotnet/command-line-api)
- **Azure.AI.OpenAI** - [learn.microsoft.com/dotnet/api/azure.ai.openai](https://learn.microsoft.com/dotnet/api/azure.ai.openai)
- **GitHub.Copilot.SDK** - [nuget.org/packages/GitHub.Copilot.SDK](https://www.nuget.org/packages/GitHub.Copilot.SDK)
- **WSL2** - [docs.microsoft.com/wsl](https://docs.microsoft.com/windows/wsl)

### Development Tools

- **VS Code** - Recommended editor with extensions
- **GitHub CLI** - For managing PRs and issues
- **Docker Desktop** - For container testing

## ❓ Getting Help

If you need help:

1. Check existing documentation
2. Search GitHub Issues  
3. Ask in GitHub Discussions
4. Join our Discord community
5. Tag maintainers in issues (sparingly)

## 🏆 Recognition

Contributors are recognized through:

- **GitHub contributors graph**
- **Release notes acknowledgments**  
- **Community shoutouts**
- **Contributor of the month**

Thank you for contributing to thresh! 🚀