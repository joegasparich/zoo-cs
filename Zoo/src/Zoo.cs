using Zoo.entities;
using Zoo.tools;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo;

public static class DebugSettings {
    public static bool CellGrid        = false;
    public static bool TerrainChunks   = false;
    public static bool ElevationGrid   = false;
    public static bool AreaGrid        = false;
    public static bool PathfindingGrid = false;
    public static bool DrawPaths       = false;
    public static bool EntityLocations = false;
    public static bool DrawPickBuffer  = false;
}

public class Zoo : ISerialisable {
    // Constants
    private const int GuestArrivalInterval = 600; // 10 seconds
    private const int EntryFee             = 20; // TODO: Make this configurable
    
    // Config
    public int     Width  = 10;
    public int     Height = 10;
    public IntVec2 Entrance;
    
    // State
    public  World                              World;
    public  ToolManager                        Tools;
    public  SelectionManager                   Selection;
    private string                             infoWidgetHandle;

    public int Funds = 1000;
    
    // Caches
    public readonly HashSet<Animal>                    Animals = new();
    public readonly HashSet<Guest>                     Guests  = new();
    public readonly HashSet<Staff>                     Staff   = new();
    public          Dictionary<string, IBlueprintable> Blueprints { get; } = new();
    
    public Zoo() {
        Tools = new ToolManager();
        Selection = new SelectionManager();

        Entrance = new IntVec2(Width / 2, Height - 1);
    }

    public void Setup() {
        World = new World(Width, Height);
        
        World.Setup();
        Tools.Setup();

        infoWidgetHandle = Find.UI.PushWindow(new Widget_ZooInfo());
    }

    public void Cleanup() {
        World.Reset();
        Tools.Cleanup();

        Find.UI.CloseWindow(infoWidgetHandle);
    }

    public void PreUpdate() {
        World.PreUpdate();
    }

    public void Update() {
        World.Update();
        Tools.ConstantUpdate();
        Selection.Update();

        if (Animals.Count > 0) {
            if (Game.Ticks % GuestArrivalInterval == 0) {
                var guest = GenEntity.CreateGuest(Entrance.TileCentre());
                Game.RegisterEntity(guest);
                AddFunds(EntryFee);
            }
        }
    }

    public void PostUpdate() {
        World.PostUpdate();
    }

    public void ConstantUpdate() {
        Tools.ConstantUpdate();
    }

    public void Render() {
        World.Render();
        Tools.Render();
        Selection.Render();
    }

    public void RenderLate() {
        Tools.RenderLate();
        World.RenderDebug();
    }

    public void OnGUI() {
        Tools.OnGUI();
    }

    public void OnInput(InputEvent evt) {
        try {
            if (!evt.consumed) Tools.OnInput(evt);
            if (!evt.consumed) Selection.OnInput(evt);
            if (!evt.consumed) World.OnInput(evt);
        }
        catch (Exception e) {
            Debug.Error("Error during input", e);
        }
    }

    public void DeductFunds(int amount) {
        Funds -= amount;
    }

    public void AddFunds(int amount) {
        Funds += amount;
    }

    public void RegisterBlueprint(IBlueprintable blueprint) {
        Blueprints.Add(blueprint.BlueprintId, blueprint);
    }
    public void UnregisterBlueprint(IBlueprintable blueprint) {
        Blueprints.Remove(blueprint.BlueprintId);
    }

    public void Serialise() {
        Find.SaveManager.ArchiveValue("funds", ref Funds);
        Find.SaveManager.ArchiveValue("entrance", ref Entrance);

        Find.SaveManager.ArchiveDeep("world", World);
    }
    public void PostLoad() {
        World.PostLoad();
    }

}