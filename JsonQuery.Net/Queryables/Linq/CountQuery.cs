using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class CountQuery : IJsonQueryable
{
    internal const string Keyword = "count";

    [QueryArgument(0)]
    public IJsonQueryable Filter { get; }

    public CountQuery(IJsonQueryable filter)
    {
        Filter = filter;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return array.Count(item => Filter.Query(item).GetBooleanValue());
    }
}