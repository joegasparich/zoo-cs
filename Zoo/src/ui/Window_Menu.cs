using System.Numerics;
using Raylib_cs;

namespace Zoo.ui; 

public class Window_Menu : Window {
    // Constants
    private static readonly Rectangle Dimensions = new (0, 0, Game.ScreenWidth, Game.ScreenHeight);
    private const int ButtonWidth = 200;
    private const int ButtonHeight = 30;
    
    // State
    private bool showSaves;

    public Window_Menu() : base(Dimensions) {}

    public override void DoWindowContents() {
        base.DoWindowContents();
        
        GUI.DrawRect(GetRect(), Color.DARKBLUE);

        var curY = 200;

        GUI.TextAlign = AlignMode.MiddleCenter;

        if (GUI.ButtonText(new Rectangle((GetWidth() - ButtonWidth) / 2, curY, ButtonWidth, ButtonHeight), "New Game")) {
            // TODO: ensure multiple of these can't be created
            // Maybe add a way for a dialog to consume all events 
            Find.UI.PushWindow(new Dialog_NewZoo());
        }
        curY += ButtonHeight + GUI.GapSmall;
        if (GUI.ButtonText(new Rectangle((GetWidth() - ButtonWidth) / 2, curY, ButtonWidth, ButtonHeight), "Load")) {
            showSaves = true;
        }

        if (showSaves) {
            DrawSaves(new Vector2((GetWidth() + ButtonWidth) / 2 + GUI.GapSmall, curY));
        }
        curY += ButtonHeight + GUI.GapSmall;
        
        GUI.TextAlign = AlignMode.TopLeft;
    }
    
    private void DrawSaves(Vector2 pos) {
        var curY = pos.Y;
        
        var saveFiles = Find.SaveManager.GetSaveFiles();
        foreach (var saveFile in saveFiles) {
            if (GUI.ButtonText(new Rectangle(pos.X, curY, ButtonWidth, ButtonHeight), saveFile.Name)) {
                Find.SaveManager.LoadGame(saveFile.Path);
            }
            curY += ButtonHeight + GUI.GapSmall;
        }
    }
}