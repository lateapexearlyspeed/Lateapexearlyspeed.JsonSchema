using System.Collections;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class NonGenericEnumerableExtensions
{
    public static IEnumerable<TResult> Select<TResult>(this IEnumerable source, Func<object, TResult> selector)
    {
        foreach (object item in source)
        {
            yield return selector(item);
        }
    }
}