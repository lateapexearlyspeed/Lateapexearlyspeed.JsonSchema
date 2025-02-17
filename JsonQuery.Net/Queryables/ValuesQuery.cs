using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter))]
public class ValuesQuery : IJsonQueryable
{
    internal const string Keyword = "values";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonObject objectData)
        {
            return null;
        }

        IEnumerable<JsonNode?> values = objectData.Select(prop => prop.Value?.DeepClone());

        return new JsonArray(values.ToArray());
    }
}