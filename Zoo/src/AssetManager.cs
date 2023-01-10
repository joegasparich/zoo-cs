using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using Zoo.defs;
using Zoo.util;

namespace Zoo;

// TODO: Find a home for this
public static class TexturePaths {
    public static readonly string Keeper = "assets/textures/keeper.png";
}

public class AssetManager {
    private JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings() {
        Converters = new List<JsonConverter>() {
            new CompJsonConverter()
        },
        ContractResolver = new CustomContractResolver()
    });
        
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
        
        // Defs
        Queue<JObject> dataQueue = new ();
        foreach (var path in Directory.EnumerateFiles("assets/defs", "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json")) {
                continue;
            }

            // TODO: Ensure ids exist in here somewhere
            
            try {
                var json = File.ReadAllText(path.Replace("\\", "/"));
                try {
                    var dataList = JsonConvert.DeserializeObject<List<JObject>>(json);
                    // var dataList = JsonSerializer.Deserialize<JsonObject[]>(json, JsonOpts)!;
                    foreach (var data in dataList) {
                        dataQueue.Enqueue(data);
                    }
                } catch {
                    var data = JsonConvert.DeserializeObject<JObject>(json)!;
                    dataQueue.Enqueue(data);
                }
            } catch (Exception e) {
                Debug.Error($"Failed to load json with path: {path}", e);
            }
        }
        
        // Resolve inheritance
        Queue<JObject>              resolvedQueue = new();
        Dictionary<string, JObject> resolvedDict  = new();
        while (dataQueue.Count > 0) {
            var data = dataQueue.Dequeue();
            if (!data.ContainsKey("abstract"))
                data.Add("abstract", false);
            if (data.TryGetValue("inherits", out var inherits)) {
                var inheritsId = inherits.Value<string>();
                if (resolvedDict.ContainsKey(inheritsId)) {
                    var merged = (JObject)resolvedDict[inheritsId].DeepClone();
                    merged.Merge(data);
                    data = merged;
                } else {
                    // TODO: Prevent looping
                    dataQueue.Enqueue(data);
                }
            }
            resolvedQueue.Enqueue(data);
            resolvedDict.Add(data["id"].ToString(), data);
        }
        
        // Deserialize
        while (resolvedQueue.Count > 0) {
            var data = resolvedQueue.Dequeue();
             
            var id = data["id"]?.ToString();
            if (id == null) {
                Debug.Error($"Failed to load def, missing id: {data}");
                continue;
            }
            
            if (defMapFlat.ContainsKey(id)) {
                Debug.Error($"Failed to load def {id}, id already exists");
                continue;
            }
            
            // Get type
            var typeString = data["class"]?.ToString();
            if (typeString == null) {
                Debug.Error($"Failed to load def {id}, no type specified");
                continue;
            }
            var type = Type.GetType("Zoo.defs." + typeString);
            if (type == null) {
                Debug.Error($"Failed to load def {id}, type {typeString} doesn't exist");
                continue;
            }
            
            Debug.Log($"Loading {typeString} with id {id}");
        
            // Instantiate
            Def instance;
            try {
                instance = (Def)data.ToObject(type, serializer)!;
            } catch (Exception e) {
                Debug.Error($"Failed to load def {id}, could not deserialize json", e);
                continue;
            }
             
            // Register
            if (!defMap.ContainsKey(type))
                defMap.Add(type, new Dictionary<string, Def>());
                
            defMap[type].Add(id, instance);
            defMapFlat[id] = instance;
        }

        DefUtility.LoadDefOfs();
    }

    public Texture2D GetTexture(string path) {
        if (!textureMap.ContainsKey(path)) {
            var texture = Raylib.LoadTexture(path);
            
            textureMap.Add(path, texture);
        }

        return textureMap[path];
    }

    public Def? GetDef(string id) {
        if (!defMapFlat.ContainsKey(id)) {
            Debug.Error($"Failed to get def with id {id}, no defs of that type have been loaded");
            return null;
        }
        
        return defMapFlat[id];
    }

    public Def? GetDef(Type type, string id) {
        if (!defMap.ContainsKey(type)) {
            Debug.Error($"Failed to get def of type {type}, no defs of that type have been loaded");
            return null;
        }

        if (!defMap[type].ContainsKey(id)) {
            Debug.Error($"Failed to get def of type {type} with id {id}, no def with that id has been loaded");
            return null;
        }

        return defMap[type][id];
    }

    public T? GetDef<T>(string id) where T : Def {
        if (!defMap.ContainsKey(typeof(T))) {
            Debug.Error($"Failed to get def of type {typeof(T)}, no defs of that type have been loaded");
            return null;
        }

        if (!defMap[typeof(T)].ContainsKey(id)) {
            Debug.Error($"Failed to get def of type {typeof(T)} with id {id}, no def with that id has been loaded");
            return null;
        }

        return (T)defMap[typeof(T)][id];
    }
    
    public List<T>? GetAllDefs<T>() where T : Def {
        if (!defMap.ContainsKey(typeof(T))) {
            Debug.Error($"Failed to get defs of type {typeof(T)}, no defs of that type have been loaded");
            return null;
        }
        
        // TODO: Cache this

        return defMap[typeof(T)].Values.Cast<T>().Where(def => !def.Abstract).ToList();
    }
}