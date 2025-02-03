using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonQuery.Net;

internal static class JsonNodeExtensions
{
    public static bool GetBooleanValue(this JsonNode? jsonNode)
    {
        if (jsonNode is null)
        {
            return false;
        }

        if (jsonNode.GetValueKind() == JsonValueKind.String)
        {
            return true;
        }

        if (jsonNode.GetValueKind() == JsonValueKind.Number)
        {
            return jsonNode.GetValue<decimal>() != 0;
        }

        if (jsonNode.GetValueKind() == JsonValueKind.True || jsonNode.GetValueKind() == JsonValueKind.False)
        {
            return jsonNode.GetValue<bool>();
        }

        return true;
    }
}