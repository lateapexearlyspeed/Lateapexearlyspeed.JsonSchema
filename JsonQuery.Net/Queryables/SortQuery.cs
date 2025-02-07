using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SortQueryConverter))]
[JsonQueryConverter(typeof(SortQueryParserConverter))]
public class SortQuery : IJsonQueryable
{
    internal const string Keyword = "sort";

    public IJsonQueryable SubQuery { get; }
    public bool IsDesc { get; }

    public SortQuery(IJsonQueryable sortQuery, bool isDesc = false)
    {
        SubQuery = sortQuery;
        IsDesc = isDesc;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        if (array.Count == 0)
        {
            return array;
        }

        JsonNode? firstItem = SubQuery.Query(array[0]);

        if (firstItem is null)
        {
            return null;
        }

        IEnumerable<JsonNode?> orderedNodes;
        if (firstItem.GetValueKind() == JsonValueKind.Number)
        {
            orderedNodes = IsDesc ? array.OrderByDescending(DecimalKeySelector) : array.OrderBy(DecimalKeySelector);
        }
        else // assume items kind is String
        {
            orderedNodes = IsDesc ? array.OrderByDescending(StringKeySelector, StringComparer.Ordinal) : array.OrderBy(StringKeySelector, StringComparer.Ordinal);
        }

        return new JsonArray(orderedNodes.Select(item => item?.DeepClone()).ToArray());

        decimal DecimalKeySelector(JsonNode? item)
        {
            JsonNode? jsonNode = SubQuery.Query(item);
            if (jsonNode is null || jsonNode.GetValueKind() != JsonValueKind.Number)
            {
                return 0;
            }
            return jsonNode.GetValue<decimal>();
        }

        string StringKeySelector(JsonNode? item)
        {
            JsonNode? jsonNode = SubQuery.Query(item);
            if (jsonNode is null || jsonNode.GetValueKind() != JsonValueKind.String)
            {
                return string.Empty;
            }

            return jsonNode.GetValue<string>();
        }
    }
}

public class SortQueryParserConverter : JsonQueryFunctionConverter<SortQuery>
{
    protected override SortQuery ReadArguments(ref JsonQueryReader reader)
    {
        IJsonQueryable sortQuery;
        bool isDesc = false;

        if (reader.TokenType == JsonQueryTokenType.EndParenthesis)
        {
            sortQuery = new GetQuery(Array.Empty<object>());
        }
        else
        {
            sortQuery = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read();

            if (reader.TokenType == JsonQueryTokenType.String)
            {
                if (reader.GetString() == "desc")
                {
                    isDesc = true;
                }

                reader.Read();
            }
        }

        return new SortQuery(sortQuery, isDesc);
    }
}

public class SortQueryConverter : JsonFormatQueryJsonConverter<SortQuery>
{
    protected override SortQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        bool isDesc = false;
        IJsonQueryable sortQuery;

        // try to go to first argument
        if (reader.TokenType == JsonTokenType.EndArray)
        {
            sortQuery = new GetQuery(Array.Empty<object>());
        }
        else
        {
            sortQuery = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

            reader.Read();

            // check to second argument
            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.GetString() == "desc")
                {
                    isDesc = true;
                }

                reader.Read();
            }
        }

        return new SortQuery(sortQuery, isDesc);
    }

    public override void Write(Utf8JsonWriter writer, SortQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());

        if (value.SubQuery is GetQuery getQuery && getQuery.Path.Length == 0 && !value.IsDesc)
        {
            writer.WriteEndArray();
            return;
        }

        JsonSerializer.Serialize(writer, value.SubQuery);

        if (value.IsDesc)
        {
            writer.WriteStringValue("desc");
        }

        writer.WriteEndArray();
    }
}