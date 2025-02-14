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

    /// <summary>
    /// Gets the <see cref="IJsonQueryable"/> type associated with the specified <paramref name="keyword"/>.
    /// </summary>
    /// <param name="keyword">The keyword of <see cref="IJsonQueryable"/> type to get</param>
    /// <param name="queryType">When this method returns, contains <see cref="IJsonQueryable"/> type associated with the specified <paramref name="keyword"/>, if the <paramref name="keyword"/> is found; otherwise, null. This parameter is passed uninitialized.</param>
    /// <returns>true if <see cref="JsonQueryableRegistry"/> contains an <see cref="IJsonQueryable"/> type with the specified <paramref name="keyword"/>; otherwise, false.</returns>
    public static bool TryGetQueryableType(string keyword, [NotNullWhen(true)] out Type? queryType) => JsonQueryables.TryGetValue(keyword, out queryType);

    /// <summary>
    /// Gets the keyword associated with specified <paramref name="queryType"/>
    /// </summary>
    /// <param name="queryType"><see cref="IJsonQueryable"/> type of keyword to get</param>
    /// <returns>The keyword associated with specified <paramref name="queryType"/></returns>
    public static string GetKeyword(Type queryType)
    {
        return QueryTypeKeywordsMap[queryType];
    }

    /// <summary>
    /// Adds the specified <paramref name="keyword"/> and <typeparamref name="TQuery"/> type to the <see cref="JsonQueryableRegistry"/>.
    /// </summary>
    /// <typeparam name="TQuery">New query type to add</typeparam>
    /// <param name="keyword">New keyword to add</param>
    /// <exception cref="ArgumentException">Same <paramref name="keyword"/> or same <typeparamref name="TQuery"/> type already exists in the <see cref="JsonQueryableRegistry"/></exception>
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