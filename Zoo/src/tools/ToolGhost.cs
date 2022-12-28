using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.tools;

public enum GhostType {
    None,
    Circle,
    Square,
    Sprite,
    SpriteSheet
}

public class ToolGhost {
    private static readonly Color DefaultGhostColour = new(102, 204, 255, 255);
    private static readonly Color DefaultBlockedColour = new(255, 102, 26, 255);

    public GhostType    Type          { get; set; }
    public bool         Snap          { get; set; }
    public bool         Follow        { get; set; }
    public bool         Elevate       { get; set; }
    public bool         Visible       { get; set; }
    public bool         CanPlace      { get; set; }
    public Texture2D?   Sprite        { get; set; }
    public SpriteSheet? SpriteSheet   { get; set; }
    public int          SpriteIndex   { get; set; }
    public Vector2      Pos           { get; set; }
    public Vector2      Scale         { get; set; }
    public float        Radius        { get; set; }
    public Vector2      Offset        { get; set; }
    public Vector2      Pivot         { get; set; }
    public Color        GhostColour   { get; set; } = DefaultGhostColour;
    public Color        BlockedColour { get; set; } = DefaultBlockedColour;

    public ToolGhost() {
        Reset();
    }
    
    public void Reset() {
        Type          = GhostType.None;
        Snap          = false;
        Follow        = true;
        Elevate       = false;
        Visible       = true;
        Sprite        = null;
        SpriteSheet   = null;
        SpriteIndex   = 0;
        CanPlace      = true;
        GhostColour   = DefaultGhostColour;
        BlockedColour = DefaultBlockedColour;

        Pos    = new Vector2(0.0f, 0.0f);
        Scale  = new Vector2(1.0f, 1.0f);
        Radius = 1;
        Offset = new Vector2(0.0f, 0.0f);
        Pivot  = new Vector2(0.0f, 0.0f);
    }
    
    public void Render() {
        if (Follow) {
            Pos = Find.Input.GetMouseWorldPosition();
        }
        if (Snap) {
            Pos = Pos.Floor();
        }

        if (!Visible) return;

        switch (Type) {
            case GhostType.Circle: 
                RenderCircle();
                break;
            case GhostType.Square:
                RenderSquare();
                break;
            case GhostType.Sprite:
                RenderSprite();
                break;
            case GhostType.SpriteSheet:
                RenderSpriteSheet();
                break;
            case GhostType.None:
            default:
                break;
        }
    }
    
    private void RenderCircle() {}
    private void RenderSquare() {}
    private void RenderSprite() {}
    private void RenderSpriteSheet() {}
}