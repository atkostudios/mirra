using System;
using System.Linq;
using System.Reflection;
using Atko.Dodge.Generation;
using NullGuard;

namespace Atko.Dodge.Images
{
    public abstract class CallableImage : MemberImage
    {
        public static bool CanCreateFrom(MethodBase method)
        {
            return !method.GetParameters().Any((current) => current.IsIn || current.IsOut || current.IsRetval);
        }

        public override bool IsPublic => Base.IsPublic;
        public override bool IsStatic => Base.IsStatic;

        MethodBase Base => (MethodBase)Member;

        InvokerDispatch<InstanceMethodInvoker> InstanceInvokers { get; }
        InvokerDispatch<StaticMethodInvoker> StaticInvokers { get; }

        protected CallableImage(Type owner, MethodBase member,
            [AllowNull] Func<int, InstanceMethodInvoker> instanceInvokerFactory,
            [AllowNull] Func<int, StaticMethodInvoker> staticInvokerFactory) : base(owner, member)
        {
            if (!CanCreateFrom(member))
            {
                throw new ArgumentException(nameof(member));
            }

            var parameters = Base.GetParameters();

            if (RequiresInstance)
            {
                InstanceInvokers = new InvokerDispatch<InstanceMethodInvoker>(parameters, instanceInvokerFactory);
            }
            else
            {
                StaticInvokers = new InvokerDispatch<StaticMethodInvoker>(parameters, staticInvokerFactory);
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
    }
}