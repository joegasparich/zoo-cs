using System.Text.Json;
using Raylib_cs;
using Zoo.util;

namespace Zoo;

public class AssetManager {
    // Config
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true, IncludeFields = true };
        
    // State
    private readonly Dictionary<string, Texture2D> textureMap = new ();

    public void LoadAssets() {
        // Textures
        foreach (var path in Directory.EnumerateFiles("assets/textures", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".png")) {
                continue;
            }

            GetTexture(path.Replace("\\", "/"));
        }
        
        // Objects
        foreach (var path in Directory.EnumerateFiles("assets/data/objects", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            var        json = File.ReadAllText(path.Replace("\\", "/"));
            ObjectData data = JsonSerializer.Deserialize<ObjectData>(json, JsonOpts)!;

            Find.Registry.RegisterObject(data);
        }
        
        // Paths
        foreach (var path in Directory.EnumerateFiles("assets/data/paths", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            var json = File.ReadAllText(path.Replace("\\", "/"));
            FootPathData data = JsonSerializer.Deserialize<FootPathData>(json, JsonOpts)!;

            Find.Registry.RegisterPath(data);
        }
        
        // Walls
        foreach (var path in Directory.EnumerateFiles("assets/data/walls", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }
            
            var      json = File.ReadAllText(path.Replace("\\", "/"));
            WallData data = JsonSerializer.Deserialize<WallData>(json, JsonOpts)!;

            Find.Registry.RegisterWall(data);
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