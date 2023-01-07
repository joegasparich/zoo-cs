using Zoo.tools;
using Zoo.util;
using Zoo.world;

namespace Zoo;

public static class DebugSettings {
    public static bool CellGrid        = false;
    public static bool BiomeChunks     = false;
    public static bool ElevationGrid   = false;
    public static bool AreaGrid        = false;
    public static bool PathfindingGrid = false;
    public static bool EntityLocations = false;
    public static bool DrawPickBuffer  = false;
}

public class Zoo : ISerialisable {
    // Config
    public int     Width  = 10;
    public int     Height = 10;
    public IntVec2 Entrance;
    
    // State
    public World       World;
    public ToolManager Tools;
    
    public Zoo() {
        Tools = new ToolManager();

        Entrance = new IntVec2(Width / 2, Height - 1);
    }

    public void Setup() {
        World = new World(Width, Height);
        
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
        Tools.Render();
    }

    public void RenderLate() {
        Tools.RenderLate();
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