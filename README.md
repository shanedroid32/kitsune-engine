# Kitsune

Kitsune is a 2D game engine for C# built on [Foster](https://www.nuget.org/packages/FosterFramework). It modernizes proven Monocle-inspired patterns — Scene, Entity, Component, tags, and collision — as layered .NET libraries you can adopt incrementally.

**Kitsune.Core** provides entity hierarchy, single-scene lifecycle, tag queries, axis-aligned hitboxes, and depth-ordered rendering hooks. **Kitsune.Bridges.Platformer** (0.3.0) adds `Solid`, `Actor`, `PlatformerBody`, forgiveness systems, moving platforms, collision layers, directional platforms (`OneWaySolid`, `JumpThroughSolid`), and lift momentum.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Build and run

```bash
dotnet build
dotnet test
```

**HelloWorld** — minimal Foster window (circle demo):

```bash
dotnet run --project Examples/HelloWorld
```

**CoreDemo** — Phase 1 Core proof (entities, collision, tags, depth ordering):

```bash
dotnet run --project Examples/CoreDemo
```

Use WASD and Space to move the orange player (A/D walk, Space/W jump). Red and blue rectangles demonstrate depth ordering (red draws on top).

**PlatformerDemo** — Platformer Bridge playground (coyote time, jump buffer, platforms):

```bash
dotnet run --project Examples/PlatformerDemo
```

Use **A/D** to walk, **Space** or **W** to jump, and **S** to drop through cyan jump-through platforms. The level is laid out in zones left-to-right: coyote ledge, jump-buffer gap, one-way crossing, jump-through terrace, a blue mover with a momentum-storage gap to the island, and a rising crusher alcove (red lip — jump off or respawn).

## Project layout

```
kitsune-engine/
├── Kitsune.Core/                 # Engine runtime (Scene, Entity, Component, Hitbox)
├── Kitsune.Bridges.Platformer/   # Platformer Bridge (Solid, Actor, PlatformerBody)
├── Kitsune.Editor/               # Editor shell stub
├── Kitsune.Core.Tests/           # Core unit tests
├── Kitsune.Bridges.Platformer.Tests/  # Bridge unit tests
├── Examples/
│   ├── HelloWorld/               # Foster wiring smoke test
│   ├── CoreDemo/                 # Core feature demo
│   └── PlatformerDemo/           # Platformer Bridge demo level
├── docs/                         # Public documentation
└── kitsune.slnx
```

## Documentation

- [Getting started](docs/getting-started.md)
- [Design principles](docs/design-principles.md)
- [Contributing](docs/contributing.md)
- [Changelog](CHANGELOG.md)

## License

MIT — see [LICENSE](LICENSE).