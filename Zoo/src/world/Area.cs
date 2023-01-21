using Raylib_cs;
using Zoo.entities;
using Zoo.util;

namespace Zoo.world; 

public class Area {
    // Config
    public string Id     { get; set; }
    public Color  Colour { get; set; }

    // State
    public List<IntVec2>                   Tiles          { get; set; } = new();
    public Dictionary<Area, HashSet<Wall>> ConnectedAreas { get; }      = new();
    public bool                            IsZooArea      => Id == AreaManager.ZooArea;

    public Area(string id) {
        Id             = id;
        Colour         = new Color(Rand.Byte(), Rand.Byte(), Rand.Byte(), (byte)255);
    }
    
    public void AddAreaConnection(Area area, Wall door) {
        if (area == this) return;
        
        if (!ConnectedAreas.ContainsKey(area)) {
            ConnectedAreas.Add(area, new HashSet<Wall>());
        }
        ConnectedAreas[area].Add(door);
    }
    
    public void RemoveAreaConnection(Area area, Wall door) {
        if (area == this) return;
        if (!ConnectedAreas.ContainsKey(area)) return;
        
        ConnectedAreas[area].Remove(door);
        if (ConnectedAreas[area].Count == 0) {
            ConnectedAreas.Remove(area);
        }
    }

    public IEnumerable<Entity> GetContainedEntities(EntityTag tag = EntityTag.All) {
        foreach (var tile in Tiles) {
            foreach (var entity in Find.World.GetEntitiesAtTile(tile)) {
                if (entity.Tags.Contains(tag))
                    yield return entity;
            }
        }
    }
    
    
}