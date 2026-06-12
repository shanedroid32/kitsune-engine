using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer.Tests;

public class OneWaySolidTests
{
    [Fact]
    public void Added_RegistersSolidTagAndGeometryLayer()
    {
        var scene = new Scene();
        var entity = new Entity();
        var hitbox = new Hitbox(16, 16);
        entity.Add(hitbox);
        entity.Add(new OneWaySolid());
        scene.Add(entity);
        scene.Begin();

        Assert.Equal([entity], scene.Tracker.GetEntities(Solid.Tag).ToArray());
        Assert.Equal(CollisionLayer.Geometry, hitbox.Layer);
    }

    [Fact]
    public void Actor_RisesThroughFromBelow()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 48));
        AddOneWay(scene, new Vector2(0, 32), 64, 8);
        scene.Begin();

        var moved = actor.MoveY(-24f);

        Assert.Equal(24f, moved);
        Assert.Equal(new Vector2(0, 24), entity.Position);
    }

    [Fact]
    public void Actor_StopsWhenFallingOntoOneWay()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 0));
        AddOneWay(scene, new Vector2(0, 32), 64, 8);
        scene.Begin();

        var moved = actor.MoveY(48f);

        Assert.Equal(0f, moved);
        Assert.Equal(new Vector2(0, 0), entity.Position);
    }

    private static (Entity Entity, Actor Actor, Hitbox Hitbox) AddActor(Scene scene, Vector2 position)
    {
        var entity = new Entity { Position = position };
        var hitbox = new Hitbox(32, 32);
        var actor = new Actor();
        entity.Add(hitbox);
        entity.Add(actor);
        scene.Add(entity);
        return (entity, actor, hitbox);
    }

    private static void AddOneWay(Scene scene, Vector2 position, float width, float height)
    {
        var platform = new Entity { Position = position };
        platform.Add(new Hitbox(width, height));
        platform.Add(new OneWaySolid());
        scene.Add(platform);
    }
}
