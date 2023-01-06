using System.Numerics;
using System.Text.Json.Nodes;
using Raylib_cs;
using Zoo.entities;
using Zoo.util;

namespace Zoo.world;

public enum Elevation : sbyte {
    Water = -1,
    Flat = 0,
    Hill = 1
}

public enum SlopeVariant {
    Flat,
    S, E, W, N,
    NW, NE, SW, SE,
    INW, INE, ISW, ISE,
    I1, I2
};

public class ElevationGrid : ISerialisable {
    // Constants
    private static readonly Color WaterColour = new Color(0, 76, 255, 204);
    
    // Config
    private int             rows;
    private int             cols;
    
    // State
    private Elevation[][]?  grid;
    private List<Vector2[]> waterPolygons = new ();
    private bool            isSetup       = false;
    
    public ElevationGrid(int rows, int cols) {
        this.rows = rows;
        this.cols = cols;
    }

    public void Setup(Elevation[][]? data = null) {
        if (isSetup) {
            Debug.Warn("Tried to setup ElevationGrid which was already setup");
            return;
        }
        
        Debug.Log("Setting up elevation grid");

        grid = data;
        if (grid == null) {
            grid = new Elevation[cols][];
            
            for (var i = 0; i < cols; i++) {
                grid[i] = new Elevation[rows];
                
                for (var j = 0; j < rows; j++) {
                    grid[i][j] = Elevation.Flat;
                }
            }
        }

        isSetup = true;

        RegenerateWaterMesh();
    }

    public void Reset() {
        if (!isSetup) {
            Debug.Warn("Tried to reset when ElevationGrid wasn't setup");
            return;
        }
        
        cols = 0;
        rows = 0;
        grid = null;
        isSetup = false;
    }

    public void Render() {
        foreach (var polygon in waterPolygons) {
            if (Find.Renderer.IsWorldPosOnScreen(polygon[0], World.WorldScale)) {
                Draw.DrawTriangleFan3D(polygon, WaterColour, (int)Depth.Water);
            }
        }
    }

    public bool CanElevate(IntVec2 gridPos, Elevation elevation) {
        if (!isSetup) {
            Debug.Warn("Elevation grid not setup");
            return false;
        }
        
        if (elevation == Elevation.Water) {
            // Check 4 surrounding wall slots for walls
            foreach(var wall in Find.World.Walls.GetSurroundingWalls(gridPos)) {
                if (wall.Exists) return false;
            }
            // Check 4 surrounding tiles
            foreach(var tile in GetSurroundingTiles(gridPos)) {
                if (!Find.World.IsPositionInMap(tile)) continue;
                
                // Check for Tile Objects
                var entity = Find.World.GetTileObjectAtTile(tile);
                if (entity?.GetComponent<TileObjectComponent>() is { Data.CanPlaceInWater: false }) return false;
                // Check for paths
                if (Find.World.FootPaths.GetFootPathAtTile(tile) is { Exists: true }) return false;
            }
        }

        if (elevation == Elevation.Hill) {
            // Check 4 surrounding tiles
            foreach(var tile in GetSurroundingTiles(gridPos)) {
                if (!Find.World.IsPositionInMap(tile)) continue;

                // Check for Tile Objects
                var entity = Find.World.GetTileObjectAtTile(tile);
                if (entity?.GetComponent<TileObjectComponent>() is { Data.CanPlaceOnSlopes: false }) return false;
                // Check for paths
                // TODO (fix): Figure out if points are being elevated in a way where this is valid
                if (Find.World.FootPaths.GetFootPathAtTile(tile) is { Exists: true }) return false;
            }
        }

        return true;
    }

    private readonly Dictionary<IntVec2, Elevation> oldPoints = new(); // For undo
    private bool SetElevation(IntVec2 gridPos, Elevation elevation) {
        if (!isSetup) {
            Debug.Warn("Elevation grid not setup");
            return false;
        }
        
        oldPoints.Clear();
        
        if (!CanElevate(gridPos, elevation)) return false;
        
        // Flatten surrounding terrain
        if (elevation != Elevation.Flat) {
            var failedToFlatten = false;
            foreach (var pos in Find.World.GetAdjacentTiles(gridPos, true)) {
                var elevationAtFlattenPoint = GetElevationAtGridPos(pos);
                if ((int)elevationAtFlattenPoint != -(int)elevation)
                    continue;
                    
                oldPoints.TryAdd(pos, elevationAtFlattenPoint);
                
                if (!SetElevation(pos, Elevation.Flat))
                    failedToFlatten = true;
            }

            if (failedToFlatten)
                return false;
        }
        
        oldPoints.TryAdd(gridPos, GetElevationAtGridPos(gridPos));
        
        grid[gridPos.X][gridPos.Y] = elevation;
        return true;
    }
    
    private readonly List<IntVec2>                  pointsToElevate   = new(); // Why did I do this?
    private readonly List<IntVec2>                  affectedCells     = new(); // So we can notify pathfinders about water
    private readonly Dictionary<IntVec2, Elevation> oldPointsInCircle = new(); // For undo
    public Dictionary<IntVec2, Elevation> SetElevationInCircle(Vector2 pos, float radius, Elevation elevation) {
        pointsToElevate.Clear();
        affectedCells.Clear();
        oldPointsInCircle.Clear();
        
        if (!isSetup) {
            Debug.Warn("Elevation grid not setup");
            return oldPointsInCircle;
        }
        if (!Find.World.IsPositionInMap(pos)) return oldPointsInCircle;

        var changed = false;

        for (var i = pos.X - radius; i <= pos.X + radius; i++) {
            for (var j = pos.Y - radius; j <= pos.Y + radius; j++) {
                var gridPos = new Vector2(i, j).Floor();
                if (!IsGridPosInGrid(gridPos)) continue;
                if (!JMath.PointInCircle(pos, radius, gridPos)) continue;
                if (GetElevationAtGridPos(gridPos) == elevation) continue;

                pointsToElevate.Add(gridPos);
                changed = true; // TODO (optimisation): Check canElevate to avoid heaps of mesh regeneration
            }
        }
        
        // Can we elevate tile objects that require flat terrain?
        // Get each elevate point
        // Collect the flatten points of each elevate point
        // Attempt to flatten and record results
        //     Recursive check for adjacent required points and see if they're also being flattened
        // For each elevate point check flatten results
        //     If all succeeded then
        //     Recursive check for adjacent required points being elevated
        //     elevate point

        // TODO: Figure out how to do path adjacency requirements (2 adjacent points or all 4)
        // What if a 4 banger raises a path but an adjacent path is a 3 banger?
        // Fuck my ass
        // Collect a list of all recursive points and then check each path around the point? How does that interact with gates and shit

        // This might be more complex than its worth tbh
        
        if (!changed) return oldPointsInCircle;
        
        foreach(var gridPos in pointsToElevate) {
            SetElevation(gridPos, elevation);
            
            foreach (var (gp, e) in oldPoints) {
                oldPointsInCircle.TryAdd(gp, e);
            }
            
            foreach (var tile in GetSurroundingTiles(gridPos)) {
                affectedCells.Add(tile);
            }
        }

        RegenerateWaterMesh();

        // Notify Biome Grid
        Messenger.Fire(EventType.ElevationUpdated, (pos, radius));
        
        // Notify Pathfinders
        if (elevation == Elevation.Water) {
            Messenger.Fire(EventType.PlaceSolid, affectedCells);
        }

        return oldPointsInCircle;
    }

    private HashSet<BiomeChunk> affectedBiomeChunks = new();
    public void SetElevationFromUndoData(Dictionary<IntVec2, Elevation> undoData) {
        affectedBiomeChunks.Clear();
        
        foreach (var (gp, e) in undoData) {
            grid[gp.X][gp.Y] = e;

            foreach (var tile in GetSurroundingTiles(gp)) {
                affectedBiomeChunks.Add(Find.World.Biomes.GetChunkAtTile(tile));
            }
        }
        
        RegenerateWaterMesh();

        foreach (var chunk in affectedBiomeChunks) {
            chunk.Regenerate();
        }
    }

    private void RegenerateWaterMesh() {
        waterPolygons.Clear();


        for (var i = 0; i < cols; i++) {
            for (var j = 0; j < rows; j++) {
                var tile = new IntVec2(i, j);
                if (GetTileBaseElevation(tile) >= 0) continue;

                var vertices = ElevationUtility.GetWaterVertices(GetTileSlopeVariant(tile));
                var average  = Vector2.Zero;
                var polygon = vertices.Select(v => {
                    var pos = new Vector2((i + v.X) * World.WorldScale, (j + v.Y) * World.WorldScale);
                    average += pos;
                    return pos;
                });
                
                // Join start and end
                polygon.Append(polygon.First());
                // Centre
                polygon.Prepend(average / vertices.Length);
                
                waterPolygons.Add(polygon.ToArray());
            }
        }
    }

    private Elevation GetElevationAtGridPos(IntVec2 gridPos) {
        if (!isSetup) {
            Debug.Warn("Elevation grid not setup");
            return Elevation.Flat;
        }
        if (!IsGridPosInGrid(gridPos)) return Elevation.Flat;
        
        return grid[gridPos.X][gridPos.Y];
    }

    public float GetTileBaseElevation(IntVec2 tile) {
        return JMath.Min(
            (int)GetElevationAtGridPos(tile),
            (int)GetElevationAtGridPos(tile + new IntVec2(0, 1)),
            (int)GetElevationAtGridPos(tile + new IntVec2(1, 0)),
            (int)GetElevationAtGridPos(tile + new IntVec2(1, 1))
        ) * ElevationUtility.ElevationHeight;
    }
    
    public SlopeVariant GetTileSlopeVariant(IntVec2 tile) {
        return ElevationUtility.GetSlopeVariant(
            GetElevationAtGridPos(tile),
            GetElevationAtGridPos(tile + new IntVec2(1, 0)),
            GetElevationAtGridPos(tile + new IntVec2(0, 1)),
            GetElevationAtGridPos(tile + new IntVec2(1, 1))
        );
    }

    public float GetElevationAtPos(Vector2 pos) {
        if (!IsGridPosInGrid(pos.Floor())) return 0;

        var baseElevation = GetTileBaseElevation(pos.Floor());
        var slopeVariant = GetTileSlopeVariant(pos.Floor());
        
        var slopeHeight = ElevationUtility.GetSlopeElevation(slopeVariant, pos);

        return baseElevation + slopeHeight;
    }

    private IntVec2[] GetSurroundingTiles(IntVec2 gridPos) {
        return new[] {
            gridPos + new IntVec2(-1, -1),
            gridPos + new IntVec2(-1, 0),
            gridPos + new IntVec2(0,  -1),
            gridPos + new IntVec2(0,  0)
        };
    }

    public bool IsPositionSloped(Vector2 pos) {
        return GetTileSlopeVariant(pos.Floor()) != SlopeVariant.Flat;
    }

    public bool IsPositionSlopeCorner(Vector2 pos) {
        var variant = GetTileSlopeVariant(pos.Floor());
        return variant != SlopeVariant.Flat &&
               variant != SlopeVariant.N    &&
               variant != SlopeVariant.S    &&
               variant != SlopeVariant.W    &&
               variant != SlopeVariant.E;
    }

    public bool IsPositionWater(Vector2 pos) {
        return IsTileWater(pos.Floor());
    }
    
    public bool IsTileWater(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return false;
        return GetTileBaseElevation(tile) < 0;
    }

    private bool IsGridPosInGrid(IntVec2 gridPos) {
        return gridPos.X >= 0 && gridPos.X < cols && gridPos.Y >= 0 && gridPos.Y < rows;
    }
    
    public void RenderDebug() {
        if (!isSetup) return;

        for (var i = 0; i < cols; i++) {
            for (var j = 0; j < rows; j++) {
                if (!Find.Renderer.IsWorldPosOnScreen(new Vector2(i, j))) continue;
                
                if (i < cols - 1) {
                    Debug.DrawLine(
                        new Vector2(i, j - (int)grid[i][j] * ElevationUtility.ElevationHeight),
                        new Vector2(i + 1, j - (int)grid[i + 1][j] * ElevationUtility.ElevationHeight),
                        Color.WHITE,
                        true
                    );
                }

                if (j < rows - 1) {
                    Debug.DrawLine(
                        new Vector2(i, j - (int)grid[i][j] * ElevationUtility.ElevationHeight),
                        new Vector2(i, j - (int)grid[i][j + 1] * ElevationUtility.ElevationHeight + 1),
                        Color.WHITE,
                        true
                    );
                }
            }
        }
    }

    public void Serialise() {
        if (Find.SaveManager.Mode == SerialiseMode.Loading)
            Reset();
        
        Find.SaveManager.ArchiveValue("cols", ref cols);
        Find.SaveManager.ArchiveValue("rows", ref rows);
        Find.SaveManager.ArchiveValue("data", 
            () => grid,
            data => Setup(data)
        );
    }
}