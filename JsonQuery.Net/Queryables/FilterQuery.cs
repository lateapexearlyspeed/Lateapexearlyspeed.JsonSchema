using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter))]
public class FilterQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "filter";

    private readonly IJsonQueryable _filter;

    public FilterQuery(IJsonQueryable filter)
    {
        _filter = filter;
    }

    public JsonNode? Query(JsonNode? data)
    {
        var result = new JsonArray();

        if (data is not JsonArray array)
        {
            return null;
        }

        foreach (JsonNode? item in array)
        {
            JsonNode? predicateResult = _filter.Query(item);

            if (predicateResult.GetBooleanValue())
            {
                result.Add(item?.DeepClone());
            }
        }

        return result;
    }

    public IJsonQueryable SubQuery => _filter;
}