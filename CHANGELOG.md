# Changelog

All notable changes to Kitsune are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). During pre-1.0 development, minor version bumps may include breaking API changes.

## [Unreleased]

### Added

- **KitsuneApp** — scene stack foundation with `Replace(Scene)`; read-only `Scene` (top) and `Scenes` view (ADR-0025)
- **KitsuneApp** — `Push(Scene)` and `Pop()` overlay stack with deferred FIFO application during Update
- **Scene** — `UpdatesWhenCovered` and `RendersWhenCovered` for suspended covered scenes
- **KitsuneApp** — `OnSceneTransition` hook after each applied Push, Pop, or Replace

### Changed

- **KitsuneApp** — `Scene` property is read-only; use `Replace` instead of assignment (breaking)

## [0.3.1] - 2026-06-12

### Added

- **Actor** — `IsRiding` (probe-only), `OnSquish`, and `Squish` for kinematic resolution (ADR-0024)
- **KinematicSolid** — solid-authoritative push/carry/squish after each `Step` (ADR-0024)
- **PlatformerBody** — `LiftMomentumStorageFrames` for horizontal grace-frame momentum after jump (default 6)

### Changed

- **PlatformerBody** — kinematic carry removed; riding actors move with `KinematicSolid.Step`
- **PlatformerDemo** — zoned one-screen layout with crusher alcove, momentum-storage gap, and respawn on squish

### Fixed

- **PlatformerBody** — lift momentum horizontal transfer uses `Actor.MoveX` so geometry blocks inherited platform speed

## [0.3.0] - 2026-06-12

### Added

- **Kitsune.Bridges.Platformer** — `IDirectionalSolid` for direction-aware solid blocking; `Actor` consults it during movement
- **Kitsune.Bridges.Platformer** — `OneWaySolid` and `JumpThroughSolid` directional platform components
- **PlatformerBody** — `DropThroughRequested` for falling through jump-through platforms
- **PlatformerBody** — lift momentum on jump from `KinematicSolid` (instant 2D platform velocity)
- **PlatformerDemo** — one-way strip, jump-through platform (hold **S**), lift momentum off moving platform

### Changed

- **PlatformerBody** — grounded probe and carry use directional solid filtering

## [0.2.1] - 2026-06-12

### Added

- **Kitsune.Core** — `CollisionLayer` bit flags and `Hitbox.Layer` / `Hitbox.CollidesWith` for collision filtering
- **Kitsune.Core** — optional `layerMask` on `Scene` collision query methods
- **Kitsune.Bridges.Platformer** — `TriggerSolid` for non-blocking `"solid"` overlaps
- **PlatformerDemo** — walk-through trigger zone (player tints green while inside)

### Changed

- **Solid** — sets entity `Hitbox.Layer` to `CollisionLayer.Geometry` on add
- **Actor** / **PlatformerBody** — movement and grounding queries use `CollisionLayer.Geometry` only

## [0.2.0] - 2026-06-12

### Added

- **Kitsune.Core** — Scene collision query (`CollideCheck`, `CollideFirst`, `CollideAll`)
- **Kitsune.Bridges.Platformer** — `Solid` and `Actor` with pixel-step movement resolution
- **Kitsune.Bridges.Platformer** — `PlatformerBody` for gravity, jumping, and grounded state
- **PlatformerBody** — coyote time (`CoyoteTimeFrames`, `CoyoteFramesRemaining`)
- **PlatformerBody** — jump buffer (`JumpBufferFrames`, `JumpBufferFramesRemaining`)
- **Actor** — `TryCeilingCornerNudge` for one-pixel left/right ceiling corner correction
- **PlatformerBody** — ceiling corner nudge during upward solid collision while jumping
- **Examples/PlatformerDemo** — runnable Platformer Bridge shell (floor, boundaries, WASD player)
- **Examples/PlatformerDemo** — one-screen level with gap, walk-off ledge, and elevated platforms
- **KinematicSolid** — ping-pong kinematic motion for moving solid entities
- **KinematicSolid** — documented simulation order; PlatformerDemo moving platform blocks the player
- **PlatformerBody** — carries grounded actors on `KinematicSolid` via `FrameDisplacement`

### Changed

- **PlatformerBody** — snappier default gravity/jump speeds for platformer feel
- **CoreDemo** — faster walk speed, lower platform, tuned jump to reach the elevated platform
- **CoreDemo** — higher jump arc (`JumpSpeed` 620, `Gravity` 950)
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