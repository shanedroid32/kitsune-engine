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
}
