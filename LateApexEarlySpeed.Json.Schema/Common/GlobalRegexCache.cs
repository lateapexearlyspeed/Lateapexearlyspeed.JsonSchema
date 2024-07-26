using System.Collections.Concurrent;
using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class GlobalRegexCache
{
    private long _lastAccessTime;
    private readonly ConcurrentDictionary<string, RegexNode> _regexDic = new(1, 31);
    private readonly List<RegexNode> _regexList = new();
    private int _removalStartIdx;
    private int _cacheSize = 15;
    private int _removalSelectSize = 30;

    public LazyCompiledRegex Get(string pattern)
    {
        if (_regexDic.TryGetValue(pattern, out RegexNode node))
        {
            node.LastAccessTime = Interlocked.Increment(ref _lastAccessTime);

            return node.Regex;
        }

        node = Add(pattern);
        node.LastAccessTime = Interlocked.Increment(ref _lastAccessTime);
        
        return node.Regex;
    }

    private RegexNode Add(string pattern)
    {
        RegexNode? node = new RegexNode(pattern, new LazyCompiledRegex(pattern)); // Create regex instance outside lock because regex creation costs more time

        lock (_regexDic)
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
            if (_regexList[i].LastAccessTime < _regexList[oldestNodeIdx].LastAccessTime)
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
        get => _cacheSize;
        set {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(CacheSize)} should be greater than 0");
            }

            lock (_regexDic)
            {
                _cacheSize = value;
            }
        }
    }

    public int RemovalSelectSize
    {
        get => _removalSelectSize;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(RemovalSelectSize)} should be greater than 0");
            }

            lock (_regexDic)
            {
                _removalSelectSize = value;
            }
        }
    }

    private class RegexNode
    {
        public RegexNode(string pattern, LazyCompiledRegex regex)
        {
            Pattern = pattern;
            Regex = regex;
        }

        public string Pattern { get; }
        public LazyCompiledRegex Regex { get; }
        public long LastAccessTime { get; set; }
    }
}
