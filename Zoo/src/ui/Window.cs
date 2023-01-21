using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.ui;

public class Window {
    // Config
    public  string            Id      { get; }
    public  Rectangle         AbsRect { get; protected set; }
    public  bool              Immediate = false;
    private Action<Rectangle> OnUI;
    public  bool              DismissOnRightClick { get; protected set; } = false;

    public Window(Rectangle rect) {
        Id = Guid.NewGuid().ToString();
        AbsRect = rect;
    }

    public Window(string id, Rectangle rect, Action<Rectangle> onUi) {
        Id = id;
        AbsRect = rect;
        OnUI = onUi;
    }

    public virtual void DoWindowContents() {
        if (OnUI != null) OnUI(GetRect());
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