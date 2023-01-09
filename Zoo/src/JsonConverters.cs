using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zoo.defs;
using Zoo.entities;

namespace Zoo; 

//-- Defs --//

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
            args: Array.Empty<object>(),
            culture: null)!;
    }
    
    private class DefJsonConverter<T> : JsonConverter<DefRef<T>> where T : Def {
        public override DefRef<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();

            var name = reader.GetString()!;

            return new DefRef<T>(name);
        }
        public override void Write(Utf8JsonWriter writer, DefRef<T> value, JsonSerializerOptions options) {}
    }
}

//-- Components --//

public class CompJsonConverter : JsonConverter<ComponentData> {
    public override ComponentData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        Utf8JsonReader readerClone = reader;

        if (readerClone.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        readerClone.Read();

        if (readerClone.TokenType != JsonTokenType.PropertyName)
            throw new JsonException();

        string? propertyName = readerClone.GetString();
        
        if (propertyName != "dataClass")
            throw new JsonException();

        readerClone.Read();
        
        if (readerClone.TokenType != JsonTokenType.String)
            throw new JsonException();

        var componentType = Type.GetType("Zoo.entities." + readerClone.GetString()!);
        return (ComponentData)JsonSerializer.Deserialize(ref reader, componentType, options);
    }
    public override void Write(Utf8JsonWriter writer, ComponentData value, JsonSerializerOptions options) {}
}