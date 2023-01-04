using Raylib_cs;
using Zoo.util;

namespace Zoo.world; 

public class Area {
    // Config
    public string Id     { get; set; }
    public Color  Colour { get; set; }

    // State
    public List<IntVec2>                   Tiles          { get; set; }
    public Dictionary<Area, HashSet<Wall>> ConnectedAreas { get; set; }

    public Area(string id) {
        Id             = id;
        Colour         = new Color(Rand.randByte(), Rand.randByte(), Rand.randByte(), (byte)255);
        Tiles          = new List<IntVec2>();
        ConnectedAreas = new Dictionary<Area, HashSet<Wall>>();
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
}