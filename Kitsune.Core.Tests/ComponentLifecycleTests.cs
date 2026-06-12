namespace Kitsune.Core.Tests;

public class ComponentLifecycleTests
{
    [Fact]
    public void Added_Update_And_Removed_RunInOrder()
    {
        var scene = new Scene();
        var entity = new Entity();
        var first = new RecordingComponent();
        var second = new RecordingComponent();
        var third = new RecordingComponent();

        entity.Add(first);
        entity.Add(second);
        entity.Add(third);
        scene.Add(entity);
        scene.Begin();

        Assert.Equal(["added-1"], first.Events);
        Assert.Equal(["added-2"], second.Events);
        Assert.Equal(["added-3"], third.Events);

        first.Events.Clear();
        second.Events.Clear();
        third.Events.Clear();

        scene.Update();

        Assert.Equal(["update-1"], first.Events);
        Assert.Equal(["update-2"], second.Events);
        Assert.Equal(["update-3"], third.Events);

        first.Events.Clear();
        second.Events.Clear();
        third.Events.Clear();

        scene.End();

        Assert.Equal(["removed-1"], first.Events);
        Assert.Equal(["removed-2"], second.Events);
        Assert.Equal(["removed-3"], third.Events);
    }

    private sealed class RecordingComponent : Component
    {
        private static int _counter;

        private readonly int _id = Interlocked.Increment(ref _counter);

        public List<string> Events { get; } = [];

        public override void Added() => Events.Add($"added-{_id}");

        public override void Update() => Events.Add($"update-{_id}");

        public override void Removed() => Events.Add($"removed-{_id}");
    }
}
