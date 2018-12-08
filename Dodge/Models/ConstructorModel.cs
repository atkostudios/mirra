using System;
using System.Linq;
using System.Reflection;
using Atko.Dodge.Dynamic;
using Atko.Dodge.Utility;

namespace Atko.Dodge.Models
{
    public class ConstructorModel : MemberModel
    {
        internal static ConstructorModel Create(Type owner, ConstructorInfo constructor)
        {
            if (constructor.GetParameters().Any((current) => current.IsIn || current.IsOut || current.IsRetval))
            {
                return null;
            }

            return new ConstructorModel(owner, constructor);
        }

        public override bool IsPublic => Constructor.IsPublic;
        public ConstructorInfo Constructor => (ConstructorInfo) Member;

        Cache<int, ConstructorInvoker> Invokers { get; } = new Cache<int, ConstructorInvoker>();

        ConstructorModel(Type owner, ConstructorInfo constructor) : base(owner, constructor) { }

        public object Call(params object[] arguments)
        {
            try
            {
                var function = Invokers.GetOrAdd(arguments.Length, (count) =>
                {
                    return CodeGenerator.Constructor(Constructor, count);
                });

                return function.Invoke(arguments);
            }
            catch (Exception exception)
            {
                throw new DodgeInvocationException(null, exception);
            }
        }
    }
}