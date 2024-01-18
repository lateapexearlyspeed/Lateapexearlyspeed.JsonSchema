using System.Diagnostics;
using System.Text;

namespace LateApexEarlySpeed.Json.Schema.Generator;

internal class JsonSchemaNamingPolicyHelper
{
    public static string ConvertNameByTransformingCasingAndSeparator(string name, bool toLowerOrUpper, char separator)
    {
        if (name.Length == 0)
        {
            return name;
        }

        List<ReadOnlyMemory<char>> words = name.SplitWords();
        Debug.Assert(words.Count != 0);

        var sb = new StringBuilder();

        foreach (ReadOnlyMemory<char> word in words)
        {
            var transformedWord = new char[word.Length];
            int result = toLowerOrUpper
                ? word.Span.ToLowerInvariant(transformedWord)
                : word.Span.ToUpperInvariant(transformedWord);
            Debug.Assert(result == word.Length);

            sb.Append(transformedWord);
            sb.Append(separator);
        }

        Debug.Assert(sb.Length != 0 && sb[^1] == separator);
        return sb.ToString(0, sb.Length - 1);
    }
}