using System.Collections.Generic;

namespace Atko.Dodge.Utility
{
    public static class EnumerableUtility
    {
        public static IEnumerable<T> Iterate<T>(this IEnumerable<T> enumerable)
        {
            foreach (var element in enumerable)
            {
                yield return element;
            }
        }
    }
}