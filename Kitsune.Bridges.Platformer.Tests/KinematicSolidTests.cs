using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer.Tests;

public class KinematicSolidTests
{
    [Fact]
    public void Step_MovesTowardEndPosition()
    {
        var scene = new Scene();
        var (entity, mover) = AddMover(scene, new Vector2(0, 0), new Vector2(100, 0));
        scene.Begin();

        mover.Step(1f / 60f);

        Assert.True(entity.Position.X > 0f);
        Assert.Equal(0f, entity.Position.Y);
    }

    [Fact]
    public void Step_PingPongsBetweenEndpoints()
    {
        var scene = new Scene();
        var (entity, mover) = AddMover(scene, new Vector2(0, 0), new Vector2(60, 0), speed: 60f);
        scene.Begin();

        for (var i = 0; i < 60; i++)
            mover.Step(1f / 60f);

        Assert.Equal(new Vector2(60, 0), entity.Position);

        for (var i = 0; i < 60; i++)
            mover.Step(1f / 60f);

        Assert.Equal(new Vector2(0, 0), entity.Position);
    }

    [Fact]
    public void Step_CarriesRiderWithDisplacement()
    {
        var scene = new Scene();
        var platform = new Entity { Position = new Vector2(0, 48) };
        platform.Add(new Hitbox(96, 32));
        platform.Add(new Solid());
        var mover = new KinematicSolid { EndPosition = new Vector2(60, 48), Speed = 60f };
        platform.Add(mover);
        scene.Add(platform);

        var (player, actor, _) = AddActor(scene, new Vector2(16, 16));
        scene.Begin();

        Assert.True(actor.IsRiding(platform));
        var startX = player.Position.X;
        mover.Step(1f / 60f);

        Assert.Equal(startX + 1f, player.Position.X);
    }

    [Fact]
    public void Step_PushesOverlappingNonRider()
    {
        var scene = new Scene();
        var floor = new Entity { Position = new Vector2(0, 80) };
        floor.Add(new Hitbox(200, 32));
        floor.Add(new Solid());
        scene.Add(floor);

        var platform = new Entity { Position = new Vector2(0, 48) };
        platform.Add(new Hitbox(64, 32));
        platform.Add(new Solid());
        var mover = new KinematicSolid { EndPosition = new Vector2(120, 48), Speed = 120f };
        platform.Add(mover);
        scene.Add(platform);

        var (obstacle, actor, _) = AddActor(scene, new Vector2(70, 48));
        scene.Begin();

        Assert.False(actor.IsRiding(platform));
        var startX = obstacle.Position.X;

        for (var i = 0; i < 30; i++)
            mover.Step(1f / 60f);

        Assert.True(obstacle.Position.X > startX + 5f);
    }

    [Fact]
    public void Step_SquishesUnhandledActorAgainstCeiling()
    {
        var scene = new Scene();
        var platform = new Entity { Position = new Vector2(0, 48) };
        platform.Add(new Hitbox(64, 32));
        platform.Add(new Solid());
        var mover = new KinematicSolid { EndPosition = new Vector2(0, 16), Speed = 240f };
        platform.Add(mover);
        scene.Add(platform);

        var ceiling = new Entity { Position = new Vector2(0, 0) };
        ceiling.Add(new Hitbox(64, 16));
        ceiling.Add(new Solid());
        scene.Add(ceiling);

        var (player, _, _) = AddActor(scene, new Vector2(16, 16));
        scene.Begin();

        for (var i = 0; i < 90; i++)
            mover.Step(1f / 60f);

        Assert.DoesNotContain(player, scene.Entities);
    }

    [Fact]
    public void Step_SquishHandlerPreventsRemoval()
    {
        var scene = new Scene();
        var platform = new Entity { Position = new Vector2(0, 48) };
        platform.Add(new Hitbox(64, 32));
        platform.Add(new Solid());
        var mover = new KinematicSolid { EndPosition = new Vector2(0, 16), Speed = 240f };
        platform.Add(mover);
        scene.Add(platform);

        var ceiling = new Entity { Position = new Vector2(0, 0) };
        ceiling.Add(new Hitbox(64, 16));
        ceiling.Add(new Solid());
        scene.Add(ceiling);

        var (player, actor, _) = AddActor(scene, new Vector2(16, 16));
        actor.OnSquish = () => true;
        scene.Begin();

        for (var i = 0; i < 90; i++)
            mover.Step(1f / 60f);

        Assert.Contains(player, scene.Entities);
    }

    [Fact]
    public void Added_KeepsSolidTagRegistration()
    {
        var scene = new Scene();
        var entity = new Entity { Position = new Vector2(0, 0) };
        entity.Add(new Solid());
        entity.Add(new KinematicSolid { EndPosition = new Vector2(40, 0) });
        scene.Add(entity);
        scene.Begin();

        Assert.Equal([entity], scene.Tracker.GetEntities(Solid.Tag));
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

    private static (Entity Entity, KinematicSolid Mover) AddMover(
        Scene scene,
        Vector2 start,
        Vector2 end,
        float speed = 120f)
    {
        var entity = new Entity { Position = start };
        entity.Add(new Solid());
        var mover = new KinematicSolid { EndPosition = end, Speed = speed };
        entity.Add(mover);
        scene.Add(entity);
        return (entity, mover);
    }
}
