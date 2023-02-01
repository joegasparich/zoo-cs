using Raylib_cs;

namespace Zoo.ui; 

public class Dialog_Settings : Dialog {
    // Constants
    private const           int       PanelWidth   = 300;
    private const           int       PanelHeight  = 300;
    private static readonly Rectangle Dimensions   = new ((Game.ScreenWidth - PanelWidth) / 2, (Game.ScreenHeight - PanelHeight) / 2, PanelWidth, PanelHeight);
    private const           int       ButtonWidth  = 200;
    private const           int       ButtonHeight = 30;
    
    // State
    private bool uiScaleDropdownOpen = false;

    public Dialog_Settings() : base(Dimensions) {
        ShowCloseX = true;
    }

    public override void DoWindowContents() {
        base.DoWindowContents();
        
        var curY = GUI.GapSmall;
        GUI.Header(new Rectangle(0, curY, GetWidth(), 20), "Settings");
        curY += 20 + GUI.GapSmall;

        using (new TextBlock(AlignMode.MiddleCenter)) {
            GUI.DropDown(new Rectangle((GetWidth() - ButtonWidth) / 2, curY, ButtonWidth, ButtonHeight), "UI Scale", ref uiScaleDropdownOpen, new() {
                ("1x", () => Game.Settings.UIScale = 1f),
                ("1.25x", () => Game.Settings.UIScale = 1.25f),
                ("1.5x", () => Game.Settings.UIScale = 1.5f),
                ("1.75x", () => Game.Settings.UIScale = 1.75f),
                ("2x", () => Game.Settings.UIScale = 2f)
            });
        }
    }

    public override void OnClose() {
        Game.SaveSettings();
        
        base.OnClose();
    }
}