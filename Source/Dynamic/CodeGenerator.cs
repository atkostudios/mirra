using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Atko.Dodge.Utility;
using NullGuard;

namespace Atko.Dodge.Dynamic
{
    static class CodeGenerator
    {
        public static StaticGetInvoker StaticGetter(MemberInfo accessor)
        {
            if ((accessor.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0)
            {
                throw new ArgumentException(nameof(accessor));
            }

            var reduced = ReduceToStaticFieldGet(accessor);
            if (reduced != null)
            {
                return reduced;
            }

            var accessExpression = GetAccessExpression(accessor);
            var castAccessExpression = Expression.Convert(accessExpression, typeof(object));

            return Expression.Lambda<StaticGetInvoker>(castAccessExpression).Compile();
        }

        public static InstanceGetInvoker InstanceGetter(MemberInfo accessor)
        {
            if ((accessor.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0)
            {
                throw new ArgumentException(nameof(accessor));
            }

            var reduced = ReduceToInstanceFieldGet(accessor);
            if (reduced != null)
            {
                return reduced;
            }

            var instanceExpression = Expression.Parameter(typeof(object), "instance");
            var castInstanceExpression = Expression.Convert(instanceExpression, accessor.DeclaringType);

            var accessExpression = GetAccessExpression(accessor, castInstanceExpression);
            var castFieldExpression = Expression.Convert(accessExpression, typeof(object));

            var parameters = new[]
            {
                instanceExpression
            };

            return Expression.Lambda<InstanceGetInvoker>(castFieldExpression, parameters).Compile();
        }

        public static StaticSetInvoker StaticSetter(MemberInfo accessor)
        {
            if ((accessor.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0)
            {
                throw new ArgumentException(nameof(accessor));
            }

            var reduced = ReduceToStaticFieldSet(accessor);
            if (reduced != null)
            {
                return reduced;
            }

            var valueParameter = Expression.Parameter(typeof(object), "value");
            var castValueParameter = Expression.Convert(valueParameter, TypeUtility.GetReturnType(accessor));
            var accessExpression = GetAccessExpression(accessor);
            var assignmentExpression = Expression.Assign(accessExpression, castValueParameter);

            var parameters = new[]
            {
                valueParameter
            };

            return Expression.Lambda<StaticSetInvoker>(assignmentExpression, parameters).Compile();
        }

        public static InstanceSetInvoker InstanceSetter(MemberInfo accessor)
        {
            if ((accessor.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0)
            {
                throw new ArgumentException(nameof(accessor));
            }

            var reduced = ReduceToInstanceFieldSet(accessor);
            if (reduced != null)
            {
                return reduced;
            }

            var instanceExpression = Expression.Parameter(typeof(object), "instance");
            var valueExpression = Expression.Parameter(typeof(object), "value");
            var castInstanceExpression = Expression.Convert(instanceExpression, accessor.DeclaringType);
            var castValueExpression = Expression.Convert(valueExpression, TypeUtility.GetReturnType(accessor));
            var accessExpression = GetAccessExpression(accessor, castInstanceExpression);
            var assignExpression = Expression.Assign(accessExpression, castValueExpression);

            var parameters = new[]
            {
                instanceExpression,
                valueExpression
            };

            return Expression.Lambda<InstanceSetInvoker>(assignExpression, parameters).Compile();
        }

        public static StaticGetInvoker StaticFieldGetter(FieldInfo field)
        {
            var name = $"__GENERATED_GET__{field.Name}";
            var method = new DynamicMethod(name, typeof(object), new Type[] { }, field.DeclaringType, true);
            var generator = method.GetILGenerator();

            {
                generator.Emit(OpCodes.Ldsfld, field);
                if (field.FieldType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, field.FieldType);
                }

                generator.Emit(OpCodes.Castclass, typeof(object));
                generator.Emit(OpCodes.Ret);
            }

            return (StaticGetInvoker) method.CreateDelegate(typeof(StaticGetInvoker));
        }

        public static InstanceGetInvoker InstanceFieldGetter(FieldInfo field)
        {
            var name = $"__GENERATED_GET__{field.Name}";
            var method = new DynamicMethod(name, typeof(object), new[] {typeof(object)}, field.DeclaringType, true);
            var generator = method.GetILGenerator();

            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, field.DeclaringType);
                generator.Emit(OpCodes.Ldfld, field);
                if (field.FieldType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, field.FieldType);
                }

                generator.Emit(OpCodes.Castclass, typeof(object));
                generator.Emit(OpCodes.Ret);
            }

            return (InstanceGetInvoker) method.CreateDelegate(typeof(InstanceGetInvoker));
        }

        public static StaticSetInvoker StaticFieldSetter(FieldInfo field)
        {
            var name = $"__GENERIC_SET__{field.Name}";
            var method = new DynamicMethod(name, typeof(void), new[] {typeof(object)}, field.DeclaringType, true);

            var generator = method.GetILGenerator();

            {
                generator.Emit(OpCodes.Ldarg_0);
                if (field.FieldType.IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, field.FieldType);
                }
                else
                {
                    generator.Emit(OpCodes.Castclass, field.FieldType);
                }

                generator.Emit(OpCodes.Stsfld, field);
                generator.Emit(OpCodes.Ret);
            }

            return (StaticSetInvoker) method.CreateDelegate(typeof(StaticSetInvoker));
        }

        public static InstanceSetInvoker InstanceFieldSetter(FieldInfo field)
        {
            var name = $"__GENERIC_SET__{field.Name}";
            var method = new DynamicMethod(name, typeof(void), new[] {typeof(object), typeof(object)},
                field.DeclaringType, true);

            var generator = method.GetILGenerator();

            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, field.DeclaringType);
                generator.Emit(OpCodes.Ldarg_1);
                if (field.FieldType.IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, field.FieldType);
                }
                else
                {
                    generator.Emit(OpCodes.Castclass, field.FieldType);
                }

                generator.Emit(OpCodes.Stfld, field);
                generator.Emit(OpCodes.Ret);
            }

            return (InstanceSetInvoker) method.CreateDelegate(typeof(InstanceSetInvoker));
        }

        public static StaticMethodInvoker StaticMethod(MethodInfo method, int argumentCount)
        {
            if (!method.IsStatic)
            {
                throw new ArgumentException(nameof(method));
            }

            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
            var argumentExpressions = GetArguments(method.GetParameters(), argumentsParameter, argumentCount);
            var callExpression = Expression.Call(null, method, argumentExpressions);

            var body = TypeUtility.GetReturnType(method) == typeof(void)
                ? Expression.Block(callExpression, Expression.Constant(null, typeof(object)))
                : (Expression) Expression.Convert(callExpression, typeof(object));

            var parameters = new[]
            {
                argumentsParameter
            };

            return Expression.Lambda<StaticMethodInvoker>(body, parameters).Compile();
        }

        public static InstanceMethodInvoker InstanceMethod(MethodInfo method, int argumentCount)
        {
            if (method.IsStatic)
            {
                throw new ArgumentException(nameof(method));
            }

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var castedInstanceParameter = Expression.Convert(instanceParameter, method.DeclaringType);
            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
            var argumentExpressions = GetArguments(method.GetParameters(), argumentsParameter, argumentCount);
            var callExpression = Expression.Call(castedInstanceParameter, method, argumentExpressions);

            var body = TypeUtility.GetReturnType(method) == typeof(void)
                ? Expression.Block(callExpression, Expression.Constant(null, typeof(object)))
                : (Expression) Expression.Convert(callExpression, typeof(object));

            var parameters = new[]
            {
                instanceParameter,
                argumentsParameter
            };

            return Expression.Lambda<InstanceMethodInvoker>(body, parameters).Compile();
        }

        public static StaticMethodInvoker Constructor(ConstructorInfo constructor, int argumentCount)
        {
            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
            var argumentExpressions = GetArguments(constructor.GetParameters(), argumentsParameter, argumentCount);
            var newExpression = Expression.New(constructor, argumentExpressions);
            var body = Expression.Convert(newExpression, typeof(object));

            var parameters = new[]
            {
                argumentsParameter
            };

            return Expression.Lambda<StaticMethodInvoker>(body, parameters).Compile();
        }

        public static IndexerGetInvoker IndexGetter(PropertyInfo property, int argumentCount)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var castedInstanceParameter = Expression.Convert(instanceParameter, property.DeclaringType);
            var indexParameter = Expression.Parameter(typeof(object[]), "index");
            var indexExpressions = GetArguments(property.GetIndexParameters(), indexParameter, argumentCount);
            var accessExpression = Expression.MakeIndex(castedInstanceParameter, property, indexExpressions);
            var castedAccessExpression = Expression.Convert(accessExpression, typeof(object));

            var parameters = new[]
            {
                instanceParameter,
                indexParameter
            };

            return Expression.Lambda<IndexerGetInvoker>(castedAccessExpression, parameters).Compile();
        }

        public static IndexerSetInvoker IndexSetter(PropertyInfo property, int argumentCount)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var castedInstanceParameter = Expression.Convert(instanceParameter, property.DeclaringType);
            var indexParameter = Expression.Parameter(typeof(object[]), "index");
            var indexExpressions = GetArguments(property.GetIndexParameters(), indexParameter, argumentCount);
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var castedValueParameter = Expression.Convert(valueParameter, property.PropertyType);
            var accessExpression = Expression.MakeIndex(castedInstanceParameter, property, indexExpressions);
            var assignExpression = Expression.Assign(accessExpression, castedValueParameter);

            var parameters = new[]
            {
                instanceParameter,
                indexParameter,
                valueParameter
            };

            return Expression.Lambda<IndexerSetInvoker>(assignExpression, parameters).Compile();
        }

        static Expression GetAccessExpression(MemberInfo member, Expression castInstanceExpression = null)
        {
            if (member.MemberType == MemberTypes.Property)
            {
                return Expression.Property(castInstanceExpression, (PropertyInfo) member);
            }

            return Expression.Field(castInstanceExpression, (FieldInfo) member);
        }

        static Expression[] GetArguments(ParameterInfo[] parameters, Expression arguments, int count)
        {
            var types = parameters
                .Select((current) => current.ParameterType)
                .ToArray();

            return Enumerable.Range(0, count)
                .Select((i) => Expression.ArrayAccess(arguments, Expression.Constant(i)))
                .Zip(types, Expression.Convert)
                .Cast<Expression>()
                .ToArray();
        }

        [return: AllowNull]
        static InstanceGetInvoker ReduceToInstanceFieldGet(MemberInfo accessor)
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
        static InstanceSetInvoker ReduceToInstanceFieldSet(MemberInfo accessor)
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
        static StaticGetInvoker ReduceToStaticFieldGet(MemberInfo accessor)
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
        static StaticSetInvoker ReduceToStaticFieldSet(MemberInfo accessor)
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