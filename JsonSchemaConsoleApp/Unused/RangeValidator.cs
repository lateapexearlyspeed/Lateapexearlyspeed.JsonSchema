using System.Text.Json;

namespace JsonSchemaConsoleApp.Unused;

internal class RangeValidator
{
    private const string MinimumKeyword = "minimum";
    private const string MaximumKeyword = "maximum";
    private const string ExclusiveMinimumKeyword = "exclusiveMinimum";
    private const string ExclusiveMaximumKeyword = "exclusiveMaximum";

    public static bool Validate<T>(JsonElement schema, T value) where T : IComparable<T>
    {
        if (schema.TryGetKeyword(MinimumKeyword, out T? minimum))
        {
            if (value.CompareTo(minimum) < 0)
            {
                return false;
            }
        }

        if (schema.TryGetKeyword(MaximumKeyword, out T? maximum))
        {
            if (value.CompareTo(maximum) > 0)
            {
                return false;
            }
        }

        if (schema.TryGetKeyword(ExclusiveMinimumKeyword, out T? exclusiveMinimum))
        {
            if (value.CompareTo(exclusiveMinimum) <= 0)
            {
                return false;
            }
        }

        if (schema.TryGetKeyword(ExclusiveMaximumKeyword, out T? exclusiveMaximumKeyword))
        {
            if (value.CompareTo(exclusiveMaximumKeyword) >= 0)
            {
                return false;
            }
        }

        return true;
    }
}