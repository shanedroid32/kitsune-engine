# Changelog

All notable changes to Kitsune are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). During pre-1.0 development, minor version bumps may include breaking API changes.

## [Unreleased]

### Added

- **Kitsune.Core** — Scene collision query (`CollideCheck`, `CollideFirst`, `CollideAll`)
- **Kitsune.Bridges.Platformer** — `Solid` and `Actor` with pixel-step movement resolution
- **Kitsune.Bridges.Platformer** — `PlatformerBody` for gravity, jumping, and grounded state

### Changed

- **PlatformerBody** — lower default gravity/jump speeds; CoreDemo spawns on floor with input before physics
- **PlatformerBody** — airborne while rising (`IsGrounded` false when moving up)
- **CoreDemo** — WASD + Space/W controls (replaces arrow keys)
- **CoreDemo** — player movement uses Platformer Bridge `Actor`/`Solid` instead of inline collision logic

### Fixed

- **PlatformerBody** — jump velocity no longer zeroed by fractional pixel-step rounding (only on solid collision)

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