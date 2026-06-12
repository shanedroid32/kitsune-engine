namespace Kitsune.Core;

/// <summary>
/// Built-in collision layer bit flags for <see cref="Hitbox.Layer"/> and query masks.
/// </summary>
public static class CollisionLayer
{
    /// <summary>
    /// Blocking world geometry (floors, walls, platforms).
    /// </summary>
    public const uint Geometry = 1 << 0;

    /// <summary>
    /// Non-blocking overlap volumes (pickups, zones, hazards detected without stopping movement).
    /// </summary>
    public const uint Trigger = 1 << 1;

    /// <summary>
    /// All built-in layers combined. Use for default <see cref="Hitbox.CollidesWith"/> masks.
    /// </summary>
    public const uint All = Geometry | Trigger;
}
