using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Solid an actor can rise through or drop down through (when <see cref="PlatformerBody.DropThroughRequested"/> is set),
/// but stands on when landing from above.
/// </summary>
public sealed class JumpThroughSolid : Component, IDirectionalSolid
{
    /// <inheritdoc />
    public override void Added()
    {
        if (Entity is null)
            return;

        Solid.ApplyLayer(Entity, CollisionLayer.Geometry);

        if (Entity.Scene is not null)
            Entity.Scene.Tags.Register(Entity, Solid.Tag);
    }

    /// <inheritdoc />
    public override void Removed()
    {
        if (Entity?.Scene is not null)
            Entity.Scene.Tags.Unregister(Entity, Solid.Tag);
    }

    /// <inheritdoc />
    public bool BlocksMovement(Hitbox actorHitbox, Vector2 moveStep, PlatformerBody? body)
    {
        if (moveStep.Y < 0f)
            return false;

        if (moveStep.Y > 0f)
            return body?.DropThroughRequested != true;

        return false;
    }
}
