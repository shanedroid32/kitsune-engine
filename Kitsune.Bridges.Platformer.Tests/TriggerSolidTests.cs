using Kitsune.Core;

namespace Kitsune.Bridges.Platformer.Tests;

public class TriggerSolidTests
{
    [Fact]
    public void Added_RegistersSolidTagAndSetsTriggerLayer()
    {
        var scene = new Scene();
        var entity = new Entity();
        var hitbox = new Hitbox(16, 16);
        entity.Add(hitbox);
        entity.Add(new TriggerSolid());
        scene.Add(entity);
        scene.Begin();

        Assert.Equal([entity], scene.Tracker.GetEntities(Solid.Tag).ToArray());
        Assert.Equal(CollisionLayer.Trigger, hitbox.Layer);
    }
}
