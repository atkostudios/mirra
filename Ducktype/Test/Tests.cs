using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using Fasterflect;
using FastMember;
using NUnit.Framework;
using Utility;

namespace Ducktype
{
    [TestFixture]
    public class Tests
    {
        static class Static
        {
            static int Number { get; }

            static int Method(int number)
            {
                return number + 1;
            }
        }

        class Dan
        {
            int Height { get; }

            string Greet(string someone)
            {
                return $"Hi {someone}! My name is Dan.";
            }
        }

        class Steve<T> : Dan
        {
            public int Age;
            public int Height { get; }
            public T Thing { get; }

            public string Greet(string someone)
            {
                return $"Hi {someone}! My name is Steve.";
            }
        }

        class Thing
        {
            public int Field;
            public static int StaticField = 2;

            public int Something(int value)
            {
                return Field += value;
            }
        }

        void SetField(Thing thing, object value)
        {
            thing.Field = (int) value;
        }

        [Test]
        public void TestDelegate()
        {

            var instance = new Thing();
            var target = typeof(Thing).GetField("Field", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var method = typeof(Thing).GetMethod("Something", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var setter = DelegateCreator.CreateInstanceSetterDelegate(target);
            var getter = DelegateCreator.CreateInstanceGetterDelegate(target);
            var caller = DelegateCreator.CreateInstanceMethodDelegate(method, 1);
            var staticField = typeof(Thing).GetField("StaticField", BindingFlags.Public | BindingFlags.Static);
            var staticGetter = DelegateCreator.CreateStaticGetterDelegate(staticField);

            for (var i = 0; i < 1000000; i++)
            {
                setter.Invoke(instance, ((int) getter.Invoke(instance)) + 1);
            }

            for (var i = 0; i < 1000000; i++)
            {
                caller.Invoke(instance, new object[]{2});
            }
            Console.WriteLine(instance.Field);

            Console.WriteLine(staticGetter.Invoke());

//            for (var i = 0; i < 1000000; i++)
//            {
//                target.SetValue(instance, ((int) target.GetValue(instance)) + 1);
//            }
//
//            Console.WriteLine(getter.Invoke(instance));

//            for (var i = 0; i < 1000000; i++)
//            {
//                setter.DynamicInvoke(instance, 2);
//            }


        }

        [Test]
        public void Test()
        {
            var steve = new Steve<string>();
//            new Duck(steve).Set("Age", 20);
//            new Duck(steve).Set("Height", 30);
//            new Duck(steve).Set("Thing", "Human");
//
//            Console.WriteLine(steve.Age);
//            Console.WriteLine(steve.Height);
//            Console.WriteLine(steve.Thing);
//            Console.WriteLine();
//
//            Console.WriteLine(new Duck(steve).Get("Age"));
//            Console.WriteLine(new Duck(steve).Get("Height"));
//            Console.WriteLine(new Duck(steve).Get("Thing"));
//            Console.WriteLine();
//
//            Console.WriteLine(new Duck(steve, typeof(Dan)).Get("Height"));
//            Console.WriteLine(new Duck(steve).Get("Height"));
//            Console.WriteLine();
//
//            Console.WriteLine(new Duck(steve, typeof(Dan)).Call("Greet", "Karen"));
//            Console.WriteLine(new Duck(steve).Call("Greet", "Karen"));
//            Console.WriteLine();

            var sum = 0;
            var duck = new Duck(steve);
            for (var i = 0; i < 10000; i++)
            {
                var height = (int) duck.Get("Height");
                sum += height;
                duck.Set("Height", height + 1);
                duck.Call("Greet", "Karen");
            }

            return;

            Console.WriteLine(sum);
            Console.WriteLine();

            var list = new List<int> {1, 2};
            var accessor = new Duck(list);
            Console.WriteLine(new Duck(typeof(List<int>)).GetImplementation(typeof(List<>)));

            Console.WriteLine(accessor.Get("Count"));
            Console.WriteLine(accessor.Get("Count"));
            Console.WriteLine();

            accessor.Call("Add", 1);

            Console.WriteLine(accessor.Get("Count"));
        }
    }
}