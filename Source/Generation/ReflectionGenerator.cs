using System;
using System.Diagnostics;
using System.Reflection;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Generation
{
    class ReflectionGenerator : CodeGenerator
    {
        public override StaticGetInvoker StaticGetter(MemberInfo accessor)
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

        public override InstanceGetInvoker InstanceGetter(MemberInfo accessor)
        {
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractInstanceFieldGetter(accessor);
            if (extracted != null)
            {
                return extracted;
            }

            var property = (PropertyInfo)accessor;
            return (instance) =>
            {
                CheckArgument(property.DeclaringType, instance);
                return property.GetValue(instance);
            };
        }

        public override StaticSetInvoker StaticSetter(MemberInfo accessor)
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

        public override InstanceSetInvoker InstanceSetter(MemberInfo accessor)
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
                CheckArgument(property.DeclaringType, instance);
                CheckArgument(property.PropertyType, value);
                property.SetValue(instance, value);
            };
        }

        public override StaticMethodInvoker StaticMethod(MethodInfo method, int argumentCount)
        {
            Debug.Assert(method.IsStatic);

            var checker = new ArgumentChecker(method.GetParameters(), argumentCount);
            return (arguments) =>
            {
                checker.CheckArguments(arguments);
                return method.Invoke(null, arguments);
            };
        }

        public override InstanceMethodInvoker InstanceMethod(MethodInfo method, int argumentCount)
        {
            Debug.Assert(!method.IsStatic);

            var checker = new ArgumentChecker(method.GetParameters(), argumentCount);
            return (instance, arguments) =>
            {
                CheckArgument(method.DeclaringType, instance);
                checker.CheckArguments(arguments);
                return method.Invoke(instance, arguments);
            };
        }

        public override StaticMethodInvoker Constructor(ConstructorInfo constructor, int argumentCount)
        {
            var checker = new ArgumentChecker(constructor.GetParameters(), argumentCount);
            return (arguments) =>
            {
                checker.CheckArguments(arguments);
                return constructor.Invoke(arguments);
            };
        }

        public override IndexerGetInvoker InstanceIndexGetter(PropertyInfo property, int argumentCount)
        {
            var checker = new ArgumentChecker(property.GetIndexParameters(), argumentCount);
            return (instance, index) =>
            {
                CheckArgument(property.DeclaringType, instance);
                checker.CheckArguments(index);
                return property.GetValue(instance, index);
            };
        }

        public override IndexerSetInvoker InstanceIndexSetter(PropertyInfo property, int argumentCount)
        {
            var checker = new ArgumentChecker(property.GetIndexParameters(), argumentCount);
            return (instance, index, value) =>
            {
                CheckArgument(property.DeclaringType, instance);
                checker.CheckArguments(index);
                property.SetValue(instance, value, index);
            };
        }

        StaticGetInvoker StaticFieldGetter(FieldInfo field)
        {
            return () => field.GetValue(null);
        }

        InstanceGetInvoker InstanceFieldGetter(FieldInfo field)
        {
            return (instance) =>
            {
                CheckArgument(field.DeclaringType, instance);
                return field.GetValue(instance);
            };
        }

        StaticSetInvoker StaticFieldSetter(FieldInfo field)
        {
            return (value) =>
            {
                CheckArgument(field.FieldType, value);
                field.SetValue(null, value);
            };
        }

        InstanceSetInvoker InstanceFieldSetter(FieldInfo field)
        {
            return (instance, value) =>
            {
                CheckArgument(field.DeclaringType, instance);
                CheckArgument(field.FieldType, value);
                field.SetValue(instance, value);
            };
        }

        [return: AllowNull]
        InstanceGetInvoker ExtractInstanceFieldGetter(MemberInfo accessor)
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
        InstanceSetInvoker ExtractInstanceFieldSetter(MemberInfo accessor)
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
        StaticGetInvoker ExtractStaticFieldGetter(MemberInfo accessor)
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
        StaticSetInvoker ExtractStaticFieldSetter(MemberInfo accessor)
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

        void CheckArgument(Type type, object argument)
        {
            if (type.IsValueType && argument == null)
            {
                throw new MirraInvocationArgumentException();
            }

            if (!type.IsInstanceOfType(argument))
            {
                throw new MirraInvocationArgumentException();
            }
        }
    }
}
