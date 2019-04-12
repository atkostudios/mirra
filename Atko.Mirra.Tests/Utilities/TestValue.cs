namespace Atko.Mirra.Tests.Utilities
{
    public struct TestValue
    {
        public int Value;

        public TestValue(int value)
        {
            Value = value;
        }

        public bool Equals(TestValue other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TestValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return $"{nameof(TestValue)}({Value})";
        }
    }
}