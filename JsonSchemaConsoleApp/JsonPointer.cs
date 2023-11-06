using System.Text;

namespace JsonSchemaConsoleApp;

/// <summary>
/// <remarks> Refer to: https://datatracker.ietf.org/doc/html/rfc6901 </remarks>
/// </summary>
public class JsonPointer
{
    private const string TokenPrefixCharString = "/";

    private char TokenPrefixChar => TokenPrefixCharString[0];

    private readonly List<string> _referenceTokens;

    public JsonPointer(IEnumerable<string> unescapedTokenCollection)
    {
        _referenceTokens = unescapedTokenCollection.ToList();
    }

    public JsonPointer(string escapedJsonPointerString)
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

    public int Count => _referenceTokens.Count;

    public string GetReferenceToken(int index)
    {
        return _referenceTokens[index];
    }

    public void AddReferenceToken(string unescapedRefToken)
    {
        _referenceTokens.Add(unescapedRefToken);
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