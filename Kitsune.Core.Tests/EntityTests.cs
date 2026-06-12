namespace Kitsune.Core.Tests;

public class EntityTests
{
    [Fact]
    public void AddChild_ParentsEntityAndRemovesFromParent()
    {
        var parent = new Entity();
        var child = new Entity();

        parent.Add(child);

        Assert.Same(parent, child.Parent);
        Assert.Contains(child, parent.Children);

        Assert.True(parent.Remove(child));
        Assert.Null(child.Parent);
        Assert.DoesNotContain(child, parent.Children);
    }

    [Fact]
    public void Scene_TracksParentChildHierarchy()
    {
        var scene = new Scene();
        var parent = new Entity();
        var child = new Entity();

        scene.Add(parent);
        parent.Add(child);

        scene.Begin();

        Assert.Same(scene, parent.Scene);
        Assert.Same(scene, child.Scene);

        parent.Remove(child);

        Assert.Null(child.Scene);
    }
}
