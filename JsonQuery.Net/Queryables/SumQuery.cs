using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter))]
public class SumQuery : IJsonQueryable
{
    internal const string Keyword = "sum";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return array.Where(item => item is not null && item.GetValueKind() == JsonValueKind.Number).Sum(item => item!.GetValue<decimal>());
    }
}