using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter<UniqByQuery>))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter<UniqByQuery>))]
public class UniqByQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "uniqBy";

    public UniqByQuery(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public IJsonQueryable SubQuery { get; }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray sourceArray)
        {
            return null;
        }

        var resultArray = new List<(JsonNode? key, JsonNode? value)>(sourceArray.Count);

        foreach (JsonNode? item in sourceArray)
        {
            JsonNode? key = SubQuery.Query(item);

            if (resultArray.All(kv => !JsonNode.DeepEquals(kv.key, key)))
            {
                resultArray.Add((key, item));
            }
        }

        return new JsonArray(resultArray.Select(kv => kv.value?.DeepClone()).ToArray());
    }
}