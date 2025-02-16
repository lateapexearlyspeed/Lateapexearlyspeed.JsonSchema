using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter<MinQuery>))]
public class MinQuery : IJsonQueryable
{
    internal const string Keyword = "min";

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

        return JsonValue.Create(numericArray.Min(item => item!.GetValue<decimal>()));
    }
}