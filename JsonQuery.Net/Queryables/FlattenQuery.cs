using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter))]
public class FlattenQuery : IJsonQueryable
{
    internal const string Keyword = "flatten";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        IEnumerable<JsonNode?> flattenArray = array.Where(item => item is JsonArray).SelectMany(item => item!.AsArray().Select(subItem => subItem?.DeepClone()));

        return new JsonArray(flattenArray.ToArray());
    }
}