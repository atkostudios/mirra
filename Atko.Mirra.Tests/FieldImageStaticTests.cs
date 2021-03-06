using System.Reflection;
using NUnit.Framework;

using Atko.Mirra;
using Atko.Mirra.Tests.Utilities;

namespace Atko.Mirra.Tests
{
    [TestFixture]
    public class FieldImageStaticTests
    {
#pragma warning disable 169
#pragma warning disable 649
        public class Class
        {
            public static TestValue PublicField = new TestValue(0);
            public static readonly TestValue PublicReadOnlyField = new TestValue(0);
            internal static TestValue HiddenField = new TestValue(0);
            internal static readonly TestValue HiddenReadOnlyField = new TestValue(0);

            static Class()
            { }
        }
#pragma warning restore 169
#pragma warning restore 649

        [SetUp]
        public void SetUp()
        {
            const BindingFlags bindings = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            typeof(Class).GetField(nameof(Class.PublicField), bindings)?.SetValue(null, new TestValue(0));
            typeof(Class).GetField(nameof(Class.PublicReadOnlyField), bindings)?.SetValue(null, new TestValue(0));
            typeof(Class).GetField(nameof(Class.HiddenField), bindings)?.SetValue(null, new TestValue(0));
            typeof(Class).GetField(nameof(Class.PublicReadOnlyField), bindings)?.SetValue(null, new TestValue(0));
        }

        [Test]
        [TestCase(nameof(Class.PublicField))]
        [TestCase(nameof(Class.PublicReadOnlyField))]
        [TestCase(nameof(Class.HiddenField))]
        [TestCase(nameof(Class.HiddenReadOnlyField))]
        public void TestGetSet(string name)
        {
            var image = typeof(Class).Image().Field(name);

            Assert.AreEqual(new TestValue(0), image.Get(null));

            image.Set(null, new TestValue(1));
            Assert.AreEqual(new TestValue(1), image.Get(null));

            image.Set(null, new TestValue(2));
            Assert.AreEqual(new TestValue(2), image.Get(null));
        }

        [Test]
        [TestCase(nameof(Class.PublicField))]
        [TestCase(nameof(Class.PublicReadOnlyField))]
        [TestCase(nameof(Class.HiddenField))]
        [TestCase(nameof(Class.HiddenReadOnlyField))]
        public void TestInstanceNotNullException(string name)
        {
            var instance = new Class();
            var image = typeof(Class).Image().Field(name);

            Assert.Throws<MirraInvocationArgumentException>(() => image.Get(instance));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Set(instance, new TestValue(1)));
        }

        [Test]
        [TestCase(nameof(Class.PublicField), "")]
        [TestCase(nameof(Class.PublicField), 0)]
        [TestCase(nameof(Class.PublicReadOnlyField), "")]
        [TestCase(nameof(Class.PublicReadOnlyField), 0)]
        [TestCase(nameof(Class.HiddenField), "")]
        [TestCase(nameof(Class.HiddenField), 0)]
        [TestCase(nameof(Class.HiddenReadOnlyField), "")]
        [TestCase(nameof(Class.HiddenReadOnlyField), 0)]
        public void TestArgumentException(string name, object argument)
        {
            var image = typeof(Class).Image().Field(name);

            Assert.Throws<MirraInvocationArgumentException>(() => image.Set(null, argument));
        }
    }
}