using Atko.Dodge.Images;
using NUnit.Framework;

namespace Atko.Dodge.Tests.Images
{
    [TestFixture]
    class PropertyModelInstanceTests
    {
        public class Class
        {
            public int PublicAutoProperty { get; set; }
            internal int HiddenAutoProperty { get; set; }

            public int PublicGetOnlyAutoProperty { get; }
            internal int PrivateGetOnlyAutoProperty { get; }

            public int PublicComputedProperty => 0;
            internal int PrivateComputedProperty => 0;
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty))]
        [TestCase(nameof(Class.HiddenAutoProperty))]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PublicComputedProperty))]
        [TestCase(nameof(Class.PrivateComputedProperty))]
        public void TestGetSet(string name)
        {
            var instance = new Class();
            var model = typeof(Class).Image().Property(name);

            Assert.AreEqual(0, model.Get(instance));

            if (model.CanSet)
            {
                Assert.False(name.Contains("Computed"));
                model.Set(instance, 1);
                Assert.AreEqual(1, model.Get(instance));

                model.Set(instance, 2);
                Assert.AreEqual(2, model.Get(instance));
            }
            else
            {
                Assert.True(name.Contains("Computed"));
                Assert.Throws<DodgeInvocationException>(() => model.Set(instance, 1));
            }
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty))]
        [TestCase(nameof(Class.HiddenAutoProperty))]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty))]
        public void TestNullInstanceException(string name)
        {
            var model = typeof(Class).Image().Property(name);

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
        public void TestArgumentException(string name, object argument)
        {
            var instance = new Class();
            var model = typeof(Class).Image().Property(name);

            Assert.Throws<DodgeInvocationException>(() => model.Set(instance, argument));
        }
    }
}