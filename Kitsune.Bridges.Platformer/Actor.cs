using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Resolves axis-separated pixel-step movement against tagged solids via Core collision query.
/// </summary>
public sealed class Actor : Component
{
    /// <summary>
    /// Moves the owning entity horizontally in 1px steps, stopping at the first blocking solid.
    /// </summary>
    /// <param name="pixels">Distance to move in pixels; sign selects direction.</param>
    /// <param name="tag">The tag to collide against.</param>
    /// <param name="onCollide">Optional callback invoked with the blocking entity when movement stops.</param>
    /// <returns>The number of pixels actually moved before stopping.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the actor is not attached to an entity with a hitbox in a scene.</exception>
    public float MoveX(float pixels, string tag = Solid.Tag, Action<Entity>? onCollide = null) =>
        MoveAxis(pixels, new Vector2(1f, 0f), tag, onCollide);

    /// <summary>
    /// Moves the owning entity vertically in 1px steps, stopping at the first blocking solid.
    /// </summary>
    /// <param name="pixels">Distance to move in pixels; sign selects direction.</param>
    /// <param name="tag">The tag to collide against.</param>
    /// <param name="onCollide">Optional callback invoked with the blocking entity when movement stops.</param>
    /// <returns>The number of pixels actually moved before stopping.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the actor is not attached to an entity with a hitbox in a scene.</exception>
    public float MoveY(float pixels, string tag = Solid.Tag, Action<Entity>? onCollide = null) =>
        MoveAxis(pixels, new Vector2(0f, 1f), tag, onCollide);

    private float MoveAxis(float pixels, Vector2 axis, string tag, Action<Entity>? onCollide)
    {
        var hitbox = RequireHitbox();
        var scene = RequireScene();
        var entity = Entity!;

        if (pixels == 0f)
            return 0f;

        ArgumentException.ThrowIfNullOrEmpty(tag);

        var stepSign = Math.Sign(pixels);
        var step = axis * stepSign;
        var stepsRemaining = (int)MathF.Floor(MathF.Abs(pixels));
        var moved = 0f;

        for (var i = 0; i < stepsRemaining; i++)
        {
            entity.Position += step;
            moved += 1f;

            if (!scene.CollideCheck(hitbox, tag))
                continue;

            var blocker = scene.CollideFirst(hitbox, tag);

            entity.Position -= step;
            moved -= 1f;

            if (blocker is not null)
                onCollide?.Invoke(blocker);

            break;
        }

        return moved;
    }

    private Hitbox RequireHitbox()
    {
        if (Entity is null)
            throw new InvalidOperationException("Actor is not attached to an entity.");

        foreach (var component in Entity.Components)
        {
            if (component is Hitbox hitbox)
                return hitbox;
        }

        throw new InvalidOperationException("Actor requires a Hitbox on the same entity.");
    }

    private Scene RequireScene()
    {
        if (Entity?.Scene is null)
            throw new InvalidOperationException("Actor must belong to a scene before moving.");

        return Entity.Scene;
    }
}
