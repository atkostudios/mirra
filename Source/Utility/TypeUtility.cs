using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NullGuard;

namespace Atko.Dodge.Utility
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

        static Cache<(Type, Type), Type> ImplementationCache { get; }
            = new Cache<(Type, Type), Type>();

        static Cache<(Type, bool, string), FieldInfo> FieldCache { get; } =
            new Cache<(Type, bool, string), FieldInfo>();

        static Cache<(PropertyInfo, bool), FieldInfo> BackingFieldCache { get; } =
            new Cache<(PropertyInfo, bool), FieldInfo>();

        public static IEnumerable<Type> Inheritance(this Type type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        [return: AllowNull]
        public static FieldInfo GetField(Type type, bool instance, string name)
        {
            return FieldCache.GetOrAdd((type, instance, name),
                (input) => GetFieldInternal(input.Item1, input.Item2, input.Item3));
        }

        [return: AllowNull]
        public static FieldInfo GetBackingField(PropertyInfo property, bool instance)
        {
            return BackingFieldCache.GetOrAdd((property, instance),
                (input) => GetBackingFieldInternal(input.Item1, input.Item2));
        }

        [return: AllowNull]
        public static Type GetImplementation(Type type, Type generic)
        {
            if (type == generic)
            {
                return type;
            }

            return ImplementationCache.GetOrAdd((type, generic),
                (input) => GetImplementationInternal(input.Item1, input.Item2));
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
        public static Type GetReturnType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo) member).PropertyType;
                case MemberTypes.Method:
                    return ((MethodInfo) member).ReturnType;
            }

            return null;
        }

        public static bool IsStatic(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                    return false;
                case MemberTypes.Property:
                    var property = ((PropertyInfo) member);
                    if (property.CanRead)
                    {
                        return property.GetGetMethod(true).IsStatic;
                    }

                    return property.GetSetMethod(true).IsStatic;
                case MemberTypes.Field:
                    return ((FieldInfo) member).IsStatic;
                case MemberTypes.Method:
                    return ((MethodInfo) member).IsStatic;
            }

            throw new ArgumentException(nameof(member));
        }

        [return: AllowNull]
        static FieldInfo GetFieldInternal(Type type, bool instance, string name)
        {
            if (!instance)
            {
                return type.GetField(name, StaticBinding);
            }

            foreach (var ancestor in Inheritance(type))
            {
                var field = ancestor.GetField(name, InstanceBinding);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }

        [return: AllowNull]
        static FieldInfo GetBackingFieldInternal(PropertyInfo property, bool instance)
        {
            if (property.GetGetMethod(true).IsDefined(typeof(CompilerGeneratedAttribute)))
            {
                var name = GetBackingFieldName(property);
                var type = property.DeclaringType;
                if (type == null)
                {
                    return null;
                }

                var field = GetField(type, instance, name);
                if (field == null)
                {
                    return null;
                }

                if (field.IsDefined(typeof(CompilerGeneratedAttribute)))
                {
                    return field;
                }

                return null;
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