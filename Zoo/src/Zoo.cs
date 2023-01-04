using Zoo.tools;
using Zoo.world;

namespace Zoo;

public static class DebugSettings {
    public static bool CellGrid        = false;
    public static bool BiomeChunks     = false;
    public static bool ElevationGrid   = false;
    public static bool AreaGrid        = false;
    public static bool PathfindingGrid = false;
}

public class Zoo : ISerialisable {
    // State
    public World World { get; }
    public ToolManager Tools;
    
    public Zoo() {
        World = new World(10, 10);
        Tools = new ToolManager();
    }

    public void Setup() {
        World.Setup();
        Tools.Setup();
    }

    public void Cleanup() {
        World.Reset();
        Tools.Cleanup();
    }

    public void PreUpdate() {
        World.PreUpdate();
        Tools.PreUpdate();
    }

    public void Update() {
        World.Update();
        Tools.Update();
    }

    public void PostUpdate() {
        World.PostUpdate();
        Tools.PostUpdate();
    }

    public void Render() {
        World.Render();
    }

    public void RenderLate() {
        Tools.Render();
        World.RenderDebug();
    }

    public void OnGUI() {
        Tools.OnGUI();
    }

    public void OnInput(InputEvent evt) {
        Tools.OnInput(evt);
    }

    public void Serialise() {
        Find.SaveManager.ArchiveDeep("world", World);
    }
}