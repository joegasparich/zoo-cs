using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.world; 

public class AreaManager {
    // Constants
    public const string ZooArea = "Zoo;";
    
    // State
    private Dictionary<string, Area> areas = new();
    private Dictionary<string, Area> tileAreaMap = new();
    
    private bool isSetup = false;

    public void Setup(IntVec2 startTile) {
        Debug.Log("Setting up areas");
        
        if (isSetup) {
            Debug.Warn("Areas already setup");
            return;
        }

        RegenerateAreas(startTile);

        isSetup = true;
    }

    public void Reset() {
        if (!isSetup) {
            Debug.Warn("Areas not setup");
            return;
        }
        
        areas.Clear();
        tileAreaMap.Clear();

        isSetup = false;
    }

    private void RegenerateAreas(IntVec2 startTile) {
        // Form areas
        FormZooArea(startTile);
        
        for (var i = 0; i < Find.World.Width; i++) {
            for (var j = 0; j < Find.World.Height; j++) {
                var tile = new IntVec2(i, j);
                if (tileAreaMap.ContainsKey(tile.ToString())) {
                    continue;
                }
                
                var area = new Area(Guid.NewGuid().ToString());
                area.SetTiles(FloodFill(tile));
                areas.Add(area.Id, area);
                foreach (var areaTile in area.Tiles) {
                    tileAreaMap.Add(areaTile.ToString(), area);
                }
            }
        }

        // Form connections
        foreach (var wall in Find.World.Walls.GetAllWalls()) {
            if (!wall.IsDoor) continue;
            var adjacentTiles = wall.GetAdjacentTiles();
            if (adjacentTiles.Count < 2) continue;

            var areaA = Find.World.Areas.GetAreaAtTile(adjacentTiles[0]);
            var areaB = Find.World.Areas.GetAreaAtTile(adjacentTiles[1]);
        
            areaA.AddAreaConnection(areaB, wall);
            areaB.AddAreaConnection(areaA, wall);
        }
        
        foreach (var area in areas.Values) {
            Messenger.Fire(EventType.AreaCreated, area);
        }
    }

    public void FormZooArea(IntVec2 entrance) {
        var zooArea = new Area(ZooArea);
        zooArea.SetTiles(FloodFill(entrance));
        areas.Add(zooArea.Id, zooArea);
        foreach (var tile in zooArea.Tiles) {
            tileAreaMap.Add(tile.ToString(), zooArea);
        }
        
        Messenger.Fire(EventType.AreaCreated, zooArea);
    }

    public void FormAreasAtWall(Wall placedWall) {
        if (!placedWall.Exists) return;

        List<IntVec2> areaATiles = new();
        List<IntVec2> areaBTiles = new();
        var           startTiles = placedWall.GetAdjacentTiles();
        
        // ! Does not handle situations where final wall is placed on the edge of the map
        // Current solution is to ensure that the map is already surrounded by walls
        if (startTiles.Count < 2) return;
        
        areaATiles = FloodFill(startTiles[0]);
        areaBTiles = FloodFill(startTiles[1]);
        
        var oldArea = GetAreaAtTile(startTiles[0]);
        
        // Return if areas weren't formed properly (false positive in loop check)
        if (areaATiles.Count + areaBTiles.Count > oldArea.Tiles.Count) {
            Debug.Warn("False positive in loop check");
            return;
        }
        
        var newArea = new Area(Guid.NewGuid().ToString());

        // Old area gets the larger area (maintains zoo area)
        var oldTiles = areaATiles.Count >= areaBTiles.Count ? areaATiles : areaBTiles;
        var newTiles = areaATiles.Count < areaBTiles.Count ? areaATiles : areaBTiles;

        // Ensure that zoo entrance is in zoo area
        if (newTiles.Contains(Find.Zoo.Entrance))
            (oldTiles, newTiles) = (newTiles, oldTiles);

        oldArea.SetTiles(oldTiles);
        foreach (var tile in oldTiles) {
            tileAreaMap[tile.ToString()] = oldArea;
        }
        newArea.SetTiles(newTiles);
        foreach (var tile in newTiles) {
            tileAreaMap[tile.ToString()] = newArea;
        }
        
        Messenger.Fire(EventType.AreaUpdated, oldArea);
        Messenger.Fire(EventType.AreaCreated, newArea);
        
        Debug.Log($"Registered new area with size {newArea.Tiles.Count}");
        
        areas.Add(newArea.Id, newArea);
    }

    public void JoinAreasAtWall(Wall removedWall) {
        var tiles = removedWall.GetAdjacentTiles();
        if (tiles.Count < 2) return;

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
        areas.Remove(areaB.Id);

        areaA.RecalculateAreaConnections();
        
        Messenger.Fire(EventType.AreaUpdated, areaA);
        Messenger.Fire(EventType.AreaRemoved, areaB);
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
    
    public void RenderDebugAreaGrid() {
        foreach (var (_, area) in areas) {
            foreach (var tile in area.Tiles) {
                if (!Find.Renderer.IsWorldPosOnScreen(tile)) continue;
                
                Debug.DrawRect(tile, IntVec2.One, area.Colour.WithAlpha(0.5f), true);
            }

            foreach (var (_, doors) in area.ConnectedAreas) {
                foreach (var door in doors) {
                    if (!Find.Renderer.IsWorldPosOnScreen(door.WorldPos)) continue;
                    
                    var tiles = door.GetAdjacentTiles();
                    var (p1, p2) = door.GetVertices();
                    if (door.Orientation == Orientation.Horizontal) {
                        Debug.DrawPolygon(new List<Vector2>{p1, p2, tiles[0].Centre()}, tiles[1].GetArea().Colour, true);
                        Debug.DrawPolygon(new List<Vector2>{p1, tiles[1].Centre(), p2}, tiles[0].GetArea().Colour, true);
                    } else {
                        Debug.DrawPolygon(new List<Vector2>{p1, tiles[0].Centre(), p2}, tiles[1].GetArea().Colour, true);
                        Debug.DrawPolygon(new List<Vector2>{p1, p2, tiles[1].Centre()},   tiles[0].GetArea().Colour, true);
                    }
                }
            }
        }
    }
}