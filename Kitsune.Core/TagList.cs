namespace Kitsune.Core;

/// <summary>
/// Indexes entities by string tag for fast lookup.
/// </summary>
public sealed class TagList
{
    private readonly Dictionary<string, HashSet<Entity>> _entitiesByTag = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers an entity under a tag.
    /// </summary>
    /// <param name="entity">The entity to register.</param>
    /// <param name="tag">The tag name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> or <paramref name="tag"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is empty.</exception>
    public void Register(Entity entity, string tag)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrEmpty(tag);

        if (!_entitiesByTag.TryGetValue(tag, out var entities))
        {
            entities = [];
            _entitiesByTag[tag] = entities;
        }

        entities.Add(entity);
    }

    /// <summary>
    /// Unregisters an entity from a tag.
    /// </summary>
    /// <param name="entity">The entity to unregister.</param>
    /// <param name="tag">The tag name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> or <paramref name="tag"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is empty.</exception>
    public void Unregister(Entity entity, string tag)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrEmpty(tag);

        if (_entitiesByTag.TryGetValue(tag, out var entities))
            entities.Remove(entity);
    }

    /// <summary>
    /// Returns all entities registered under a tag.
    /// </summary>
    /// <param name="tag">The tag name.</param>
    /// <returns>The entities with the given tag.</returns>
    internal IReadOnlyCollection<Entity> GetEntities(string tag)
    {
        if (_entitiesByTag.TryGetValue(tag, out var entities))
            return entities;

        return [];
    }

    internal void UnregisterAll(Entity entity)
    {
        foreach (var entities in _entitiesByTag.Values)
            entities.Remove(entity);
    }
}
