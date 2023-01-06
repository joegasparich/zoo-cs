using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.ui; 

public class Dialog : Window {
    // Constants
    private const string WindowNPatchPath       = "assets/textures/ui/window.png";
    // private const string WindowHeaderNPatchPath = "assets/textures/ui/window_header.png";
    public const  int    WindowHeaderHeight     = 18;
    
    // Config
    public    bool   Draggable { get; protected set; }
    public    string Title     { get; protected set; }
    private   bool   doBackground     = true;
    protected Color  backgroundColour = Color.WHITE.WithAlpha(0.7f);
    
    // State
    protected bool    headerHovered;
    protected bool    isDragging;
    private   Vector2 dragPos = Vector2.Zero;

    public Dialog(Rectangle rect) : base(rect) {}
    public Dialog(string id, Rectangle rect, Action<Rectangle> onUi, bool doBackground = true) : base(id, rect, onUi) {
        this.doBackground = doBackground;
    }

    public override void DoWindowContents() {
        if (isDragging) {
            var newPos = Find.Input.GetMousePos() - dragPos;
            AbsRect = AbsRect with { x = newPos.X, y = newPos.Y };
        }
        
        if (doBackground) {
            // var backgroundTexture = !Title.NullOrEmpty() || Draggable ? WindowHeaderNPatchPath : WindowNPatchPath;
            GUI.DrawTextureNPatch(GetRect(), Find.AssetManager.GetTexture(WindowNPatchPath), 20, backgroundColour);
        }
        
        var headerRect = new Rectangle(0, 0, GetWidth(), WindowHeaderHeight);
        if (!Title.NullOrEmpty()) {
            GUI.TextAlign = AlignMode.MiddleCenter;
            GUI.Label(headerRect, Title);
            GUI.TextAlign = AlignMode.TopLeft;
        }
        
        headerHovered = false;
        if (GUI.HoverableArea(headerRect)) {
            headerHovered = true;
        
            if (Draggable)
                Find.UI.SetCursor(MouseCursor.MOUSE_CURSOR_POINTING_HAND);
        }
        
        base.DoWindowContents();
    }

    public override void OnInput(InputEvent evt) {
        base.OnInput(evt);

        if (evt.consumed) return;
        
        // Dragging
        if (Draggable) {
            if (headerHovered && evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
                dragPos    = evt.mousePos - new Vector2(AbsRect.x, AbsRect.y);
                isDragging = true;
                Find.UI.BringWindowToFront(Id);
                evt.Consume();
            }
            if (isDragging && evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
                isDragging = false;
                evt.Consume();
            }
        }
    }
}