using System.Collections;
using System.Text;

namespace JsonSchemaConsoleApp;

/// <summary>
/// <remarks> Refer to: https://datatracker.ietf.org/doc/html/rfc6901 </remarks>
/// </summary>
public class JsonPointer : IEnumerable<string>
{
    private const string TokenPrefixCharString = "/";

    private static char TokenPrefixChar => TokenPrefixCharString[0];

    private readonly List<string> _referenceTokens;

    internal JsonPointer(IEnumerable<string> unescapedTokenCollection)
    {
        _referenceTokens = unescapedTokenCollection.ToList();
    }

    private JsonPointer(string escapedJsonPointerString)
    {
        _referenceTokens = new List<string>();

        int curIdx = 0;
        while (curIdx < escapedJsonPointerString.Length && escapedJsonPointerString[curIdx] == TokenPrefixChar)
        {
            curIdx++;

            int nextPrefixIdx = escapedJsonPointerString.IndexOf(TokenPrefixChar, curIdx);
            if (nextPrefixIdx != -1)
            {
                string escapedReferenceToken = escapedJsonPointerString.Substring(curIdx, nextPrefixIdx - curIdx);
                _referenceTokens.Add(UnescapeReferenceToken(escapedReferenceToken));

                curIdx = nextPrefixIdx;
            }
            else
            {
                string escapedReferenceToken = escapedJsonPointerString.Substring(curIdx, escapedJsonPointerString.Length - curIdx);
                _referenceTokens.Add(UnescapeReferenceToken(escapedReferenceToken));

                curIdx = escapedJsonPointerString.Length;
            }
        }

        // _referenceTokens = escapedJsonPointerString.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private static string UnescapeReferenceToken(string escapedReferenceToken)
    {
        return escapedReferenceToken.Replace("~1", TokenPrefixCharString).Replace("~0", "~");
    }

    /// <returns>If <paramref name="escapedJsonPointerString"/> is an invalid json pointer format, return null.</returns>
    public static JsonPointer? Create(string escapedJsonPointerString)
    {
        // Invalid json pointer format, return null
        if (!string.IsNullOrEmpty(escapedJsonPointerString) && escapedJsonPointerString[0] != TokenPrefixChar)
        {
            return null;
        }

        return new JsonPointer(escapedJsonPointerString);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<string> GetEnumerator()
    {
        return _referenceTokens.GetEnumerator();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (string referenceToken in _referenceTokens)
        {
            sb.Append(TokenPrefixChar).Append(EscapeReferenceToken(referenceToken));
        }

        return sb.ToString();
    }

    private static string EscapeReferenceToken(string referenceToken)
    {
        return referenceToken.Replace("~", "~0").Replace(TokenPrefixCharString, "~1");
    }
}