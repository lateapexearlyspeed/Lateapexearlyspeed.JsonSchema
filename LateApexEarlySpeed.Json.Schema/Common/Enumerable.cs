using System.Collections;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class EnumerableExtensions
{
    public static IEnumerable<TResult> Select<TResult>(this IEnumerable source, Func<object, TResult> selector)
    {
        foreach (object item in source)
        {
            yield return selector(item);
        }
    }

    /// <returns>One of duplicated item if duplication occurs according to <paramref name="keySelector"/>; default value otherwise.</returns>
    public static TSource? FindFirstDuplicatedItem<TSource, TKey>(this ICollection<TSource> collection, Func<TSource, TKey> keySelector)
    {
        if (collection.Count == 0 || collection.Count == 1)
        {
            return default;
        }

        var hash = new HashSet<TKey>(collection.Count);
        foreach (TSource item in collection)
        {
            if (!hash.Add(keySelector(item)))
            {
                return item;
            }
        }

        return default;
    }
}