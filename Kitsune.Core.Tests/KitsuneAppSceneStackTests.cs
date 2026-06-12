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

    [Fact]
    public void Push_Null_Throws()
    {
        var app = new TestKitsuneApp();

        Assert.Throws<ArgumentNullException>(() => app.Push(null!));
    }

    [Fact]
    public void Push_KeepsCoveredSceneBegun()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();

        app.Replace(level);
        app.Push(pause);

        Assert.True(level.IsActive);
        Assert.True(pause.IsActive);
        Assert.Equal([level, pause], app.Scenes);
        Assert.Same(pause, app.Scene);
    }

    [Fact]
    public void Push_FrozenCoveredScene_SkipsUpdate()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();
        var levelEntity = new Entity();
        var pauseEntity = new Entity();
        var levelComponent = new RecordingComponent();
        var pauseComponent = new RecordingComponent();

        levelEntity.Add(levelComponent);
        pauseEntity.Add(pauseComponent);
        level.Add(levelEntity);
        pause.Add(pauseEntity);
        app.Replace(level);
        app.Push(pause);
        levelComponent.Events.Clear();
        pauseComponent.Events.Clear();

        app.RunUpdate();

        Assert.DoesNotContain("update", levelComponent.Events);
        Assert.Contains("update", pauseComponent.Events);
    }

    [Fact]
    public void Push_UpdatesWhenCovered_AllowsCoveredUpdate()
    {
        var app = new TestKitsuneApp();
        var level = new Scene { UpdatesWhenCovered = true };
        var pause = new Scene();
        var levelEntity = new Entity();
        var pauseEntity = new Entity();
        var levelComponent = new RecordingComponent();
        var pauseComponent = new RecordingComponent();

        levelEntity.Add(levelComponent);
        pauseEntity.Add(pauseComponent);
        level.Add(levelEntity);
        pause.Add(pauseEntity);
        app.Replace(level);
        app.Push(pause);
        levelComponent.Events.Clear();
        pauseComponent.Events.Clear();

        app.RunUpdate();

        Assert.Contains("update", levelComponent.Events);
        Assert.Contains("update", pauseComponent.Events);
    }

    [Fact]
    public void Pop_ResumesCoveredSceneUpdate()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();
        var levelEntity = new Entity();
        var levelComponent = new RecordingComponent();

        levelEntity.Add(levelComponent);
        level.Add(levelEntity);
        app.Replace(level);
        app.Push(pause);
        app.Pop();
        levelComponent.Events.Clear();

        app.RunUpdate();

        Assert.Contains("update", levelComponent.Events);
        Assert.False(pause.IsActive);
        Assert.Same(level, app.Scene);
    }

    [Fact]
    public void Pop_EmptyStack_IsNoOp()
    {
        var app = new TestKitsuneApp();

        app.Pop();

        Assert.Null(app.Scene);
        Assert.Empty(app.Scenes);
    }

    [Fact]
    public void Push_DuringUpdate_IsDeferredUntilAfterUpdate()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();
        var entity = new Entity();

        entity.Add(new DeferPushComponent(app, pause));
        level.Add(entity);
        app.Replace(level);

        app.RunUpdate();

        Assert.Equal([level, pause], app.Scenes);
        Assert.Same(pause, app.Scene);
    }

    [Fact]
    public void Push_DuringUpdate_DoesNotUpdateNewTopUntilNextFrame()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();
        var entity = new Entity();
        var pauseEntity = new Entity();
        var pauseComponent = new RecordingComponent();

        entity.Add(new DeferPushComponent(app, pause));
        level.Add(entity);
        pauseEntity.Add(pauseComponent);
        pause.Add(pauseEntity);
        app.Replace(level);

        app.RunUpdate();

        Assert.DoesNotContain("update", pauseComponent.Events);

        app.RunUpdate();

        Assert.Contains("update", pauseComponent.Events);
    }

    [Fact]
    public void RenderStack_IncludesCoveredScene_ByDefault()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();

        app.Replace(level);
        app.Push(pause);

        Assert.Equal([level, pause], app.GetScenesForRender());
    }

    [Fact]
    public void RenderStack_SkipsCoveredScene_WhenRendersWhenCoveredFalse()
    {
        var app = new TestKitsuneApp();
        var level = new Scene { RendersWhenCovered = false };
        var pause = new Scene();

        app.Replace(level);
        app.Push(pause);

        Assert.Equal([pause], app.GetScenesForRender());
    }

    [Fact]
    public void Replace_MultiSceneStack_EndsAllScenes()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();
        var title = new Scene();

        app.Replace(level);
        app.Push(pause);
        app.Replace(title);

        Assert.False(level.IsActive);
        Assert.False(pause.IsActive);
        Assert.True(title.IsActive);
        Assert.Same(title, app.Scene);
        Assert.Single(app.Scenes);
    }

    [Fact]
    public void OnSceneTransition_Replace_ReportsPreviousTopBeforeWipe()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();
        var title = new Scene();

        app.Replace(level);
        app.Push(pause);
        app.Transitions.Clear();
        app.Replace(title);

        Assert.Equal([(pause, title)], app.Transitions);
    }

    [Fact]
    public void OnSceneTransition_Push_ReportsPreviousAndNewTop()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();

        app.Replace(level);
        app.Transitions.Clear();
        app.Push(pause);

        Assert.Equal([(level, pause)], app.Transitions);
    }

    [Fact]
    public void OnSceneTransition_Pop_ReportsPreviousAndNewTop()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();

        app.Replace(level);
        app.Push(pause);
        app.Transitions.Clear();
        app.Pop();

        Assert.Equal([(pause, level)], app.Transitions);
    }

    [Fact]
    public void Pop_Empty_DoesNotFireOnSceneTransition()
    {
        var app = new TestKitsuneApp();

        app.Pop();

        Assert.Empty(app.Transitions);
    }

    [Fact]
    public void PushThenPop_SameFrame_RestoresPriorTopWithoutSpuriousBegin()
    {
        var app = new TestKitsuneApp();
        var level = new Scene();
        var pause = new Scene();
        var entity = new Entity();
        var component = new RecordingComponent();

        entity.Add(new DeferPushThenPopComponent(app, pause));
        entity.Add(component);
        level.Add(entity);
        app.Replace(level);
        app.Transitions.Clear();

        app.RunUpdate();

        Assert.Same(level, app.Scene);
        Assert.True(level.IsActive);
        Assert.False(pause.IsActive);
        Assert.Equal(1, component.Events.Count(e => e == "added"));
        Assert.Equal([(level, pause), (pause, level)], app.Transitions);
    }

    private sealed class TestKitsuneApp : Kitsune.Core.KitsuneApp
    {
        public TestKitsuneApp() : base("Test", 1, 1) { }

        public List<(Scene? Previous, Scene? New)> Transitions { get; } = [];

        public void RunUpdate() => Update();

        protected override void OnSceneTransition(Scene? previousTop, Scene? newTop) =>
            Transitions.Add((previousTop, newTop));
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

    private sealed class DeferPushComponent(Kitsune.Core.KitsuneApp app, Scene next) : Component
    {
        public override void Update() => app.Push(next);
    }

    private sealed class DeferPushThenPopComponent(Kitsune.Core.KitsuneApp app, Scene pause) : Component
    {
        public override void Update()
        {
            app.Push(pause);
            app.Pop();
        }
    }
}
