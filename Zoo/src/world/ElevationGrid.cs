using System.Numerics;
using Raylib_cs;
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

public class ElevationGrid {
    private static readonly Color WaterColour = new Color(0, 76, 255, 204);
    
    private int             rows;
    private int             cols;
    private Elevation[,]    grid;
    private List<Vector2[]> waterPolygons = new ();
    private bool            isSetup       = false;
    
    public ElevationGrid(int rows, int cols) {
        this.rows = rows;
        this.cols = cols;
        grid = new Elevation[rows, cols];
    }

    public void Setup() {
        if (isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Tried to setup ElevationGrid which was already setup");
            return;
        }
        
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Setting up biome grid");

        for (var i = 0; i < cols; i++) {
            for (var j = 0; j < rows; j++) {
                grid[i, j] = Elevation.Flat;
            }
        }

        grid[3, 3] = Elevation.Water;
        
        isSetup = true;

        GenerateWaterMesh();
    }

    public void Reset() {
        if (!isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Tried to reset when ElevationGrid wasn't setup");
            return;
        }
        
        cols = 0;
        rows = 0;
        grid = null;
        isSetup = false;
    }

    public void Render() {
        foreach (var polygon in waterPolygons) {
            Draw.DrawTriangleFan3D(polygon, WaterColour, Depth.Water.ToInt());
        }
    }

    public bool CanElevate(IntVec2 gridPos, Elevation elevation) {
        if (!isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Elevation grid not setup");
            return false;
        }
        
        if (elevation == Elevation.Water) {
            // Check for walls
            // Check for tile objects
            // Check for paths
        }

        if (elevation == Elevation.Hill) {
            // Check for tile objects
            // Check for paths
        }

        return true;
    }

    private bool SetElevation(IntVec2 gridPos, Elevation elevation) {
        if (!isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Elevation grid not setup");
            return false;
        }
        
        if (!CanElevate(gridPos, elevation)) return false;
        
        // Flatten surrounding terrain
        if (elevation != Elevation.Flat) {
            var failedToFlatten = false;
            foreach (var pos in Find.World.GetAdjacentTiles(gridPos, true)) {
                if (GetElevationAtGridPos(pos).ToInt() != -elevation.ToInt())
                    continue;
                    
                if (!SetElevation(pos, Elevation.Flat))
                    failedToFlatten = true;
            }

            if (failedToFlatten)
                return false;
        }
        
        grid[gridPos.X, gridPos.Y] = elevation;
        return true;
    }
    
    private List<IntVec2> pointsToElevate = new ();
    private List<IntVec2> affectedCells   = new();
    public void SetElevationInCircle(Vector2 pos, Elevation elevation, int radius) {
        if (!isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Elevation grid not setup");
            return;
        }
        if (!Find.World.IsPositionInMap(pos)) return;

        bool changed = false;
        
        pointsToElevate.Clear();

        for (float i = pos.X - radius; i <= pos.X + radius; i++) {
            for (float j = pos.Y - radius; j <= pos.Y + radius; j++) {
                var gridPos = new Vector2(i, j).Floor();
                if (!Find.World.IsPositionInMap(gridPos)) continue;

                if (JMath.PointInCircle(pos, radius, gridPos)) {
                    if (GetElevationAtGridPos(gridPos) != elevation) {
                        pointsToElevate.Add(gridPos);
                        changed = true; // TODO: Check canElevate to avoid heaps of mesh regeneration
                    }
                }
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
        
        if (!changed) return;
        
        affectedCells.Clear();
        
        foreach(var gridPos in pointsToElevate) {
            SetElevation(gridPos, elevation);
            
            foreach (var tile in GetSurroundingTiles(gridPos)) {
                affectedCells.Add(tile);
            }
        }

        GenerateWaterMesh();

        Messenger.Fire(EventType.ElevationUpdated, (pos, radius));
    }

    private void GenerateWaterMesh() {
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
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Elevation grid not setup");
            return Elevation.Flat;
        }
        if (!Find.World.IsPositionInMap(gridPos)) return Elevation.Flat;
        
        return grid[gridPos.X, gridPos.Y];
    }

    public float GetTileBaseElevation(IntVec2 tile) {
        return JMath.Min(
            GetElevationAtGridPos(tile).ToInt(),
            GetElevationAtGridPos(tile + new IntVec2(0, 1)).ToInt(),
            GetElevationAtGridPos(tile + new IntVec2(1, 0)).ToInt(),
            GetElevationAtGridPos(tile + new IntVec2(1, 1)).ToInt()
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
        if (!Find.World.IsPositionInMap(pos)) return 0;

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
}