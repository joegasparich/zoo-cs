using Raylib_cs;
using Zoo.tools;

namespace Zoo.ui; 

public class Toolbar : Window {
    private const int ButtonWidth = 70;
    private const int ButtonHeight = 25;

    private ToolManager toolManager;

    public Toolbar(ToolManager toolManager) : base(new Rectangle(0, 0, Game.ScreenWidth, 45)) {
        this.toolManager = toolManager;
    }

    public override void DoWindowContents() {
        base.DoWindowContents();

        var curX = GUI.GapSmall;
        GUI.TextAlign = AlignMode.MiddleCenter;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Biome");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Elevation");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Wall");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Door");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Path");
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Object")) {
            toolManager.SetTool(ToolType.TileObject);
        }
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Delete");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "New");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Save");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Load");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Debug");
        curX += ButtonWidth + GUI.GapSmall;

        GUI.TextAlign = AlignMode.TopLeft;
    }
}