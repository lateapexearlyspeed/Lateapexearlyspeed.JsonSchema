using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace JsonSchemaConsoleApp;

internal static class JsonElementExtensions
{
    public static bool TryGetKeyword<T>(this JsonElement schema, string keyword, [NotNullWhen(true)] out T? value)
    {
        if (!schema.TryGetProperty(keyword, out JsonElement valueElement))
        {
            value = default;
            return false;
        }

        Type type = typeof(T);
        if (type == typeof(string))
        {
            value = (T)(object)valueElement.GetString()!;
        }
        else if (type == typeof(int))
        {
            value = (T)(object)valueElement.GetInt32();
        }
        else if (type == typeof(uint))
        {
            value = (T)(object)valueElement.GetUInt32();
        }
        else if (type == typeof(double))
        {
            value = (T)(object)valueElement.GetDouble();
        }
        else if (type == typeof(JsonElement))
        {
            value = (T)(object)valueElement;
        }
        else
        {
            value = valueElement.Deserialize<T>()!;
        }

        return true;
    }
}