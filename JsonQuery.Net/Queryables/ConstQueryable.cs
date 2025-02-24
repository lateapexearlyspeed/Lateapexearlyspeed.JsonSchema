using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ConstQueryableConverter))]
public class ConstQueryable : IJsonQueryable
{
    public JsonNode? Value { get; }

    public ConstQueryable(JsonNode? value)
    {
        Value = value;
    }

    public JsonNode? Query(JsonNode? data)
    {
        return Value;
    }
}

public class ConstQueryableConverter : JsonConverter<ConstQueryable>
{
    public override ConstQueryable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ConstQueryable value, JsonSerializerOptions options)
    {
        if (value.Value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            value.Value.WriteTo(writer);
        }
    }
}