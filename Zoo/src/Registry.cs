using System.Numerics;
using Raylib_cs;
using Zoo.entities;

namespace Zoo;

public static class ObjectType {
    public static readonly string Foliage = "foliage";
    public static readonly string Building = "building";
    public static readonly string Consumable = "consumables";
    public static readonly string Misc = "misc"; 
}

public class ObjectData {
    public string      Id;
    public string      Name;
    public string      Type; // TODO: Translate this to ObjectType
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
    private readonly Dictionary<string, ObjectData>   objectRegistry = new();
    private readonly Dictionary<string, WallData>     wallRegistry   = new();
    private readonly Dictionary<string, FootPathData> pathRegistry   = new();

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
    
    public List<ObjectData> GetAllObjects() {
        return objectRegistry.Values.ToList();
    }
    public List<WallData> GetAllWalls() {
        return wallRegistry.Values.ToList();
    }
    public List<FootPathData> GetAllFootPaths() {
        return pathRegistry.Values.ToList();
    }
}