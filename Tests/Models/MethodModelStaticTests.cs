using System.Linq;
using Atko.Dodge.Models;
using NUnit.Framework;

namespace Atko.Dodge.Tests.Models
{
    [TestFixture]
    class MethodModelStaticTests
    {
        class Class
        {
            public static void PublicArgumentlessMethodWithVoidReturn() { }

            internal static void HiddenArgumentlessMethodWithVoidReturn() { }

            public static int PublicArgumentlessMethod()
            {
                return 1;
            }

            internal static int HiddenArgumentlessMethod()
            {
                return 1;
            }

            public static int PublicMethod(int value)
            {
                return value + 1;
            }

            internal static int HiddenMethod(int value)
            {
                return value + 1;
            }

            public static int PublicParamsMethod(string content, int[] value)
            {
                return content.Length + value.Sum();
            }

            internal static int HiddenParamsMethod(string content, int[] value)
            {
                return content.Length + value.Sum();
            }
        }

        [Test]
        [TestCase(nameof(Class.PublicArgumentlessMethodWithVoidReturn))]
        [TestCase(nameof(Class.HiddenArgumentlessMethodWithVoidReturn))]
        public void TestArgumentlessMethodWithVoidReturn(string name)
        {
            var model = typeof(Class).Model().Method(name);
            var result = model.Call(null);

            Assert.IsNull(result);

            Assert.Throws<DodgeInvocationException>(() => model.Call(null, (object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, 1));
            Assert.Throws<DodgeInvocationException>(() => model.Call(new Class()));
        }

        [Test]
        [TestCase(nameof(Class.PublicArgumentlessMethod))]
        [TestCase(nameof(Class.HiddenArgumentlessMethod))]
        public void TestArgumentlessMethod(string name)
        {
            var model = typeof(Class).Model().Method(name);
            var result = model.Call(null);

            Assert.AreEqual(1, result);

            Assert.Throws<DodgeInvocationException>(() => model.Call(null, (object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, 1));
            Assert.Throws<DodgeInvocationException>(() => model.Call(new Class()));
        }

        [Test]
        [TestCase(nameof(Class.PublicMethod))]
        [TestCase(nameof(Class.HiddenMethod))]
        public void TestMethod(string name)
        {
            var model = typeof(Class).Model().Method(name, typeof(int));
            var result = model.Call(null, 1);

            Assert.AreEqual(2, result);

            Assert.Throws<DodgeInvocationException>(() => model.Call(null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, 1, 1));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, (object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, "string"));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, "string", 2));
            Assert.Throws<DodgeInvocationException>(() => model.Call(new Class(), 1));
        }

        [Test]
        [TestCase(nameof(Class.PublicParamsMethod))]
        [TestCase(nameof(Class.HiddenParamsMethod))]
        public void TestParamsMethod(string name)
        {
            var model = typeof(Class).Model().Method(name, typeof(string), typeof(int[]));
            var result = model.Call(null, "string", new[] {1, 2, 3});

            Assert.AreEqual(12, result);

            Assert.Throws<DodgeInvocationException>(() => model.Call(null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, 1, 1));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, (object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, "string"));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, "string", 2));
            Assert.Throws<DodgeInvocationException>(() => model.Call(new Class(), "string", new[] {1, 2, 3}));
        }
    }
}