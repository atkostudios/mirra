using System;
using System.Reflection;
using Atko.Mirra.Generation;
using NullGuard;

namespace Atko.Mirra.Images
{
    public abstract class AccessorImage : GetSetImage
    {
        Lazy<InstanceGetInvoker> LazyInstanceGetInvoker { get; }
        Lazy<InstanceSetInvoker> LazyInstanceSetInvoker { get; }
        Lazy<StaticGetInvoker> LazyStaticGetInvoker { get; }
        Lazy<StaticSetInvoker> LazyStaticSetInvoker { get; }

        protected AccessorImage(PropertyInfo property) : this((MemberInfo)property)
        { }

        protected AccessorImage(FieldInfo field) : this((MemberInfo)field)
        { }

        AccessorImage(MemberInfo member) : base(member)
        {
            LazyStaticGetInvoker = new Lazy<StaticGetInvoker>(() => CodeGenerator.Instance.StaticGetter(Member));
            LazyStaticSetInvoker = new Lazy<StaticSetInvoker>(() => CodeGenerator.Instance.StaticSetter(Member));
            LazyInstanceGetInvoker = new Lazy<InstanceGetInvoker>(() => CodeGenerator.Instance.InstanceGetter(Member));
            LazyInstanceSetInvoker = new Lazy<InstanceSetInvoker>(() => CodeGenerator.Instance.InstanceSetter(Member));
        }

        [return: AllowNull]
        public object Get([AllowNull] object instance)
        {
            AssertInstanceMatches(instance);

            try
            {
                if (IsStatic)
                {
                    return LazyStaticGetInvoker.Value.Invoke();
                }

                return LazyInstanceGetInvoker.Value.Invoke(instance);
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

        public void Set([AllowNull] object instance, [AllowNull] object value)
        {
            AssertCanSet();
            AssertInstanceMatches(instance);

            try
            {
                if (IsStatic)
                {
                    LazyStaticSetInvoker.Value.Invoke(value);
                }
                else
                {
                    LazyInstanceSetInvoker.Value.Invoke(instance, value);
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