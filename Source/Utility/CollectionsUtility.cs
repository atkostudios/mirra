using System;
using System.Collections.Generic;

namespace Atko.Mirra.Utility
{
    static class CollectionsUtility
    {
        public static IEnumerable<T> Iterate<T>(this IEnumerable<T> enumerable)
        {
            foreach (var element in enumerable)
            {
                yield return element;
            }
        }

        public static T[] CopyArray<T>(this T[] array)
        {
            var size = array.Length;
            var copy = new T[size];
            Array.Copy(array, copy, size);
            return copy;
        }

        public static bool ElementsAreEqual<T>(this IReadOnlyList<T> collection, IReadOnlyList<T> other)
        {
            if (collection.Count != other.Count)
            {
                return false;
            }

            for (var i = 0; i < collection.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(collection[i], other[i]))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public static Dictionary<TKey, TValue> ToDictionaryByFirst<TKey, TValue>(this IEnumerable<TValue> enumerable,
            Func<TValue, TKey> selector)
        {
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var value in enumerable)
            {
                var key = selector(value);
                if (dictionary.ContainsKey(key))
                {
                    continue;
                }

                dictionary[key] = value;
            }

            return dictionary;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue> factory)
        {
            if (dictionary.TryGetValue(key, out var cached))
            {
                return cached;
            }

            return dictionary[key] = factory();
        }
    }
}