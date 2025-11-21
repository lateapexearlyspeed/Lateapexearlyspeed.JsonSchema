using System.Collections;
using System.Diagnostics;
using System.Text;

namespace LateApexEarlySpeed.Json.Schema.Common;

public abstract class ImmutableJsonPointer : IReadOnlyList<string>
{
    protected const string TokenPrefixCharString = "/";

    protected static char TokenPrefixChar => TokenPrefixCharString[0];

    protected abstract IReadOnlyList<string> ReferenceTokens { get; }

    public override int GetHashCode()
    {
        if (ReferenceTokens.Count >= 7)
        {
            return HashCode.Combine(ReferenceTokens.Count, ReferenceTokens[0], ReferenceTokens[1], ReferenceTokens[2], ReferenceTokens[3], ReferenceTokens[4], ReferenceTokens[5], ReferenceTokens[6]);
        }

        if (ReferenceTokens.Count >= 3)
        {
            return HashCode.Combine(ReferenceTokens.Count, ReferenceTokens[0], ReferenceTokens[1], ReferenceTokens[2]);
        }

        switch (ReferenceTokens.Count)
        {
            case 2:
                return HashCode.Combine(ReferenceTokens.Count, ReferenceTokens[0], ReferenceTokens[1]);
            case 1:
                return HashCode.Combine(ReferenceTokens.Count, ReferenceTokens[0]);
            default:
                Debug.Assert(ReferenceTokens.Count == 0);
                return HashCode.Combine(ReferenceTokens.Count);
        }
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not ImmutableJsonPointer other) return false;

        return ReferenceTokens.SequenceEqual(other.ReferenceTokens);
    }

    public static bool operator ==(ImmutableJsonPointer? left, ImmutableJsonPointer? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ImmutableJsonPointer? left, ImmutableJsonPointer? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (string referenceToken in ReferenceTokens)
        {
            sb.Append(TokenPrefixChar).Append(EscapeReferenceToken(referenceToken));
        }

        return sb.ToString();
    }

    private static string EscapeReferenceToken(string referenceToken)
    {
        return referenceToken.Replace("~", "~0").Replace(TokenPrefixCharString, "~1");
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumeratorInternal();
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
        return GetEnumeratorInternal();
    }

    protected abstract IEnumerator<string> GetEnumeratorInternal();

    public int Count => ReferenceTokens.Count;

    public string this[int index] => ReferenceTokens[index];
}