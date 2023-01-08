using System.Text.Json.Nodes;
using Raylib_cs;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Elevation : Tool {
    // Constants
    private const float DefaultRadius      = 0.65f;
    private const float RadiusStep         = 0.1f;
    private const float RadiusMin          = 0.45f;
    private const float RadiusMax          = 2.05f;
    private const int   PlaceIntervalTicks = 5;

    // Virtual Properties
    public override string   Name => "Elevation Tool";
    public override ToolType Type => ToolType.Elevation;
    
    // State
    private Elevation?                     currentElevation;
    private bool                           isDragging;
    private Dictionary<IntVec2, Elevation> oldElevationData = new();
    private float                          radius           = DefaultRadius;

    public Tool_Elevation(ToolManager tm) : base(tm) {}

    public override void Set() {
        Ghost.Type    = GhostType.Circle;
        Ghost.Radius  = radius;
        Ghost.Elevate = true;
    }

    public override void OnInput(InputEvent evt) {
        if (!currentElevation.HasValue) return;
        
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
        
        if (evt.inputDown == InputType.DecreaseBrushSize) {
            radius       = Math.Max(RadiusMin, radius - RadiusStep);
            Ghost.Radius = radius;
            evt.Consume();
        }
        if (evt.inputDown == InputType.IncreaseBrushSize) {
            radius       = Math.Min(RadiusMax, radius + RadiusStep);
            Ghost.Radius = radius;
            evt.Consume();
        }
    }

    public override void Update() {
        if (!currentElevation.HasValue) return;
        if (!isDragging || Game.Ticks % PlaceIntervalTicks != 0) return;
        
        var oldPoints = Find.World.Elevation.SetElevationInCircle(Find.Input.GetMouseWorldPos(), radius, currentElevation.Value);

        foreach (var (p, e) in oldPoints) {
            oldElevationData.TryAdd(p, e);
        }
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immElevationPanel", new Rectangle(10, 60, 100, 100), inRect => {
            GUI.TextAlign = AlignMode.MiddleCenter;

            if (GUI.ButtonText(new Rectangle(10, 10, 80, 20), "Water", selected: currentElevation == Elevation.Water)) SetElevation(Elevation.Water);
            if (GUI.ButtonText(new Rectangle(10, 40, 80, 20), "Flat",  selected: currentElevation == Elevation.Flat))  SetElevation(Elevation.Flat);
            if (GUI.ButtonText(new Rectangle(10, 70, 80, 20), "Hill",  selected: currentElevation == Elevation.Hill))  SetElevation(Elevation.Hill);

            GUI.TextAlign = AlignMode.TopLeft;
        });
    }
    
    private void SetElevation(Elevation? elevation) {
        currentElevation = elevation;

        if (currentElevation != null) {
            Ghost.Visible = true;
        } else {
            Ghost.Visible = false;
        }
    }
}