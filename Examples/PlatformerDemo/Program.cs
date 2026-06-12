using System.Numerics;
using Foster.Framework;
using Kitsune.Bridges.Platformer;
using Kitsune.Core;

using var game = new PlatformerDemoApp();
game.Run();

/// <summary>
/// Playable shell for the Platformer Bridge — floor, boundaries, and a controllable player.
/// </summary>
sealed class PlatformerDemoApp : KitsuneApp
{
    internal static Input? GameInput { get; private set; }

    internal static float DeltaTime { get; private set; }

    public PlatformerDemoApp() : base("Kitsune Platformer Demo", 960, 540)
    {
        Scene = BuildScene();
    }

    protected override void Startup()
    {
        base.Startup();
        GameInput = Input;
    }

    protected override void Shutdown()
    {
        GameInput = null;
        base.Shutdown();
    }

    protected override void Update()
    {
        DeltaTime = (float)Time.Delta;
        base.Update();
    }

    private static Scene BuildScene()
    {
        var scene = new Scene();

        AddSolid(scene, new Vector2(0, 500), new Vector2(960, 40));
        AddSolid(scene, new Vector2(0, 0), new Vector2(40, 540));
        AddSolid(scene, new Vector2(920, 0), new Vector2(40, 540));

        var player = new Entity
        {
            Position = new Vector2(120, 468),
            Depth = 10,
        };
        player.Add(new Hitbox(32, 32));
        player.Add(new Actor());
        player.Add(new WasdDriver());
        player.Add(new PlatformerBody
        {
            Gravity = 950f,
            JumpSpeed = 620f,
            DeltaTimeSource = () => DeltaTime,
        });
        player.Add(new RectSprite(32, 32, new Color(0.95f, 0.55f, 0.25f, 1f)));
        scene.Add(player);
        scene.Tags.Register(player, "player");

        return scene;
    }

    private static void AddSolid(Scene scene, Vector2 position, Vector2 size)
    {
        var wall = new Entity
        {
            Position = position,
            Depth = 1,
        };
        wall.Add(new Hitbox(size.X, size.Y));
        wall.Add(new Solid());
        wall.Add(new RectSprite(size.X, size.Y, new Color(0.35f, 0.38f, 0.45f, 1f)));
        scene.Add(wall);
    }
}

/// <summary>
/// Draws a filled rectangle at the entity's world position.
/// </summary>
sealed class RectSprite(float width, float height, Color color) : Component
{
    public override void Render(Batcher batcher)
    {
        if (Entity is null)
            return;

        batcher.Rect(Entity.WorldPosition, new Vector2(width, height), color);
    }
}

/// <summary>
/// WASD movement and Space/W jump for the PlatformerDemo player.
/// </summary>
sealed class WasdDriver : Component
{
    private const float Speed = 380f;

    public override void Update()
    {
        if (Entity is null || PlatformerDemoApp.GameInput is null)
            return;

        var actor = FindComponent<Actor>();
        var body = FindComponent<PlatformerBody>();
        if (actor is null || body is null)
            return;

        var keyboard = PlatformerDemoApp.GameInput.Keyboard;

        if (keyboard.Pressed(Keys.Space) || keyboard.Pressed(Keys.W))
            body.JumpRequested = true;

        var moveX = 0f;
        if (keyboard.Down(Keys.A))
            moveX -= 1f;
        if (keyboard.Down(Keys.D))
            moveX += 1f;

        if (moveX != 0f)
            actor.MoveX(moveX * Speed * PlatformerDemoApp.DeltaTime);
    }

    private T? FindComponent<T>() where T : Component
    {
        foreach (var component in Entity!.Components)
        {
            if (component is T match)
                return match;
        }

        return null;
    }
}
