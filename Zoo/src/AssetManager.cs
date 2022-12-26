using System.Text.Json;
using Raylib_cs;
using Zoo.util;

namespace Zoo;

public class AssetManager {
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true, IncludeFields = true };
        
    private readonly Dictionary<string, Texture2D> textureMap = new ();
    private readonly Dictionary<string, SpriteSheet> spriteSheetMap = new ();

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

            if (data.SpritePath != null)
                GetTexture(data.SpritePath);
            
            if (data.SpriteSheet?.TexturePath != null)
                LoadSpriteSheet(data.SpriteSheet.Value);
            
            Find.Registry.RegisterObject(path, data);
        }
        
        // Paths
        foreach (var path in Directory.EnumerateFiles("assets/data/paths", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            var      json = File.ReadAllText(path);
            PathData data = JsonSerializer.Deserialize<PathData>(json, JsonOpts)!;
            
            LoadSpriteSheet(data.SpriteSheet);
            
            Find.Registry.RegisterPath(path, data);
        }
        
        // Walls
        foreach (var path in Directory.EnumerateFiles("assets/data/walls", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            var      json = File.ReadAllText(path);
            WallData data = JsonSerializer.Deserialize<WallData>(json, JsonOpts)!;
            
            LoadSpriteSheet(data.SpriteSheet);
            
            Find.Registry.RegisterWall(path, data);
        }
    }

    public Texture2D GetTexture(string path) {
        if (!textureMap.ContainsKey(path)) {
            var texture = Raylib.LoadTexture(path);
            
            textureMap.Add(path, texture);
        }

        return textureMap[path];
    }

    public SpriteSheet? LoadSpriteSheet(SpriteSheet spritesheet) {
        if (spritesheet.TexturePath.NullOrEmpty()) return null;

        if (!spriteSheetMap.ContainsKey(spritesheet.TexturePath)) {
            spritesheet.Texture = GetTexture(spritesheet.TexturePath);
            spriteSheetMap.Add(spritesheet.TexturePath, spritesheet);
        }

        return spriteSheetMap[spritesheet.TexturePath];
    }
    
    public SpriteSheet? GetSpriteSheet(string texturePath) {
        if (texturePath.NullOrEmpty()) return null;
        if (!spriteSheetMap.ContainsKey(texturePath)) return null;
        
        return spriteSheetMap[texturePath];
    }
}