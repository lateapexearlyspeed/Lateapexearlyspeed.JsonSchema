using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter))]
public class ProdQuery : IJsonQueryable
{
    internal const string Keyword = "prod";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        decimal result = array.Where(item => item is not null && item.GetValueKind() == JsonValueKind.Number)
            .Select(item => item!.GetValue<decimal>()).Aggregate((pre, current) => current * pre);

        return JsonValue.Create(result);
    }
}