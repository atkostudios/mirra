using Atko.Dodge.Models;
using NullGuard;
using NUnit.Framework;

namespace Atko.Dodge.Test
{
    [TestFixture]
    public class FieldModelTests
    {
        [Test]
        [TestCase("PublicField")]
        [TestCase("PrivateField")]
        public void TestInstance(string name)
        {
            var instance = new TestClass();
            var model = typeof(TestClass).Model().Field(name);

            Assert.AreEqual(model.Get(instance), 0);

            model.Set(instance, 1);
            Assert.AreEqual(model.Get(instance), 1);

            model.Set(instance, 2);
            Assert.AreEqual(model.Get(instance), 2);
        }

        [Test]
        [TestCase("PublicField")]
        [TestCase("PrivateField")]
        public void TestInstanceNullException(string name)
        {
            var model = typeof(TestClass).Model().Field(name);

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
        [TestCase("PublicField", "string")]
        [TestCase("PublicField", null)]
        [TestCase("PrivateField", "string")]
        [TestCase("PrivateField", null)]
        public void TestInstanceArgumentException(string name, [AllowNull] object argument)
        {
            var instance = new TestClass();
            var model = typeof(TestClass).Model().Field(name);

            Assert.Throws<DodgeInvocationException>(() =>
            {
                model.Set(instance, argument);
            });
        }

        [Test]
        [TestCase("PublicStaticField")]
        [TestCase("PrivateStaticField")]
        public void TestStatic(string name)
        {
            var model = typeof(TestClass).Model().Field(name);

            Assert.AreEqual(model.Get(null), 0);

            model.Set(null, 1);
            Assert.AreEqual(model.Get(null), 1);

            model.Set(null, 2);
            Assert.AreEqual(model.Get(null), 2);
        }

        [Test]
        [TestCase("PublicStaticField")]
        [TestCase("PrivateStaticField")]
        public void TestInstanceNotNullException(string name)
        {
            var instance = new TestClass();
            var model = typeof(TestClass).Model().Field(name);

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
        [TestCase("PublicStaticField", "string")]
        [TestCase("PublicStaticField", null)]
        [TestCase("PrivateStaticField", "string")]
        [TestCase("PrivateStaticField", null)]
        public void TestStaticArgumentException(string name, [AllowNull] object argument)
        {
            var model = typeof(TestClass).Model().Field(name);

            Assert.Throws<DodgeInvocationException>(() =>
            {
                model.Set(null, argument);
            });
        }
    }
}