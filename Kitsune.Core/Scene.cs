using Foster.Framework;

namespace Kitsune.Core;

/// <summary>
/// Top-level container for a self-contained slice of gameplay.
/// </summary>
public class Scene
{
    private readonly List<Entity> _roots = [];
    private readonly HashSet<Entity> _entities = [];

    /// <summary>
    /// Whether this scene has begun and not yet ended.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The root entities owned by this scene.
    /// </summary>
    public IReadOnlyList<Entity> Entities => _roots;

    /// <summary>
    /// The tag list used to register entities by tag.
    /// </summary>
    public TagList Tags { get; } = new();

    /// <summary>
    /// The tracker used to query entities by tag.
    /// </summary>
    public Tracker Tracker { get; }

    /// <summary>
    /// Creates a new scene.
    /// </summary>
    public Scene()
    {
        Tracker = new Tracker(Tags);
    }

    /// <summary>
    /// Adds a root entity to the scene.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the entity already belongs to a scene.</exception>
    public void Add(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.Scene is not null)
            throw new InvalidOperationException("Entity already belongs to a scene.");

        _roots.Add(entity);
        RegisterEntity(entity);

        if (IsActive)
            entity.OnBegin();
    }

    /// <summary>
    /// Removes a root entity from the scene.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    /// <returns><see langword="true"/> when the entity was removed; otherwise <see langword="false"/>.</returns>
    public bool Remove(Entity entity)
    {
        if (!_roots.Remove(entity))
            return false;

        if (IsActive)
            entity.OnEnd();

        UnregisterEntity(entity);
        return true;
    }

    /// <summary>
    /// Starts the scene and activates all entities.
    /// </summary>
    public void Begin()
    {
        if (IsActive)
            return;

        IsActive = true;

        foreach (var entity in _roots)
            entity.OnBegin();
    }

    /// <summary>
    /// Ends the scene and deactivates all entities.
    /// </summary>
    public void End()
    {
        if (!IsActive)
            return;

        for (var i = _roots.Count - 1; i >= 0; i--)
            _roots[i].OnEnd();

        IsActive = false;
    }

    /// <summary>
    /// Updates all entities in the scene.
    /// </summary>
    public void Update()
    {
        if (!IsActive)
            return;

        foreach (var entity in _roots)
            entity.UpdateHierarchy();
    }

    /// <summary>
    /// Renders all visible entities in depth order.
    /// </summary>
    /// <param name="batcher">The Foster batcher used for drawing.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="batcher"/> is null.</exception>
    public void Render(Batcher batcher)
    {
        ArgumentNullException.ThrowIfNull(batcher);

        if (!IsActive)
            return;

        foreach (var entity in GetRenderOrder())
        {
            if (!entity.Visible)
                continue;

            foreach (var component in entity.Components)
                component.Render(batcher);
        }
    }

    /// <summary>
    /// Tests whether <paramref name="source"/> overlaps any tagged entity's hitbox.
    /// </summary>
    /// <param name="source">The hitbox to test, attached to an entity in this scene.</param>
    /// <param name="tag">The tag to query.</param>
    /// <returns><see langword="true"/> when an overlap exists; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="source"/> is not attached to an entity in this scene.</exception>
    public bool CollideCheck(Hitbox source, string tag)
    {
        ValidateCollisionSource(source, tag);
        return CollideFirst(source, tag) is not null;
    }

    /// <summary>
    /// Returns the first entity registered under <paramref name="tag"/> whose hitbox overlaps <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The hitbox to test, attached to an entity in this scene.</param>
    /// <param name="tag">The tag to query.</param>
    /// <returns>The first overlapping entity in tag registration order, or <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="source"/> is not attached to an entity in this scene.</exception>
    public Entity? CollideFirst(Hitbox source, string tag)
    {
        ValidateCollisionSource(source, tag);

        var sourceEntity = source.Entity!;

        foreach (var target in Tracker.GetEntities(tag))
        {
            if (target == sourceEntity)
                continue;

            if (OverlapsAnyHitbox(source, target))
                return target;
        }

        return null;
    }

    /// <summary>
    /// Returns every entity registered under <paramref name="tag"/> whose hitbox overlaps <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The hitbox to test, attached to an entity in this scene.</param>
    /// <param name="tag">The tag to query.</param>
    /// <returns>Overlapping entities in tag registration order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="source"/> is not attached to an entity in this scene.</exception>
    public IEnumerable<Entity> CollideAll(Hitbox source, string tag)
    {
        ValidateCollisionSource(source, tag);

        var sourceEntity = source.Entity!;

        foreach (var target in Tracker.GetEntities(tag))
        {
            if (target == sourceEntity)
                continue;

            if (OverlapsAnyHitbox(source, target))
                yield return target;
        }
    }

    internal IReadOnlyList<Entity> GetRenderOrder()
    {
        var drawOrder = new List<Entity>(_entities.Count);
        drawOrder.AddRange(_entities);
        drawOrder.Sort(static (a, b) => a.Depth.CompareTo(b.Depth));
        return drawOrder;
    }

    internal void AdoptEntity(Entity entity)
    {
        RegisterEntity(entity);

        if (IsActive)
            entity.OnBegin();
    }

    internal void ReleaseEntity(Entity entity)
    {
        if (IsActive)
            entity.OnEnd();

        UnregisterEntity(entity);
    }

    private void RegisterEntity(Entity entity)
    {
        RegisterEntityHierarchy(entity);
    }

    private void RegisterEntityHierarchy(Entity entity)
    {
        if (!_entities.Add(entity))
            return;

        entity.Scene = this;

        foreach (var child in entity.Children)
            RegisterEntityHierarchy(child);
    }

    private void UnregisterEntity(Entity entity)
    {
        UnregisterEntityHierarchy(entity);
    }

    private void UnregisterEntityHierarchy(Entity entity)
    {
        foreach (var child in entity.Children)
            UnregisterEntityHierarchy(child);

        _entities.Remove(entity);
        Tags.UnregisterAll(entity);
        entity.Scene = null;
    }

    private void ValidateCollisionSource(Hitbox source, string tag)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(tag);

        if (source.Entity is null || source.Entity.Scene != this)
            throw new InvalidOperationException("Source hitbox must be attached to an entity in this scene.");
    }

    private static bool OverlapsAnyHitbox(Hitbox source, Entity target)
    {
        foreach (var component in target.Components)
        {
            if (component is Hitbox targetHitbox && Hitbox.Overlaps(source, targetHitbox))
                return true;
        }

        return false;
    }
}
