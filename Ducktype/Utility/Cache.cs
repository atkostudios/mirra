using System.Collections.Concurrent;

namespace Utility
{
    public class Cache<TKey, TValue> : ConcurrentDictionary<TKey, TValue> { }
}