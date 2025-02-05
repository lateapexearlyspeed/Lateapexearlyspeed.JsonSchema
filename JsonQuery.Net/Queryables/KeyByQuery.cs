using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(GetQueryParameterConverter<KeyByQuery>))]
[JsonQueryConverter(typeof(GetQueryParameterParserConverter<KeyByQuery>))]
public class KeyByQuery : IJsonQueryable, ISubGetQuery
{
    internal const string Keyword = "keyBy";

    public KeyByQuery(GetQuery getQuery)
    {
        SubGetQuery = getQuery;
    }

    public GetQuery SubGetQuery { get; }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        var newProperties = new Dictionary<string, JsonNode?>();

        foreach (JsonNode? item in array)
        {
            JsonNode? keyNode = SubGetQuery.Query(item);

            string key = keyNode is null ? "null" : keyNode.ToString();

            newProperties.TryAdd(key, item?.DeepClone());
        }

        return new JsonObject(newProperties);
    }
}