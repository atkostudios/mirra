using System;
using System.Reflection;
using Atko.Dodge.Generation;
using Atko.Dodge.Utility;
using NullGuard;

namespace Atko.Dodge.Models
{
    public class MethodModel : CallableModel
    {
        public MethodInfo Method => (MethodInfo) Member;

        internal MethodModel(Type owner, MethodInfo method) :
            base(owner, method,
                (argumentCount) => Generate.InstanceMethod(method, argumentCount),
                (argumentCount) => Generate.StaticMethod(method, argumentCount)) { }

        [return: AllowNull]
        public object Call([AllowNull] object instance, params object[] arguments)
        {
            return CallInternal(instance, arguments);
        }
    }
}