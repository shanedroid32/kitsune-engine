using System.Collections.ObjectModel;
using Foster.Framework;

namespace Kitsune.Core;

/// <summary>
/// Base application type for Kitsune games built on Foster.
/// </summary>
public abstract class KitsuneApp : App
{
    private readonly List<Scene> _scenes = [];
    private Batcher? _batcher;
    private bool _inUpdate;
    private Scene? _pendingReplace;

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
    /// Ends every scene on the stack and leaves <paramref name="scene"/> as the sole top scene.
    /// Applies immediately outside <see cref="Update"/>; deferred until after the current update pass when called during <see cref="Update"/>.
    /// </summary>
    /// <param name="scene">The scene to make active.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scene"/> is null.</exception>
    public void Replace(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        if (_inUpdate)
        {
            _pendingReplace = scene;
            return;
        }

        ApplyReplace(scene);
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
            Scene?.Update();
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

        if (Scene is not null && _batcher is not null)
            Scene.Render(_batcher);

        _batcher?.Render(Window);
        _batcher?.Clear();
    }

    private void ApplyReplace(Scene scene)
    {
        EndAllScenes();
        _scenes.Add(scene);
        scene.Begin();
    }

    private void EndAllScenes()
    {
        for (var i = _scenes.Count - 1; i >= 0; i--)
            _scenes[i].End();

        _scenes.Clear();
    }

    private void FlushPendingStackChanges()
    {
        if (_pendingReplace is not Scene scene)
            return;

        _pendingReplace = null;
        ApplyReplace(scene);
    }
}
