using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class FirstQuery : IJsonQueryable
{
    internal const string Keyword = "first";

    public IJsonQueryable Filter { get; }

    public FirstQuery(IJsonQueryable filter)
    {
        Filter = filter;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return array.FirstOrDefault(item => Filter.Query(item).GetBooleanValue());
    }
}