using System.Numerics;
using Foster.Framework;
using Kitsune.Bridges.Platformer;
using Kitsune.Core;

using var game = new PlatformerDemoApp();
game.Run();

/// <summary>
/// One-screen Platformer Bridge demo — forgiveness-friendly gaps, ledges, and platforms.
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

        AddSolid(scene, new Vector2(0, 0), new Vector2(40, 540));
        AddSolid(scene, new Vector2(920, 0), new Vector2(40, 540));

        // Main floor with a gap (jump buffer — press jump before landing on the right side).
        AddSolid(scene, new Vector2(40, 500), new Vector2(220, 40));
        AddSolid(scene, new Vector2(380, 500), new Vector2(540, 40));

        // Safety floor — keeps missed jumps inside the play area.
        AddSolid(scene, new Vector2(40, 528), new Vector2(840, 12));

        // Walk-off ledge (coyote time — jump shortly after leaving the right edge).
        AddSolid(scene, new Vector2(80, 448), new Vector2(180, 20), new Color(0.42f, 0.44f, 0.52f, 1f));

        // Trigger zone — walk through without blocking (player tints while inside).
        AddTriggerZone(scene, new Vector2(300, 468), new Vector2(60, 32));

        // Elevated platforms on the right side of the gap.
        AddSolid(scene, new Vector2(420, 400), new Vector2(160, 20), new Color(0.40f, 0.43f, 0.50f, 1f));
        AddSolid(scene, new Vector2(620, 360), new Vector2(140, 20), new Color(0.38f, 0.41f, 0.48f, 1f));

        // Moving platform (registered before player so KinematicSolid updates first each frame).
        AddMovingPlatform(scene, new Vector2(260, 500), new Vector2(360, 500), 70f);

        var player = new Entity
        {
            Position = new Vector2(80, 468),
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
        var playerSprite = new RectSprite(32, 32, new Color(0.95f, 0.55f, 0.25f, 1f));
        player.Add(playerSprite);
        player.Add(new TriggerSensor(playerSprite));
        scene.Add(player);
        scene.Tags.Register(player, "player");

        return scene;
    }

    private static void AddMovingPlatform(Scene scene, Vector2 start, Vector2 end, float speed)
    {
        var platform = new Entity
        {
            Position = start,
            Depth = 2,
        };
        platform.Add(new Hitbox(80, 20));
        platform.Add(new Solid());
        platform.Add(new KinematicSolid
        {
            EndPosition = end,
            Speed = speed,
            DeltaTimeSource = () => DeltaTime,
        });
        platform.Add(new RectSprite(80, 20, new Color(0.55f, 0.65f, 0.78f, 1f)));
        scene.Add(platform);
    }

    private static void AddSolid(Scene scene, Vector2 position, Vector2 size) =>
        AddSolid(scene, position, size, new Color(0.35f, 0.38f, 0.45f, 1f));

    private static void AddSolid(Scene scene, Vector2 position, Vector2 size, Color color)
    {
        var wall = new Entity
        {
            Position = position,
            Depth = 1,
        };
        wall.Add(new Hitbox(size.X, size.Y));
        wall.Add(new Solid());
        wall.Add(new RectSprite(size.X, size.Y, color));
        scene.Add(wall);
    }

    private static void AddTriggerZone(Scene scene, Vector2 position, Vector2 size)
    {
        var zone = new Entity
        {
            Position = position,
            Depth = 0,
        };
        zone.Add(new Hitbox(size.X, size.Y));
        zone.Add(new TriggerSolid());
        zone.Add(new RectSprite(size.X, size.Y, new Color(0.35f, 0.85f, 0.45f, 0.35f)));
        scene.Add(zone);
    }
}

/// <summary>
/// Tints the player sprite while overlapping a <see cref="TriggerSolid"/>.
/// </summary>
sealed class TriggerSensor(RectSprite sprite) : Component
{
    private static readonly Color Normal = new(0.95f, 0.55f, 0.25f, 1f);
    private static readonly Color InTrigger = new(0.55f, 0.95f, 0.45f, 1f);

    public override void Update()
    {
        if (Entity?.Scene is null)
            return;

        Hitbox? hitbox = null;
        foreach (var component in Entity.Components)
        {
            if (component is Hitbox box)
            {
                hitbox = box;
                break;
            }
        }

        if (hitbox is null)
            return;

        var inside = Entity.Scene.CollideCheck(hitbox, Solid.Tag, CollisionLayer.Trigger);
        sprite.Color = inside ? InTrigger : Normal;
    }
}

/// <summary>
/// Draws a filled rectangle at the entity's world position.
/// </summary>
sealed class RectSprite(float width, float height, Color color) : Component
{
    public Color Color { get; set; } = color;

    public override void Render(Batcher batcher)
    {
        if (Entity is null)
            return;

        batcher.Rect(Entity.WorldPosition, new Vector2(width, height), Color);
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
