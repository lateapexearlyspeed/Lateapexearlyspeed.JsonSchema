namespace LateApexEarlySpeed.Json.Schema.JInstance;

internal static class JsonInstanceElementNoBoxExtensions
{
    public static int GetPropertyCount(this JsonInstanceElement instance)
    {
        int count = 0;

        foreach (JsonInstanceProperty _ in instance.EnumerateObject())
        {
            count++;
        }

        return count;
    }

    public static int GetArrayLength(this JsonInstanceElement instance)
    {
        int count = 0;

        foreach (JsonInstanceElement _ in instance.EnumerateArray())
        {
            count++;
        }

        return count;
    }

    public static Dictionary<string, JsonInstanceElement> ToPropertyDictionary(this JsonInstanceElement instance)
    {
        var properties = new Dictionary<string, JsonInstanceElement>();

        foreach (JsonInstanceProperty property in instance.EnumerateObject())
        {
            properties[property.Name] = property.Value;
        }

        return properties;
    }

    public static HashSet<string> ToPropertyNameSet(this JsonInstanceElement instance, IEqualityComparer<string>? comparer = null)
    {
        var propertyNames = new HashSet<string>(comparer);

        foreach (JsonInstanceProperty property in instance.EnumerateObject())
        {
            propertyNames.Add(property.Name);
        }

        return propertyNames;
    }

    public static JsonInstanceElement[] ToArray(this JsonInstanceElement instance)
    {
        int arrayLength = instance.GetArrayLength();
        if (arrayLength == 0)
        {
            return Array.Empty<JsonInstanceElement>();
        }

        var items = new JsonInstanceElement[arrayLength];
        int idx = 0;

        foreach (JsonInstanceElement item in instance.EnumerateArray())
        {
            items[idx++] = item;
        }

        return items;
    }
}