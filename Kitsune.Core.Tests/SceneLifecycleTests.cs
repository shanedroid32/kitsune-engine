namespace Kitsune.Core.Tests;

public class SceneLifecycleTests
{
    [Fact]
    public void Begin_ActivatesComponents_End_DeactivatesComponents()
    {
        var scene = new Scene();
        var entity = new Entity();
        var component = new RecordingComponent();

        entity.Add(component);
        scene.Add(entity);

        Assert.Empty(component.Events);

        scene.Begin();
        Assert.Equal(["added"], component.Events);

        component.Events.Clear();
        scene.End();
        Assert.Equal(["removed"], component.Events);
    }

    private sealed class RecordingComponent : Component
    {
        public List<string> Events { get; } = [];

        public override void Added() => Events.Add("added");

        public override void Removed() => Events.Add("removed");
    }
}
