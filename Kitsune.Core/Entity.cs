using System.Numerics;

namespace Kitsune.Core;

/// <summary>
/// A game object with position, visibility, optional hierarchy, and attached components.
/// </summary>
public class Entity
{
    private readonly List<Component> _components = [];
    private readonly List<Entity> _children = [];

    /// <summary>
    /// The scene that owns this entity, if any.
    /// </summary>
    public Scene? Scene { get; internal set; }

    /// <summary>
    /// The parent entity in the hierarchy, if any.
    /// </summary>
    public Entity? Parent { get; private set; }

    /// <summary>
    /// Local position relative to the parent, or world position when there is no parent.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// World-space position accounting for the parent hierarchy.
    /// </summary>
    public Vector2 WorldPosition => Parent is null ? Position : Parent.WorldPosition + Position;

    /// <summary>
    /// Whether this entity and its components are rendered.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Draw order; entities with higher depth render on top.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// The components attached to this entity.
    /// </summary>
    public IReadOnlyList<Component> Components => _components;

    /// <summary>
    /// The child entities parented to this entity.
    /// </summary>
    public IReadOnlyList<Entity> Children => _children;

    /// <summary>
    /// Adds a component to this entity.
    /// </summary>
    /// <param name="component">The component to attach.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the component is already attached.</exception>
    public void Add(Component component)
    {
        ArgumentNullException.ThrowIfNull(component);

        if (component.Entity is not null)
            throw new InvalidOperationException("Component is already attached to an entity.");

        _components.Add(component);
        component.Entity = this;

        if (Scene?.IsActive == true)
            component.Added();
    }

    /// <summary>
    /// Removes a component from this entity.
    /// </summary>
    /// <param name="component">The component to detach.</param>
    /// <returns><see langword="true"/> when the component was removed; otherwise <see langword="false"/>.</returns>
    public bool Remove(Component component)
    {
        if (!_components.Remove(component))
            return false;

        if (Scene?.IsActive == true)
            component.Removed();

        component.Entity = null;
        return true;
    }

    /// <summary>
    /// Parents a child entity under this entity.
    /// </summary>
    /// <param name="child">The child entity to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="child"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the child already has a parent or would create a cycle.</exception>
    public void Add(Entity child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child.Parent is not null)
            throw new InvalidOperationException("Entity already has a parent.");

        if (child == this || IsAncestorOf(child))
            throw new InvalidOperationException("Cannot parent an entity to itself or its descendant.");

        _children.Add(child);
        child.Parent = this;

        if (Scene is not null)
            Scene.AdoptEntity(child);
    }

    /// <summary>
    /// Removes a child entity from this entity.
    /// </summary>
    /// <param name="child">The child entity to remove.</param>
    /// <returns><see langword="true"/> when the child was removed; otherwise <see langword="false"/>.</returns>
    public bool Remove(Entity child)
    {
        if (!_children.Remove(child))
            return false;

        child.Parent = null;

        if (Scene is not null)
            Scene.ReleaseEntity(child);

        return true;
    }

    /// <summary>
    /// Called when the entity enters an active scene.
    /// </summary>
    internal void OnBegin()
    {
        foreach (var component in _components)
            component.Added();

        foreach (var child in _children)
            child.OnBegin();
    }

    /// <summary>
    /// Called when the entity leaves an active scene.
    /// </summary>
    internal void OnEnd()
    {
        for (var i = _children.Count - 1; i >= 0; i--)
            _children[i].OnEnd();

        for (var i = _components.Count - 1; i >= 0; i--)
            _components[i].Removed();
    }

    /// <summary>
    /// Updates this entity and its descendants.
    /// </summary>
    internal void UpdateHierarchy()
    {
        foreach (var component in _components)
            component.Update();

        foreach (var child in _children)
            child.UpdateHierarchy();
    }

    private bool IsAncestorOf(Entity other)
    {
        for (var current = Parent; current is not null; current = current.Parent)
        {
            if (current == other)
                return true;
        }

        return false;
    }
}
