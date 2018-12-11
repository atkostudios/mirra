using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Atko.Dodge.Utility;
using NullGuard;

namespace Atko.Dodge.Generation
{
    static class Generate
    {
        public static StaticGetInvoker StaticGetter(MemberInfo accessor)
        {
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractStaticFieldGetter(accessor);
            if (extracted != null)
            {
                return extracted;
            }

            var accessExpression = GetAccessExpression(accessor);
            var castAccessExpression = Expression.Convert(accessExpression, typeof(object));

            return Expression.Lambda<StaticGetInvoker>(castAccessExpression).Compile();
        }

        public static InstanceGetInvoker InstanceGetter(MemberInfo accessor)
        {
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractInstanceFieldGetter(accessor);
            if (extracted != null)
            {
                return extracted;
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
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractStaticFieldSetter(accessor);
            if (extracted != null)
            {
                return extracted;
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
            Debug.Assert(IsAccessor(accessor));

            var extracted = ExtractInstanceFieldSetter(accessor);
            if (extracted != null)
            {
                return extracted;
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

        public static StaticMethodInvoker StaticMethod(MethodInfo method, int argumentCount)
        {
            Debug.Assert(method.IsStatic);

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
            Debug.Assert(!method.IsStatic);

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

        public static IndexerGetInvoker InstanceIndexGetter(PropertyInfo property, int argumentCount)
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

        public static IndexerSetInvoker InstanceIndexSetter(PropertyInfo property, int argumentCount)
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

        static StaticGetInvoker StaticFieldGetter(FieldInfo field)
        {
            var name = $"__GENERATED_GET__{field.Name}";
            var method = new DynamicMethod(name, typeof(object), ArrayUtility<Type>.Empty, field.DeclaringType, true);
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

        static InstanceGetInvoker InstanceFieldGetter(FieldInfo field)
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

        static StaticSetInvoker StaticFieldSetter(FieldInfo field)
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

        static InstanceSetInvoker InstanceFieldSetter(FieldInfo field)
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

        [return: AllowNull]
        static Expression GetAccessExpression(MemberInfo member, Expression castInstanceExpression = null)
        {
            if (member is PropertyInfo property)
            {
                return Expression.Property(castInstanceExpression, property);
            }

            if (member is FieldInfo field)
            {
                return Expression.Field(castInstanceExpression, field);
            }

            return null;
        }

        static Expression[] GetArguments(ParameterInfo[] parameters, Expression arguments, int count)
        {
            #if DEBUG
            var minArgumentCount = parameters
                .TakeWhile((current) => !current.IsOptional)
                .Count();

            var maxArgumentCount = parameters.Length;

            Debug.Assert(count >= minArgumentCount && count <= maxArgumentCount);
            #endif

            var types = parameters
                .Select((current) => current.ParameterType)
                .ToArray();

            return Enumerable
                .Range(0, count)
                .Select((i) => Expression.ArrayAccess(arguments, Expression.Constant(i)))
                .Zip(types, Expression.Convert)
                .Cast<Expression>()
                .ToArray();
        }

        static bool IsAccessor(MemberInfo member)
        {
            return (member.MemberType & (MemberTypes.Property | MemberTypes.Field)) != 0;
        }
    }
}