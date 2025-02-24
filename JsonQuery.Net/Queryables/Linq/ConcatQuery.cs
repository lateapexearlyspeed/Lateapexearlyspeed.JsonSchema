using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class ConcatQuery : IJsonQueryable
{
    internal const string Keyword = "concat";

    [QueryArgument(0)]
    public IJsonQueryable FirstArrayQuery { get; }

    [QueryArgument(1)]
    public IJsonQueryable SecondArrayQuery { get; }

    public ConcatQuery(IJsonQueryable firstArrayQuery, IJsonQueryable secondArrayQuery)
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

        IEnumerable<JsonNode?> result = firstArray.Concat(secondArray);

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}