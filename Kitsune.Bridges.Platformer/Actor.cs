using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Resolves axis-separated pixel-step movement against tagged solids via Core collision query.
/// </summary>
public sealed class Actor : Component
{
    /// <summary>
    /// Optional handler invoked when kinematic resolution traps this actor.
    /// Return <see langword="true"/> if squish was handled; otherwise the entity is removed from the scene.
    /// </summary>
    public Func<bool>? OnSquish { get; set; }

    /// <summary>
    /// Whether this actor is riding the given solid — resting on its top surface per downward probe
    /// and directional-solid rules.
    /// </summary>
    /// <param name="solid">The solid entity to test (typically a <see cref="KinematicSolid"/> owner).</param>
    /// <returns><see langword="true"/> when a downward probe finds the given solid as the blocking surface.</returns>
    public bool IsRiding(Entity solid)
    {
        ArgumentNullException.ThrowIfNull(solid);

        var hitbox = RequireHitbox();
        var scene = RequireScene();
        var entity = Entity!;

        entity.Position += new Vector2(0f, 1f);
        var ground = DirectionalSolidCollision.FindFirstBlocking(
            scene, entity, hitbox, Solid.Tag, new Vector2(0f, 1f));
        entity.Position -= new Vector2(0f, 1f);

        return ground == solid;
    }

    /// <summary>
    /// Whether this actor is riding the given kinematic solid.
    /// </summary>
    /// <param name="kinematic">The kinematic solid component to test.</param>
    /// <returns><see langword="true"/> when <see cref="IsRiding(Entity)"/> is true for its entity.</returns>
    public bool IsRiding(KinematicSolid kinematic)
    {
        ArgumentNullException.ThrowIfNull(kinematic);

        return kinematic.Entity is not null && IsRiding(kinematic.Entity);
    }

    /// <summary>
    /// Handles squish from kinematic resolution.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="OnSquish"/> handled it; otherwise the entity was removed.</returns>
    public bool Squish()
    {
        if (Entity?.Scene is null)
            return false;

        if (OnSquish?.Invoke() == true)
            return true;

        Entity.Scene.Remove(Entity);
        return false;
    }

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

    internal float MoveXFromKinematic(float pixels, Entity kinematicSolid, Action<Entity>? onCollide = null) =>
        MoveAxis(pixels, new Vector2(1f, 0f), Solid.Tag, onCollide, kinematicSolid);

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

    internal float MoveYFromKinematic(float pixels, Entity kinematicSolid, Action<Entity>? onCollide = null) =>
        MoveAxis(pixels, new Vector2(0f, 1f), Solid.Tag, onCollide, kinematicSolid);

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

    private float MoveAxis(float pixels, Vector2 axis, string tag, Action<Entity>? onCollide, Entity? ignoreSolid = null)
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

            var blocker = FindFirstBlockingSolid(hitbox, tag, step);
            if (blocker is null || blocker == ignoreSolid)
                continue;

            entity.Position -= step;
            moved -= 1f;
            onCollide?.Invoke(blocker);
            break;
        }

        return moved;
    }

    private Entity? FindFirstBlockingSolid(Hitbox hitbox, string tag, Vector2 step) =>
        DirectionalSolidCollision.FindFirstBlocking(RequireScene(), Entity!, hitbox, tag, step);

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
