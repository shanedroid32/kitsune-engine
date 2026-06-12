using System.Numerics;

namespace Kitsune.Core.Tests;

public class CollisionLayerTests
{
    [Fact]
    public void CollideCheck_IgnoresTargetWhenLayerNotInMask()
    {
        var scene = new Scene();
        var source = AddTaggedBox(scene, new Vector2(0, 0), 32, 32, "mover");
        var trigger = AddTaggedBox(scene, new Vector2(16, 16), 32, 32, "solid", CollisionLayer.Trigger);

        Assert.False(scene.CollideCheck(source, "solid", CollisionLayer.Geometry));
        Assert.Null(scene.CollideFirst(source, "solid", CollisionLayer.Geometry));
        Assert.Empty(scene.CollideAll(source, "solid", CollisionLayer.Geometry));
    }

    [Fact]
    public void CollideCheck_DetectsTriggerWithTriggerMask()
    {
        var scene = new Scene();
        var source = AddTaggedBox(scene, new Vector2(0, 0), 32, 32, "mover");
        var trigger = AddTaggedBox(scene, new Vector2(16, 16), 32, 32, "solid", CollisionLayer.Trigger);

        Assert.True(scene.CollideCheck(source, "solid", CollisionLayer.Trigger));
        Assert.Equal(trigger.Entity, scene.CollideFirst(source, "solid", CollisionLayer.Trigger));
    }

    [Fact]
    public void CollideCheck_GeometryBlocksWithGeometryMask()
    {
        var scene = new Scene();
        var source = AddTaggedBox(scene, new Vector2(0, 0), 32, 32, "mover");
        var wall = AddTaggedBox(scene, new Vector2(16, 16), 32, 32, "solid", CollisionLayer.Geometry);

        Assert.True(scene.CollideCheck(source, "solid", CollisionLayer.Geometry));
        Assert.Equal(wall.Entity, scene.CollideFirst(source, "solid", CollisionLayer.Geometry));
    }

    [Fact]
    public void CollideCheck_DefaultMaskIncludesAllBuiltInLayers()
    {
        var scene = new Scene();
        var source = AddTaggedBox(scene, new Vector2(0, 0), 32, 32, "mover");
        AddTaggedBox(scene, new Vector2(16, 16), 32, 32, "solid", CollisionLayer.Trigger);

        Assert.True(scene.CollideCheck(source, "solid"));
    }

    private static Hitbox AddTaggedBox(Scene scene, Vector2 position, float width, float height, string tag, uint layer = CollisionLayer.Geometry)
    {
        var entity = new Entity { Position = position };
        var hitbox = new Hitbox(width, height) { Layer = layer };
        entity.Add(hitbox);
        scene.Add(entity);
        scene.Tags.Register(entity, tag);
        return hitbox;
    }
}
