using System.Collections;

namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// An immutable type
/// <remarks> Refer to: https://datatracker.ietf.org/doc/html/rfc6901 </remarks>
/// </summary>
public class ArrayBasedImmutableJsonPointer : ImmutableJsonPointer
{
    /// <summary>
    /// Unescaped reference tokens
    /// </summary>
    private readonly string[] _referenceTokens;

    internal ArrayBasedImmutableJsonPointer(IReadOnlyCollection<string> unescapedTokenCollection)
    {
        _referenceTokens = new string[unescapedTokenCollection.Count];

        int i = 0;
        foreach (string unescapedToken in unescapedTokenCollection)
        {
            _referenceTokens[i++] = unescapedToken;
        }
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(((IEnumerable<string>)_referenceTokens).GetEnumerator());
    }

    protected override IEnumerator<string> GetEnumeratorInternal()
    {
        // ReSharper disable NotDisposedResourceIsReturned
        return GetEnumerator();
        // ReSharper restore NotDisposedResourceIsReturned
    }

    public readonly struct Enumerator : IEnumerator<string>
    {
        private readonly IEnumerator<string> _underlyingEnumerator;

        internal Enumerator(IEnumerator<string> enumerator)
        {
            _underlyingEnumerator = enumerator;
        }

        public bool MoveNext()
        {
            return _underlyingEnumerator.MoveNext();
        }

        public void Reset()
        {
            _underlyingEnumerator.Reset();
        }

        public string Current => _underlyingEnumerator.Current;

        object? IEnumerator.Current => ((IEnumerator)_underlyingEnumerator).Current;

        public void Dispose()
        {
            _underlyingEnumerator.Dispose();
        }
    }

    protected override IReadOnlyList<string> ReferenceTokens => _referenceTokens;
}