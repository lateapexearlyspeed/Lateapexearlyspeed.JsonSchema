using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ObjectQueryConverter))]
public class ObjectQuery : IJsonQueryable
{
    internal const string Keyword = "object";

    public Dictionary<string, IJsonQueryable> PropertiesQueries { get; }

    public ObjectQuery(Dictionary<string, IJsonQueryable> propertiesQueries)
    {
        PropertiesQueries = propertiesQueries;
    }

    public JsonNode Query(JsonNode? data)
    {
        return new JsonObject(PropertiesQueries.Select(prop => KeyValuePair.Create(prop.Key, prop.Value.Query(data)?.DeepClone())));
    }
}

public class ObjectQueryConverter : JsonFormatQueryJsonConverter<ObjectQuery>
{
    protected override ObjectQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var objectQuery = new ObjectQuery(JsonSerializer.Deserialize<Dictionary<string, IJsonQueryable>>(ref reader)!);

        reader.Read();

        return objectQuery;
    }

    public override void Write(Utf8JsonWriter writer, ObjectQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.PropertiesQueries);

        writer.WriteEndArray();
    }
}