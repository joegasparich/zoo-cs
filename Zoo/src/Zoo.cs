using Zoo.tools;
using Zoo.world;

namespace Zoo; 

public class Zoo {
    public World World { get; }
    public ToolManager Tools;
    
    public Zoo() {
        World = new World(10, 10);
        Tools = new ToolManager();
    }

    public void Setup() {
        World.Setup();
        Tools.Setup();
    }

    public void Cleanup() {
        World.Cleanup();
        Tools.Cleanup();
    }

    public void PreUpdate() {
        World.PreUpdate();
        Tools.PreUpdate();
    }

    public void Update() {
        World.Update();
        Tools.Update();
    }

    public void PostUpdate() {
        World.PostUpdate();
        Tools.PostUpdate();
    }

    public void Render() {
        World.Render();
    }

    public void RenderLate() {
        Tools.Render();
    }

    public void OnGUI() {
        Tools.OnGUI();
    }

    public void OnInput(InputEvent evt) {
        Tools.OnInput(evt);
    }
}