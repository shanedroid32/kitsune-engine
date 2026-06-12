namespace Kitsune.Core.Tests;

public class TagListTests
{
    [Fact]
    public void Register_And_Unregister_EntityByTag()
    {
        var scene = new Scene();
        var player = new Entity();
        var enemy = new Entity();

        scene.Add(player);
        scene.Add(enemy);

        scene.Tags.Register(player, "player");
        scene.Tags.Register(enemy, "enemy");
        scene.Tags.Register(enemy, "hostile");

        Assert.Equal([player], scene.Tracker.GetEntities("player"));
        Assert.Equal([enemy], scene.Tracker.GetEntities("enemy"));
        Assert.Equal([enemy], scene.Tracker.GetEntities("hostile"));

        scene.Tags.Unregister(enemy, "hostile");

        Assert.Empty(scene.Tracker.GetEntities("hostile"));
        Assert.Equal([enemy], scene.Tracker.GetEntities("enemy"));
    }

    [Fact]
    public void Scene_Remove_UnregistersAllTags()
    {
        var scene = new Scene();
        var player = new Entity();

        scene.Add(player);
        scene.Tags.Register(player, "player");
        scene.Tags.Register(player, "controllable");

        Assert.Equal([player], scene.Tracker.GetEntities("player"));
        Assert.Equal([player], scene.Tracker.GetEntities("controllable"));

        scene.Remove(player);

        Assert.Empty(scene.Tracker.GetEntities("player"));
        Assert.Empty(scene.Tracker.GetEntities("controllable"));
    }
}
