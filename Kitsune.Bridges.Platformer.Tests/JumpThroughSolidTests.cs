using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer.Tests;

public class JumpThroughSolidTests
{
    [Fact]
    public void Added_RegistersSolidTagAndGeometryLayer()
    {
        var scene = new Scene();
        var entity = new Entity();
        var hitbox = new Hitbox(16, 16);
        entity.Add(hitbox);
        entity.Add(new JumpThroughSolid());
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
        AddJumpThrough(scene, new Vector2(0, 32), 64, 8);
        scene.Begin();

        var moved = actor.MoveY(-24f);

        Assert.Equal(24f, moved);
        Assert.Equal(new Vector2(0, 24), entity.Position);
    }

    [Fact]
    public void Actor_LandsWhenFallingOntoTop()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 0));
        AddJumpThrough(scene, new Vector2(0, 32), 64, 8);
        scene.Begin();

        Assert.Equal(0f, actor.MoveY(48f));
        Assert.Equal(new Vector2(0, 0), entity.Position);
    }

    [Fact]
    public void Actor_DropsThroughWhenDropThroughRequested()
    {
        var scene = new Scene();
        var (entity, actor, body) = AddBody(scene, new Vector2(0, 16));
        AddJumpThroughFloor(scene, new Vector2(0, 48), 64, 32);
        scene.Begin();
        Settle(body);

        Assert.True(body.IsGrounded);
        Assert.Equal(16f, entity.Position.Y);

        body.DropThroughRequested = true;
        var moved = actor.MoveY(16f);

        Assert.Equal(16f, moved);
        Assert.Equal(new Vector2(0, 32), entity.Position);
    }

    [Fact]
    public void Actor_StaysOnPlatformWhenDropThroughNotRequested()
    {
        var scene = new Scene();
        var (entity, actor, body) = AddBody(scene, new Vector2(0, 16));
        AddJumpThroughFloor(scene, new Vector2(0, 48), 64, 32);
        scene.Begin();
        Settle(body);

        Assert.True(body.IsGrounded);

        var moved = actor.MoveY(8f);

        Assert.Equal(0f, moved);
        Assert.Equal(new Vector2(0, 16), entity.Position);
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

    private static (Entity Entity, Actor Actor, PlatformerBody Body) AddBody(Scene scene, Vector2 position)
    {
        var entity = new Entity { Position = position };
        var hitbox = new Hitbox(32, 32);
        var actor = new Actor();
        var body = new PlatformerBody();
        entity.Add(hitbox);
        entity.Add(actor);
        entity.Add(body);
        scene.Add(entity);
        return (entity, actor, body);
    }

    private static void Settle(PlatformerBody body)
    {
        for (var i = 0; i < 120; i++)
            body.Simulate(1f / 60f);
    }

    private static void AddJumpThroughFloor(Scene scene, Vector2 position, float width, float height)
    {
        var floor = new Entity { Position = position };
        floor.Add(new Hitbox(width, height));
        floor.Add(new JumpThroughSolid());
        scene.Add(floor);
    }

    private static void AddJumpThrough(Scene scene, Vector2 position, float width, float height)
    {
        var platform = new Entity { Position = position };
        platform.Add(new Hitbox(width, height));
        platform.Add(new JumpThroughSolid());
        scene.Add(platform);
    }
}
