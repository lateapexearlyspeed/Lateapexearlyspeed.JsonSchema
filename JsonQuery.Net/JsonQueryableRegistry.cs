using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

public static class JsonQueryableRegistry
{
    private static readonly Dictionary<string, Type> JsonQueryables = new();
    private static readonly Dictionary<Type, string> QueryTypeKeywordsMap = new();

    static JsonQueryableRegistry()
    {
        IEnumerable<Type> builtInQueryTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && typeof(IJsonQueryable).IsAssignableFrom(type) && type != typeof(ConstQueryable));

        foreach (Type builtInQueryType in builtInQueryTypes)
        {
            string keyword = (string)builtInQueryType.GetField("Keyword", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue();
            AddQueryableType(keyword, builtInQueryType);
        }
    }

    public static bool TryGetQueryableType(string keyword, [NotNullWhen(true)] out Type? queryType) => JsonQueryables.TryGetValue(keyword, out queryType);

    internal static string GetKeyword(Type queryType)
    {
        return QueryTypeKeywordsMap[queryType];
    }

    public static void AddQueryableType<TQuery>(string keyword) where TQuery : IJsonQueryable
    {
        AddQueryableType(keyword, typeof(TQuery));
    }

    private static void AddQueryableType(string keyword, Type queryType)
    {
        if (JsonQueryables.ContainsKey(keyword) || QueryTypeKeywordsMap.ContainsKey(queryType))
        {
            throw new ArgumentException($"There is already same keyword:{keyword} or query type existing in {nameof(JsonQueryableRegistry)}");
        }

        JsonQueryables.Add(keyword, queryType);
        QueryTypeKeywordsMap.Add(queryType, keyword);
    }
}