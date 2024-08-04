using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace LateApexEarlySpeed.Json.Schema.Common;

public static class RegexFactory
{
    /// <summary>
    /// Gets default timeout value that specifies how long a pattern matching method should attempt a match before it times out
    /// </summary>
    /// <returns>The value is 200 milliseconds</returns>
    public static TimeSpan DefaultMatchTimeout { get; } = TimeSpan.FromMilliseconds(200);

    internal static Regex Create([StringSyntax(StringSyntaxAttribute.Regex, "options")] string pattern, RegexOptions options)
    {
        return new Regex(pattern, options, DefaultMatchTimeout);
    }

    internal static Regex Create([StringSyntax(StringSyntaxAttribute.Regex, "options")] string pattern, RegexOptions options, TimeSpan matchTimeout)
    {
        return new Regex(pattern, options, matchTimeout);
    }
}