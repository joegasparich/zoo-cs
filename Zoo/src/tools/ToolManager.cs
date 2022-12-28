using Raylib_cs;
using Zoo.ui;

namespace Zoo.tools; 

public class ToolManager {
    public  ToolGhost Ghost { get;}

    private Tool   activeTool;
    private string toolbarHandle;
    
    public ToolManager() {
        Ghost = new ToolGhost();
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
    }

    public void SetTool(ToolType type) {
        if (activeTool != null && activeTool.Type == type) return;

        if (activeTool != null) {
            activeTool.Unset();
            Ghost.Reset();
        }
        
        // TODO: can we automate this
        switch(type) {
            // case ToolType.Biome:
            //     activeTool = new BiomeTool(this);
            //     break;
            // case ToolType.Elevation:
            //     activeTool = new ElevationTool(this);
            //     break;
            // case ToolType.Wall:
            //     activeTool = new WallTool(this);
            //     break;
            // case ToolType.Door:
            //     activeTool = new DoorTool(this);
            //     break;
            // case ToolType.Path:
            //     activeTool = new PathTool(this);
            //     break;
            // case ToolType.TileObject:
            //     activeTool = new TileObjectTool(this);
            //     break;
            // case ToolType.Delete:
            //     activeTool = new DeleteTool(this);
            //     break;
            case ToolType.None:
            default:
                activeTool = new NoTool(this);
                break;
        }

        activeTool.Set();
    }

    public Tool GetActiveTool() {
        return activeTool;
    }
}