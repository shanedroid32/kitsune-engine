using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer.Tests;

public class DirectionalSolidTests
{
    [Fact]
    public void Actor_RespectsDirectionalSolid_BlocksHorizontalPassesVertical()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(32, 0));
        AddDirectionalWall(scene, new Vector2(32, 0), 32, 32, blocksHorizontal: true);
        scene.Begin();

        Assert.Equal(0f, actor.MoveX(8f));
        Assert.Equal(new Vector2(32, 0), entity.Position);

        var movedY = actor.MoveY(8f);
        Assert.Equal(8f, movedY);
        Assert.Equal(new Vector2(32, 8), entity.Position);
    }

    [Fact]
    public void Actor_UnconditionalSolidStillBlocksAllAxes()
    {
        var scene = new Scene();
        var (entity, actor, _) = AddActor(scene, new Vector2(32, 0));
        AddSolidWall(scene, new Vector2(32, 0), 32, 32);
        scene.Begin();

        Assert.Equal(0f, actor.MoveX(8f));
        Assert.Equal(new Vector2(32, 0), entity.Position);

        Assert.Equal(0f, actor.MoveY(8f));
        Assert.Equal(new Vector2(32, 0), entity.Position);
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

    private static Entity AddDirectionalWall(Scene scene, Vector2 position, float width, float height, bool blocksHorizontal)
    {
        var wall = new Entity { Position = position };
        wall.Add(new Hitbox(width, height));
        wall.Add(new StubDirectionalSolid(blocksHorizontal));
        scene.Add(wall);
        scene.Tags.Register(wall, Solid.Tag);
        return wall;
    }

    private sealed class StubDirectionalSolid(bool blocksHorizontal) : Component, IDirectionalSolid
    {
        public bool BlocksMovement(Hitbox actorHitbox, Vector2 moveStep, PlatformerBody? body) =>
            blocksHorizontal ? moveStep.X != 0f : moveStep.Y != 0f;
    }
}
