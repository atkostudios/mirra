using System.Collections.Generic;

namespace Utility
{
    struct ArrayHash<T>
    {
        public T[] Array { get; }

        public ArrayHash(T[] elements)
        {
            Array = elements;
        }

        public bool Equals(ArrayHash<T> other)
        {
            if (Array.Length != other.Array.Length)
            {
                return false;
            }

            for (var i = 0; i < Array.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(Array[i], other.Array[i]))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ArrayHash<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            var code = 0;
            foreach (var element in Array)
            {
                code = (code * 397) ^ element?.GetHashCode() ?? 1;
            }

            return code;
        }
    }
}