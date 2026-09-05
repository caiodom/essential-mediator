# Releasing EssentialMediator

This document describes the stable release process for EssentialMediator. Releases are intentionally explicit: merging code does not publish packages. Publication occurs only when a stable SemVer tag is created on a commit contained in `main`.

## Release Model

The repository follows this flow:

```text
feature/fix/docs branch
        |
        v
develop
        |
        | release pull request
        v
main
        |
        | stable tag: vMAJOR.MINOR.PATCH
        v
GitHub Actions Release workflow
        |
        +--> validate tag and main ancestry
        +--> restore / format / build / test / coverage
        +--> package validation / pack / consumer smoke test
        +--> NuGet Trusted Publishing (OIDC)
        +--> NuGet.org
        +--> GitHub Release
```

`develop` is the integration branch. `main` represents releasable/stable code.

## One-Time NuGet.org Trusted Publishing Setup

The release workflow uses NuGet.org Trusted Publishing. It does not require a long-lived NuGet API key in GitHub.

Before the first release, configure a Trusted Publishing policy in the NuGet.org account that owns the packages.

Use the following GitHub identity values:

- repository owner: `caiodom`;
- repository: `essential-mediator`;
- workflow file: `release.yml`;
- GitHub environment: `release`.

Configure package scopes narrowly enough to allow the intended EssentialMediator packages and versions while avoiding unrelated package IDs. The project currently publishes:

- `EssentialMediator.Abstractions`;
- `EssentialMediator`;
- `EssentialMediator.Extensions.DependencyInjection`.

The workflow exchanges the GitHub OIDC token for a temporary NuGet API key immediately before publication.

## One-Time GitHub Setup

Create a GitHub Actions environment named:

```text
release
```

Define the Actions variable `NUGET_USER` either at repository level or on the `release` environment. Its value must be the NuGet.org profile name used by the Trusted Publishing policy, not an email address.

Do not create or store a `NUGET_API_KEY` secret for the normal release path.

For additional release safety, configure the `release` environment with deployment protection such as required reviewers before publication is allowed to proceed.

Repository branch protection should also require pull requests and CI for both `develop` and `main`, and should block force pushes and branch deletion.

## Versioning

Stable releases use Semantic Versioning tags in the exact form:

```text
vMAJOR.MINOR.PATCH
```

Examples:

```text
v1.0.0
v1.1.0
v1.1.1
v2.0.0
```

Pre-release tags such as `v1.0.0-beta.1` are intentionally rejected by the current release workflow.

The release workflow derives the NuGet/MSBuild version from the tag. Do not manually edit all package projects solely to create a release version.

Use the usual SemVer intent:

- **PATCH**: backward-compatible bug fix;
- **MINOR**: backward-compatible functionality;
- **MAJOR**: intentional breaking public API or behavioral change.

Before `1.0.0`, versioning remains pre-stable even though project files may contain package metadata used by local/CI pack operations.

## Preparing a Release

Before opening the release pull request:

1. Ensure the intended changes are already merged into `develop`.
2. Ensure `develop` CI is green.
3. Review public API changes and package metadata.
4. Review dependency/security state and NuGet audit output.
5. Confirm package validation succeeds for all three public packages.
6. Confirm the external NuGet consumer smoke test succeeds.
7. Review documentation and release notes/changelog for user-facing changes.
8. Run controlled-machine benchmarks if the release introduces or claims meaningful performance changes.

Do not create the release tag from `develop`.

## Release Pull Request

Create a release PR from:

```text
develop -> main
```

The PR should summarize the release contents and call out:

- user-visible behavior changes;
- public API additions/removals;
- bug fixes;
- dependency changes;
- security fixes;
- known limitations;
- migration steps for breaking releases.

Wait for all required checks and review the final diff before merging.

## Creating the Release Tag

After the release PR is merged, identify the exact commit at the head of `main` that should be released.

Create and push an annotated stable tag for that commit, for example:

```bash
git switch main
git pull --ff-only origin main
git tag -a v1.0.0 -m "EssentialMediator v1.0.0"
git push origin v1.0.0
```

The workflow refuses to release a tag whose commit is not contained in `main`.

Creating the tag is the explicit publication action. Do not create a release tag until the NuGet Trusted Publishing policy, GitHub `release` environment, and `NUGET_USER` variable are correctly configured.

## What the Release Workflow Does

For a valid stable tag, `.github/workflows/release.yml`:

1. validates the tag format;
2. verifies that the tagged commit is contained in `main`;
3. validates the non-sensitive Trusted Publishing configuration;
4. restores the solution and benchmark project;
5. verifies formatting;
6. builds in Release with warnings as errors;
7. verifies the benchmark project compiles;
8. runs the test suite with coverage;
9. enforces the line/branch coverage thresholds;
10. packs the three public NuGet packages with the version derived from the tag;
11. runs SDK package validation during packing;
12. executes the external package-consumer smoke test;
13. exchanges GitHub OIDC credentials for a temporary NuGet API key;
14. publishes packages in dependency order: Abstractions, Core, DI;
15. creates a GitHub Release and attaches `.nupkg` and `.snupkg` artifacts.

Package publication is intentionally not configured with `--skip-duplicate`. If the version already exists, the release fails visibly rather than pretending a duplicate publication succeeded.

## Failed Releases

NuGet packages are immutable after publication. Never attempt to overwrite a version that has already been pushed.

If the workflow fails **before** any package is published, fix the issue and decide whether the same tag can safely be retried according to the failure cause.

If the workflow fails **after one or more packages have been published**, do not reuse or move the published tag/version casually. Inspect which package versions reached NuGet.org, fix the cause, and normally prepare a new patch version so consumers see a coherent immutable release set.

Do not delete or force-move a published release tag as a routine recovery mechanism.

If a published package has a serious defect, use NuGet.org deprecation guidance where appropriate and release a corrected version. For a security issue, follow `SECURITY.md` and coordinate disclosure/remediation.

## After the First Stable Release

After `1.0.0` has been successfully published, create a focused follow-up PR that configures package/API compatibility baselines against the published stable version where appropriate. This turns the existing SDK package validation into a stronger guard against accidental breaking changes in later releases.

Also update the changelog/release documentation to treat `1.0.0` as the compatibility baseline.

## Release Verification

After a successful release:

- verify all three packages and the intended version are visible on NuGet.org;
- verify symbol packages/source navigation are available as expected;
- verify the GitHub Release exists and points to the correct tag/commit;
- install the published DI package in a clean external project and execute a minimal mediator flow;
- verify the generated release notes accurately describe the release;
- only then consider the release complete.

## Never Include in Release Configuration

Do not commit or paste into issues/PRs/logs:

- NuGet API keys;
- GitHub tokens;
- credentials or passwords;
- production secrets;
- private signing material.

Trusted Publishing exists specifically to avoid storing a persistent NuGet publishing credential in the repository or GitHub Actions secrets.
