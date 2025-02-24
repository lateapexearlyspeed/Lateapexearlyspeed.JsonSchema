using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class AllQuery : IJsonQueryable
{
    internal const string Keyword = "all";

    [QueryArgument(0)]
    public IJsonQueryable SubQuery { get; }

    public AllQuery(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return array.All(item => SubQuery.Query(item).GetBooleanValue());
    }
}