using Raylib_cs;
using Zoo.util;

namespace Zoo.world; 

public class AreaManager {
    private const string ZooArea = "Zoo;";
    
    private Dictionary<string, Area> areas = new();
    private Dictionary<string, Area> tileAreaMap = new();
    
    private bool isSetup = false;

    public void Setup() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Setting up areas");
        
        if (isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Areas already setup");
            return;
        }
        
        var zooArea  = new Area(ZooArea);
        zooArea.Tiles = FloodFill(new IntVec2(1, 1)); // TODO: change this to zoo entrance
        areas.Add(zooArea.Id, zooArea);
        foreach (var tile in zooArea.Tiles) {
            tileAreaMap.Add(tile.ToString(), zooArea);
        }

        isSetup = true;
    }

    public void Reset() {
        if (!isSetup) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "Areas not setup");
            return;
        }
        
        areas.Clear();
        tileAreaMap.Clear();

        isSetup = false;
    }

    public void FormAreas(Wall placedWall) {
        if (!placedWall.Exists) return;

        List<IntVec2> areaATiles = new();
        List<IntVec2> areaBTiles = new();
        var           startTiles = Find.World.Walls.GetAdjacentTiles(placedWall);
        
        // ! Does not handle situations where final wall is placed on the edge of the map
        // Current solution is to ensure that the map is already surrounded by walls
        if (startTiles.Length < 2) return;
        
        areaATiles = FloodFill(startTiles[0]);
        areaBTiles = FloodFill(startTiles[1]);
        
        var oldArea = GetAreaAtTile(startTiles[0]);
        
        // Return if areas weren't formed properly (false positive in loop check)
        if (areaATiles.Count + areaBTiles.Count > oldArea.Tiles.Count) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, "False positive in loop check");
            return;
        }
        
        var newArea = new Area(Guid.NewGuid().ToString());

        var larger = areaATiles.Count >= areaBTiles.Count ? areaATiles : areaBTiles;
        var smaller = areaATiles.Count < areaBTiles.Count ? areaATiles : areaBTiles;

        // Old area gets the larger area (maintains zoo area)
        oldArea.Tiles = larger;
        foreach (var tile in larger) {
            tileAreaMap[tile.ToString()] = oldArea;
        }
        newArea.Tiles = smaller;
        foreach (var tile in smaller) {
            tileAreaMap[tile.ToString()] = newArea;
        }
        
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, $"Registered new area with size {newArea.Tiles.Count}");
        
        areas.Add(newArea.Id, newArea);
    }

    public void JoinAreas(Wall removedWall) {
        var tiles = Find.World.Walls.GetAdjacentTiles(removedWall);
        if (tiles.Length < 2) return;

        var areaA = GetAreaAtTile(tiles[0]);
        var areaB = GetAreaAtTile(tiles[1]);

        if (areaA == areaB) return;
        
        // If one of the areas is the main zoo area then ensure we keep it
        if (areaB.Id == ZooArea) {
            (areaA, areaB) = (areaB, areaA);
        }
        
        areaA.Tiles.AddRange(areaB.Tiles);
        foreach (var tile in areaB.Tiles) {
            tileAreaMap[tile.ToString()] = areaA;
        }
        foreach (var areaConnection in areaB.ConnectedAreas) {
            var (area, doors) = areaConnection;
            if (area == areaA) continue;
            
            foreach (var door in doors) {
                areaA.AddAreaConnection(area, door);
                area.AddAreaConnection(areaA, door);
            }
        }
        areas.Remove(areaB.Id);
    }
    
    public List<Area> GetAreas() {
        return new List<Area>(areas.Values);
    }
    
    public Area? GetAreaByID(string id) {
        return !areas.ContainsKey(id) ? null : areas[id];
    }
    
    public Area? GetAreaAtTile(IntVec2 tile) {
        if (!Find.World.IsPositionInMap(tile)) return null;
        
        return !tileAreaMap.ContainsKey(tile.ToString()) ? null : tileAreaMap[tile.ToString()];
    }
    
    private readonly HashSet<IntVec2> tileSet   = new();
    private readonly Stack<IntVec2>   openTiles = new();
    private List<IntVec2> FloodFill(IntVec2 startTile) {
        tileSet.Clear();
        openTiles.Clear();

        tileSet.Add(startTile);
        openTiles.Push(startTile);

        while (openTiles.Any()) {
            var currentTile = openTiles.Pop();
            var neighbours = Find.World.GetAccessibleAdjacentTiles(currentTile);
            
            foreach(var neighbour in neighbours) {
                if (tileSet.Contains(neighbour)) continue;
                
                tileSet.Add(neighbour);
                openTiles.Push(neighbour);
            }
        }
        
        return new List<IntVec2>(tileSet);
    }

    private readonly Dictionary<string, int>    bfsDist  = new();
    private readonly Dictionary<string, string> bfsPrev  = new();
    private readonly Queue<Area>                bfsQueue = new();
    public List<Area> BFS(Area start, Area end) {
        bfsDist.Clear();
        bfsPrev.Clear();
        bfsQueue.Clear();
        
        bfsDist.Add(start.Id, 0);
        bfsQueue.Enqueue(start);

        var curDist = 1;
        while (bfsQueue.Any()) {
            var current = bfsQueue.Dequeue();

            foreach (var (neighbour, _) in current.ConnectedAreas) {
                if (bfsDist.ContainsKey(neighbour.Id) && curDist >= bfsDist[neighbour.Id]) continue;
                
                bfsPrev.Add(neighbour.Id, current.Id);
                bfsDist.Add(neighbour.Id, curDist);
                bfsQueue.Enqueue(neighbour);
                
                // Found end, construct path
                if (neighbour == end) {
                    var path = new List<Area>();
                    path.Add(end);
                    var n = end.Id;
                    while (bfsPrev.ContainsKey(n) && n != start.Id) {
                        n = bfsPrev[n];
                        path.Add(GetAreaByID(n)!);
                    }
                    path.Reverse();
                    return path;
                }
            }

            curDist++;
        }

        return new List<Area>();
    }
}