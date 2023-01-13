using System.Collections.Concurrent;
using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.world;

internal struct Tile {
    public bool[] connections = { true, true, true, true, true, true, true, true };
    public Tile() {}
}

internal struct Node {
    public IntVec2 parent = new IntVec2(-1, -1);
    public float   gCost  = -1;
    public float   hCost  = -1;
    public float   fCost  = -1;
    public Node() {}
}

public class Pathfinder {
    // Constants
    private const bool Diagonals = false;
    
    // Config
    private int  cols;
    private int  rows;
    
    // State
    private Tile[,] tileGrid;
    private bool isSetup = false;

    private ConcurrentDictionary<CancellationTokenSource, Task<List<IntVec2>?>> pathRequests;

    public Pathfinder(int width, int height) {
        cols = width;
        rows = height;
        
        tileGrid = new Tile[cols, rows];
        pathRequests = new ();
        
        // Populate connections
        for (var i = 0; i < width; i++) {
            for (var j = 0; j < height; j++) {
                tileGrid[i, j] = new Tile();
            }
        }
        
        isSetup = true;
    }

    public void Reset() {
        tileGrid = null;
        pathRequests.Clear();
        isSetup  = false;
    }
    
    public (Task<List<IntVec2>?>, CancellationTokenSource)? RequestPath(IntVec2 start, IntVec2 end, AccessibilityType accessibility) {
        if (!isSetup) return null;

        var cancel = new CancellationTokenSource();
        var token  = cancel.Token;
        var task = Task.Run(() => {
            if (!isSetup) return null;
            var path = GetPath(start, end, accessibility);
            pathRequests.TryRemove(cancel, out _);
            return path;
        }, token);
        pathRequests[cancel] = task;
        return (task, cancel);
    }
    
    // https://www.geeksforgeeks.org/a-search-algorithm/
    private List<IntVec2>? GetPath(IntVec2 from, IntVec2 to, AccessibilityType accessibility) {
        if (!isSetup) return null;
        if (!Find.World.IsPositionInMap(from) || !Find.World.IsPositionInMap(to)) return null;
        if (!IsAccessible(from, accessibility) || !IsAccessible(to, accessibility)) return null;
        if (from == to) return null;

        var closedList  = new bool[cols, rows];
        var cellDetails = new Node[cols, rows];
        // Populate nodes
        for (var i = 0; i < cols; i++) {
            for (var j = 0; j < rows; j++) {
                cellDetails[i, j] = new Node();
            }
        }

        var (x, y) = from;
        cellDetails[x, y].fCost = 0;
        cellDetails[x, y].gCost = 0;
        cellDetails[x, y].hCost = 0;
        cellDetails[x, y].parent = from;

        var openList = new PriorityQueue<IntVec2, float>();

        openList.Enqueue(from, cellDetails[x, y].fCost);

        while (openList.Count > 0) {
            (x, y) = openList.Dequeue();
            closedList[x, y] = true;

            foreach(var neighbour in GetNeighbours(new IntVec2(x, y), accessibility)) {
                var (nx, ny) = neighbour;

                if (neighbour == to) {
                    // Destination found!
                    cellDetails[nx, ny].parent = new IntVec2(x, y);
                    return ReconstructPath(cellDetails, to);
                }

                // If the successor is already on the closed list then ignore it
                if (closedList[nx, ny]) continue;

                var gNew = cellDetails[x, y].gCost + Find.World.GetTileCost(new IntVec2(x, y), accessibility);
                var hNew = CalculateHValue(neighbour, to);
                var fNew = gNew + hNew;

                // If it isn’t on the open list, add it to the open list. Make the current square the parent of this
                // square. Record the f, g, and h costs of the square cell
                //             OR
                // If it is on the open list already, check to see if this path to that square is better, using
                // 'f' cost as the measure.
                if (!cellDetails[nx, ny].fCost.NearlyEquals(-1.0f) && cellDetails[nx, ny].fCost <= fNew) continue;

                openList.Enqueue(new IntVec2(nx, ny), fNew);
                // Update the details of this cell
                cellDetails[nx,ny].gCost = gNew;
                cellDetails[nx,ny].fCost = fNew;
                cellDetails[nx,ny].hCost = hNew;
                cellDetails[nx,ny].parent = new IntVec2(x, y);
            }
        }

        Debug.Log($"Failed to find route from {from} to {to}");
        return null;
    }
    
    public void CancelPathRequest(CancellationTokenSource cancel) {
        if (!isSetup) return;
        Debug.Assert(cancel != null);
        if (pathRequests.ContainsKey(cancel)) {
            cancel.Cancel();
            pathRequests.TryRemove(cancel, out _);
        }
    }

    private List<IntVec2> ReconstructPath(Node[,] tileDetails, IntVec2 dest) {
        var path = new List<IntVec2>();
        
        path.Add(dest);
        var (x, y) = dest;
        while (tileDetails[x, y].parent != new IntVec2(x, y)) {
            var nextNode = tileDetails[x, y].parent;
            path.Add(nextNode);
            x = nextNode.X;
            y = nextNode.Y;
        }
        
        path.Reverse();
        return path;
    }

    public void SetAccessibility(IntVec2 tile, Direction direction, bool accessible) {
        if (!isSetup) return;
        if (!Find.World.IsPositionInMap(tile)) return;
        tileGrid[tile.X, tile.Y].connections[(int) direction] = accessible;
    }

    private bool IsAccessible(IntVec2 tilePos, AccessibilityType accessibility) {
        if (!isSetup) return false;
        if (!Find.World.IsPositionInMap(tilePos)) return false;
        var (x, y) = tilePos;
        var tile = tileGrid[x, y];
        
        if (Find.World.GetTileCost(tilePos, accessibility) <= 0) return false;
        
        // Make sure at least one direction is accessible
        if (y > 0 &&                    tileGrid[x,     y - 1].connections[(int)Direction.S])  return true;
        if (x < cols-1 &&               tileGrid[x + 1, y]    .connections[(int)Direction.W])  return true;
        if (y < rows-1 &&               tileGrid[x,     y + 1].connections[(int)Direction.N])  return true;
        if (x > 0 &&                    tileGrid[x - 1, y]    .connections[(int)Direction.E])  return true;

        if (!Diagonals) return false;
        
        if (x < cols-1 && y > 0 &&      tileGrid[x + 1, y - 1].connections[(int)Direction.SW]) return true;
        if (x < cols-1 && y < rows-1 && tileGrid[x + 1, y + 1].connections[(int)Direction.NW]) return true;
        if (x > 0 && y < rows-1 &&      tileGrid[x - 1, y + 1].connections[(int)Direction.NE]) return true;
        if (x > 0 && y > 0 &&           tileGrid[x - 1, y - 1].connections[(int)Direction.SE]) return true;

        return false;
    }
    
    private List<IntVec2> GetNeighbours(IntVec2 tilePos, AccessibilityType accessibility) {
        if (!isSetup) return new List<IntVec2>();
        if (!Find.World.IsPositionInMap(tilePos)) return new List<IntVec2>();
        var (x, y) = tilePos;
        var width  = tileGrid.GetLength(0);
        var height = tileGrid.GetLength(1);
        var tile   = tileGrid[x, y];

        var connections = new List<IntVec2>();

        if (y > 0                       && IsAccessible(new IntVec2(x, y - 1), accessibility)     && tile.connections[(int)Direction.N])  connections.Add(new IntVec2(x, y - 1));
        if (x < width-1                 && IsAccessible(new IntVec2(x + 1, y), accessibility)     && tile.connections[(int)Direction.E])  connections.Add(new IntVec2(x + 1, y));
        if (y < height-1                && IsAccessible(new IntVec2(x, y + 1), accessibility)     && tile.connections[(int)Direction.S])  connections.Add(new IntVec2(x, y + 1));
        if (x > 0                       && IsAccessible(new IntVec2(x - 1, y), accessibility)     && tile.connections[(int)Direction.W])  connections.Add(new IntVec2(x - 1, y));

        if (!Diagonals) return connections;
        
        if (x < width-1 && y > 0        && IsAccessible(new IntVec2(x + 1, y - 1), accessibility) && tile.connections[(int)Direction.NE]) connections.Add(new IntVec2(x + 1, y - 1));
        if (x < width-1 && y < height-1 && IsAccessible(new IntVec2(x + 1, y + 1), accessibility) && tile.connections[(int)Direction.SE]) connections.Add(new IntVec2(x + 1, y + 1));
        if (x > 0 && y < height-1       && IsAccessible(new IntVec2(x - 1, y + 1), accessibility) && tile.connections[(int)Direction.SW]) connections.Add(new IntVec2(x - 1, y + 1));
        if (x > 0 && y > 0              && IsAccessible(new IntVec2(x - 1, y - 1), accessibility) && tile.connections[(int)Direction.NW]) connections.Add(new IntVec2(x - 1, y - 1));

        return connections;
    }

    private static float CalculateHValue(IntVec2 a, IntVec2 b) {
        return a.DistanceSquared(b);
    }

    public void DrawDebugGrid(AccessibilityType accessibility = AccessibilityType.NoWater) {
        for (var i = 0; i < cols; i++) {
            for (var j = 0; j < rows; j++) {
                if (!Find.Renderer.IsWorldPosOnScreen(new Vector2(i, j))) continue;
                if (!IsAccessible(new IntVec2(i, j), accessibility)) continue;

                foreach(var neighbour in GetNeighbours(new IntVec2(i, j), accessibility)) {
                    var (nx, ny) = neighbour;
                    Debug.DrawLine(new Vector2(i + 0.5f, j + 0.5f), new Vector2(nx + 0.5f, ny + 0.5f), Color.BLUE, true);
                }
            }
        }
    }
}