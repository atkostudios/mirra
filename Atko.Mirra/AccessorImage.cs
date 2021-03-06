using System;
using System.Reflection;
using NullGuard;

using Atko.Mirra.Generation;

namespace Atko.Mirra
{
    /// <summary>
    /// Wrapper class for both <see cref="PropertyInfo"/> and <see cref="FieldInfo"/> that provides extended
    /// functionality and reflection performance.
    /// </summary>
    public abstract class AccessorImage : GetSetImage
    {
        Lazy<InstanceGetInvoker> LazyInstanceGetInvoker { get; }
        Lazy<InstanceSetInvoker> LazyInstanceSetInvoker { get; }
        Lazy<StaticGetInvoker> LazyStaticGetInvoker { get; }
        Lazy<StaticSetInvoker> LazyStaticSetInvoker { get; }

        internal AccessorImage(PropertyInfo property) : this((MemberInfo)property)
        { }

        internal AccessorImage(FieldInfo field) : this((MemberInfo)field)
        { }

        AccessorImage(MemberInfo member) : base(member)
        {
            LazyStaticGetInvoker = new Lazy<StaticGetInvoker>(() => CodeGenerator.StaticGetter(Member));
            LazyStaticSetInvoker = new Lazy<StaticSetInvoker>(() => CodeGenerator.StaticSetter(Member));
            LazyInstanceGetInvoker = new Lazy<InstanceGetInvoker>(() => CodeGenerator.InstanceGetter(Member));
            LazyInstanceSetInvoker = new Lazy<InstanceSetInvoker>(() => CodeGenerator.InstanceSetter(Member));
        }

        /// <summary>
        /// Get the value of the property or field on a provided instance object.
        /// If the property or field is non-static, the instance parameter must be an instance of the correct type.
        /// If the property or field is static, the instance parameter must be null.
        /// </summary>
        /// <param name="instance">The instance to get the property or field value from.</param>
        /// <returns>The value of the property or field.</returns>
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

        /// <summary>
        /// Set the value of the property or field on a provided instance object.
        /// If the property or field is non-static, the instance parameter must be an instance of the correct type.
        /// If the property or field is static, the instance parameter must be null.
        /// </summary>
        /// <param name="instance">The instance to set the property or field value on.</param>
        /// <param name="value">The value to assign to the property or field.</param>
        /// <summary>
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