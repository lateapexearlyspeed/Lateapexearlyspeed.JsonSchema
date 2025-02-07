using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(GetQueryConverter))]
[JsonQueryConverter(typeof(GetQueryParserConverter))]
public class GetQuery : IJsonQueryable
{
    internal const string Keyword = "get";

    public object[] Path { get; }

    public GetQuery(object[] path)
    {
        Path = path;
    }

    public JsonNode? Query(JsonNode? data)
    {
        return QueryPropertyNameAndValue(data).propertyValue;
    }

    public (string? propertyName, JsonNode? propertyValue, bool exist) QueryPropertyNameAndValue(JsonNode? data)
    {
        JsonNode? curNode = data;
        foreach (object segment in Path)
        {
            if (curNode is JsonObject jsonObject)
            {
                string propertyName;
                if (segment is string stringSegment)
                {
                    propertyName = stringSegment;
                }
                else
                {
                    Debug.Assert(segment is int);
                    propertyName = segment.ToString();
                }

                if (!jsonObject.TryGetPropertyValue(propertyName, out curNode))
                {
                    return (GetTheLastPropertyName(), null, false);
                }
            }
            else if (curNode is JsonArray jsonArray)
            {
                if (segment is int index && index < jsonArray.Count)
                {
                    curNode = jsonArray[index];
                }
                else
                {
                    return (GetTheLastPropertyName(), null, false);
                }
            }
            else
            {
                return (GetTheLastPropertyName(), null, false);
            }
        }

        return (GetTheLastPropertyName(), curNode, true);
    }

    private string? GetTheLastPropertyName()
    {
        return Path.Length == 0 ? null : Path[Path.Length - 1].ToString();
    }
}

public class GetQueryParserConverter : JsonQueryFunctionConverter<GetQuery>
{
    protected override GetQuery ReadArguments(ref JsonQueryReader reader)
    {
        var propertyPath = new List<object>();
        while (reader.TokenType != JsonQueryTokenType.EndParenthesis)
        {
            object segment;
            if (reader.TokenType == JsonQueryTokenType.String)
            {
                segment = reader.GetString();
            }
            else if (reader.TokenType == JsonQueryTokenType.Number)
            {
                segment = (int)reader.GetDecimal();
            }
            else
            {
                throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for 'get' query", reader.Position);
            }

            propertyPath.Add(segment);

            reader.Read();
        }

        return new GetQuery(propertyPath.ToArray());
    }
}

public class GetQueryConverter : JsonFormatQueryJsonConverter<GetQuery>
{
    protected override GetQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var propertyPath = new List<object>();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            object segment;
            if (reader.TokenType == JsonTokenType.String)
            {
                segment = reader.GetString()!;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                segment = (int)reader.GetDecimal();
            }
            else
            {
                throw new JsonException($"Invalid json type: {reader.TokenType} for get path");
            }

            propertyPath.Add(segment);

            reader.Read();
        }

        return new GetQuery(propertyPath.ToArray());
    }

    public override void Write(Utf8JsonWriter writer, GetQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());

        foreach (object item in value.Path)
        {
            if (item is string stringItem)
            {
                writer.WriteStringValue(stringItem);
            }
            else
            {
                Debug.Assert(item is int);

                writer.WriteNumberValue((int)item);
            }
        }

        writer.WriteEndArray();
    }
}