# Getting started

## Install the SDK

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download).

## Clone and build

```bash
git clone https://github.com/shanedroid32/kitsune-engine.git
cd kitsune-engine
dotnet build
dotnet test
```

## Run the examples

Start with **HelloWorld** to confirm Foster windowing works:

```bash
dotnet run --project Examples/HelloWorld
```

**CoreDemo** exercises Kitsune Core — entities, hitbox collision, tag queries via `Tracker`, and depth-ordered rendering:

```bash
dotnet run --project Examples/CoreDemo
```

Use **A/D** to walk and **Space** or **W** to jump.

**PlatformerDemo** showcases the Platformer Bridge — coyote time, jump buffer, moving platforms, one-way and jump-through solids, and a one-screen level:

```bash
dotnet run --project Examples/PlatformerDemo
```

Use **A/D** to walk, **Space** or **W** to jump, and **S** to drop through jump-through platforms. Try the walk-off ledge, floor gap, orange one-way, cyan jump-through platform, and jumping off the moving platform for lift momentum.

## Use Kitsune in your project

Add project references to the packages you need:

```xml
<ProjectReference Include="path/to/Kitsune.Core/Kitsune.Core.csproj" />
<ProjectReference Include="path/to/Kitsune.Bridges.Platformer/Kitsune.Bridges.Platformer.csproj" />
```

Subclass `KitsuneApp`, assign a `Scene`, and compose entities with Core and Bridge components:

```csharp
using Kitsune.Bridges.Platformer;
using Kitsune.Core;

public sealed class MyGame : KitsuneApp
{
    public MyGame() : base("My Game")
    {
        var scene = new Scene();
        var player = new Entity { Position = new(100, 100) };
        player.Add(new Hitbox(32, 32));
        player.Add(new Actor());
        player.Add(new PlatformerBody());
        scene.Add(player);
        Scene = scene;
    }
}
```

`KitsuneApp` wires Foster's `Startup` / `Update` / `Render` / `Shutdown` to the active scene automatically.

## NuGet (0.3.0)

Packages are versioned `0.x` during active development. When published to [nuget.org](https://www.nuget.org/packages/Kitsune.Core):

```bash
dotnet add package Kitsune.Core
dotnet add package Kitsune.Bridges.Platformer
```

See [CHANGELOG.md](../CHANGELOG.md) for release notes.