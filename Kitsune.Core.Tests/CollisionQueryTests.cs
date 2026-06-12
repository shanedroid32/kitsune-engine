using System.Numerics;

namespace Kitsune.Core.Tests;

public class CollisionQueryTests
{
    [Fact]
    public void CollideCheck_ReturnsTrueWhenTaggedHitboxOverlaps()
    {
        var scene = new Scene();
        var source = AddTaggedBox(scene, new Vector2(0, 0), 32, 32, "mover");
        var blocker = AddTaggedBox(scene, new Vector2(16, 16), 32, 32, "solid");

        Assert.True(scene.CollideCheck(source, "solid"));
        Assert.Equal(blocker.Entity, scene.CollideFirst(source, "solid"));
    }

    [Fact]
    public void CollideCheck_ReturnsFalseWhenSeparated()
    {
        var scene = new Scene();
        var source = AddTaggedBox(scene, new Vector2(0, 0), 32, 32, "mover");
        AddTaggedBox(scene, new Vector2(128, 128), 32, 32, "solid");

        Assert.False(scene.CollideCheck(source, "solid"));
        Assert.Null(scene.CollideFirst(source, "solid"));
        Assert.Empty(scene.CollideAll(source, "solid"));
    }

    [Fact]
    public void CollideCheck_ExcludesSourceEntity()
    {
        var scene = new Scene();
        var entity = new Entity { Position = new Vector2(0, 0) };
        var hitbox = new Hitbox(32, 32);
        entity.Add(hitbox);
        scene.Add(entity);
        scene.Tags.Register(entity, "solid");

        Assert.False(scene.CollideCheck(hitbox, "solid"));
        Assert.Null(scene.CollideFirst(hitbox, "solid"));
    }

    [Fact]
    public void CollideFirst_ReturnsFirstOverlapInRegistrationOrder()
    {
        var scene = new Scene();
        var source = AddTaggedBox(scene, new Vector2(0, 0), 64, 64, "mover");

        var first = AddTaggedBox(scene, new Vector2(8, 8), 32, 32, "solid");
        var second = AddTaggedBox(scene, new Vector2(24, 24), 32, 32, "solid");

        Assert.Equal(first.Entity, scene.CollideFirst(source, "solid"));
        Assert.Equal([first.Entity!, second.Entity!], scene.CollideAll(source, "solid").ToArray());
    }

    [Fact]
    public void CollideQuery_SkipsTaggedEntitiesWithoutHitbox()
    {
        var scene = new Scene();
        var source = AddTaggedBox(scene, new Vector2(0, 0), 32, 32, "mover");

        var empty = new Entity { Position = new Vector2(8, 8) };
        scene.Add(empty);
        scene.Tags.Register(empty, "solid");

        Assert.False(scene.CollideCheck(source, "solid"));
    }

    [Fact]
    public void CollideQuery_ThrowsWhenSourceNotInScene()
    {
        var scene = new Scene();
        var detached = new Hitbox(32, 32);

        Assert.Throws<InvalidOperationException>(() => scene.CollideCheck(detached, "solid"));
        Assert.Throws<InvalidOperationException>(() => scene.CollideFirst(detached, "solid"));
        Assert.Throws<InvalidOperationException>(() => scene.CollideAll(detached, "solid").ToArray());
    }

    [Fact]
    public void CollideQuery_ThrowsWhenSourceBelongsToAnotherScene()
    {
        var sceneA = new Scene();
        var sceneB = new Scene();
        var hitbox = AddTaggedBox(sceneA, new Vector2(0, 0), 32, 32, "mover");

        Assert.Throws<InvalidOperationException>(() => sceneB.CollideCheck(hitbox, "solid"));
    }

    private static Hitbox AddTaggedBox(Scene scene, Vector2 position, float width, float height, string tag)
    {
        var entity = new Entity { Position = position };
        var hitbox = new Hitbox(width, height);
        entity.Add(hitbox);
        scene.Add(entity);
        scene.Tags.Register(entity, tag);
        return hitbox;
    }
}
