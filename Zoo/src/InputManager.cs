using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo;

public enum InputEventType {
    Key,
    MouseButton,
    MouseScroll
}

public class InputEvent {
    public InputEventType type;
    public KeyboardKey?   keyDown   = null;
    public KeyboardKey?   keyUp     = null;
    public KeyboardKey?   keyHeld   = null;
    public MouseButton?   mouseDown = null;
    public MouseButton?   mouseUp   = null;
    public MouseButton?   mouseHeld = null;
    public Vector2        mousePos;
    public Vector2        mouseWorldPos;
    public float          mouseScroll;
    public bool           consumed = false;
    
    public InputEvent(InputEventType type) {
        this.type = type;
    }

    public void Consume() {
        consumed = true;
    }
}

public class InputManager {
    // Constants
    private static readonly int MouseButtonNull = -1;
    private static readonly int MouseButtonMax  = (int)MouseButton.MOUSE_BUTTON_BACK;
    private static readonly int KeyMax          = (int)KeyboardKey.KEY_KB_MENU;

    // State
    private InputEvent currentEvent;

    public void ProcessInput() {
        // Key events
        for (int k = 0; k < KeyMax; k++) {
            var key = (KeyboardKey)k;
            if (!Raylib.IsKeyPressed(key) && !Raylib.IsKeyReleased(key) && !Raylib.IsKeyDown(key))
                continue;

            var evt = new InputEvent(InputEventType.Key);
            evt.keyDown       = Raylib.IsKeyPressed(key) ? key : null;
            evt.keyUp         = Raylib.IsKeyReleased(key) ? key : null;
            evt.keyHeld       = Raylib.IsKeyDown(key) ? key : null;
            evt.mousePos      = Raylib.GetMousePosition();
            evt.mouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.mousePos);

            FireInputEvent(evt);
        }

        // Mouse events
        for (int mb = 0; mb < MouseButtonMax; mb++) {
            var mouseButton = (MouseButton)mb;
            if (!Raylib.IsMouseButtonPressed(mouseButton) && !Raylib.IsMouseButtonReleased(mouseButton) && !Raylib.IsMouseButtonDown(mouseButton))
                continue;

            var evt = new InputEvent(InputEventType.MouseButton);
            evt.mouseDown     = Raylib.IsMouseButtonPressed(mouseButton) ? mouseButton : null;
            evt.mouseUp       = Raylib.IsMouseButtonReleased(mouseButton) ? mouseButton : null;
            evt.mouseHeld     = Raylib.IsMouseButtonDown(mouseButton) ? mouseButton : null;
            evt.mousePos      = Raylib.GetMousePosition();
            evt.mouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.mousePos);

            FireInputEvent(evt);
        }

        if (Raylib.GetMouseWheelMove() != 0) {
            var evt = new InputEvent(InputEventType.MouseScroll);
            evt.mouseScroll   = Raylib.GetMouseWheelMove();
            evt.mousePos      = Raylib.GetMousePosition();
            evt.mouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.mousePos);

            FireInputEvent(evt);
        }
    }

    public void FireInputEvent(InputEvent evt) {
        currentEvent = evt;
        
        Game.OnInput(evt);
        // Messenger::fire(EventType::InputEvent);
    }
    
    public Vector2 GetMousePos() {
        return Raylib.GetMousePosition();
    }
    
    public Vector2 GetMouseWorldPos() {
        return Find.Renderer.ScreenToWorldPos(GetMousePos());
    }
    
    public InputEvent GetCurrentEvent() {
        return currentEvent;
    }
}