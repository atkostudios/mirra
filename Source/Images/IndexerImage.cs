using System;
using System.Diagnostics;
using System.Reflection;
using Atko.Mirra.Generation;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Images
{
    public class IndexerImage : GetSetImage
    {
        public static bool CanCreateFrom(PropertyInfo property)
        {
            var parameters = property.GetIndexParameters();
            if (parameters.Length == 0)
            {
                return false;
            }

            if (TypeUtility.HasSpecialParameters(parameters))
            {
                return false;
            }

            return true;
        }

        public override bool IsPublic => Property.GetMethod.IsPublic || (Property.SetMethod?.IsPublic ?? false);
        public override bool IsStatic => Property.GetMethod.IsStatic;

        public override bool CanSet => Property.SetMethod != null;

        public override Type Type => Property.PropertyType;

        public PropertyInfo Property => (PropertyInfo)Member;

        InvokerDispatch<IndexerGetInvoker> GetInvokers { get; }
        InvokerDispatch<IndexerSetInvoker> SetInvokers { get; }

        public IndexerImage(PropertyInfo member) : base(member)
        {
            Debug.Assert(CanCreateFrom(member));

            var parameters = Property.GetIndexParameters();
            GetInvokers = new InvokerDispatch<IndexerGetInvoker>(parameters,
                (argumentCount) => CodeGenerator.Instance.InstanceIndexGetter(Property, argumentCount));

            SetInvokers = new InvokerDispatch<IndexerSetInvoker>(parameters,
                (argumentCount) => CodeGenerator.Instance.InstanceIndexSetter(Property, argumentCount));
        }

        [return: AllowNull]
        public object Get([AllowNull] object instance, object index)
        {
            return Get(instance, new[] { index });
        }

        [return: AllowNull]
        public object Get([AllowNull] object instance, object[] index)
        {
            AssertInstanceMatches(instance);

            var invoker = GetInvokers.Get(index.Length);
            if (invoker == null)
            {
                throw new MirraInvocationArgumentCountException();
            }

            try
            {
                return invoker.Invoke(instance, index);
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

        public void Set(object instance, object index, object value)
        {
            Set(instance, new[] { index }, value);
        }

        public void Set(object instance, object[] index, object value)
        {
            AssertCanSet();
            AssertInstanceMatches(instance);

            var invoker = SetInvokers.Get(index.Length);
            if (invoker == null)
            {
                throw new MirraInvocationArgumentCountException();
            }

            try
            {
                invoker.Invoke(instance, index, value);
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