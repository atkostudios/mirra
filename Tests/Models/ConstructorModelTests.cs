using Atko.Dodge.Models;
using NUnit.Framework;

namespace Atko.Dodge.Tests.Models
{
    [TestFixture]
    class ConstructorModelTests
    {
        #pragma warning disable 169

        public class First { }

        public class Second
        {
            public int Value { get; }

            public Second(int value)
            {
                Value = value;
            }
        }

        #pragma warning restore 169

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
            Assert.AreEqual(((Second) instance).Value, 1);
        }
    }
}