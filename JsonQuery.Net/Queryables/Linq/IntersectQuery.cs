using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class IntersectQuery : IJsonQueryable
{
    internal const string Keyword = "intersect";

    public IJsonQueryable FirstArrayQuery { get; }
    public IJsonQueryable SecondArrayQuery { get; }

    public IntersectQuery(IJsonQueryable firstArrayQuery, IJsonQueryable secondArrayQuery)
    {
        FirstArrayQuery = firstArrayQuery;
        SecondArrayQuery = secondArrayQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (FirstArrayQuery.Query(data) is not JsonArray firstArray)
        {
            return null;
        }

        if (SecondArrayQuery.Query(data) is not JsonArray secondArray)
        {
            return null;
        }

        IEnumerable<JsonNode?> result = firstArray.Intersect(secondArray, new JsonNodeEqualityComparer());

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}