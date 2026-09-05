# Changelog

All notable changes to EssentialMediator are documented in this file.

The project follows [Semantic Versioning](https://semver.org/) for stable releases. Until the first stable release is published, current work remains under `[Unreleased]`.

## [Unreleased]

### Added

- Typed request and notification dispatch wrappers that keep reflection out of the normal dispatch hot path after wrapper creation.
- Independent mediator and handler lifetime configuration for the Microsoft dependency-injection integration.
- Built-in logging, performance, and DataAnnotations validation pipeline behaviors.
- Configurable slow-request threshold for the performance behavior.
- Parallel notification publishing with consistent exception and cancellation propagation.
- Fail-fast assembly scanning with loader exception details instead of silently skipping broken types.
- .NET 10 support with the SDK feature band pinned through `global.json`.
- BenchmarkDotNet project covering direct handler calls, mediator dispatch, pipeline behavior dispatch, and notification publishing.
- Repository-wide `.editorconfig` and CI whitespace-format verification.
- Central Package Management through `Directory.Packages.props`.
- NuGet package validation for all three public packages.
- NuGet package symbol generation (`.snupkg`) and source/debugging metadata.
- External package-consumer smoke testing that restores, builds, and runs using the generated NuGet packages.
- Coverage collection with minimum gates of 90% line coverage and 80% branch coverage.
- NuGet vulnerability auditing for direct and transitive dependencies, with high and critical advisories promoted to build errors.
- Controlled stable release workflow for `vMAJOR.MINOR.PATCH` tags on commits contained in `main`.
- NuGet.org Trusted Publishing via GitHub OIDC and short-lived publishing credentials.
- Security policy, contribution guide, and release guide.

### Changed

- Migrated all projects and GitHub Actions from .NET 9 to .NET 10.
- Aligned Microsoft.Extensions, ASP.NET Core OpenAPI, and Entity Framework Core dependencies with the .NET 10 stable line.
- Updated Swashbuckle packages to a version compatible with Microsoft.OpenApi v2 used by .NET 10.
- GitHub Actions now use Node 24-capable releases and are pinned to immutable commit SHAs.
- Package metadata and common MSBuild settings are centralized where practical.
- The README was rewritten to reflect the actual supported API, behavior semantics, package model, and benchmark policy.
- Release package versions are derived from stable release tags rather than requiring manual edits across package projects.
- NuGet publishing is performed in dependency order: Abstractions, Core, then Dependency Injection.

### Fixed

- Corrected request/notification handler-method cache collisions when one concrete handler type implements more than one handler contract.
- Corrected inconsistent notification failure semantics where synchronous handler failures could previously be swallowed while asynchronous failures propagated.
- Corrected notification cancellation propagation.
- Corrected `AddPerformanceBehavior(slowRequestThresholdMs)` so the configured threshold actually reaches `PerformanceBehavior`.
- Corrected dispatch of void requests registered as `IRequestHandler<TRequest>` after the typed dispatcher refactor.
- Removed dependence on Microsoft DI extension methods from the mediator core dispatch path.
- Rejected `IMediator` Singleton registration in the Microsoft DI integration to prevent root-scope/captive scoped-service resolution.
- Removed an empty sample project artifact that was not part of the solution.
- Removed the automatic `develop` to `main` pull-request workflow that could mask creation failures and promote integration changes toward release implicitly.

### Security

- Release publication no longer requires a long-lived NuGet API key stored in GitHub Actions secrets.
- GitHub Actions used by CI/release are pinned to exact commits to reduce supply-chain drift.
- High and critical known NuGet vulnerabilities fail restore/build gates.
- Release tags are validated for stable SemVer format and must point to commits contained in `main`.
- The release workflow does not hide duplicate-version publication failures.

## Release History

No stable release has been published yet.

When `1.0.0` is released, move the applicable entries from `[Unreleased]` into a dated section such as:

```text
## [1.0.0] - YYYY-MM-DD
```

and use the published version as the initial compatibility baseline for future package/API validation.
