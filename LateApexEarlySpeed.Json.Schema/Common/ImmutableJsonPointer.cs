using System.Collections;
using System.Diagnostics;
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
    public static ImmutableJsonPointer Empty { get; } = new(Enumerable.Empty<string>());

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

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ImmutableJsonPointer)obj);
    }

    protected bool Equals(ImmutableJsonPointer other)
    {
        return _referenceTokens.SequenceEqual(other._referenceTokens);
    }

    public override int GetHashCode()
    {
        if (_referenceTokens.Count >= 7)
        {
            return HashCode.Combine(_referenceTokens.Count, _referenceTokens[0], _referenceTokens[1], _referenceTokens[2], _referenceTokens[3], _referenceTokens[4], _referenceTokens[5], _referenceTokens[6]);
        }

        if (_referenceTokens.Count >= 3)
        {
            return HashCode.Combine(_referenceTokens.Count, _referenceTokens[0], _referenceTokens[1], _referenceTokens[2]);
        }

        switch (_referenceTokens.Count)
        {
            case 2:
                return HashCode.Combine(_referenceTokens.Count, _referenceTokens[0], _referenceTokens[1]);
            case 1:
                return HashCode.Combine(_referenceTokens.Count, _referenceTokens[0]);
            default:
                Debug.Assert(_referenceTokens.Count == 0);
                return HashCode.Combine(_referenceTokens.Count);
        }
    }

    public static bool operator ==(ImmutableJsonPointer? left, ImmutableJsonPointer? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ImmutableJsonPointer? left, ImmutableJsonPointer? right)
    {
        return !Equals(left, right);
    }
}