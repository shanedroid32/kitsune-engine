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

Use **A/D** to walk, **Space** or **W** to jump, and **S** to drop through jump-through platforms. The demo level is zoned left-to-right: coyote ledge, jump-buffer gap, one-way, jump-through terrace, a horizontal mover with a gap that needs lift momentum storage, and a rising crusher alcove (jump off before the red lip).

## Use Kitsune in your project

Add project references to the packages you need:

```xml
<ProjectReference Include="path/to/Kitsune.Core/Kitsune.Core.csproj" />
<ProjectReference Include="path/to/Kitsune.Bridges.Platformer/Kitsune.Bridges.Platformer.csproj" />
```

Subclass `KitsuneApp`, load a scene with `Replace`, and compose entities with Core and Bridge components:

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
        Replace(scene);
    }
}
```

`KitsuneApp` owns a scene stack and wires Foster's loop to it:

- **`Replace(scene)`** — end every scene on the stack and make one scene the sole top (level loads, game over → menu)
- **`Push(scene)`** / **`Pop()`** — overlay without tearing down covered scenes (pause menus); changes during `Update` apply after that pass
- **`Scene`** — read-only handle to the top scene; **`Scenes`** — read-only bottom-to-top stack view
- **`OnSceneTransition`** — override to reset input, play audio, or persist state after each stack change

Covered scenes stay begun but frozen unless `UpdatesWhenCovered` is set; they render underneath overlays by default (`RendersWhenCovered`).

## NuGet (0.4.0)

Packages are versioned `0.x` during active development. When published to [nuget.org](https://www.nuget.org/packages/Kitsune.Core):

```bash
dotnet add package Kitsune.Core
dotnet add package Kitsune.Bridges.Platformer
```

See [CHANGELOG.md](../CHANGELOG.md) for release notes.