using Foster.Framework;

namespace Kitsune.Core;

/// <summary>
/// Base application type for Kitsune games built on Foster.
/// </summary>
public abstract class KitsuneApp : App
{
    private Batcher? _batcher;

    /// <summary>
    /// The currently active scene, if any.
    /// </summary>
    public Scene? Scene { get; set; }

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

    /// <inheritdoc />
    protected override void Startup()
    {
        _batcher = new(GraphicsDevice);
        Scene?.Begin();
    }

    /// <inheritdoc />
    protected override void Shutdown()
    {
        Scene?.End();
        _batcher?.Dispose();
        _batcher = null;
    }

    /// <inheritdoc />
    protected override void Update()
    {
        Scene?.Update();
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
}
