using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.util;

namespace Zoo.world;

public enum FootPathSpriteIndex {
    Flat = 0,
    HillEast = 1,
    HillWest = 2,
    HillNorth = 3,
    HillSouth = 4
}

public class FootPath : ISerialisable {
    // Config
    public FootPathDef? Data;
    public IntVec2      Tile;
    public bool         Indestructible;
    public Color?       OverrideColour;

    // Properties
    public bool    Exists => Data != null;
    public Vector2 Pos    => Tile;

    public void Serialise() {
        Find.SaveManager.ArchiveValue("pathId",         () => Data.Id, id => Data = Find.AssetManager.GetDef<FootPathDef>(id));
        Find.SaveManager.ArchiveValue("tile",           ref Tile);
        Find.SaveManager.ArchiveValue("indestructible", ref Indestructible);
    }
}

public class FootPathGrid : ISerialisable {
    // Config
    private int cols;
    private int rows;
    
    // State
    private bool         isSetup = false;
    private FootPath[][] grid;
    
    public FootPathGrid(int cols, int rows) {
        this.cols = cols;
        this.rows = rows;
    }

    public void Setup(string[][]? data = null) {
        Debug.Log("Setting up path grid");
        
        grid = new FootPath[cols][];
        
        for (var i = 0; i < cols; i++) {
            grid[i] = new FootPath[rows];
            for (var j = 0; j < rows; j++) {
                var pathData = data?[i][j] != null ? Find.AssetManager.GetDef<FootPathDef>(data[i][j]) : null;
                grid[i][j] = new FootPath() {
                    Data = pathData,
                    Tile = new IntVec2(i, j),
                };
            }
        }
        
        isSetup = true;
    }

    public void Reset() {
        grid = null;
        cols = 0;
        rows = 0;
        
        isSetup = false;
    }

    public void Render() {
        if (!isSetup) {
            Debug.Warn("Path grid not setup");
            return;
        }
        
        for (int i = 0; i < cols; i++) {
            for (int j = 0; j < rows; j++) {
                var path = grid[i][j];
                if (!path.Exists) continue;
                if (path.Data == null) continue;

                var (spriteIndex, elevation) = FootPathUtility.GetSpriteInfo(path);
                Vector2 pos = path.Tile;
                pos -= new Vector2(0, 1 + elevation);
                
                path.Data.GraphicData.Blit(
                    pos: pos * World.WorldScale,
                    depth: (int)Depth.GroundCover,
                    overrideColour: path.OverrideColour,
                    index: (int)spriteIndex
                );
                
                path.OverrideColour = null;
            }
        }
    }

    public FootPath? PlacePathAtTile(FootPathDef data, IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return null;
        if (GetFootPathAtTile(tile)!.Exists) return null;
        
        grid[tile.X][tile.Y] = new FootPath() {
            Data = data,
            Tile = tile,
        };

        var path = grid[tile.X][tile.Y];
        
        Find.World.UpdateAccessibilityGrids(path.Tile);
        
        return grid[tile.X][tile.Y];
    }

    public void RemovePathAtTile(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return;
        if (!GetFootPathAtTile(tile)!.Exists) return;
        
        grid[tile.X][tile.Y] = new FootPath() {
            Tile = tile
        };

        Find.World.UpdateAccessibilityGrids(tile);
    }
    
    public IEnumerable<FootPath> GetAllFootPaths() {
        for (var i = 0; i < cols; i++) {
            for (var j = 0; j < rows; j++) {
                var path = grid[i][j];
                if (path.Exists)
                    yield return path;
            }
        }
    }

    public FootPath? GetFootPathAtTile(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return null;
        return grid[tile.X][tile.Y];
    }

    public void Serialise() {
        if (Find.SaveManager.Mode == SerialiseMode.Loading)
            Reset();
        
        Find.SaveManager.ArchiveValue("cols", ref cols);
        Find.SaveManager.ArchiveValue("rows", ref rows);
        
        if (Find.SaveManager.Mode == SerialiseMode.Loading)
            Setup();
        
        Find.SaveManager.ArchiveCollection("paths",
            GetAllFootPaths(),
            paths => paths.Select(pathData => GetFootPathAtTile(pathData["pos"].ToObject<IntVec2>()))
        );
    }
}