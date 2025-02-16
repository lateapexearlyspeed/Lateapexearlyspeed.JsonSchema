using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter<MapQuery>))]
public class MapQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "map";

    private readonly IJsonQueryable _itemQuery;

    public MapQuery(IJsonQueryable itemQuery)
    {
        _itemQuery = itemQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return new JsonArray(array.Select(item => _itemQuery.Query(item)?.DeepClone()).ToArray());
    }

    public IJsonQueryable SubQuery => _itemQuery;
}