using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter))]
public class KeysQuery : IJsonQueryable
{
    internal const string Keyword = "keys";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonObject objectData)
        {
            return null;
        }

        IEnumerable<JsonValue> keys = objectData.Select(prop => JsonValue.Create(prop.Key));

        return new JsonArray(keys.ToArray<JsonNode>());
    }
}