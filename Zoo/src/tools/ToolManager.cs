using System.Text.Json.Nodes;
using Raylib_cs;
using Zoo.ui;

namespace Zoo.tools;

public class ToolAction {
    public string         Name;
    public object         Data;
    public Action<object> Undo;
}

public class ToolManager {
    public  ToolGhost Ghost { get;}

    private Tool              activeTool;
    private Stack<ToolAction> actionStack = new();
    private string            toolbarHandle;
    
    public ToolManager() {
        Ghost      = new ToolGhost(this);
        activeTool = new Tool_None(this);
    }

    public void Setup() {
        SetTool(ToolType.None);
        toolbarHandle = Find.UI.PushWindow(new Toolbar(this));
    }

    public void Cleanup() {
        SetTool(ToolType.None);
        Find.UI.CloseWindow(toolbarHandle);
    }

    public void PreUpdate() {
        activeTool.PreUpdate();
    }
    
    public void Update() {
        activeTool.Update();
    }
    
    public void PostUpdate() {
        activeTool.PostUpdate();
    }

    public void Render() {
        Ghost.Render();
        activeTool.Render();
    }
    
    public void OnGUI() {
        activeTool.OnGUI();
    }

    public void OnInput(InputEvent evt) {
        activeTool.OnInput(evt);
        if (evt.consumed) return;

        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_RIGHT) {
            SetTool(ToolType.None);
            evt.Consume();
        }
        
        // TODO: Ctrl/Cmd + Z
        if (evt.keyDown == KeyboardKey.KEY_Z) {
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
            case ToolType.Biome:
                activeTool = new Tool_Biome(this);
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