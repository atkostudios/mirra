using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NullGuard;
using Source.Utility;

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

        static Cache<Pair<Type, Type>, Type> ImplementationCache { get; }
            = new Cache<Pair<Type, Type>, Type>();

        public static IEnumerable<Type> Inheritance(this Type type)
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

        public static bool IsBackingField(this FieldInfo field)
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
        public static Type GetImplementation(Type type, Type generic)
        {
            if (type == generic)
            {
                return type;
            }

            return ImplementationCache.GetOrAdd(new Pair<Type, Type>(type, generic),
                (input) => GetImplementationInternal(input.First, input.Second));
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

        static Type GetImplementationInternal(Type type, Type generic)
        {
            if (!generic.IsGenericType || !generic.IsGenericTypeDefinition)
            {
                return generic.IsAssignableFrom(type) ? generic : null;
            }

            if (generic.IsInterface)
            {
                foreach (var ancestor in Inheritance(type))
                {
                    var interfaces = ancestor.GetInterfaces();
                    foreach (var implemented in interfaces)
                    {
                        if (implemented.IsGenericType &&
                            implemented.GetGenericTypeDefinition() == generic)
                        {
                            return implemented;
                        }
                    }
                }
            }
            else
            {
                foreach (var ancestor in Inheritance(type))
                {
                    if (ancestor.IsGenericType &&
                        ancestor.GetGenericTypeDefinition() == generic)
                    {
                        return ancestor;
                    }
                }
            }

            return null;
        }
    }
}