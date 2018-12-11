using System;
using System.Reflection;
using Atko.Dodge.Generation;
using NullGuard;

namespace Atko.Dodge.Models
{
    public abstract class AccessorModel : MemberModel
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

        protected AccessorModel(Type owner, PropertyInfo property) : this(owner, (MemberInfo) property) { }
        protected AccessorModel(Type owner, FieldInfo member) : this(owner, (MemberInfo) member) { }

        AccessorModel(Type owner, MemberInfo member) : base(owner, member)
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
            catch (Exception exception)
            {
                throw new DodgeInvocationException(null, exception);
            }
        }

        public void Set([AllowNull] object instance, [AllowNull] object value)
        {
            AssertInstanceMatches(instance);
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
            catch (Exception exception)
            {
                throw new DodgeInvocationException(null, exception);
            }
        }
    }
}