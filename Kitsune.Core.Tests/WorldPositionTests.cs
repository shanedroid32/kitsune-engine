using System.Numerics;

namespace Kitsune.Core.Tests;

public class WorldPositionTests
{
    [Fact]
    public void WorldPosition_AccumulatesParentOffset()
    {
        var parent = new Entity { Position = new Vector2(100, 50) };
        var child = new Entity { Position = new Vector2(20, 10) };

        parent.Add(child);

        Assert.Equal(new Vector2(100, 50), parent.WorldPosition);
        Assert.Equal(new Vector2(120, 60), child.WorldPosition);
    }

    [Fact]
    public void Hitbox_GetBounds_UsesWorldPosition()
    {
        var parent = new Entity { Position = new Vector2(10, 0) };
        var child = new Entity { Position = new Vector2(5, 5) };
        parent.Add(child);

        var hitbox = new Hitbox(16, 16);
        child.Add(hitbox);

        var bounds = hitbox.GetBounds();
        Assert.Equal(15f, bounds.X);
        Assert.Equal(5f, bounds.Y);
    }
}
