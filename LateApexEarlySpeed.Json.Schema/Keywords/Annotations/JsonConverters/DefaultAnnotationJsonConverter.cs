using System.Text.Json;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

public class DefaultAnnotationJsonConverter : JsonConverter<DefaultAnnotation>
{
    public override DefaultAnnotation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement defaultValue = JsonElement.ParseValue(ref reader);

        return new DefaultAnnotation { Value = defaultValue };
    }

    public override void Write(Utf8JsonWriter writer, DefaultAnnotation value, JsonSerializerOptions options)
    {
        value.Value.WriteTo(writer);
    }
}