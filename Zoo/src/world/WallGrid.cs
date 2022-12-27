using System.Numerics;
using Raylib_cs;
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

public struct Wall {
    public WallData?   Data           = null;
    public Orientation Orientation    = Orientation.Vertical;
    public Vector2     WorldPos       = default;
    public IntVec2     GridPos        = default;
    public bool        Exists         = false;
    public bool        Indestructable = false;
    public bool        IsDoor         = false;
    public Color       OverrideColour = Color.WHITE;
    public Wall() {}
}

public class WallGrid {
    private bool     isSetup = false;
    private Wall[][] grid;
    private int      cols;
    private int      rows;
    
    public WallGrid(int cols, int rows) {
        this.cols = cols;
        this.rows = rows;
    }

    public void Setup() {
        if (isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Tried to setup WallGrid which was already setup");
            return;
        }
        
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Setting up wall grid");
        
        grid = new Wall[cols * 2 + 1][];
        
        for (var i = 0; i < cols * 2 + 1; i++) {
            var orientation = (Orientation)(i % 2);
            grid[i] = new Wall[rows + orientation.ToInt()];
            
            for (var j = 0; j < rows + orientation.ToInt(); j++) {
                var worldPos = WallUtility.WallToWorldPosition(new IntVec2(i, j), orientation);
                grid[i][j] = new() {
                    Data = null,
                    Orientation = orientation,
                    WorldPos = worldPos,
                    GridPos = new IntVec2(i, j),
                };
            }
        }

        isSetup = true;
        
        PlaceWallAtTile(Find.Registry.GetWall(WALLS.IRON_FENCE), new IntVec2(3, 3), Side.North);
    }

    public void Reset() {
        if (!isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Tried to reset when WallGrid wasn't setup");
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
            for (var j = 0; j < rows + orientation.ToInt(); j++) {
                var wall = grid[i][j];
                if (!wall.Exists) continue;
                
                var (spriteIndex, elevation) = WallUtility.GetSpriteInfo(wall);
                var pos = orientation == Orientation.Horizontal ? new Vector2(i / 2.0f, j) : new Vector2(i / 2.0f, j + 1);
                pos -= new Vector2(0.5f, 2.0f + elevation); // Offset cell size
                var spriteSheet = wall.Data!.Value.SpriteSheet;
                
                Find.Renderer.Blit(
                    texture: spriteSheet.Texture,
                    pos: pos * World.WorldScale,
                    depth: Find.Renderer.GetDepth(wall.WorldPos.Y),
                    scale: new Vector2(1, 2) * World.WorldScale,
                    source: spriteSheet.GetCellBounds(spriteIndex),
                    color: wall.OverrideColour
                );
                
                wall.OverrideColour = Color.WHITE;
            }
        }
    }

    public Wall? PlaceWallAtTile(WallData data, IntVec2 tile, Side side) {
        if (!IsWallPosInMap(tile, side)) return null;
        if (GetWallAtTile(tile, side)!.Value.Exists) return null;
        
        var (x, y, orientation) = WallUtility.GetGridPosition(tile, side);

        grid[x][y] = new Wall() {
            Data           = data,
            Orientation    = orientation,
            WorldPos       = WallUtility.WallToWorldPosition(new IntVec2(x, y), orientation),
            GridPos        = new IntVec2(x, y),
            Exists         = true,
            Indestructable = false,
        };
        
        var wall = grid[x][y];
        
        // updatePathfindingAtWall(wall);
        //
        // if (shouldCheckForLoop(wall) && checkForLoop(&wall)) {
        //     Root::zoo()->world->areaManager->formAreas(grid.at(x).at(y));
        // }

        return wall;
    }

    public void DeleteWall(Wall wall) {
        DeleteWallAtTile(wall.WorldPos.Floor(), wall.Orientation == Orientation.Horizontal ? Side.North : Side.West);
    }

    public void DeleteWallAtTile(IntVec2 tile, Side side) {
        if (!IsWallPosInMap(tile, side)) return;
        
        // Get grid pos
        var (x, y, orientation) = WallUtility.GetGridPosition(tile, side);
        if (!grid[x][y].Exists || grid[x][y].Indestructable) return;
        
        // Set to empty wall
        grid[x][y] = new Wall() {
            Data           = null,
            Orientation    = orientation,
            WorldPos       = WallUtility.WallToWorldPosition(new IntVec2(x, y), orientation),
            GridPos        = new IntVec2(x, y),
            Exists         = false,
            Indestructable = false,
        };
        
        // auto& wall = grid[x][y];
        //
        // updatePathfindingAtWall(wall);
        // Root::zoo()->world->areaManager->joinAreas(wall);
    }

    public void PlaceDoor(Wall wall) {
        if (!wall.Exists) return;
        
        wall.IsDoor = true;
        
        // auto adjacentTiles = getAdjacentTiles(*wall);
        // if (adjacentTiles.size() < 2) return;
        //
        // auto areaA = Root::zoo()->world->areaManager->getAreaAtPosition(adjacentTiles[0]);
        // auto areaB = Root::zoo()->world->areaManager->getAreaAtPosition(adjacentTiles[1]);
        //
        // areaA->addAreaConnection(areaB, wall);
        // areaB->addAreaConnection(areaA, wall);
    }

    public void RemoveDoor(Wall wall) {
            if (!wall.Exists) return;
        
        wall.IsDoor = false;
        
        // auto adjacentTiles = getAdjacentTiles(*wall);
        // if (adjacentTiles.size() < 2) return;
        //
        // auto areaA = Root::zoo()->world->areaManager->getAreaAtPosition(adjacentTiles[0]);
        // auto areaB = Root::zoo()->world->areaManager->getAreaAtPosition(adjacentTiles[1]);
        //
        // areaA->removeAreaConnection(areaB, wall);
        // areaB->removeAreaConnection(areaA, wall);
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

    private List<Wall> adjacentWalls = new ();
    public Wall[] GetAdjacentWalls(Wall wall) {
        adjacentWalls.Clear();
        var x = wall.GridPos.X;
        var y = wall.GridPos.Y;

        if (wall.Orientation == Orientation.Horizontal) {
            if (GetWallByGridPos(new IntVec2(x - 2, y))     is { Exists: true }) adjacentWalls.Add(grid[x - 2][y]);
            if (GetWallByGridPos(new IntVec2(x + 2, y))     is { Exists: true }) adjacentWalls.Add(grid[x + 2][y]);
            if (GetWallByGridPos(new IntVec2(x - 1, y))     is { Exists: true }) adjacentWalls.Add(grid[x - 1][y]);
            if (GetWallByGridPos(new IntVec2(x + 1, y))     is { Exists: true }) adjacentWalls.Add(grid[x + 1][y]);
            if (GetWallByGridPos(new IntVec2(x - 1, y - 1)) is { Exists: true }) adjacentWalls.Add(grid[x - 1][y - 1]);
            if (GetWallByGridPos(new IntVec2(x + 1, y - 1)) is { Exists: true }) adjacentWalls.Add(grid[x + 1][y - 1]);
        } else {
            if (GetWallByGridPos(new IntVec2(x - 1, y))     is { Exists: true }) adjacentWalls.Add(grid[x - 1][y]);
            if (GetWallByGridPos(new IntVec2(x + 1, y))     is { Exists: true }) adjacentWalls.Add(grid[x + 1][y]);
            if (GetWallByGridPos(new IntVec2(x - 1, y + 1)) is { Exists: true }) adjacentWalls.Add(grid[x - 1][y + 1]);
            if (GetWallByGridPos(new IntVec2(x + 1, y + 1)) is { Exists: true }) adjacentWalls.Add(grid[x + 1][y + 1]);
            if (GetWallByGridPos(new IntVec2(x, y + 1))     is { Exists: true }) adjacentWalls.Add(grid[x][y + 1]);
            if (GetWallByGridPos(new IntVec2(x, y - 1))     is { Exists: true }) adjacentWalls.Add(grid[x][y - 1]);
        }

        return adjacentWalls.ToArray();
    }

    private List<IntVec2> adjacentTiles = new ();
    public IntVec2[] GetAdjacentTiles(Wall wall) {
        adjacentTiles.Clear();

        var (x, y) = wall.WorldPos;
        
        if (wall.Orientation == Orientation.Horizontal) {
            if (Find.World.IsPositionInMap(new Vector2(x - 0.5f, y - 1.0f))) adjacentTiles.Add(new Vector2(x - 0.5f, y - 1.0f).Floor());
            if (Find.World.IsPositionInMap(new Vector2(x - 0.5f, y))) adjacentTiles.Add(new Vector2(x - 0.5f, y).Floor());
        } else {
            if (Find.World.IsPositionInMap(new Vector2(x - 1.0f, y - 0.5f))) adjacentTiles.Add(new Vector2(x - 1.0f, y - 0.5f).Floor());
            if (Find.World.IsPositionInMap(new Vector2(x, y - 0.5f))) adjacentTiles.Add(new Vector2(x, y - 0.5f).Floor());
        }

        return adjacentTiles.ToArray();
    }

    public IntVec2? GetOppositeTile(Wall wall, IntVec2 tile) {
        foreach (var t in GetAdjacentTiles(wall)) {
            if (t != tile) return t;
        }
        return null;
    }

    public Wall[] GetSurroundingWalls(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return Array.Empty<Wall>();

        return new[] {
            GetWallAtTile(tile, Side.North)!.Value,
            GetWallAtTile(tile, Side.West)!.Value,
            GetWallAtTile(tile, Side.South)!.Value,
            GetWallAtTile(tile, Side.East)!.Value
        };
    }

    public bool IsWallSloped(Wall wall) {
        var (v1, v2) = wall.GetVertices();
        return JMath.RoundToInt(Find.World.Elevation.GetElevationAtPos(v1)) != JMath.RoundToInt(Find.World.Elevation.GetElevationAtPos(v2));     
    }
}