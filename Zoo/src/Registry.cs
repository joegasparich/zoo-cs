using System.ComponentModel;
using System.Numerics;
using Raylib_cs;

namespace Zoo;

public static class ObjectType {
    public static readonly string Foliage = "foliage";
    public static readonly string Building = "building";
    public static readonly string Consumable = "consumables";
    public static readonly string Misc = "misc"; 
}

public struct ObjectData {
    public string       AssetPath;
    public string       Name;
    public string?      SpritePath;
    public Texture2D?   Sprite;
    public SpriteSheet? SpriteSheet;
    public string       Type;
    public Vector2      Origin;
    public Vector2      Size;
    public bool         Solid;
    public bool         CanPlaceOnSlopes;
    public bool         CanPlaceInWater;
}

public struct WallData {
    public string      AssetPath;
    public string      Name;
    public string      Type;
    public bool        Solid;
    public SpriteSheet SpriteSheet;
}

public struct PathData {
    public string      AssetPath;
    public string      Name;
    public SpriteSheet SpriteSheet;
}

public class Registry {
    private readonly Dictionary<string, ObjectData> objectRegistry = new ();
    private readonly Dictionary<string, WallData>   wallRegistry   = new ();
    private readonly Dictionary<string, PathData>   pathRegistry   = new();

    public void RegisterObject(string assetPath, ObjectData data) {
        objectRegistry.Add(assetPath, data);
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, $"Registered object {data.Name}");
    }
    public void RegisterWall(string assetPath, WallData data) {
        wallRegistry.Add(assetPath, data);
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, $"Registered wall {data.Name}");
    }
    public void RegisterPath(string assetPath, PathData data) {
        pathRegistry.Add(assetPath, data);
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, $"Registered path {data.Name}");
    }
    
    // TODO: Automate loading assets similar to DefOf?
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