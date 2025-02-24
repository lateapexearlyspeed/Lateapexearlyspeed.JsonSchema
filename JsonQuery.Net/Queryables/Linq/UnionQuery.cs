using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class UnionQuery : IJsonQueryable
{
    internal const string Keyword = "union";

    [QueryArgument(0)]
    public IJsonQueryable FirstArrayQuery { get; }

    [QueryArgument(1)] 
    public IJsonQueryable SecondArrayQuery { get; }

    public UnionQuery(IJsonQueryable firstArrayQuery, IJsonQueryable secondArrayQuery)
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

        IEnumerable<JsonNode?> result = firstArray.Union(secondArray, new JsonNodeEqualityComparer());

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}