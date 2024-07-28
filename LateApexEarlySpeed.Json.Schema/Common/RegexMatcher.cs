namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class RegexMatcher
{
    public static GlobalRegexCache GlobalRegexProvider { get; } = new();

    public static bool IsMatch(string pattern, string input)
    {
        LazyCompiledRegex regex = GlobalRegexProvider.Get(pattern);
        
        return regex.IsMatch(input);
    }
}
