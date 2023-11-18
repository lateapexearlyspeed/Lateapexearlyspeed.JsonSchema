using System.Collections;
using System.Text;

namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// An immutable type
/// <remarks> Refer to: https://datatracker.ietf.org/doc/html/rfc6901 </remarks>
/// </summary>
public class ImmutableJsonPointer : IEnumerable<string>
{
    private const string TokenPrefixCharString = "/";

    private static char TokenPrefixChar => TokenPrefixCharString[0];

    /// <summary>
    /// Unescaped reference tokens
    /// </summary>
    private readonly List<string> _referenceTokens;

    internal ImmutableJsonPointer(IEnumerable<string> unescapedTokenCollection)
    {
        _referenceTokens = unescapedTokenCollection.ToList();
    }

    private ImmutableJsonPointer(string escapedJsonPointerString)
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
    public static ImmutableJsonPointer? Create(string escapedJsonPointerString)
    {
        // Invalid json pointer format, return null
        if (!string.IsNullOrEmpty(escapedJsonPointerString) && escapedJsonPointerString[0] != TokenPrefixChar)
        {
            return null;
        }

        return new ImmutableJsonPointer(escapedJsonPointerString);
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

    /// <summary>
    /// This method will not modify current instance, it is a immutable operation
    /// </summary>
    /// <returns>Newly created <see cref="ImmutableJsonPointer"/> instance.</returns>
    public ImmutableJsonPointer Add(string unescapedReferenceToken)
    {
        return new ImmutableJsonPointer(this.Append(unescapedReferenceToken));
    }

    /// <summary>
    /// This method will not modify current instance, it is a immutable operation
    /// </summary>
    /// <returns>Newly created <see cref="ImmutableJsonPointer"/> instance.</returns>
    public ImmutableJsonPointer Add(int arrayItemIdx)
    {
        return new ImmutableJsonPointer(this.Append(arrayItemIdx.ToString()));
    }
}