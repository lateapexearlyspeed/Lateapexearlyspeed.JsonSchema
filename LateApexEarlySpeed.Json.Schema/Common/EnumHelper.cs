using System.Collections.Concurrent;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class EnumHelper<TEnum> where TEnum : Enum
{
    private static readonly ConcurrentDictionary<TEnum, string> EnumNamesCache = new(Environment.ProcessorCount, Enum.GetValues(typeof(TEnum)).Length);

    public static string GetCachedStringName(TEnum enumValue)
    {
        return EnumNamesCache.GetOrAdd(enumValue, enumArg => enumArg.ToString());
    }
}