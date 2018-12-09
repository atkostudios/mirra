using System;
using System.Linq;
using System.Reflection;
using Atko.Dodge.Dynamic;
using NullGuard;

namespace Atko.Dodge.Models
{
    public abstract class CallableModel : MemberModel
    {
        public static bool CanCreateFrom(MethodBase method)
        {
            return !method.GetParameters().Any((current) => current.IsIn || current.IsOut || current.IsRetval);
        }

        public override bool IsPublic => Base.IsPublic;

        MethodBase Base => (MethodBase) Member;

        InvokerDispatch<InstanceMethodInvoker> InstanceInvokers { get; }
        InvokerDispatch<StaticMethodInvoker> StaticInvokers { get; }

        protected CallableModel(Type owner, MethodBase member) : base(owner, member)
        {
            if (!CanCreateFrom(member))
            {
                throw new ArgumentException(nameof(member));
            }

            var parameters = Base.GetParameters();

            if (RequiresInstance)
            {
                InstanceInvokers =
                    new InvokerDispatch<InstanceMethodInvoker>(parameters, GetInstanceMethodInvoker);
            }
            else
            {
                StaticInvokers =
                    new InvokerDispatch<StaticMethodInvoker>(parameters, GetStaticMethodInvoker);
            }
        }

        protected object CallInternal([AllowNull] object instance, object[] arguments)
        {
            AssertInstanceMatches(instance);

            try
            {
                if (RequiresInstance)
                {
                    var invoker = InstanceInvokers.Get(arguments.Length);
                    if (invoker == null)
                    {
                        throw new DodgeInvocationException("Invalid number of arguments for invocation.");
                    }

                    return invoker.Invoke(instance, arguments);
                }
                else
                {
                    var invoker = StaticInvokers.Get(arguments.Length);
                    if (invoker == null)
                    {
                        throw new DodgeInvocationException("Invalid number of arguments for invocation.");
                    }

                    return invoker.Invoke(arguments);
                }
            }
            catch (Exception exception)
            {
                throw new DodgeInvocationException(null, exception);
            }
        }

        protected abstract InstanceMethodInvoker GetInstanceMethodInvoker(int argumentCount);
        protected abstract StaticMethodInvoker GetStaticMethodInvoker(int argumentCount);
    }
}