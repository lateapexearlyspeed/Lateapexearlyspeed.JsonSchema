using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class SelectManyQuery : IJsonQueryable
{
    internal const string Keyword = "selectMany";

    [QueryParam(0)]
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

        var jsonArrays = new List<JsonArray>(array.Count);

        foreach (JsonNode? item in array)
        {
            if (SelectorQuery.Query(item) is JsonArray subArray)
            {
                jsonArrays.Add(subArray);
            }
        }

        IEnumerable<JsonNode?> result = jsonArrays.SelectMany(subArray => subArray);

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}