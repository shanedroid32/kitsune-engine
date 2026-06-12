using Kitsune.Core;

namespace Kitsune.Bridges.Platformer.Tests;

public class SolidTests
{
    [Fact]
    public void Added_RegistersSolidTag()
    {
        var scene = new Scene();
        var wall = new Entity();
        wall.Add(new Solid());
        scene.Add(wall);
        scene.Begin();

        Assert.Equal([wall], scene.Tracker.GetEntities(Solid.Tag));
    }

    [Fact]
    public void Removed_UnregistersSolidTag()
    {
        var scene = new Scene();
        var wall = new Entity();
        var solid = new Solid();
        wall.Add(solid);
        scene.Add(wall);
        scene.Begin();

        wall.Remove(solid);

        Assert.Empty(scene.Tracker.GetEntities(Solid.Tag));
    }
}
