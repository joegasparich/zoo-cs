using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools;

public enum GhostType {
    None,
    Circle,
    Square,
    Sprite,
    SpriteSheet
}

public class ToolGhost {
    private const int CircleResolution = 16;
    private static readonly Color DefaultGhostColour = new(102, 204, 255, 255);
    private static readonly Color DefaultBlockedColour = new(255, 102, 26, 255);

    private ToolManager toolManager;

    public GhostType    Type          { get; set; }
    public bool         Snap          { get; set; }
    public bool         Follow        { get; set; }
    public bool         Elevate       { get; set; }
    public bool         Visible       { get; set; }
    public Texture2D?   Sprite        { get; set; }
    public SpriteSheet? SpriteSheet   { get; set; }
    public int          SpriteIndex   { get; set; }
    public Vector2      Pos           { get; set; }
    public Side         Side          { get; set; }
    public Vector2      Scale         { get; set; }
    public float        Radius        { get; set; }
    public Vector2      Offset        { get; set; }
    public Vector2      Origin        { get; set; }
    public Color        GhostColour   { get; set; } = DefaultGhostColour;
    public Color        BlockedColour { get; set; } = DefaultBlockedColour;

    public bool CanPlace => toolManager.GetActiveTool().CanPlace(this);       

    public ToolGhost(ToolManager toolManager) {
        this.toolManager = toolManager;
        
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
        GhostColour   = DefaultGhostColour;
        BlockedColour = DefaultBlockedColour;

        Pos    = Vector2.Zero;
        Side   = Side.North;
        Scale  = Vector2.One;
        Radius = 1;
        Offset = Vector2.Zero;
        Origin = Vector2.Zero;
    }
    
    public void Render() {
        if (Follow) {
            Pos  = Find.Input.GetMouseWorldPos();
            Side = World.GetQuadrantAtPos(Pos);
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

    private void RenderCircle() {
        var fillVertices = new Vector2[CircleResolution + 2];
        var lineVertices = new Vector2[CircleResolution];

        fillVertices[0] = Pos * World.WorldScale;
        for (var n = 1; n < CircleResolution + 1; ++n) {
            fillVertices[n] = Pos;

            fillVertices[n].X += Radius * MathF.Sin(2.0f * MathF.PI * n / CircleResolution);
            fillVertices[n].Y += Radius * MathF.Cos(2.0f * MathF.PI * n / CircleResolution);

            if (Elevate)
                fillVertices[n].Y -= Find.World.Elevation.GetElevationAtPos(fillVertices[n]);

            fillVertices[n] *= World.WorldScale;
            lineVertices[n - 1] = fillVertices[n];
        }
        fillVertices[CircleResolution + 1] = fillVertices[1];

        Draw.DrawLineStrip3D(lineVertices, GhostColour, Depth.UI.ToInt());
        Draw.DrawTriangleFan3D(fillVertices, GhostColour.WithAlpha(0.5f), Depth.UI.ToInt());
    }

    private void RenderSquare() {
        var fillVertices = new Vector2[6];
        var lineVertices = new Vector2[4];

        fillVertices[0] = (Pos + new Vector2(Scale.X/2, Scale.Y/2)) * World.WorldScale; // Centre for fan
        fillVertices[1] = (Pos + new Vector2(0,         0))         * World.WorldScale;
        fillVertices[2] = (Pos + new Vector2(0,         Scale.Y))   * World.WorldScale;
        fillVertices[3] = (Pos + new Vector2(Scale.X,   Scale.Y))   * World.WorldScale;
        fillVertices[4] = (Pos + new Vector2(Scale.X,   0))         * World.WorldScale;
        fillVertices[5] = fillVertices[1]; // close loop
        
        lineVertices[0] = fillVertices[1];
        lineVertices[1] = fillVertices[2];
        lineVertices[2] = fillVertices[3];
        lineVertices[3] = fillVertices[4];

        Draw.DrawLineStrip3D(lineVertices, GhostColour, Depth.UI.ToInt());
        Draw.DrawTriangleFan3D(fillVertices, GhostColour.WithAlpha(0.5f), Depth.UI.ToInt());
    }

    private void RenderSprite() {
        if (!Sprite.HasValue) return;
        
        var pos = (Pos + Offset) * World.WorldScale;
        if (Elevate)
            pos.Y -= Find.World.Elevation.GetElevationAtPos(Pos + Offset);

        Find.Renderer.Blit(
            texture: Sprite.Value,
            pos: pos,
            depth: Depth.UI.ToInt(),
            scale: new Vector2(Sprite.Value.width, Sprite.Value.height) * Renderer.PixelScale,
            origin: Origin,
            color: CanPlace ? GhostColour : BlockedColour
        );
    }

    private void RenderSpriteSheet() {
        if (!SpriteSheet.HasValue) return;
        
        var pos = (Pos + Offset) * World.WorldScale;
        if (Elevate)
            pos.Y -= Find.World.Elevation.GetElevationAtPos(Pos + Offset);
        var source = SpriteSheet.Value.GetCellBounds(SpriteIndex);
        
        Find.Renderer.Blit(
            texture: SpriteSheet.Value.Texture,
            pos: pos,
            depth: Depth.UI.ToInt(),
            scale: new Vector2(SpriteSheet.Value.Texture.width * source.width, SpriteSheet.Value.Texture.height * source.height) * Renderer.PixelScale,
            origin: Origin,
            source: source,
            color: CanPlace ? GhostColour : BlockedColour
        );
    }
}