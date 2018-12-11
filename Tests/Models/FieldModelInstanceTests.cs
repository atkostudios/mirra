using System;
using Atko.Dodge.Models;
using NUnit.Framework;

namespace Atko.Dodge.Tests.Models
{
    [TestFixture]
    public class FieldModelInstanceTests
    {
        #pragma warning disable 169
        #pragma warning disable 649
        public class Class
        {
            public int PublicField;
            public readonly int PublicReadOnlyField;
            internal int HiddenField;
            internal readonly int HiddenReadOnlyField;
        }
        #pragma warning restore 169
        #pragma warning restore 649

        [Test]
        [TestCase(nameof(Class.PublicField))]
        [TestCase(nameof(Class.PublicReadOnlyField))]
        [TestCase(nameof(Class.HiddenField))]
        [TestCase(nameof(Class.HiddenReadOnlyField))]
        public void TestGetSet(string name)
        {
            var instance = new Class();
            var model = typeof(Class).Model().Field(name);

            Assert.AreEqual(0, model.Get(instance));

            model.Set(instance, 1);
            Assert.AreEqual(1, model.Get(instance));

            model.Set(instance, 2);
            Assert.AreEqual(2, model.Get(instance));
            Console.WriteLine(model);
        }

        [Test]
        [TestCase(nameof(Class.PublicField))]
        [TestCase(nameof(Class.PublicReadOnlyField))]
        [TestCase(nameof(Class.HiddenField))]
        [TestCase(nameof(Class.HiddenReadOnlyField))]
        public void TestInstanceNullException(string name)
        {
            var model = typeof(Class).Model().Field(name);

            Assert.Throws<DodgeInvocationException>(() => model.Get(null));
            Assert.Throws<DodgeInvocationException>(() => model.Set(null, 1));
        }

        [Test]
        [TestCase(nameof(Class.PublicField), "string")]
        [TestCase(nameof(Class.PublicField), null)]
        [TestCase(nameof(Class.PublicReadOnlyField), "string")]
        [TestCase(nameof(Class.PublicReadOnlyField), null)]
        [TestCase(nameof(Class.HiddenField), "string")]
        [TestCase(nameof(Class.HiddenField), null)]
        [TestCase(nameof(Class.HiddenReadOnlyField), "string")]
        [TestCase(nameof(Class.HiddenReadOnlyField), null)]
        public void TestArgumentException(string name, object argument)
        {
            var instance = new Class();
            var model = typeof(Class).Model().Field(name);

            Assert.Throws<DodgeInvocationException>(() => model.Set(instance, argument));
        }
    }
}