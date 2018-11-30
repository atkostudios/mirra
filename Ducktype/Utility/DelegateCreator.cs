using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Utility
{
    public static class DelegateCreator
    {
        public static Func<object> CreateStaticGetterDelegate(MemberInfo member)
        {
            var expression = member is PropertyInfo property
                ? Expression.Property(null, property)
                : Expression.Field(null, (FieldInfo) member);

            var castedExpression = Expression.Convert(expression, typeof(object));

            return Expression.Lambda<Func<object>>(castedExpression).Compile();
        }

        public static Func<object, object> CreateInstanceGetterDelegate(MemberInfo member)
        {
            var instanceExpression = Expression.Parameter(typeof(object), "instance");
            var castInstanceExpression = Expression.Convert(instanceExpression, member.DeclaringType);

            var fieldExpression = Expression.PropertyOrField(castInstanceExpression, member.Name);
            var castFieldExpression = Expression.Convert(fieldExpression, typeof(object));

            var parameters = new[]
            {
                instanceExpression
            };

            return Expression.Lambda<Func<object, object>>(castFieldExpression, parameters).Compile();
        }

        public static Action<object> CreateStaticSetterDelegate(MemberInfo member)
        {
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var expression = member is PropertyInfo property
                ? Expression.Property(null, property)
                : Expression.Field(null, (FieldInfo) member);

            var assignmentExpression = Expression.Assign(expression, valueParameter);

            var parameters = new []
            {
                valueParameter
            };

            return Expression.Lambda<Action<object>>(assignmentExpression, parameters).Compile();
        }

        public static Action<object, object> CreateInstanceSetterDelegate(MemberInfo member)
        {
            var returnType = member is FieldInfo field
                ? field.FieldType
                : ((PropertyInfo) member).PropertyType;

            var instanceExpression = Expression.Parameter(typeof(object), "instance");
            var valueExpression = Expression.Parameter(typeof(object), "value");
            var castInstanceExpression = Expression.Convert(instanceExpression, member.DeclaringType);
            var castValueExpression = Expression.Convert(valueExpression, returnType);
            var fieldExpression = Expression.PropertyOrField(castInstanceExpression, member.Name);
            var assignExpression = Expression.Assign(fieldExpression, castValueExpression);

            var parameters = new[]
            {
                instanceExpression,
                valueExpression
            };

            return Expression.Lambda<Action<object, object>>(assignExpression, parameters).Compile();
        }

        public static Func<object[], object> CreateStaticMethodDelegate(MethodInfo method, int argumentCount)
        {
            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
            var argumentExpressions = GetMethodArgumentExpressions(method, argumentsParameter, argumentCount);
            var callExpression = Expression.Call(null, method, argumentExpressions);
            var callExpressionCasted = Expression.Convert(callExpression, typeof(object));

            var parameters = new []
            {
                argumentsParameter
            };

            return Expression.Lambda<Func<object, object[]>>(callExpressionCasted, parameters).Compile();
        }

        public static Func<object, object[], object> CreateInstanceMethodDelegate(MethodInfo method, int argumentCount)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var instanceParameterCasted = Expression.Convert(instanceParameter, method.DeclaringType);
            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
            var argumentExpressions = GetMethodArgumentExpressions(method, argumentsParameter, argumentCount);
            var callExpression = Expression.Call(instanceParameterCasted, method, argumentExpressions);
            var callExpressionCasted = Expression.Convert(callExpression, typeof(object));

            var parameters = new []
            {
                instanceParameter,
                argumentsParameter
            };

            return Expression.Lambda<Func<object, object[], object>>(callExpressionCasted, parameters).Compile();
        }

        static Expression[] GetMethodArgumentExpressions(MethodInfo method, Expression arguments, int count)
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