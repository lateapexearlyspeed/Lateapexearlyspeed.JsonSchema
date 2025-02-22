using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class SelectManyQuery : IJsonQueryable
{
    internal const string Keyword = "selectMany";

    public IJsonQueryable SelectorQuery { get; }

    public SelectManyQuery(IJsonQueryable selectorQuery)
    {
        SelectorQuery = selectorQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        IEnumerable<JsonNode?> result = array.Where(item => item is JsonArray subArray && SelectorQuery.Query(subArray) is JsonArray).SelectMany(item => SelectorQuery.Query(item)!.AsArray());

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}