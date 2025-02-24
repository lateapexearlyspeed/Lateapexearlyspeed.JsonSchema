using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class SkipWhileQuery : IJsonQueryable
{
    internal const string Keyword = "skipWhile";

    [QueryParam(0)]
    public IJsonQueryable PredicateQuery { get; }

    public SkipWhileQuery(IJsonQueryable predicateQuery)
    {
        PredicateQuery = predicateQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        IEnumerable<JsonNode?> result = array.SkipWhile(item => PredicateQuery.Query(item).GetBooleanValue());

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}