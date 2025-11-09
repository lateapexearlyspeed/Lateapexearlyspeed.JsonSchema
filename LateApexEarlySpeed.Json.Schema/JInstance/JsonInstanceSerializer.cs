using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

internal class JsonInstanceSerializer
{
    public static JsonInstanceElement SerializeToElement(object value)
    {
        return new JsonInstanceElement(JsonSerializer.SerializeToElement(value), LinkedListBasedImmutableJsonPointer.Empty);
    }

    public static JsonInstanceElement Deserialize(string json)
    {
        JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        return new JsonInstanceElement(jsonElement, LinkedListBasedImmutableJsonPointer.Empty);
    }
}