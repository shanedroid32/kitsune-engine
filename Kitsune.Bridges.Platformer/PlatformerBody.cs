using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Vertical platformer physics state — gravity, jumping, and grounded tracking via <see cref="Actor"/>.
/// </summary>
public sealed class PlatformerBody : Component
{
    /// <summary>
    /// Downward acceleration in pixels per second squared.
    /// </summary>
    public float Gravity { get; set; } = 520f;

    /// <summary>
    /// Initial upward speed in pixels per second when a jump starts.
    /// </summary>
    public float JumpSpeed { get; set; } = 210f;

    /// <summary>
    /// When <see langword="true"/> on the next simulation step, starts a jump if grounded.
    /// </summary>
    public bool JumpRequested { get; set; }

    /// <summary>
    /// Optional provider for frame delta time; when set, <see cref="Update"/> runs simulation automatically.
    /// </summary>
    public Func<float>? DeltaTimeSource { get; set; }

    /// <summary>
    /// Whether the body is resting on a solid below it after the last simulation step.
    /// </summary>
    public bool IsGrounded { get; private set; }

    /// <summary>
    /// Whether the body was grounded before the last simulation step.
    /// </summary>
    public bool WasGrounded { get; private set; }

    private float _verticalVelocity;

    /// <inheritdoc />
    public override void Update()
    {
        if (DeltaTimeSource is not null)
            Simulate(DeltaTimeSource());
    }

    /// <summary>
    /// Applies gravity, resolves jumping, and moves vertically through the sibling <see cref="Actor"/>.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds.</param>
    public void Simulate(float deltaTime)
    {
        var actor = RequireActor();

        WasGrounded = IsGrounded;

        if (JumpRequested && IsGrounded)
        {
            _verticalVelocity = -JumpSpeed;
            JumpRequested = false;
        }

        _verticalVelocity += Gravity * deltaTime;

        var verticalMove = _verticalVelocity * deltaTime;
        var moved = actor.MoveY(verticalMove);

        if (verticalMove > 0f && moved > 0f && moved + 0.001f < verticalMove)
            _verticalVelocity = 0f;
        else if (verticalMove < 0f && moved > 0f && moved + 0.001f < MathF.Abs(verticalMove))
            _verticalVelocity = 0f;

        IsGrounded = ProbeGrounded(actor);

        if (IsGrounded && _verticalVelocity > 0f)
            _verticalVelocity = 0f;
    }

    private bool ProbeGrounded(Actor actor)
    {
        var hitbox = RequireHitbox();
        var scene = RequireScene();
        var entity = Entity!;

        entity.Position += new System.Numerics.Vector2(0f, 1f);
        var grounded = scene.CollideCheck(hitbox, Solid.Tag);
        entity.Position -= new System.Numerics.Vector2(0f, 1f);

        return grounded;
    }

    private Actor RequireActor()
    {
        if (Entity is null)
            throw new InvalidOperationException("PlatformerBody is not attached to an entity.");

        foreach (var component in Entity.Components)
        {
            if (component is Actor actor)
                return actor;
        }

        throw new InvalidOperationException("PlatformerBody requires an Actor on the same entity.");
    }

    private Hitbox RequireHitbox()
    {
        if (Entity is null)
            throw new InvalidOperationException("PlatformerBody is not attached to an entity.");

        foreach (var component in Entity.Components)
        {
            if (component is Hitbox hitbox)
                return hitbox;
        }

        throw new InvalidOperationException("PlatformerBody requires a Hitbox on the same entity.");
    }

    private Scene RequireScene()
    {
        if (Entity?.Scene is null)
            throw new InvalidOperationException("PlatformerBody must belong to a scene before simulating.");

        return Entity.Scene;
    }
}
