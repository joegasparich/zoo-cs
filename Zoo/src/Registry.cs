using System.ComponentModel;
using System.Numerics;
using Raylib_cs;

namespace Zoo;

public enum ObjectType {
    [Description("foliage")] Foliage,
    [Description("building")] Building,
    [Description("consumable")] Consumable,
    [Description("misc")] Misc
}

public struct ObjectData {
    public string     assetPath;
    public string     name;
    public string     spritePath;
    public string     spriteSheetPath;
    public ObjectType type;
    public Vector2    pivot;
    public Vector2    size;
    public bool       solid;
    public bool       canPlaceOnSlopes;
    public bool       canPlaceInWater;
}

public struct WallData {
    public string assetPath;
    public string name;
    public string type;
    public bool   solid;
    public string spriteSheetPath;
}

public struct PathData {
    public string assetPath;
    public string name;
    public string spriteSheetPath;
}

public class Registry {
    private Dictionary<string, ObjectData> objectRegistry = new ();
    private Dictionary<string, WallData>   wallRegistry   = new ();
    private Dictionary<string, PathData>   pathRegistry   = new();

    public void RegisterObject(string assetPath, ObjectData data) {
        objectRegistry.Add(assetPath, data);
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, $"Registered object {data.name}");
    }
    public void RegisterWall(string assetPath, WallData data) {
        wallRegistry.Add(assetPath, data);
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, $"Registered wall {data.name}");
    }
    public void RegisterPath(string assetPath, PathData data) {
        pathRegistry.Add(assetPath, data);
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, $"Registered path {data.name}");
    }
    
    public ObjectData GetObject(string assetPath) {
        return objectRegistry[assetPath];
    }
    public WallData GetWall(string assetPath) {
        return wallRegistry[assetPath];
    }
    public PathData GetPath(string assetPath) {
        return pathRegistry[assetPath];
    }
    
    public List<ObjectData> GetAllObjects() {
        return objectRegistry.Values.ToList();
    }
    public List<WallData> GetAllWalls() {
        return wallRegistry.Values.ToList();
    }
    public List<PathData> GetAllPaths() {
        return pathRegistry.Values.ToList();
    }
}