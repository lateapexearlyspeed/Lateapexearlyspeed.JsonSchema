using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(LimitQueryConverter))]
[JsonQueryConverter(typeof(LimitQueryParserConverter))]
public class LimitQuery : IJsonQueryable
{
    internal const string Keyword = "limit";

    public int LimitSize { get; }

    public LimitQuery(int limitSize)
    {
        LimitSize = limitSize;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return new JsonArray(array.SkipLast(array.Count - LimitSize).Select(item => item?.DeepClone()).ToArray());
    }
}

public class LimitQueryParserConverter : JsonQueryConverter<LimitQuery>
{
    public override LimitQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        if (reader.TokenType != JsonQueryTokenType.Number)
        {
            throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(LimitQuery)}", reader.Position);
        }

        int limitSize = (int)reader.GetDecimal();

        reader.Read();

        return new LimitQuery(limitSize);
    }
}

public class LimitQueryConverter : JsonFormatQueryJsonConverter<LimitQuery>
{
    protected override LimitQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out int limitSize))
        {
            throw new JsonException("Invalid json value for limit query value");
        }

        reader.Read();

        return new LimitQuery(limitSize);
    }

    public override void Write(Utf8JsonWriter writer, LimitQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        writer.WriteNumberValue(value.LimitSize);

        writer.WriteEndArray();
    }
}