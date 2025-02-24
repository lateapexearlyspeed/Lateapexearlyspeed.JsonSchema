using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class LastQuery : IJsonQueryable
{
    internal const string Keyword = "last";

    [QueryArgument(0)]
    public IJsonQueryable Filter { get; }

    public LastQuery(IJsonQueryable filter)
    {
        Filter = filter;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return array.LastOrDefault(item => Filter.Query(item).GetBooleanValue());
    }
}