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
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = {
            new Vector2JsonConverter()
        }
    };
    
    public JsonObject CurrentSaveNode { get; set; }
    public  SerialiseMode mode { get; private set; }
    
    public void NewGame() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Starting new game");
        Find.SceneManager.LoadScene(new ZooScene());
    }

    public void SaveGame(string filePath) {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Saving game");
        
        JsonObject saveData = new JsonObject();
        mode            = SerialiseMode.Saving;
        CurrentSaveNode = saveData;
        Game.Serialise();
        
        // Save json object to file
        string json = JsonSerializer.Serialize(saveData, serializeOptions);
        File.WriteAllText(filePath, json);
    }
    
    public void LoadGame(string filePath) {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Loading game");
        
        var json = File.ReadAllText(filePath);
        var saveData = JsonSerializer.Deserialize<JsonObject>(json, serializeOptions);

        mode            = SerialiseMode.Loading;
        CurrentSaveNode = saveData;
        Game.Serialise();
    }
    
    public void SerialiseValue<T>(string label, ref T? value) {
        switch (mode) {
            case SerialiseMode.Saving:
                CurrentSaveNode[label] = JsonSerializer.Serialize(value, serializeOptions);
                break;
            case SerialiseMode.Loading:
                value = JsonSerializer.Deserialize<T>(CurrentSaveNode[label]!.ToString(), serializeOptions);
                break;
        }
    }

    public void SerialiseValue<T>(string label, Func<T> get, Action<T?> set) {
        switch (mode) {
            case SerialiseMode.Saving: {
                var value = get();
                SerialiseValue(label, ref value);
                break;
            }
            case SerialiseMode.Loading: {
                var value = default(T);
                SerialiseValue(label, ref value);
                set(value);
                break;
            }
        }
    }
    
    public void SerialiseCustom(string label, Func<JsonNode> get, Action<JsonNode> set) {
        switch (mode) {
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
    
    public void SerialiseDeep(string label, ISerialisable value) {
        var parent = CurrentSaveNode;
        switch (mode) {
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
}