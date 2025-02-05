using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter<MapValuesQuery>))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter<MapValuesQuery>))]
public class MapValuesQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "mapValues";

    private readonly IJsonQueryable _valueQuery;

    public MapValuesQuery(IJsonQueryable valueQuery)
    {
        _valueQuery = valueQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonObject objectData)
        {
            return null;
        }

        var result = new JsonObject();

        foreach (KeyValuePair<string, JsonNode?> prop in objectData)
        {
            result.Add(prop.Key, _valueQuery.Query(prop.Value)?.DeepClone());
        }

        return result;
    }

    public IJsonQueryable SubQuery => _valueQuery;
}