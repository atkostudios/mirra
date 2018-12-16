using Atko.Mirra.Images;
using NUnit.Framework;

namespace Atko.Mirra.Tests.Images
{
    [TestFixture]
    class PropertyImageInstanceTests
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
            var image = typeof(Class).Image().Property(name);

            Assert.AreEqual(0, image.Get(instance));

            if (image.CanSet)
            {
                Assert.False(name.Contains("Computed"));
                image.Set(instance, 1);
                Assert.AreEqual(1, image.Get(instance));

                image.Set(instance, 2);
                Assert.AreEqual(2, image.Get(instance));
            }
            else
            {
                Assert.True(name.Contains("Computed"));
                Assert.Throws<MirraInvocationCannotSetException>(() => image.Set(instance, 1));
            }
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty))]
        [TestCase(nameof(Class.HiddenAutoProperty))]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty))]
        public void TestNullInstanceException(string name)
        {
            var image = typeof(Class).Image().Property(name);

            Assert.Throws<MirraInvocationArgumentException>(() => image.Get(null));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Set(null, 1));
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty), null)]
        [TestCase(nameof(Class.PublicAutoProperty), "")]
        [TestCase(nameof(Class.HiddenAutoProperty), null)]
        [TestCase(nameof(Class.HiddenAutoProperty), "")]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty), null)]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty), "")]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty), null)]
        [TestCase(nameof(Class.PrivateGetOnlyAutoProperty), "")]
        public void TestArgumentException(string name, object argument)
        {
            var instance = new Class();
            var image = typeof(Class).Image().Property(name);

            Assert.Throws<MirraInvocationArgumentException>(() => image.Set(instance, argument));
        }
    }
}