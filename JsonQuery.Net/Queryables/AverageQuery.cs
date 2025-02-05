using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter<AverageQuery>))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter<AverageQuery>))]
public class AverageQuery : IJsonQueryable
{
    internal const string Keyword = "average";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        var numericArray = array.Where(item => item is not null && item.GetValueKind() == JsonValueKind.Number).ToArray();

        if (numericArray.Length == 0)
        {
            return null;
        }

        decimal result = numericArray.Average(item => item!.GetValue<decimal>());
        return JsonValue.Create(result);
    }
}