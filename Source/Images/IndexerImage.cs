using System;
using System.Linq;
using System.Reflection;
using Atko.Mirra.Generation;

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

            return !parameters.Any((current) => current.IsIn || current.IsOut || current.IsRetval);
        }

        public override bool IsPublic => Property.GetMethod.IsPublic || (Property.SetMethod?.IsPublic ?? false);
        public override bool IsStatic => Property.GetMethod.IsStatic;

        public bool CanGet => Property.CanRead;
        public bool CanSet => Property.CanWrite;

        public PropertyInfo Property => (PropertyInfo)Member;

        InvokerDispatch<IndexerGetInvoker> GetInvokers { get; }
        InvokerDispatch<IndexerSetInvoker> SetInvokers { get; }

        public IndexerImage(Type owner, PropertyInfo member) : base(owner, member)
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
                throw new MirraInvocationException("Invalid number of index arguments.");
            }

            try
            {
                return invoker.Invoke(instance, index);
            }
            catch (Exception exception)
            {
                throw new MirraInvocationException(null, exception);
            }
        }

        public void Set(object instance, object index, object value)
        {
            Set(instance, new[] { index }, value);
        }

        public void Set(object instance, object[] index, object value)
        {
            var invoker = SetInvokers.Get(index.Length);
            if (invoker == null)
            {
                throw new MirraInvocationException("Invalid number of index arguments.");
            }

            try
            {
                invoker.Invoke(instance, index, value);
            }
            catch (Exception exception)
            {
                throw new MirraInvocationException(null, exception);
            }
        }
    }
}