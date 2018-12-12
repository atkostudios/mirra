using Atko.Mirra.Images;
using NUnit.Framework;

namespace Atko.Mirra.Tests.Images
{
    [TestFixture]
    class ConstructorImageTests
    {
        public class First
        { }

        public class Second
        {
            public int Value { get; }

            public Second(int value)
            {
                Value = value;
            }
        }

        public class Third
        {
            public string Name { get; }
            public int[] Values { get; }

            public Third(string name, params int[] values)
            {
                Name = name;
                Values = values;
            }
        }

        [Test]
        public void TestDefaultConstructor()
        {
            var image = typeof(First).Image().Constructor();
            var instance = image.Call();
            Assert.IsInstanceOf<First>(instance);
        }

        [Test]
        public void TestArgumentConstructor()
        {
            var image = typeof(Second).Image().Constructor(typeof(int));
            var instance = image.Call(1);
            Assert.IsInstanceOf<Second>(instance);
            Assert.AreEqual(1, ((Second)instance).Value);

            Assert.Throws<MirraInvocationException>(() => image.Call());
            Assert.Throws<MirraInvocationException>(() => image.Call((object)null));
            Assert.Throws<MirraInvocationException>(() => image.Call("string"));
        }

        [Test]
        public void TestParamsArgumentConstructor()
        {
            var image = typeof(Third).Image().Constructor(typeof(string), typeof(int[]));
            var instance = image.Call("string", new[] { 1, 2, 3 });
            Assert.IsInstanceOf<Third>(instance);
            Assert.AreEqual("string", ((Third)instance).Name);
            Assert.AreEqual(new[] { 1, 2, 3 }, ((Third)instance).Values);

            Assert.Throws<MirraInvocationException>(() => image.Call());
            Assert.Throws<MirraInvocationException>(() => image.Call((object)null));
            Assert.Throws<MirraInvocationException>(() => image.Call("string"));
            Assert.Throws<MirraInvocationException>(() => image.Call("string", 1));
        }
    }
}