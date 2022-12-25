using System.Text.Json;
using Raylib_cs;

namespace Zoo;

public struct SpriteSheetData {
    public string SpritePath;
    public int    CellWidth;
    public int    CellHeight;
}

public class AssetManager {
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true, IncludeFields = true };
        
    private readonly Dictionary<string, Texture2D> textureMap = new ();

    public void LoadAssets() {
        // Textures
        foreach (var path in Directory.EnumerateFiles("assets/textures", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".png")) {
                continue;
            }

            GetTexture(path);
        }
        
        // Objects
        foreach (var path in Directory.EnumerateFiles("assets/data/objects", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            var        json = File.ReadAllText(path);
            ObjectData data = JsonSerializer.Deserialize<ObjectData>(json, JsonOpts)!;
            
            Game.Registry.RegisterObject(path, data);
        }
        
        // Paths
        foreach (var path in Directory.EnumerateFiles("assets/data/paths", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            var      json = File.ReadAllText(path);
            PathData data = JsonSerializer.Deserialize<PathData>(json, JsonOpts)!;
            
            Game.Registry.RegisterPath(path, data);
        }
        
        // Walls
        foreach (var path in Directory.EnumerateFiles("assets/data/walls", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            var      json = File.ReadAllText(path);
            WallData data = JsonSerializer.Deserialize<WallData>(json, JsonOpts)!;
            
            Game.Registry.RegisterWall(path, data);
        }
    }

    public Texture2D GetTexture(string path) {
        if (!textureMap.ContainsKey(path)) {
            var texture = Raylib.LoadTexture(path);
            
            textureMap.Add(path, texture);
        }

        return textureMap[path];
    }
}