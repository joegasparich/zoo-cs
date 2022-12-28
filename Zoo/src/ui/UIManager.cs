﻿using Raylib_cs;
using Zoo.util;

namespace Zoo.ui;

public enum UIEvent {
    None,
    Draw,
    Input
}

public class UIManager {
    public UIEvent   CurrentEvent      { get; private set; }
    public Rectangle CurrentDrawBounds { get; private set; }

    private List<Window>               windowStack      = new();
    private Dictionary<string, Window> openWindowMap    = new();
    private List<Window>               windowsToOpen    = new();
    private HashSet<string>            windowsToClose   = new();
    private HashSet<string>            immediateWindows = new();

    private MouseCursor cursor;
    private string      hoveredWindowId;
    private string      currentWindowId;

    public UIManager() {
        CurrentEvent = UIEvent.Draw;
        CurrentDrawBounds = new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight); // TODO: Dynamic
    }

    public void Init() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Initializing UI");
    }

    public void OnInput(InputEvent evt) {
        CurrentEvent = UIEvent.Input;
        Game.OnGUI();
        
        // Clear closed immediate windows
        windowStack.RemoveAll(window => window.Immediate && !immediateWindows.Contains(window.Id));
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

    public void PreRender() {
        Raylib.SetMouseCursor(cursor);
        SetCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
    }

    public void Render() {
        CurrentEvent = UIEvent.Draw;
        Game.OnGUI();
        
        // Clear closed immediate windows
        windowStack.RemoveAll(window => window.Immediate && !immediateWindows.Contains(window.Id));
        immediateWindows.Clear();
        
        // First loop through in reverse to find the hovered window
        for (int i = windowStack.Count - 1; i >= 0; i--) {
            var window = windowStack[i];
            if (JMath.PointInRect(window.AbsRect, Find.Input.GetMousePosition())) {
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

    public string PushWindow(Window window) {
        windowsToOpen.Add(window);
        openWindowMap.Add(window.Id, window);
        return window.Id;
    }

    public void CloseWindow(string id) {
        openWindowMap.Remove(id);
        windowsToClose.Add(id);
    }

    public void BringWindowToFront(string id) {
        var index = windowStack.FindIndex(window => window.Id == id);
        windowStack.MoveItemAtIndexToFront(index);
    }
    
    public Window? GetWindow(string id) {
        return openWindowMap.TryGetValue(id, out var window) ? window : null;
    }
    
    public bool IsWindowOpen(string id) {
        return openWindowMap.ContainsKey(id);
    }

    public void DoImmediateWindow(string id, Rectangle initialRect, Action<Rectangle> onUI, bool doBackground) {
        if (CurrentEvent == UIEvent.None) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Immediate windows must be called in OnGUI");
            return;
        }
        
        var found = windowStack.Any(window => window.Id == id);

        if (!found) {
            var window = new Window(id, initialRect, onUI, doBackground);
            window.Immediate = true;
            PushWindow(window);
        }

        immediateWindows.Add(id);
    }

    public void SetCursor(MouseCursor c) {
        cursor = c;
    }

    public Rectangle GetAbsRect(Rectangle rect) {
        return new Rectangle(rect.x + CurrentDrawBounds.x, rect.y + CurrentDrawBounds.y, rect.width, rect.height);
    }

    public bool IsMouseOverRect(Rectangle rect) {
        if (currentWindowId != hoveredWindowId)
            return false;
        
        return JMath.PointInRect(GetAbsRect(rect), Find.Input.GetMousePosition());
    }
}