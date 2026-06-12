# Kitsune

Kitsune is a 2D game engine for C# built on [Foster](https://www.nuget.org/packages/FosterFramework). It modernizes proven Monocle-inspired patterns — Scene, Entity, Component, tags, and collision — as layered .NET libraries you can adopt incrementally.

Phase 1 ships **Kitsune.Core** only: entity hierarchy, single-scene lifecycle, tag queries, axis-aligned hitboxes, and depth-ordered rendering hooks.

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

Use **A/D** to walk and **Space** or **W** to jump. Try the walk-off ledge on the left (coyote time), the floor gap (jump buffer), and stand on the blue moving platform to ride it.

## Project layout

```
kitsune-engine/
├── Kitsune.Core/                 # Engine runtime (Scene, Entity, Component, Hitbox)
├── Kitsune.Bridges.Platformer/   # Platformer bridge stub (Phase 2)
├── Kitsune.Editor/               # Editor shell stub
├── Kitsune.Core.Tests/           # Unit tests
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