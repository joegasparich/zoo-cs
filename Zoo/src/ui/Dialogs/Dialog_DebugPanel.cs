using Raylib_cs;

namespace Zoo.ui; 

public class Dialog_DebugPanel : Dialog {
    // Constants
    private const           int       ButtonWidth   = 100;
    private const           int       ButtonHeight  = 25;
    private const           int       PanelWidth    = ButtonWidth + GUI.GapSmall * 2;
    private static readonly Rectangle Dimensions    = new Rectangle(Game.ScreenWidth - PanelWidth, 70, PanelWidth, 200);
    private static readonly Color     EnabledColor  = new Color(26, 110, 20, 255);
    private static readonly Color     DisabledColor = new Color(130, 26, 26, 255);

    public Dialog_DebugPanel() : base(Dimensions) {
        Title     = "Debug";
        Draggable = true;
    }

    public override void DoWindowContents() {
        base.DoWindowContents();

        var curY = DefaultWindowHeaderHeight + 10;
        
        GUI.TextColour = Color.WHITE;
        GUI.TextAlign  = AlignMode.MiddleCenter;
        
        if (GUI.ButtonText(new Rectangle(10, curY, ButtonWidth, ButtonHeight), "Cell Grid", DebugSettings.CellGrid ? EnabledColor : DisabledColor))
            DebugSettings.CellGrid =  !DebugSettings.CellGrid;
        curY += ButtonHeight + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(10, curY, ButtonWidth, ButtonHeight), "Biome Chunks", DebugSettings.BiomeChunks ? EnabledColor : DisabledColor))
            DebugSettings.BiomeChunks =  !DebugSettings.BiomeChunks;
        curY += ButtonHeight + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(10, curY, ButtonWidth, ButtonHeight), "Elevation Grid", DebugSettings.ElevationGrid ? EnabledColor : DisabledColor))
            DebugSettings.ElevationGrid =  !DebugSettings.ElevationGrid;
        curY += ButtonHeight + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(10, curY, ButtonWidth, ButtonHeight), "Area Grid", DebugSettings.AreaGrid ? EnabledColor : DisabledColor))
            DebugSettings.AreaGrid =  !DebugSettings.AreaGrid;
        curY += ButtonHeight + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(10, curY, ButtonWidth, ButtonHeight), "Pathfinding Grid", DebugSettings.PathfindingGrid ? EnabledColor : DisabledColor))
            DebugSettings.PathfindingGrid =  !DebugSettings.PathfindingGrid;
        curY += ButtonHeight + GUI.GapSmall;

        GUI.TextColour = Color.BLACK;
        GUI.TextAlign  = AlignMode.TopLeft;
    }
}