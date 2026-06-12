using System.Numerics;
using Foster.Framework;
using Kitsune.Core;

using var game = new HelloWorldApp();
game.Run();

/// <summary>
/// Minimal Foster wiring demo through <see cref="KitsuneApp"/>.
/// </summary>
sealed class HelloWorldApp : KitsuneApp
{
    public HelloWorldApp() : base("Hello, world!")
    {
        Replace(new Scene());
    }

    protected override void Render()
    {
        Window.Clear(new Color(0.08f, 0.09f, 0.12f, 1f));

        if (Batcher is null)
            return;

        var center = new Vector2(Window.WidthInPixels, Window.HeightInPixels) / 2;
        Batcher.Circle(new Circle(center, 48), 24, new Color(0.95f, 0.55f, 0.25f, 1f));
        Batcher.Render(Window);
        Batcher.Clear();
    }
}
