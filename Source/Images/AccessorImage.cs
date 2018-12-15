using System;
using System.Reflection;
using Atko.Mirra.Generation;
using NullGuard;

namespace Atko.Mirra.Images
{
    public abstract class AccessorImage : MemberImage
    {
        public abstract bool CanGet { get; }
        public abstract bool CanSet { get; }

        InstanceGetInvoker InstanceGetInvoker => LazyInstanceGetInvoker.Value;
        InstanceSetInvoker InstanceSetInvoker => LazyInstanceSetInvoker.Value;
        StaticGetInvoker StaticGetInvoker => LazyStaticGetInvoker.Value;
        StaticSetInvoker StaticSetInvoker => LazyStaticSetInvoker.Value;

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
            LazyStaticGetInvoker = new Lazy<StaticGetInvoker>(() => Generate.StaticGetter(Member));
            LazyStaticSetInvoker = new Lazy<StaticSetInvoker>(() => Generate.StaticSetter(Member));
            LazyInstanceGetInvoker = new Lazy<InstanceGetInvoker>(() => Generate.InstanceGetter(Member));
            LazyInstanceSetInvoker = new Lazy<InstanceSetInvoker>(() => Generate.InstanceSetter(Member));
        }

        [return: AllowNull]
        public object Get([AllowNull] object instance)
        {
            AssertInstanceMatches(instance);
            try
            {
                if (IsStatic)
                {
                    return StaticGetInvoker.Invoke();
                }

                return InstanceGetInvoker.Invoke(instance);
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
            AssertInstanceMatches(instance);

            if (!CanSet)
            {
                throw new MirraInvocationCannotSetException();
            }

            try
            {
                if (IsStatic)
                {
                    StaticSetInvoker.Invoke(value);
                }
                else
                {
                    InstanceSetInvoker.Invoke(instance, value);
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