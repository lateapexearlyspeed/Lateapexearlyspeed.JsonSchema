using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter))]
public class MapKeysQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "mapKeys";

    private readonly IJsonQueryable _keyQuery;

    public MapKeysQuery(IJsonQueryable keyQuery)
    {
        _keyQuery = keyQuery;
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
            JsonNode? newKeyNode = _keyQuery.Query(JsonValue.Create(prop.Key));

            if (newKeyNode is not null && newKeyNode.GetValueKind() == JsonValueKind.String)
            {
                result.Add(newKeyNode.GetValue<string>(), prop.Value?.DeepClone());
            }
        }

        return result;
    }

    public IJsonQueryable SubQuery => _keyQuery;
}