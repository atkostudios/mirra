using NUnit.Framework;

using Atko.Mirra;

namespace Atko.Mirra.Tests
{
    [TestFixture]
    public class IndexerImageInstanceTests
    {
        class GetClass
        {
            public int[] Integers { get; } = { 0, 1, 2 };

            int this[int index] => Integers[index];

            int this[int first, int second] => Integers[first] + Integers[second];
        }

        class GetSetClass
        {
            public int[] Integers { get; } = { 0, 1, 2 };

            int this[int index]
            {
                get => Integers[index];
                set => Integers[index] = value;
            }

            int this[int first, int second]
            {
                get => Integers[first] + Integers[second];
                set
                {
                    Integers[first] = value;
                    Integers[second] = value;
                }
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void TestGetSingle(int index)
        {
            var instance = new GetClass();
            var image = typeof(GetClass).Image().Indexer(typeof(int));

            Assert.AreEqual(instance.Integers[index], image.Get(instance, index));

            Assert.Throws<MirraInvocationCannotSetException>(() => image.Set(instance, index, -1));
            Assert.Throws<MirraInvocationCannotSetException>(() => image.Set(instance, index, ""));
            Assert.Throws<MirraInvocationException>(() => image.Get(instance, -1));
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        public void TestGetMultiple(int first, int second)
        {
            var instance = new GetClass();
            var image = typeof(GetClass).Image().Indexer(typeof(int), typeof(int));

            var expected = instance.Integers[first] + instance.Integers[second];
            var actual = image.Get(instance, new object[] { first, second });

            Assert.AreEqual(expected, actual);

            Assert.Throws<MirraInvocationCannotSetException>(() => image.Set(instance, first, new object[] { -1 }));
            Assert.Throws<MirraInvocationCannotSetException>(() => image.Set(instance, first, ""));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Get(instance, first));
            Assert.Throws<MirraInvocationException>(() => image.Get(instance, new object[] { first, -1 }));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void TestGetSetSingle(int index)
        {
            var instance = new GetSetClass();
            var image = typeof(GetSetClass).Image().Indexer(typeof(int));

            image.Set(instance, index, -1);
            Assert.AreEqual(-1, image.Get(instance, index));

            Assert.Throws<MirraInvocationArgumentException>(() => image.Set(instance, "", 1));
            Assert.Throws<MirraInvocationException>(() => image.Set(instance, -1, 1));
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        public void TestGetSetMultiple(int first, int second)
        {
            var instance = new GetSetClass();
            var image = typeof(GetSetClass).Image().Indexer(typeof(int), typeof(int));
            var index = new object[] { first, second };

            image.Set(instance, index, -1);
            Assert.AreEqual(-2, image.Get(instance, index));

            Assert.Throws<MirraInvocationArgumentException>(() => image.Set(instance, new object[] { "", "" }, 1));
            Assert.Throws<MirraInvocationException>(() => image.Set(instance, new object[] { -1, -1 }, 1));
        }
    }
}