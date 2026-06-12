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
