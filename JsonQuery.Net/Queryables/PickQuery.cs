using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(PickQueryConverter))]
[JsonQueryConverter(typeof(PickQueryParserConverter))]
public class PickQuery : IJsonQueryable
{
    internal const string Keyword = "pick";

    public GetQuery[] GetQueries { get; }

    public PickQuery(GetQuery[] getQueries)
    {
        GetQueries = getQueries;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is JsonArray array)
        {
            var newArray = new JsonArray();

            foreach (JsonNode? item in array)
            {
                if (item is JsonObject itemObject)
                {
                    newArray.Add(PickObject(itemObject));
                }
            }

            return newArray;
        }

        if (data is JsonObject jsonObject)
        {
            return PickObject(jsonObject);
        }

        return null;
    }

    private JsonObject PickObject(JsonObject sourceObject)
    {
        IEnumerable<KeyValuePair<string, JsonNode?>> properties = GetQueries.Select(propQuery => propQuery.QueryPropertyNameAndValue(sourceObject))
            .Where(prop => prop.propertyName is not null)
            .Select(prop => KeyValuePair.Create(prop.propertyName!, prop.propertyValue?.DeepClone()));

        return new JsonObject(properties);
    }
}

public class PickQueryParserConverter : JsonQueryConverter<PickQuery>
{
    public override PickQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        var getQueries = new List<GetQuery>();
        while (reader.TokenType != JsonQueryTokenType.EndParenthesis)
        {
            IJsonQueryable query = JsonQueryParser.ParseSingleQuery(ref reader);
            if (query is not GetQuery getQuery)
            {
                throw new JsonQueryParseException($"Invalid query type: {query.GetType()} for {typeof(PickQuery)}", reader.Position);
            }

            getQueries.Add(getQuery);

            reader.Read();
        }

        return new PickQuery(getQueries.ToArray());
    }
}

public class PickQueryConverter : JsonConverter<PickQuery>
{
    public override PickQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();

        var getQueries = new List<GetQuery>();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            getQueries.Add(JsonSerializer.Deserialize<GetQuery>(ref reader)!);

            reader.Read();
        }

        return new PickQuery(getQueries.ToArray());
    }

    public override void Write(Utf8JsonWriter writer, PickQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        foreach (GetQuery getQuery in value.GetQueries)
        {
            JsonSerializer.Serialize(writer, getQuery);
        }

        writer.WriteEndArray();
    }
}