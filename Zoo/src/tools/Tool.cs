﻿using System.Numerics;

namespace Zoo.tools;

public enum ToolType {
    None,
    Biome,
    Elevation,
    Wall,
    Door,
    FootPath,
    TileObject,
    Delete
}

public abstract class Tool {
    // References
    protected ToolManager toolManager;

    // Virtual Properties
    public virtual string   Name => "";
    public virtual ToolType Type => ToolType.None;
    
    // Properties
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
    public virtual bool CanPlace(ToolGhost ghost) => true;
}