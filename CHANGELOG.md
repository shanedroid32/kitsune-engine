# Changelog

All notable changes to Kitsune are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). During pre-1.0 development, minor version bumps may include breaking API changes.

## [Unreleased]

### Added

- **Kitsune.Core** — Scene collision query (`CollideCheck`, `CollideFirst`, `CollideAll`)

## [0.1.1] - 2026-06-12

### Added

- NuGet publish workflow (Trusted Publishing / OIDC) and shared package metadata
- GitHub issue templates and triage labels
- `CHANGELOG.md` and `Directory.Build.props`
- Pack verification step in CI

### Changed

- Fixed `getting-started.md` and `contributing.md` for public repo root
- NuGet push steps expand package globs correctly on Windows runners

## [0.1.0] - 2026-06-11

### Added

- **Kitsune.Core** — `Entity`, `Component`, `Scene`, `TagList`, `Tracker`, `Hitbox`, `KitsuneApp`
- Single-scene lifecycle with depth-ordered rendering hooks
- Axis-aligned hitbox collision
- **Kitsune.Bridges.Platformer** — stub assembly (packable, no gameplay yet)
- **Kitsune.Editor** — stub shell (not packable)
- **Examples** — `HelloWorld` (Foster wiring), `CoreDemo` (collision, tags, depth)
- **Kitsune.Core.Tests** — 11 unit tests
- CI — Windows build, test, and format check on `main`
- Public docs — README, getting started, design principles, contributing