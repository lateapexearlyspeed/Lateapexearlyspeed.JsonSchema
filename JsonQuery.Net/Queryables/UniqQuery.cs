using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter))]
public class UniqQuery : IJsonQueryable
{
    internal const string Keyword = "uniq";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        var result = new List<JsonNode?>();

        foreach (JsonNode? item in array)
        {
            if (result.All(node => !JsonNode.DeepEquals(node, item)))
            {
                result.Add(item?.DeepClone());
            }
        }

        return new JsonArray(result.ToArray());
    }
}