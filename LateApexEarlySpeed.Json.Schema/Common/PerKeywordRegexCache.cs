using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class PerKeywordRegexCache
{
    private readonly ConditionalWeakTable<KeywordBase, ConcurrentDictionary<string, LazyCompiledRegex>> _regexCache = new();

    public LazyCompiledRegex Get(KeywordBase keywordInstance, string pattern)
    {
        ConcurrentDictionary<string, LazyCompiledRegex> patterns = _regexCache.GetValue(keywordInstance, _ => new ConcurrentDictionary<string, LazyCompiledRegex>());

        return patterns.GetOrAdd(pattern, p => new LazyCompiledRegex(p));
    }
}