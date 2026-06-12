using System.Numerics;
using Kitsune.Core;

namespace Kitsune.Bridges.Platformer;

internal static class KinematicActorResolution
{
    public static void ResolveAfterMove(Scene scene, Entity solidEntity, Vector2 displacement, IReadOnlyList<Actor> riders)
    {
        if (displacement == Vector2.Zero)
            return;

        var pushed = new HashSet<Actor>();
        var actors = EnumerateActors(scene).ToList();

        foreach (var actor in actors)
        {
            if (actor.Entity == solidEntity || actor.Entity?.Scene is null)
                continue;

            if (!Overlaps(actor.Entity, solidEntity))
                continue;

            if (!TryDisplaceActor(actor, displacement, solidEntity))
                continue;

            pushed.Add(actor);
        }

        foreach (var rider in riders)
        {
            if (pushed.Contains(rider) || rider.Entity?.Scene is null)
                continue;

            TryDisplaceActor(rider, displacement, solidEntity);
        }
    }

    public static List<Actor> CollectRiders(Scene scene, Entity solidEntity)
    {
        var riders = new List<Actor>();

        foreach (var actor in EnumerateActors(scene).ToList())
        {
            if (actor.Entity == solidEntity)
                continue;

            if (actor.IsRiding(solidEntity))
                riders.Add(actor);
        }

        return riders;
    }

    private static bool TryDisplaceActor(Actor actor, Vector2 displacement, Entity kinematicSolid)
    {
        if (actor.Entity?.Scene is null)
            return false;

        if (displacement.X != 0f)
        {
            var movedX = actor.MoveXFromKinematic(displacement.X, kinematicSolid);
            if (IsBlocked(movedX, displacement.X) && Overlaps(actor.Entity!, kinematicSolid))
            {
                actor.Squish();
                return false;
            }
        }

        if (displacement.Y != 0f)
        {
            var movedY = actor.MoveYFromKinematic(displacement.Y, kinematicSolid);
            if (IsBlocked(movedY, displacement.Y) && Overlaps(actor.Entity!, kinematicSolid))
            {
                actor.Squish();
                return false;
            }
        }

        return true;
    }

    private static bool IsBlocked(float moved, float requested) =>
        MathF.Abs(requested) > 0.001f && MathF.Abs(moved) < MathF.Abs(requested) - 0.001f;

    private static bool Overlaps(Entity a, Entity b)
    {
        foreach (var hitboxA in a.Components)
        {
            if (hitboxA is not Hitbox ha)
                continue;

            foreach (var hitboxB in b.Components)
            {
                if (hitboxB is not Hitbox hb)
                    continue;

                if (Hitbox.Overlaps(ha, hb))
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<Actor> EnumerateActors(Scene scene)
    {
        foreach (var root in scene.Entities)
        {
            foreach (var actor in EnumerateActors(root))
                yield return actor;
        }
    }

    private static IEnumerable<Actor> EnumerateActors(Entity entity)
    {
        foreach (var component in entity.Components)
        {
            if (component is Actor actor)
                yield return actor;
        }

        foreach (var child in entity.Children)
        {
            foreach (var actor in EnumerateActors(child))
                yield return actor;
        }
    }
}
