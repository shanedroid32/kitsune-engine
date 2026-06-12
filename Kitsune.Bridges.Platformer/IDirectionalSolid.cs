using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

/// <summary>
/// A solid whose blocking depends on movement direction or body state (one-way, jump-through).
/// </summary>
public interface IDirectionalSolid
{
    /// <summary>
    /// Whether this solid blocks the actor for the given one-pixel move step.
    /// </summary>
    /// <param name="actorHitbox">The moving actor's hitbox after the step was applied.</param>
    /// <param name="moveStep">The one-pixel displacement just applied (axis-separated).</param>
    /// <param name="body">The actor entity's <see cref="PlatformerBody"/>, if present.</param>
    /// <returns><see langword="true"/> when movement should stop against this solid.</returns>
    bool BlocksMovement(Hitbox actorHitbox, Vector2 moveStep, PlatformerBody? body);
}
