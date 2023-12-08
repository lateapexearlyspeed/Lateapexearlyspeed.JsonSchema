using System.Collections.Generic;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class DictionaryExtensions
{
    public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
    {
        return dic.TryGetValue(key, out TValue? value) ? value : default;
    }
}