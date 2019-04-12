using System.Linq;
using NUnit.Framework;

using Atko.Mirra;

namespace Atko.Mirra.Tests
{
    [TestFixture]
    class MethodImageStaticTests
    {
        class Class
        {
            public static void PublicArgumentlessMethodWithVoidReturn()
            { }

            internal static void HiddenArgumentlessMethodWithVoidReturn()
            { }

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
            var image = typeof(Class).Image().Method(name);
            var result = image.Call(null);

            Assert.IsNull(result);

            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null, (object)null));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null, 1));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(new Class()));
        }

        [Test]
        [TestCase(nameof(Class.PublicArgumentlessMethod))]
        [TestCase(nameof(Class.HiddenArgumentlessMethod))]
        public void TestArgumentlessMethod(string name)
        {
            var image = typeof(Class).Image().Method(name);
            var result = image.Call(null);

            Assert.AreEqual(1, result);

            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null, (object)null));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null, 1));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(new Class()));
        }

        [Test]
        [TestCase(nameof(Class.PublicMethod))]
        [TestCase(nameof(Class.HiddenMethod))]
        public void TestMethod(string name)
        {
            var image = typeof(Class).Image().Method(name, typeof(int));
            var result = image.Call(null, 1);

            Assert.AreEqual(2, result);

            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null, 1, 1));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(null, (object)null));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(null, ""));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null, "", 2));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(new Class(), 1));
        }

        [Test]
        [TestCase(nameof(Class.PublicParamsMethod))]
        [TestCase(nameof(Class.HiddenParamsMethod))]
        public void TestParamsMethod(string name)
        {
            var image = typeof(Class).Image().Method(name, typeof(string), typeof(int[]));
            var result = image.Call(null, "", new[] { 1, 2, 3 });

            Assert.AreEqual(6, result);

            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(null, 1, 1));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null, (object)null));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(null, ""));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(null, "", 2));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(new Class(), "", new[] { 1, 2, 3 }));
        }
    }
}