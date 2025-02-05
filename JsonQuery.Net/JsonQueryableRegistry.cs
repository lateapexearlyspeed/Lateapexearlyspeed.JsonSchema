using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

internal static class JsonQueryableRegistry
{
    private static readonly Dictionary<string, Type> JsonQueryables;
    private static readonly Dictionary<Type, string> QueryTypeKeywordsMap;

    static JsonQueryableRegistry()
    {
        JsonQueryables = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && typeof(IJsonQueryable).IsAssignableFrom(type) && type != typeof(ConstQueryable)).ToDictionary(type => (string)type.GetField("Keyword", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue());
        QueryTypeKeywordsMap = JsonQueryables.ToDictionary(kv => kv.Value, kv => kv.Key);
    }

    public static bool TryGetQueryableType(string keyword, [NotNullWhen(true)] out Type? queryType) => JsonQueryables.TryGetValue(keyword, out queryType);

    public static string GetKeyword(Type queryType)
    {
        return QueryTypeKeywordsMap[queryType];
    }
}