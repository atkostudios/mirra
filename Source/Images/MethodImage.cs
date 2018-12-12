using System;
using System.Reflection;
using Atko.Mirra.Generation;
using NullGuard;

namespace Atko.Mirra.Images
{
    public class MethodImage : CallableImage
    {
        public MethodInfo Method => (MethodInfo)Member;

        internal MethodImage(Type owner, MethodInfo method) : base(owner, method,
            (argumentCount) => Generate.InstanceMethod(method, argumentCount),
            (argumentCount) => Generate.StaticMethod(method, argumentCount))
        { }

        [return: AllowNull]
        public object Call([AllowNull] object instance, params object[] arguments)
        {
            return CallInternal(instance, arguments);
        }
    }
}