using Raylib_cs;

namespace Zoo.ui; 

public class Dialog_Settings : Dialog {
    // Constants
    private const           int       PanelWidth  = 300;
    private const           int       PanelHeight = 300;
    private static readonly Rectangle Dimensions  = new ((Game.ScreenWidth - PanelWidth) / 2, (Game.ScreenHeight - PanelHeight) / 2, PanelWidth, PanelHeight);
    
    public Dialog_Settings(Rectangle rect) : base(rect) { }
}