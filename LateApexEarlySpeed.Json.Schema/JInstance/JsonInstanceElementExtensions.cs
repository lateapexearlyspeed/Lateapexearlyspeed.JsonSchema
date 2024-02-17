using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

internal static class JsonInstanceElementExtensions
{
    public static TValue? Deserialize<TValue>(this JsonInstanceElement element)
    {
        return element.InternalJsonElement.Deserialize<TValue>();
    }

    public static object? Deserialize(this JsonInstanceElement element, Type returnType)
    {
        return element.InternalJsonElement.Deserialize(returnType);
    }
}