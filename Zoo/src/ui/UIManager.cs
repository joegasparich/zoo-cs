using Raylib_cs;
using Zoo.util;

namespace Zoo.ui;

public enum UIEvent {
    None,
    Draw,
    Input
}

public class UIManager {
    // Constants
    private const string DefaultFontPath = "assets/fonts/Pixeltype.ttf";
    public const  int    DefaultFontSize = 10;
    
    // Resources
    public Font DefaultFont;
    
    // State
    private List<Window>               windowStack      = new();
    private Dictionary<string, Window> openWindowMap    = new();
    private List<Window>               windowsToOpen    = new();
    private HashSet<string>            windowsToClose   = new();
    private HashSet<string>            immediateWindows = new();

    private MouseCursor cursor;
    private string      hoveredWindowId;
    private string      currentWindowId;
    private string?     currentFocusId = null;
    
    // Properties
    public UIEvent   CurrentEvent      { get; private set; }
    public Rectangle CurrentDrawBounds { get; private set; }
    public float     UIScale           => Game.Settings.UIScale;

    public UIManager() {
        CurrentEvent = UIEvent.Draw;
        CurrentDrawBounds = new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight); // TODO: Dynamic
    }

    public void Init() {
        Debug.Log("Initializing UI");

        // DefaultFont = Raylib.LoadFont(DefaultFontPath);
        DefaultFont = Raylib.LoadFontEx(DefaultFontPath, DefaultFontSize, null, 0);
        Raylib.SetTextureFilter(DefaultFont.texture, TextureFilter.TEXTURE_FILTER_POINT);
    }

    public void OnInput(InputEvent evt) {
        CurrentEvent = UIEvent.Input;
        
        // Lose focus
        if (evt.keyDown is KeyboardKey.KEY_ESCAPE || evt.mouseDown is MouseButton.MOUSE_LEFT_BUTTON) {
            currentFocusId = null;
        }
        
        Game.OnGUI();
        
        // Clear closed immediate windows
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            if (windowStack[i].Immediate && !immediateWindows.Contains(windowStack[i].Id)) {
                openWindowMap.Remove(windowStack[i].Id);
                windowStack.RemoveAt(i);
            }
        }
        immediateWindows.Clear();
        
        // Loop backwards so that top windows consume events first
        for (int i = windowStack.Count - 1; i >= 0; i--) {
            var window = windowStack[i];

            var mouseOver = JMath.PointInRect(window.AbsRect, evt.mousePos);
            if (hoveredWindowId.NullOrEmpty() && mouseOver) {
                hoveredWindowId = window.Id;
            }
            
            CurrentDrawBounds = window.AbsRect;
            currentWindowId = window.Id;
            window.OnInput(evt);
            
            // Consume event if it's a mouse button down on the window
            if (JMath.PointInRect(CurrentDrawBounds, evt.mousePos) && evt.mouseDown.HasValue) {
                evt.Consume();
                break;
            }
        }
        CurrentDrawBounds = new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight);
        CurrentEvent      = UIEvent.None;
    }

    public void PostInput(InputEvent evt) {
        if (!evt.consumed && evt.mouseDown == MouseButton.MOUSE_BUTTON_RIGHT) {
            foreach (var window in windowStack) {
                if (window.DismissOnRightClick) {
                    CloseWindow(window.Id);
                    evt.Consume();
                    break;
                }
            }
        }
    }

    public void PreRender() {
        Raylib.SetMouseCursor(cursor);
        SetCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
    }

    public void Render() {
        CurrentEvent = UIEvent.Draw;
        Game.OnGUI();
        
        // Clear closed immediate windows
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            if (windowStack[i].Immediate && !immediateWindows.Contains(windowStack[i].Id)) {
                openWindowMap.Remove(windowStack[i].Id);
                windowStack.RemoveAt(i);
            }
        }
        immediateWindows.Clear();
        
        // First loop through in reverse to find the hovered window
        for (int i = windowStack.Count - 1; i >= 0; i--) {
            var window = windowStack[i];
            if (JMath.PointInRect(window.AbsRect.Multiply(UIScale), Find.Input.GetMousePos())) {
                hoveredWindowId = window.Id;
                break;
            }
        }
        
        // Render windows
        foreach (var window in windowStack) {
            CurrentDrawBounds = window.AbsRect;
            currentWindowId   = window.Id;
            window.DoWindowContents();
        }
        
        CurrentDrawBounds = new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight);
        CurrentEvent      = UIEvent.None;
    }

    public void PostRender() {
        windowStack.RemoveAll(window => windowsToClose.Contains(window.Id));
        windowStack.AddRange(windowsToOpen);
        
        windowsToOpen.Clear();
        windowsToClose.Clear();
    }

    public void OnScreenResized() {
        foreach (var window in windowStack) {
            window.OnScreenResized(Game.ScreenWidth, Game.ScreenHeight);
        }
    }

    public string PushWindow(Window window) {
        windowsToOpen.Add(window);
        openWindowMap.Add(window.Id, window);
        return window.Id;
    }

    public void CloseWindow(string id) {
        if (!openWindowMap.ContainsKey(id)) return;
        
        openWindowMap[id].OnClose();
        
        openWindowMap.Remove(id);
        windowsToClose.Add(id);
    }
    
    public void CloseAllWindows() {
        foreach (var window in windowStack) {
            window.OnClose();
        }
        windowStack.Clear();
        openWindowMap.Clear();
    }

    public void BringWindowToFront(string id) {
        var index = windowStack.FindIndex(window => window.Id == id);
        windowStack.MoveItemAtIndexToBack(index);
    }
    
    public Window? GetWindow(string id) {
        return openWindowMap.TryGetValue(id, out var window) ? window : null;
    }
    
    public bool IsWindowOpen(string id) {
        if (id.NullOrEmpty()) return false;
        return openWindowMap.ContainsKey(id);
    }

    public void DoImmediateWindow(string id, Rectangle initialRect, Action<Rectangle> onUI, bool dialog = true) {
        if (CurrentEvent == UIEvent.None) {
            Debug.Warn("Immediate windows must be called in OnGUI");
            return;
        }
        
        var found = windowStack.Any(window => window.Id == id);

        if (!found) {
            Window window;
            if (dialog)
                window = new Dialog(id, initialRect, onUI);
            else
                window = new Window(id, initialRect, onUI);
            window.Immediate = true;
            PushWindow(window);
        }

        immediateWindows.Add(id);
    }

    public void SetCursor(MouseCursor c) {
        cursor = c;
    }
    
    public void SetFocus(string focusId) {
        currentFocusId = focusId;
    }
    
    public bool IsFocused(string focusId) {
        return currentFocusId == focusId;
    }

    public Rectangle GetAbsRect(Rectangle rect) {
        return new Rectangle(rect.x + CurrentDrawBounds.x, rect.y + CurrentDrawBounds.y, rect.width, rect.height).Multiply(UIScale);
    }

    public bool IsMouseOverRect(Rectangle rect) {
        if (currentWindowId != hoveredWindowId)
            return false;
        
        return JMath.PointInRect(GetAbsRect(rect), Find.Input.GetMousePos());
    }
}