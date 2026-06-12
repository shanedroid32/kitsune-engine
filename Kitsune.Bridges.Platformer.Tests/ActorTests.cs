using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer.Tests;

public class ActorTests
{
    [Fact]
    public void MoveX_MovesFreelyWhenNoSolids()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 0));
        scene.Begin();

        var moved = actor.MoveX(12f);

        Assert.Equal(12f, moved);
        Assert.Equal(new Vector2(12, 0), entity.Position);
    }

    [Fact]
    public void MoveX_StopsAtSolidAndReturnsDistanceMoved()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 0));
        AddSolidWall(scene, new Vector2(64, 0), 32, 32);
        scene.Begin();

        var moved = actor.MoveX(100f);

        Assert.Equal(32f, moved);
        Assert.Equal(new Vector2(32, 0), entity.Position);
    }

    [Fact]
    public void MoveX_InvokesOnCollideWithBlockingEntity()
    {
        var scene = new Scene();
        var (_, actor, _) = AddActor(scene, new Vector2(0, 0));
        var wall = AddSolidWall(scene, new Vector2(64, 0), 32, 32);
        scene.Begin();

        Entity? blocker = null;
        actor.MoveX(100f, onCollide: e => blocker = e);

        Assert.Equal(wall, blocker);
    }

    [Fact]
    public void MoveY_StopsAtSolid()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 0));
        AddSolidWall(scene, new Vector2(0, 48), 32, 32);
        scene.Begin();

        var moved = actor.MoveY(100f);

        Assert.Equal(16f, moved);
        Assert.Equal(new Vector2(0, 16), entity.Position);
    }

    [Fact]
    public void TryCeilingCornerNudge_SlipsPastPartialCeilingLip()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 18));
        AddSolidWall(scene, new Vector2(31, 16), 1, 16);
        scene.Begin();

        Assert.Equal(0f, actor.MoveY(-1f));
        Assert.Equal(new Vector2(0, 18), entity.Position);

        var nudged = actor.TryCeilingCornerNudge();

        Assert.Equal(-1f, nudged);
        Assert.Equal(new Vector2(-1, 17), entity.Position);
    }

    [Fact]
    public void TryCeilingCornerNudge_DoesNotMoveUnderFullWidthCeiling()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 18));
        AddSolidWall(scene, new Vector2(0, 16), 32, 16);
        scene.Begin();

        Assert.Equal(0f, actor.MoveY(-1f));
        Assert.Equal(new Vector2(0, 18), entity.Position);

        var nudged = actor.TryCeilingCornerNudge();

        Assert.Equal(0f, nudged);
        Assert.Equal(new Vector2(0, 18), entity.Position);
    }

    [Fact]
    public void MoveX_BlockedByKinematicSolidMidPath()
    {
        var scene = new Scene();
        var platform = new Entity { Position = new Vector2(40, 0) };
        platform.Add(new Hitbox(32, 32));
        platform.Add(new Solid());
        var mover = new KinematicSolid { EndPosition = new Vector2(120, 0), Speed = 60f };
        platform.Add(mover);
        scene.Add(platform);

        var (player, actor, _) = AddActor(scene, new Vector2(0, 0));
        scene.Begin();

        for (var i = 0; i < 24; i++)
            mover.Step(1f / 60f);

        Assert.Equal(64f, platform.Position.X);

        var moved = actor.MoveX(100f);

        Assert.Equal(32f, moved);
        Assert.Equal(32f, player.Position.X);
    }

    [Fact]
    public void MoveX_PassesThroughTriggerSolid()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 0));
        AddTriggerWall(scene, new Vector2(16, 0), 32, 32);
        scene.Begin();

        var moved = actor.MoveX(32f);

        Assert.Equal(32f, moved);
        Assert.Equal(new Vector2(32, 0), entity.Position);
    }

    [Fact]
    public void IsRiding_TrueWhenStandingOnKinematicSolid()
    {
        var scene = new Scene();
        var platform = new Entity { Position = new Vector2(0, 48) };
        platform.Add(new Hitbox(64, 32));
        platform.Add(new Solid());
        platform.Add(new KinematicSolid { EndPosition = new Vector2(60, 48) });
        scene.Add(platform);

        var (_, actor, _) = AddActor(scene, new Vector2(16, 16));
        scene.Begin();

        Assert.True(actor.IsRiding(platform));
    }

    [Fact]
    public void IsRiding_FalseWhenAirborneAbovePlatform()
    {
        var scene = new Scene();
        var platform = new Entity { Position = new Vector2(0, 48) };
        platform.Add(new Hitbox(64, 32));
        platform.Add(new Solid());
        platform.Add(new KinematicSolid { EndPosition = new Vector2(60, 48) });
        scene.Add(platform);

        var (_, actor, _) = AddActor(scene, new Vector2(16, 0));
        scene.Begin();

        Assert.False(actor.IsRiding(platform));
    }

    [Fact]
    public void IsRiding_FalseWhenBelowOneWayPlatform()
    {
        var scene = new Scene();
        var platform = new Entity { Position = new Vector2(0, 32) };
        platform.Add(new Hitbox(64, 8));
        platform.Add(new OneWaySolid());
        scene.Add(platform);

        var (_, actor, _) = AddActor(scene, new Vector2(16, 40));
        scene.Begin();

        Assert.False(actor.IsRiding(platform));
    }

    [Fact]
    public void IsRiding_FalseOnJumpThroughWhenDropThroughRequested()
    {
        var scene = new Scene();
        var platform = new Entity { Position = new Vector2(0, 32) };
        platform.Add(new Hitbox(64, 16));
        platform.Add(new JumpThroughSolid());
        scene.Add(platform);

        var entity = new Entity { Position = new Vector2(16, 0) };
        var hitbox = new Hitbox(32, 32);
        var actor = new Actor();
        var body = new PlatformerBody { DropThroughRequested = true };
        entity.Add(hitbox);
        entity.Add(actor);
        entity.Add(body);
        scene.Add(entity);
        scene.Begin();

        Assert.False(actor.IsRiding(platform));
    }

    [Fact]
    public void Squish_RemovesEntityWhenUnhandled()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 0));
        scene.Begin();

        Assert.False(actor.Squish());
        Assert.DoesNotContain(entity, scene.Entities);
    }

    [Fact]
    public void Squish_SkipsRemoveWhenHandlerReturnsTrue()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(0, 0));
        scene.Begin();

        actor.OnSquish = () => true;

        Assert.True(actor.Squish());
        Assert.Contains(entity, scene.Entities);
    }

    [Fact]
    public void MoveX_ThrowsWhenEntityHasNoHitbox()
    {
        var scene = new Scene();
        var entity = new Entity();
        var actor = new Actor();
        entity.Add(actor);
        scene.Add(entity);
        scene.Begin();

        Assert.Throws<InvalidOperationException>(() => actor.MoveX(4f));
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

    private static Entity AddSolidWall(Scene scene, Vector2 position, float width, float height)
    {
        var wall = new Entity { Position = position };
        wall.Add(new Hitbox(width, height));
        wall.Add(new Solid());
        scene.Add(wall);
        return wall;
    }

    private static Entity AddTriggerWall(Scene scene, Vector2 position, float width, float height)
    {
        var trigger = new Entity { Position = position };
        trigger.Add(new Hitbox(width, height));
        trigger.Add(new TriggerSolid());
        scene.Add(trigger);
        return trigger;
    }
}
