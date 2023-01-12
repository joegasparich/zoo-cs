using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zoo.util;

namespace Zoo;

public enum SerialiseMode {
    Saving,
    Loading
}

public class SaveFile {
    public string Name;
    public string Path;
}

public class SaveManager {
    // Constants
    private const string SaveDir         = "saves/";
    private const string DefaultSaveName = "save";
    
    // State
    public JObject    CurrentSaveNode;
    public SerialiseMode Mode;
    
    public void NewGame(int width, int height) {
        Debug.Log("Starting new game");
        var scene = new Scene_Zoo();
        scene.Zoo.Width  = width;
        scene.Zoo.Height = height;
        
        Find.SceneManager.LoadScene(scene);
    }

    public void SaveGame(string name = DefaultSaveName, bool overwrite = false) {
        Debug.Log("Saving game");

        JObject saveData = new JObject();
        saveData.Add("saveName", name);
        Mode            = SerialiseMode.Saving;
        CurrentSaveNode = saveData;

        try {
            Game.Serialise();
        }
        catch (Exception e) {
            Debug.Error("Error saving game: ", e);

            return;
        }

        var fileName = name.ToSnakeCase();

        if (!overwrite) {
            var postFix = 1;
            while (File.Exists($"{SaveDir}{fileName}.json")) {
                fileName = $"{name.ToSnakeCase()}_{postFix}";
                postFix++;
            }
        }
        
        // Save json object to file
        var jsonString = JsonConvert.SerializeObject(saveData);
        File.WriteAllText($"{SaveDir}{fileName}.json", jsonString);
    }
    
    public void LoadGame(string filePath) {
        Debug.Log("Loading game");
        
        var json = File.ReadAllText(filePath);
        var saveData = JsonConvert.DeserializeObject<JObject>(json);
        
        Find.SceneManager.LoadScene(new Scene_Zoo());

        Mode            = SerialiseMode.Loading;
        CurrentSaveNode = saveData;
        
        Game.ClearEntities();
        
        try {
            Game.Serialise();
        }
        catch (Exception e) {
            Find.SceneManager.LoadScene(new Scene_Menu());
            
            Debug.Error("Error loading game: ", e);
        }
    }

    public IEnumerable<SaveFile> GetSaveFiles() {
        if (!Directory.Exists(SaveDir))
            Directory.CreateDirectory(SaveDir);
        
        var files = Directory.GetFiles(SaveDir, "*.json");
        return files.ToList()
            .OrderByDescending(File.GetLastWriteTime)
            .Select(f => new SaveFile {
                Name = Path.GetFileNameWithoutExtension(f),
                Path = f
            });
    }

    // TODO: Allow serialising regular values
    public JObject Serialise(ISerialisable value) {
        var node = new JObject();
        
        Mode            = SerialiseMode.Saving;
        CurrentSaveNode = node;
        
        value.Serialise();
        return node;
    }

    public void ArchiveValue<T>(string label, ref T? value) {
        switch (Mode) {
            case SerialiseMode.Saving:
                CurrentSaveNode.Add(label, JToken.FromObject(value));
                break;
            case SerialiseMode.Loading:
                if (CurrentSaveNode[label] == null)
                    break;
                
                value = CurrentSaveNode[label]!.ToObject<T>();
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
    
    public void ArchiveCustom(string label, Func<JToken> get, Action<JToken> set) {
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
                CurrentSaveNode = new JObject();
                value.Serialise();
                parent[label] = CurrentSaveNode;
                break;
            case SerialiseMode.Loading:
                CurrentSaveNode = parent[label] as JObject;
                value.Serialise();
                break;
        }
        CurrentSaveNode = parent;
    }

    public void ArchiveCollection(string label, IEnumerable<ISerialisable> collection, Func<JArray, IEnumerable<ISerialisable>> select) {
        var parent = CurrentSaveNode;
        switch (Mode) {
            case SerialiseMode.Saving: {
                var array = new JArray();
                foreach (var value in collection) {
                    CurrentSaveNode = new JObject();
                    value.Serialise();
                    array.Add(CurrentSaveNode);
                }
                parent[label] = array;
                break;
            }
            case SerialiseMode.Loading: {
                var array = parent[label] as JArray;
                var i = 0;
                foreach (var value in select(array)) {
                    CurrentSaveNode = array[i++] as JObject; 
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

    public JObject SerialiseToNode(ISerialisable value) {
        var node = new JObject();
        Find.SaveManager.CurrentSaveNode = node;
        Find.SaveManager.Mode            = SerialiseMode.Saving;
        Find.SaveManager.ArchiveDeep("key", value);

        return node;
    }

    public void DeserialiseFromNode(ISerialisable value, JObject node) {
        Find.SaveManager.CurrentSaveNode = node;
        Find.SaveManager.Mode            = SerialiseMode.Loading;
        Find.SaveManager.ArchiveDeep("key", value);
    }
}