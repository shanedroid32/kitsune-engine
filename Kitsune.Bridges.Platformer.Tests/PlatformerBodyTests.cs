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
    public void Simulate_JumpReachesMeaningfulHeight()
    {
        var scene = new Scene();
        var (entity, body, _) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 64, 32);
        scene.Begin();

        for (var i = 0; i < 120; i++)
            body.Simulate(1f / 60f);

        var groundedY = entity.Position.Y;

        body.JumpRequested = true;
        for (var i = 0; i < 20; i++)
            body.Simulate(1f / 60f);

        Assert.True(entity.Position.Y < groundedY - 15f);
        Assert.False(body.IsGrounded);
    }

    [Fact]
    public void Simulate_CoyoteJumpSucceedsAfterWalkingOffLedge()
    {
        var scene = new Scene();
        var (entity, body, actor) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 48, 32);
        body.CoyoteTimeFrames = 6;
        scene.Begin();

        for (var i = 0; i < 30; i++)
            body.Simulate(1f / 60f);

        while (body.IsGrounded)
        {
            actor.MoveX(1f);
            body.Simulate(1f / 60f);
        }

        var airY = entity.Position.Y;
        Assert.False(body.IsGrounded);
        Assert.True(body.CoyoteFramesRemaining > 0);

        body.JumpRequested = true;
        body.Simulate(1f / 60f);

        Assert.True(entity.Position.Y < airY);
        Assert.False(body.IsGrounded);
    }

    [Fact]
    public void Simulate_CoyoteJumpFailsAfterWindowExpires()
    {
        var scene = new Scene();
        var (entity, body, actor) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 48, 32);
        body.CoyoteTimeFrames = 4;
        scene.Begin();

        for (var i = 0; i < 30; i++)
            body.Simulate(1f / 60f);

        while (body.IsGrounded)
        {
            actor.MoveX(1f);
            body.Simulate(1f / 60f);
        }

        for (var i = 0; i < body.CoyoteTimeFrames + 1; i++)
            body.Simulate(1f / 60f);

        Assert.Equal(0, body.CoyoteFramesRemaining);

        var yBefore = entity.Position.Y;
        body.JumpRequested = true;
        body.Simulate(1f / 60f);

        Assert.True(entity.Position.Y > yBefore);
    }

    [Fact]
    public void Simulate_CoyoteTimeResetsOnLanding()
    {
        var scene = new Scene();
        var (entity, body, actor) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 48, 32);
        AddSolidFloor(scene, new Vector2(-64, 200), 256, 32);
        body.CoyoteTimeFrames = 6;
        scene.Begin();

        for (var i = 0; i < 30; i++)
            body.Simulate(1f / 60f);

        while (body.IsGrounded)
        {
            actor.MoveX(1f);
            body.Simulate(1f / 60f);
        }

        Assert.True(body.CoyoteFramesRemaining > 0);

        for (var i = 0; i < 240; i++)
            body.Simulate(1f / 60f);

        Assert.True(body.IsGrounded);
        Assert.Equal(0, body.CoyoteFramesRemaining);
    }

    [Fact]
    public void Simulate_JumpBufferFiresOnLanding()
    {
        var scene = new Scene();
        var (entity, body, _) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 64, 32);
        body.JumpBufferFrames = 90;
        scene.Begin();

        for (var i = 0; i < 30; i++)
            body.Simulate(1f / 60f);

        body.JumpRequested = true;
        body.Simulate(1f / 60f);
        Assert.False(body.IsGrounded);

        var peakY = entity.Position.Y;
        while (entity.Position.Y <= peakY)
        {
            body.Simulate(1f / 60f);
            peakY = MathF.Min(peakY, entity.Position.Y);
        }

        Assert.False(body.IsGrounded);

        body.JumpRequested = true;
        body.Simulate(1f / 60f);
        Assert.True(body.JumpBufferFramesRemaining > 0);
        Assert.False(body.IsGrounded);

        while (!body.IsGrounded)
            body.Simulate(1f / 60f);

        Assert.True(body.IsGrounded);
        var landY = entity.Position.Y;

        body.Simulate(1f / 60f);

        Assert.True(entity.Position.Y < landY);
        Assert.False(body.IsGrounded);
        Assert.Equal(0, body.JumpBufferFramesRemaining);
    }

    [Fact]
    public void Simulate_JumpBufferExpiresBeforeLateLanding()
    {
        var scene = new Scene();
        var (entity, body, _) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 64, 32);
        body.JumpBufferFrames = 2;
        scene.Begin();

        for (var i = 0; i < 30; i++)
            body.Simulate(1f / 60f);

        body.JumpRequested = true;
        body.Simulate(1f / 60f);

        body.JumpRequested = true;
        body.Simulate(1f / 60f);
        Assert.True(body.JumpBufferFramesRemaining > 0);

        for (var i = 0; i < body.JumpBufferFrames + 2; i++)
            body.Simulate(1f / 60f);

        Assert.Equal(0, body.JumpBufferFramesRemaining);
        Assert.False(body.IsGrounded);

        while (!body.IsGrounded)
            body.Simulate(1f / 60f);

        var landY = entity.Position.Y;
        body.Simulate(1f / 60f);

        Assert.Equal(landY, entity.Position.Y);
        Assert.True(body.IsGrounded);
    }

    [Fact]
    public void Simulate_JumpBufferDoesNotDoubleJumpInAir()
    {
        var scene = new Scene();
        var (entity, body, _) = AddBody(scene, new Vector2(0, 16));
        AddSolidFloor(scene, new Vector2(0, 48), 64, 32);
        scene.Begin();

        for (var i = 0; i < 30; i++)
            body.Simulate(1f / 60f);

        body.JumpRequested = true;
        body.Simulate(1f / 60f);
        Assert.False(body.IsGrounded);

        var peakY = entity.Position.Y;
        while (entity.Position.Y <= peakY)
        {
            body.Simulate(1f / 60f);
            peakY = MathF.Min(peakY, entity.Position.Y);
        }

        body.JumpRequested = true;
        body.Simulate(1f / 60f);

        Assert.False(body.IsGrounded);
        Assert.True(body.JumpBufferFramesRemaining > 0);

        var yAfterBuffer = entity.Position.Y;
        for (var i = 0; i < 3; i++)
            body.Simulate(1f / 60f);

        Assert.False(body.IsGrounded);
        Assert.True(entity.Position.Y > yAfterBuffer);
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
