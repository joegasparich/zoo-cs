using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.ui;

public class Window {
    
    // Config
    public  string            Id { get; }
    public  bool              Immediate = false;
    public  bool              DismissOnRightClick { get; protected set; } = false;
    public  Rectangle         AbsRect;
    private Action<Rectangle> onUI;
    
    // Properties

    public bool IsHovered => Find.UI.IsMouseOverRect(GetRect());

    public Window(Rectangle rect) {
        Id = Guid.NewGuid().ToString();
        AbsRect = rect;
    }

    public Window(string id, Rectangle rect, Action<Rectangle> onUi) {
        Id = id;
        AbsRect = rect;
        onUI = onUi;
    }

    public virtual void DoWindowContents() {
        if (onUI != null) onUI(GetRect());
    }

    public virtual void OnInput(InputEvent evt) {
        DoWindowContents();
    }

    public virtual void OnScreenResized(int width, int height) {
        
    }
    
    public virtual void OnClose() {}

    public virtual void Close() {
        Find.UI.CloseWindow(Id);
    }
    
    public float GetWidth() {
        return Math.Min(AbsRect.width, Game.ScreenWidth - AbsRect.x);
    }
    
    public float GetHeight() {
        return Math.Min(AbsRect.height, Game.ScreenHeight - AbsRect.y);
    }

    public Rectangle GetRect() {
        return new Rectangle(0, 0, GetWidth(), GetHeight());
    }
}