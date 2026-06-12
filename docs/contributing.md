# Contributing

Kitsune is solo-led. External contributions are welcome via focused pull requests; larger changes are best discussed first.

## Getting set up

1. Install the .NET 10 SDK.
2. Clone the repository (`git clone https://github.com/shanedroid32/kitsune-engine.git`).
3. Run `dotnet build`, `dotnet test`, and `dotnet format --verify-no-changes` before opening a PR.

## Reporting issues

Use [GitHub Issues](https://github.com/shanedroid32/kitsune-engine/issues) for bugs, feature requests, and contributor-facing work. Pick the appropriate template when filing.

Triage labels:

| Label | Meaning |
|-------|---------|
| `needs-triage` | Maintainer needs to evaluate |
| `needs-info` | Waiting on reporter for details |
| `ready-for-agent` | Fully specified, suitable for an AFK agent |
| `ready-for-human` | Requires human implementation |
| `wontfix` | Will not be actioned |

## Scope and versioning

- Pre-1.0 packages use `0.x.y` semver. Minor bumps may include breaking API changes until `1.0.0`.
- Phase 1 is Core only. Platformer bridge gameplay, editor functionality, and Phase 2 systems belong in separate issues/PRs.
- Do not add agent artifacts, private domain docs, or research files to the public repo.

## Pull requests

- Keep PRs scoped to one concern.
- Include tests for Core logic changes.
- Ensure XML documentation on new public API members in shipped engine projects.
- Update [CHANGELOG.md](../CHANGELOG.md) under **Unreleased** for user-visible changes.

## Releases and NuGet publish

Maintainers publish packages from tagged releases:

1. Bump `<Version>` in the relevant `.csproj` files and finalize [CHANGELOG.md](../CHANGELOG.md).
2. Create a GitHub release (tag `v0.x.y`) — this triggers the **Publish** workflow.
3. The workflow pushes `Kitsune.Core` and `Kitsune.Bridges.Platformer` to nuget.org.

Publishing uses [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) (OIDC) — no long-lived API keys.

**One-time setup:**

1. On [nuget.org](https://www.nuget.org), open **Trusted Publishing** and add a policy:
   - **Repository owner:** `shanedroid32`
   - **Repository:** `kitsune-engine`
   - **Workflow file:** `publish.yml`
2. In GitHub repo settings → Secrets, add `NUGET_USER` with your nuget.org **profile name** (not your email).

Use **Actions → Publish → Run workflow** for a manual publish without a release. Request the OIDC login step shortly before push — temporary keys expire after one hour.

## Code style

The repo uses `.editorconfig` and CI enforces `dotnet format`. Match existing naming and file-scoped namespace style.