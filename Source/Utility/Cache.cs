using System.Collections.Concurrent;

namespace Atko.Mirra.Utility
{
    class Cache<TKey, TValue> : ConcurrentDictionary<TKey, TValue> { }
}