using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo;

public class AssetManager {
    // Config
    private static readonly JsonSerializerOptions JsonOpts = new() {
        Converters ={
            new JsonStringEnumConverter()
        },
        PropertyNameCaseInsensitive = true, 
        IncludeFields = true
    };
        
    // State
    private readonly Dictionary<string, Texture2D> textureMap = new ();

    public void LoadAssets() {
        // Textures
        foreach (var path in Directory.EnumerateFiles("assets/textures", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".png")) {
                continue;
            }

            try {
                GetTexture(path.Replace("\\", "/"));
            }
            catch (Exception e) {
                Debug.Error($"Failed to load texture with path: {path}", e);
                textureMap.Remove(path);
            }
        }
        
        // Objects
        foreach (var path in Directory.EnumerateFiles("assets/data/objects", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            try {
                var        json = File.ReadAllText(path.Replace("\\", "/"));
                ObjectData data = JsonSerializer.Deserialize<ObjectData>(json, JsonOpts)!;

                Find.Registry.RegisterObject(data);
            }
            catch (Exception e) {
                Debug.Error($"Failed to load object with path: {path}", e);
            }
        }
        
        // Paths
        foreach (var path in Directory.EnumerateFiles("assets/data/paths", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            try {
                var json = File.ReadAllText(path.Replace("\\", "/"));
                FootPathData data = JsonSerializer.Deserialize<FootPathData>(json, JsonOpts)!;

                Find.Registry.RegisterPath(data);
            }
            catch (Exception e) {
                Debug.Error($"Failed to load footpath with path: {path}", e);
            }
        }
        
        // Walls
        foreach (var path in Directory.EnumerateFiles("assets/data/walls", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }
            
            try {
                var      json = File.ReadAllText(path.Replace("\\", "/"));
                WallData data = JsonSerializer.Deserialize<WallData>(json, JsonOpts)!;

                Find.Registry.RegisterWall(data);
            }
            catch (Exception e) {
                Debug.Error($"Failed to load wall with path: {path}", e);
            }
        }
        
        // Biomes
        try {
           var json = File.ReadAllText("assets/data/biomes.json");
            Biome[] data = JsonSerializer.Deserialize<Biome[]>(json, JsonOpts)!;
            
            foreach (var biome in data) {
                biome.Colour.a = 255;
                Find.Registry.RegisterBiome(biome);
            }
        }
        catch (Exception e) {
            Debug.Error($"Failed to load biomes", e);
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