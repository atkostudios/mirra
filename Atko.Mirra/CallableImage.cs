using System;
using System.Diagnostics;
using System.Reflection;
using NullGuard;

using Atko.Mirra.Utilities;

namespace Atko.Mirra
{
    public abstract class CallableImage : TypeMemberImage
    {
        internal static bool CanCreateFrom(MethodBase method)
        {
            var parameters = method.GetParameters();
            if (TypeUtility.HasSpecialParameters(parameters))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool IsPublic => Base.IsPublic;

        /// <inheritdoc/>
        public override bool IsStatic => Base.IsStatic;

        MethodBase Base => (MethodBase)Member;

        InvokerDispatch<InstanceMethodInvoker> InstanceInvokers { get; }
        InvokerDispatch<StaticMethodInvoker> StaticInvokers { get; }

        internal CallableImage(MethodBase member,
            [AllowNull] Func<int, InstanceMethodInvoker> instanceInvokerFactory,
            [AllowNull] Func<int, StaticMethodInvoker> staticInvokerFactory) : base(member)
        {
            Debug.Assert(CanCreateFrom(member));

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
                        throw new MirraInvocationArgumentCountException();
                    }

                    return invoker.Invoke(instance, arguments);
                }
                else
                {
                    var invoker = StaticInvokers.Get(arguments.Length);
                    if (invoker == null)
                    {
                        throw new MirraInvocationArgumentCountException();
                    }

                    return invoker.Invoke(arguments);
                }
            }
            catch (MirraException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CheckException(exception);
                throw;
            }
        }
    }
}