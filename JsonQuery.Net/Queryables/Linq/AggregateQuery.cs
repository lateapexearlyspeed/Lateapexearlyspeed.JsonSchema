using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class AggregateQuery : IJsonQueryable
{
    internal const string Keyword = "aggregate";

    [QueryParam(0)]
    public IJsonQueryable SubQuery { get; }

    public AggregateQuery(IJsonQueryable subQuery)
    {
        SubQuery = subQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return array.Aggregate((aggregated, cur) => SubQuery.Query(new JsonArray(aggregated?.DeepClone(), cur?.DeepClone())));
    }
}