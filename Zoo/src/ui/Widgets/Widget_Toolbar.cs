using Raylib_cs;
using Zoo.tools;

namespace Zoo.ui; 

public class Widget_Toolbar : Window {
    // Constants
    private const int ButtonWidth = 70;
    private const int ButtonHeight = 25;
    
    // References
    private ToolManager toolManager;

    // State
    private string debugDialogId;
    private string saveDialogId;

    public Widget_Toolbar(ToolManager toolManager) : base(new Rectangle(0, 0, Game.ScreenWidth, 45)) {
        this.toolManager = toolManager;
    }

    public override void DoWindowContents() {
        base.DoWindowContents();

        var curX = GUI.GapSmall;
        GUI.TextAlign = AlignMode.MiddleCenter;
        
        // TODO: Do tool buttons with a loop?
        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Biome")) {
            toolManager.SetTool(ToolType.Biome);
        }
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Elevation")) {
            toolManager.SetTool(ToolType.Elevation);
        }
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Wall")) {
            toolManager.SetTool(ToolType.Wall);
        }
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Door")) {
            toolManager.SetTool(ToolType.Door);
        }
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Path")) {
            toolManager.SetTool(ToolType.FootPath);
        }
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Object")) {
            toolManager.SetTool(ToolType.TileObject);
        }
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Delete")) {
            toolManager.SetTool(ToolType.Delete);
        }
        curX += ButtonWidth + GUI.GapSmall;
        
        // Saves

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Menu")) {
            Find.SceneManager.LoadScene(new Scene_Menu());
        }
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Save")) {
            if (!Find.UI.IsWindowOpen(saveDialogId)) {
                saveDialogId = Find.UI.PushWindow(new Dialog_Save());
            } else {
                Find.UI.CloseWindow(saveDialogId);
            }
        }
        curX += ButtonWidth + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle(curX, 10, ButtonWidth, ButtonHeight), "Debug")) {
            if (!Find.UI.IsWindowOpen(debugDialogId)) {
                debugDialogId = Find.UI.PushWindow(new Dialog_DebugPanel());
            } else {
                Find.UI.CloseWindow(debugDialogId);
            }
        }
        curX += ButtonWidth + GUI.GapSmall;

        GUI.Reset();
    }
}