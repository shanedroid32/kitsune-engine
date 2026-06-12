using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Solid that blocks only when an actor moves downward onto it. Rising and horizontal movement pass through.
/// </summary>
public sealed class OneWaySolid : Component, IDirectionalSolid
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
    public bool BlocksMovement(Hitbox actorHitbox, Vector2 moveStep, PlatformerBody? body) =>
        moveStep.Y > 0f;
}
