using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer.Tests;

public class PlatformerBodyTests
{
    [Fact]
    public void Simulate_FallsWithGravityUntilLanding()
    {
        var scene = new Scene();
        var startY = 0f;
        var (entity, body, _) = AddBody(scene, new Vector2(0, startY));
        AddSolidFloor(scene, new Vector2(0, 48), 64, 32);
        scene.Begin();

        for (var i = 0; i < 120; i++)
            body.Simulate(1f / 60f);

        Assert.True(entity.Position.Y > startY);
        Assert.Equal(16f, entity.Position.Y);
        Assert.True(body.IsGrounded);
    }

    [Fact]
    public void Simulate_IsGroundedWhenRestingOnSolid()
    {
        var scene = new Scene();
        var (entity, body, _) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 64, 32);
        scene.Begin();

        for (var i = 0; i < 120; i++)
            body.Simulate(1f / 60f);

        Assert.Equal(16f, entity.Position.Y);
        Assert.True(body.IsGrounded);
    }

    [Fact]
    public void Simulate_JumpClearsGroundBriefly()
    {
        var scene = new Scene();
        var (entity, body, _) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 64, 32);
        scene.Begin();

        for (var i = 0; i < 120; i++)
            body.Simulate(1f / 60f);

        Assert.True(body.IsGrounded);
        var groundedY = entity.Position.Y;

        body.JumpRequested = true;
        body.Simulate(1f / 60f);

        Assert.False(body.IsGrounded);
        Assert.True(entity.Position.Y < groundedY);
    }

    private static (Entity Entity, PlatformerBody Body, Actor Actor) AddBody(Scene scene, Vector2 position)
    {
        var entity = new Entity { Position = position };
        var hitbox = new Hitbox(32, 32);
        var actor = new Actor();
        var body = new PlatformerBody();
        entity.Add(hitbox);
        entity.Add(actor);
        entity.Add(body);
        scene.Add(entity);
        return (entity, body, actor);
    }

    private static void AddSolidFloor(Scene scene, Vector2 position, float width, float height)
    {
        var floor = new Entity { Position = position };
        floor.Add(new Hitbox(width, height));
        floor.Add(new Solid());
        scene.Add(floor);
    }
}
