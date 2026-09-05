# Security Policy

Security reports are taken seriously. Please avoid disclosing vulnerability details publicly before a fix is available.

## Supported Versions

EssentialMediator has not published its first stable release yet. Until `1.0.0` is released, security fixes target the latest development version only.

After the first stable release, the latest stable release line will receive security fixes. Older release lines may receive fixes when the issue is severe and a safe backport is practical.

| Version | Security support |
| --- | --- |
| Latest stable release | Supported |
| Current `develop` branch before `1.0.0` | Pre-release, best effort |
| Older or unsupported releases | Not guaranteed |

## Reporting a Vulnerability

Prefer GitHub's private vulnerability reporting / Security Advisory flow for this repository when it is available. Include enough information for the issue to be reproduced and assessed:

- affected package and version or commit SHA;
- impact and realistic attack scenario;
- minimal reproduction steps or proof of concept;
- relevant runtime and operating-system details;
- suggested mitigation, if known.

Do **not** include secrets, production credentials, personal data, or data belonging to third parties in a report or reproduction.

If a private reporting option is not available, open a public issue containing **no vulnerability details** and request a private contact channel. Do not publish exploit steps, payloads, or sensitive logs in that issue.

## Scope

Security reports are especially useful when they concern:

- request, notification, or pipeline dispatch behavior that crosses an intended trust boundary;
- dependency-injection lifetime behavior that can expose or incorrectly retain scoped data;
- assembly scanning or handler discovery that can load or execute unintended code;
- package, build, release, or dependency-supply-chain integrity;
- vulnerabilities in direct or transitive dependencies used by the published packages.

Security issues in the sample application that do not affect the EssentialMediator packages themselves may be handled as normal bugs unless they demonstrate a vulnerability in the library.

## Coordinated Disclosure

Please allow maintainers reasonable time to reproduce, assess, fix, test, and release a remediation before public disclosure. When a vulnerability is confirmed, the project will aim to document affected versions, remediation, and upgrade guidance in the corresponding security advisory or release notes.

## Security Controls in the Repository

The repository CI currently includes warnings-as-errors, automated tests and coverage thresholds, NuGet vulnerability auditing, package validation, external package-consumer smoke testing, pinned GitHub Action SHAs, and a tag-gated release workflow. These controls reduce risk but do not replace responsible security review or vulnerability reporting.
