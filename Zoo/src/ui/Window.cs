using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.ui; 

public class Window {
    // Constants
    private const string WindowNPatchPath       = "assets/textures/ui/window.png";
    private const string WindowHeaderNPatchPath = "assets/textures/ui/window_header.png";
    public const  int    WindowHeaderHeight     = 18;

    // Config
    public  string            Id        { get; }
    public  Rectangle         AbsRect   { get; private set; }
    public  bool              Draggable { get; protected set; }
    public  string            Title     { get; protected set; }
    public  bool              Immediate = false;
    private Action<Rectangle> OnUI;
    private bool              doBackground = true;

    // State
    protected bool    headerHovered;
    protected bool    isDragging;
    private   Vector2 dragPos = Vector2.Zero;

    public Window(Rectangle rect) {
        Id = Guid.NewGuid().ToString();
        AbsRect = rect;
    }

    public Window(string id, Rectangle rect, Action<Rectangle> onUi, bool doBackground) {
        Id = id;
        AbsRect = rect;
        OnUI = onUi;
        this.doBackground = doBackground;
    }

    public virtual void DoWindowContents() {
        if (isDragging) {
            var newPos = Find.Input.GetMousePos() - dragPos;
            AbsRect = AbsRect with { x = newPos.X, y = newPos.Y };
        }

        if (doBackground) {
            var backgroundTexture = !Title.NullOrEmpty() || Draggable ? WindowHeaderNPatchPath : WindowNPatchPath;
            GUI.DrawTextureNPatch(GetRect(), Find.AssetManager.GetTexture(backgroundTexture), 20);
        }

        var headerRect = new Rectangle(0, 0, GetWidth(), WindowHeaderHeight);
        if (!Title.NullOrEmpty()) {
            GUI.TextAlign = AlignMode.MiddleCenter;
            GUI.DrawText(headerRect, Title);
            GUI.TextAlign = AlignMode.TopLeft;
        }

        headerHovered = false;
        if (GUI.HoverableArea(headerRect)) {
            headerHovered = true;
        
            if (Draggable)
                Find.UI.SetCursor(MouseCursor.MOUSE_CURSOR_POINTING_HAND);
        }

        if (OnUI != null) OnUI(GetRect());
    }

    public virtual void OnInput(InputEvent evt) {
        DoWindowContents();
        
        // Dragging
        if (Draggable) {
            if (headerHovered && evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
                dragPos    = evt.mousePos - new Vector2(AbsRect.x, AbsRect.y);
                isDragging = true;
                Find.UI.BringWindowToFront(Id);
            }
            if (isDragging && evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
                isDragging = false;
            }
        }
    }

    public virtual void Close() {
        Find.UI.CloseWindow(Id);
    }
    
    public float GetWidth() {
        return AbsRect.width;
    }
    
    public float GetHeight() {
        return AbsRect.height;
    }

    public Rectangle GetRect() {
        return new Rectangle(0, 0, GetWidth(), GetHeight());
    }
}