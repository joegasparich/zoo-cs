using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zoo.util;

public class Vector2JsonConverter : JsonConverter<Vector2> {
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.GetString().ToVector2();
    }
    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options) {
        writer.WriteRawValue(value.ToString(), true);
    }
}