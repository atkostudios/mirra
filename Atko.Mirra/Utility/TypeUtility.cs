using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NullGuard;

namespace Atko.Mirra.Utility
{
    static class TypeUtility
    {
        public const BindingFlags InstanceBinding = BindingFlags.Instance |
                                                    BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.DeclaredOnly;

        public const BindingFlags StaticBinding = BindingFlags.Static |
                                                  BindingFlags.Public |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.DeclaredOnly;

        const string BackingFieldPrefix = "<";
        const string BackingFieldSuffix = ">k__BackingField";

        public static IEnumerable<Type> Inheritance(Type type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        public static string GetBackingFieldName(PropertyInfo property)
        {
            return $"{BackingFieldPrefix}{property.Name}{BackingFieldSuffix}";
        }

        public static bool IsBackingField(FieldInfo field)
        {
            return field.IsDefined(typeof(CompilerGeneratedAttribute)) &&
                   field.Name.StartsWith(BackingFieldPrefix) &&
                   field.Name.EndsWith(BackingFieldSuffix);
        }

        [return: AllowNull]
        public static FieldInfo GetBackingField(PropertyInfo property, bool instance)
        {
            var name = GetBackingFieldName(property);
            var type = property.DeclaringType;
            if (type == null)
            {
                return null;
            }

            return type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
        }

        [return: AllowNull]
        public static Type GetReturnType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
            }

            return null;
        }

        public static bool HasSpecialParameters(IEnumerable<ParameterInfo> parameters)
        {
            return parameters.Any((current) => current.IsIn || current.IsOut || current.ParameterType.IsByRef);
        }

        public static bool CanBeConstant(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(String);
        }

        public static bool CanBeConstantStruct(Type type)
        {
            return type.IsPrimitive || type.IsEnum;
        }
    }
}