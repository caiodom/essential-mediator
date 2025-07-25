# EssentialMediator Source Code

This directory contains the core source code for the EssentialMediator library, organized into modular packages.

## Package Structure

### EssentialMediator.Abstractions
**Core interfaces and contracts with zero dependencies**

- Contains all public interfaces (`IMediator`, `IRequest`, `IRequestHandler`, etc.)
- Provides basic types like `Unit` for void operations
- Zero external dependencies for maximum compatibility
- Designed for referencing in domain/application layers

### EssentialMediator
**Core implementation of the mediator pattern**

- Main `Mediator` class implementation
- Custom exception types for better debugging
- Lightweight with minimal dependencies
- Auto-references `EssentialMediator.Abstractions`

### EssentialMediator.Extensions.DependencyInjection
**Microsoft.Extensions.DependencyInjection integration**

- `AddEssentialMediator()` extension methods
- Assembly scanning for automatic handler registration
- Configurable service lifetimes (Singleton, Scoped, Transient)
- Fluent configuration API
- Graceful error handling for assembly loading issues

## Architecture Principles

### Separation of Concerns
- **Abstractions** - Interfaces only, no implementation
- **Core** - Business logic and implementation
- **Extensions** - Framework-specific integrations

### Dependency Direction
```
EssentialMediator.Extensions.DependencyInjection
         ↓
EssentialMediator
         ↓
EssentialMediator.Abstractions
```

### Zero Dependencies Goal
- Abstractions package has zero external dependencies
- Core package minimizes dependencies
- Extension packages only depend on their specific frameworks

## Building the Source

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio 2022 or equivalent

### Build Commands
```bash
# Restore packages
dotnet restore

# Build all projects
dotnet build

# Build specific project
dotnet build src/EssentialMediator/EssentialMediator.csproj

# Build in Release mode
dotnet build -c Release
```

### Package Creation
```bash
# Create NuGet packages
dotnet pack -c Release

# Packages will be created in bin/Release/ directories
```

## Development Guidelines

### Code Style
- Use latest C# language features
- Enable nullable reference types
- Follow Microsoft coding conventions
- Include XML documentation for public APIs

### Testing
- Maintain high test coverage
- Write tests for all public APIs
- Include integration tests for DI scenarios
- Test error conditions and edge cases

### Documentation
- Update README files for any API changes
- Include XML documentation comments
- Provide usage examples in documentation
- Keep package descriptions current

## Package Dependencies

### EssentialMediator.Abstractions
- **No external dependencies** - Core design principle

### EssentialMediator
- `EssentialMediator.Abstractions` (current version)
- `Microsoft.Extensions.Logging.Abstractions` (minimal logging)

### EssentialMediator.Extensions.DependencyInjection
- `EssentialMediator` (current version)
- `Microsoft.Extensions.DependencyInjection.Abstractions`

## Versioning Strategy

- **Semantic Versioning** (SemVer) for all packages
- **Synchronized versions** across all packages
- **Breaking changes** only in major versions
- **Backwards compatibility** maintained within major versions
