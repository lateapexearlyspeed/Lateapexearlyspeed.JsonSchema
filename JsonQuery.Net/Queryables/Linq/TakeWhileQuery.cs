using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class TakeWhileQuery : IJsonQueryable
{
    internal const string Keyword = "takeWhile";

    [QueryParam(0)]
    public IJsonQueryable PredicateQuery { get; }

    public TakeWhileQuery(IJsonQueryable predicateQuery)
    {
        PredicateQuery = predicateQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        IEnumerable<JsonNode?> result = array.TakeWhile(item => PredicateQuery.Query(item).GetBooleanValue());

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}