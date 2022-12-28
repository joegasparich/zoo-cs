using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.world;

public enum FootPathSpriteIndex {
    Flat = 0,
    HillEast = 1,
    HillWest = 2,
    HillNorth = 3,
    HillSouth = 4
}

public class FootPath {
    public FootPathData? Data           = null;
    public IntVec2   Pos            = default;
    public bool      Exists         = false;
    public bool      Indestructable = false;
    public Color     OverrideColour = Color.WHITE;
    public FootPath() {}
}

public class FootPathGrid {
    private bool        isSetup = false;
    private FootPath[,] grid;
    private int         cols;
    private int         rows;
    
    public FootPathGrid(int cols, int rows) {
        this.cols = cols;
        this.rows = rows;
        grid = new FootPath[cols, rows];
    }

    public void Setup() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Setting up path grid");
        
        for (int i = 0; i < cols; i++) {
            for (int j = 0; j < rows; j++) {
                grid[i, j] = new FootPath() {
                    Pos = new IntVec2(i, j)
                };
            }
        }
        
        isSetup = true;

        PlacePathAtTile(Find.Registry.GetFootPath(FOOTPATHS.DIRT_PATH), new IntVec2(5, 5));
    }

    public void Reset() {
        grid = null;
        cols = 0;
        rows = 0;
        
        isSetup = false;
    }

    public void Render() {
        if (!isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Path grid not setup");
            return;
        }
        
        for (int i = 0; i < cols; i++) {
            for (int j = 0; j < rows; j++) {
                var path = grid[i, j];
                if (!path.Exists || !path.Data.HasValue) continue;

                var (spriteIndex, elevation) = FootPathUtility.GetSpriteInfo(path);
                Vector2 pos = path.Pos;
                pos -= new Vector2(0, 1 + elevation);
                
                Find.Renderer.Blit(
                    texture: path.Data.Value!.SpriteSheet.Texture,
                    pos: pos * World.WorldScale,
                    depth: Depth.GroundCover.ToInt(),
                    scale: new Vector2(1, 2) * World.WorldScale,
                    source: path.Data.Value!.SpriteSheet.GetCellBounds(spriteIndex.ToInt()),
                    color: path.OverrideColour
                );
                
                path.OverrideColour = Color.WHITE;
            }
        }
    }

    public FootPath? PlacePathAtTile(FootPathData data, IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return null;
        if (GetPathAtTile(tile)!.Exists) return null;
        
        grid[tile.X, tile.Y] = new FootPath() {
            Data = data,
            Pos = tile,
            Exists = true
        };
        
        // TODO: update pathfinding once using walkability grid
        
        return grid[tile.X, tile.Y];
    }

    public void RemovePathAtTile(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return;
        if (!GetPathAtTile(tile)!.Exists) return;
        
        grid[tile.X, tile.Y] = new FootPath() {
            Pos = tile
        };

        // TODO: update pathfinding once using walkability grid
    }

    public FootPath? GetPathAtTile(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return null;
        return grid[tile.X, tile.Y];
    }
}