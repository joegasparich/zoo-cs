using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Zoo.defs;
using Zoo.entities;

namespace Zoo; 

//-- Converters --//

public sealed class CustomContractResolver : DefaultContractResolver {
    protected override JsonConverter ResolveContractConverter(Type type) {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DefRef<>)) {
            Type defType = type.GetGenericArguments()[0];
            return (JsonConverter)Activator.CreateInstance(typeof(DefJsonConverter<>).MakeGenericType(defType));
        }
        return base.ResolveContractConverter(type);
    }
}

public class DefJsonConverter<T> : JsonConverter<DefRef<T>> where T : Def {
    public override bool CanWrite => false;


    public override DefRef<T>? ReadJson(JsonReader reader, Type objectType, DefRef<T>? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        return (DefRef<T>)Activator.CreateInstance(objectType, reader.Value as string);
    }

    public override void WriteJson(JsonWriter writer, DefRef<T>? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}

public class CompJsonConverter : JsonConverter {
    // TODO: This needs to be kept in sync with the main serializer config, can we automate this?
    private JsonSerializer internalSerializer = JsonSerializer.Create(new JsonSerializerSettings() {
        ContractResolver = new CustomContractResolver()
    });
    
    public override bool CanConvert(Type objectType) {
        return objectType == typeof(ComponentData);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        var obj           = JObject.Load(reader);
        var dataClassName = obj["dataClass"]?.Value<string>() ?? "ComponentData";
        var componentType = Type.GetType("Zoo.entities." + dataClassName);
        return obj.ToObject(componentType, internalSerializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}
