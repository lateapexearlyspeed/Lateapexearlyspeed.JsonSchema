namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class RegexMatcher
{
    public static GlobalRegexCache GlobalRegexProvider { get; } = new();

    public static bool IsMatch(string pattern, string input, TimeSpan matchTimeout)
    {
        LazyCompiledRegex regex = GlobalRegexProvider.Get(pattern, matchTimeout);
        
        return regex.IsMatch(input);
    }
}
