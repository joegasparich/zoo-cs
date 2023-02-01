using Raylib_cs;

namespace Zoo.ui; 

public class Dialog_NewZoo : Dialog {
    // Constants
    private const           int       PanelWidth  = 300;
    private const           int       PanelHeight = 300;
    private static readonly Rectangle Dimensions  = new ((Game.ScreenWidth - PanelWidth) / 2, (Game.ScreenHeight - PanelHeight) / 2, PanelWidth, PanelHeight);
    
    // State
    private string width = "100";
    private string height = "100";

    public Dialog_NewZoo() : base(Dimensions) {
        ShowCloseX = true;
    }

    public override void DoWindowContents() {
        base.DoWindowContents();

        var curY = GUI.GapSmall;
        GUI.Header(new Rectangle(0, curY, GetWidth(), 20), "New Zoo");
        curY += 20 + GUI.GapSmall;

        GUI.TextAlign = AlignMode.MiddleCenter;
        GUI.Label(new Rectangle(GetWidth() / 2 - 100 - GUI.GapSmall, curY, 100, 10), "Width");
        GUI.Label(new Rectangle(GetWidth() / 2 + GUI.GapSmall,       curY, 100, 10), "Height");
        curY += 10;
        
        GUI.TextInput(new Rectangle(GetWidth() / 2 - 100 - GUI.GapSmall, curY, 100, 20), ref width,  "zooWidth");
        GUI.TextInput(new Rectangle(GetWidth() / 2 + GUI.GapSmall,       curY, 100, 20), ref height, "zooHeight");
        curY += 20 + GUI.GapSmall;

        if (GUI.ButtonText(new Rectangle((GetWidth() - 120) / 2, curY, 120, 20), "Create")) {
            if (int.TryParse(width, out var w) && int.TryParse(height, out var h)) {
                Find.SaveManager.NewGame(w, h);
                Close();
            }
            
        }
        
        GUI.Reset();
    }
}