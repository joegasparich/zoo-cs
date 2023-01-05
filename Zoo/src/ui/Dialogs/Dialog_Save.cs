using Raylib_cs;
using Zoo.util;

namespace Zoo.ui; 

public class Dialog_Save : Dialog {
    // Constants
    private const           int       PanelWidth = 300;
    private static readonly Rectangle Dimensions = new ((Game.ScreenWidth - PanelWidth) / 2, 50, PanelWidth, 130);
    
    // State
    private string saveName = "";
    
    public Dialog_Save() : base(Dimensions) {}

    public override void DoWindowContents() {
        base.DoWindowContents();

        var curY = GUI.GapSmall;
        GUI.Header(new Rectangle(0, curY, GetWidth(), 20), "Save Game");
        curY += 20 + GUI.GapSmall;

        GUI.TextAlign = AlignMode.MiddleLeft;
        GUI.TextInput(new Rectangle(GUI.GapSmall, curY, GetWidth() - GUI.GapSmall * 2, 30), ref saveName, "saveName");
        curY += 30 + GUI.GapSmall;

        GUI.TextAlign = AlignMode.MiddleCenter;
        if (GUI.ButtonText(new Rectangle(PanelWidth / 2 - 100, curY, 200, 30), "Save")) {
            if (saveName.Length > 0) {
                Find.SaveManager.SaveGame(saveName);
                Close();
            }
        }
        GUI.Reset();
    }
}