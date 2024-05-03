using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class SingleSchemaJsonConverter<T> : JsonConverter<T> where T : ISingleSubSchema, new()
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new T { Schema = JsonSerializer.Deserialize<JsonSchema>(ref reader)! };
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Schema, options);
    }

    public override bool HandleNull => true;
}