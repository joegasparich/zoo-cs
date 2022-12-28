using System.Numerics;

namespace Zoo.tools;

public enum ToolType {
    None,
    Biome,
    Elevation,
    Wall,
    Door,
    Path,
    TileObject,
    Delete
}

public abstract class Tool {
    protected ToolManager toolManager;
        
    public virtual string Name { get; }
    public virtual ToolType Type { get; }

    protected ToolGhost Ghost => toolManager.Ghost;
    
    public Tool(ToolManager toolManager) {
        this.toolManager = toolManager;
    }

    public virtual void Set() {}
    public virtual void Unset() {}
    public virtual void PreUpdate() {}
    public virtual void Update() {}
    public virtual void PostUpdate() {}
    public virtual void Render() {}
    public virtual void OnGUI() {}
    public virtual void OnInput(InputEvent  evt) {}
    public virtual bool CanPlace(Vector2 pos) => true;
}