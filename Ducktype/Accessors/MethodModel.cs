using System;
using System.Linq;
using System.Reflection;
using NullGuard;
using Utility;

namespace Ducktype.Models
{
    public class MethodModel : MemberModel
    {
        [return: AllowNull]
        internal static MethodModel Create(Type owner, MethodInfo method)
        {
            if (method.GetParameters().Any((current) => current.IsIn || current.IsOut || current.IsRetval))
            {
                return null;
            }

            return new MethodModel(owner, method);
        }

        public override bool IsPublic => Method.IsPublic;
        public MethodInfo Method => (MethodInfo) Member;

        Cache<int, InstanceMethodInvoker> InstanceInvokers { get; } = new Cache<int, InstanceMethodInvoker>();
        Cache<int, StaticMethodInvoker> StaticInvokers { get; } = new Cache<int, StaticMethodInvoker>();

        MethodModel(Type owner, MethodInfo method) : base(owner, method) { }

        [return: AllowNull]
        public object Call([AllowNull] object instance, params object[] arguments)
        {
            AssertInstanceMatches(instance);
            try
            {
                if (IsStatic)
                {
                    var function = StaticInvokers.GetOrAdd(arguments.Length, (count) =>
                    {
                        return CodeGenerator.StaticMethod(Method, count);
                    });

                    return function.Invoke(arguments);
                }
                else
                {
                    var function = InstanceInvokers.GetOrAdd(arguments.Length, (count) =>
                    {
                        return CodeGenerator.InstanceMethod(Method, count);
                    });

                    return function.Invoke(instance, arguments);
                }
            }
            catch (Exception exception)
            {
                throw new DucktypeInvocationException(null, exception);
            }
        }
    }
}