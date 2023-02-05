﻿using Raylib_cs;
using Zoo.ui;

namespace Zoo.tools;

public class ToolAction {
    public string         Name;
    public object         Data;
    public Action<object> Undo;
}

public class ToolManager {
    // References
    public  ToolGhost Ghost { get;}

    // State
    private Tool              activeTool;
    private Stack<ToolAction> actionStack = new();
    private string            toolbarHandle;
    
    public ToolManager() {
        Ghost      = new ToolGhost(this);
        activeTool = new Tool_None(this);
    }

    public void Setup() {
        SetTool(ToolType.None);
        toolbarHandle = Find.UI.PushWindow(new Widget_Toolbar(this));
    }

    public void Cleanup() {
        SetTool(ToolType.None);
        Find.UI.CloseWindow(toolbarHandle);
    }

    public void ConstantUpdate() {
        activeTool.Update();
    }

    public void Render() {
        Ghost.UpdatePos();
        activeTool.Render();
        Ghost.Render();
    }

    public void RenderLate() {
        activeTool.RenderLate();
        Ghost.RenderLate();
    }
    
    public void OnGUI() {
        activeTool.OnGUI();
    }

    public void OnInput(InputEvent evt) {
        activeTool.OnInput(evt);
        if (evt.consumed) return;

        if (activeTool.Type != ToolType.None && evt.mouseDown == MouseButton.MOUSE_BUTTON_RIGHT) {
            SetTool(ToolType.None);
            evt.Consume();
        }
        
        // TODO: Ctrl/Cmd + Z
        if (evt.inputDown == InputType.Undo) {
            Undo();
            evt.Consume();
        }
    }

    public void SetTool(ToolType type) {
        if (activeTool != null && activeTool.Type == type) return;

        if (activeTool != null) {
            activeTool.Unset();
            Ghost.Reset();
        }
        
        // TODO: can we automate this
        switch(type) {
            case ToolType.Terrain:
                activeTool = new Tool_Terrain(this);
                break;
            case ToolType.Elevation:
                activeTool = new Tool_Elevation(this);
                break;
            case ToolType.Wall:
                activeTool = new Tool_Wall(this);
                break;
            case ToolType.Door:
                activeTool = new Tool_Door(this);
                break;
            case ToolType.FootPath:
                activeTool = new Tool_FootPath(this);
                break;
            case ToolType.Animal:
                activeTool = new Tool_Animal(this);
                break;
            case ToolType.Staff:
                activeTool = new Tool_Staff(this);
                break;
            case ToolType.TileObject:
                activeTool = new Tool_TileObject(this);
                break;
            case ToolType.Delete:
                activeTool = new Tool_Delete(this);
                break;
            case ToolType.None:
            default:
                activeTool = new Tool_None(this);
                break;
        }

        activeTool.Set();
    }

    public Tool GetActiveTool() {
        return activeTool;
    }
    
    public void PushAction(ToolAction action) {
        actionStack.Push(action);
    }

    public void Undo() {
        if (actionStack.Count == 0) return;
        
        var action = actionStack.Pop();
        action.Undo(action.Data);
    }
}