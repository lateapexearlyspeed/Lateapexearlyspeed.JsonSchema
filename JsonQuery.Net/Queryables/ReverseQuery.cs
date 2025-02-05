using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(ParameterlessQueryConverter<ReverseQuery>))]
[JsonQueryConverter(typeof(ParameterlessQueryParserConverter<ReverseQuery>))]
public class ReverseQuery : IJsonQueryable
{
    internal const string Keyword = "reverse";

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return new JsonArray(array.Reverse().Select(item => item?.DeepClone()).ToArray());
    }
}