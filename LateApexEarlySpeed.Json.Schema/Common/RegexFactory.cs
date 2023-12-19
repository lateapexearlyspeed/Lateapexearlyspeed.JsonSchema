using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class RegexFactory
{
    public static Regex Create([StringSyntax(StringSyntaxAttribute.Regex, "options")] string pattern, RegexOptions options)
    {
        return new Regex(pattern, options, TimeSpan.FromMilliseconds(200));
    }
}