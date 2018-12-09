using System.Linq;
using Atko.Dodge.Models;
using NUnit.Framework;

namespace Atko.Dodge.Tests.Models
{
    [TestFixture]
    class MethodModelInstanceTests
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
            var model = typeof(Class).Model().Method(name);
            var result = model.Call(instance);

            Assert.AreEqual(1, instance.InvokeCount);
            Assert.IsNull(result);

            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, (object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, 1));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null));
        }

        [Test]
        [TestCase(nameof(Class.PublicArgumentlessMethod))]
        [TestCase(nameof(Class.HiddenArgumentlessMethod))]
        public void TestArgumentlessMethod(string name)
        {
            var instance = new Class();
            var model = typeof(Class).Model().Method(name);
            var result = model.Call(instance);

            Assert.AreEqual(1, instance.InvokeCount);
            Assert.AreEqual(1, result);

            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, (object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, 1));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null));
        }

        [Test]
        [TestCase(nameof(Class.PublicMethod))]
        [TestCase(nameof(Class.HiddenMethod))]
        public void TestMethod(string name)
        {
            var instance = new Class();
            var model = typeof(Class).Model().Method(name, typeof(int));
            var result = model.Call(instance, 1);

            Assert.AreEqual(2, result);

            Assert.Throws<DodgeInvocationException>(() => model.Call(instance));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, 1, 1));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, (object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, "string"));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, "string", 2));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, 1));
        }

        [Test]
        [TestCase(nameof(Class.PublicParamsMethod))]
        [TestCase(nameof(Class.HiddenParamsMethod))]
        public void TestParamsMethod(string name)
        {
            var instance = new Class();
            var model = typeof(Class).Model().Method(name, typeof(string), typeof(int[]));
            var result = model.Call(instance, "string", new[] {1, 2, 3});

            Assert.AreEqual(12, result);

            Assert.Throws<DodgeInvocationException>(() => model.Call(instance));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, 1, 1));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, (object) null));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, "string"));
            Assert.Throws<DodgeInvocationException>(() => model.Call(instance, "string", 2));
            Assert.Throws<DodgeInvocationException>(() => model.Call(null, "string", new[] {1, 2, 3}));
        }
    }
}