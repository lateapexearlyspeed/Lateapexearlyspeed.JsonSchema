using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SplitQueryConverter))]
[JsonQueryConverter(typeof(SplitQueryParserConverter))]
public class SplitQuery : IJsonQueryable
{
    internal const string Keyword = "split";

    public IJsonQueryable SubQuery { get; }
    public string? Separator { get; }

    public SplitQuery(IJsonQueryable query, string? separator = null)
    {
        SubQuery = query;
        Separator = separator;
    }

    public JsonNode? Query(JsonNode? data)
    {
        JsonNode? stringNode = SubQuery.Query(data);

        if (stringNode is null || stringNode.GetValueKind() != JsonValueKind.String)
        {
            return null;
        }

        string stringContent = stringNode.GetValue<string>();

        IEnumerable<string> words;
        if (Separator is null)
        {
            words = stringContent.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
        }
        else if (Separator == string.Empty)
        {
            words = stringContent.Select(c => c.ToString());
        }
        else
        {
            words = stringContent.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        }

        return new JsonArray(words.Select(word => JsonValue.Create(word)).ToArray<JsonNode?>());
    }
}

internal class SplitQueryParserConverter : JsonQueryFunctionConverter<SplitQuery>
{
    protected override SplitQuery ReadArguments(ref JsonQueryReader reader)
    {
        IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        if (reader.TokenType == JsonQueryTokenType.String)
        {
            var splitQuery = new SplitQuery(query, reader.GetString());
            reader.Read();

            return splitQuery;
        }

        return new SplitQuery(query);
    }
}

internal class SplitQueryConverter : JsonFormatQueryJsonConverter<SplitQuery>
{
    protected override SplitQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader, options)!;

        reader.Read();

        if (reader.TokenType == JsonTokenType.String)
        {
            var splitQuery = new SplitQuery(query, reader.GetString());
            reader.Read();

            return splitQuery;
        }

        return new SplitQuery(query);
    }

    public override void Write(Utf8JsonWriter writer, SplitQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.SubQuery, options);
        if (value.Separator is not null)
        {
            writer.WriteStringValue(value.Separator);
        }

        writer.WriteEndArray();
    }
}