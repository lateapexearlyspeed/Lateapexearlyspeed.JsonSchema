using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonQuery.Net;

public class JsonNodeEqualityComparer : IEqualityComparer<JsonNode?>
{
    public bool Equals(JsonNode? x, JsonNode? y)
    {
        return JsonNode.DeepEquals(x, y);
    }

    public int GetHashCode(JsonNode? obj)
    {
        JsonValueKind kind = obj is null ? JsonValueKind.Null : obj.GetValueKind();

        return (int)kind;
    }
}