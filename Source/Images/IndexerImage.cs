using System;
using System.Reflection;
using Atko.Mirra.Generation;
using Atko.Mirra.Utility;

namespace Atko.Mirra.Images
{
    public class IndexerImage : MemberImage
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

        public bool CanGet => Property.CanRead;
        public bool CanSet => Property.CanWrite;

        public PropertyInfo Property => (PropertyInfo)Member;

        InvokerDispatch<IndexerGetInvoker> GetInvokers { get; }
        InvokerDispatch<IndexerSetInvoker> SetInvokers { get; }

        public IndexerImage(PropertyInfo member) : base(member)
        {
            if (!CanCreateFrom(member))
            {
                throw new ArgumentException(nameof(member));
            }

            var parameters = Property.GetIndexParameters();
            GetInvokers = new InvokerDispatch<IndexerGetInvoker>(parameters,
                (argumentCount) => Generate.InstanceIndexGetter(Property, argumentCount));

            SetInvokers = new InvokerDispatch<IndexerSetInvoker>(parameters,
                (argumentCount) => Generate.InstanceIndexSetter(Property, argumentCount));
        }

        public object Get(object instance, object index)
        {
            return Get(instance, new[] { index });
        }

        public object Get(object instance, object[] index)
        {
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
            if (!CanSet)
            {
                throw new MirraInvocationCannotSetException();
            }

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