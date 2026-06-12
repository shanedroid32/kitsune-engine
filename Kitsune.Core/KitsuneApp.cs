using System.Collections.ObjectModel;
using Foster.Framework;

namespace Kitsune.Core;

/// <summary>
/// Base application type for Kitsune games built on Foster.
/// </summary>
public abstract class KitsuneApp : App
{
    private readonly List<Scene> _scenes = [];
    private readonly Queue<StackOperation> _pendingOperations = new();
    private Batcher? _batcher;
    private bool _inUpdate;

    /// <summary>
    /// The scene at the top of the stack, if any.
    /// </summary>
    public Scene? Scene => _scenes.Count > 0 ? _scenes[^1] : null;

    /// <summary>
    /// The scenes on the stack from bottom to top.
    /// </summary>
    public IReadOnlyList<Scene> Scenes => new ReadOnlyCollection<Scene>(_scenes);

    /// <summary>
    /// The draw batcher created during startup, or <see langword="null"/> before <see cref="Startup"/>.
    /// </summary>
    protected Batcher? Batcher => _batcher;

    /// <summary>
    /// Creates a Kitsune application with the given window configuration.
    /// </summary>
    /// <param name="title">The application and window title.</param>
    /// <param name="width">The initial window width in pixels.</param>
    /// <param name="height">The initial window height in pixels.</param>
    protected KitsuneApp(string title, int width = 1280, int height = 720)
        : base(new AppConfig
        {
            ApplicationName = title,
            WindowTitle = title,
            Width = width,
            Height = height,
        })
    {
    }

    /// <summary>
    /// Adds <paramref name="scene"/> on top of the stack without ending covered scenes.
    /// Applies immediately outside <see cref="Update"/>; deferred until after the current update pass when called during <see cref="Update"/>.
    /// </summary>
    /// <param name="scene">The scene to push.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scene"/> is null.</exception>
    public void Push(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        EnqueueOrApply(new PushOperation(scene));
    }

    /// <summary>
    /// Removes the top scene from the stack and ends it.
    /// Applies immediately outside <see cref="Update"/>; deferred until after the current update pass when called during <see cref="Update"/>.
    /// No-op when the stack is empty.
    /// </summary>
    public void Pop()
    {
        EnqueueOrApply(PopOperation.Instance);
    }

    /// <summary>
    /// Ends every scene on the stack and leaves <paramref name="scene"/> as the sole top scene.
    /// Applies immediately outside <see cref="Update"/>; deferred until after the current update pass when called during <see cref="Update"/>.
    /// </summary>
    /// <param name="scene">The scene to make active.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scene"/> is null.</exception>
    public void Replace(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        EnqueueOrApply(new ReplaceOperation(scene));
    }

    /// <inheritdoc />
    protected override void Startup()
    {
        _batcher = new(GraphicsDevice);
    }

    /// <inheritdoc />
    protected override void Shutdown()
    {
        EndAllScenes();
        _batcher?.Dispose();
        _batcher = null;
    }

    /// <inheritdoc />
    protected override void Update()
    {
        _inUpdate = true;

        try
        {
            UpdateScenes();
        }
        finally
        {
            _inUpdate = false;
            FlushPendingStackChanges();
        }
    }

    /// <inheritdoc />
    protected override void Render()
    {
        Window.Clear(Color.Black);

        if (_batcher is not null)
        {
            foreach (var scene in GetScenesForRender())
                scene.Render(_batcher);
        }

        _batcher?.Render(Window);
        _batcher?.Clear();
    }

    /// <summary>
    /// Called after each applied <see cref="Push"/>, <see cref="Pop"/>, or <see cref="Replace"/> stack operation.
    /// </summary>
    /// <param name="previousTop">The top scene before the operation, or <see langword="null"/> when the stack was empty.</param>
    /// <param name="newTop">The top scene after the operation, or <see langword="null"/> when the stack is empty.</param>
    protected virtual void OnSceneTransition(Scene? previousTop, Scene? newTop)
    {
    }

    internal IReadOnlyList<Scene> GetScenesForUpdate() => GetScenesForPass(includeWhenCovered: scene => scene.UpdatesWhenCovered);

    internal IReadOnlyList<Scene> GetScenesForRender() => GetScenesForPass(includeWhenCovered: scene => scene.RendersWhenCovered);

    private void EnqueueOrApply(StackOperation operation)
    {
        if (_inUpdate)
        {
            _pendingOperations.Enqueue(operation);
            return;
        }

        Apply(operation);
    }

    private void FlushPendingStackChanges()
    {
        while (_pendingOperations.Count > 0)
            Apply(_pendingOperations.Dequeue());
    }

    private void Apply(StackOperation operation)
    {
        switch (operation)
        {
            case PushOperation push:
                ApplyPush(push.Scene);
                break;
            case PopOperation:
                ApplyPop();
                break;
            case ReplaceOperation replace:
                ApplyReplace(replace.Scene);
                break;
            default:
                throw new InvalidOperationException($"Unknown stack operation: {operation.GetType().Name}");
        }
    }

    private void ApplyPush(Scene scene)
    {
        var previousTop = Scene;
        _scenes.Add(scene);
        scene.Begin();
        OnSceneTransition(previousTop, scene);
    }

    private void ApplyPop()
    {
        if (_scenes.Count == 0)
            return;

        var previousTop = _scenes[^1];
        previousTop.End();
        _scenes.RemoveAt(_scenes.Count - 1);
        OnSceneTransition(previousTop, Scene);
    }

    private void ApplyReplace(Scene scene)
    {
        var previousTop = Scene;
        EndAllScenes();
        _scenes.Add(scene);
        scene.Begin();
        OnSceneTransition(previousTop, scene);
    }

    private void EndAllScenes()
    {
        for (var i = _scenes.Count - 1; i >= 0; i--)
            _scenes[i].End();

        _scenes.Clear();
    }

    private void UpdateScenes()
    {
        foreach (var scene in GetScenesForUpdate())
            scene.Update();
    }

    private IReadOnlyList<Scene> GetScenesForPass(Func<Scene, bool> includeWhenCovered)
    {
        if (_scenes.Count == 0)
            return [];

        var result = new List<Scene>(_scenes.Count);

        for (var i = 0; i < _scenes.Count; i++)
        {
            var scene = _scenes[i];
            if (i == _scenes.Count - 1 || includeWhenCovered(scene))
                result.Add(scene);
        }

        return result;
    }

    private abstract class StackOperation;

    private sealed class PushOperation(Scene scene) : StackOperation
    {
        public Scene Scene { get; } = scene;
    }

    private sealed class PopOperation : StackOperation
    {
        public static PopOperation Instance { get; } = new();

        private PopOperation()
        {
        }
    }

    private sealed class ReplaceOperation(Scene scene) : StackOperation
    {
        public Scene Scene { get; } = scene;
    }
}
