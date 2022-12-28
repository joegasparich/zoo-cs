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

            GetTexture(path.Replace("\\", "/"));
        }
        
        // Objects
        foreach (var path in Directory.EnumerateFiles("assets/data/objects", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }
            var finalPath = path.Replace("\\", "/");

            var        json = File.ReadAllText(finalPath);
            ObjectData data = JsonSerializer.Deserialize<ObjectData>(json, JsonOpts)!;

            data.AssetPath = finalPath;
            if (data.SpritePath != null)
                data.Sprite = GetTexture(data.SpritePath);
            if (data.SpriteSheet?.TexturePath != null)
                data.SpriteSheet = LoadSpriteSheet(data.SpriteSheet.Value);
            
            Find.Registry.RegisterObject(finalPath, data);
        }
        
        // Paths
        foreach (var path in Directory.EnumerateFiles("assets/data/paths", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }
            var finalPath = path.Replace("\\", "/");

            var      json = File.ReadAllText(finalPath);
            FootPathData data = JsonSerializer.Deserialize<FootPathData>(json, JsonOpts)!;

            data.AssetPath = finalPath;
            data.SpriteSheet = LoadSpriteSheet(data.SpriteSheet) ?? data.SpriteSheet;
            
            Find.Registry.RegisterPath(finalPath, data);
        }
        
        // Walls
        foreach (var path in Directory.EnumerateFiles("assets/data/walls", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }
            var finalPath = path.Replace("\\", "/");
            
            var      json = File.ReadAllText(finalPath);
            WallData data = JsonSerializer.Deserialize<WallData>(json, JsonOpts)!;

            data.AssetPath   = finalPath;
            data.SpriteSheet = LoadSpriteSheet(data.SpriteSheet) ?? data.SpriteSheet;
            
            Find.Registry.RegisterWall(finalPath, data);
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