# Contributing to EssentialMediator

Thanks for considering a contribution to EssentialMediator. The project aims to stay small, predictable, and easy to consume, so changes should solve a concrete problem without adding unnecessary abstraction.

## Development Requirements

Use the .NET SDK selected by the repository root `global.json`. The project currently targets .NET 10.

Clone the repository and restore dependencies:

```bash
git clone https://github.com/caiodom/essential-mediator.git
cd essential-mediator
dotnet restore EssentialMediator.sln
dotnet restore benchmarks/EssentialMediator.Benchmarks/EssentialMediator.Benchmarks.csproj
```

## Branch and Pull Request Flow

`main` is reserved for released code and `develop` is the integration branch.

1. Update your local `develop` branch.
2. Create a focused branch from the current `develop` head.
3. Make one logical change per pull request.
4. Add or update tests for behavioral changes.
5. Run the validation commands below.
6. Open the pull request against `develop`.

Do not implement directly on `main` or `develop`.

Useful branch prefixes include:

- `fix/` for bug fixes;
- `feat/` for new capabilities;
- `build/` for build and packaging changes;
- `ci/` for workflow changes;
- `docs/` for documentation;
- `chore/` for focused maintenance.

## Local Validation

Run the same core checks enforced by CI:

```bash
dotnet format whitespace EssentialMediator.sln --no-restore --verify-no-changes

dotnet build EssentialMediator.sln \
  --no-restore \
  --configuration Release \
  --warnaserror

dotnet build benchmarks/EssentialMediator.Benchmarks/EssentialMediator.Benchmarks.csproj \
  --no-restore \
  --configuration Release \
  --warnaserror

dotnet test tests/EssentialMediator.Tests/EssentialMediator.Tests.csproj \
  --no-build \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

python3 scripts/check-coverage.py \
  --pattern "TestResults/**/coverage.cobertura.xml" \
  --min-line 90 \
  --min-branch 80
```

CI also packs all three public NuGet packages and executes an external package-consumer smoke test.

## Coding Guidelines

- Keep code, comments, public XML documentation, commit messages, and repository documentation in English.
- Prefer clear framework-native .NET patterns over custom abstractions unless the abstraction solves a demonstrated problem.
- Preserve nullable-reference-type correctness.
- Treat compiler warnings as errors rather than suppressing them globally.
- Avoid reflection in dispatch hot paths unless there is a measured and justified reason.
- Keep public APIs small and intentional.
- Do not silently swallow handler, notification, cancellation, configuration, or assembly-loading failures.
- Do not add a dependency when the .NET SDK or BCL already provides the required capability.

## Tests

Behavioral fixes should include a regression test that fails without the fix. New public behavior should cover both the expected path and important failure semantics.

The CI coverage floor is currently:

- 90% line coverage;
- 80% branch coverage.

Coverage is a guardrail, not a substitute for useful assertions.

## Public API and Package Changes

EssentialMediator publishes three packages:

- `EssentialMediator.Abstractions`;
- `EssentialMediator`;
- `EssentialMediator.Extensions.DependencyInjection`.

Changes to public contracts require extra care because consumers may compile against them independently. Avoid breaking public API changes unless they are intentionally planned for an appropriate major release.

The repository uses SDK package validation during `dotnet pack`. After the first stable release is available, compatibility validation can use that published version as a baseline.

Package versions for external dependencies are centrally managed in `Directory.Packages.props`. Do not add version attributes back to individual `PackageReference` items.

## Dependencies and Security

Before adding a dependency, confirm that it is necessary, actively maintained, compatible with the supported target framework, and appropriate for redistribution in a public NuGet package.

NuGet Audit is enabled for direct and transitive dependencies. High and critical known vulnerabilities fail the build. Do not suppress vulnerability warnings without documenting a concrete reason and mitigation.

For security vulnerabilities, follow `SECURITY.md` rather than posting sensitive details in a public issue.

## Benchmarks

Performance claims should be backed by reproducible BenchmarkDotNet results. Hosted CI runners are useful for proving that benchmarks compile, but their timing results should not be treated as stable performance evidence.

Run benchmarks on a controlled machine with:

```bash
dotnet run --project benchmarks/EssentialMediator.Benchmarks/EssentialMediator.Benchmarks.csproj -c Release
```

When publishing benchmark results, include hardware, operating system, .NET runtime/SDK version, benchmark commit SHA, and BenchmarkDotNet output.

## Releases

Normal pull requests and merges do not publish packages.

Stable releases use tags in the form `vMAJOR.MINOR.PATCH` on commits contained in `main`. The release workflow re-runs validation before publishing packages to NuGet.org and creating the GitHub Release.

Only maintainers should create release tags.

## Pull Request Checklist

Before requesting merge, confirm that:

- the change is focused and the PR targets `develop`;
- formatting verification passes;
- the Release build has zero warnings;
- tests pass and coverage remains above the repository thresholds;
- public API or package changes are intentional and documented;
- new dependencies are justified;
- no secrets, credentials, generated binaries, or unrelated changes are included.
