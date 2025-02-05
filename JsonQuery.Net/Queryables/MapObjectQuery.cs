using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(MapObjectQueryConverter))]
[JsonQueryConverter(typeof(MapObjectQueryParserConverter))]
public class MapObjectQuery : IJsonQueryable
{
    internal const string Keyword = "mapObject";

    public IJsonQueryable KeyQuery { get; }
    public IJsonQueryable ValueQuery { get; }

    public MapObjectQuery(IJsonQueryable keyQuery, IJsonQueryable valueQuery)
    {
        KeyQuery = keyQuery;
        ValueQuery = valueQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonObject objectData)
        {
            return null;
        }

        var result = new JsonObject();

        foreach (KeyValuePair<string, JsonNode?> prop in objectData)
        {
            var origPropObject = new JsonObject { { "key", prop.Key }, { "value", prop.Value?.DeepClone() } };

            JsonNode? keyNode = KeyQuery.Query(origPropObject);

            if (keyNode is not null && keyNode.GetValueKind() == JsonValueKind.String)
            {
                result.Add(keyNode.GetValue<string>(), ValueQuery.Query(origPropObject)?.DeepClone());
            }
        }

        return result;
    }
}

internal class MapObjectQueryParserConverter : JsonQueryConverter<MapObjectQuery>
{
    public override MapObjectQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        if (reader.TokenType != JsonQueryTokenType.StartBrace)
        {
            throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(MapObjectQuery)}", reader.Position);
        }

        ObjectQuery objectQuery = JsonQueryParser.ParseObjectQuery(ref reader);

        reader.Read();

        return new MapObjectQuery(objectQuery.PropertiesQueries["key"], objectQuery.PropertiesQueries["value"]);
    }
}

internal class MapObjectQueryConverter : JsonConverter<MapObjectQuery>
{
    public override MapObjectQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();

        ObjectQuery objectQuery = JsonSerializer.Deserialize<ObjectQuery>(ref reader)!;

        reader.Read();

        return new MapObjectQuery(objectQuery.PropertiesQueries["key"], objectQuery.PropertiesQueries["value"]);
    }

    public override void Write(Utf8JsonWriter writer, MapObjectQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        var dic = new Dictionary<string, IJsonQueryable>(2) { { "key", value.KeyQuery }, { "value", value.ValueQuery } };

        JsonSerializer.Serialize(writer, dic);

        writer.WriteEndArray();
    }
}