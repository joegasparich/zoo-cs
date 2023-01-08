using System.Text.Json.Nodes;
using Raylib_cs;
using Zoo.defs;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Biome : Tool {
    // Constants
    private const float DefaultRadius      = 0.65f;
    private const float RadiusStep         = 0.1f;
    private const float RadiusMin          = 0.45f;
    private const float RadiusMax          = 2.05f;
    private const int   PlaceIntervalTicks = 5;
    private const int   ButtonSize         = 30;

    // Virtual Properties
    public override string   Name => "Biome Tool";
    public override ToolType Type => ToolType.Biome;
    
    // State
    private BiomeDef?                           currentBiome;
    private bool                             isDragging;
    private Dictionary<string, string[][][]> oldChunkData = new();
    private float                            radius       = DefaultRadius;

    public Tool_Biome(ToolManager tm) : base(tm) {}

    public override void Set() {
        Ghost.Type    = GhostType.Circle;
        Ghost.Radius  = radius;
        Ghost.Elevate = true;
    }

    public override void OnInput(InputEvent evt) {
        if (currentBiome == null) return;
        
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
                Name = "Biome brush",
                // Copy here so we don't lose the data when clearing
                Data = oldChunkData.ToDictionary(entry => entry.Key, entry => entry.Value),
                Undo = data => {
                    foreach (var (key, value) in (Dictionary<string, string[][][]>)data) {
                        var pos   = IntVec2.FromString(key);
                        var chunk = Find.World.Biomes.GetChunk(pos.X, pos.Y);
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
        if (currentBiome == null) return;
        if (!isDragging || Game.Ticks % PlaceIntervalTicks != 0) return;

        var pos = Find.Input.GetMouseWorldPos() * BiomeGrid.BiomeScale;
        
        // Save backups for undo
        foreach (var chunk in Find.World.Biomes.GetChunksInRadius(pos, radius * BiomeGrid.BiomeScale)) {
            var key = new IntVec2(chunk.X, chunk.Y).ToString();
            if (!oldChunkData.ContainsKey(key))
                oldChunkData.Add(key, chunk.Save());
        }
        
        // Set biome in a circle
        Find.World.Biomes.SetBiomeInRadius(pos, radius * BiomeGrid.BiomeScale, currentBiome);
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immBiomePanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (BiomeDef biome in Find.AssetManager.GetAll<BiomeDef>()) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);

                if (GUI.ButtonEmpty(buttonRect, biome.Colour, currentBiome == biome))
                    SetBiome(biome);
                
                i++;
            }
        });
    }

    private void SetBiome(BiomeDef? biome) {
        currentBiome  = biome;

        if (currentBiome != null) {
            Ghost.Visible = true;
        } else {
            Ghost.Visible = false;
        }
    }
}