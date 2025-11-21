using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// An immutable type
/// <remarks> Refer to: https://datatracker.ietf.org/doc/html/rfc6901 </remarks>
/// </summary>
public class LinkedListBasedImmutableJsonPointer : ImmutableJsonPointer
{
    public static LinkedListBasedImmutableJsonPointer Empty { get; } = new(new SingleLinkedList<string>());

    /// <summary>
    /// Unescaped reference tokens
    /// </summary>
    private readonly SingleLinkedList<string> _referenceTokens;

    private LinkedListBasedImmutableJsonPointer(SingleLinkedList<string> referenceTokens)
    {
        _referenceTokens = referenceTokens;
    }

    /// <returns>If <paramref name="escapedJsonPointerString"/> is an invalid json pointer format, return null.</returns>
    public static LinkedListBasedImmutableJsonPointer? Create(string escapedJsonPointerString)
    {
        // Invalid json pointer format, return null
        if (!string.IsNullOrEmpty(escapedJsonPointerString) && escapedJsonPointerString[0] != TokenPrefixChar)
        {
            return null;
        }

        var referenceTokens = new SingleLinkedList<string>();

        int curIdx = 0;
        while (curIdx < escapedJsonPointerString.Length)
        {
            curIdx++;

            int nextPrefixIdx = escapedJsonPointerString.IndexOf(TokenPrefixChar, curIdx);

            // Cannot find '/', so set 'nextPrefixIdx' to end of 'escapedJsonPointerString',
            // then eventually 'curIdx' will also be at the end of 'escapedJsonPointerString', and exit loop.
            if (nextPrefixIdx == -1)
            {
                nextPrefixIdx = escapedJsonPointerString.Length;
            }

            string escapedReferenceToken = escapedJsonPointerString.Substring(curIdx, nextPrefixIdx - curIdx);
            string? unescapedReferenceToken = UnescapeReferenceToken(escapedReferenceToken);
            if (unescapedReferenceToken is null)
            {
                return null;
            }

            referenceTokens.Add(unescapedReferenceToken);

            curIdx = nextPrefixIdx;
        }

        return new LinkedListBasedImmutableJsonPointer(referenceTokens);
    }

    /// <returns>
    /// If <paramref name="escapedReferenceToken"/> is not a valid 'escaped' reference token value, return null.
    /// </returns>
    private static string? UnescapeReferenceToken(string escapedReferenceToken)
    {
        bool findTildeChar = false;
        int idx = 0;

        while (true)
        {
            int tildeIdx = escapedReferenceToken.IndexOf('~', idx);

            if (tildeIdx == -1)
            {
                break;
            }

            findTildeChar = true;
            int nextIdx = tildeIdx + 1;

            // Invalid '~' in escapedReferenceToken
            if (nextIdx >= escapedReferenceToken.Length || (escapedReferenceToken[nextIdx] != '1' && escapedReferenceToken[nextIdx] != '0'))
            {
                return null;
            }

            idx = tildeIdx + 2;
        }

        return findTildeChar 
            ? escapedReferenceToken.Replace("~1", TokenPrefixCharString).Replace("~0", "~") 
            : escapedReferenceToken;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(_referenceTokens.GetEnumerator());
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

        internal Enumerator(SingleLinkedList<string>.Enumerator enumerator)
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

    /// <summary>
    /// This method will not modify current instance, it is an immutable operation
    /// </summary>
    /// <remarks>Based on benchmark, this method is hot path, so we create <see cref="SingleLinkedList{T}"/> type which can share common path nodes for several <see cref="SingleLinkedList{T}"/> instances inside same path</remarks>
    /// <returns>Newly created <see cref="LinkedListBasedImmutableJsonPointer"/> instance.</returns>
    public LinkedListBasedImmutableJsonPointer Add(string unescapedReferenceToken)
    {
        return new LinkedListBasedImmutableJsonPointer(_referenceTokens.CreateByAppend(unescapedReferenceToken));
    }

    /// <summary>
    /// This method will not modify current instance, it is an immutable operation
    /// </summary>
    /// <returns>Newly created <see cref="LinkedListBasedImmutableJsonPointer"/> instance.</returns>
    public LinkedListBasedImmutableJsonPointer Add(int arrayItemIdx)
    {
        return new LinkedListBasedImmutableJsonPointer(_referenceTokens.CreateByAppend(arrayItemIdx.ToString()));
    }

    protected override IReadOnlyList<string> ReferenceTokens => _referenceTokens;
}

internal class SingleLinkedList<T> : IReadOnlyList<T>
{
    private SingleLinkedNode<T>? _head;

    public int Count { get; private set; }

    private SingleLinkedNode<T>[]? _nodesCache;

    public SingleLinkedList(IEnumerable<T> collection)
    {
        foreach (T value in collection)
        {
            _head = new SingleLinkedNode<T>(value, _head);
            Count++;
        }
    }

    public SingleLinkedList()
    {
    }

    private SingleLinkedList(SingleLinkedNode<T>? head, int count)
    {
        _head = head;
        Count = count;
    }

    /// <summary>
    /// Create new <see cref="SingleLinkedList{T}"/> instance without modifying original <see cref="SingleLinkedList{T}"/> instance
    /// </summary>
    public SingleLinkedList<T> CreateByAppend(T value)
    {
        SingleLinkedNode<T> newHead = new SingleLinkedNode<T>(value, _head);

        return new SingleLinkedList<T>(newHead, Count + 1);
    }

    /// <summary>
    /// Modify original <see cref="SingleLinkedList{T}"/>
    /// </summary>
    public void Add(T value)
    {
        _head = new SingleLinkedNode<T>(value, _head);
        Count++;
        InvalidateNodesCache();
    }

    private void InvalidateNodesCache()
    {
        _nodesCache = null;
    }

    public T this[int index]
    {
        get
        {
            CreateNodesCacheIfInvalidated();

            return _nodesCache[index].Value;
        }
    }

    [MemberNotNull(nameof(_nodesCache))]
    private void CreateNodesCacheIfInvalidated()
    {
        if (_nodesCache is not null)
        {
            return;
        }

        _nodesCache = new SingleLinkedNode<T>[Count];
        SingleLinkedNode<T>? curNode = _head;
        int i = Count - 1;
        while (curNode is not null)
        {
            _nodesCache[i--] = curNode;
            curNode = curNode.Next;
        }
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly SingleLinkedList<T> _list;
        private int _idx = -1;

        /// <remarks>Don't call Enumerable related methods on <paramref name="list"/> here</remarks>
        public Enumerator(SingleLinkedList<T> list)
        {
            _list = list;
            list.CreateNodesCacheIfInvalidated();

            Current = default!;
        }

        public T Current { get; private set; }

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (++_idx < _list.Count)
            {
                Debug.Assert(_list._nodesCache is not null);
                Current = _list._nodesCache[_idx].Value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _idx = -1;
        }

        public void Dispose()
        {
        }
    }
}

internal class SingleLinkedNode<T>
{
    public SingleLinkedNode(T value, SingleLinkedNode<T>? next = null)
    {
        Value = value;
        Next = next;
    }

    public T Value { get; }
    public SingleLinkedNode<T>? Next { get; set; }
}