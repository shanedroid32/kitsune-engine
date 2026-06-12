using System.Numerics;
using Foster.Framework;

namespace Kitsune.Core;

/// <summary>
/// An axis-aligned rectangular collider attached to an entity.
/// </summary>
public sealed class Hitbox : Component
{
    /// <summary>
    /// The width and height of the hitbox in pixels.
    /// </summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// Offset from the owning entity's world position to the hitbox top-left corner.
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Creates a hitbox with the given size.
    /// </summary>
    /// <param name="width">Hitbox width in pixels.</param>
    /// <param name="height">Hitbox height in pixels.</param>
    public Hitbox(float width, float height)
    {
        Size = new Vector2(width, height);
    }

    /// <summary>
    /// Creates a hitbox with the given size and offset.
    /// </summary>
    /// <param name="width">Hitbox width in pixels.</param>
    /// <param name="height">Hitbox height in pixels.</param>
    /// <param name="offsetX">Horizontal offset from the entity position.</param>
    /// <param name="offsetY">Vertical offset from the entity position.</param>
    public Hitbox(float width, float height, float offsetX, float offsetY)
    {
        Size = new Vector2(width, height);
        Offset = new Vector2(offsetX, offsetY);
    }

    /// <summary>
    /// Gets the world-space bounds of this hitbox.
    /// </summary>
    /// <returns>The axis-aligned rectangle for this hitbox.</returns>
    public Rect GetBounds()
    {
        var position = Entity?.WorldPosition ?? Vector2.Zero;
        var topLeft = position + Offset;
        return new Rect(topLeft.X, topLeft.Y, Size.X, Size.Y);
    }

    /// <summary>
    /// Tests whether two hitboxes overlap.
    /// </summary>
    /// <param name="a">The first hitbox.</param>
    /// <param name="b">The second hitbox.</param>
    /// <returns><see langword="true"/> when the hitboxes overlap; otherwise <see langword="false"/>.</returns>
    public static bool Overlaps(Hitbox a, Hitbox b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.GetBounds().Overlaps(b.GetBounds());
    }
}
