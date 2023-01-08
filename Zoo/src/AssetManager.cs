using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Raylib_cs;
using Zoo.defs;
using Zoo.util;

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
    private readonly Dictionary<string, Texture2D>             textureMap = new();
    private readonly Dictionary<Type, Dictionary<string, Def>> defMap     = new();
    private readonly Dictionary<string, Def>                   defMapFlat = new();

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
        
        // Data
        Queue<JsonObject> dataQueue = new ();
        foreach (var path in Directory.EnumerateFiles("assets/defs", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            try {
                var json = File.ReadAllText(path.Replace("\\", "/"));
                try {
                    var dataList = JsonSerializer.Deserialize<JsonObject[]>(json, JsonOpts)!;
                    foreach (var data in dataList) {
                        dataQueue.Enqueue(data);
                    }
                } catch {
                    var data = JsonSerializer.Deserialize<JsonObject>(json, JsonOpts)!;
                    dataQueue.Enqueue(data);
                }
            } catch (Exception e) {
                Debug.Error($"Failed to load json with path: {path}", e);
            }
        }
        
        while (dataQueue.Count > 0) {
            var data = dataQueue.Dequeue();
            
            var id = data["id"]?.ToString();
            if (id == null) {
                Debug.Error($"Failed to load data, missing id: {data}");
                continue;
            }
            
            Debug.Log($"Loading data with id {id}");
            
            if (defMapFlat.ContainsKey(id)) {
                Debug.Error($"Failed to load data {id}, id already exists");
                continue;
            }
            
            // Get type
            var typeString = data["class"]?.ToString();
            if (typeString == null) {
                Debug.Error($"Failed to load data {id}, no type specified");
                continue;
            }
            var type = Type.GetType("Zoo.defs." + typeString);
            if (type == null) {
                Debug.Error($"Failed to load data {id}, type {typeString} doesn't exist");
                continue;
            }

            // Instantiate
            Def instance;
            try {
                instance = (Def)JsonSerializer.Deserialize(data, type, JsonOpts);
            } catch (Exception e) {
                Debug.Error($"Failed to load data {id}, could not deserialize json", e);
                continue;
            }
            
            // Inheritance
            if (instance.InheritsFrom != null) {
                if (!defMapFlat.ContainsKey(instance.InheritsFrom)) {
                    // TODO: prevent loop
                    dataQueue.Enqueue(data);
                    continue;
                }
                var parent = defMapFlat[instance.InheritsFrom];
                instance.InheritFrom(parent);
            }
            
            // Register
            if (!defMap.ContainsKey(type))
                defMap.Add(type, new Dictionary<string, Def>());
            
            defMap[type].Add(id, instance);
            defMapFlat.Add(id, instance);
        }
    }

    public Texture2D GetTexture(string path) {
        if (!textureMap.ContainsKey(path)) {
            var texture = Raylib.LoadTexture(path);
            
            textureMap.Add(path, texture);
        }

        return textureMap[path];
    }

    public T? Get<T>(string id) where T : Def {
        if (!defMap.ContainsKey(typeof(T))) {
            Debug.Error($"Failed to get asset of type {typeof(T)}, no assets of that type have been loaded");
            return null;
        }

        if (!defMap[typeof(T)].ContainsKey(id)) {
            Debug.Error($"Failed to get asset of type {typeof(T)} with id {id}, no asset with that id has been loaded");
            return null;
        }

        return (T)defMap[typeof(T)][id];
    }
    
    public List<T>? GetAll<T>() where T : Def {
        if (!defMap.ContainsKey(typeof(T))) {
            Debug.Error($"Failed to get assets of type {typeof(T)}, no assets of that type have been loaded");
            return null;
        }
        
        // TODO: Cache this

        return defMap[typeof(T)].Values.Cast<T>().Where(def => !def.Abstract).ToList();
    }
}