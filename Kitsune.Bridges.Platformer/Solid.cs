using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Marks an entity as platformer collision geometry and registers it under the <c>solid</c> tag.
/// </summary>
public sealed class Solid : Component
{
    /// <summary>
    /// The tag registered for solid collision geometry.
    /// </summary>
    public const string Tag = "solid";

    /// <inheritdoc />
    public override void Added()
    {
        if (Entity?.Scene is not null)
            Entity.Scene.Tags.Register(Entity, Tag);
    }

    /// <inheritdoc />
    public override void Removed()
    {
        if (Entity?.Scene is not null)
            Entity.Scene.Tags.Unregister(Entity, Tag);
    }
}
