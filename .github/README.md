# CI/CD Workflows

This project uses GitHub Actions for CI/CD automation with two main workflows:

## Configured Workflows

### 1. **CI** (`.github/workflows/ci.yml`)
**Triggered on:**
- Pull Requests to `main` and `develop`
- Direct push to `main`

**Responsibilities:**
- Project build
- Test execution
- Code quality validation

### 2. **Auto PR to Main** (`.github/workflows/auto-pr-to-main.yml`)
**Triggered on:**
- Push to `develop` branch

**Responsibilities:**
- Complete build and tests
- Automatic PR creation to `main` (if everything passes)
- PR with detailed description and review checklist

## Development Flow

```
Develop Branch → Push → Auto CI Build & Test → Success → Auto PR to Main → Code Review → Merge to Main
                                           → Failure → Fix Issues → Back to Develop
```

## Automated Process

1. **Development**: Work on the `develop` branch
2. **Push**: Push changes to `develop`
3. **Automatic CI**: GitHub Actions runs build and tests
4. **Automatic PR**: If everything passes, automatically creates PR to `main`
5. **Manual Review**: Team reviews the PR before merge
6. **Merge**: After approval, merge to `main`

## Automatic Labels

Automatic PRs receive the following labels:
- `automated` - Indicates it was created automatically
- `ci-cd` - Related to CI/CD process
- `ready-for-review` - Ready for review

## Configuration

### Required Permissions
The workflow requires a Personal Access Token (PAT) to create Pull Requests:

1. **Create PAT**: GitHub Settings → Developer settings → Personal access tokens → Generate new token
2. **Required scopes**: `repo` (full repository access)
3. **Add as secret**: Repository Settings → Secrets and variables → Actions → New repository secret
4. **Secret name**: `PAT_TOKEN`
5. **Secret value**: Your generated PAT

### Customization
To modify behavior, edit the files:
- `.github/workflows/ci.yml` - Basic CI
- `.github/workflows/auto-pr-to-main.yml` - Auto PR

## Security

- Workflow only executes after tests pass
- PRs are created as draft by default (configurable)
- Automatic assignee is the original push author
- All changes go through mandatory review
