using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo;

public enum InputType {
    CameraLeft,
    CameraRight,
    CameraUp,
    CameraDown,
    CameraZoomIn,
    CameraZoomOut,
    Undo, // TODO: combos
    Pause,
    NormalSpeed,
    FastSpeed,
    FasterSpeed,
    IncreaseBrushSize,
    DecreaseBrushSize,
    RotateClockwise,
    RotateCounterClockwise
}

public enum InputEventType {
    Key,
    MouseButton,
    MouseScroll,
    Input
}

public class InputEvent {
    public InputEventType type;
    public KeyboardKey?   keyDown   = null;
    public KeyboardKey?   keyUp     = null;
    public KeyboardKey?   keyHeld   = null;
    public MouseButton?   mouseDown = null;
    public MouseButton?   mouseUp   = null;
    public MouseButton?   mouseHeld = null;
    public InputType?         inputDown = null;
    public InputType?         inputUp   = null;
    public InputType?         inputHeld = null;
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
    
    // Collections
    private Dictionary<KeyboardKey, InputType[]> inputs = new() {
        // Default inputs
        {KeyboardKey.KEY_W, new[] {InputType.CameraUp}},
        {KeyboardKey.KEY_A, new[] {InputType.CameraLeft}},
        {KeyboardKey.KEY_S, new[] {InputType.CameraDown}},
        {KeyboardKey.KEY_D, new[] {InputType.CameraRight}},
        {KeyboardKey.KEY_UP, new[] {InputType.CameraUp}},
        {KeyboardKey.KEY_LEFT, new[] {InputType.CameraLeft}},
        {KeyboardKey.KEY_DOWN, new[] {InputType.CameraDown}},
        {KeyboardKey.KEY_RIGHT, new[] {InputType.CameraRight}},
        {KeyboardKey.KEY_COMMA, new[] {InputType.CameraZoomOut}},
        {KeyboardKey.KEY_PERIOD, new[] {InputType.CameraZoomIn}},
        {KeyboardKey.KEY_Z, new[] {InputType.Undo}},
        {KeyboardKey.KEY_SPACE, new[] {InputType.Pause}},
        {KeyboardKey.KEY_ONE, new[] {InputType.NormalSpeed}},
        {KeyboardKey.KEY_TWO, new[] {InputType.FastSpeed}},
        {KeyboardKey.KEY_THREE, new[] {InputType.FasterSpeed}},
        {KeyboardKey.KEY_LEFT_BRACKET, new[] {InputType.DecreaseBrushSize}},
        {KeyboardKey.KEY_RIGHT_BRACKET, new[] {InputType.IncreaseBrushSize}},
        {KeyboardKey.KEY_Q, new[] {InputType.RotateCounterClockwise}},
        {KeyboardKey.KEY_E, new[] {InputType.RotateClockwise}},
    };

    // State
    private InputEvent currentEvent;

    public void ProcessInput() {
        // Key events
        for (int k = 0; k < KeyMax; k++) {
            var key = (KeyboardKey)k;
            if (!Raylib.IsKeyPressed(key) && !Raylib.IsKeyReleased(key) && !Raylib.IsKeyDown(key))
                continue;

            // Raw Keys
            var evt = new InputEvent(InputEventType.Key);
            evt.keyDown       = Raylib.IsKeyPressed(key) ? key : null;
            evt.keyUp         = Raylib.IsKeyReleased(key) ? key : null;
            evt.keyHeld       = Raylib.IsKeyDown(key) ? key : null;
            evt.mousePos      = Raylib.GetMousePosition();
            evt.mouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.mousePos);

            FireInputEvent(evt);
            
            // Inputs
            if (!inputs.ContainsKey(key)) continue;
            
            foreach (var input in inputs[key]) {
                evt = new InputEvent(InputEventType.Input);
                evt.inputDown       = Raylib.IsKeyPressed(key) ? input : null;
                evt.inputUp         = Raylib.IsKeyReleased(key) ? input : null;
                evt.inputHeld       = Raylib.IsKeyDown(key) ? input : null;
                evt.mousePos        = Raylib.GetMousePosition();
                evt.mouseWorldPos   = Find.Renderer.ScreenToWorldPos(evt.mousePos);
                FireInputEvent(evt);
            }
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

    public void RegisterInput(InputType input, KeyboardKey key) {
        if (!inputs.ContainsKey(key))
            inputs[key] = Array.Empty<InputType>();

        inputs[key] = inputs[key].Append(input).ToArray();
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