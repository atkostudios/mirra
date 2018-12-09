using Atko.Dodge.Models;
using NUnit.Framework;

namespace Atko.Dodge.Tests.Models
{
    [TestFixture]
    class PropertyModelTests
    {
        public class Class
        {
            public static int PublicStaticAutoProperty { get; set; }
            internal static int HiddenStaticAutoProperty { get; set; }

            public static int PublicStaticGetOnlyAutoProperty { get; }
            internal static int HiddenStaticGetOnlyAutoProperty { get; }

            public static int PublicStaticGetOnlyProperty => 0;
            internal static int PrivateStaticGetOnlyProperty => 0;

            public int PublicAutoProperty { get; set; }
            internal int HiddenAutoProperty { get; set; }

            public int PublicGetOnlyAutoProperty { get; }
            internal int PrivateGetOnlyAutoProperty { get; }

            public int PublicGetOnlyProperty => 0;
            internal int PrivateGetOnlyProperty => 0;
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty))]
        [TestCase(nameof(Class.HiddenAutoProperty))]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PublicGetOnlyProperty))]
        [TestCase(nameof(Class.PrivateGetOnlyProperty))]
        public void TestInstance(string name)
        {
            var instance = new Class();
            var model = typeof(Class).Model().Property(name);

            Assert.AreEqual(0, model.Get(instance));

            if (model.CanSet)
            {
                Assert.False(name.Contains("GetOnly"));
                model.Set(instance, 1);
                Assert.AreEqual(1, model.Get(instance));

                model.Set(instance, 2);
                Assert.AreEqual(2, model.Get(instance));
            }
            else
            {
                Assert.True(name.Contains("GetOnly"));
                Assert.Throws<DodgeInvocationException>(() => model.Set(instance, 1));
            }
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty))]
        [TestCase(nameof(Class.HiddenAutoProperty))]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty))]
        public void TestInstanceNullException(string name)
        {
            var model = typeof(Class).Model().Property(name);

            Assert.Throws<DodgeInvocationException>(() => model.Get(null));
            Assert.Throws<DodgeInvocationException>(() => model.Set(null, 1));
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty), null)]
        [TestCase(nameof(Class.PublicAutoProperty), "string")]
        [TestCase(nameof(Class.HiddenAutoProperty), null)]
        [TestCase(nameof(Class.HiddenAutoProperty), "string")]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty), null)]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty), "string")]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty), null)]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty), "string")]
        public void TestInstanceArgumentException(string name, object argument)
        {
            var instance = new Class();
            var model = typeof(Class).Model().Property(name);

            Assert.Throws<DodgeInvocationException>(() => model.Set(instance, argument));
        }

        [Test]
        [TestCase(nameof(Class.PublicStaticAutoProperty))]
        [TestCase(nameof(Class.HiddenStaticAutoProperty))]
        [TestCase(nameof(Class.PublicStaticGetOnlyAutoProperty))]
        [TestCase(nameof(Class.HiddenStaticGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PublicStaticGetOnlyProperty))]
        [TestCase(nameof(Class.PrivateStaticGetOnlyProperty))]
        public void TestStatic(string name)
        {
            var model = typeof(Class).Model().Property(name);

            Assert.AreEqual(0, model.Get(null));

            if (model.CanSet)
            {
                Assert.False(name.Contains("GetOnly"));
                model.Set(null, 1);
                Assert.AreEqual(1, model.Get(null));

                model.Set(null, 2);
                Assert.AreEqual(2, model.Get(null));
            }
            else
            {
                Assert.True(name.Contains("GetOnly"));
                Assert.Throws<DodgeInvocationException>(() => model.Set(null, 1));
            }
        }

        [Test]
        [TestCase(nameof(Class.PublicStaticAutoProperty))]
        [TestCase(nameof(Class.HiddenStaticAutoProperty))]
        [TestCase(nameof(Class.PublicStaticGetOnlyAutoProperty))]
        [TestCase(nameof(Class.HiddenStaticGetOnlyAutoProperty))]
        public void TestStaticNotNullException(string name)
        {
            var instance = new Class();
            var model = typeof(Class).Model().Property(name);

            Assert.Throws<DodgeInvocationException>(() => model.Get(instance));
            Assert.Throws<DodgeInvocationException>(() => model.Set(instance, 1));
        }

        [Test]
        [TestCase(nameof(Class.PublicStaticAutoProperty), null)]
        [TestCase(nameof(Class.PublicStaticAutoProperty), "string")]
        [TestCase(nameof(Class.HiddenStaticAutoProperty), null)]
        [TestCase(nameof(Class.HiddenStaticAutoProperty), "string")]
        [TestCase(nameof(Class.PublicStaticGetOnlyAutoProperty), null)]
        [TestCase(nameof(Class.PublicStaticGetOnlyAutoProperty), "string")]
        [TestCase(nameof(Class.HiddenStaticGetOnlyAutoProperty), null)]
        [TestCase(nameof(Class.HiddenStaticGetOnlyAutoProperty), "string")]
        public void TestStaticArgumentException(string name, object argument)
        {
            var model = typeof(Class).Model().Property(name);

            Assert.Throws<DodgeInvocationException>(() => model.Set(null, argument));
        }
    }
}