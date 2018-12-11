using System;
using System.Reflection;
using Atko.Dodge.Generation;
using NullGuard;

namespace Atko.Dodge.Models
{
    public class ConstructorModel : CallableModel
    {
        public ConstructorInfo Constructor => (ConstructorInfo) Member;

        internal ConstructorModel(Type owner, ConstructorInfo constructor) :
            base(owner, constructor, null,
                (argumentCount) => Generate.Constructor(constructor, argumentCount)) { }

        public object Call(params object[] arguments)
        {
            return CallInternal(null, arguments);
        }
    }
}