using System.Collections.Concurrent;

namespace Atko.Dodge.Utility
{
    class Cache<TKey, TValue> : ConcurrentDictionary<TKey, TValue> { }
}