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

public class FootPath : ISerialisable, IBlueprintable {
    // Config
    public FootPathDef? Data;
    public IntVec2      Tile;
    public bool         Indestructible;
    public Color?       OverrideColour;

    // Properties
    public bool    Empty       => Data == null;
    public bool    IsBlueprint { get; internal set; }
    public bool    Exists      => !Empty && !IsBlueprint;
    public Vector2 Pos         => Tile;
    public string  UniqueId    => $"path{Tile.ToString()}";

    public void BuildBlueprint() {
        if (!IsBlueprint) return;

        IsBlueprint = false;
        Find.World.Blueprints.UnregisterBlueprint(this);
        Find.World.FootPaths.OnPathBuilt(this);
    }

    public List<IntVec2> GetBuildTiles() {
        return new List<IntVec2> { Tile };
    }

    public void Serialise() {
        Find.SaveManager.ArchiveValue("pathId",         () => Data.Id, id => Data = Find.AssetManager.GetDef<FootPathDef>(id));
        Find.SaveManager.ArchiveValue("tile",           ref Tile);
        Find.SaveManager.ArchiveValue("indestructible", ref Indestructible);
        Find.SaveManager.ArchiveValue("isBlueprint",    () => IsBlueprint, b => IsBlueprint = b);

        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            if (IsBlueprint)
                Find.World.Blueprints.RegisterBlueprint(this);
        }
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
                if (path.Empty) continue;
                if (path.Data == null) continue;

                var (spriteIndex, elevation) = FootPathUtility.GetSpriteInfo(path);
                Vector2 pos = path.Tile;
                pos -= new Vector2(0, 1 + elevation);
                
                path.Data.GraphicData.Blit(
                    pos: pos * World.WorldScale,
                    depth: (int)Depth.GroundCover,
                    overrideColour: path.OverrideColour ?? (path.IsBlueprint ? Colour.Blueprint : Color.WHITE),
                    index: (int)spriteIndex
                );
                
                path.OverrideColour = null;
            }
        }
    }

    public FootPath? PlacePathAtTile(FootPathDef data, IntVec2 tile, bool isBlueprint = false) {
        if (!Find.World.IsPositionInMap(tile)) return null;
        if (GetFootPathAtTile(tile)!.Exists) return null;
        
        grid[tile.X][tile.Y] = new FootPath() {
            Data = data,
            Tile = tile,
            IsBlueprint = isBlueprint
        };

        var path = grid[tile.X][tile.Y];

        if (isBlueprint)
            Find.World.Blueprints.RegisterBlueprint(path);
        
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
    
    public void OnPathBuilt(FootPath path) {
        Find.World.UpdateAccessibilityGrids(path.Tile);
    }
    
    public IEnumerable<FootPath> GetAllFootPaths(bool includeBlueprints) {
        for (var i = 0; i < cols; i++) {
            for (var j = 0; j < rows; j++) {
                var path = grid[i][j];
                if (path.Exists || includeBlueprints && path.IsBlueprint)
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
            GetAllFootPaths(true),
            paths => paths.Select(pathData => GetFootPathAtTile(pathData["pos"].ToObject<IntVec2>()))
        );
    }
}