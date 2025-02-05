using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(JsonQueryableConverter))]
public interface IJsonQueryable
{
    JsonNode? Query(JsonNode? data);
}

public class JsonQueryableConverter : JsonConverter<IJsonQueryable>
{
    public override IJsonQueryable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Utf8JsonReader detectorReader = reader;

        if (detectorReader.TokenType == JsonTokenType.StartObject)
        {
            throw new JsonException("Invalid json format for json query");
        }

        if (detectorReader.TokenType == JsonTokenType.StartArray)
        {
            detectorReader.Read();

            if (detectorReader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Invalid token type '{detectorReader.TokenType}' for json query keyword");
            }

            string queryKeyword = detectorReader.GetString()!;

            if (JsonQueryableRegistry.TryGetQueryableType(queryKeyword, out Type? queryableType))
            {
                return (IJsonQueryable)JsonSerializer.Deserialize(ref reader, queryableType)!;
            }

            throw new JsonException($"Invalid json query keyword: {queryKeyword}");
        }

        return new ConstQueryable(JsonNode.Parse(ref reader));
    }

    public override void Write(Utf8JsonWriter writer, IJsonQueryable value, JsonSerializerOptions options)
    {
        if (value is ConstQueryable constQuery)
        {
            if (constQuery.Value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                constQuery.Value.WriteTo(writer);
            }
        }
        else
        {
            JsonSerializer.Serialize(writer, value, value.GetType());
        }
    }

    public override bool HandleNull => true;
}