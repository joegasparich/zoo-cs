using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Zoo.defs;
using Zoo.entities;

namespace Zoo; 

//-- Converters --//

public sealed class CustomContractResolver : DefaultContractResolver {
    protected override JsonConverter? ResolveContractConverter(Type type) {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DefRef<>)) {
            var defType = type.GetGenericArguments()[0];
            return Activator.CreateInstance(typeof(DefJsonConverter<>).MakeGenericType(defType)) as JsonConverter;
        }
        return base.ResolveContractConverter(type);
    }
}

public class DefJsonConverter<T> : JsonConverter<DefRef<T>> where T : Def {

    public override DefRef<T>? ReadJson(JsonReader reader, Type objectType, DefRef<T>? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        return Activator.CreateInstance(objectType, reader.Value as string) as DefRef<T>;
    }

    public override void WriteJson(JsonWriter writer, DefRef<T>? value, JsonSerializer serializer) {
        writer.WriteValue(value.Def.Id);
    }
}

public class CompJsonConverter : JsonConverter {
    // This needs to be kept in sync with the main serializer config
    private readonly JsonSerializer internalSerializer = JsonSerializer.Create(new JsonSerializerSettings() {
        ContractResolver = new CustomContractResolver()
    });

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(List<ComponentData>);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        var tokenReader = reader as JTokenReader;
        var token       = tokenReader.CurrentToken;
        var components  = new List<ComponentData>();

        foreach (JProperty child in token) {
            var compType = Type.GetType($"Zoo.entities.{child.Name}");

            if (!compType.IsAssignableTo(typeof(Component))) {
                Debug.Warn("Component type not found: " + child.Name);
                continue;
            }

            var compDataType = compType.GetProperty("DataType")?.GetValue(null) as Type;
            ComponentData componentData;
            if (compDataType != null) {
                componentData = child.Value.ToObject(compDataType, internalSerializer) as ComponentData;
            } else {
                componentData = child.Value.ToObject(typeof(ComponentData), internalSerializer) as ComponentData;
                var compClassProp = componentData.GetType().GetField("compClass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                compClassProp.SetValue(componentData, child.Name);
            }
            components.Add(componentData);
        }

        reader.Skip();
        return components;
    }

    public override bool CanWrite => false;
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}
