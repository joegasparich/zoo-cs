using System.Numerics;
using System.Text.Json.Serialization;
using Zoo.world;

namespace Zoo;

public enum ObjectType {
    Foliage,
    Building,
    Consumable,
    Misc
}

public class ObjectData {
    public string      Id;
    public string      Name;
    public ObjectType  Type;
    public GraphicData GraphicData;
    public Vector2     Size             = Vector2.One;
    public bool        Solid            = true;
    public bool        CanPlaceOnSlopes = false;
    public bool        CanPlaceInWater  = false;
    public bool        CanRotate        = false;
}

public class WallData {
    public string      Id;
    public string      Name;
    public string      Type; // TODO
    public GraphicData GraphicData;
    public bool        Solid = true;
}

public class FootPathData {
    public string      Id;
    public string      Name;
    public GraphicData GraphicData;
}

public class Registry {
    // Collections
    private readonly Dictionary<string, ObjectData>   objectRegistry = new();
    private readonly Dictionary<string, WallData>     wallRegistry   = new();
    private readonly Dictionary<string, FootPathData> pathRegistry   = new();
    private readonly Dictionary<int, Biome>           biomeRegistry  = new();

    public void RegisterObject(ObjectData data) {
        objectRegistry.Add(data.Id, data);
        Debug.Log($"Registered object {data.Name}");
    }
    public void RegisterWall(WallData data) {
        wallRegistry.Add(data.Id, data);
        Debug.Log($"Registered wall {data.Name}");
    }
    public void RegisterPath(FootPathData data) {
        pathRegistry.Add(data.Id, data);
        Debug.Log($"Registered path {data.Name}");
    }
    public void RegisterBiome(Biome biome) {
        biomeRegistry.Add(biome.Id, biome);
        Debug.Log($"Registered biome {biome.Name}");
    }
    
    // TODO: Automate loading assets similar to DefOf?
    public ObjectData GetObject(string id) {
        return objectRegistry[id];
    }
    public WallData GetWall(string id) {
        return wallRegistry[id];
    }
    public FootPathData GetFootPath(string id) {
        return pathRegistry[id];
    }
    public Biome GetBiome(int id) {
        return biomeRegistry[id];
    }
    
    public List<ObjectData> GetAllObjects() {
        return objectRegistry.Values.ToList();
    }
    public List<WallData> GetAllWalls() {
        return wallRegistry.Values.ToList();
    }
    public List<FootPathData> GetAllFootPaths() {
        return pathRegistry.Values.ToList();
    }
    public List<Biome> GetAllBiomes() {
        return biomeRegistry.Values.ToList();
    }
}