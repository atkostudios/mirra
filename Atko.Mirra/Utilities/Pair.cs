using System.Collections.Generic;

namespace Atko.Mirra.Utilities
{
    struct Pair<TFirst, TSecond>
    {
        public static bool operator ==(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
        {
            return !(left == right);
        }

        public TFirst First { get; }
        public TSecond Second { get; }

        public Pair(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }

        public bool Equals(Pair<TFirst, TSecond> pair)
        {
            return
                EqualityComparer<TFirst>.Default.Equals(First, pair.First) &&
                EqualityComparer<TSecond>.Default.Equals(Second, pair.Second);
        }

        public override bool Equals(object obj)
        {
            return obj is Pair<TFirst, TSecond> pair && Equals(pair);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (First.GetHashCode() * 397) ^ Second.GetHashCode();
            }
        }
    }
}