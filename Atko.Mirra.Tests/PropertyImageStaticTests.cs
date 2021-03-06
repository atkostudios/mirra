using System.Reflection;
using NUnit.Framework;

using Atko.Mirra;
using Atko.Mirra.Tests.Utilities;

namespace Atko.Mirra.Tests
{
    [TestFixture]
    [SingleThreaded]
    class PropertyImageStaticTests
    {
        public class Class
        {
            public static TestValue PublicAutoProperty { get; set; } = new TestValue(0);
            internal static TestValue HiddenAutoProperty { get; set; } = new TestValue(0);

            public static TestValue PublicGetOnlyAutoProperty { get; } = new TestValue(0);
            internal static TestValue HiddenGetOnlyAutoProperty { get; } = new TestValue(0);

            public static TestValue PublicComputedProperty => new TestValue(0);
            internal static TestValue HiddenComputedProperty => new TestValue(0);
        }

        [SetUp]
        public void SetUp()
        {
            const BindingFlags bindings = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            FieldInfo GetBackingField(PropertyInfo property)
            {
                return property.DeclaringType?.GetField($"<{property.Name}>k__BackingField", bindings);
            }

            typeof(Class).GetProperty(nameof(Class.PublicAutoProperty), bindings)?.SetValue(null, new TestValue(0));
            typeof(Class).GetProperty(nameof(Class.HiddenAutoProperty), bindings)?.SetValue(null, new TestValue(0));
            GetBackingField(typeof(Class).GetProperty(nameof(Class.PublicGetOnlyAutoProperty), bindings))
                ?.SetValue(null, new TestValue(0));

            GetBackingField(typeof(Class).GetProperty(nameof(Class.HiddenGetOnlyAutoProperty), bindings))
                ?.SetValue(null, new TestValue(0));
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty))]
        [TestCase(nameof(Class.HiddenAutoProperty))]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty))]
        [TestCase(nameof(Class.HiddenGetOnlyAutoProperty))]
        [TestCase(nameof(Class.PublicComputedProperty))]
        [TestCase(nameof(Class.HiddenComputedProperty))]
        public void TestGetSet(string name)
        {
            var image = typeof(Class).Image().Property(name);

            Assert.AreEqual(new TestValue(0), image.Get(null));

            if (image.CanSet)
            {
                Assert.False(name.Contains("Computed"));
                image.Set(null, new TestValue(1));
                Assert.AreEqual(new TestValue(1), image.Get(null));

                image.Set(null, new TestValue(2));
                Assert.AreEqual(new TestValue(2), image.Get(null));
            }
            else
            {
                Assert.True(name.Contains("Computed"));
                Assert.Throws<MirraInvocationCannotSetException>(() => image.Set(null, new TestValue(1)));
            }
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty))]
        [TestCase(nameof(Class.HiddenAutoProperty))]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty))]
        [TestCase(nameof(Class.HiddenGetOnlyAutoProperty))]
        public void TestInstanceNotNullException(string name)
        {
            var instance = new Class();
            var image = typeof(Class).Image().Property(name);

            Assert.Throws<MirraInvocationArgumentException>(() => image.Get(instance));
            Assert.Throws<MirraInvocationArgumentException>(() => image.Set(instance, new TestValue(1)));
        }

        [Test]
        [TestCase(nameof(Class.PublicAutoProperty), 1)]
        [TestCase(nameof(Class.PublicAutoProperty), "")]
        [TestCase(nameof(Class.HiddenAutoProperty), 1)]
        [TestCase(nameof(Class.HiddenAutoProperty), "")]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty), 1)]
        [TestCase(nameof(Class.PublicGetOnlyAutoProperty), "")]
        [TestCase(nameof(Class.HiddenGetOnlyAutoProperty), 1)]
        [TestCase(nameof(Class.HiddenGetOnlyAutoProperty), "")]
        public void TestArgumentException(string name, object argument)
        {
            var image = typeof(Class).Image().Property(name);

            Assert.Throws<MirraInvocationArgumentException>(() => image.Set(null, argument));
        }
    }
}