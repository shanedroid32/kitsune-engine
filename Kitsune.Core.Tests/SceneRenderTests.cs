namespace Kitsune.Core.Tests;

public class SceneRenderTests
{
    [Fact]
    public void GetRenderOrder_SortsByAscendingDepth()
    {
        var scene = new Scene();
        var back = new Entity { Depth = 0 };
        var middle = new Entity { Depth = 5 };
        var front = new Entity { Depth = 10 };

        scene.Add(middle);
        scene.Add(back);
        scene.Add(front);

        var order = scene.GetRenderOrder();

        Assert.Equal([back, middle, front], order);
    }
}
