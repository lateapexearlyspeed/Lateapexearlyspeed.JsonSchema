using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter<SizeQuery>))]
public class SizeQuery : IJsonQueryable
{
    internal const string Keyword = "size";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is JsonArray array)
        {
            return JsonValue.Create(array.Count);
        }

        if (data is not null && data.GetValueKind() == JsonValueKind.String)
        {
            return JsonValue.Create(data.GetValue<string>().Length);
        }

        return null;
    }
}