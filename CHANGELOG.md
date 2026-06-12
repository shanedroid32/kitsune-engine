# Changelog

All notable changes to Kitsune are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). During pre-1.0 development, minor version bumps may include breaking API changes.

## [Unreleased]

### Added

- NuGet publish workflow and shared package metadata
- GitHub issue templates and triage labels
- This changelog

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