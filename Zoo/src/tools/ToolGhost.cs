using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using Raylib_cs;
using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools;

public enum GhostType {
    None,
    Circle,
    Square,
    Sprite
}

public class ToolGhost {
    // Constants
    private const int CircleResolution = 16;
    private static readonly Color DefaultGhostColour = new(102, 204, 255, 255);
    private static readonly Color DefaultBlockedColour = new(255, 102, 26, 255);

    // References
    private ToolManager toolManager;

    // Config
    public GhostType    Type;
    public bool         Snap;
    public bool         Follow;
    public bool         Elevate;
    public bool         Visible;
    public GraphicData? Graphics;
    public int          SpriteIndex;
    public Vector2      Scale;
    public Vector2      Pos;
    public Side         Side;
    public float        Radius;
    public Vector2      Offset;
    public Color        GhostColour   = DefaultGhostColour;
    public Color        BlockedColour = DefaultBlockedColour;

    // Properties
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
        Graphics      = null;
        SpriteIndex   = 0;
        GhostColour   = DefaultGhostColour;
        BlockedColour = DefaultBlockedColour;

        Pos    = Vector2.Zero;
        Side   = Side.North;
        Scale  = Vector2.One;
        Radius = 1;
        Offset = Vector2.Zero;
    }

    public void UpdatePos() {
        if (Follow) {
            Pos  = Find.Input.GetMouseWorldPos();
            Side = World.GetQuadrantAtPos(Pos);
        }
        if (Snap) {
            Pos = Pos.Floor();
        }
    }
    
    public void Render() {
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

        Draw.DrawLineStrip3D(lineVertices, GhostColour, (int)Depth.UI);
        Draw.DrawTriangleFan3D(fillVertices, GhostColour.WithAlpha(0.5f), (int)Depth.UI);
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

        Draw.DrawLineStrip3D(lineVertices, GhostColour, (int)Depth.UI);
        Draw.DrawTriangleFan3D(fillVertices, GhostColour.WithAlpha(0.5f), (int)Depth.UI);
    }

    private void RenderSprite() {
        if (Graphics == null) return;
        
        var pos = (Pos + Offset) * World.WorldScale;
        if (Elevate)
            pos.Y -= Find.World.Elevation.GetElevationAtPos(Pos + Offset);
        
        Graphics.Blit(
            pos: pos,
            depth: (int)Depth.UI,
            colour: CanPlace ? GhostColour : BlockedColour,
            index: SpriteIndex
        );
    }
}