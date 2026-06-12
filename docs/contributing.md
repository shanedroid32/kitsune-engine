# Contributing

Kitsune is solo-led. External contributions are welcome via focused pull requests; larger changes are best discussed first.

## Getting set up

1. Install the .NET 10 SDK.
2. Clone the repository and work inside `source/`.
3. Run `dotnet build`, `dotnet test`, and `dotnet format --verify-no-changes` before opening a PR.

## Scope and versioning

- Pre-1.0 packages use `0.x.y` semver. Minor bumps may include breaking API changes until `1.0.0`.
- Phase 1 is Core only. Platformer bridge gameplay, editor functionality, and Phase 2 systems belong in separate issues/PRs.
- Do not add agent artifacts, private domain docs, or research files under `source/`.

## Pull requests

- Keep PRs scoped to one concern.
- Include tests for Core logic changes.
- Ensure XML documentation on new public API members in shipped engine projects.

## Code style

The repo uses `.editorconfig` and CI enforces `dotnet format`. Match existing naming and file-scoped namespace style.