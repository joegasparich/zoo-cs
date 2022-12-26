using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.world;

public enum Side {
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public class World {
    public const int LargerThanWorld = 10000;
    
    public  int  Width  { get; }
    public  int  Height { get; }
    
    public BiomeGrid Biomes { get; }
        
    private bool isSetup = false;
    
    public World(int width, int height) {
        Width = width;
        Height = height;
        
        Biomes = new BiomeGrid(Width * BiomeGrid.BiomeScale, Height * BiomeGrid.BiomeScale);
    }
    
    public void Setup() {
        if (isSetup) return;
        
        Biomes.Setup();
        isSetup = true;
        
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "World setup complete");
    }

    public void PreUpdate() { }
    public void Update() { }

    public void PostUpdate() {
        Biomes.PostUpdate();
    }

    public void Render() {
        Biomes.Render();
    }
    public void RenderDebug() { }

    public bool IsPositionInMap(Vector2 pos) {
        return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
    }

    public IntVec2 GetTileInDirection(IntVec2 tile, Side dir) {
            switch (dir) {
            case Side.North: return new IntVec2(tile.X,     tile.Y - 1);
            case Side.East:  return new IntVec2(tile.X + 1, tile.Y);
            case Side.South: return new IntVec2(tile.X,     tile.Y + 1);
            case Side.West:  return new IntVec2(tile.X - 1, tile.Y);
            default:         return tile;
        }
    }

    public Side GetQuadrantAtPos(Vector2 pos) {
        var xRel = (pos.X + LargerThanWorld) % 1f - 0.5f;
        var yRel = (pos.Y + LargerThanWorld) % 1f - 0.5f;
        
        if (yRel <= 0 && Math.Abs(yRel) >= Math.Abs(xRel)) return Side.North;
        if (xRel >= 0 && Math.Abs(xRel) >= Math.Abs(yRel)) return Side.East;
        if (yRel >= 0 && Math.Abs(yRel) >= Math.Abs(xRel)) return Side.South;
        if (xRel <= 0 && Math.Abs(xRel) >= Math.Abs(yRel)) return Side.West;

        return Side.North;
    }
}