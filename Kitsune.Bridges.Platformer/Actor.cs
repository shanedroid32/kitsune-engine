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

    /// <summary>
    /// After an upward <see cref="MoveY"/> is blocked, tries a one-pixel left nudge then a one-pixel right nudge.
    /// A nudge is kept when the entity can move up one pixel afterward; that upward pixel is applied as part of the nudge.
    /// </summary>
    /// <param name="tag">The tag to collide against.</param>
    /// <param name="onCollide">Optional callback invoked when a nudge step hits a blocking solid.</param>
    /// <returns>Horizontal pixels nudged (&lt;0 left, &gt;0 right, 0 when no nudge cleared the ceiling lip).</returns>
    /// <exception cref="InvalidOperationException">Thrown when the actor is not attached to an entity with a hitbox in a scene.</exception>
    public float TryCeilingCornerNudge(string tag = Solid.Tag, Action<Entity>? onCollide = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(tag);

        if (TryNudgeDirection(-1f, tag, onCollide, out var horizontalMoved))
            return -horizontalMoved;

        if (TryNudgeDirection(1f, tag, onCollide, out horizontalMoved))
            return horizontalMoved;

        return 0f;
    }

    private bool TryNudgeDirection(float horizontalPixels, string tag, Action<Entity>? onCollide, out float horizontalMoved)
    {
        horizontalMoved = MoveX(horizontalPixels, tag, onCollide);
        if (horizontalMoved == 0f && horizontalPixels != 0f)
            return false;

        if (MoveY(-1f, tag, onCollide) >= 1f)
            return true;

        MoveX(-horizontalPixels, tag, onCollide);
        horizontalMoved = 0f;
        return false;
    }

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

            if (!scene.CollideCheck(hitbox, tag, CollisionLayer.Geometry))
                continue;

            var blocker = scene.CollideFirst(hitbox, tag, CollisionLayer.Geometry);

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
