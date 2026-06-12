using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Vertical platformer physics state — gravity, jumping, and grounded tracking via <see cref="Actor"/>.
/// </summary>
/// <remarks>
/// When grounded on a <see cref="KinematicSolid"/>, applies that platform's <see cref="KinematicSolid.FrameDisplacement"/>
/// after vertical simulation so the entity rides moving solids. Requires the platform entity to update before this body
/// on the same frame (see <see cref="KinematicSolid"/> remarks).
/// </remarks>
public sealed class PlatformerBody : Component
{
    /// <summary>
    /// Downward acceleration in pixels per second squared.
    /// </summary>
    public float Gravity { get; set; } = 1000f;

    /// <summary>
    /// Initial upward speed in pixels per second when a jump starts.
    /// </summary>
    public float JumpSpeed { get; set; } = 540f;

    /// <summary>
    /// Grace period in frames after leaving the ground (without jumping) during which
    /// <see cref="JumpRequested"/> still starts a jump. Default matches common 60fps platformer feel (~100ms).
    /// </summary>
    public int CoyoteTimeFrames { get; set; } = 6;

    /// <summary>
    /// Remaining coyote-time frames after the last simulation step; zero when inactive.
    /// </summary>
    public int CoyoteFramesRemaining { get; private set; }

    /// <summary>
    /// Grace period in frames to remember an airborne jump press so the jump fires on the next
    /// grounded or coyote-qualified frame. Default matches common 60fps platformer feel (~100ms).
    /// </summary>
    public int JumpBufferFrames { get; set; } = 6;

    /// <summary>
    /// Remaining jump-buffer frames after the last simulation step; zero when inactive.
    /// </summary>
    public int JumpBufferFramesRemaining { get; private set; }

    /// <summary>
    /// When <see langword="true"/> on the next simulation step, starts or buffers a jump.
    /// Buffered jumps fire when grounded or within coyote time before the buffer expires.
    /// </summary>
    public bool JumpRequested { get; set; }

    /// <summary>
    /// When <see langword="true"/> on the next simulation step, allows falling through
    /// <see cref="JumpThroughSolid"/> platforms while grounded on them.
    /// </summary>
    public bool DropThroughRequested { get; set; }

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
    private KinematicSolid? _lastRiddenKinematic;

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

        var canJump = IsGrounded || CoyoteFramesRemaining > 0;
        var jumpedThisFrame = false;

        if (JumpRequested)
        {
            if (canJump)
                jumpedThisFrame = StartJump(deltaTime);
            else
            {
                JumpBufferFramesRemaining = JumpBufferFrames;
                JumpRequested = false;
            }
        }
        else if (JumpBufferFramesRemaining > 0 && canJump)
        {
            jumpedThisFrame = StartJump(deltaTime);
        }

        _verticalVelocity += Gravity * deltaTime;

        var verticalMove = _verticalVelocity * deltaTime;
        var moved = actor.MoveY(verticalMove);

        var stepsRequested = (int)MathF.Floor(MathF.Abs(verticalMove));
        if (stepsRequested > 0 && moved < stepsRequested)
        {
            if (verticalMove < 0f && actor.TryCeilingCornerNudge() != 0f)
            {
                moved += 1f;
                var remainingUp = verticalMove + moved;
                if (remainingUp < 0f)
                    moved += actor.MoveY(remainingUp);
            }
            else
                _verticalVelocity = 0f;
        }

        IsGrounded = _verticalVelocity < 0f ? false : ProbeGrounded(actor);

        if (IsGrounded && _verticalVelocity > 0f)
            _verticalVelocity = 0f;

        if (IsGrounded)
        {
            ApplyPlatformCarry();
            CoyoteFramesRemaining = 0;
        }
        else if (WasGrounded && !jumpedThisFrame)
            CoyoteFramesRemaining = CoyoteTimeFrames;
        else if (CoyoteFramesRemaining > 0)
            CoyoteFramesRemaining--;

        if (JumpBufferFramesRemaining > 0 && !jumpedThisFrame)
            JumpBufferFramesRemaining--;

        DropThroughRequested = false;
    }

    private bool StartJump(float deltaTime)
    {
        _verticalVelocity = -JumpSpeed;
        ApplyLiftMomentum(deltaTime);
        JumpRequested = false;
        CoyoteFramesRemaining = 0;
        JumpBufferFramesRemaining = 0;
        return true;
    }

    private void ApplyLiftMomentum(float deltaTime)
    {
        if (deltaTime <= 0f || !WasGrounded)
            return;

        var kinematic = ResolveRiddenKinematic();
        if (kinematic is null)
            return;

        var displacement = kinematic.FrameDisplacement;
        if (displacement == System.Numerics.Vector2.Zero)
            return;

        var velocity = displacement / deltaTime;
        _verticalVelocity += velocity.Y;
        Entity!.Position += new System.Numerics.Vector2(displacement.X, 0f);
        _lastRiddenKinematic = null;
    }

    private KinematicSolid? ResolveRiddenKinematic()
    {
        if (_lastRiddenKinematic is not null)
            return _lastRiddenKinematic;

        var hitbox = RequireHitbox();
        var scene = RequireScene();
        var entity = Entity!;

        entity.Position += new System.Numerics.Vector2(0f, 1f);
        var ground = DirectionalSolidCollision.FindFirstBlocking(
            scene, entity, hitbox, Solid.Tag, new System.Numerics.Vector2(0f, 1f));
        entity.Position -= new System.Numerics.Vector2(0f, 1f);

        if (ground is null)
            return null;

        foreach (var component in ground.Components)
        {
            if (component is KinematicSolid kinematic)
                return kinematic;
        }

        return null;
    }

    private void ApplyPlatformCarry()
    {
        var hitbox = RequireHitbox();
        var scene = RequireScene();
        var entity = Entity!;

        entity.Position += new System.Numerics.Vector2(0f, 1f);
        var ground = DirectionalSolidCollision.FindFirstBlocking(
            scene, entity, hitbox, Solid.Tag, new System.Numerics.Vector2(0f, 1f));
        entity.Position -= new System.Numerics.Vector2(0f, 1f);

        if (ground is null)
        {
            _lastRiddenKinematic = null;
            return;
        }

        _lastRiddenKinematic = null;

        foreach (var component in ground.Components)
        {
            if (component is not KinematicSolid kinematic)
                continue;

            _lastRiddenKinematic = kinematic;
            var delta = kinematic.FrameDisplacement;
            if (delta != System.Numerics.Vector2.Zero)
                entity.Position += delta;

            return;
        }
    }

    private bool ProbeGrounded(Actor actor)
    {
        var hitbox = RequireHitbox();
        var scene = RequireScene();
        var entity = Entity!;

        entity.Position += new System.Numerics.Vector2(0f, 1f);
        var grounded = DirectionalSolidCollision.FindFirstBlocking(
            scene, entity, hitbox, Solid.Tag, new System.Numerics.Vector2(0f, 1f)) is not null;
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
