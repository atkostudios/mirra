using System.Collections.Concurrent;

namespace Atko.Dodge.Utility
{
    public class Cache<TKey, TValue> : ConcurrentDictionary<TKey, TValue> { }
}