namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class RegexMatcher
{
    static RegexMatcher()
    {
        RegexProvider = DefaultRegexProvider;
    }

    public static RegexCache DefaultRegexProvider { get; } = new();

    public static IRegexProvider RegexProvider { get; set; }

    public static bool IsMatch(string pattern, string input)
    {
        LazyCompiledRegex regex = RegexProvider.Get(pattern);

        return regex.IsMatch(input);
    }
}

public interface IRegexProvider
{
    LazyCompiledRegex Get(string pattern);
}