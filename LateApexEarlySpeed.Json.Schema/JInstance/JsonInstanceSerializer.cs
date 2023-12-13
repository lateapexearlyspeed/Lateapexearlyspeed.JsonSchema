using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

internal class JsonInstanceSerializer
{
    public static JsonInstanceElement SerializeToElement(object value)
    {
        return new JsonInstanceElement(JsonSerializer.SerializeToElement(value), ImmutableJsonPointer.Empty);
    }
}