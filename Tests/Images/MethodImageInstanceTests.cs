using System.Linq;
using Atko.Mirra.Images;
using NUnit.Framework;

namespace Atko.Mirra.Tests.Images
{
    [TestFixture]
    class MethodImageInstanceTests
    {
        class Class
        {
            public int InvokeCount { get; set; }

            public void PublicArgumentlessMethodWithVoidReturn()
            {
                InvokeCount++;
            }

            internal void HiddenArgumentlessMethodWithVoidReturn()
            {
                InvokeCount++;
            }

            public int PublicArgumentlessMethod()
            {
                return ++InvokeCount;
            }

            internal int HiddenArgumentlessMethod()
            {
                return ++InvokeCount;
            }

            public int PublicMethod(int value)
            {
                return value + 1;
            }

            internal int HiddenMethod(int value)
            {
                return value + 1;
            }

            public int PublicParamsMethod(string content, int[] value)
            {
                return content.Length + value.Sum();
            }

            internal int HiddenParamsMethod(string content, int[] value)
            {
                return content.Length + value.Sum();
            }
        }

        [Test]
        [TestCase(nameof(Class.PublicArgumentlessMethodWithVoidReturn))]
        [TestCase(nameof(Class.HiddenArgumentlessMethodWithVoidReturn))]
        public void TestArgumentlessMethodWithVoidReturn(string name)
        {
            var instance = new Class();
            var image = typeof(Class).Image().Method(name);
            var result = image.Call(instance);

            Assert.AreEqual(1, instance.InvokeCount);
            Assert.IsNull(result);

            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance, (object)null));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance, 1));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(null));
        }

        [Test]
        [TestCase(nameof(Class.PublicArgumentlessMethod))]
        [TestCase(nameof(Class.HiddenArgumentlessMethod))]
        public void TestArgumentlessMethod(string name)
        {
            var instance = new Class();
            var image = typeof(Class).Image().Method(name);
            var result = image.Call(instance);

            Assert.AreEqual(1, instance.InvokeCount);
            Assert.AreEqual(1, result);

            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance, (object)null));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance, 1));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(null));
        }

        [Test]
        [TestCase(nameof(Class.PublicMethod))]
        [TestCase(nameof(Class.HiddenMethod))]
        public void TestMethod(string name)
        {
            var instance = new Class();
            var image = typeof(Class).Image().Method(name, typeof(int));
            var result = image.Call(instance, 1);

            Assert.AreEqual(2, result);

            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance, 1, 1));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(instance, (object)null));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(instance, ""));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance, "", 2));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(null, 1));
        }

        [Test]
        [TestCase(nameof(Class.PublicParamsMethod))]
        [TestCase(nameof(Class.HiddenParamsMethod))]
        public void TestParamsMethod(string name)
        {
            var instance = new Class();
            var image = typeof(Class).Image().Method(name, typeof(string), typeof(int[]));
            var result = image.Call(instance, "", new[] { 1, 2, 3 });

            Assert.AreEqual(6, result);

            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(instance, 1, 1));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance, (object)null));
            Assert.Throws<MirraInvocationArgumentCountException>(() => image.Call(instance, ""));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(instance, "", 2));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Call(null, "", new[] { 1, 2, 3 }));
        }
    }
}