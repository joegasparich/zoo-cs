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
    
    public ElevationGrid Elevation { get; }
    public BiomeGrid Biomes { get; }
        
    private bool isSetup = false;
    
    public World(int width, int height) {
        Width = width;
        Height = height;
        
        Elevation = new ElevationGrid(Width, Height);
        Biomes = new BiomeGrid(Width * BiomeGrid.BiomeScale, Height * BiomeGrid.BiomeScale);
    }
    
    public void Setup() {
        if (isSetup) return;
        
        Elevation.Setup();
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

    public IEnumerable<IntVec2> GetAdjacentTiles(IntVec2 tile, bool diagonals = false) {
        for (int i = 0; i < 4; i++) {
            var adj = GetTileInDirection(tile, (Side) i);
            if (IsPositionInMap(adj)) yield return adj;
        }

        if (diagonals) {
            for (int i = 0; i < 4; i++) {
                var adj = GetTileInDirection(tile, (Side) i);
                var adj2 = GetTileInDirection(adj, (Side) ((i + 1) % 4));
                if (IsPositionInMap(adj2)) yield return adj2;
            }
        }
    }

    public static Side GetQuadrantAtPos(Vector2 pos) {
        var xRel = (pos.X + LargerThanWorld) % 1f - 0.5f;
        var yRel = (pos.Y + LargerThanWorld) % 1f - 0.5f;
        
        if (yRel <= 0 && Math.Abs(yRel) >= Math.Abs(xRel)) return Side.North;
        if (xRel >= 0 && Math.Abs(xRel) >= Math.Abs(yRel)) return Side.East;
        if (yRel >= 0 && Math.Abs(yRel) >= Math.Abs(xRel)) return Side.South;
        if (xRel <= 0 && Math.Abs(xRel) >= Math.Abs(yRel)) return Side.West;

        return Side.North;
    }
}