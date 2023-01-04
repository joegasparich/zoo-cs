using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using Raylib_cs;
using Zoo.util;

namespace Zoo;

public enum SerialiseMode {
    Saving,
    Loading
}

public class CustomEncoder : TextEncoderSettings {
    public override void AllowCharacter(char character) {
        base.AllowCharacter(character);
    }
}

public class SaveManager {
    private readonly JsonSerializerOptions serializeOptions = new() {
        IncludeFields = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    
    public JsonObject CurrentSaveNode { get; set; }
    public SerialiseMode Mode { get; set; }
    
    public void NewGame() {
        Debug.Log("Starting new game");
        Find.SceneManager.LoadScene(new ZooScene());
    }

    public void SaveGame(string filePath) {
        Debug.Log("Saving game");
        
        JsonObject saveData = new JsonObject();
        Mode            = SerialiseMode.Saving;
        CurrentSaveNode = saveData;
        Game.Serialise();
        
        // Save json object to file
        string json = JsonSerializer.Serialize(saveData, serializeOptions);
        File.WriteAllText(filePath, json);
    }
    
    public void LoadGame(string filePath) {
        Debug.Log("Loading game");
        
        var json = File.ReadAllText(filePath);
        var saveData = JsonSerializer.Deserialize<JsonObject>(json, serializeOptions);

        Mode            = SerialiseMode.Loading;
        CurrentSaveNode = saveData;
        Game.Serialise();
    }

    // TODO: Allow serialising regular values
    public JsonObject Serialise(ISerialisable value) {
        var node = new JsonObject();
        
        Mode            = SerialiseMode.Saving;
        CurrentSaveNode = node;
        
        value.Serialise();
        return node;
    }

    public T? Deserialise<T>(JsonNode node) {
        return node.Deserialize<T>(serializeOptions);
    }
    
    public void ArchiveValue<T>(string label, ref T? value) {
        switch (Mode) {
            case SerialiseMode.Saving:
                CurrentSaveNode.Add(label, JsonSerializer.SerializeToNode(value, serializeOptions));
                break;
            case SerialiseMode.Loading:
                if (CurrentSaveNode[label] == null)
                    break;
                
                value = JsonSerializer.Deserialize<T>(CurrentSaveNode[label]!.ToJsonString(), serializeOptions);
                break;
        }
    }

    public void ArchiveValue<T>(string label, Func<T> get, Action<T?>? set) {
        switch (Mode) {
            case SerialiseMode.Saving: {
                var value = get();
                ArchiveValue(label, ref value);
                break;
            }
            case SerialiseMode.Loading: {
                if (set == null) break;
                
                var value = default(T);
                ArchiveValue(label, ref value);
                set(value);
                break;
            }
        }
    }
    
    public void ArchiveCustom(string label, Func<JsonNode> get, Action<JsonNode> set) {
        switch (Mode) {
            case SerialiseMode.Saving: {
                CurrentSaveNode[label] = get();
                break;
            }
            case SerialiseMode.Loading: {
                set(CurrentSaveNode[label]);
                break;
            }
        }
    }
    
    public void ArchiveDeep(string label, ISerialisable value) {
        var parent = CurrentSaveNode;
        switch (Mode) {
            case SerialiseMode.Saving:
                CurrentSaveNode = new JsonObject();
                value.Serialise();
                parent[label] = CurrentSaveNode;
                break;
            case SerialiseMode.Loading:
                CurrentSaveNode = parent[label]!.AsObject();
                value.Serialise();
                break;
        }
        CurrentSaveNode = parent;
    }

    public void ArchiveCollection(string label, IEnumerable<ISerialisable> collection, Func<JsonArray, IEnumerable<ISerialisable>> select) {
        var parent = CurrentSaveNode;
        switch (Mode) {
            case SerialiseMode.Saving: {
                var array = new JsonArray();
                foreach (var value in collection) {
                    CurrentSaveNode = new JsonObject();
                    value.Serialise();
                    array.Add(CurrentSaveNode);
                }
                parent[label] = array;
                break;
            }
            case SerialiseMode.Loading: {
                var array = parent[label]!.AsArray();
                var i = 0;
                foreach (var value in select(array)) {
                    CurrentSaveNode = array[i++].AsObject(); 
                    value.Serialise();
                }
                break;
            }
        }
        CurrentSaveNode = parent;
    }

    // public void ArchiveListDeep<T>(string label, List<T> list) where T : ISerialisable, new() {
    //     var parent = CurrentSaveNode;
    //
    //     switch (Mode) {
    //         case SerialiseMode.Saving:
    //             var saveData  = new JsonArray();
    //             
    //             foreach (var item in list) {
    //                 var node = new JsonObject();
    //                 CurrentSaveNode = node;
    //                 item.Serialise();
    //                 saveData.Add(node);
    //             }
    //             parent[label] = saveData;
    //             break;
    //         case SerialiseMode.Loading:
    //             foreach (var item in parent[label].AsArray()) {
    //                 var newItem = new T();
    //                 CurrentSaveNode = (JsonObject)item;
    //                 newItem.Serialise();
    //                 list.Add(newItem);
    //             }
    //             break;
    //     }
    //     CurrentSaveNode = parent;
    // }

    public JsonObject SerialiseToNode(ISerialisable value) {
        var node = new JsonObject();
        Find.SaveManager.CurrentSaveNode = node;
        Find.SaveManager.Mode            = SerialiseMode.Saving;
        Find.SaveManager.ArchiveDeep("key", value);

        return node;
    }

    public void DeserialiseFromNode(ISerialisable value, JsonObject node) {
        Find.SaveManager.CurrentSaveNode = node;
        Find.SaveManager.Mode            = SerialiseMode.Loading;
        Find.SaveManager.ArchiveDeep("key", value);
    }
}