using System;
using System.Reflection;
using NullGuard;

namespace Ducktype.Models
{
    public abstract class AccessorModel : MemberModel
    {
        public abstract bool CanGet { get; }
        public abstract bool CanSet { get; }

        InstanceGetInvoker InstanceGetInvoker =>
            IsInstance
                ? instanceGetInvoker ??
                  (instanceGetInvoker = CodeGenerator.InstanceGetter(Member))
                : null;

        InstanceSetInvoker InstanceSetInvoker =>
            IsInstance
                ? instanceSetInvoker ??
                  (instanceSetInvoker = CodeGenerator.InstanceSetter(Member))
                : null;

        StaticGetInvoker StaticGetInvoker =>
            IsInstance
                ? null
                : staticGetInvoker ?? (staticGetInvoker = CodeGenerator.StaticGetter(Member));

        StaticSetInvoker StaticSetInvoker =>
            IsInstance
                ? null
                : staticSetInvoker ?? (staticSetInvoker = CodeGenerator.StaticSetter(Member));

        InstanceGetInvoker instanceGetInvoker;
        InstanceSetInvoker instanceSetInvoker;
        StaticGetInvoker staticGetInvoker;
        StaticSetInvoker staticSetInvoker;

        protected AccessorModel(Type owner, PropertyInfo property) : base(owner, property) { }
        protected AccessorModel(Type owner, FieldInfo property) : base(owner, property) { }

        AccessorModel(Type owner, MemberInfo member) : base(owner, member) { }

        [AllowNull]
        public object this[object instance]
        {
            get => Get(instance);
            set => Set(instance, value);
        }

        [return: AllowNull]
        public object Get([AllowNull] object instance)
        {
            AssertInstanceMatches(instance);
            try
            {
                return InstanceGetInvoker.Invoke(instance);
            }
            catch (Exception exception)
            {
                throw new DucktypeInvocationException(null, exception);
            }
        }

        public void Set([AllowNull] object instance, [AllowNull] object value)
        {
            AssertInstanceMatches(instance);
            try
            {
                InstanceSetInvoker.Invoke(instance, value);
            }
            catch (Exception exception)
            {
                throw new DucktypeInvocationException(null, exception);
            }
        }
    }
}