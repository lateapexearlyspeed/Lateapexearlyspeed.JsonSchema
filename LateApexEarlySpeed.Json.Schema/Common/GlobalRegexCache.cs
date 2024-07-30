using System.Collections.Concurrent;
using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class GlobalRegexCache
{
    private readonly ConcurrentDictionary<string, RegexNode> _regexDic = new(1, 31);
    private readonly List<RegexNode> _regexList;

    private volatile RegexNode? _lastAccessNode;
    private int _removalStartIdx;
    private int _cacheSize = 15;
    private const int RemovalSelectSize = 30;

    private object SyncObj => _regexDic;

    public GlobalRegexCache()
    {
        _regexList = new(_cacheSize);
    }

    public LazyCompiledRegex Get(string pattern)
    {
        long lastAccessTime = 0;

        if (_lastAccessNode is not null)
        {
            RegexNode lastAccessNode = _lastAccessNode;

            if (lastAccessNode.Pattern == pattern)
            {
                return lastAccessNode.Regex;
            }

            lastAccessTime = Volatile.Read(ref lastAccessNode.LastAccessTime);
        }

        if (!_regexDic.TryGetValue(pattern, out RegexNode node))
        {
            node = Add(pattern);
        }

        Volatile.Write(ref node.LastAccessTime, lastAccessTime + 1);
        _lastAccessNode = node;

        return node.Regex;
    }

    private RegexNode Add(string pattern)
    {
        RegexNode? node = new RegexNode(pattern, new LazyCompiledRegex(pattern)); // Create regex instance outside lock because regex creation costs more time

        lock (SyncObj)
        {
            if (_regexList.Count >= CacheSize)
            {
                if (_regexList.Count > RemovalSelectSize)
                {
                    int endIdx = _removalStartIdx + RemovalSelectSize - 1;
                    if (endIdx >= _regexList.Count)
                    {
                        _removalStartIdx = 0;
                        endIdx = RemovalSelectSize - 1;
                    }

                    FindAndRemove(_removalStartIdx, endIdx);

                    _removalStartIdx++;
                }
                else
                {
                    FindAndRemove(0, _regexList.Count - 1);
                }
            }

            if (_regexDic.TryAdd(pattern, node))
            {
                _regexList.Add(node);
                return node;
            }

            node = _regexDic.GetValueOrDefault(pattern);
            Debug.Assert(node is not null);

            return node;
        }
    }

    private void FindAndRemove(int startIdx, int endIdx)
    {
        int oldestNodeIdx = startIdx;

        for (int i = startIdx + 1; i <= endIdx; i++)
        {
            if (Volatile.Read(ref _regexList[i].LastAccessTime) < Volatile.Read(ref _regexList[oldestNodeIdx].LastAccessTime))
            {
                oldestNodeIdx = i;
            }
        }

        RegexNode nodeToRemove = _regexList[oldestNodeIdx];

        _regexList.RemoveAt(oldestNodeIdx);
        _regexDic.TryRemove(nodeToRemove.Pattern, out _);
    }

    public int CacheSize
    {
        get => Volatile.Read(ref _cacheSize);

        set {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(CacheSize)} should be greater than 0");
            }

            lock (SyncObj)
            {
                if (value == _cacheSize)
                {
                    return;
                }

                if (value < _regexList.Count)
                {
                    for (int i = value; i < _regexList.Count; i++)
                    {
                        RegexNode node = _regexList[i];
                        _regexDic.TryRemove(node.Pattern, out _);
                    }

                    _regexList.RemoveRange(value, _regexList.Count - value);

                    Debug.Assert(_regexDic.Count == value);
                    Debug.Assert(_regexList.Count == value);
                }

                _regexList.Capacity = value;
                _cacheSize = value;
            }
        }
    }

    private class RegexNode
    {
        public long LastAccessTime;

        public RegexNode(string pattern, LazyCompiledRegex regex)
        {
            Pattern = pattern;
            Regex = regex;
        }

        public string Pattern { get; }
        public LazyCompiledRegex Regex { get; }
    }
}
