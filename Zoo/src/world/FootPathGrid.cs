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

public class FootPath : ISerialisable {
    public FootPathData? Data           = null;
    public IntVec2       Pos            = default;
    public bool          Indestructable = false;
    public Color         OverrideColour = Color.WHITE;

    public bool Exists => Data != null;

    public void Serialise() {
        Find.SaveManager.ArchiveValue("pathId",         () => Data.Id, id => Data = Find.Registry.GetFootPath(id));
        Find.SaveManager.ArchiveValue("pos",            ref Pos);
        Find.SaveManager.ArchiveValue("indestructable", ref Indestructable);
    }
}

public class FootPathGrid : ISerialisable {
    private bool         isSetup = false;
    private FootPath[][] grid;
    private int          cols;
    private int          rows;
    
    public FootPathGrid(int cols, int rows) {
        this.cols = cols;
        this.rows = rows;
    }

    public void Setup(string[][]? data = null) {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Setting up path grid");
        
        grid = new FootPath[cols][];
        
        for (var i = 0; i < cols; i++) {
            grid[i] = new FootPath[rows];
            for (var j = 0; j < rows; j++) {
                var pathData = data?[i][j] != null ? Find.Registry.GetFootPath(data[i][j]) : null;
                grid[i][j] = new FootPath() {
                    Data = pathData,
                    Pos = new IntVec2(i, j),
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
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Path grid not setup");
            return;
        }
        
        for (int i = 0; i < cols; i++) {
            for (int j = 0; j < rows; j++) {
                var path = grid[i][j];
                if (!path.Exists) continue;
                if (path.Data == null) continue;

                var (spriteIndex, elevation) = FootPathUtility.GetSpriteInfo(path);
                Vector2 pos = path.Pos;
                pos -= new Vector2(0, 1 + elevation);
                
                path.Data.GraphicData.Blit(
                    pos: pos * World.WorldScale,
                    depth: Depth.GroundCover.ToInt(),
                    colour: path.OverrideColour,
                    index: spriteIndex.ToInt()
                );
                
                path.OverrideColour = Color.WHITE;
            }
        }
    }

    public FootPath? PlacePathAtTile(FootPathData data, IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return null;
        if (GetFootPathAtTile(tile)!.Exists) return null;
        
        grid[tile.X][tile.Y] = new FootPath() {
            Data = data,
            Pos = tile,
        };
        
        // TODO (optimisation): update pathfinding once using walkability grid
        
        return grid[tile.X][tile.Y];
    }

    public void RemovePathAtTile(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return;
        if (!GetFootPathAtTile(tile)!.Exists) return;
        
        grid[tile.X][tile.Y] = new FootPath() {
            Pos = tile
        };

        // TODO (optimisation): update pathfinding once using walkability grid
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
        
        Find.SaveManager.ArchiveCollection("walls",
            GetAllFootPaths(),
            paths => paths.Select(pathData => 
                GetFootPathAtTile(Find.SaveManager.Deserialise<IntVec2>(pathData["pos"])))
        );
    }
}