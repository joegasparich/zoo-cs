using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Raylib_cs;
using Zoo.defs;
using Zoo.util;

namespace Zoo;

// TODO: Find a home for this
public static class TexturePaths {
    public static readonly string Keeper = "assets/textures/keeper.png";
}

// TODO: Find a way to directly resolve def references instead of using a wrapper
public class DefJsonConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type type) {
        if (!type.IsGenericType)
            return false;

        return type.GetGenericTypeDefinition() == typeof(DefRef<>);
    }

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options) {
        Type defType = type.GetGenericArguments()[0];

        return (JsonConverter)Activator.CreateInstance(
            typeof(DefJsonConverter<>).MakeGenericType(defType),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: new object[] { options },
            culture: null)!;
    }
    
    private class DefJsonConverter<T> : JsonConverter<DefRef<T>> where T : Def {
        public DefJsonConverter(JsonSerializerOptions options) {}
        
        public override DefRef<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();

            var name = reader.GetString()!;

            return new DefRef<T>(name);
        }
        public override void Write(Utf8JsonWriter writer, DefRef<T> value, JsonSerializerOptions options) {}
    }
}


public class AssetManager {
    // Config
    private static readonly JsonSerializerOptions JsonOpts = new() {
        Converters = {
            new JsonStringEnumConverter(),
            new DefJsonConverterFactory()
        },
        PropertyNameCaseInsensitive = true, 
        IncludeFields               = true,
        
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
        
        // Defs
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
                instance = (Def)JsonSerializer.Deserialize(data, type, JsonOpts);
            } catch (Exception e) {
                Debug.Error($"Failed to load def {id}, could not deserialize json", e);
                continue;
            }
            
            // Inheritance
            if (instance.InheritsFrom != null) {
                if (!dataQueue.Any(d => d["id"]?.ToString() == instance.InheritsFrom) && !defMapFlat.ContainsKey(instance.InheritsFrom)) {
                    Debug.Error($"Failed to load def {id}, could not find parent {instance.InheritsFrom}");
                    continue;
                }
                if (!defMapFlat.ContainsKey(instance.InheritsFrom)) {
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

        DefUtility.LoadDefOfs();
    }

    public Texture2D GetTexture(string path) {
        if (!textureMap.ContainsKey(path)) {
            var texture = Raylib.LoadTexture(path);
            
            textureMap.Add(path, texture);
        }

        return textureMap[path];
    }

    public Def? Get(Type type, string id) {
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

    public T? Get<T>(string id) where T : Def {
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
    
    public List<T>? GetAll<T>() where T : Def {
        if (!defMap.ContainsKey(typeof(T))) {
            Debug.Error($"Failed to get defs of type {typeof(T)}, no defs of that type have been loaded");
            return null;
        }
        
        // TODO: Cache this

        return defMap[typeof(T)].Values.Cast<T>().Where(def => !def.Abstract).ToList();
    }
}