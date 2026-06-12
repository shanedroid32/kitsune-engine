using System.Numerics;

namespace Kitsune.Core.Tests;

public class HitboxTests
{
    [Fact]
    public void Overlaps_ReturnsTrueForIntersectingRects()
    {
        var left = CreateEntityWithHitbox(new Vector2(0, 0), 32, 32);
        var right = CreateEntityWithHitbox(new Vector2(16, 16), 32, 32);

        Assert.True(Hitbox.Overlaps(left, right));
    }

    [Fact]
    public void Overlaps_ReturnsFalseForSeparatedRects()
    {
        var left = CreateEntityWithHitbox(new Vector2(0, 0), 32, 32);
        var right = CreateEntityWithHitbox(new Vector2(64, 64), 32, 32);

        Assert.False(Hitbox.Overlaps(left, right));
    }

    private static Hitbox CreateEntityWithHitbox(Vector2 position, float width, float height)
    {
        var entity = new Entity { Position = position };
        var hitbox = new Hitbox(width, height);
        entity.Add(hitbox);
        return hitbox;
    }
}
