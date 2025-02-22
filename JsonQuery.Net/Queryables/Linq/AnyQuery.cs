using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class AnyQuery : IJsonQueryable
{
    internal const string Keyword = "any";

    public IJsonQueryable SubQuery { get; }

    public AnyQuery(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return array.Any(item => SubQuery.Query(item).GetBooleanValue());
    }
}