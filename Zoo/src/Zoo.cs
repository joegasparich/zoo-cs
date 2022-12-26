using Zoo.world;

namespace Zoo; 

public class Zoo {
    public World World { get; }
    // public ToolManager Tools;
    
    public Zoo() {
        World = new World(10, 10);
        // Tools = new ToolManager();
    }

    public void Setup() {
        World.Setup();
    }

    public void PreUpdate() {
        World.PreUpdate();
    }

    public void Update() {
        World.Update();
    }

    public void PostUpdate() {
        World.PostUpdate();
    }

    public void Render() {
        World.Render();
    }

    public void RenderLate() { }
    public void OnGUI()      { }
    public void OnInput(InputEvent evt) { }
}