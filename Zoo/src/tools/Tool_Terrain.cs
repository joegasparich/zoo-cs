using System.Text.Json.Nodes;
using Raylib_cs;
using Zoo.defs;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Terrain : Tool {
    // Constants
    private const float DefaultRadius      = 0.65f;
    private const float RadiusStep         = 0.1f;
    private const float RadiusMin          = 0.45f;
    private const float RadiusMax          = 2.05f;
    private const int   PlaceIntervalTicks = 5;
    private const int   ButtonSize         = 30;

    // Virtual Properties
    public override string   Name => "Terrain Tool";
    public override ToolType Type => ToolType.Terrain;
    
    // State
    private TerrainDef?                      currentTerrain;
    private bool                             isDragging;
    private Dictionary<string, string[][][]> oldChunkData = new();
    private float                            radius       = DefaultRadius;

    public Tool_Terrain(ToolManager tm) : base(tm) {}

    public override void Set() {
        Ghost.Type    = GhostType.Circle;
        Ghost.Radius  = radius;
        Ghost.Elevate = true;
    }

    public override void OnInput(InputEvent evt) {
        if (currentTerrain == null) return;
        
        // Only listen to down and up events so that we can't start dragging from UI
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = true;
            oldChunkData.Clear();
            
            evt.Consume();
        }
        if (evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            if (!isDragging) return;
            isDragging = false;

            toolManager.PushAction(new ToolAction() {
                Name = "Terrain brush",
                // Copy here so we don't lose the data when clearing
                Data = oldChunkData.ToDictionary(entry => entry.Key, entry => entry.Value),
                Undo = data => {
                    foreach (var (key, value) in (Dictionary<string, string[][][]>)data) {
                        var pos   = IntVec2.FromString(key);
                        var chunk = Find.World.Terrain.GetChunk(pos.X, pos.Y);
                        chunk.Load(value);
                    }
                }
            });
            
            evt.Consume();
        }

        if (evt.inputDown == InputType.DecreaseBrushSize) {
            radius = Math.Max(RadiusMin, radius - RadiusStep);
            Ghost.Radius = radius;
            evt.Consume();
        }
        if (evt.inputDown == InputType.IncreaseBrushSize) {
            radius = Math.Min(RadiusMax, radius + RadiusStep);
            Ghost.Radius = radius;
            evt.Consume();
        }
    }

    public override void Update() {
        if (currentTerrain == null) return;
        if (!isDragging || Game.Ticks % PlaceIntervalTicks != 0) return;

        var pos = Find.Input.GetMouseWorldPos() * TerrainGrid.TerrainScale;
        
        // Save backups for undo
        foreach (var chunk in Find.World.Terrain.GetChunksInRadius(pos, radius * TerrainGrid.TerrainScale)) {
            var key = new IntVec2(chunk.X, chunk.Y).ToString();
            if (!oldChunkData.ContainsKey(key))
                oldChunkData.Add(key, chunk.Save());
        }
        
        // Set terrain in a circle
        Find.World.Terrain.SetTerrainInRadius(pos, radius * TerrainGrid.TerrainScale, currentTerrain);
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immTerrainPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (TerrainDef terrain in Find.AssetManager.GetAllDefs<TerrainDef>()) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);

                if (GUI.ButtonEmpty(buttonRect, terrain.Colour, currentTerrain == terrain))
                    SetTerrain(terrain);
                
                i++;
            }
        });
    }

    private void SetTerrain(TerrainDef? terrain) {
        currentTerrain  = terrain;

        if (currentTerrain != null) {
            Ghost.Visible = true;
        } else {
            Ghost.Visible = false;
        }
    }
}