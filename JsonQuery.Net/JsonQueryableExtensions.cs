using System.Text.Json;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

public static class JsonQueryableExtensions
{
    public static string GetKeyword(this IJsonQueryable queryable)
    {
        return JsonQueryableRegistry.GetKeyword(queryable.GetType());
    }

    /// <summary>
    /// Serialize query engine to json format query
    /// </summary>
    /// <param name="queryable">query engine</param>
    /// <returns>json format query string</returns>
    public static string SerializeToJsonFormat(this IJsonQueryable queryable)
    {
        return JsonSerializer.Serialize(queryable);
    }
}