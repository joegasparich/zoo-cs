using Raylib_cs;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Biome : Tool {
    private const float DefaultRadius      = 0.65f;
    private const int   PlaceIntervalTicks = 5;
    
    private Biome currentBiome;
    private bool  isDragging;
    
    public override string   Name => "Biome Tool";
    public override ToolType Type => ToolType.Biome;

    public Tool_Biome(ToolManager tm) : base(tm) {}

    public override void Set() {
        currentBiome = Biome.Sand;

        Ghost.Type    = GhostType.Circle;
        Ghost.Radius  = DefaultRadius;
        Ghost.Elevate = true;
    }

    public override void Update() {
        if (!isDragging || Game.Ticks % PlaceIntervalTicks != 0) return;
        
        Find.World.Biomes.SetBiomeInRadius(Find.Input.GetMouseWorldPos() * BiomeGrid.BiomeScale, Ghost.Radius * BiomeGrid.BiomeScale, currentBiome);
    }

    public override void OnInput(InputEvent evt) {
        // Only listen to down and up events so that we can't start dragging from UI
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = true;
            evt.Consume();
        }
        if (evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            if (!isDragging) return;

            isDragging = false;
            evt.Consume();
        }
    }
}