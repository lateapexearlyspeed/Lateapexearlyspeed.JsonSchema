using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class RegexMatcher
{
    private static readonly PerKeywordRegexCache PerKeywordRegexProvider = new();
    
    public static GlobalRegexCache GlobalRegexProvider { get; } = new();

    public static RegexLifetime RegexLifetime { get; set; } = RegexLifetime.Global;

    public static bool IsMatch(string pattern, KeywordBase keywordInstance, string input)
    {
        LazyCompiledRegex regex = RegexLifetime == RegexLifetime.Global ? GlobalRegexProvider.Get(pattern) : PerKeywordRegexProvider.Get(keywordInstance, pattern);
        
        return regex.IsMatch(input);
    }
}

public enum RegexLifetime
{
    Global,
    Instance
}