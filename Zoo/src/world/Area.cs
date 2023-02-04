using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.util;

namespace Zoo.world; 

public class Area {
    // Config
    public string Id     { get; }
    public Color  Colour { get; }
    
    // Cache
    public  HashSet<Wall> Entrances  = new();
    public  HashSet<Wall> OuterWalls = new();

    // State
    private List<IntVec2> tiles      = new();

    // Properties
    public Dictionary<Area, HashSet<Wall>> ConnectedAreas { get; } = new();
    public bool                            IsZooArea      => Id == AreaManager.ZooArea;

    public List<IntVec2> Tiles {
        get => tiles;
    }

    public void SetTiles(List<IntVec2> value) {
        tiles = value;
        RecalculateOuterWalls();
        RecalculateAreaConnections();
    }

    public Area(string id) {
        Id     = id;
        Colour = new Color(Rand.Byte(), Rand.Byte(), Rand.Byte(), (byte)255);
    }
    
    public void AddAreaConnection(Area area, Wall door) {
        if (area == this) return;
        
        if (!ConnectedAreas.ContainsKey(area)) {
            ConnectedAreas.Add(area, new HashSet<Wall>());
        }
        ConnectedAreas[area].Add(door);

        RecalculateEntrances();
    }
    
    public void RemoveAreaConnection(Area area, Wall door) {
        if (area == this) return;
        if (!ConnectedAreas.ContainsKey(area)) return;
        
        ConnectedAreas[area].Remove(door);
        if (ConnectedAreas[area].Count == 0) {
            ConnectedAreas.Remove(area);
        }

        RecalculateEntrances();
    }

    public void RecalculateAreaConnections() {
        // Remove previous connections
        foreach (var (otherArea, doors) in ConnectedAreas) {
            foreach (var door in doors) {
                otherArea.RemoveAreaConnection(this, door);
                RemoveAreaConnection(otherArea, door);
            }
        }
        ConnectedAreas.Clear();

        RecalculateOuterWalls();
        foreach (var wall in OuterWalls) {
            if (!wall.IsDoor) continue;
            var otherArea = wall.GetAdjacentTiles().First(tile => tile.GetArea() != this).GetArea();

            AddAreaConnection(otherArea, wall);
            otherArea.AddAreaConnection(this, wall);
        }

        RecalculateEntrances();
    }

    public IEnumerable<Entity> GetContainedEntities(EntityTag tag = EntityTag.All) {
        foreach (var tile in Tiles) {
            foreach (var entity in Find.World.GetEntitiesAtTile(tile)) {
                if (entity.Tags.Contains(tag))
                    yield return entity;
            }
        }
    }

    public bool ReachableFrom(Area area) {
        if (this == area) return true;
        
        return !Find.World.Areas.BFS(area, this).NullOrEmpty();
    }

    public IntVec2 RandomTile() {
        return Tiles.RandomElement();
    }

    // TODO: Cache common predicates
    public IntVec2 RandomTileWhere(Func<IntVec2, bool> pred) {
        // First try a couple random tiles before doing a full search
        for (var i = 0; i < 5; i++) {
            var tile = Tiles.RandomElement();
            if (pred(tile)) return tile;
        }
        
        return Tiles.Where(pred).RandomElement();
    }

    private void RecalculateOuterWalls() {
        OuterWalls.Clear();
        foreach (var tile in Tiles) {
            foreach (var wall in Find.World.Walls.GetWallsSurroundingTile(tile)) {
                if (wall.GetAdjacentTiles().Any(tile => tile.GetArea() != this))
                    OuterWalls.Add(wall);
            }
        }
    }
    
    private void RecalculateEntrances() {
        Entrances = ConnectedAreas.Values.SelectMany(doors => doors).ToHashSet();
    }
}