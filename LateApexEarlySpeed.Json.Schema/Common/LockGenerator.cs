using System.Collections.Concurrent;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class LockGenerator<TKey>
{
    private readonly ConcurrentDictionary<TKey, object> _locks = new();

    public object GetLock(TKey key)
    {
        return _locks.GetOrAdd(key, _ => new object());
    }
}