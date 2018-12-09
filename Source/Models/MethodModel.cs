using System;
using System.Reflection;
using Atko.Dodge.Dynamic;
using NullGuard;

namespace Atko.Dodge.Models
{
    public class MethodModel : CallableModel
    {
        public MethodInfo Method => (MethodInfo) Member;

        InvokerDispatch<InstanceMethodInvoker> InstanceInvokers { get; }
        InvokerDispatch<StaticMethodInvoker> StaticInvokers { get; }
        internal MethodModel(Type owner, MethodInfo method) : base(owner, method) { }

        [return: AllowNull]
        public object Call([AllowNull] object instance, params object[] arguments)
        {
            return CallInternal(instance, arguments);
        }

        protected override InstanceMethodInvoker GetInstanceMethodInvoker(int argumentCount)
        {
            return CodeGenerator.InstanceMethod(Method, argumentCount);
        }

        protected override StaticMethodInvoker GetStaticMethodInvoker(int argumentCount)
        {
            return CodeGenerator.StaticMethod(Method, argumentCount);
        }
    }
}