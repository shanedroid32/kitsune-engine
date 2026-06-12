using System.Numerics;
using Foster.Framework;
using Kitsune.Bridges.Platformer;
using Kitsune.Core;

using var game = new PlatformerDemoApp();
game.Run();

/// <summary>
/// One-screen Platformer Bridge demo — zoned layout teaching forgiveness, directional platforms, and kinematic hazards.
/// </summary>
sealed class PlatformerDemoApp : KitsuneApp
{
    internal static Input? GameInput { get; private set; }

    internal static float DeltaTime { get; private set; }

    private static readonly Vector2 SpawnPosition = new(72, 468);

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
        AddSolid(scene, new Vector2(40, 528), new Vector2(840, 12));

        // Zone 1 — Spawn & coyote (wide floor, walk-off ledge with room to react).
        AddSolid(scene, new Vector2(40, 500), new Vector2(200, 40));
        AddSolid(scene, new Vector2(56, 448), new Vector2(150, 20), new Color(0.42f, 0.44f, 0.52f, 1f));

        // Zone 2 — Jump buffer (120px gap; press jump before the right-hand floor).
        AddSolid(scene, new Vector2(360, 500), new Vector2(220, 40));

        // Zone 3 — Trigger (open floor on the left, away from the gap lane).
        AddTriggerZone(scene, new Vector2(96, 464), new Vector2(88, 36));

        // Zone 4 — One-way (optional upper crossing above the gap; rise through from the pit).
        AddOneWay(scene, new Vector2(248, 456), new Vector2(88, 12));

        // Zone 5 — Jump-through terrace (cyan; 100px+ clear below before the floor).
        AddJumpThrough(scene, new Vector2(400, 368), new Vector2(160, 16));
        AddSolid(scene, new Vector2(360, 500), new Vector2(140, 40));

        // Zone 6 — Momentum storage (pit between mover and island; jump off at the right end).
        AddSolid(scene, new Vector2(608, 500), new Vector2(100, 40));
        AddMovingPlatform(scene, new Vector2(140, 500), new Vector2(420, 500), 120f, width: 60);

        // Zone 7 — Crusher alcove (ride the lift up; jump off before the red lip or respawn).
        AddSolid(scene, new Vector2(660, 396), new Vector2(120, 20), new Color(0.40f, 0.43f, 0.50f, 1f));
        AddSolid(scene, new Vector2(668, 256), new Vector2(104, 20), new Color(0.72f, 0.32f, 0.28f, 1f));
        AddRisingPlatform(scene, new Vector2(676, 440), new Vector2(676, 284), 52f, width: 72);

        var player = new Entity
        {
            Position = SpawnPosition,
            Depth = 10,
        };
        player.Add(new Hitbox(32, 32));
        var actor = new Actor
        {
            OnSquish = () =>
            {
                player.Position = SpawnPosition;
                return true;
            },
        };
        player.Add(actor);
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

    private static void AddMovingPlatform(Scene scene, Vector2 start, Vector2 end, float speed, float width = 80f)
    {
        var platform = new Entity
        {
            Position = start,
            Depth = 2,
        };
        platform.Add(new Hitbox(width, 20));
        platform.Add(new Solid());
        platform.Add(new KinematicSolid
        {
            EndPosition = end,
            Speed = speed,
            DeltaTimeSource = () => DeltaTime,
        });
        platform.Add(new RectSprite(width, 20, new Color(0.55f, 0.65f, 0.78f, 1f)));
        scene.Add(platform);
    }

    private static void AddRisingPlatform(Scene scene, Vector2 start, Vector2 end, float speed, float width = 72f)
    {
        var lift = new Entity
        {
            Position = start,
            Depth = 4,
        };
        lift.Add(new Hitbox(width, 20));
        lift.Add(new Solid());
        lift.Add(new KinematicSolid
        {
            EndPosition = end,
            Speed = speed,
            DeltaTimeSource = () => DeltaTime,
        });
        lift.Add(new RectSprite(width, 20, new Color(0.58f, 0.58f, 0.62f, 1f)));
        scene.Add(lift);
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

    private static void AddOneWay(Scene scene, Vector2 position, Vector2 size)
    {
        var platform = new Entity
        {
            Position = position,
            Depth = 3,
        };
        platform.Add(new Hitbox(size.X, size.Y));
        platform.Add(new OneWaySolid());
        platform.Add(new RectSprite(size.X, size.Y, new Color(0.75f, 0.55f, 0.35f, 1f)));
        scene.Add(platform);
    }

    private static void AddJumpThrough(Scene scene, Vector2 position, Vector2 size)
    {
        var platform = new Entity
        {
            Position = position,
            Depth = 3,
        };
        platform.Add(new Hitbox(size.X, size.Y));
        platform.Add(new JumpThroughSolid());
        platform.Add(new RectSprite(size.X, size.Y, new Color(0.45f, 0.72f, 0.88f, 1f)));
        scene.Add(platform);
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

        if (keyboard.Down(Keys.S))
            body.DropThroughRequested = true;

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
