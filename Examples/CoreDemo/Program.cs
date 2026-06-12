using System.Numerics;
using Foster.Framework;
using Kitsune.Bridges.Platformer;
using Kitsune.Core;

using var game = new CoreDemoApp();
game.Run();

/// <summary>
/// Demonstrates Kitsune Core: entities, hitboxes, tags, tracker, and depth ordering.
/// Movement resolution uses the Platformer Bridge Actor and Solid components.
/// </summary>
sealed class CoreDemoApp : KitsuneApp
{
    internal static Input? GameInput { get; private set; }

    internal static float DeltaTime { get; private set; }

    public CoreDemoApp() : base("Kitsune Core Demo", 960, 540)
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

        AddWall(scene, new Vector2(0, 500), new Vector2(960, 40));
        AddWall(scene, new Vector2(0, 0), new Vector2(40, 540));
        AddWall(scene, new Vector2(920, 0), new Vector2(40, 540));
        AddWall(scene, new Vector2(300, 350), new Vector2(200, 20));

        var player = new Entity
        {
            Position = new Vector2(120, 420),
            Depth = 10,
        };
        player.Add(new Hitbox(32, 32));
        player.Add(new Actor());
        player.Add(new ArrowKeyDriver());
        player.Add(new RectSprite(32, 32, new Color(0.95f, 0.55f, 0.25f, 1f)));
        scene.Add(player);
        scene.Tags.Register(player, "player");

        var back = new Entity
        {
            Position = new Vector2(520, 280),
            Depth = 0,
        };
        back.Add(new RectSprite(80, 80, Color.Blue));
        scene.Add(back);

        var front = new Entity
        {
            Position = new Vector2(550, 310),
            Depth = 5,
        };
        front.Add(new RectSprite(80, 80, Color.Red));
        scene.Add(front);

        return scene;
    }

    private static void AddWall(Scene scene, Vector2 position, Vector2 size)
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
/// Reads arrow keys and drives <see cref="Actor"/> movement for the demo.
/// </summary>
sealed class ArrowKeyDriver : Component
{
    private const float Speed = 220f;

    public override void Update()
    {
        if (Entity is null || CoreDemoApp.GameInput is null)
            return;

        var actor = FindActor();
        if (actor is null)
            return;

        var keyboard = CoreDemoApp.GameInput.Keyboard;
        var move = Vector2.Zero;

        if (keyboard.Down(Keys.Left))
            move.X -= 1f;
        if (keyboard.Down(Keys.Right))
            move.X += 1f;
        if (keyboard.Down(Keys.Up))
            move.Y -= 1f;
        if (keyboard.Down(Keys.Down))
            move.Y += 1f;

        if (move == Vector2.Zero)
            return;

        if (move.LengthSquared() > 1f)
            move = Vector2.Normalize(move);

        move *= Speed * CoreDemoApp.DeltaTime;

        actor.MoveX(move.X);
        actor.MoveY(move.Y);
    }

    private Actor? FindActor()
    {
        foreach (var component in Entity!.Components)
        {
            if (component is Actor actor)
                return actor;
        }

        return null;
    }
}
