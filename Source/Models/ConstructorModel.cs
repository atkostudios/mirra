using System;
using System.Reflection;
using Atko.Dodge.Dynamic;
using NullGuard;

namespace Atko.Dodge.Models
{
    public class ConstructorModel : CallableModel
    {
        public ConstructorInfo Constructor => (ConstructorInfo) Member;

        internal ConstructorModel(Type owner, ConstructorInfo constructor) : base(owner, constructor) { }

        public object Call(params object[] arguments)
        {
            return CallInternal(null, arguments);
        }

        [return: AllowNull]
        protected override InstanceMethodInvoker GetInstanceMethodInvoker(int argumentCount)
        {
            return null;
        }

        protected override StaticMethodInvoker GetStaticMethodInvoker(int argumentCount)
        {
            return CodeGenerator.Constructor(Constructor, argumentCount);
        }
    }
}