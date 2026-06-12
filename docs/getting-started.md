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

Then run **CoreDemo** to see Kitsune Core in action — entities, hitbox collision, tag queries via `Tracker`, and depth-ordered rendering:

```bash
dotnet run --project Examples/CoreDemo
```

## Use Kitsune in your project

Add a project reference to `Kitsune.Core`:

```xml
<ProjectReference Include="path/to/Kitsune.Core/Kitsune.Core.csproj" />
```

Subclass `KitsuneApp`, assign a `Scene`, and add `Entity` instances with `Component` types:

```csharp
using Kitsune.Core;

public sealed class MyGame : KitsuneApp
{
    public MyGame() : base("My Game")
    {
        var scene = new Scene();
        var player = new Entity { Position = new(100, 100) };
        scene.Add(player);
        Scene = scene;
    }
}
```

`KitsuneApp` wires Foster's `Startup` / `Update` / `Render` / `Shutdown` to the active scene automatically.

## NuGet (0.1.0)

Packages are versioned `0.x` during active development. When published to [nuget.org](https://www.nuget.org/packages/Kitsune.Core):

```bash
dotnet add package Kitsune.Core
```

See [CHANGELOG.md](../CHANGELOG.md) for release notes.