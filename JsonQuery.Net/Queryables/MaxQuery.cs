using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter))]
public class MaxQuery : IJsonQueryable
{
    internal const string Keyword = "max";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        JsonNode?[] numericArray = array.Where(item => item is not null && item.GetValueKind() == JsonValueKind.Number).ToArray();

        if (numericArray.Length == 0)
        {
            return null;
        }

        return JsonValue.Create(numericArray.Max(item => item!.GetValue<decimal>()));
    }
}