using Atko.Dodge.Models;
using NullGuard;
using NUnit.Framework;

namespace Atko.Dodge.Test
{
    [TestFixture]
    public class PropertyModelTests
    {
        [Test]
        [TestCase("PublicAutoProperty")]
        [TestCase("PrivateAutoProperty")]
        [TestCase("PublicGetOnlyAutoProperty")]
        [TestCase("PrivateGetOnlyAutoProperty")]
        [TestCase("PublicGetOnlyProperty")]
        [TestCase("PrivateGetOnlyProperty")]
        public void TestInstance(string name)
        {
            var instance = new TestClass();
            var model = typeof(TestClass).Model().Property(name);

            Assert.AreEqual(model.Get(instance), 0);

            if (model.CanSet)
            {
                Assert.False(name.Contains("GetOnly"));
                model.Set(instance, 1);
                Assert.AreEqual(model.Get(instance), 1);

                model.Set(instance, 2);
                Assert.AreEqual(model.Get(instance), 2);
            }
            else
            {
                Assert.True(name.Contains("GetOnly"));
                Assert.Throws<DodgeInvocationException>(() => model.Set(instance, 1));
            }
        }

        [Test]
        [TestCase("PublicAutoProperty")]
        [TestCase("PrivateAutoProperty")]
        [TestCase("PublicGetOnlyAutoProperty")]
        [TestCase("PrivateGetOnlyAutoProperty")]
        public void TestInstanceNullException(string name)
        {
            var model = typeof(TestClass).Model().Property(name);

            Assert.Throws<DodgeInvocationException>(() =>
            {
                model.Get(null);
            });

            Assert.Throws<DodgeInvocationException>(() =>
            {
                model.Set(null, 1);
            });
        }

        [Test]
        [TestCase("PublicAutoProperty", null)]
        [TestCase("PublicAutoProperty", "string")]
        [TestCase("PrivateAutoProperty", null)]
        [TestCase("PrivateAutoProperty", "string")]
        [TestCase("PublicGetOnlyAutoProperty", null)]
        [TestCase("PublicGetOnlyAutoProperty", "string")]
        [TestCase("PrivateGetOnlyAutoProperty", null)]
        [TestCase("PrivateGetOnlyAutoProperty", "string")]
        public void TestInstanceArgumentException(string name, [AllowNull] object argument)
        {
            var instance = new TestClass();
            var model = typeof(TestClass).Model().Property(name);

            Assert.Throws<DodgeInvocationException>(() =>
            {
                model.Set(instance, argument);
            });
        }

        [Test]
        [TestCase("PublicStaticAutoProperty")]
        [TestCase("PrivateStaticAutoProperty")]
        [TestCase("PublicStaticGetOnlyAutoProperty")]
        [TestCase("PrivateStaticGetOnlyAutoProperty")]
        [TestCase("PublicStaticGetOnlyProperty")]
        [TestCase("PrivateStaticGetOnlyProperty")]
        public void TestStatic(string name)
        {
            var model = typeof(TestClass).Model().Property(name);

            Assert.AreEqual(model.Get(null), 0);

            if (model.CanSet)
            {
                Assert.False(name.Contains("GetOnly"));
                model.Set(null, 1);
                Assert.AreEqual(model.Get(null), 1);

                model.Set(null, 2);
                Assert.AreEqual(model.Get(null), 2);
            }
            else
            {
                Assert.True(name.Contains("GetOnly"));
                Assert.Throws<DodgeInvocationException>(() => model.Set(null, 1));
            }
        }

        [Test]
        [TestCase("PublicStaticAutoProperty")]
        [TestCase("PrivateStaticAutoProperty")]
        [TestCase("PublicStaticGetOnlyAutoProperty")]
        [TestCase("PrivateStaticGetOnlyAutoProperty")]
        public void TestStaticNotNullException(string name)
        {
            var instance = new TestClass();
            var model = typeof(TestClass).Model().Property(name);

            Assert.Throws<DodgeInvocationException>(() =>
            {
                model.Get(instance);
            });

            Assert.Throws<DodgeInvocationException>(() =>
            {
                model.Set(instance, 1);
            });
        }

        [Test]
        [TestCase("PublicStaticAutoProperty", null)]
        [TestCase("PublicStaticAutoProperty", "string")]
        [TestCase("PrivateStaticAutoProperty", null)]
        [TestCase("PrivateStaticAutoProperty", "string")]
        [TestCase("PublicStaticGetOnlyAutoProperty", null)]
        [TestCase("PublicStaticGetOnlyAutoProperty", "string")]
        [TestCase("PrivateStaticGetOnlyAutoProperty", null)]
        [TestCase("PrivateStaticGetOnlyAutoProperty", "string")]
        public void TestStaticArgumentException(string name, [AllowNull] object argument)
        {
            var model = typeof(TestClass).Model().Property(name);

            Assert.Throws<DodgeInvocationException>(() =>
            {
                model.Set(null, argument);
            });
        }
    }
}