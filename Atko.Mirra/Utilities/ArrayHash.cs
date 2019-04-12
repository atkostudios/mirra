namespace Atko.Mirra.Utilities
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
            return Array.ElementsAreEqual(other.Array);
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
            unchecked
            {
                var code = 0;
                foreach (var element in Array)
                {
                    code = (code * 397) ^ (element?.GetHashCode() ?? 0);
                }

                return code;
            }
        }
    }
}