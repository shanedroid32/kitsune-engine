namespace Kitsune.Core;

/// <summary>
/// Enumerates entities by tag via a <see cref="TagList"/>.
/// </summary>
public sealed class Tracker
{
    private readonly TagList _tagList;

    /// <summary>
    /// Creates a tracker backed by the given tag list.
    /// </summary>
    /// <param name="tagList">The tag list used for queries.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tagList"/> is null.</exception>
    public Tracker(TagList tagList)
    {
        ArgumentNullException.ThrowIfNull(tagList);
        _tagList = tagList;
    }

    /// <summary>
    /// Returns all entities registered with a single tag.
    /// </summary>
    /// <param name="tag">The tag to query.</param>
    /// <returns>An enumerable of matching entities.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is empty.</exception>
    public IEnumerable<Entity> GetEntities(string tag)
    {
        ArgumentException.ThrowIfNullOrEmpty(tag);
        return _tagList.GetEntities(tag);
    }
}
