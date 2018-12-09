using Atko.Dodge.Models;
using NUnit.Framework;

namespace Atko.Dodge.Tests.Models
{
    [TestFixture]
    class ConstructorModelTests
    {
        public class First { }

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
            var model = typeof(First).Model().Constructor();
            var instance = model.Call();
            Assert.IsInstanceOf<First>(instance);
        }

        [Test]
        public void TestArgumentConstructor()
        {
            var model = typeof(Second).Model().Constructor(typeof(int));
            var instance = model.Call(1);
            Assert.IsInstanceOf<Second>(instance);
            Assert.AreEqual(1, ((Second) instance).Value);

            Assert.Throws<DodgeInvocationException>(() => model.Call());
            Assert.Throws<DodgeInvocationException>(() => model.Call((object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call("string"));
        }

        [Test]
        public void TestParamsArgumentConstructor()
        {
            var model = typeof(Third).Model().Constructor(typeof(string), typeof(int[]));
            var instance = model.Call("string", new[] {1, 2, 3});
            Assert.IsInstanceOf<Third>(instance);
            Assert.AreEqual("string", ((Third) instance).Name);
            Assert.AreEqual(new[] {1, 2, 3}, ((Third) instance).Values);

            Assert.Throws<DodgeInvocationException>(() => model.Call());
            Assert.Throws<DodgeInvocationException>(() => model.Call((object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call("string"));
            Assert.Throws<DodgeInvocationException>(() => model.Call("string", 1));
        }
    }
}