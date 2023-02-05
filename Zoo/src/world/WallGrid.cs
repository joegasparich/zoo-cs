using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using Zoo.defs;
using Zoo.util;

namespace Zoo.world;

public enum Orientation {
    Vertical = 0,
    Horizontal = 1
}

public enum WallSpriteIndex {
    Horizontal = 0,
    Vertical = 1,
    DoorHorizontal = 2,
    DoorVertical = 3,
    HillEast = 4,
    HillWest = 5,
    HillNorth = 6,
    HillSouth = 7,
};

public class Wall : ISerialisable {
    // Config
    public WallDef? Data           = null;
    public IntVec2  GridPos        = default;
    public bool     Indestructable = false;
    public bool     IsDoor         = false;
    public Color?   OverrideColour;
    
    // Properties
    public Orientation Orientation => (Orientation)(GridPos.X % 2);
    public Vector2     WorldPos    => WallUtility.WallToWorldPosition(GridPos, Orientation);
    public bool        Exists      => Data != null;

    public void Serialise() {
        Find.SaveManager.ArchiveValue("wallId", () => Data.Id, id => Data = Find.AssetManager.GetDef<WallDef>(id));
        Find.SaveManager.ArchiveValue("gridPos", ref GridPos);
        Find.SaveManager.ArchiveValue("indestructable", ref Indestructable);
        Find.SaveManager.ArchiveValue("isDoor", ref IsDoor);
    }
}

public record WallSaveData(string? id, bool isDoor, bool indestructable);

public class WallGrid : ISerialisable {
    // Config
    private int cols;
    private int rows;
    
    // State
    private bool     isSetup = false;
    private Wall[][] grid;
    
    public WallGrid(int cols, int rows) {
        this.cols = cols;
        this.rows = rows;
    }

    public void Setup(WallSaveData?[][]? data = null) {
        if (isSetup) {
            Debug.Warn("Tried to setup WallGrid which was already setup");
            return;
        }
        
        Debug.Log("Setting up wall grid");
        
        grid = new Wall[cols * 2 + 1][];
        
        for (var i = 0; i < cols * 2 + 1; i++) {
            var orientation = (Orientation)(i % 2);
            grid[i] = new Wall[rows + (int)orientation];
            
            for (var j = 0; j < rows + (int)orientation; j++) {
                var worldPos = WallUtility.WallToWorldPosition(new IntVec2(i, j), orientation);
                var wallData = data?[i][j]?.id != null ? Find.AssetManager.GetDef<WallDef>(data[i][j]!.id!) : null;
                grid[i][j] = new Wall {
                    Data           = wallData,
                    GridPos        = new IntVec2(i, j),
                    IsDoor         = data?[i][j]?.isDoor ?? false,
                    Indestructable = data?[i][j]?.indestructable ?? false
                };
            }
        }

        UpdatePathfinding();

        isSetup = true;
    }

    public void Reset() {
        if (!isSetup) {
            Debug.Warn("Tried to reset when WallGrid wasn't setup");
            return;
        }
        
        grid = null;
        cols = 0;
        rows = 0;

        isSetup = false;
    }

    public void Render() {
        for (var i = 0; i < cols * 2 + 1; i++) {
            var orientation = (Orientation)(i % 2);
            for (var j = 0; j < rows + (int)orientation; j++) {
                var wall = grid[i][j];
                if (!wall.Exists) continue;
                if (wall.Data == null) continue;
                
                var (spriteIndex, elevation) = WallUtility.GetSpriteInfo(wall);
                var pos = orientation == Orientation.Horizontal ? new Vector2(i / 2.0f, j) : new Vector2(i / 2.0f, j + 1);
                pos -= new Vector2(0.5f, 2.0f + elevation); // Offset cell size
                
                wall.Data.GraphicData.Blit(
                    pos: pos * World.WorldScale,
                    depth: Find.Renderer.GetDepth(wall.WorldPos.Y),
                    overrideColour: wall.OverrideColour,
                    index: (int)spriteIndex
                );
                
                wall.OverrideColour = null;
            }
        }
    }

    public Wall? PlaceWallAtTile(WallDef data, IntVec2 tile, Side side, bool indestructable = false) {
        if (!IsWallPosInMap(tile, side)) return null;
        if (GetWallAtTile(tile, side)!.Exists) return null;
        
        var (x, y, orientation) = WallUtility.GetGridPosition(tile, side);

        grid[x][y] = new Wall() {
            Data           = data,
            GridPos        = new IntVec2(x, y),
            Indestructable = indestructable,
        };
        
        var wall = grid[x][y];
        
        UpdatePathfindingAtWall(wall);
        
        if (ShouldCheckForLoop(wall) && CheckForLoop(wall)) {
            Find.World.Areas.FormAreasAtWall(wall);
        }

        return wall;
    }

    public void RemoveWall(Wall wall) {
        RemoveWallAtTile(wall.WorldPos.Floor(), wall.Orientation == Orientation.Horizontal ? Side.North : Side.West);
    }

    public void RemoveWallAtTile(IntVec2 tile, Side side) {
        if (!IsWallPosInMap(tile, side)) return;
        
        // Get grid pos
        var (x, y, orientation) = WallUtility.GetGridPosition(tile, side);
        if (!grid[x][y].Exists || grid[x][y].Indestructable) return;
        
        // Set to empty wall
        grid[x][y] = new Wall() {
            Data           = null,
            GridPos        = new IntVec2(x, y),
            Indestructable = false,
        };
        
        var wall = grid[x][y];
        
        UpdatePathfindingAtWall(wall);
        Find.World.Areas.JoinAreasAtWall(wall);
    }

    public void PlaceDoor(Wall wall) {
        if (!wall.Exists) return;
        
        wall.IsDoor = true;
        
        var adjacentTiles = wall.GetAdjacentTiles();
        if (adjacentTiles.Length < 2) return;
        
        var areaA = Find.World.Areas.GetAreaAtTile(adjacentTiles[0]);
        var areaB = Find.World.Areas.GetAreaAtTile(adjacentTiles[1]);
        
        areaA.AddAreaConnection(areaB, wall);
        areaB.AddAreaConnection(areaA, wall);
    }

    public void RemoveDoor(Wall wall) {
        if (!wall.Exists) return;
        
        wall.IsDoor = false;
        
        var adjacentTiles = wall.GetAdjacentTiles();
        if (adjacentTiles.Length < 2) return;
        
        var areaA = Find.World.Areas.GetAreaAtTile(adjacentTiles[0]);
        var areaB = Find.World.Areas.GetAreaAtTile(adjacentTiles[1]);
        
        areaA.RemoveAreaConnection(areaB, wall);
        areaB.RemoveAreaConnection(areaA, wall);
    }

    private void UpdatePathfindingAtWall(Wall wall) {
        var (x, y) = wall.WorldPos;

        if (wall.Orientation == Orientation.Horizontal) {
            if (Find.World.IsPositionInMap(new Vector2(x - 0.5f, y - 1))) {
                var north = new Vector2(x - 0.5f, y - 1.0f).Floor();
                Find.World.Pathfinder.SetAccessibility(north, Direction.S, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(north, Direction.SE, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(north, Direction.SW, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(north + new IntVec2(-1, 0), Direction.SE, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(north + new IntVec2(1, 0), Direction.SW, !wall.Exists);
            }
            if (Find.World.IsPositionInMap(new Vector2(x - 0.5f, y))) {
                var south = new Vector2(x - 0.5f, y).Floor();
                Find.World.Pathfinder.SetAccessibility(south, Direction.N, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(south, Direction.NE, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(south, Direction.NW, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(south + new IntVec2(-1, 0), Direction.NE, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(south + new IntVec2(1, 0), Direction.NW, !wall.Exists);
            }
        } else {
            if (Find.World.IsPositionInMap(new Vector2(x - 1.0f, y - 0.5f))) {
                var west = new Vector2(x - 1.0f, y - 0.5f).Floor();
                Find.World.Pathfinder.SetAccessibility(west, Direction.E, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(west, Direction.NE, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(west, Direction.SE, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(west + new IntVec2(0, -1), Direction.SE, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(west + new IntVec2(0, 1), Direction.NE, !wall.Exists);
            }
            if (Find.World.IsPositionInMap(new Vector2(x, y - 0.5f))) {
                var east = new Vector2(x, y - 0.5f).Floor();
                Find.World.Pathfinder.SetAccessibility(east, Direction.W, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(east, Direction.NW, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(east, Direction.SW, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(east + new IntVec2(0, -1), Direction.SW, !wall.Exists);
                Find.World.Pathfinder.SetAccessibility(east + new IntVec2(0, 1), Direction.NW, !wall.Exists);
            }
        }
    }

    // This is expensive as fuck so only use it on load
    private void UpdatePathfinding() {
        for (var i = 0; i < cols * 2 + 1; i++) {
            var orientation = i % 2;
            for (var j = 0; j < rows + orientation; j++) {
                var wall = grid[i][j];
                if (wall.Exists)
                    UpdatePathfindingAtWall(wall);
            }
        }
    }

    public IEnumerable<Wall> GetAllWalls() {
        for (var i = 0; i < cols * 2 + 1; i++) {
            var orientation = i % 2;
            for (var j = 0; j < rows + orientation; j++) {
                var wall = grid[i][j];
                if (wall.Exists)
                    yield return wall;
            }
        }
    }

    public Wall? GetWallAtTile(IntVec2 tile, Side side) {
        if (!IsWallPosInMap(tile, side)) {
            // Invert position and side if tile pos is correct but on the outside of the map
            if (Find.World.IsPositionInMap(tile + new IntVec2(0, 1)) && side == Side.South) {
                tile += new IntVec2(0, 1);
                side =  Side.North;
            } else if (Find.World.IsPositionInMap(tile + new IntVec2(0, -1)) && side == Side.North) {
                tile += new IntVec2(0, -1);
                side =  Side.South;
            } else if (Find.World.IsPositionInMap(tile + new IntVec2(1, 0)) && side == Side.East) {
                tile += new IntVec2(-1, 0);
                side =  Side.West;
            } else if (Find.World.IsPositionInMap(tile + new IntVec2(-1, 0)) && side == Side.West) {
                tile += new IntVec2(1, 0);
                side =  Side.East;
            } else {
                // Position is outside map
                return null;
            }
        }

        return side switch {
            Side.North => grid[tile.X * 2 + 1][tile.Y],
            Side.East => grid[tile.X * 2 + 2][tile.Y],
            Side.South => grid[tile.X * 2 + 1][tile.Y + 1],
            Side.West => grid[tile.X * 2][tile.Y],
            _ => null
        };
    }
    
    public Wall? GetWallByGridPos(IntVec2 gridPos) {
        if (!IsWallGridPosInMap(gridPos)) return null;
        
        return grid[gridPos.X][gridPos.Y];
    }

    private bool IsWallPosInMap(IntVec2 tile, Side side) {
        return Find.World.IsPositionInMap(tile) ||
               Find.World.IsPositionInMap(tile + new IntVec2(0, 1))  && side == Side.South ||
               Find.World.IsPositionInMap(tile + new IntVec2(0, -1)) && side == Side.North ||
               Find.World.IsPositionInMap(tile + new IntVec2(1, 0))  && side == Side.East  ||
               Find.World.IsPositionInMap(tile + new IntVec2(-1, 0)) && side == Side.West;
    }

    private bool IsWallGridPosInMap(IntVec2 gridPos) {
        return gridPos.X >= 0 && gridPos.X < grid.Length && gridPos.Y >= 0 && gridPos.Y < grid[gridPos.X].Length;
    }

    public IntVec2? GetOppositeTile(Wall wall, IntVec2 tile) {
        foreach (var t in wall.GetAdjacentTiles()) {
            if (t != tile) return t;
        }
        return null;
    }

    public Wall[] GetWallsSurroundingTile(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return Array.Empty<Wall>();

        return new[] {
            GetWallAtTile(tile, Side.North)!,
            GetWallAtTile(tile, Side.West)!,
            GetWallAtTile(tile, Side.South)!,
            GetWallAtTile(tile, Side.East)!
        };
    }

    public List<Wall> GetWallsSurroundingPoint(IntVec2 point) {
        var walls = new List<Wall>();

        if (point.X < 0 || point.X > Find.World.Width || point.Y < 0 || point.Y > Find.World.Height) return walls;

        if (GetWallAtTile(point,               Side.North) != null) walls.Add(GetWallAtTile(point,               Side.North)!);
        if (GetWallAtTile(point,               Side.West)  != null) walls.Add(GetWallAtTile(point,               Side.West)!);
        if (GetWallAtTile(point - IntVec2.One, Side.South) != null) walls.Add(GetWallAtTile(point - IntVec2.One, Side.South)!);
        if (GetWallAtTile(point - IntVec2.One, Side.East)  != null) walls.Add(GetWallAtTile(point - IntVec2.One, Side.East)!);

        return walls;
    }

    public bool IsWallSloped(Wall wall) {
        var (v1, v2) = wall.GetVertices();
        return !Find.World.Elevation.GetElevationAtPos(v1).NearlyEquals(Find.World.Elevation.GetElevationAtPos(v2));     
    }

    // TODO (optimisation): See if we can optimise these two functions
    private bool ShouldCheckForLoop(Wall wall) {
        var adjacent = wall.GetAdjacentWalls();
        if (adjacent.Length < 2) return false;

        if (wall.Orientation == Orientation.Horizontal) {
            if (adjacent.Any(w => w.WorldPos.X > wall.WorldPos.X) && adjacent.Any(w => w.WorldPos.X < wall.WorldPos.X)) {
                return true;
            }
        } else {
            if (adjacent.Any(w => w.WorldPos.Y > wall.WorldPos.Y) && adjacent.Any(w => w.WorldPos.Y < wall.WorldPos.Y)) {
                return true;
            }
        }

        return false;
    }
    
    private readonly HashSet<Wall> loopWalls = new ();
    private bool CheckForLoop(Wall startWall, Wall? currentWall = null, HashSet<Wall>? checkedWalls = null, int depth = 0) {
        currentWall  ??= startWall;
        if (checkedWalls == null) {
            loopWalls.Clear();
            checkedWalls = loopWalls;
        }
        
        // Expand current node
        checkedWalls.Add(currentWall);

        var found = false;
        foreach (var wall in currentWall.GetAdjacentWalls()) {
            if (wall == startWall && depth > 1) return true;

            if (!checkedWalls.Contains(wall)) {
                found = CheckForLoop(startWall, wall, checkedWalls, depth + 1);
            }
            if (found) break;
        }

        return found;
    }

    public void Serialise() {
        if (Find.SaveManager.Mode == SerialiseMode.Loading)
            Reset();
        
        Find.SaveManager.ArchiveValue("cols", ref cols);
        Find.SaveManager.ArchiveValue("rows", ref rows);
        
        if (Find.SaveManager.Mode == SerialiseMode.Loading)
            Setup();

        Find.SaveManager.ArchiveCollection("walls",
            GetAllWalls(),
            walls => walls.Select(wallData => GetWallByGridPos(wallData["gridPos"].ToObject<IntVec2>()))
        );
        
        UpdatePathfinding();
    }
}