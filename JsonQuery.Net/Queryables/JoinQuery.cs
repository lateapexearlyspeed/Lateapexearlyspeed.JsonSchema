using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(JoinQueryConverter))]
[JsonQueryConverter(typeof(JoinQueryParserConverter))]
public class JoinQuery : IJsonQueryable
{
    internal const string Keyword = "join";

    public string Separator { get; }

    public JoinQuery(string separator = "")
    {
        Separator = separator;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return string.Join(Separator, array.Where(item => item is not null && item.GetValueKind() == JsonValueKind.String).Select(item => item!.GetValue<string>()));
    }
}

internal class JoinQueryParserConverter : JsonQueryConverter<JoinQuery>
{
    public override JoinQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        if (reader.TokenType == JsonQueryTokenType.String)
        {
            var joinQuery = new JoinQuery(reader.GetString());
            reader.Read();

            return joinQuery;
        }

        return new JoinQuery();
    }
}

internal class JoinQueryConverter : JsonFormatQueryJsonConverter<JoinQuery>
{
    protected override JoinQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var joinQuery = new JoinQuery(reader.GetString()!);
            reader.Read();

            return joinQuery;
        }

        return new JoinQuery();
    }

    public override void Write(Utf8JsonWriter writer, JoinQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());

        if (!string.IsNullOrEmpty(value.Separator))
        {
            writer.WriteStringValue(value.Separator);
        }

        writer.WriteEndArray();
    }
}