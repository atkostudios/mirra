using System;
using System.Reflection;
using Atko.Dodge.Dynamic;
using NullGuard;

namespace Atko.Dodge.Models
{
    public abstract class AccessorModel : MemberModel
    {
        public abstract bool CanGet { get; }
        public abstract bool CanSet { get; }

        InstanceGetInvoker InstanceGetInvoker =>
            IsStatic
                ? null
                : instanceGetInvoker ??
                  (instanceGetInvoker = CodeGenerator.InstanceGetter(Member));

        InstanceSetInvoker InstanceSetInvoker =>
            IsStatic
                ? null
                : instanceSetInvoker ??
                  (instanceSetInvoker = CodeGenerator.InstanceSetter(Member));

        StaticGetInvoker StaticGetInvoker =>
            IsStatic
                ? staticGetInvoker ?? (staticGetInvoker = CodeGenerator.StaticGetter(Member))
                : null;

        StaticSetInvoker StaticSetInvoker =>
            IsStatic
                ? staticSetInvoker ?? (staticSetInvoker = CodeGenerator.StaticSetter(Member))
                : null;

        InstanceGetInvoker instanceGetInvoker;
        InstanceSetInvoker instanceSetInvoker;
        StaticGetInvoker staticGetInvoker;
        StaticSetInvoker staticSetInvoker;

        protected AccessorModel(Type owner, PropertyInfo property) : base(owner, property) { }
        protected AccessorModel(Type owner, FieldInfo member) : base(owner, member) { }

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
                throw new DodgeInvocationException(null, exception);
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
                throw new DodgeInvocationException(null, exception);
            }
        }
    }
}