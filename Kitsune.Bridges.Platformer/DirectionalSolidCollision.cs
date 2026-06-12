using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

internal static class DirectionalSolidCollision
{
    public static Entity? FindFirstBlocking(
        Scene scene,
        Entity actorEntity,
        Hitbox hitbox,
        string tag,
        Vector2 step)
    {
        foreach (var target in scene.CollideAll(hitbox, tag, CollisionLayer.Geometry))
        {
            if (IsBlockedBy(actorEntity, target, hitbox, step))
                return target;
        }

        return null;
    }

    public static bool IsBlockedBy(Entity actorEntity, Entity blocker, Hitbox hitbox, Vector2 step)
    {
        PlatformerBody? body = null;

        foreach (var component in actorEntity.Components)
        {
            if (component is PlatformerBody platformerBody)
            {
                body = platformerBody;
                break;
            }
        }

        var hasDirectional = false;

        foreach (var component in blocker.Components)
        {
            if (component is not IDirectionalSolid directional)
                continue;

            hasDirectional = true;

            if (directional.BlocksMovement(hitbox, step, body))
                return true;
        }

        return !hasDirectional;
    }
}
