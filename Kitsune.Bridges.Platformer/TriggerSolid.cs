using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Registers under the <c>solid</c> tag but uses the <see cref="CollisionLayer.Trigger"/> layer so
/// <see cref="Actor"/> movement passes through while explicit trigger queries can still detect overlap.
/// </summary>
public sealed class TriggerSolid : Component
{
    /// <inheritdoc />
    public override void Added()
    {
        if (Entity is null)
            return;

        Solid.ApplyLayer(Entity, CollisionLayer.Trigger);

        if (Entity.Scene is not null)
            Entity.Scene.Tags.Register(Entity, Solid.Tag);
    }

    /// <inheritdoc />
    public override void Removed()
    {
        if (Entity?.Scene is not null)
            Entity.Scene.Tags.Unregister(Entity, Solid.Tag);
    }
}
