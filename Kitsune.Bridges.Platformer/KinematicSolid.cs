using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// Moves the owning entity along a ping-pong path between its spawn position and <see cref="EndPosition"/>.
/// Intended for use on entities that also have <see cref="Solid"/>.
/// </summary>
/// <remarks>
/// <para>
/// Simulation order: <see cref="Scene"/> updates root entities in registration order; each entity
/// updates components in add order. Register moving solid entities <em>before</em> actors that
/// should collide with them so <see cref="Update"/> advances the platform before
/// <see cref="Actor"/> / <see cref="PlatformerBody"/> run on the same frame.
/// </para>
/// <para>
/// Each <see cref="Step"/> uses solid-authoritative resolution: precompute riders, move the solid,
/// push overlapping actors, carry riders, and squish when displacement cannot be resolved.
/// </para>
/// </remarks>
public sealed class KinematicSolid : Component
{
    /// <summary>
    /// World position the entity ping-pongs toward from its spawn point.
    /// </summary>
    public Vector2 EndPosition { get; set; }

    /// <summary>
    /// Travel speed in pixels per second.
    /// </summary>
    public float Speed { get; set; } = 60f;

    /// <summary>
    /// Optional provider for frame delta time; when set, <see cref="Update"/> advances motion automatically.
    /// </summary>
    public Func<float>? DeltaTimeSource { get; set; }

    /// <summary>
    /// World-space displacement applied by the most recent <see cref="Step"/> call.
    /// Read by <see cref="PlatformerBody"/> for lift momentum at jump.
    /// </summary>
    public Vector2 FrameDisplacement { get; private set; }

    private Vector2 _startPosition;
    private Vector2 _targetPosition;
    private bool _initialized;

    /// <inheritdoc />
    public override void Added()
    {
        if (Entity is null)
            return;

        _startPosition = Entity.Position;
        _targetPosition = EndPosition;
        _initialized = true;
    }

    /// <inheritdoc />
    public override void Update()
    {
        if (DeltaTimeSource is not null)
            Step(DeltaTimeSource());
    }

    /// <summary>
    /// Advances motion along the ping-pong path for the given elapsed time.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds.</param>
    public void Step(float deltaTime)
    {
        FrameDisplacement = Vector2.Zero;

        if (Entity is null || !_initialized || Speed <= 0f || deltaTime <= 0f)
            return;

        var scene = Entity.Scene;
        if (scene is null)
            return;

        var riders = KinematicActorResolution.CollectRiders(scene, Entity);

        var before = Entity.Position;
        var current = before;
        var toTarget = _targetPosition - current;
        var distance = toTarget.Length();

        if (distance < 0.001f)
        {
            Entity.Position = _targetPosition;
            SwapEndpoints();
        }
        else
        {
            var move = Speed * deltaTime;
            if (move >= distance)
            {
                Entity.Position = _targetPosition;
                SwapEndpoints();
            }
            else
                Entity.Position = current + toTarget / distance * move;
        }

        FrameDisplacement = Entity.Position - before;

        if (FrameDisplacement != Vector2.Zero)
            KinematicActorResolution.ResolveAfterMove(scene, Entity, FrameDisplacement, riders);
    }

    private void SwapEndpoints()
    {
        (_startPosition, _targetPosition) = (_targetPosition, _startPosition);
    }
}
