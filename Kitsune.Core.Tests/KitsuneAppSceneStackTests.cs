namespace Kitsune.Core.Tests;

public class KitsuneAppSceneStackTests
{
    [Fact]
    public void Scene_IsNull_WhenStackEmpty()
    {
        var app = new TestKitsuneApp();

        Assert.Null(app.Scene);
        Assert.Empty(app.Scenes);
    }

    [Fact]
    public void Replace_Null_Throws()
    {
        var app = new TestKitsuneApp();

        Assert.Throws<ArgumentNullException>(() => app.Replace(null!));
    }

    [Fact]
    public void Replace_BeginsScene_AndExposesTop()
    {
        var app = new TestKitsuneApp();
        var scene = new Scene();

        app.Replace(scene);

        Assert.True(scene.IsActive);
        Assert.Same(scene, app.Scene);
        Assert.Single(app.Scenes);
    }

    [Fact]
    public void Replace_EndsPreviousTop()
    {
        var app = new TestKitsuneApp();
        var first = new Scene();
        var second = new Scene();

        app.Replace(first);
        app.Replace(second);

        Assert.False(first.IsActive);
        Assert.True(second.IsActive);
        Assert.Same(second, app.Scene);
        Assert.Single(app.Scenes);
    }

    [Fact]
    public void Replace_OutsideUpdate_AppliesImmediately()
    {
        var app = new TestKitsuneApp();
        var scene = new Scene();
        var entity = new Entity();
        var component = new RecordingComponent();

        entity.Add(component);
        scene.Add(entity);
        app.Replace(scene);

        Assert.Equal(["added"], component.Events);
    }

    [Fact]
    public void Update_OnlyRunsTopScene()
    {
        var app = new TestKitsuneApp();
        var scene = new Scene();
        var entity = new Entity();
        var component = new RecordingComponent();

        entity.Add(component);
        scene.Add(entity);
        app.Replace(scene);
        component.Events.Clear();

        app.RunUpdate();

        Assert.Contains("update", component.Events);
    }

    [Fact]
    public void Replace_DuringUpdate_IsDeferredUntilAfterUpdate()
    {
        var app = new TestKitsuneApp();
        var first = new Scene();
        var second = new Scene();
        var entity = new Entity();

        entity.Add(new DeferReplaceComponent(app, second));
        first.Add(entity);
        app.Replace(first);

        app.RunUpdate();

        Assert.False(first.IsActive);
        Assert.True(second.IsActive);
        Assert.Same(second, app.Scene);
    }

    [Fact]
    public void Replace_DuringUpdate_DoesNotUpdateNewTopUntilNextFrame()
    {
        var app = new TestKitsuneApp();
        var first = new Scene();
        var second = new Scene();
        var entity = new Entity();
        var component = new RecordingComponent();
        var secondEntity = new Entity();

        entity.Add(new DeferReplaceComponent(app, second));
        first.Add(entity);
        secondEntity.Add(component);
        second.Add(secondEntity);
        app.Replace(first);

        app.RunUpdate();

        Assert.DoesNotContain("update", component.Events);

        app.RunUpdate();

        Assert.Contains("update", component.Events);
    }

    private sealed class TestKitsuneApp : Kitsune.Core.KitsuneApp
    {
        public TestKitsuneApp() : base("Test", 1, 1) { }

        public void RunUpdate() => Update();
    }

    private sealed class RecordingComponent : Component
    {
        public List<string> Events { get; } = [];

        public override void Added() => Events.Add("added");

        public override void Removed() => Events.Add("removed");

        public override void Update() => Events.Add("update");
    }

    private sealed class DeferReplaceComponent(Kitsune.Core.KitsuneApp app, Scene next) : Component
    {
        public override void Update() => app.Replace(next);
    }
}
