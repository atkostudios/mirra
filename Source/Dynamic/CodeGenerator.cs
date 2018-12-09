using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Atko.Dodge.Utility;

namespace Atko.Dodge.Dynamic
{
    static class CodeGenerator
    {
        public static StaticGetInvoker StaticGetter(MemberInfo member)
        {
            if ((member.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0)
            {
                throw new ArgumentException(nameof(member));
            }

            var accessExpression = GetAccessExpression(member);
            var castAccessExpression = Expression.Convert(accessExpression, typeof(object));

            return Expression.Lambda<StaticGetInvoker>(castAccessExpression).Compile();
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

        public static InstanceGetInvoker InstanceFieldSetter(FieldInfo field)
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

            return (InstanceGetInvoker) method.CreateDelegate(typeof(InstanceGetInvoker));
        }

        public static InstanceGetInvoker InstanceGetter(MemberInfo member)
        {
            if ((member.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0)
            {
                throw new ArgumentException(nameof(member));
            }

            var instanceExpression = Expression.Parameter(typeof(object), "instance");
            var castInstanceExpression = Expression.Convert(instanceExpression, member.DeclaringType);

            var accessExpression = GetAccessExpression(member, castInstanceExpression);
            var castFieldExpression = Expression.Convert(accessExpression, typeof(object));

            var parameters = new[]
            {
                instanceExpression
            };

            return Expression.Lambda<InstanceGetInvoker>(castFieldExpression, parameters).Compile();
        }

        public static StaticSetInvoker StaticSetter(MemberInfo member)
        {
            if ((member.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0)
            {
                throw new ArgumentException(nameof(member));
            }

            var valueParameter = Expression.Parameter(typeof(object), "value");
            var castValueParameter = Expression.Convert(valueParameter, TypeUtility.GetReturnType(member));
            var accessExpression = GetAccessExpression(member);
            var assignmentExpression = Expression.Assign(accessExpression, castValueParameter);

            var parameters = new[]
            {
                valueParameter
            };

            return Expression.Lambda<StaticSetInvoker>(assignmentExpression, parameters).Compile();
        }

        public static InstanceSetInvoker InstanceSetter(MemberInfo member)
        {
            if ((member.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0)
            {
                throw new ArgumentException(nameof(member));
            }

            var instanceExpression = Expression.Parameter(typeof(object), "instance");
            var valueExpression = Expression.Parameter(typeof(object), "value");
            var castInstanceExpression = Expression.Convert(instanceExpression, member.DeclaringType);
            var castValueExpression = Expression.Convert(valueExpression, TypeUtility.GetReturnType(member));
            var accessExpression = GetAccessExpression(member, castInstanceExpression);
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
            if (!method.IsStatic)
            {
                throw new ArgumentException(nameof(method));
            }

            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
            var argumentExpressions = GetMethodArgumentExpressions(method, argumentsParameter, argumentCount);
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
            var argumentExpressions = GetMethodArgumentExpressions(method, argumentsParameter, argumentCount);
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
            var argumentExpressions = GetMethodArgumentExpressions(constructor, argumentsParameter, argumentCount);
            var newExpression = Expression.New(constructor, argumentExpressions);
            var body = Expression.Convert(newExpression, typeof(object));

            var parameters = new[]
            {
                argumentsParameter
            };

            return Expression.Lambda<StaticMethodInvoker>(body, parameters).Compile();
        }

        static Expression GetAccessExpression(MemberInfo member, Expression castInstanceExpression = null)
        {
            if (member is PropertyInfo property && !property.CanWrite)
            {
                var backing = TypeUtility.GetBackingField(property, castInstanceExpression != null);
                if (backing != null)
                {
                    return Expression.Field(castInstanceExpression, backing);
                }
            }

            if (member.MemberType == MemberTypes.Property)
            {
                return Expression.Property(castInstanceExpression, (PropertyInfo) member);
            }

            return Expression.Field(castInstanceExpression, (FieldInfo) member);
        }

        static Expression[] GetMethodArgumentExpressions(MethodBase method, Expression arguments, int count)
        {
            var types = method
                .GetParameters()
                .Select((current) => current.ParameterType)
                .ToArray();

            return Enumerable.Range(0, count)
                .Select((i) => Expression.ArrayAccess(arguments, Expression.Constant(i)))
                .Zip(types, Expression.Convert)
                .Cast<Expression>()
                .ToArray();
        }
    }
}