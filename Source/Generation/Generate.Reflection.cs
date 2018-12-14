#if !HAVE_DYNAMIC

using System.Diagnostics;
using System.Reflection;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Generation
{
    static partial class Generate
    {
        public static StaticGetInvoker StaticGetter(MemberInfo accessor)
        {
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractStaticFieldGetter(accessor);
            if (extracted != null)
            {
                return extracted;
            }

            var property = (PropertyInfo)accessor;
            return () => property.GetValue(null);
        }

        public static InstanceGetInvoker InstanceGetter(MemberInfo accessor)
        {
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractInstanceFieldGetter(accessor);
            if (extracted != null)
            {
                return extracted;
            }

            var property = (PropertyInfo)accessor;
            return (instance) => property.GetValue(instance);
        }

        public static StaticSetInvoker StaticSetter(MemberInfo accessor)
        {
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractStaticFieldSetter(accessor);
            if (extracted != null)
            {
                return extracted;
            }

            var property = (PropertyInfo)accessor;

            return (value) =>
            {
                CheckArgument(property.PropertyType, value);
                property.SetValue(null, value);
            };
        }

        public static InstanceSetInvoker InstanceSetter(MemberInfo accessor)
        {
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractInstanceFieldSetter(accessor);
            if (extracted != null)
            {
                return extracted;
            }

            var property = (PropertyInfo)accessor;
            return (instance, value) =>
            {
                CheckInstance(property.DeclaringType, instance);
                CheckArgument(property.PropertyType, value);
                property.SetValue(instance, value);
            };
        }

        public static StaticMethodInvoker StaticMethod(MethodInfo method, int argumentCount)
        {
            Debug.Assert(method.IsStatic);

            var checker = new ArgumentChecker(method.GetParameters());
            return (arguments) =>
            {
                checker.Check(arguments);
                return method.Invoke(null, arguments);
            };
        }

        public static InstanceMethodInvoker InstanceMethod(MethodInfo method, int argumentCount)
        {
            Debug.Assert(!method.IsStatic);

            var checker = new ArgumentChecker(method.GetParameters());
            return (instance, arguments) =>
            {
                checker.Check(arguments);
                return method.Invoke(instance, arguments);
            };
        }

        public static StaticMethodInvoker Constructor(ConstructorInfo constructor, int argumentCount)
        {
            var checker = new ArgumentChecker(constructor.GetParameters());
            return (arguments) =>
            {
                checker.Check(arguments);
                return constructor.Invoke(arguments);
            };
        }

        public static IndexerGetInvoker InstanceIndexGetter(PropertyInfo property, int argumentCount)
        {
            var checker = new ArgumentChecker(property.GetIndexParameters());
            return (instance, index) =>
            {
                checker.Check(index);
                return property.GetValue(instance, index);
            };
        }

        public static IndexerSetInvoker InstanceIndexSetter(PropertyInfo property, int argumentCount)
        {
            var checker = new ArgumentChecker(property.GetIndexParameters());
            return (instance, index, value) =>
            {
                checker.Check(index);
                property.SetValue(instance, value, index);
            };
        }

        static StaticGetInvoker StaticFieldGetter(FieldInfo field)
        {
            return () => field.GetValue(null);
        }

        static InstanceGetInvoker InstanceFieldGetter(FieldInfo field)
        {
            return (instance) =>
            {
                CheckInstance(field.DeclaringType, instance);
                return field.GetValue(instance);
            };
        }

        static StaticSetInvoker StaticFieldSetter(FieldInfo field)
        {
            return (value) =>
            {
                CheckArgument(field.FieldType, value);
                field.SetValue(null, value);
            };
        }

        static InstanceSetInvoker InstanceFieldSetter(FieldInfo field)
        {
            return (instance, value) =>
            {
                CheckInstance(field.DeclaringType, instance);
                CheckArgument(field.FieldType, value);
                field.SetValue(instance, value);
            };
        }

        [return: AllowNull]
        static InstanceGetInvoker ExtractInstanceFieldGetter(MemberInfo accessor)
        {
            if (accessor is FieldInfo field)
            {
                return InstanceFieldGetter(field);
            }

            if (accessor is PropertyInfo property)
            {
                var backing = TypeUtility.GetBackingField(property, true);
                if (backing != null)
                {
                    return InstanceFieldGetter(backing);
                }
            }

            return null;
        }

        [return: AllowNull]
        static InstanceSetInvoker ExtractInstanceFieldSetter(MemberInfo accessor)
        {
            if (accessor is FieldInfo field)
            {
                return InstanceFieldSetter(field);
            }

            if (accessor is PropertyInfo property)
            {
                var backing = TypeUtility.GetBackingField(property, true);
                if (backing != null)
                {
                    return InstanceFieldSetter(backing);
                }
            }

            return null;
        }

        [return: AllowNull]
        static StaticGetInvoker ExtractStaticFieldGetter(MemberInfo accessor)
        {
            if (accessor is FieldInfo field)
            {
                return StaticFieldGetter(field);
            }

            if (accessor is PropertyInfo property)
            {
                var backing = TypeUtility.GetBackingField(property, false);
                if (backing != null)
                {
                    return StaticFieldGetter(backing);
                }
            }

            return null;
        }

        [return: AllowNull]
        static StaticSetInvoker ExtractStaticFieldSetter(MemberInfo accessor)
        {
            if (accessor is FieldInfo field)
            {
                return StaticFieldSetter(field);
            }

            if (accessor is PropertyInfo property)
            {
                var backing = TypeUtility.GetBackingField(property, false);
                if (backing != null)
                {
                    return StaticFieldSetter(backing);
                }
            }

            return null;
        }
    }
}

#endif