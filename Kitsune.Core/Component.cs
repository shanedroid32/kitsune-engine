using Foster.Framework;

namespace Kitsune.Core;

/// <summary>
/// Attachable behavior on an <see cref="Entity"/> with lifecycle hooks.
/// </summary>
public abstract class Component
{
    /// <summary>
    /// The entity this component is attached to, or <see langword="null"/> before attachment.
    /// </summary>
    public Entity? Entity { get; internal set; }

    /// <summary>
    /// Called when the component is added to an entity that is active in a scene.
    /// </summary>
    public virtual void Added() { }

    /// <summary>
    /// Called once per frame while the owning entity is active in a scene.
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Called once per frame to draw this component while the owning entity is visible.
    /// </summary>
    /// <param name="batcher">The Foster batcher used for drawing.</param>
    public virtual void Render(Batcher batcher) { }

    /// <summary>
    /// Called when the component is removed from its entity or the scene ends.
    /// </summary>
    public virtual void Removed() { }
}
