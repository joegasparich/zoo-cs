using System.Text.Json.Nodes;
using Raylib_cs;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Elevation : Tool {
    private const float     DefaultRadius      = 0.65f;
    private const int       PlaceIntervalTicks = 5;
    
    private Elevation                      currentElevation = Elevation.Hill;
    private bool                           isDragging;
    private Dictionary<IntVec2, Elevation> oldElevationData = new();

    public Tool_Elevation(ToolManager tm) : base(tm) {}

    public override string   Name => "Elevation Tool";
    public override ToolType Type => ToolType.Elevation;

    public override void Set() {
        currentElevation = Elevation.Hill;

        Ghost.Type    = GhostType.Circle;
        Ghost.Radius  = DefaultRadius;
        Ghost.Elevate = true;
    }

    public override void OnInput(InputEvent evt) {
        // Only listen to down and up events so that we can't start dragging from UI
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = true;

            oldElevationData.Clear();
            
            evt.Consume();
        }
        if (evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            if (!isDragging) return;
            isDragging = false;

            toolManager.PushAction(new ToolAction() {
                Name = "Elevation brush",
                // Copy here so we don't lose the data when clearing
                Data = oldElevationData.ToDictionary(entry => entry.Key, entry => entry.Value),
                Undo = data => {
                    Find.World.Elevation.SetElevationFromUndoData((Dictionary<IntVec2, Elevation>)data);
                }
            });
            
            evt.Consume();
        }
    }

    public override void Update() {
        if (!isDragging || Game.Ticks % PlaceIntervalTicks != 0) return;
        
        var oldPoints = Find.World.Elevation.SetElevationInCircle(Find.Input.GetMouseWorldPos(), Ghost.Radius, currentElevation);

        foreach (var (p, e) in oldPoints) {
            oldElevationData.TryAdd(p, e);
        }
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immElevationPanel", new Rectangle(10, 60, 100, 100), inRect => {
            GUI.TextAlign = AlignMode.MiddleCenter;

            if (GUI.ButtonText(new Rectangle(10, 10, 80, 20), "Water")) currentElevation = Elevation.Water;
            if (GUI.ButtonText(new Rectangle(10, 40, 80, 20), "Flat"))  currentElevation = Elevation.Flat;
            if (GUI.ButtonText(new Rectangle(10, 70, 80, 20), "Hill")) currentElevation = Elevation.Hill;

            GUI.TextAlign = AlignMode.TopLeft;
        });
    }
}