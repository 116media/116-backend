# 116-backend
116 (Cent-Seize) is a bold digital platform that promotes music and hip-hop culture in DR and beyond. Through articles, video shows, and exclusive behind-the-scenes content, it connects fans with artists, highlights emerging talent, and tells the stories shaping the culture.

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- IDE: [Visual Studio Code](https://code.visualstudio.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/)

### ⚡ Quick Setup

To get started with development, run the setup script:

```bash
# Run the automated setup script
../scripts/setup.sh
```

This script will:
- 🔍 **Check for csharpier** and install it if needed
- 🔧 **Configure git hooks** for automatic code quality checks
- ✅ **Set up your development environment** with all necessary tools

### 💡 Development Workflow

1. 🚀 **Run setup**: `./scripts/setup.sh` (first time only)
2. 🌿 **Create feature branch**: Following naming conventions
3. 💻 **Make your changes**: Code will be auto-formatted on save
4. 💾 **Commit changes**: Pre-commit hook formats code automatically
5. 📤 **Push to remote**: Pre-push hook validates branch and style rules
6. 🔄 **Create Pull Request**: For code review and integration

## Git Workflow

This project follows a structured branching strategy designed for scalable development and reliable releases.

### Branch Structure

#### Main Branch (`main`)
The `main` branch contains the most up-to-date, production-ready code. All release branches are created from this branch, and it serves as the single source of truth for the application's current state.

#### Release Branches (`x.y.z`)
Release branches follow [Semantic Versioning (SemVer)](https://semver.org/) specification:
- **Format**: `MAJOR.MINOR.PATCH` (e.g., `1.0.0`, `1.2.3`, `2.0.0`)
- **MAJOR**: Breaking changes that are not backward compatible
- **MINOR**: New features that are backward compatible
- **PATCH**: Bug fixes that are backward compatible

Release branches are created directly from `main` and represent stable versions of the application.

#### Development Branch (`develop`)
The `develop` branch is the integration branch where all feature development converges. It contains the latest development changes and serves as the staging area before merging into `main`.

**Important**: The `develop` branch must always be rebased from the latest version branch to maintain a clean history and incorporate the most recent stable changes.

#### Feature Branches
All development work is done in feature branches that follow a strict naming convention:

**Pattern**: `^(feat|chore|bug|fix|doc|docs|style|refactor|perf|test|build|ci|revert|)-[a-z]+(-[a-z]+)*$`

**Branch Types**:
- `feat-`: New features or functionality
- `chore-`: Maintenance tasks, dependency updates
- `bug-`: Bug fixes and issue resolution
- `fix-`: General fixes and corrections
- `doc-`/`docs-`: Documentation updates
- `style-`: Code style, formatting changes
- `refactor-`: Code refactoring without functionality changes
- `perf-`: Performance improvements
- `test-`: Test additions or modifications
- `build-`: Build system or deployment changes
- `ci-`: Continuous integration configuration
- `revert-`: Reverting previous changes

**Examples**:
- `feat-user-authentication`
- `fix-api-response-validation`
- `chore-update-dependencies`
- `docs-git-workflow`

### Workflow Rules

1. **Feature Branch Creation**: All feature branches must be created from and regularly rebased against the `develop` branch
2. **Development Integration**: Feature branches are merged into `develop` via pull requests
3. **Release Preparation**: When `develop` is stable, it's merged into the latest release version, then merged into `main` for production release.
4. **Version Management**: Release branches are tagged and created from `main`
5. **Hotfixes**: Critical fixes can be branched directly from release branches and merged back to both `main` and `develop`

### Best Practices

- Keep feature branches focused on single features or fixes
- Rebase regularly to maintain clean commit history
- Use descriptive commit messages following conventional commit format
- Ensure all tests pass before merging
- Review code through pull requests before integration

## Development Environment Setup

This project uses .NET 9.0 and follows strict code formatting standards to ensure consistency across the codebase.

### Code Formatting Configuration

The project includes comprehensive formatting configuration that works automatically with both VS Code and Rider:

#### Files Included:
- `.editorconfig` - Universal formatting rules for all editors
- `.vscode/settings.json` - VS Code specific settings with auto-format on save
- `.vscode/extensions.json` - Recommended extensions for VS Code
- `.idea/codeStyles/` - Rider/IntelliJ code style configuration

#### VS Code Setup:
1. Install recommended extensions when prompted, or install manually:
   - `ms-dotnettools.csharp` - C# language support
   - `ms-dotnettools.csdevkit` - C# Dev Kit
   - `editorconfig.editorconfig` - EditorConfig support

2. Auto-formatting is enabled by default for:
   - Format on save
   - Format on type
   - Format on paste
   - Organize imports on save
   - Fix all auto-fixable issues on save

#### Rider Setup:
1. Rider will automatically detect and apply the project's code style settings
2. To enable "format on save" in Rider:
   - Go to **File > Settings > Tools > Actions on Save**
   - Enable **Reformat code** and **Optimize imports**

### 🛡️ Code Quality & Git Hooks

This project uses automated code quality checks to maintain consistent standards:

#### 🎯 Pre-commit Hook
- 🎨 **Automatic formatting** of staged C# files using `csharpier`
- 📝 **Re-stages formatted files** automatically
- ✅ **Ensures consistent code style** before commits

#### 🔒 Pre-push Hook
- 🛡️ **Branch name validation** enforcing naming conventions
- 🚫 **Blocks direct pushes** to protected branches (`main`, `develop`, `master`)
- 📏 **Code formatting verification** using `dotnet csharpier --check`
- 📐 **Ensures code formatting consistency** across the codebase

#### 🛠️ Manual csharpier Commands

```bash
# Format all C# files in the project
dotnet csharpier .

# Format specific file
dotnet csharpier path/to/file.cs

# Check for formatting issues without fixing them
dotnet csharpier --check .

# Format only staged files (used by pre-commit hook)
./scripts/formatting.sh --staged-only
```

### 📋 Code Style Standards

The project enforces:
- 📏 **Indentation**: 4 spaces
- 🔚 **Line endings**: LF (Unix-style)
- 🌐 **Encoding**: UTF-8
- ✂️ **Trailing whitespace**: Automatically trimmed
- 📄 **Final newline**: Automatically added
- 🎯 **C# conventions**: Microsoft's recommended style guidelines
- 🔧 **Braces**: Always required for control statements
- 🛡️ **Null checking**: Prefer pattern matching and null coalescing

