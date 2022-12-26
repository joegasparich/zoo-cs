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
    private const float ElevationHeight = 0.5f;

    private int          rows;
    private int          cols;
    private Elevation[,] grid;
    private bool         isSetup = false;
    
    public ElevationGrid(int rows, int cols) {
        this.rows = rows;
        this.cols = cols;
        grid = new Elevation[rows, cols];
    }

    public void Setup() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Setting up biome grid");

        for (var i = 0; i < cols; i++) {
            for (var j = 0; j < rows; j++) {
                grid[i, j] = Elevation.Flat;
            }
        }
        
        isSetup = true;
    }

    public void Reset() {
        cols = 0;
        rows = 0;
        grid = null;
        isSetup = false;
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

    public bool SetElevation(IntVec2 gridPos, Elevation elevation) {
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
        
        
    }

    public Elevation GetElevationAtGridPos(IntVec2 gridPos) {
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
        ) * ElevationHeight;
    }
    
    private SlopeVariant GetTileSlopeVariant(IntVec2 tile) {
        var nw = GetElevationAtGridPos(tile);
        var ne = GetElevationAtGridPos(tile + new IntVec2(1, 0));
        var sw = GetElevationAtGridPos(tile + new IntVec2(0, 1));
        var se = GetElevationAtGridPos(tile + new IntVec2(1, 1));

        if (nw == ne && nw == sw && nw == se) return SlopeVariant.Flat;
        if (nw == ne && sw == se && nw < sw) return SlopeVariant.N;
        if (nw == sw && ne == se && nw > ne) return SlopeVariant.E;
        if (nw == ne && sw == se && nw > sw) return SlopeVariant.S;
        if (nw == sw && ne == se && nw < ne) return SlopeVariant.W;
        if (se == sw && se == ne && nw > se) return SlopeVariant.SE;
        if (sw == nw && sw == se && ne > sw) return SlopeVariant.SW;
        if (ne == nw && ne == se && sw > ne) return SlopeVariant.NE;
        if (nw == sw && nw == ne && se > nw) return SlopeVariant.NW;
        if (se == sw && se == ne && nw < se) return SlopeVariant.INW;
        if (sw == nw && sw == se && ne < sw) return SlopeVariant.INE;
        if (ne == nw && ne == se && sw < ne) return SlopeVariant.ISW;
        if (nw == sw && nw == ne && se < nw) return SlopeVariant.ISE;
        if (nw == se && sw == ne && nw > ne) return SlopeVariant.I1;
        if (nw == se && sw == ne && nw < ne) return SlopeVariant.I2;

        // How did you get here?
        return SlopeVariant.Flat;
    }

    public float GetElevationAtPos(Vector2 pos) {
        if (!Find.World.IsPositionInMap(pos)) return 0;

        var relX = pos.X % 1f;
        var relY = pos.Y % 1f;
        var baseElevation = GetTileBaseElevation(pos.Floor());
        var slopeVariant = GetTileSlopeVariant(pos.Floor());
        
        // Tried to come up with a formula instead of using enums but I'm too dumb
        switch (slopeVariant) {
            case SlopeVariant.Flat:
                return baseElevation;
            case SlopeVariant.N:
                return baseElevation + ElevationHeight * relY;
            case SlopeVariant.S:
                return baseElevation + ElevationHeight * (1 - relY);
            case SlopeVariant.W:
                return baseElevation + ElevationHeight * relX;
            case SlopeVariant.E:
                return baseElevation + ElevationHeight * (1 - relX);
            case SlopeVariant.SE:
                return baseElevation + ElevationHeight * MathF.Max(1 - relX - relY, 0.0f);
            case SlopeVariant.SW:
                return baseElevation + ElevationHeight * MathF.Max(1 - (1 - relX) - relY, 0.0f);
            case SlopeVariant.NE:
                return baseElevation + ElevationHeight * MathF.Max(1 - relX - (1 - relY), 0.0f);
            case SlopeVariant.NW:
                return baseElevation + ElevationHeight * MathF.Max(1 - (1 - relX) - (1 - relY), 0.0f);
            case SlopeVariant.ISE:
                return baseElevation + ElevationHeight * MathF.Min(1 - relX + 1 - relY, 1.0f);
            case SlopeVariant.ISW:
                return baseElevation + ElevationHeight * MathF.Min(relX + 1 - relY, 1.0f);
            case SlopeVariant.INE:
                return baseElevation + ElevationHeight * MathF.Min(1 - relX + relY, 1.0f);
            case SlopeVariant.INW:
                return baseElevation + ElevationHeight * MathF.Min(relX + relY, 1.0f);
            case SlopeVariant.I1:
                return baseElevation + ElevationHeight * MathF.Max(1 - relX - relY, 1 - (1 - relX) - (1 - relY));
            case SlopeVariant.I2:
                return baseElevation + ElevationHeight * MathF.Max(1 - (1 - relX) - relY, 1 - relX - (1 - relY));
            default:
                // You shouldn't be here
                return 0;
        }
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