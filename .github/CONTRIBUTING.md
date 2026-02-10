# Contributing to thresh

Thank you for your interest in contributing to thresh! 🎉

## Branch Strategy

We use a **two-branch workflow**:

- **`dev`** - Development branch (default for all PRs)
- **`main`** - Production branch (releases only)

### Important Rules

1. **All PRs should target `dev`** unless it's an emergency hotfix
2. `main` is only updated via merges from `dev` during releases
3. Never commit directly to `main`

## Development Workflow

### 1. Fork & Clone

```bash
git clone https://github.com/YOUR_USERNAME/thresh.git
cd thresh
git remote add upstream https://github.com/dealer426/thresh.git
```

### 2. Create Feature Branch from `dev`

```bash
git checkout dev
git pull upstream dev
git checkout -b feature/your-feature-name
```

### 3. Make Changes

- Write code
- Add tests if applicable
- Update documentation

### 4. Test Locally

```bash
# Build and test
dotnet build thresh/Thresh/Thresh.csproj
dotnet test

# Test documentation (if changed)
cd website
npm run build
```

### 5. Commit with Conventional Commits

```bash
git add .
git commit -m "feat: add new blueprint for Rust development"
```

**Commit types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code restructuring
- `test`: Adding tests
- `chore`: Maintenance tasks

### 6. Push and Create PR

```bash
git push origin feature/your-feature-name
```

Then create a PR on GitHub **targeting the `dev` branch**.

## Code Style

- Follow C# conventions (.NET 9)
- Use meaningful variable names
- Add XML documentation for public APIs
- Keep functions focused and single-purpose

## Documentation

- Update `CHANGELOG.md` for notable changes
- Update `README.md` if adding new features
- Add/update docs in `website/docs/` for user-facing changes

## Questions?

- Open a [Discussion](https://github.com/dealer426/thresh/discussions)
- Ask in an [Issue](https://github.com/dealer426/thresh/issues)

Thank you for contributing! 🚀
